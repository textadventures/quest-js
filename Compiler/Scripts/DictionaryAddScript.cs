using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class DictionaryAddScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "dictionary add"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DictionaryAddScript(
                new Expression(parameters[0], GameLoader),
                new Expression(parameters[1], GameLoader),
                new Expression(parameters[2], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class DictionaryAddScript : ScriptBase
    {
        private IFunction m_dictionary;
        private IFunction m_key;
        private IFunction m_value;

        public DictionaryAddScript(IFunction dictionary, IFunction key, IFunction value)
        {
            m_dictionary = dictionary;
            m_key = key;
            m_value = value;
        }

        public override string Save(Context c)
        {
            return SaveScript("dictionaryadd", m_dictionary.Save(c), m_key.Save(c), m_value.Save(c));
        }
    }

    public class DictionaryRemoveScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "dictionary remove"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DictionaryRemoveScript(
                new Expression(parameters[0], GameLoader),
                new Expression(parameters[1], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class DictionaryRemoveScript : ScriptBase
    {
        private IFunction m_dictionary;
        private IFunction m_key;

        public DictionaryRemoveScript(IFunction dictionary, IFunction key)
        {
            m_dictionary = dictionary;
            m_key = key;
        }

        public override string Save(Context c)
        {
            return SaveScript("dictionaryremove", m_dictionary.Save(c), m_key.Save(c));
        }
    }
}
