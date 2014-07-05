using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class PictureScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "picture"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new PictureScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class PictureScript : ScriptBase
    {
        private IFunction m_filename;

        public PictureScript(IFunction function)
        {
            m_filename = function;
        }

        public override string Save(Context c)
        {
            return SaveScript("picture", m_filename.Save(c));
        }
    }
}
