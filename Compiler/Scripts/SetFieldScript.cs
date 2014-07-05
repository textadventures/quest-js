using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class SetFieldScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "set"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new SetFieldScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class SetFieldScript : ScriptBase
    {
        private IFunction m_obj;
        private IFunction m_field;
        private IFunction m_value;

        public SetFieldScript(IFunction obj, IFunction field, IFunction value)
        {
            m_obj = obj;
            m_field = field;
            m_value = value;
        }

        public override string Save(Context c)
        {
            return SaveScript("set", m_obj.Save(c), m_field.Save(c), m_value.Save(c));
        }
    }
}
