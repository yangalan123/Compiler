using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class GrammarAnalyser
    {
        String error_message;
        List<List<String>> sym_list;
        HashSet<String> Relation_operator = new HashSet<string>(new string[] { "<>","<=",">=","<",">","="});
        HashSet<String> state_header = new HashSet<string>(new string[] { "if", "while" ,"call","repeat","begin","read","write"});
        int index = 0;
        public void init()
        {
            error_message="";
            index = 0;
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
                            error_message += "(Error Code 30) 这个数太大 (line:"+sym_list[index][3]+")\r\n";
                        index += 1;
                        
                        return true;
                    }
                    else
                        error_message += "(Error Code 2) =号后应为数(line:" + sym_list[index][3] + ")\r\n";
                }
                else if (sym_list[index][0] == ":=")
                    error_message += "(Error Code 1) 应为=而不是:= (line:"+sym_list[index][3]+")\r\n";
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
                    index += 1;
                    while (sym_list[index][0]==",")
                    {
                        index += 1;
                        if (sym_list[index][1]!="标识符")
                        {
                            error_message += "(Error Code 4) const, var, procedure之后应为标识符(line: " + sym_list[index][3] + ")\r\n";
                            return false;
                        }
                        index += 1;
                    }
                    if (sym_list[index][0] == ";")
                    {
                        index += 1;
                        return true;
                    }
                    else
                        error_message += "(Error Code 5) 缺少逗号或分号 (line:"+sym_list[index][3]+")\r\n";
                }
                else
                {
                    error_message += "(Error Code 4) const, var, procedure之后应为标识符(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            return false;
        }
        bool procedure_header()
        {
            if (sym_list[index][0]=="procedure")
            {
                index += 1;
                if (sym_list[index][1]=="标识符")
                {
                    index += 1;
                    if (sym_list[index][0]==";")
                    {
                        index += 1;
                        return true;
                    }
                    else
                        error_message += "(Error Code 5) 缺少逗号或分号 (line:" + sym_list[index][3] + ")\r\n";

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
                    index += 1;
                    backup = index;
                    flag = statement();
                    if(!flag)
                    {
                        error_message += "(Error Code 7)应为语句(line:" + sym_list[backup][3] + ")\r\n";
                        return false;
                    }
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
                return false;
            }
            int backup = index;
            flag = program();
            if (!flag)
            {
                error_message += "(Error Code 29)过程体定义错误(line:" + sym_list[backup][3] + ")\r\n";
                return false;
            }
            if (index<sym_list.Count())
            while (sym_list[index][0]==";" )
            {
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
            }
            //error_message += "";
            if (index >= sym_list.Count || sym_list[index][0]!=";")
            {
                error_message += "(Error Code 5) 缺少逗号或分号 (line:" + sym_list[index][3] + ")\r\n";
                return false;
            }
            index += 1;
            return true;
        }
        bool factor()
        {
            if (sym_list[index][1]=="标识符")
            {
                index += 1;
                return true;
            }
            else if (sym_list[index][1] == "无符号整数")
            {
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
                    error_message += "(Error Code 22)缺少右括号(line: " + sym_list[index][3] + ")\r\n";
                    return false;
                }
            }
            else
                error_message += "(Error Code 31)无效的因子开头(line: " + sym_list[index][3] + ")\r\n";
            return false;
        }
        bool term()
        {
            bool flag = factor();
            int backup = index;
            if (!flag)
            {
                error_message += "(Error Code 31)项定义错误(line:" + sym_list[backup][3] + ")\r\n";
                return false;
            }
            else if (index < sym_list.Count)
            {
                while (sym_list[index][0] == "*" || sym_list[index][0] == "/")
                {
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
            if (sym_list[index][0]=="+" || sym_list[index][1]=="-")
            {
                index += 1;
                first = true;
            }
            int backup = index;
            if (term())
            {
                while (index < sym_list.Count() && sym_list[index][0] == "+" || sym_list[index][0] == "-")
                {
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
                if (first)
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
                index += 1;
                if (sym_list[index][0]==":=")
                {
                    index += 1;
                    bool flag = expr();
                    //if (flag) index += 1;
                    return flag;
                }
                else
                    error_message += "(Error Code 13)应为赋值运算符:=(line:" + sym_list[index][3] + ")\r\n";
            }
            return false;
        }
        bool while_loop_statement()
        {
            if (sym_list[index][0]=="while")
            {
                int backup = index;
                index += 1;
                bool flag = condition();
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
                    error_message += "(Error Code 18)应为do(line:"+sym_list[backup][3]+")\r\n";
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
                error_message += "(Error Code 17)应为分号或end(其实按照文法只能是end)(line:"+sym_list[index][3]+")\r\n";
            return false;
        }
        bool repeat_statement()
        {
            if (sym_list[index][0] == "repeat")
            {
                int backup = index;
                index += 1;
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
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0]==",")
                        {
                            index += 1;
                            if(sym_list[index][1]!="标识符")
                            {
                                error_message += "Illegal Read Parameters at" + sym_list[index][3] + "\r\n";
                                return false;
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
                            error_message += "Missing ( in building read statement at" + sym_list[index][3]+"\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(first symbol after ( )Building Read Statement Failed at" + sym_list[index][3] + "\r\n";
                        return false;
                    }
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
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0] == ",")
                        {
                            index += 1;
                            if (sym_list[index][1] != "标识符")
                            {
                                error_message += "Illegal Read Parameters at" + sym_list[index][3] + "\r\n";
                                return false;
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
                            error_message += "Missing ( in building read statement at" + sym_list[index][3] + "\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(first symbol after ( )Building Read Statement Failed at" + sym_list[index][3] + "\r\n";
                        return false;
                    }
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
                    error_message += "build assign statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0]=="if")
            {
                int backup = index;
                bool flag = condition_statement();
                if (!flag)
                {
                    error_message += "build condition statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "while")
            {
                int backup = index;
                bool flag = while_loop_statement();
                if (!flag)
                {
                    error_message += "build while_loop statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "repeat")
            {
                int backup = index;
                bool flag = repeat_statement();
                if (!flag)
                {
                    error_message += "build repeat statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "begin")
            {
                int backup = index;
                bool flag = block_statement();
                if (!flag)
                {
                    error_message += "build block statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "call")
            {
                int backup = index;
                bool flag = call_statement();
                if (!flag)
                {
                    error_message += "build call statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "read")
            {
                int backup = index;
                bool flag = read_statement();
                if (!flag)
                {
                    error_message += "build read statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            else if (sym_list[index][0] == "write")
            {
                int backup = index;
                bool flag = write_statement();
                if (!flag)
                {
                    error_message += "build write statement failed at" + sym_list[backup][3] + "\r\n";
                    return false;
                }
                return true;
            }
            return false;
        }
        bool program()
              {
            try
            {
                bool flag = true;
                if (index < sym_list.Count() && sym_list[index][0] == "const")
                {
                    index += 1;
                    if (!const_declaration_part())
                        return false;
                }
                if (index < sym_list.Count() && sym_list[index][0] == "var")
                {
                    flag = variable_declaration_part();
                    if (!flag) return false;
                }
                if (index < sym_list.Count() && sym_list[index][0] == "procedure")
                {
                    flag = procedure_declaration_part();
                    if (!flag) return false;
                }
                flag = statement();
                //if (!flag) return false;
                
                if (index == sym_list.Count() - 1) return true;
                return true;
            }
            catch(Exception e)
            {
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
            if (!flag)
            {
                Console.WriteLine(error_message);
            }
            flag = checkend();
            if (!flag) error_message += "Incomplete Program, Checking at" + sym_list[index][3]+"\r\n";
            if (!flag)
            {
                Console.WriteLine(error_message);
            }
            return error_message;
        }
    }
}
