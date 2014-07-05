using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest.Scripts
{
    public class RequestScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "request"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new RequestScript(parameters[0], new Expression(parameters[1], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class RequestScript : ScriptBase
    {
        private string m_request;
        private IFunction m_data;

        private static List<string> s_ignoreFunctions = new List<string>
        {
            "SetMenuBackground",
            "SetMenuForeground",
            "SetMenuHoverBackground",
            "SetMenuHoverForeground",
            "SetMenuFontName",
            "SetMenuFontSize"
        };

        public RequestScript(string request, IFunction data)
        {
            m_data = data;
            m_request = request;
        }

        private static Regex s_runScript = new Regex("\"(.*); *\" \\+ (.*)");

        public override string Save(Context c)
        {
            if (m_request == "RunScript")
            {
                string data = m_data.Save(c);
                if (s_runScript.IsMatch(data))
                {
                    Match result = s_runScript.Match(data);
                    if (s_ignoreFunctions.Contains(result.Groups[1].Value))
                    {
                        // ignore the hyperlink menu formatting functions.
                        // TO DO: Should only ignore these in web profile.
                        return string.Empty;
                    }
                    return string.Format("{0}({1});", result.Groups[1].Value, result.Groups[2].Value);
                }
                else
                {
                    if (!data.StartsWith("\"") || !data.EndsWith("\"") || data.Substring(1, data.Length - 2).Contains("\""))
                    {
                        throw new NotImplementedException("Unhandled RunScript conversion: " + data);
                    }
                    if (!data.Contains(";"))
                    {
                        return (data.Substring(1, data.Length - 2) + "();");
                    }
                    else
                    {
                        var args = data.Substring(1, data.Length - 2).Split(';');
                        return args[0].Trim() + "(" + string.Join(",", args.Skip(1).Select(a => a.Trim()).ToArray()) + ")";
                    }
                }
            }
            else
            {
                return SaveScript("request", string.Format("\"{0}\"", m_request), m_data.Save(c));
            }
        }
    }
}
