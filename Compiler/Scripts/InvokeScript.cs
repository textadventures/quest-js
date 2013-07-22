using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class InvokeScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "invoke"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 1:
                    return new InvokeScript(new Expression(parameters[0], GameLoader));
                case 2:
                    return new InvokeScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1, 2 }; }
        }
    }

    public class InvokeScript : ScriptBase
    {
        private IFunction m_script;
        private IFunction m_parameters = null;

        public InvokeScript(IFunction script)
        {
            m_script = script;
        }

        public InvokeScript(IFunction script, IFunction parameters)
            : this(script)
        {
            m_parameters = parameters;
        }

        public override string Save()
        {
            string parameters = (m_parameters == null) ? null : m_parameters.Save();
            if (string.IsNullOrEmpty(parameters))
            {
                return SaveScript("invoke", m_script.Save());
            }
            else
            {
                return SaveScript("invoke", m_script.Save(), parameters);
            }
        }
    }
}
