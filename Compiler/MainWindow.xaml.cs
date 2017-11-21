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
        class PL0parser
        {
            const int EOF_FLAG = -1;
            const int SUCCESS = 1;
            const int FAIL = 0;
            StringBuilder token = new StringBuilder();
            private StringBuilder programstr = null;
            private int index = 0;
            private int length = 0;
            HashSet<string> BOUNDSYM = new HashSet<string>(new string[] { ";", ",", "(", ")" });
            HashSet<string> OPERATORS = new HashSet<string>(new string[] { "+", "-", "*", "/", ":=", "=", "<>", "<=", ">=", "<", ">" });
            HashSet<string> KEYWORDS = new HashSet<string>(new string[] { "const", "var", "procedure", "if", "then", "while", "do", "begin", "end", "call", "read", "repeat", "until", "read", "write" ,"odd"});
            List<List<string>> result = new List<List<string>>(); 
            public List<List<string>> parse(string text)
            {
                programstr = new StringBuilder(text);
                index = 0;
                length = programstr.Length;
                getsym();
                return result;
            }
            bool isdigit(char x)
            {
                if (x >= '0' && x <= '9')
                    return true;
                return false;
            }
            bool isLetter(char x)
            {
                if (x >= 'a' && x <= 'z')
                    return true;
                if (x >= 'A' && x <= 'Z')
                    return true;
                return false;
            }
            bool isblank(char x)
            {
                if (x == '\n' || x == '\r' || x == '\t' || x == ' ')
                    return true;
                return false;
            }
            bool isKEYWORD(String str)
            {
                if (KEYWORDS.Contains(str))
                    return true;
                return false;
            }
            bool isBOUNDSYMBOL(String str)
            {
                if (BOUNDSYM.Contains(str))
                    return true;
                return false;
            }
            bool isOPERATOR(String str)
            {
                if (OPERATORS.Contains(str))
                    return true;
                return false;
            }
            List<string> newItem(String symbol0,String symbol1,String symbol2)
            {
                return new List<string>(new string[] { symbol0, symbol1, symbol2});
            }
            void getsym()
            {
                token.Clear();
                do
                {
                    token.Clear();
                    Tuple<int, char> res = nextchar();
                    if (res.Item1 == EOF_FLAG) break;
                    if (isblank(res.Item2)) continue;
                    char now_char = res.Item2;
                    if (isLetter(now_char))
                    {
                        while (isLetter(now_char) || isdigit(now_char))
                        {
                            token.Append(now_char);
                            res = nextchar();
                            if (res.Item1 == EOF_FLAG)
                                break;
                            now_char = res.Item2;
                        }
                        retract();
                        if (isKEYWORD(token.ToString()))
                        {
                            result.Add(newItem(token.ToString(),"关键字",""));
                        }
                        else
                        {
                            result.Add(newItem(token.ToString(), "标识符", ""));
                        }
                        //res = nextchar();
                    }
                    else if (isdigit(now_char))
                    {
                        while (isdigit(now_char))
                        {
                            token.Append(now_char);
                            res = nextchar();
                            if (res.Item1 == EOF_FLAG)
                                break;
                            now_char = res.Item2;
                        }
                        if (res.Item2=='.')
                        {
                            token.Append(now_char);
                            res = nextchar();
                            if (res.Item1 == EOF_FLAG)
                            {
                                result.Add(newItem("无效的浮点格式", "", ""));
                                break;
                            }
                            now_char = res.Item2;
                            while (isdigit(now_char))
                            {
                                token.Append(now_char);
                                res = nextchar();
                                if (res.Item1 == EOF_FLAG)
                                    break;
                                now_char = res.Item2;
                            }
                            retract();
                            try
                            {
                                float floatnumber = float.Parse(token.ToString());
                                result.Add(newItem(token.ToString(), "浮点数常数", Convert.ToString(BitConverter.DoubleToInt64Bits(floatnumber), 2)));
                            }catch
                            {
                                result.Add(newItem("无效的浮点格式", "", ""));
                            }
                        }
                        else
                        {
                            result.Add(newItem(token.ToString(), "无符号整数常数",""));
                            retract();
                        }
                        //res = nextchar();
                    }
                    else if (isBOUNDSYMBOL(Convert.ToString(res.Item2)))
                    {
                        token.Append(now_char);
                        result.Add(newItem(token.ToString(), "界符", ""));
                       // res = nextchar();
                    }
                    else if (isOPERATOR(Convert.ToString(res.Item2)) || res.Item2==':')
                    {
                        token.Append(now_char);
                        if (res.Item2 != '>' && res.Item2 != '<' && res.Item2!=':')
                            result.Add(newItem(token.ToString(), "操作符", ""));
                        else
                        {
                            res = nextchar();
                            if (isOPERATOR(Convert.ToString(res.Item2)))
                            {
                                token.Append(res.Item2);
                                result.Add(newItem(token.ToString(), "操作符", ""));
                            }
                            else
                                retract();
                        }
                        //res = nextchar();
                    }
                    if (res.Item1 == EOF_FLAG) break;
                } while (true);
                    
            }
            void retract()
            {
                index -= 1;
            }
            Tuple<int,char> nextchar()
            {
                if (index<length)
                {
                    Tuple<int, char> x = new Tuple<int,char>(SUCCESS,programstr[index++]);
                    return x;
                }
                else
                {
                    Tuple<int, char> x = new Tuple<int, char>(EOF_FLAG, '0');
                    return x;
                }

            }
        }
        String InputFileName = "";
        String OutputFileName = "";
        PL0parser parser = new PL0parser();
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //button for "run"
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
                this.outputdir = dialog.SelectedPath;
            }
        }
    }
}
