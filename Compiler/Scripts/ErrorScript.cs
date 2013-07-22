using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class ErrorScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "error"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ErrorScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class ErrorScript : ScriptBase
    {
        private IFunction m_function;

        public ErrorScript(IFunction function)
        {
            m_function = function;
        }

        public override string Save()
        {
            return SaveScript("error", m_function.Save());
        }
    }
}
