using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class PlaySoundScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "play sound"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new PlaySoundScript(
                new Expression(parameters[0], GameLoader),
                new Expression(parameters[1], GameLoader),
                new Expression(parameters[2], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class PlaySoundScript : ScriptBase
    {
        private IFunction m_filename;
        private IFunction m_synchronous;
        private IFunction m_loop;

        public PlaySoundScript(IFunction function, IFunction synchronous, IFunction loop)
        {
            m_filename = function;
            m_synchronous = synchronous;
            m_loop = loop;
        }

        public override string Save(Context c)
        {
            return SaveScript("playsound", m_filename.Save(c), m_synchronous.Save(c), m_loop.Save(c));
        }
    }

    public class StopSoundScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "stop sound"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new StopSoundScript();
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class StopSoundScript : ScriptBase
    {
        public StopSoundScript()
        {
        }

        public override string Save(Context c)
        {
            return "stopsound();";
        }
    }
}
