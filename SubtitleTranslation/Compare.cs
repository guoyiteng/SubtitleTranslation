using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace SubtitleTranslation
{
    class Compare
    {
        private static string[] sentences;
        private static string[] vocabularies;
        private static List<string[]> subtitleList = new List<string[]>();

        public static string[] translateSubtitle(string fileName, int level)
        {
            StreamReader subtitleReader = new StreamReader(fileName, Encoding.Default);
            string content = subtitleReader.ReadToEnd();
            sentences = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            subtitleReader.Close();
            StreamReader vocabularyReader = new StreamReader(String.Format("{0}.txt", level.ToString()), Encoding.Default);
            string[] knownVocabularies = vocabularyReader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            vocabularyReader.Close();
            StreamReader abnormalVocabularyReader = new StreamReader("abnormal.txt", Encoding.Default);
            string[] abnormalVocabularies = abnormalVocabularyReader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            abnormalVocabularyReader.Close();
            vocabularies = new string[knownVocabularies.Length + abnormalVocabularies.Length];
            knownVocabularies.CopyTo(vocabularies, 0);
            abnormalVocabularies.CopyTo(vocabularies, knownVocabularies.Length);

            Parallel.For(0, sentences.Length, performLoopProcessing);

            string[,] subtitle = new string[subtitleList.Count, 3];
            for (int i = 0; i < subtitle.GetLength(0); i++)
            {
                subtitle[i, 0] = subtitleList[i][0];
                subtitle[i, 1] = subtitleList[i][1];
                subtitle[i, 2] = subtitleList[i][2];
            }

            int count = 0;
            string[] newSentences = new string[sentences.Length];
            while(true)
            {
                if (subtitle.GetLength(0) < (count + 1) * 100)
                {
                    int tempLength = subtitle.GetLength(0) - (count * 100);
                    string[,] tempList = new string[tempLength, 3];
                    for (int i = 0; i < tempLength; i++)
                    {
                        tempList[i, 0] = subtitle[i + count * 100, 0];
                        tempList[i, 1] = subtitle[i + count * 100, 1];
                        tempList[i, 2] = subtitle[i + count * 100, 2];
                    }
                    sentences = replaceSentence(tempList, sentences);
                    break;
                }
                else
                {
                    string[,] tempList = new string[100, 3];
                    for (int i = 0; i < 100; i++)
                    {
                        tempList[i, 0] = subtitle[i + count * 100, 0];
                        tempList[i, 1] = subtitle[i + count * 100, 1];
                        tempList[i, 2] = subtitle[i + count * 100, 2];
                    }
                    sentences = replaceSentence(tempList, sentences);
                }
                count++;
            }
            return sentences;

        }

        public static string[] replaceSentence(string[,] subtitle, string[] sentences)
        {
            string[,] translationResult = WordAlignment.Translate(subtitle);
            for (int i = 0; i < subtitle.GetLength(0); i++)
            {
                if (!sentences[int.Parse(subtitle[i, 0])].Contains("(")) // not a good way to solve the problem
                {
                    sentences[int.Parse(subtitle[i, 0])] = Regex.Replace(sentences[int.Parse(subtitle[i, 0])], translationResult[i, 1], string.Format("{0}({1})", translationResult[i, 1], translationResult[i, 3]));
                }
                //sentences[int.Parse(subtitle[i, 0])] = sentences[int.Parse(subtitle[i, 0])].Replace(translationResult[i, 1], translationResult[i, 3]);
            }
            return sentences;
        }

        private static void performLoopProcessing(int x)
        {
            MatchCollection sentenceWords = Regex.Matches(sentences[x], "[a-z]+", RegexOptions.IgnoreCase);
            foreach (Match sentenceWord in sentenceWords)
            {
                bool isKnown = true;
                foreach (string vocabulary in vocabularies)
                {
                    if (sentenceWord.ToString().ToLower() == vocabulary.ToLower())
                    {
                        isKnown = true;
                        break;
                    }
                    else
                    {
                        isKnown = false;
                    }
                }

                if (!isKnown)
                {
                    
                    subtitleList.Add(new string[] { x.ToString(), sentenceWord.ToString(), sentences[x] });
                    
                }
            }
        }



    }
}
