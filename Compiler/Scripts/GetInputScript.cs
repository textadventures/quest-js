using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class GetInputScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "get input"; }
        }

        public IScript Create(string script, Element proc)
        {
            string callback = Utility.GetScript(script.Substring(9).Trim());
            IScript callbackScript = ScriptFactory.CreateScript(callback);
            return new GetInputScript(ScriptFactory, callbackScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }
    }

    public class GetInputScript : ScriptBase
    {
        private IScript m_callbackScript;
        private IScriptFactory m_scriptFactory;

        public GetInputScript(IScriptFactory scriptFactory, IScript callbackScript)
        {
            m_scriptFactory = scriptFactory;
            m_callbackScript = callbackScript;
        }

        public override string Save(Context c)
        {
            return string.Format("getinput_async (function(result) {{ {0} }});",
                m_callbackScript.Save(c)
            );
        }
    }
}
