using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest
{
    public interface IFunction
    {
        string Save();
    }

    public class Expression : IFunction
    {
        private string m_expression;
        private GameLoader m_gameLoader;

        public Expression(string expression, GameLoader loader)
        {
            m_expression = expression;
            m_gameLoader = loader;
            if (loader == null) throw new ArgumentNullException();
        }

        public string Save()
        {
            // call utility function, pass in obj names, to convert obj references to object_objectname
            // also needs to remove spaces in object or variable names
            // also convert "and" &&, "or" ||, "not" !, "xor" ^
            // and "=" must be "==", also check what not-equals operator is in FLEE, convert to != if necessary

            string result = m_expression;
            //result = Utility.ReplaceObjectNames(result, m_gameLoader.ElementNamesRegexes);
            result = Utility.ReplaceRespectingQuotes(result, " and ", " && ");
            result = Utility.ReplaceRespectingQuotes(result, " or ", " || ");
            result = Utility.ReplaceRespectingQuotes(result, " xor ", " ^ ");
            result = Utility.ReplaceRespectingQuotes(result, "True", "true");
            result = Utility.ReplaceRespectingQuotes(result, "False", "false");
            // replace = with ==, but leave >= and <= alone
            result = Utility.ReplaceRegexMatchesRespectingQuotes(result, new Regex(@"([^<>])="), "$1==", false);
            result = Utility.ReplaceRespectingQuotes(result, "<>", "!=");
            result = ReplaceNotConditions(result);
            result = Utility.ReplaceRegexMatchesRespectingQuotes(result, new Regex(@"\bnot "), "!", false);
            result = Utility.ReplaceReservedVariableNames(result);
            result = Utility.ReplaceOverloadedFunctionNames(result);
            result = Utility.ReplaceObjectNames(result, m_gameLoader.ElementNamesRegexes);
            result = Utility.ConvertVariableNamesWithSpaces(result);

            return result;
        }

        private static Regex s_conditionDelimitersRegex = new Regex(@"(&&|\|\|)");

        private string ReplaceNotConditions(string input)
        {
            string[] conditions = s_conditionDelimitersRegex.Split(input);
            string result = string.Empty;

            bool isDelimiter = true;

            foreach (string condition in conditions)
            {
                isDelimiter = !isDelimiter;

                if (isDelimiter)
                {
                    result += condition;
                }
                else
                {
                    if (condition.Trim().StartsWith("not "))
                    {
                        int idx = condition.IndexOf("not ");
                        result += condition.Substring(0, idx + 4) + "(" + condition.Substring(idx + 4) + ")";
                    }
                    else
                    {
                        result += condition;
                    }
                }
            }

            return result;
        }
    }
}
