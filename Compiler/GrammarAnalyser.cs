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
                if (sym_list[index][0]=="=")
                {
                    index += 1;
                    if (sym_list[index][1] =="无符号整数")
                    {
                        index += 1;
                        return true;
                    }
                }
            }
            error_message += "Error in a single constant declaration statement:" + index.ToString() + "\r\n";
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
                            if (!flag) return false;
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
                            error_message += "Wrong Variable Declaraion in" + index.ToString() + "\r\n";
                            return false;
                        }
                        index += 1;
                    }
                    if (sym_list[index][0]==";")
                    {
                        index += 1;
                        return true;
                    }
                }
                else
                {
                    error_message += "Wrong Variable Declaraion in" + index.ToString() + "\r\n";
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
                }
            }
            return false;
        }
        bool condition_statement()
        {
            if (sym_list[index][0]=="if")
            {
                int backup = index;
                bool flag = condition();
                if (!flag)
                {
                    error_message += "(if-condition)Building Condition Statement Failed at" + backup.ToString() + "\r\n";
                    return false;
                }
                if (sym_list[index][0]=="Then")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if(!flag)
                    {
                        error_message += "(then-statement)Building Condition Statement Failed at" + backup.ToString() + "\r\n";
                        return false;
                    }
                }
                if (sym_list[index][0]=="else")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(then-statement)Building Condition Statement Failed at" + backup.ToString() + "\r\n";
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
                    error_message += "Failure occurred in building an expression in" + backup.ToString() + "\r\n";
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
                    error_message += "Failure occurred in building an expression in" + backup.ToString() + "\r\n";
                    return false;
                }
                if (Relation_operator.Contains(sym_list[index][0]))
                {
                    backup = index;
                    flag = expr();
                    if (!flag)
                    {
                        error_message += "Failure occurred in building an expression in" + backup.ToString() + "\r\n";
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        bool procedure_declaration_part()
        {
            bool flag = procedure_header();
            if (!flag)
            {
                error_message += "Error in procedure header in" + index.ToString() + "\r\n";
                return false;
            }
            flag = program();
            if (!flag)
            {
                error_message += "Failed in parsing procedure declaration in " + index.ToString() + "\r\n";
                return false;
            }
            if (index<sym_list.Count())
            while (sym_list[index][0]!=";"&& sym_list[index][0]=="procedure" && (flag=procedure_declaration_part()))
            {
            }
            error_message += "";
            if (index >= sym_list.Count || sym_list[index][0]!=";")
            {
                error_message += "missing ';' at the end of procedure declaration in " + index.ToString() + "\r\n";
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
                    error_message += "Failure occurred in building an expression in" + backup.ToString() + "\r\n";
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
                    error_message += "Missing ')' in building a factor in " + index.ToString() + "\r\n";
                    return false;
                }
            }
            return false;
        }
        bool term()
        {
            bool flag = factor();
            int backup = index;
            if (!flag)
            {
                error_message += "Failure occurred in building term at" + backup.ToString() + "\r\n";
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
                        error_message += "Failure occurred in building a term in" + backup.ToString() + "\r\n";
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
            if (sym_list[index][0]=="+" || sym_list[index][1]=="-")
            {
                index += 1;
            }
            if (term())
            {
                if (index<sym_list.Count())
                while (sym_list[index][0] == "+" || sym_list[index][0] == "-")
                {
                    index += 1;
                    bool flag = term();
                    if (!flag)
                    {
                        error_message += "Failure occurred in building an expression in" + index.ToString() + "\r\n";
                        return false;
                    }
                    //index += 1;
                }
                //have increased index by 1
                return true;
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
                error_message += "Missing assign symbol at" + index.ToString() + "\r\n";
            }
            return false;
        }
        bool while_loop_statement()
        {
            if (sym_list[index][0]=="while")
            {
                int backup = index;
                bool flag = condition();
                if (!flag)
                {
                    error_message += "(while-condition)Building While Loop Statement at "+backup.ToString()+"\r\n";
                    return false;
                }
                if (sym_list[index][0]=="do")
                {
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "(do-statement)Building While Loop Statement at" + backup.ToString() + "\r\n";
                        return false;
                  
                    }
                }
                return true;
            }
            return false;
        }
        bool call_statement()
        {
            if (sym_list[index][0]=="call")
            {

                index += 1;
                if (sym_list[index][1]=="标识符")
                {
                    return true;
                }
            }
            error_message += "Building Call Statement Failed at" + index.ToString() + "\r\n";
            return false;
        }
        bool block_statement()
        {
            if (sym_list[index][0]=="begin")
            {
                int backup = index;
                bool flag = statement();
                if (!flag)
                {
                    error_message += "Building block statement failed at " + index.ToString() + "\r\n";
                    return false;
                }
                while (sym_list[index][0]==";")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "Building block statement failed at " + index.ToString() + "\r\n";
                        return false;
                    }
                }
            }
            if (sym_list[index][0]=="end")
            {
                index += 1;
                return true;
            }
            return false;
        }
        bool repeat_statement()
        {
            if (sym_list[index][0] == "repeat")
            {
                int backup = index;
                bool flag = statement();
                if (!flag)
                {
                    error_message += "Building repeat statement failed at " + backup.ToString() + "\r\n";
                    return false;
                }
                while (sym_list[index][0] == ";")
                {
                    index += 1;
                    backup = index;
                    flag = statement();
                    if (!flag)
                    {
                        error_message += "Building repeat statement failed at " + backup.ToString() + "\r\n";
                        return false;
                    }
                }
            }
            if (sym_list[index][0] == "until")
            {
                index += 1;
                int backup = index;
                bool flag = condition();
                if (!flag)
                {
                    error_message += "(until-condition)Building repeat statement at" + backup.ToString() + "\r\n";
                    return false;
                }
                return true;
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
                    if (sym_list[index][1]=="标识符")
                    {
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0]==",")
                        {
                            if(sym_list[index][1]!="标识符")
                            {
                                error_message += "Illegal Read Parameters at" + index.ToString() + "\r\n";
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
                            error_message += "Missing ( in building read statement at" + index.ToString()+"\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(first symbol after ( )Building Read Statement Failed at" + index.ToString() + "\r\n";
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
                    if (sym_list[index][1] == "标识符")
                    {
                        index += 1;
                        while (index < sym_list.Count() && sym_list[index][0] == ",")
                        {
                            if (sym_list[index][1] != "标识符")
                            {
                                error_message += "Illegal Read Parameters at" + index.ToString() + "\r\n";
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
                            error_message += "Missing ( in building read statement at" + index.ToString() + "\r\n";
                            return false;
                        }
                    }
                    else
                    {
                        error_message += "(first symbol after ( )Building Read Statement Failed at" + index.ToString() + "\r\n";
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
                    error_message += "build assign statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build condition statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build while_loop statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build repeat statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build block statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build call statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build read statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "build write statement failed at" + backup.ToString() + "\r\n";
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
                    error_message += "Incomplete subProgram, Checking at" + index.ToString() + "\r\n";
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
            if (!flag) error_message += "Incomplete Program, Checking at" + index.ToString()+"\r\n";
            if (!flag)
            {
                Console.WriteLine(error_message);
            }
            return error_message;
        }
    }
}
