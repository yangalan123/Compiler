using System;
using System.Windows;
using System.Text;
using System.Collections.Generic;

namespace Compiler
{

    public partial class MainWindow : Window
    {
        class PL0LexParser
        {
            const int EOF_FLAG = -1;
            const int SUCCESS = 1;
            const int FAIL = 0;
            StringBuilder token = new StringBuilder();
            private StringBuilder programstr = null;
            private int index = 0;
            private int length = 0;
            HashSet<string> BOUNDSYM = new HashSet<string>(new string[] { ";", ",", "(", ")" ,"."});
            HashSet<string> OPERATORS = new HashSet<string>(new string[] { "+", "-", "*", "/", ":=", "=", "<>", "<=", ">=", "<", ">" });
            HashSet<string> KEYWORDS = new HashSet<string>(new string[] { "const", "var", "procedure", "if", "then", "while", "do", "begin", "end", "call", "read", "repeat", "until", "read", "write" ,"odd"});
            List<List<string>> result = new List<List<string>>(); 
            public void init()
            {
                result = new List<List<string>>();
                index = 0;
                length = 0;
                programstr = null;
            }
            public List<List<string>> parse(string text)
            {
                init();
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
                                int length = token.Length;
                                token.Remove(length - 1,1);
                                try
                                {
                                    long num = long.Parse(token.ToString());
                                    result.Add(newItem(token.ToString(), "无符号整数", Convert.ToString(num, 2)));
                                }
                                catch
                                {
                                    result.Add(newItem(token.ToString(), "无符号整数", "数值超过long范围"));
                                    return;
                                }
                                retract();
                                continue;
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
                                return;
                            }
                        }
                        else
                        {
                          
                            try
                            {
                                long num = long.Parse(token.ToString());
                                result.Add(newItem(token.ToString(), "无符号整数", Convert.ToString(num,2)));
                            }
                            catch
                            {
                                result.Add(newItem(token.ToString(), "无符号整数", "数值超过long范围"));
                                return;
                            }
                            
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
                            result.Add(newItem(token.ToString(), "单字符运算符", ""));
                        else
                        {
                            res = nextchar();
                            var testtoken = new StringBuilder(token.ToString());
                            testtoken.Append(res.Item2);
                            if (isOPERATOR(Convert.ToString(res.Item2)))
                            {
                                if (OPERATORS.Contains(testtoken.ToString()))
                                {
                                    token.Append(res.Item2);
                                    result.Add(newItem(token.ToString(), "双字符运算符", ""));
                                }
                                else
                                {
                                    result.Add(newItem("无效的操作符", "", ""));
                                    return;
                                }
                            }
                            else
                            {
                                result.Add(newItem(token.ToString(), "单字符运算符", ""));
                                retract();
                            }
                        }
                        //res = nextchar();
                    }
                    else
                    {
                        result.Add(newItem(now_char.ToString(), "未知字符", "错误!"));
                        return;
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
    }
}
