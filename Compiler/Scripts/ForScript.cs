using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class ForScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "for"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            IScript loopScript = ScriptFactory.CreateScript(loop);

            if (parameters.Count() == 3)
            {
                return new ForScript(ScriptFactory, parameters[0], new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader), loopScript);
            }
            else if (parameters.Count() == 4)
            {
                return new ForScript(ScriptFactory, parameters[0], new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader), loopScript, new Expression(parameters[3], GameLoader));
            }
            else
            {
                throw new Exception(string.Format("'for' script should have 3 or 4 parameters: 'for ({0})'", param));
            }
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }
    }

    public class ForScript : ScriptBase
    {
        private IFunction m_from;
        private IFunction m_to;
        private IScript m_loopScript;
        private string m_variable;
        private IScriptFactory m_scriptFactory;
        private IFunction m_step;

        public ForScript(IScriptFactory scriptFactory, string variable, IFunction from, IFunction to, IScript loopScript)
            : this(scriptFactory, variable, from, to, loopScript, null)
        {
        }

        public ForScript(IScriptFactory scriptFactory, string variable, IFunction from, IFunction to, IScript loopScript, IFunction step)
        {
            m_scriptFactory = scriptFactory;
            m_variable = variable;
            m_from = from;
            m_to = to;
            m_loopScript = loopScript;
            m_step = step;
        }

        public override string Save(Context c)
        {
            string step = (m_step == null) ? "++" : "+=" + m_step.Save(c);
            string result = string.Format("for (var {0} = {1}; {0} <= {2}; {0}{3}) {{\n", m_variable, m_from.Save(c), m_to.Save(c), step);
            result += m_loopScript.Save(c);
            result += Environment.NewLine + "}";
            return result;
        }
    }
}
