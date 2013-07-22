using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class AskScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "ask"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string callback = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 1)
            {
                throw new Exception(string.Format("'ask' script should have 1 parameter: 'ask ({0})'", param));
            }
            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new AskScript(ScriptFactory, new Expression(parameters[0], GameLoader), callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }
    }

    public class AskScript : ScriptBase
    {
        private IFunction m_caption;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public AskScript(IScriptFactory scriptFactory, IFunction caption, IScript callbackScript)
        {
            m_scriptFactory = scriptFactory;
            m_caption = caption;
            m_callbackScript = callbackScript;
        }

        public override string Save()
        {
            return string.Format("ask ({0}, function(result) {{ {1} }});",
                m_caption.Save(),
                m_callbackScript.Save()
            );
        }
    }
}
