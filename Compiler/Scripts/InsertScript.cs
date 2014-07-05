using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class InsertScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "insert"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new InsertScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class InsertScript : ScriptBase
    {
        private IFunction m_filename;

        public InsertScript(IFunction filename)
        {
            m_filename = filename;
        }

        public override string Save(Context c)
        {
            return SaveScript("insertHtml", m_filename.Save(c));
        }
    }
}
