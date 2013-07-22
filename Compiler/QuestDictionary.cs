using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class QuestDictionary<T>
    {
        public QuestDictionary()
        {
        }

        public QuestDictionary(IDictionary<string, T> dictionary)
        {
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    m_dictionary.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private Dictionary<string, T> m_dictionary = new Dictionary<string, T>();

        public void Add(string key, T value)
        {
            m_dictionary.Add(key, value);
        }

        public Dictionary<string, T> Dictionary { get { return m_dictionary; } }
    }
}
