using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace SubtitleTranslation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog1 = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("没有选中难度");
            }
            else if (openFileDialog1.FileName == "")
            {
                System.Windows.MessageBox.Show("没有选中字幕文件");
            }
            else
            {
                Action runTranslationAction = new Action(runTranslation);
                runTranslationAction.BeginInvoke(null, null);
            }
        }

        private void runTranslation()
        {
            string[] newSubtitle = null;
            string path = null;
            int index = 0;
            button.Dispatcher.Invoke(new Action(() => {
                progressBar.IsIndeterminate = true;
                path = openFileDialog1.FileName;
                index = comboBox.SelectedIndex;
            }),DispatcherPriority.SystemIdle);

            newSubtitle = Compare.translateSubtitle(path, index);

            button.Dispatcher.Invoke(new Action(() => {
                progressBar.IsIndeterminate = false;
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                folderBrowserDialog1.Description = "请选择新字幕的存储路径";
                folderBrowserDialog1.ShowDialog();
                StreamWriter srtWriter = new StreamWriter(String.Format("{0}\\{1}", folderBrowserDialog1.SelectedPath.ToString(), "Generated.srt"));
                string newSrt = "";
                foreach (string sub in newSubtitle)
                {
                    newSrt += sub;
                    newSrt += "\r\n";
                }
                srtWriter.Write(newSrt);
                srtWriter.Close();
                System.Windows.MessageBox.Show("Finished");
            }));
            

        }

        public void goProBar(int current, int Max)
        {
            progressBar.Maximum = Max;
            progressBar.Value = current;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog1.Filter = "字幕文件(*.srt)|*.srt|所有文件(*.*)|*.*";
            openFileDialog1.Title = "请选择原始纯英文字幕文件";
            openFileDialog1.ShowDialog();
            this.label.Content = openFileDialog1.FileName;
        }


        



    }

}
