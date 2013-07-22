using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest
{
    public static class Utility
    {
        private const string k_spaceReplacementString = "___SPACE___";

        public static string GetParameter(string script)
        {
            string afterParameter;
            return GetParameter(script, out afterParameter);
        }

        public static string GetParameter(string script, out string afterParameter)
        {
            return GetParameterInt(script, '(', ')', out afterParameter);
        }

        public static string GetScript(string script)
        {
            string afterScript;
            return GetScript(script, out afterScript);
        }

        public static string GetScript(string script, out string afterScript)
        {
            afterScript = null;
            string obscuredScript = ObscureStrings(script);
            int bracePos = obscuredScript.IndexOf('{');
            int crlfPos = obscuredScript.IndexOf("\n", StringComparison.Ordinal);
            int commentPos = obscuredScript.IndexOf("//", StringComparison.Ordinal);
            if (crlfPos == -1) return script;

            if (bracePos == -1 || crlfPos < bracePos || (commentPos != -1 && commentPos < bracePos && commentPos < crlfPos))
            {
                afterScript = script.Substring(crlfPos + 1);
                return script.Substring(0, crlfPos);
            }

            string beforeBrace = script.Substring(0, bracePos);
            string insideBraces = GetParameterInt(script, '{', '}', out afterScript);

            string result;

            if (insideBraces.Contains("\n"))
            {
                result = beforeBrace + "{" + insideBraces + "}";
            }
            else
            {
                // maybe not necessary or correct, maybe always have it in braces
                result = beforeBrace + insideBraces;
            }

            return result;
        }

        private static string GetParameterInt(string text, char open, char close, out string afterParameter)
        {
            afterParameter = null;
            string obscuredText = ObscureStrings(text);
            int start = obscuredText.IndexOf(open);
            if (start == -1) return null;

            bool finished = false;
            int braceCount = 1;
            int pos = start;

            while (!finished)
            {
                pos++;
                string curChar = obscuredText.Substring(pos, 1);
                if (curChar == open.ToString()) braceCount++;
                if (curChar == close.ToString()) braceCount--;
                if (braceCount == 0 || pos == obscuredText.Length - 1) finished = true;
            }

            if (braceCount != 0)
            {
                throw new Exception(string.Format("Missing '{0}'", close));
            }

            afterParameter = text.Substring(pos + 1);
            return text.Substring(start + 1, pos - start - 1);
        }

        public static string RemoveSurroundingBraces(string input)
        {
            input = input.Trim();
            if (input.StartsWith("{") && input.EndsWith("}"))
            {
                return input.Substring(1, input.Length - 2);
            }
            else
            {
                return input;
            }
        }

        public static string GetTextAfter(string text, string startsWith)
        {
            return text.Substring(startsWith.Length);
        }

        public static List<string> SplitParameter(string text)
        {
            List<string> result = new List<string>();
            bool inQuote = false;
            bool processThisCharacter;
            bool processNextCharacter = true;
            int bracketCount = 0;
            string curParam = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                processThisCharacter = processNextCharacter;
                processNextCharacter = true;

                string curChar = text.Substring(i, 1);

                if (processThisCharacter)
                {
                    if (curChar == "\\")
                    {
                        // Don't process the character after a backslash
                        processNextCharacter = false;
                    }
                    else if (curChar == "\"")
                    {
                        inQuote = !inQuote;
                    }
                    else
                    {
                        if (!inQuote)
                        {
                            if (curChar == "(") bracketCount++;
                            if (curChar == ")") bracketCount--;
                            if (bracketCount == 0 && curChar == ",")
                            {
                                result.Add(curParam.Trim());
                                curParam = string.Empty;
                                continue;
                            }
                        }
                    }
                }

                curParam += curChar;
            }
            result.Add(curParam.Trim());

            return result;
        }

        public static void ResolveVariableName(ref string name, out string obj, out string variable)
        {
            int eqPos = name.LastIndexOf(".");
            if (eqPos == -1)
            {
                obj = null;
                variable = name;
                return;
            }

            obj = name.Substring(0, eqPos);
            variable = name.Substring(eqPos + 1);
        }

        private static Regex s_detectComments = new Regex("//");

        public static string ReplaceRegexMatchesRespectingQuotes(string input, Regex regex, string replaceWith, bool replaceInsideQuote)
        {
            return ReplaceRespectingQuotes(input, replaceInsideQuote, text => regex.Replace(text, replaceWith));
        }

        public static string ReplaceRespectingQuotes(string input, string searchFor, string replaceWith)
        {
            return ReplaceRespectingQuotes(input, false, text => text.Replace(searchFor, replaceWith));
        }

        public static string ConvertVariableNamesWithSpaces(string input)
        {
            return ReplaceRespectingQuotes(input, false, text =>
            {
                // Split the text up into a word array, for example
                // "my variable = 12" becomes "my", "variable", "=", "12".
                // If any two adjacent elements end and begin with word characters,
                // (in this case, "my" and "variable" because "y" and "v" match
                // regex "\w"), then we remove the space and replace it with
                // our space placeholder. However, if one of the two words is a
                // keyword ("and", "not" etc.), we don't convert it.
                string[] words = text.Split(' ');
                string result = words[0];

                if (words.Length == 1) return result;

                for (int i = 1; i < words.Length; i++)
                {
                    if (IsSplitVariableName(words[i - 1], words[i]))
                    {
                        result += k_spaceReplacementString;
                    }
                    else
                    {
                        result += " ";
                    }
                    result += words[i];
                }

                return result;
            });
        }

        public static string SpaceReplacementString { get { return k_spaceReplacementString; } }

        // Given two words e.g. "my" and "variable", see if they together comprise a variable name

        private static Regex s_wordRegex1 = new System.Text.RegularExpressions.Regex(@"(\w+)$");
        private static Regex s_wordRegex2 = new System.Text.RegularExpressions.Regex(@"^(\w+)");
        private static List<string> s_keywords = new List<string> { "and", "or", "xor", "not", "if", "in" };

        private static bool IsSplitVariableName(string word1, string word2)
        {
            if (!(s_wordRegex1.IsMatch(word1) && s_wordRegex2.IsMatch(word2))) return false;

            string word1last = s_wordRegex1.Match(word1).Groups[1].Value;
            string word2first = s_wordRegex2.Match(word2).Groups[1].Value;

            if (s_keywords.Contains(word1last)) return false;
            if (s_keywords.Contains(word2first)) return false;
            return true;
        }

        public static IList<string> ExpressionKeywords
        {
            get { return s_keywords.AsReadOnly(); }
        }

        private static string ReplaceRespectingQuotes(string input, bool replaceInsideQuote, Func<string, string> replaceFunction)
        {
            // We ignore regex matches which appear within quotes by splitting the string
            // at the position of quote marks, and then alternating whether we replace or not.
            string[] sections = SplitQuotes(input);
            string result = string.Empty;

            bool insideQuote = false;
            for (int i = 0; i <= sections.Length - 1; i++)
            {
                string section = sections[i];
                bool doReplace = (insideQuote && replaceInsideQuote) || (!insideQuote && !replaceInsideQuote);
                if (doReplace)
                {
                    result += replaceFunction(section);
                }
                else
                {
                    result += section;
                }
                if (i < sections.Length - 1) result += "\"";
                insideQuote = !insideQuote;
            }

            return result;
        }

        private static string[] SplitQuotes(string text)
        {
            List<string> result = new List<string>();
            bool processThisCharacter;
            bool processNextCharacter = true;
            string curParam = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                processThisCharacter = processNextCharacter;
                processNextCharacter = true;

                string curChar = text.Substring(i, 1);

                if (processThisCharacter)
                {
                    if (curChar == "\\")
                    {
                        // Don't process the character after a backslash
                        processNextCharacter = false;
                    }
                    else
                    {
                        if (curChar == "\"")
                        {
                            result.Add(curParam);
                            curParam = string.Empty;
                            continue;
                        }
                    }
                }

                curParam += curChar;
            }
            result.Add(curParam);

            return result.ToArray();
        }

        public static string RemoveComments(string input)
        {
            if (!input.Contains("//")) return input;
            if (input.Contains("\n"))
            {
                return RemoveCommentsMultiLine(input);
            }

            // Replace any occurrences of "//" which are inside string expressions. Then any occurrences of "//"
            // which remain mark the beginning of a comment.
            string obfuscateDoubleSlashesInsideStrings = ReplaceRegexMatchesRespectingQuotes(input, s_detectComments, "--", true);
            if (obfuscateDoubleSlashesInsideStrings.Contains("//"))
            {
                return input.Substring(0, obfuscateDoubleSlashesInsideStrings.IndexOf("//"));
            }
            return input;
        }

        private static string RemoveCommentsMultiLine(string input)
        {
            List<string> output = new List<string>();
            foreach (string inputLine in (input.Split(new string[] { "\n" }, StringSplitOptions.None)))
            {
                output.Add(RemoveComments(inputLine));
            }
            return string.Join("\n", output.ToArray());
        }

        public static List<string> SplitIntoLines(string text)
        {
            List<string> lines = new List<string>(text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
            List<string> result = new List<string>();
            foreach (string line in lines)
            {
                string trimLine = line.Trim();
                if (trimLine.Length > 0) result.Add(trimLine);
            }
            return result;
        }

        public static string SafeXML(string input)
        {
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string[] s_listSplitDelimiters = new string[] { "; ", ";" };

        public static string[] ListSplit(string value)
        {
            return value.Split(s_listSplitDelimiters, StringSplitOptions.None);
        }

        public static string ObscureStrings(string input)
        {
            string[] sections = SplitQuotes(input);
            string result = string.Empty;

            bool insideQuote = false;
            for (int i = 0; i <= sections.Length - 1; i++)
            {
                string section = sections[i];
                if (insideQuote)
                {
                    result = result + new string('-', section.Length);
                }
                else
                {
                    result = result + section;
                }
                if (i < sections.Length - 1) result += "\"";
                insideQuote = !insideQuote;
            }
            return result;
        }

        public static bool IsRegexMatch(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }

        public static int GetMatchStrength(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            // The idea is that you have a regex like
            //          look at (?<object>.*)
            // And you have a string like
            //          look at thing
            // The strength is the length of the "fixed" bit of the string, in this case "look at ".
            // So we calculate this as the length of the input string, minus the length of the
            // text that matches the named groups.

            int lengthOfTextMatchedByGroups = 0;

            foreach (string groupName in regex.GetGroupNames())
            {
                // exclude group names like "0", we only want the explicitly named groups
                if (!TextAdventures.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    lengthOfTextMatchedByGroups += groupMatch.Length;
                }
            }

            return input.Length - lengthOfTextMatchedByGroups;
        }

        public static QuestDictionary<string> Populate(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            QuestDictionary<string> result = new QuestDictionary<string>();

            foreach (string groupName in regex.GetGroupNames())
            {
                if (!TextAdventures.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    result.Add(groupName, groupMatch);
                }
            }

            return result;
        }

        public static string ConvertVerbSimplePattern(string pattern)
        {
            // For verbs, we replace "eat; consume; munch" with
            // "^eat (?<object>.*)$|^consume (?<object>.*)$|^munch (?<object>.*)$"

            // Optionally the position of the object can be specified, for example
            // "switch #object# on" would become "^switch (?<object>.*) on$"

            string[] verbs = Utility.ListSplit(pattern);
            string result = string.Empty;
            foreach (string verb in verbs)
            {
                if (result.Length > 0) result += "|";
                const string objectRegex = "(?<object>.*)";

                string textToAdd;
                if (verb.Contains("#object#"))
                {
                    textToAdd = "^" + verb.Replace("#object#", objectRegex) + "$";
                }
                else
                {
                    textToAdd = "^" + verb + " " + objectRegex + "$";
                }
                result += textToAdd;
            }

            return result;
        }

        public static string ConvertVerbSimplePatternForEditor(string pattern)
        {
            // For verbs, we replace "eat; consume; munch" with
            // "eat #object#; consume #object#; munch #object#"

            string[] verbs = Utility.ListSplit(pattern);
            string result = string.Empty;
            foreach (string verb in verbs)
            {
                if (result.Length > 0) result += "; ";

                string textToAdd;
                if (verb.Contains("#object#"))
                {
                    textToAdd = verb;
                }
                else
                {
                    textToAdd = verb + " #object#";
                }
                result += textToAdd;
            }

            return result;
        }


        // Valid attribute names:
        //  - must not start with a number
        //  - must not contain keywords "and", "or" etc.
        //  - can contain spaces, but not at the beginning or end
        private static Regex s_validAttributeName = new Regex(@"^[A-Za-z][\w ]*\w$");

        public static bool IsValidAttributeName(string name)
        {
            if (!s_validAttributeName.IsMatch(name)) return false;
            if (name.Split(' ').Any(w => s_keywords.Contains(w))) return false;
            return true;
        }

        private static List<string> s_jsKeywords = new List<string> { "var", "continue", "default" };
        private static List<Regex> s_jsKeywordsRegex = null;

        public static string ReplaceReservedVariableNames(string input)
        {
            if (s_jsKeywordsRegex == null)
            {
                s_jsKeywordsRegex = CreateKeywordRegexList(s_jsKeywords);
            }
            return ReplaceVariableNames(input, s_jsKeywordsRegex, "variable_");
        }

        private static List<string> s_overloadedFunctions = new List<string> { "TypeOf", "DynamicTemplate", "Eval" };
        private static List<Regex> s_overloadedFunctionsRegex = null;

        public static string ReplaceOverloadedFunctionNames(string input)
        {
            if (s_overloadedFunctionsRegex == null)
            {
                s_overloadedFunctionsRegex = CreateKeywordRegexList(s_overloadedFunctions);
            }
            return ReplaceVariableNames(input, s_overloadedFunctionsRegex, "overloadedFunctions.");
        }

        private static List<string> s_dynamicTemplateVariableNames = new List<string> { "object", "object1", "object2", "text", "exit" };
        private static List<Regex> s_dynamicTemplateVariableRegex = null;

        public static string ReplaceDynamicTemplateVariableNames(string input)
        {
            if (s_dynamicTemplateVariableRegex == null)
            {
                s_dynamicTemplateVariableRegex = CreateKeywordRegexList(s_dynamicTemplateVariableNames);
            }
            string result = input;
            foreach (Regex regex in s_dynamicTemplateVariableRegex)
            {
                result = ReplaceRegexMatchesRespectingQuotes(result, regex, "params[\"$1\"]", false);
            }
            return result;
        }

        public static string ReplaceObjectNames(string input, List<Tuple<Regex, string>> objectReplaceRegexes)
        {
            string result = input;
            foreach (var objectReplaceRegex in objectReplaceRegexes)
            {
                result = ReplaceRespectingQuotes(result, false, text => {
                    return objectReplaceRegex.Item1.Replace(text, match => {
                        // If match is immediately after a "." character, don't do the replacement as we don't
                        // want to replace attribute names that happen to be the same as object names.
                        if (match.Index == 0 || text.Substring(match.Index - 1, 1) != ".")
                        {
                            return objectReplaceRegex.Item2;
                        }
                        else
                        {
                            return match.Value;
                        }
                    });
                });
            }
            return result;
        }

        public static List<Regex> CreateKeywordRegexList(IEnumerable<string> keywords)
        {
            return new List<Regex>(from keyword in keywords
                                   select new Regex(@"\b(" + keyword + @")\b"));
        }

        private static string ReplaceVariableNames(string input, IEnumerable<Regex> keywordRegexes, string prefix)
        {
            string result = ReplaceVariableNames(input, keywordRegexes, (text, regex) => regex.Replace(text, prefix + "$1"));

            // We don't want to change attribute names, so change back any that have been accidentally converted by the above
            result = result.Replace("." + prefix, ".");

            return result;
        }

        private static string ReplaceVariableNames(string input, IEnumerable<Regex> keywordRegexes, Func<string, Regex, string> replaceFunction)
        {
            string result = input;
            foreach (Regex regex in keywordRegexes)
            {
                result = ReplaceRespectingQuotes(result, false, text => replaceFunction(text, regex));
            }
            return result;
        }

        public static string EscapeString(string input)
        {
            if (input == null) input = string.Empty;
            return string.Format("\"{0}\"", input.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace(Environment.NewLine, "\\n"));
        }
    }
}
