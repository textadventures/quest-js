using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class OnReadyScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "on ready"; }
        }

        public IScript Create(string script, Element proc)
        {
            string callback = Utility.GetScript(script.Substring(8).Trim());
            IScript callbackScript = ScriptFactory.CreateScript(callback);
            return new OnReadyScript(ScriptFactory, callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }
    }

    public class OnReadyScript : ScriptBase
    {
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public OnReadyScript(IScriptFactory scriptFactory, IScript callbackScript)
        {
            m_scriptFactory = scriptFactory;
            m_callbackScript = callbackScript;
        }

        public override string Save(Context c)
        {
            return string.Format("on_ready (function() {{ {0} }});",
                m_callbackScript.Save(c)
            );
        }
    }
}
