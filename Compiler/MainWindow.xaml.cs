using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compiler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {
        String InputFileName = "";
        String OutputFileName = "";
        PL0LexParser parser = new PL0LexParser();
        String outputdir = "";
        public MainWindow()
        {
            InitializeComponent();
        }
        string output(List<List<string>> tuples)
        {
            StringBuilder sb = new StringBuilder();
            foreach (List<string> item in tuples)
            {
                sb.Append(item[0] +"\t"+ item[1] +"\t"+ item[2]+"\n");
            }
            return sb.ToString();

        }
        private void Button_Click(object sender, RoutedEventArgs e)//button for "run"
        {
            
            InputFileName = this.InputFileDest.Text;
            OutputFileName = this.OutputFileDest.Text;
            if (OutputFileName.Length==0)
            {
                System.Windows.Forms.MessageBox.Show("没有指定输出路径!", "警告！", MessageBoxButtons.OK);
                return;
            }
            string text = "";
            if (this.ModeSelectBox.SelectedIndex == 0)
            {
                //FileStream inputfile = File.OpenRead(InputFileName);
                //FileStream outputfile = File.OpenWrite(OutputFileName);
                text = this.TextCode.Text;
            }
            else
                text = File.ReadAllText(InputFileName);
            List<List<string>> result = this.parser.parse(text);
            File.WriteAllText(OutputFileName, output(result));
            Process.Start("explorer.exe", @outputdir);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//button for 'selecting input file'
        {
            
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
        private void Button_Click_2(object sender, RoutedEventArgs e)//button for 'selecting output file'
        {
            
      
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && 
                !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                ((System.Windows.Controls.Button)sender).Content = "(点击可以再次选择)";
                //OutputFileName = dialog.FileName;
                this.OutputFileDest.Text = dialog.SelectedPath+"\\Report.txt";
                this.outputdir = dialog.SelectedPath;
            }
        }
    }
}
