using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class GrammarAnalyser
    {
        //OPR  0-return,1-NEGATIVE,2-PLUS,3-MINUS,4-MULTIPLY,5-DIVIDE,6-ODD,7-eql,8-neq,9-lss
        //OPR  10-geq,11-gtr,12-leq
        String error_message;
        List<List<String>> symbol_table, symbol_table_stack;
        //List<List<String>> backup_symbol_table, backup_symbol_table_stack;
        List<int> subprogram_index_table;
        List<string>procedure_stack;
        Dictionary<int, List<string>> procedure_pcode_list;
        Dictionary<int, int> procedure_table;
        //Array<string> pcode;
        int id = 0,now_ptr=0;
        List<List<String>> sym_list;
       // HashSet<String> First_Statement = new HashSet<string>(new string[] {"call","begin","if","while","repeat","read","write" });
        HashSet<String> Follow_Statement = new HashSet<string>(new string[] { ".",";","end","else","until"});
        HashSet<String> Follow_Operators = new HashSet<string>(new string[] { "+","-","*","/",";",".",")","end","then","do","else","until"});
        HashSet<String> Relation_operator = new HashSet<string>(new string[] { "<>","<=",">=","<",">","="});
        HashSet<String> state_header = new HashSet<string>(new string[] { "if", "while" ,"call","repeat","begin","read","write"});
        int index = 0;
        int ga_backup_index;
        StringBuilder ga_backup_error_message;
        public void init()
        {
            error_message="";
            index = 0;
            symbol_table = new List<List<string>>();
            symbol_table_stack = new List<List<string>>();
            id = 0;
            //block_num = -1;
            now_ptr = 0;
            subprogram_index_table = new List<int>();
            procedure_stack = new List<string>();
            procedure_pcode_list = new Dictionary<int,string>();
            procedure_table = new Dictionary<int, int>();
        }
        void backup()
        {
            ga_backup_index = index;
            ga_backup_error_message = new StringBuilder(error_message);
        }
        void restore()
        {
            index = ga_backup_index;
            error_message = ga_backup_error_message.ToString();
        }
        void gen(string command,int l,int a)
        {
            if (!procedure_pcode_list.ContainsKey(now_ptr))
            {
                procedure_pcode_list.Add(now_ptr, new List<string>());
            }
            procedure_pcode_list[now_ptr].Add(command + " "+l.ToString()+" " + a.ToString() + "\r\n");
        }
        bool const_declaration_atom()
        {
            if (sym_list[index][1]=="标识符")
            {
                index += 1;
                if (sym_list[index][0] == "=")
                {
                    index += 1;
                    if (sym_list[index][1] == "无符号整数")
                    {
                        if (sym_list[index][2] == "数值超过long范围")
                        {
                            error_message += "(Error Code 30)这个数太大(line:" + sym_list[index][3] + ")\r\n";
                            add_to_symbol_table_stack(build_entry(sym_list[index-2][0],"Constant","INF"));
                        }
                        else
                        {
                            add_to_symbol_table_stack(build_entry(sym_list[index - 2][0], "Constant", sym_list[index][0]));
                            gen("LIT", 0, Convert.ToInt32(sym_list[index][0]));
                        } 
                        index += 1;

                        return true;
                    }
                    else
                        error_message += "(Error Code 2) =号后应为数(line:" + sym_list[index][3] + ")\r\n";
                }
                else if (sym_list[index][0] == ":=")
                    error_message += "(Error Code 1) 应为=而不是:= (line:" + sym_list[index][3] + ")\r\n";
                else
                    error_message += "(Error Code 3) 标识符后应为=(line:" + sym_list[index][3] + ")\r\n";
            }
            else
                error_message += "(Error Code 4) const,var,procedure之后应为标识符(line:" + sym_list[index][3] + ")\r\n";
            return false;
        }
        bool const_declaration_part()
        {

            bool flag = const_declaration_atom();
            if (!flag) return false;
          
            do
            {
                if (sym_list[index][0] == ";")
                {
                    index += 1;
                    break;
                }
                    
                if (sym_list[index][0] == ",")
                {

                        index += 1;
                        if (sym_list[index][1] == "标识符")
                        {
                            flag = const_declaration_atom();
                            if (!flag)
                            {
                                error_message += "(Error Code 26)常量声明时,逗号后应为标识符(line:"+sym_list[index][3]+")\r\n";
                                return false;
                            }
                            //index += 1;
                        }
                    
                }
                
            } while (index < sym_list.Count());
            return true;
        }
        bool variable_declaration_part()
        {
            if (sym_list[index][0]=="var")
            {
                index += 1;
                if (sym_list[index][1]=="标识符")
                {
                    add_to_symbol_table_stack(build_entry(sym_list[index][0], "Variable", "*"));
                    
                    index += 1;
                    while (sym_list[index][0]==",")
                    {
                        index += 1;
                        if (sym_list[index][1]!="标识符")
                        {
                            error_message += "(Error Code 4) const, var, procedure之后应为标识符(line: " + sym_list[index][3] + ")\r\n";
                            return false;
                        }
                        add_to_symbol_table_stack(build_entry(sym_list[index][0], "Variable", "*"));
                        index += 1;
                    }
                    if (sym_list[index][0] == ";")
                    {
                        index += 1;
                        return true;
                    }
                    else
                        error_message += "(Error Code 5) 漏掉逗号或分号 (line:"+sym_list[index][3]+")\r\n";
                }
                else
                {
                    error_message += "(Error Code 4) const, var, procedure之后应为标识符(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            return false;
        }
        void add_to_procedure_stack(string name)
        {
            foreach(var item in procedure_stack)
            {
                if (item==name)
                {
                    error_message += "过程名重复(line:" + sym_list[index][3] + ")\r\n";
                    return;
                }
            }
            procedure_stack.Add(name);
        }
        bool procedure_header()
        {
            if (sym_list[index][0]=="procedure")
            {
                index += 1;
                if (sym_list[index][1]=="标识符")
                {
                    add_to_procedure_stack(sym_list[index][0]);
                    add_to_symbol_table_stack(build_entry(sym_list[index][0], "Procedure", procedure_stack.Count.ToString()));
                    index += 1;
                    if (sym_list[index][0]==";")
                    {
                        index += 1;
                        return true;
                    }
                    else
                        error_message += "(Error Code 5) 漏掉逗号或分号 (line:" + sym_list[index][3] + ")\r\n";

                }
                else
                {
                    error_message += "(Error Code 4) const, var, procedure之后应为标识符(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            return false;
        }
        bool condition_statement()
        {
            int cx1 = 0;
            if (sym_list[index][0]=="if")
            {
                int backup = index;
                index += 1;
                bool flag = condition();
                if (!flag)
                {
                    error_message += "(Error Code 25)while,if,until后应为条件(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (sym_list[index][0]=="then")
                {
                    gen("JPC",0,0);
                    cx1 = procedure_pcode_list[now_ptr].Count()-1;
                    index += 1;
                    backup = index;
                    flag = statement();
                    if(!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                    procedure_pcode_list[now_ptr][cx1] = "JPC" + " 0 " + (procedure_pcode_list[now_ptr].Count()).ToString()+"\r\n";
                }
                else
                {
                    error_message += "(Error Code 16)应为then(line:"+sym_list[index][3]+")\r\n";
                    return false;
                }
                if (sym_list[index][0]=="else")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        bool condition()
        {
            if (sym_list[index][0]=="odd")
            {
                gen("OPR",0,6);
                index += 1;
                int backup = index;
                bool flag = expr();
                if (!flag)
                {
                    error_message += "(Error Code 27)应为表达式(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                return true;
            }
            else
            {
                int backup = index;
                bool flag = expr();
                if (!flag)
                {
                    error_message += "(Error Code 27)应为表达式(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (Relation_operator.Contains(sym_list[index][0]))
                {
                    switch(sym_list[index][0])
                    {
                        case "=":
                            gen("OPR", 0, 7);
                            break;
                        case "<>":
                            gen("OPR", 0, 8);
                            break;
                        case "<":
                            gen("OPR", 0, 9);
                            break;
                        case ">=":
                            gen("OPR",0,10);
                            break;
                        case ">":
                            gen("OPR",0,11);
                            break;
                        case "<=":
                            gen("OPR",0,12);
                            break;
                    }
                    backup = index;
                    index += 1;
                    flag = expr();
                    if (!flag)
                    {
                        error_message += "(Error Code 27)应为表达式(line:" + sym_list[backup][3] + ")\r\n"; 
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (sym_list[index][0]==":=")
                        error_message += "(Error Code 1) 应为=而不是:= (line:" + sym_list[index][3] + ")\r\n";
                    error_message += "(Error Code 20)应为关系运算符(line:"+sym_list[index][3]+")\r\n";
                    return false;
                }
            }
            return false;
        }
        bool procedure_declaration_part()
        {
            bool flag = procedure_header();
            if (!flag)
            {
                error_message += "(Error Code 28)过程头部定义错误(line:" + sym_list[index][3] + ")\r\n";
            
                procedure_stack.RemoveAt(procedure_stack.Count - 1);
                return false;
            }
            int backup = index;
            flag = program();
            if (!flag)
            {
                error_message += "(Error Code 29)过程体定义错误(line:" + sym_list[backup][3] + ")\r\n";
           
                procedure_stack.RemoveAt(procedure_stack.Count - 1);
                return false;
            }
            /*if (index<sym_list.Count())
            while (sym_list[index][0]==";" )
            {
                    if (index >= sym_list.Count || sym_list[index + 1][0] != "procedure")
                        break;
                    backup = index;
                    index += 1;
                    var sb = new StringBuilder(error_message);
                    flag = procedure_declaration_part();
                    if (!flag)
                    {
                        index = backup;
                        error_message = sb.ToString();
                        break;
                    }
            }*/
            //error_message += "";
            if (index >= sym_list.Count || sym_list[index][0]!=";")
            {
                if (index >= sym_list.Count)
                {
                    error_message += "(Error Code 6)过程说明后的符号''不正确(line:" + sym_list[index - 1][3] + ")\r\n";
                    //return false;
                }
                else
                {
                    error_message += "(Error Code 5)漏掉逗号或分号(line:" + sym_list[index][3] + ")\r\n";
                    error_message += "(Error Code 6)过程说明后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                }
                procedure_stack.RemoveAt(procedure_stack.Count - 1);
                return false;
            }
            index += 1;
            //int len = procedure_stack.Count;
            procedure_stack.RemoveAt(procedure_stack.Count - 1);
            if (index >= sym_list.Count || sym_list[index][0] != "procedure")
                return true;
            if (index < sym_list.Count && sym_list[index][0] == "procedure")
                return procedure_declaration_part();
            return true;
        }
        bool factor()
        {
            if (sym_list[index][1]=="标识符")
            {
                string str=query_symbol_table_stack(sym_list[index][0], "expression_not_assign");
                if (str!="Error")
                {
                    string[] strs = str.Split(' ');
                    if (strs[2]=="Variable")
                    {
                        int BL = Convert.ToInt32(strs[0]);
                        int ON = Convert.ToInt32(strs[1]);
                        gen("LOD", BL, ON);
                    }
                    else if (strs[2]=="Constant")
                    {
                        var entry = strs[3].Split('\r');
                        if (entry[2]!="INF")
                        {
                            int val = Convert.ToInt32(entry[2]);
                            gen("LIT", 0, val);
                        }
                    }
                }
                index += 1;
                return true;
            }
            else if (sym_list[index][1] == "无符号整数")
            {
                if (sym_list[index][2] == "数值超过long范围")
                    error_message += "(Error Code 30)这个数太大(line:" + sym_list[index][3] + ")\r\n";
                index += 1;
                return true;
            }
            else if (sym_list[index][0]=="(")
            {
                index += 1;
                int backup = index;
                bool flag = expr();
                if (!flag)
                {
                    error_message += "(Error Code 27)应为表达式(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                //index += 1;
                if (sym_list[index][0]==")")
                {
                    index += 1;
                    return true;
                }
                else
                {
                    error_message += "(Error Code 22)漏右括号(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            else
            {
                //if (sym_list[index][0]=="")
                //error_message += "(Error Code 21)表达式中不可有过程标识符()"
                error_message += "(Error Code 37)无效的因子开头(line: " + sym_list[index][3] + ")\r\n";
            }
                
            return false;
        }
        bool term()
        {
            int backup = index;
            bool flag = factor();
            if (!flag)
            {
                error_message += "(Error Code 31)项定义错误(line:" + sym_list[backup][3] + ")\r\n";
                return false;
            }
            else if (index < sym_list.Count)
            {
                if (!Follow_Operators.Contains(sym_list[index][0]) && !Relation_operator.Contains(sym_list[index][0]))
                    error_message += "(Error Code 23)因子后不可为此符号("+sym_list[index][0]+")(line:"+sym_list[index][3]+")\r\n";
                while (sym_list[index][0] == "*" || sym_list[index][0] == "/")
                {
                    if (sym_list[index][0]=="*")
                    {
                        gen("OPR", 0, 4);
                    }
                    else
                    {
                        gen("OPR", 0, 5);
                    }
                    index += 1;
                    backup = index;
                    flag = factor();
                    if (!flag)
                    {
                        error_message += "(Error Code 32)因子定义错误(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                    //index += 1;
                }
                //have increased index by 1
                return true;
            }
            else return true;
            //return false;
        }
        bool expr()
        {
            bool first = false;
            bool neg = false;
            if (sym_list[index][0]=="+" || sym_list[index][0]=="-")
            {
                if (sym_list[index][0]=="-")
                {
                    neg = true;
                    //gen("OPR", 0, 1);
                }
                gen("OPR",0,1);
                index += 1;
                first = true;
            }
            int backup = index;
            if (term())
            {
                if (neg)
                    gen("OPR", 0, 1);
                first = true;
                while (index < sym_list.Count() && sym_list[index][0] == "+" || sym_list[index][0] == "-")
                {
                    if (sym_list[index][0] == "+")
                        gen("OPR", 0, 2);
                    else if (sym_list[index][0] == "-")
                        gen("OPR",0,3);
                    index += 1;
                    bool flag = term();
                    if (!flag)
                    {
                        error_message += "(Error Code 34)项定义错误(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                    //index += 1;
                }
                //have increased index by 1
                return true;
            }
            else
            {
                if (!first)
                {
                    error_message += "(Error Code 24)表达式不能以此开头(line: " + sym_list[backup][3] + ")\r\n";
                }
                else
                {
                    error_message += "(Error Code 33)表达式首项定义错误(line: " + sym_list[backup][3] + ")\r\n";
                }
            }
            return false;
        }
        bool assign_statement()
        {
            if (sym_list[index][1]=="标识符")
            {
                string str=query_symbol_table_stack(sym_list[index][0], "assign");
                index += 1;
                if (sym_list[index][0]==":=")
                {
                    index += 1;
                    bool flag = expr();
                    //if (flag) index += 1;
                    if (str!="Error" && flag)
                    { 
                        var strs = str.Split(' ');
                        int BL = Convert.ToInt32(strs[0]);
                        int ON = Convert.ToInt32(strs[1]);
                        gen("STO", BL, ON);
                    }
                    
                    return flag;
                }
                else
                    error_message += "(Error Code 13)应为赋值运算符:=(line:" + sym_list[index][3] + ")\r\n";
            }
            return false;
        }
        bool while_loop_statement()
        {
            int cx1 = 0;
            if (sym_list[index][0]=="while")
            {
                int backup = index;
                index += 1;
                bool flag = condition();
                gen("JPC", 0, 0);
                cx1 = procedure_pcode_list[now_ptr].Count() - 1;
                if (!flag)
                {
                    error_message += "(Error Code 25)while,if,until后应为条件(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (sym_list[index][0] == "do")
                {
                    backup = index;
                    index += 1;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;

                    }
                }
                else
                { error_message += "(Error Code 18)应为do(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                procedure_pcode_list[now_ptr][cx1] = "JPC" + " 0 " + (procedure_pcode_list[now_ptr].Count()).ToString() + "\r\n";
                return true;
            }
            return false;
        }
        bool call_statement()
        {
            if (sym_list[index][0]=="call")
            {

                index += 1;
                if (sym_list[index][1] == "标识符")
                {
                    string str=query_symbol_table_stack(sym_list[index][0],"Call");
                    if (str!="Error")
                    {
                        var strs = str.Split(' ');
                        int BL = Convert.ToInt32(strs[0]);
                        int ON = Convert.ToInt32(strs[1]);
                        gen("CAL",BL,ON);
                    }
                    index += 1;
                    return true;
                }
                else
                    error_message += "(Error Code 14)call后应为标识符(line:"+sym_list[index][3]+")\r\n";
            }
           // error_message += "Building Call Statement Failed at" + sym_list[index][3] + "\r\n";
            return false;
        }
        bool block_statement()
        {
            if (sym_list[index][0]=="begin")
            {
                index += 1;
                int backup = index;
                bool flag = statement();
                if (!flag)
                {
                    error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                while (sym_list[index][0]==";")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                }
            }
            if (sym_list[index][0] == "end")
            {
                index += 1;
                return true;
            }
            else
                error_message += "(Error Code 17)应为分号或end(line:"+sym_list[index][3]+")\r\n";
            return false;
        }
        bool repeat_statement()
        {
            int cx1 = 0;
            if (sym_list[index][0] == "repeat")
            {
                int backup = index;
                index += 1;
                cx1 = procedure_pcode_list[now_ptr].Count();
                bool flag = statement();
                if (!flag)
                {
                    error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                while (sym_list[index][0] == ";")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
                }
            }
            if (sym_list[index][0] == "until")
            {
                int backup = index;
                index += 1;
                bool flag = condition();
                if (!flag)
                {
                    error_message += "(Error Code 25)while,if,until后应为条件(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                procedure_pcode_list[now_ptr][cx1] = "JPC" + " 0 " + (cx1).ToString() + "\r\n";
                return true;
            }
            else
            {
                error_message += "(Error Code 35)缺少until(line:" + sym_list[index][3] + ")\r\n";
            }
            return false;
        }
        bool read_statement()
        {
            if (sym_list[index][0]=="read")
            {
                index += 1;
                if (sym_list[index][0]=="(")
                {
                    index += 1;
                    if (sym_list[index][1]=="标识符")
                    {
                        string str = query_symbol_table_stack(sym_list[index][0], "others");
                        if (str != "Error")
                        {
                            var strs = str.Split(' ');
                            int BL = Convert.ToInt32(strs[0]);
                            int ON = Convert.ToInt32(strs[1]);
                            gen("RED", BL, ON);
                        }
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0]==",")
                        {
                            index += 1;
                            if(sym_list[index][1]!="标识符")
                            {
                                error_message += "(Error Code 36)应为标识符(line:" + sym_list[index][3] + ")\r\n";
                                return false;
                            }
                            str = query_symbol_table_stack(sym_list[index][0], "others");
                            if (str != "Error")
                            {
                                var strs = str.Split(' ');
                                int BL = Convert.ToInt32(strs[0]);
                                int ON = Convert.ToInt32(strs[1]);
                                gen("RED", BL, ON);
                            }
                            index += 1;
                        }
                        if (sym_list[index][0]==")")
                        {
                            index += 1;
                            return true;
                        }
                        else
                        {
                            error_message += "(Error Code 22)漏右括号(line: " + sym_list[index][3] + ")\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(Error Code 36)应为标识符(line:" + sym_list[index][3] + ")\r\n";
                        return false;
                    }
                }
                else
                {
                    error_message += "(Error Code 40)应为左括号(line:" + sym_list[index][3] + ")\r\n";
                }
            }
            return false;
        }
        bool write_statement()
        {
            if (sym_list[index][0] == "write")
            {
                index += 1;
                if (sym_list[index][0] == "(")
                {
                    index += 1;
                    if (sym_list[index][1] == "标识符")
                    {
                        string str=query_symbol_table_stack(sym_list[index][0], "others");
                        if (str != "Error")
                        {
                            var strs = str.Split(' ');
                            int BL = Convert.ToInt32(strs[0]);
                            int ON = Convert.ToInt32(strs[1]);
                            gen("WRT", 0, 0);
                        }
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0] == ",")
                        {
                            index += 1;
                            if (sym_list[index][1] != "标识符")
                            {
                                error_message += "(Error Code 36)应为标识符(line:" + sym_list[index][3] + ")\r\n";
                                return false;
                            }
                            str = query_symbol_table_stack(sym_list[index][0], "others");
                            if (str != "Error")
                            {
                                var strs = str.Split(' ');
                                int BL = Convert.ToInt32(strs[0]);
                                int ON = Convert.ToInt32(strs[1]);
                                gen("WRT", 0, 0);
                            }
                            index += 1;
                        }
                        if (sym_list[index][0] == ")")
                        {
                            index += 1;
                            return true;
                        }
                        else
                        {
                            error_message += "(Error Code 22)漏右括号(line: " + sym_list[index][3] + ")\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(Error Code 36)应为标识符(line:" + sym_list[index][3] + ")\r\n";
                        return false;
                    }
                }
                else
                {
                    error_message += "(Error Code 40)应为左括号(line:" + sym_list[index][3] + ")\r\n";
                }
            }
            return false;
        }
        bool statement()
        {
            if (!(sym_list[index][1]=="标识符" || state_header.Contains(sym_list[index][0])))
                return true;
            if (sym_list[index][1] =="标识符")
            {
                int backup = index;
                bool flag = assign_statement();
                if (!flag)
                {
                    error_message += "赋值语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号"+ sym_list[index][0]+"不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:"+sym_list[index][3]+")\r\n";
                return true;
            }
            else if (sym_list[index][0]=="if")
            {
                int backup = index;
                bool flag = condition_statement();
                if (!flag)
                {
                    error_message += "选择语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号"+sym_list[index][0]+"不正确(line:" + sym_list[index][3] + ")\r\n"+ "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "while")
            {
                int backup = index;
                bool flag = while_loop_statement();
                if (!flag)
                {
                    error_message += "当型循环语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "repeat")
            {
                int backup = index;
                bool flag = repeat_statement();
                if (!flag)
                {
                    error_message += "repeat循环语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "begin")
            {
                int backup = index;
                bool flag = block_statement();
                if (!flag)
                {
                    error_message += "复合语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "call")
            {
                int backup = index;
                bool flag = call_statement();
                if (!flag)
                {
                    error_message += "过程调用语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "read")
            {
                int backup = index;
                bool flag = read_statement();
                if (!flag)
                {
                    error_message += "读语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            else if (sym_list[index][0] == "write")
            {
                int backup = index;
                bool flag = write_statement();
                if (!flag)
                {
                    error_message += "写语句不规范(line:" + sym_list[backup][3] + ")\r\n";
                    return false;
                }
                if (!Follow_Statement.Contains(sym_list[index][0]))
                    error_message += "(Error Code 8)程序体内语句部分后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n" + "(Error Code 19)语句后的符号" + sym_list[index][0] + "不正确(line:" + sym_list[index][3] + ")\r\n";
                if (state_header.Contains(sym_list[index][0]) || sym_list[index][1] == "标识符")
                    error_message += "(Error Code 10)语句之间漏分号(line:" + sym_list[index][3] + ")\r\n";
                return true;
            }
            return false;
        }
        string build_entry(string name,string variable_type,string value)
        {
            // res = "";
            string res = name + "\r" + variable_type + "\r" + value+"\r";
            return res;
        }
        bool add_to_symbol_table_stack(string entry)
        {
            var dict = symbol_table_stack.Last();
            string[] new_one = entry.Split('\r');
            foreach(var item in dict)
            {
                var old_one = item.Split('\r');
                if (old_one[0].Equals(new_one[0]))
                {
                    //Console.WriteLine(old_one[0]+"\n"+new_one[0]);
                    error_message += "(Error Code 38)标识符"+new_one[0]+"重定义(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            if (new_one[1]=="Procedure")
            {
                if (!procedure_table.ContainsKey(id))
                {
                    procedure_table.Add(id, 0);
                }
                procedure_table[id] = symbol_table.Count;
            }
            entry += id.ToString()+"\r";
            id += 1;
            dict.Add(entry);
            return true;
        }
        string query_symbol_table_stack(string name,string type)
        {
            if (type=="Call")
            {
                
                var length = symbol_table_stack.Count-1;
                int _index = -1;
                while (length >= 0)
                {
                    _index += 1;
                    var now_list = symbol_table_stack[length];
                    foreach (var item in now_list)
                    {
                        var old_one = item.Split('\r');
                        if (old_one[0].Equals(name) && old_one[1] != "Procedure")
                        {
                            error_message += "(Error Code 15)不可调用常量或变量(line:" + sym_list[index][3] + ")\r\n";
                            return "Error";
                        }
                        if (old_one[0].Equals(name) && old_one[1]=="Procedure")
                        {
                            return (symbol_table_stack.Count - length).ToString() + " " + _index.ToString()+" "+old_one[1]+" "+item;
                        }
                    }
                    length--;
                }
                error_message += "调用了未定义的函数" + name + "(line:" + sym_list[index][3] + ")\r\n";
            }
            else if (type=="expression_not_assign" || type=="assign" || type=="others")
            {
                if (procedure_stack.Count>0 && name.Equals(procedure_stack.Last()))
                {
                    error_message += "(Error Code 21)表达式内不可有过程标识符(line:"+sym_list[index][3]+")\r\n";
                    if (type=="assign")
                    {
                        error_message += "(Error Code 12)不可向常量或过程赋值(line:" + sym_list[index][3] + ")\r\n";
                        return "Error";
                    }
                    return "Error";
                }
                int stack_count = symbol_table_stack.Count-1;
                while (stack_count>=0)
                {
                    var now_list = symbol_table_stack[stack_count];
                    int _index = -1;
                    foreach (var item in now_list)
                    {
                        _index += 1;
                        var old_one = item.Split('\r');
                        if (old_one[0] == name && old_one[1] == "Constant" && type == "assign")
                        {
                            error_message += "(Error Code 12)不可向常量或过程赋值(line:" + sym_list[index][3] + ")\r\n";
                            return "Error";
                        }
                        if (old_one[0] == name && old_one[1] != "Procedure")
                        {
                            return (symbol_table_stack.Count-stack_count).ToString()+" "+_index.ToString()+" "+old_one[1];
                        }

                    }
                    stack_count--;
                }
                error_message += "(Error Code 11)标识符"+name+"未说明(line:" + sym_list[index][3] + ")\r\n";
                return "Error";
                

            }
            return "Error";
           // var program = 0;
        }
        bool program()
        {
            try
            {
                
                subprogram_index_table.Add(now_ptr);
                symbol_table_stack.Add(new List<string>());
                symbol_table.Add(new List<string>());
                now_ptr = symbol_table.Count - 1;
                bool flag = true;
                if (index < sym_list.Count() && sym_list[index][0] == "const")
                {
                    index += 1;
                    if (!const_declaration_part())
                    {
                        symbol_table[now_ptr] = symbol_table_stack.Last();
                        subprogram_index_table.RemoveAt(subprogram_index_table.Count - 1);
                        if (subprogram_index_table.Count > 0)
                            now_ptr = subprogram_index_table.Last();
                        int len = symbol_table_stack.Count;
                        if (subprogram_index_table.Count > 0)
                            symbol_table_stack.RemoveAt(len - 1);
                        return false;
                    }
                       
                }
                if (index < sym_list.Count() && sym_list[index][0] == "var")
                {
                    flag = variable_declaration_part();
                    if (!flag)
                    {
                        symbol_table[now_ptr] = symbol_table_stack.Last();
                        subprogram_index_table.RemoveAt(subprogram_index_table.Count - 1);
                        if (subprogram_index_table.Count > 0)
                            now_ptr = subprogram_index_table.Last();
                        int len = symbol_table_stack.Count;
                        if (subprogram_index_table.Count > 0)
                            symbol_table_stack.RemoveAt(len - 1);
                        return false;
                    }
                }
                if (index < sym_list.Count() && sym_list[index][0] == "procedure")
                {
                    flag = procedure_declaration_part();
                    if (!flag)
                    {
                        symbol_table[now_ptr] = symbol_table_stack.Last();
                        subprogram_index_table.RemoveAt(subprogram_index_table.Count - 1);
                        if (subprogram_index_table.Count > 0)
                            now_ptr = subprogram_index_table.Last();
                        int len = symbol_table_stack.Count;
                        if (subprogram_index_table.Count > 0)
                            symbol_table_stack.RemoveAt(len - 1);
                        return false;
                    }
                }
                flag = statement();
                //if (!flag) return false;

                symbol_table[now_ptr] = symbol_table_stack.Last();
                subprogram_index_table.RemoveAt(subprogram_index_table.Count-1);
                if (subprogram_index_table.Count>0)
                    now_ptr = subprogram_index_table.Last();
                int len0 = symbol_table_stack.Count;
                if (subprogram_index_table.Count > 0)
                    symbol_table_stack.RemoveAt(len0 - 1);

                if (index == sym_list.Count() - 1) return true;
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                if (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException)
                {
                    error_message += "Incomplete subProgram, Checking at" + sym_list[index][3] + "\r\n";
                    return false;
                }
                else
                {
                    error_message = e.StackTrace.ToString();
                    return false;
                }
            }
            return true;
        }
        bool checkend()
        {
            if (index<sym_list.Count() && sym_list[index][0]==".")
            {
                index += 1;
                return true;
            }
            return false;
        }
 
        public String parse(List<List<String>> sym_list)
        {
            this.sym_list = sym_list;
            bool flag = program();
            /* if (!flag)
             {
                 Console.WriteLine(error_message);
             }*/
            if (!flag) return error_message;
            int backup = index;
            flag = checkend();
            if (!flag) error_message += "(Error Code 9)应为句号(line:" + sym_list[backup][3]+")\r\n";
            if (!flag)
            {
                Console.WriteLine(error_message);
            }
            return error_message;
        }
    }
}
