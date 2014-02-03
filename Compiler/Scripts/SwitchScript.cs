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
            var cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, proc);

            return new SwitchScript(new Expression(param, GameLoader), cases, defaultScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }

        private List<Tuple<List<IFunction>, IScript>> ProcessCases(string cases, out IScript defaultScript, Element proc)
        {
            bool finished = false;
            string remainingCases;
            string afterExpr;
            var result = new List<Tuple<List<IFunction>, IScript>>();
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

                        var matchList = Utility.SplitParameter(expr);
                        var expressions = matchList.Select(match => new Expression(match, GameLoader)).Cast<IFunction>().ToList();

                        result.Add(Tuple.Create(expressions, script));
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

        public SwitchScript(IFunction expression, List<Tuple<List<IFunction>, IScript>> cases, IScript defaultScript)
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
            private List<Tuple<List<IFunction>, IScript>> m_cases;

            public SwitchCases(SwitchScript parent, List<Tuple<List<IFunction>, IScript>> cases)
            {
                m_cases = cases;
            }

            public string Save()
            {
                string result = string.Empty;
                foreach (var caseItem in m_cases)
                {
                    foreach (var expression in caseItem.Item1)
                    {
                        result += string.Format("case {0}:\n", expression.Save());
                    }
                    result += string.Format("{0}\nbreak;\n", caseItem.Item2.Save());
                }
                return result;
            }
        }
    }
}