using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class MsgScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "msg"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            if (GameLoader.Version >= WorldModelVersion.v540)
            {
                return new FunctionCallScript(GameLoader, "OutputText",
                                              new List<IFunction> { new Expression(parameters[0], GameLoader) }, null);
            }
            return new MsgScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class MsgScript : ScriptBase
    {
        private IFunction m_function;

        public MsgScript(IFunction function)
        {
            m_function = function;
        }

        public override string Save(Context c)
        {
            return SaveScript("msg", m_function.Save(c));
        }
    }
}
