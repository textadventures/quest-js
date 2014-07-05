using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class WaitScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "wait"; }
        }

        public IScript Create(string script, Element proc)
        {
            string callback = Utility.GetScript(script.Substring(4).Trim());
            IScript callbackScript = ScriptFactory.CreateScript(callback);
            return new WaitScript(ScriptFactory, callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }
    }

    public class WaitScript : ScriptBase
    {
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public WaitScript(IScriptFactory scriptFactory, IScript callbackScript)
        {
            m_scriptFactory = scriptFactory;
            m_callbackScript = callbackScript;
        }

        public override string Save(Context c)
        {
            return string.Format("wait_async (function() {{ {0} }});",
                m_callbackScript.Save(c)
            );
        }
    }
}
