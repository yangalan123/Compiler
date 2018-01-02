using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualBasic;

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
        GrammarAnalyser GAparser = new GrammarAnalyser();
        Interpreter interpreter = new Interpreter();
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
                sb.Append(item[0] +"\t"+ item[1] +"\t"+ item[2]+"\t"+item[3]+"\n");
            }
            return sb.ToString();

        }
        private void Button_Click(object sender, RoutedEventArgs e)//button for "run"
        {
            this.pb.Minimum = 0;
            this.pb.Maximum = 100;
            this.pb.Value = 0;
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
            this.pb.Value = 20;
            this.run_button.Content = "读取代码ing..";
            List<List<string>> result = this.parser.parse(text);
            this.pb.Value = 30;
            this.run_button.Content = "词法分析完成";
            //File.WriteAllText(OutputFileName, output(result));
            GAparser.init();
            var error = GAparser.parse(result);
            File.WriteAllText(OutputFileName,error);
            this.pb.Value = 50;
            this.run_button.Content = "语法分析完成";
            var s = "";
            foreach (var item in GAparser.Codes)
            {
                s += item;
            }
            if (error.Length == 0)
            {
                File.AppendAllText(OutputFileName, s);
                interpreter.inter(GAparser.Codes, this.InputContent.Text);
                File.AppendAllText(OutputFileName, "\r\n" + interpreter.result);
            }
            else
            {
                System.Windows.MessageBox.Show(error,"错误",MessageBoxButton.OK);
            }
            this.pb.Value = 100;
            this.run_button.Content = "运行完成！（点击以再次执行）";
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
