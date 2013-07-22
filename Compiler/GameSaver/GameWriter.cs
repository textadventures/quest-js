using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class GameWriter
    {
        private StringBuilder data = new StringBuilder();
        private Dictionary<Element, string> postElementScript = new Dictionary<Element, string>();
        private List<Element> writtenElements = new List<Element>();

        public void AddLine(string line)
        {
            data.AppendLine(EncodeNonAsciiCharacters(line));
        }

        public string Save()
        {
            return data.ToString();
        }

        public void AddPostElementScript(Element element, string script)
        {
            string result = string.Empty;
            if (postElementScript.ContainsKey(element))
            {
                result = postElementScript[element] + Environment.NewLine;
            }
            result += script;

            postElementScript[element] = result;
        }

        public string GetPostElementScript(Element element)
        {
            if (!postElementScript.ContainsKey(element))
            {
                return string.Empty;
            }
            return postElementScript[element];
        }

        public void MarkElementWritten(Element element)
        {
            writtenElements.Add(element);
        }

        public bool IsElementWritten(Element element)
        {
            return writtenElements.Contains(element);
        }

        // from http://stackoverflow.com/questions/1615559/converting-unicode-strings-to-escaped-ascii-string
        private string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
