using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class DestroyScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "destroy"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DestroyScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class DestroyScript : ScriptBase
    {
        private IFunction m_expr;

        public DestroyScript(IFunction expr)
        {
            m_expr = expr;
        }

        public override string Save(Context c)
        {
            return SaveScript("destroy", m_expr.Save(c));
        }
    }
}
