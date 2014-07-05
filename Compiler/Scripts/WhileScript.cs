using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class WhileScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "while"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);
            IScript loopScript = ScriptFactory.CreateScript(loop);

            return new WhileScript(new Expression(param, GameLoader), loopScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }
    }

    public class WhileScript : ScriptBase
    {
        private IFunction m_expression;
        private IScript m_loopScript;

        public WhileScript(IFunction expression, IScript loopScript)
        {
            m_expression = expression;
            m_loopScript = loopScript;
        }

        public override string Save(Context c)
        {
            string result = string.Format("while ({0}) {{\n", m_expression.Save(c));
            result += m_loopScript.Save(c);
            result += Environment.NewLine + "}";
            return result;
        }

        public override string Keyword
        {
            get
            {
                return "while";
            }
        }
    }
}
