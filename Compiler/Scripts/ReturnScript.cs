using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class ReturnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "return"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            throw new Exception("Invalid constructor for 'return' script");
        }

        protected override IScript CreateInt(List<string> parameters, Element proc)
        {
            return new ReturnScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }

        protected override bool RequireProcedure
        {
            get
            {
                return true;
            }
        }
    }

    public class ReturnScript : ScriptBase
    {
        private IFunction m_returnValue;

        public ReturnScript(IFunction returnValue)
        {
            m_returnValue = returnValue;
        }

        public override string Save(Context c)
        {
            return SaveScript("return", m_returnValue.Save(c));
        }
    }
}
