using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class DoScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "do"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 2:
                    return new DoActionScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader));
                case 3:
                    return new DoActionScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2, 3 }; }
        }
    }

    public class DoActionScript : ScriptBase
    {
        private IFunction m_obj;
        private IFunction m_action;
        private IFunction m_parameters = null;

        public DoActionScript(IFunction obj, IFunction action)
        {
            m_obj = obj;
            m_action = action;
        }

        public DoActionScript(IFunction obj, IFunction action, IFunction parameters)
            : this(obj, action)
        {
            m_parameters = parameters;
        }

        public override string Save(Context c)
        {
            string parameters = (m_parameters == null) ? null : m_parameters.Save(c);
            if (!string.IsNullOrEmpty(parameters))
            {
                return SaveScript("runscriptattribute3", m_obj.Save(c), m_action.Save(c), m_parameters.Save(c));
            }
            else
            {
                return SaveScript("runscriptattribute2", m_obj.Save(c), m_action.Save(c));
            }
        }
    }
}
