using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class ListAddScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "list add"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListAddScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListAddScript : ScriptBase
    {
        private IFunction m_list;
        private IFunction m_value;

        public ListAddScript(IFunction list, IFunction value)
        {
            m_list = list;
            m_value = value;
        }

        public override string Save()
        {
            return SaveScript("listadd", m_list.Save(), m_value.Save());
        }
    }

    public class ListRemoveScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "list remove"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new ListRemoveScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class ListRemoveScript : ScriptBase
    {
        private IFunction m_list;
        private IFunction m_value;

        public ListRemoveScript(IFunction list, IFunction value)
        {
            m_list = list;
            m_value = value;
        }

        public override string Save()
        {
            return SaveScript("listremove", m_list.Save(), m_value.Save());
        }
    }
}
