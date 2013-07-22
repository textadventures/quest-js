using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class SwitchScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "switch"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            IScript defaultScript;
            Dictionary<IFunction, IScript> cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, proc);

            return new SwitchScript(new Expression(param, GameLoader), cases, defaultScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }

        private Dictionary<IFunction, IScript> ProcessCases(string cases, out IScript defaultScript, Element proc)
        {
            bool finished = false;
            string remainingCases;
            string afterExpr;
            Dictionary<IFunction, IScript> result = new Dictionary<IFunction, IScript>();
            defaultScript = null;

            cases = Utility.RemoveSurroundingBraces(cases);

            while (!finished)
            {
                cases = Utility.GetScript(cases, out remainingCases);
                if (cases != null) cases = cases.Trim();

                if (!string.IsNullOrEmpty(cases))
                {
                    if (cases.StartsWith("case"))
                    {
                        string expr = Utility.GetParameter(cases, out afterExpr);
                        string caseScript = Utility.GetScript(afterExpr);
                        IScript script = ScriptFactory.CreateScript(caseScript, proc);

                        result.Add(new Expression(expr, GameLoader), script);
                    }
                    else if (cases.StartsWith("default"))
                    {
                        defaultScript = ScriptFactory.CreateScript(cases.Substring(8).Trim());
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid inside switch block: '{0}'", cases));
                    }
                }

                cases = remainingCases;
                if (string.IsNullOrEmpty(cases)) finished = true;
            }

            return result;
        }
    }

    public class SwitchScript : ScriptBase
    {
        private IFunction m_expr;
        private SwitchCases m_cases;
        private IScript m_default;

        public SwitchScript(IFunction expression, Dictionary<IFunction, IScript> cases, IScript defaultScript)
            : this(expression, defaultScript)
        {
            m_cases = new SwitchCases(this, cases);
        }

        private SwitchScript(IFunction expression, IScript defaultScript)
        {
            m_expr = expression;
            m_default = defaultScript ?? new MultiScript();
        }

        public override string Save()
        {
            string result = string.Format("switch ({0}) {{\n", m_expr.Save());
            result += m_cases.Save();
            if (m_default != null && ((MultiScript)m_default).Scripts.Count() > 0) result += "default:\n" + m_default.Save();
            result += Environment.NewLine + "}";
            return result;
        }

        private class SwitchCases
        {
            private Dictionary<IFunction, IScript> m_cases = new Dictionary<IFunction, IScript>();
            private SwitchScript m_parent;

            public SwitchCases(SwitchScript parent, Dictionary<IFunction, IScript> cases)
                : this(parent)
            {

                foreach (var switchCase in cases)
                {
                    IFunction expression = switchCase.Key;
                    IScript script = switchCase.Value;

                    m_cases.Add(expression, script);
                }
            }

            private SwitchCases(SwitchScript parent)
            {
                m_parent = parent;
            }

            public string Save()
            {
                string result = string.Empty;
                foreach (KeyValuePair<IFunction, IScript> caseItem in m_cases)
                {
                    result += string.Format("case {0}:\n{1}\nbreak;\n", caseItem.Key.Save(), caseItem.Value.Save());
                }
                return result;
            }
        }
    }
}