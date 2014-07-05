using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class ShowMenuScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "show menu"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string callback = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 3)
            {
                throw new Exception(string.Format("'show menu' script should have 3 parameters: 'show menu ({0})'", param));
            }
            IScript callbackScript = ScriptFactory.CreateScript(callback);

            return new ShowMenuScript(ScriptFactory, new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader), callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }
    }

    public class ShowMenuScript : ScriptBase
    {
        private IFunction m_caption;
        private IFunction m_options;
        private IFunction m_allowCancel;
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public ShowMenuScript(IScriptFactory scriptFactory, IFunction caption, IFunction options, IFunction allowCancel, IScript callbackScript)
        {
            m_scriptFactory = scriptFactory;
            m_caption = caption;
            m_options = options;
            m_allowCancel = allowCancel;
            m_callbackScript = callbackScript;
        }

        public override string Save(Context c)
        {
            return string.Format("showmenu_async ({0}, {1}, {2}, function(result) {{ {3} }});",
                m_caption.Save(c),
                m_options.Save(c),
                m_allowCancel.Save(c),
                m_callbackScript.Save(c)
            );
        }
    }
}
