using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class ElementNameMapper
    {
        private const string k_namePrefix = "_obj";
        private Dictionary<string, string> m_map = new Dictionary<string, string>();
        private int m_count = 0;

        public string AddToMap(string elementName)
        {
            m_count++;
            string mappedName = k_namePrefix + m_count;
            m_map.Add(elementName, mappedName);
            return mappedName;
        }

        public string GetMappedName(string elementName)
        {
            return m_map[elementName];
        }
    }
}
