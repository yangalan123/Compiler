using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Compiler
{
    class Interpreter
    {
        List<string> codes = null;
        int[] datastack = new int[MAX_STACK_LENGTH];
        List<int> input_num = new List<int>();
        const int MAX_STACK_LENGTH = 1024;
        int p, b,t,input_index;
        public string result;
        int getbase(int l)
        {
            int b1 = b;
            while (l>0)
            {
                b1 = datastack[l];
                l -= 1;
            }
            return b1;
        }
        void init()
        {
            //stack = new int[MAX_STACK_LENGTH];
            b = 0; //no direct outer
            p = 0;
            t = -1;
            input_index = 0;
            input_num.Clear();
            result = "";
        }
        public void inter(List<string> codes,string input)
        {
            init();
            this.codes = codes;
            try
            {
                var strs = input.Split();
                foreach (var item in strs)
                {
                    try
                    {
                        int x = Convert.ToInt32(item);
                        input_num.Add(x);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }catch(Exception e)
            {

            }
            result+="Start\r\n";
            int local = 3;
            try
            {
                do
                {
                    //p = p + 1;
                    var command = codes[p].Split(' ');
                    p = p + 1;
                    int lev = Convert.ToInt32(command[1]);
                    int bias = Convert.ToInt32(command[2]);
                    if (command[0]=="LIT")
                    {
                        t = t + 1;
                        datastack[t] = bias;
                    }
                    else if (command[0] == "OPR")
                    {
                        int x = Convert.ToInt32(command[2]);
                        switch (x)
                        {
                            case 0:
                                t = b - 1;
                                p = datastack[t + 3];
                                b = datastack[t + 2];
                                break;
                            case 1:
                                datastack[t] = -datastack[t];
                                break;
                            case 2:
                                t = t - 1;
                                datastack[t] += datastack[t + 1];
                                break;
                            case 3:
                                t = t - 1;
                                datastack[t] -= datastack[t + 1];
                                break;
                            case 4:
                                t = t - 1;
                                datastack[t] *= datastack[t + 1];
                                break;
                            case 5:
                                t = t - 1;
                                datastack[t] /= datastack[t + 1];
                                break;
                            case 6:
                                datastack[t] = datastack[t] % 2;
                                break;
                            case 7:
                                t = t - 1;
                                datastack[t] = (datastack[t] == datastack[t + 1]) ? 1 : 0;
                                break;
                            case 8:
                                t = t - 1;
                                datastack[t] = (datastack[t] != datastack[t + 1]) ? 1 : 0;
                                break;
                            case 9:
                                t = t - 1;
                                datastack[t] = (datastack[t] < datastack[t + 1]) ? 1 : 0;
                                break;
                            case 10:
                                t = t - 1;
                                datastack[t] = (datastack[t] >= datastack[t + 1]) ? 1 : 0;
                                break;
                            case 11:
                                t = t - 1;
                                datastack[t] = (datastack[t] > datastack[t + 1]) ? 1 : 0;
                                break;
                            case 12:
                                t = t - 1;
                                datastack[t] = (datastack[t] <= datastack[t + 1]) ? 1 : 0;
                                break;
                            default:
                                throw new Exception("OPR operand out of range");
                                // break;

                        }
                    }
                    else if (command[0] == "LOD")
                    {
                        //int lev = Convert.ToInt32(command[1]);
                        //int bias = Convert.ToInt32(command[2]);
                        datastack[++t] = datastack[getbase(lev) + bias+local];

                    }
                    else if (command[0] == "STO")
                    {
                        //int lev = Convert.ToInt32(command[1]);
                        //int bias = Convert.ToInt32(command[2]);
                        datastack[getbase(lev) + bias+local] = datastack[t];
                        t = t - 1;
                    }
                    else if (command[0] == "CAL")
                    {
                        //int lev = Convert.ToInt32(command[1]);
                        //int bias = Convert.ToInt32(command[2]);
                        datastack[t + 1] = getbase(lev);
                        datastack[t + 2] = b;
                        datastack[t + 3] = p;
                        b = t + 1;
                        p = bias;
                    }
                    else if (command[0] == "INT")
                    {
                        t = t + bias;
                    }
                    else if (command[0] == "JMP")
                    {
                        p = bias;
                    }
                    else if (command[0] == "JPC")
                    {
                        if (datastack[t] == 0)
                        {
                            p = bias;
                        }
                        t = t - 1;
                    }
                    else if (command[0] == "RED")
                    {
                        //Console.WriteLine("Please input a number:");
                        /*string s = Console.ReadLine();
                        int x = 0;
                        while (true)
                        {
                            try
                            {
                                x = Convert.ToInt32(s);
                                break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Not a number,please try again:");
                            }
                        }*/
                        try
                        {
                            int x = input_num[input_index++];
                            datastack[getbase(lev) + bias + local] = x;
                        }catch(Exception e)
                        {
                            result += "No enough input parameters\r\n";
                            return;
                        }
                    }
                    else if (command[0] == "WRT")
                    {
                        //result+=((datastack[getbase(lev) + bias+local]).ToString()+"\r\n");
                        result += ((datastack[t]).ToString() + "\r\n");
                        //t = t + 1;
                    }
                    else throw new Exception("No such command");

                } while (p != 0);
                result+="END";
            }catch (Exception e)
            {
                if (e is OutOfMemoryException || e is ArgumentOutOfRangeException)
                {
                    result+="Exceed The Stack Limit\r\n";
                    return;
                }
                else
                {
                    result += e.Message+"\r\n";
                    result += e.StackTrace+"\r\n";
                    return;
                    //Console.WriteLine(e.Message);
                    //Console.WriteLine(e.StackTrace);
                }
            }
        }

    }
}
