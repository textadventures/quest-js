using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest.Scripts
{
    public class JSScriptConstructor : IScriptConstructor
    {
        private static List<string> s_prefixFunctions = new List<string>
        {
            "StartOutputSection",
            "EndOutputSection",
            "HideOutputSection"
        };

        public string Keyword
        {
            get { return "JS."; }
        }

        private static Regex s_jsFunctionName = new Regex(@"^JS\.([\w\.\@]*)");

        public IScript Create(string script, Element proc)
        {
            var param = Utility.GetParameter(script);

            List<IFunction> expressions = null;

            if (param != null)
            {
                var parameters = Utility.SplitParameter(param);
                if (parameters.Count != 1 || parameters[0].Trim().Length != 0)
                {
                    expressions = new List<IFunction>(parameters.Select(p => new Expression(p, GameLoader)));
                }
            }

            if (!s_jsFunctionName.IsMatch(script))
            {
                throw new Exception(string.Format("Invalid JS function name in '{0}'", script));
            }

            var functionName = s_jsFunctionName.Match(script).Groups[1].Value;

            if (s_prefixFunctions.Contains(functionName))
            {
                functionName = "Js" + functionName;
            }

            return new JSScript(functionName, expressions);
        }

        public IScriptFactory ScriptFactory { set; private get; }
        public GameLoader GameLoader { set; private get; }
    }

    public class JSScript : ScriptBase
    {
        private string m_function;
        private List<IFunction> m_parameters;

        public JSScript(string function, List<IFunction> parameters)
        {
            m_function = function;
            m_parameters = parameters;
        }

        public override string Save()
        {
            return string.Format("{0} ({1})", m_function, m_parameters == null ? string.Empty : string.Join(", ", m_parameters.Select(p => p.Save())));
        }
    }
}
