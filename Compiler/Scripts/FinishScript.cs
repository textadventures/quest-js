using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class FinishScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "finish"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new FinishScript();
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class FinishScript : ScriptBase
    {
        public FinishScript()
        {
        }

        public override string Save()
        {
            return "finish();";
        }
    }
}
