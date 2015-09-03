using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbMigrator.Core.Util
{

    public class FlagFilter
    {

        public FlagFilter(string filterExpression)
        {
            filterExpression = filterExpression.Replace(" ", "");
            TextExpression = filterExpression;
            this.testExpression = Parse(filterExpression);
        }

        public bool Test(params string[] flags)
        {
            return this.testExpression(flags);
        }

        Func<string[], bool> NotFunc(Func<string[], bool> func)
        {
            return (tags) => !func(tags);
        }

        private Func<string[], bool> Parse(string expression)
        {
            List<Func<string[], bool>> tests = new List<Func<string[], bool>> {
                (tags) => { return true; }
            };

            bool? and = null;
            bool not = false;
            bool firstLoop = true;

            while (expression.Length > 0)
            {
                var character = expression[0];
                if (character == '(')
                {
                    expression = expression.Substring(1);
                    int position = -1;
                    int parentesesBalance = -1;
                    int parentesesTestPos = 0;
                    while (expression.Length > parentesesTestPos)
                    {
                        char currentParentesesTestChar = expression[parentesesTestPos];
                        if (currentParentesesTestChar == '(')
                        {
                            parentesesBalance--;
                        }
                        else if (currentParentesesTestChar == ')')
                        {
                            parentesesBalance++;
                        }
                        if (parentesesBalance == 0)
                        {
                            position = parentesesTestPos;
                            break;
                        }
                        parentesesTestPos++;
                    }

                    if (position < 0)
                    {
                        throw new ArgumentException("Missing ')'.");
                    }
                    string subexpression = expression.Substring(0, position);
                    Func<string[], bool> parsedTest = Parse(subexpression);
                    expression = expression.Substring(subexpression.Length + 1);
                    
                    if(not){
                        parsedTest = NotFunc(parsedTest);
                        not = false;
                    }

                    if (and == null)
                    {
                        tests = new List<Func<string[], bool>> { parsedTest };
                    }
                    else if (and.Value)
                    {
                        int fnIndex = tests.Count() - 1;
                        tests.Add((tags) => tests.ElementAt(fnIndex)(tags) &&  parsedTest(tags));
                    }
                    else
                    {
                        int fnIndex = tests.Count() - 1;
                        tests.Add((tags) => tests.ElementAt(fnIndex)(tags) || parsedTest(tags));
                    }
                    and = null;
                }
                else if (character == '&')
                {
                    //and
                    if (and != null)
                    {
                        throw new ArgumentException("Invalid combination of characters. '||', '&&', '&!' and '!&' are forbbiden.");
                    }
                    if (firstLoop)
                    {
                        throw new ArgumentException("An expression cannot start with '&'.");
                    }
                    and = true;
                    expression = expression.Substring(1);
                }
                else if (character == '|')
                {
                    //or
                    if (and != null)
                    {
                        throw new ArgumentException("Invalid combination of characters. '||', '&&', '&!' and '!&' are forbbiden.");
                    }
                    if (firstLoop)
                    {
                        throw new ArgumentException("An expression cannot start with '|'.");
                    }
                    and = false;
                    expression = expression.Substring(1);

                }
                else if (character == '!')
                {
                    //negate next expression
                    if (not)
                    {
                        throw new ArgumentException("Invalid combination of characters. '!!' is forbbiden.");
                    }
                    not = true;
                    expression = expression.Substring(1);
                }
                else
                {
                    //simple tag

                    var match = Regex.Match(expression, "^[^&|!]+");
                    var tag = match.Value;
                    expression = expression.Substring(tag.Length);

                    bool _not = not;
                    if (and == null)
                    {
                        tests = new List<Func<string[], bool>> { (tags) => { return (_not ? !tags.Contains(tag) : tags.Contains(tag)); } };
                    }
                    else if (and.Value)
                    {
                        int fnIndex = tests.Count() - 1;
                        tests.Add((tags) => tests.ElementAt(fnIndex)(tags) && (_not ? !tags.Contains(tag) : tags.Contains(tag)));
                    }
                    else
                    {
                        int fnIndex = tests.Count() - 1;
                        tests.Add((tags) => tests.ElementAt(fnIndex)(tags) || (_not ? !tags.Contains(tag) : tags.Contains(tag)));
                        //tests.Add((tags) => tests.ElementAt(fnIndex)(tags) || (containsTag ? tags.Contains(tag) : !tags.Contains(tag)));
                    }
                    not = false;
                    and = null;
                }
                firstLoop = false;
            }

            if (and != null || not)
            {
                throw new ArgumentException("Expression or sub expression cannot and with '!', '&' or '|'.");
            }

            return tests.Last();
        }

        private Func<string[], bool> testExpression;
        public string TextExpression { get; private set; }

    }
}
