using System;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SubtitleTranslation
{
    class WordAlignment
    {


        private static TranslatorService.TranslateArray2Response[] TranslateArray2Method(string authToken, string[] sourceTexts)
        {
            // Add TranslatorService as a service reference, Address:http://api.microsofttranslator.com/V2/Soap.svc
            TranslatorService.LanguageServiceClient client = new TranslatorService.LanguageServiceClient();
            //Set Authorization header before sending the request
            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", authToken);

            // Creates a block within which an OperationContext object is in scope.
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
                string[] translateArraySourceTexts = sourceTexts;
                TranslatorService.TranslateOptions translateArrayOptions = new TranslatorService.TranslateOptions(); // Use the default options
                //Keep appId parameter blank as we are sending access token in authorization header.
                TranslatorService.TranslateArray2Response[] translatedTexts = client.TranslateArray2("", translateArraySourceTexts, "en", "zh-CN", translateArrayOptions);
                return translatedTexts;
            }
        }

        private static string getToken()
        {
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("Sub_Tran", "3xCoCclw+INNTopCnOQPjM/cLGUIS/DI141YWADEP1Q=");
            try
            {
                admToken = admAuth.GetAccessToken();
                DateTime tokenReceived = DateTime.Now;
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                return headerValue;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Press any key to continue...");
                return "error";
            }
        }

        public static string[,] Translate(string[,] subtitle)
        {
            //subtitle: string{No., word, sentence, meaning}
            string headerValue = getToken();
            string[] sourceTexts = new string[subtitle.GetLength(0)];
            string[,] results = new string[subtitle.GetLength(0), 4];
            for (int i = 0; i < subtitle.GetLength(0); i++)
            {
                sourceTexts[i] = subtitle[i, 2];
                results[i, 0] = subtitle[i, 0];
                results[i, 1] = subtitle[i, 1];
                results[i, 2] = subtitle[i, 2];
            }
            TranslatorService.TranslateArray2Response[] translatedTexts = TranslateArray2Method(headerValue, sourceTexts);
            for (int i = 0; i < sourceTexts.Length; i++)
            {
                int index = subtitle[i, 2].IndexOf(subtitle[i, 1]);
                string[] alignInfos = translatedTexts[i].Alignment.Split(' ');
                string align = null;
                foreach (string ind in alignInfos)
                {
                    if (ind.StartsWith((index.ToString() + ":" + (index + subtitle[i,1].Length - 1).ToString())))
                    {
                        align = ind;
                        break;
                    }
                }
                if (align == null)
                {
                    results[i, 3] = Dict.TranslateMethod(headerValue, subtitle[i, 1]);
                }
                else
                {
                    string[] aligns = ((align.Split('-'))[1]).Split(':');
                    string translatedText = translatedTexts[i].TranslatedText;
                    string meaning = translatedText.Substring(int.Parse(aligns[0]), int.Parse(aligns[1]) - int.Parse(aligns[0]) + 1);
                    results[i, 3] = meaning;
                }
            }

            return results;
        }
    }
}
