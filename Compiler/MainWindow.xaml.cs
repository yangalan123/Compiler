using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Text;
namespace Compiler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    
    public partial class MainWindow : Window
    {
        class PL0parser
        {
            public Tuple<string,string,string>[] parse(string text)
            {
                Tuple<string, string, string>[] result = null;
                return result;
            }
        }
        String InputFileName = "";
        String OutputFileName = "";
        PL0parser parser = new PL0parser();
        public MainWindow()
        {
            InitializeComponent();
        }
        string output(Tuple<String,String,String>[] tuples)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Tuple<String,String,String> item in tuples)
            {
                sb.Append(item.Item1 +'\t'+ item.Item2 +'\t'+ item.Item3+'\n');
            }
            return sb.ToString();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //button for 'run'
            InputFileName = this.InputFileDest.Text;
            OutputFileName = this.OutputFileDest.Text;
            string text = "";
            if (this.ModeSelectBox.SelectedIndex == 0)
            {
                //FileStream inputfile = File.OpenRead(InputFileName);
                //FileStream outputfile = File.OpenWrite(OutputFileName);
                text = this.TextCode.Text;
            }
            else
                text = File.ReadAllText(InputFileName);
            Tuple<string, string, string>[] result = this.parser.parse(text);
            File.WriteAllText(OutputFileName, output(result));

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //button for 'selecting input file'
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.RestoreDirectory = true;
            dialog.Filter = "*.txt|*.*";
            //dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((System.Windows.Controls.Button)sender).Content = "(点击可以再次选择)";
                //InputFileName = dialog.FileName;
                this.InputFileDest.Text = dialog.FileName;
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //button for 'selecting output file'
      
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && 
                !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                ((System.Windows.Controls.Button)sender).Content = "(点击可以再次选择)";
                //OutputFileName = dialog.FileName;
                this.OutputFileDest.Text = dialog.SelectedPath+"\\Report.txt";
            }
        }
    }
}
