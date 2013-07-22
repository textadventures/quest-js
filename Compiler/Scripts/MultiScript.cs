using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class MultiScript : ScriptBase
    {
        private List<IScript> m_scripts;

        public MultiScript(params IScript[] scripts)
        {
            m_scripts = new List<IScript>(scripts);
        }

        private MultiScript() { }

        public void Add(params IScript[] scripts)
        {
            m_scripts.AddRange(scripts);
        }

        public IEnumerable<IScript> Scripts
        {
            get
            {
                return m_scripts.AsReadOnly();
            }
        }

        public override string Line
        {
            get
            {
                string result = string.Empty;
                foreach (IScript script in m_scripts)
                {
                    result += script.Line + Environment.NewLine;
                }
                return result;
            }
            set
            {
                throw new Exception("Cannot set Line in MultiScript");
            }
        }

        public override string Save()
        {
            string result = string.Empty;

            foreach (IScript script in m_scripts)
            {
                if (result.Length > 0) result += Environment.NewLine;
                result += script.Save();
            }

            return result;
        }
    }
}
