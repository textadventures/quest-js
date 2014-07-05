using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class UndoScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "undo"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new UndoScript();
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class UndoScript : ScriptBase
    {
        public UndoScript()
        {
        }

        public override string Save(Context c)
        {
            return "undo();";
        }
    }

    public class StartTransactionConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "start transaction"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new StartTransactionScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class StartTransactionScript : ScriptBase
    {
        private IFunction m_command;

        public StartTransactionScript(IFunction command)
        {
            m_command = command;
        }

        public override string Save(Context c)
        {
            return SaveScript("starttransaction", m_command.Save(c));
        }
    }
}
