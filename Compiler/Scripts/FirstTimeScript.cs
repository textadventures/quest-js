using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class FirstTimeScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "firsttime"; }
        }

        public IScript Create(string script, Element proc)
        {
            // Get script after "firsttime" keyword
            script = script.Substring(9).Trim();
            string firstTime = Utility.GetScript(script);
            IScript firstTimeScript = ScriptFactory.CreateScript(firstTime);

            return new FirstTimeScript(firstTimeScript);
        }

        public static void AddOtherwiseScript(IScript firstTimeScript, string script, IScriptFactory scriptFactory)
        {
            // Get script after "otherwise" keyword
            script = script.Substring(9).Trim();
            string otherwise = Utility.GetScript(script);
            IScript otherwiseScript = scriptFactory.CreateScript(otherwise);
            ((FirstTimeScript)firstTimeScript).SetOtherwiseScript(otherwiseScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }
    }

    public class FirstTimeScript : ScriptBase
    {
        private IScript m_firstTimeScript;
        private IScript m_otherwiseScript;
        private int m_id;
        private static int s_lastId = 0;

        public FirstTimeScript(IScript firstTimeScript)
        {
            s_lastId++;
            m_id = s_lastId;
            m_firstTimeScript = firstTimeScript;
        }

        internal void SetOtherwiseScript(IScript script)
        {
            m_otherwiseScript = script;
        }

        public override string Save()
        {
            string result = "if (!HasAttribute(GetObject(\"game\"), \"_firstTimeScriptsRun\")) set (GetObject(\"game\"), \"_firstTimeScriptsRun\", NewStringList());\n";
            result += string.Format("if ($.inArray(\"{0}\", GetObject(\"game\")._firstTimeScriptsRun) == -1) {{\n", m_id);
            result += string.Format("listadd(GetObject(\"game\")._firstTimeScriptsRun, \"{0}\");\n", m_id);
            result += m_firstTimeScript.Save();
            result += "}\n";

            if (m_otherwiseScript != null)
            {
                result += string.Format("else {{ {0} }}", m_otherwiseScript.Save());
            }

            return result;
        }
    }
}
