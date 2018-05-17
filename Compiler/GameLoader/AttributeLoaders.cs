using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TextAdventures.Quest
{
    partial class GameLoader
    {
        private delegate void AddErrorHandler(string error);

        private Dictionary<string, IAttributeLoader> m_attributeLoaders = new Dictionary<string, IAttributeLoader>();
        private Dictionary<string, IValueLoader> m_valueLoaders = new Dictionary<string, IValueLoader>();

        private void AddLoaders()
        {
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IAttributeLoader)))
            {
                AddLoader((IAttributeLoader)Activator.CreateInstance(t));
            }

            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IValueLoader)))
            {
                AddValueLoader((IValueLoader)Activator.CreateInstance(t));
            }
        }

        private void AddLoader(IAttributeLoader loader)
        {
            m_attributeLoaders.Add(loader.AppliesTo, loader);
            loader.GameLoader = this;
        }

        private void AddValueLoader(IValueLoader loader)
        {
            m_valueLoaders.Add(loader.AppliesTo, loader);
            loader.GameLoader = this;
        }

        private object ReadXmlValue(string type, XElement xml)
        {
            IValueLoader loader;

            if (m_valueLoaders.TryGetValue(type, out loader))
            {
                return loader.GetValue(xml);
            }

            AddError(string.Format("Unrecognised nested attribute type '{0}'", type));

            return null;
        }

        private interface IAttributeLoader
        {
            string AppliesTo { get; }
            void Load(Element element, string attribute, string value);
            GameLoader GameLoader { set; }
            bool SupportsMode(LoadMode mode);
        }

        private interface IValueLoader
        {
            string AppliesTo { get; }
            object GetValue(XElement xml);
            GameLoader GameLoader { set; }
        }

        private abstract class AttributeLoaderBase : IAttributeLoader
        {
            #region IAttributeLoader Members

            public abstract string AppliesTo { get; }
            public abstract void Load(Element element, string attribute, string value);

            public GameLoader GameLoader { set; protected get; }

            public virtual bool SupportsMode(LoadMode mode)
            {
                return true;
            }

            #endregion
        }

        private class SimpleStringListLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "simplestringlist"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                string[] values = GetValues(value);
                element.Fields.Set(attribute, new QuestList<string>(values));
            }

            protected string[] GetValues(string value)
            {
                string[] values;
                if (value.IndexOf("\n") >= 0)
                {
                    values = Utility.SplitIntoLines(value).ToArray();
                }
                else
                {
                    values = Utility.ListSplit(value);
                }
                return values;
            }
        }

        private class ListExtensionLoader : SimpleStringListLoader
        {
            public override string AppliesTo
            {
                get { return "listextend"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                string[] values = GetValues(value);
                // TO DO
                throw new NotImplementedException();
                //element.Fields.AddFieldExtension(attribute, new QuestList<string>(values, true));
            }
        }

        private class ObjectListLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "objectlist"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                List<string> values = new List<string>(GetValues(value));
                element.Fields.AddObjectList(attribute, values);
            }

            protected IEnumerable<string> GetValues(string value)
            {
                string[] values;
                if (value.IndexOf("\n") >= 0)
                {
                    values = Utility.SplitIntoLines(value).ToArray();
                }
                else
                {
                    values = Utility.ListSplit(value);
                }
                return values.Where(v => v.Length > 0);
            }
        }

        private class ScriptLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "script"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                element.Fields.Set(attribute, GameLoader.ScriptFactory.CreateScript(value));
            }
        }

        private class StringLoader : AttributeLoaderBase, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "string"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                element.Fields.Set(attribute, value);
            }

            public object GetValue(XElement xml)
            {
                return xml.Value;
            }
        }

        private class DoubleLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "double"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                double num;
                if (double.TryParse(value, out num))
                {
                    element.Fields.Set(attribute, num);
                }
                else
                {
                    GameLoader.AddError(string.Format("Invalid number specified '{0}.{1} = {2}'", element.Name, attribute, value));
                }
            }
        }

        private class IntLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "int"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                int num;
                if (int.TryParse(value, out num))
                {
                    element.Fields.Set(attribute, num);
                }
                else
                {
                    GameLoader.AddError(string.Format("Invalid number specified '{0}.{1} = {2}'", element.Name, attribute, value));
                }
            }
        }

        private class BooleanLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "boolean"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                switch (value)
                {
                    case "":
                    case "true":
                        element.Fields.Set(attribute, true);
                        break;
                    case "false":
                        element.Fields.Set(attribute, false);
                        break;
                    default:
                        GameLoader.AddError(string.Format("Invalid boolean specified '{0}.{1} = {2}'", element.Name, attribute, value));
                        break;
                }
            }
        }

        private class SimplePatternLoader : AttributeLoaderBase
        {
            // TO DO: It would be nice if we could also specify optional text in square brackets
            // e.g. ask man about[ the] #subject#

            private System.Text.RegularExpressions.Regex m_regex = new System.Text.RegularExpressions.Regex(
                "#([A-Za-z]\\w+)#");

            public override string AppliesTo
            {
                get { return "simplepattern"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                if (element.Fields.GetAsType<bool>("isverb"))
                {
                    LoadVerb(element, attribute, value);
                }
                else
                {
                    LoadCommand(element, attribute, value);
                }
            }

            private void LoadCommand(Element element, string attribute, string value)
            {
                value = value.Replace("(", @"\(").Replace(")", @"\)").Replace(".", @"\.");
                value = m_regex.Replace(value, MatchReplace);

                if (value.Contains("#"))
                {
                    GameLoader.AddError(string.Format("Invalid command pattern '{0}.{1} = {2}'", element.Name, attribute, value));
                }

                // Now split semi-colon separated command patterns
                string[] patterns = Utility.ListSplit(value);
                string result = string.Empty;
                foreach (string pattern in patterns)
                {
                    if (result.Length > 0) result += "|";
                    result += "^" + pattern + "$";
                }

                element.Fields.Set(attribute, result);
            }

            private string MatchReplace(System.Text.RegularExpressions.Match m)
            {
                // "#blah#" needs to be converted to "(?<blah>.*)"
                return "(?<" + m.Groups[1].Value + ">.*)";
            }

            private void LoadVerb(Element element, string attribute, string value)
            {
                element.Fields.Set(attribute, Utility.ConvertVerbSimplePattern(value));
            }

            public override bool SupportsMode(LoadMode mode)
            {
                return (mode == LoadMode.Play);
            }
        }

        private class SimpleStringDictionaryLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "simplestringdictionary"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                QuestDictionary<string> result = new QuestDictionary<string>();

                string[] values = Utility.ListSplit(value);
                foreach (string pair in values)
                {
                    if (pair.Length > 0)
                    {
                        string trimmedPair = pair.Trim();
                        int splitPos = trimmedPair.IndexOf('=');
                        if (splitPos == -1)
                        {
                            GameLoader.AddError(string.Format("Missing '=' in dictionary element '{0}' in '{1}.{2}'", trimmedPair, element.Name, attribute));
                            return;
                        }
                        string key = trimmedPair.Substring(0, splitPos).Trim();
                        string dictValue = trimmedPair.Substring(splitPos + 1).Trim();
                        result.Add(key, dictValue);
                    }
                }

                element.Fields.Set(attribute, result);
            }
        }

        private class SimpleObjectDictionaryLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "simpleobjectdictionary"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                string[] values = Utility.ListSplit(value);
                foreach (string pair in values)
                {
                    if (pair.Length > 0)
                    {
                        string trimmedPair = pair.Trim();
                        int splitPos = trimmedPair.IndexOf('=');
                        if (splitPos == -1)
                        {
                            GameLoader.AddError(string.Format("Missing '=' in dictionary element '{0}' in '{1}.{2}'", trimmedPair, element.Name, attribute));
                            return;
                        }
                        string key = trimmedPair.Substring(0, splitPos).Trim();
                        string dictValue = trimmedPair.Substring(splitPos + 1).Trim();
                        result.Add(key, dictValue);
                    }
                }

                element.Fields.AddObjectDictionary(attribute, result);
            }
        }

        private class ObjectReferenceLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "object"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                if (value == "key")
                {
                    value = "key ";
                }
                element.Fields.AddObjectRef(attribute, value);
            }
        }
    }
}
