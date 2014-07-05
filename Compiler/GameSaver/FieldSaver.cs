using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TextAdventures.Quest.Scripts;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest
{
    internal class FieldSaver
    {
        private Dictionary<Type, IFieldSaver> m_savers = new Dictionary<Type, IFieldSaver>();
        private CommandPatternSaver m_commandPatternSaver = new CommandPatternSaver();

        public FieldSaver()
        {
            // Use Reflection to create instances of all IFieldSavers
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IFieldSaver)))
            {
                AddSaver((IFieldSaver)Activator.CreateInstance(t));
            }
        }

        private void AddSaver(IFieldSaver saver)
        {
            if (saver.AppliesTo != null)
            {
                m_savers.Add(saver.AppliesTo, saver);
            }
        }

        public void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
        {
            if (value == null) return;
            IFieldSaver saver;
            if (TryGetSaver(element, attribute, value.GetType(), out saver))
            {
                saver.Save(writer, element, attribute, value, isFinal);
            }
            else
            {
                throw new Exception(string.Format("ERROR: No FieldSaver for attribute {0}, type: {1}", attribute, value.GetType().ToString()));
            }
        }

        private bool TryGetSaver(Element element, string attribute, Type type, out IFieldSaver saver)
        {
            if (element.Type == ObjectType.Command && attribute == "pattern" && type == typeof(string))
            {
                saver = m_commandPatternSaver;
                return true;
            }

            if (m_savers.TryGetValue(type, out saver)) return true;

            foreach (KeyValuePair<Type, IFieldSaver> s in m_savers)
            {
                if (s.Key.IsAssignableFrom(type))
                {
                    saver = s.Value;
                    return true;
                }
            }

            return false;
        }

        private interface IFieldSaver
        {
            Type AppliesTo { get; }
            void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal);
        }

        private abstract class FieldSaverBase : IFieldSaver
        {
            public abstract Type AppliesTo { get; }
            public abstract void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal);

            protected void WriteAttribute(GameWriter writer, Element element, string attribute, string value, bool isFinal)
            {
                if (attribute.Contains(" "))
                {
                    attribute = attribute.Replace(" ", Utility.SpaceReplacementString);
                }

                writer.AddLine(string.Format("\"{0}\": {1}{2}", attribute, value, isFinal ? "" : ","));
            }
        }

        private class StringSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(string); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                string strValue = (string)value;
                base.WriteAttribute(writer, element, attribute, Utility.EscapeString(strValue), isFinal);
            }
        }

        private class BooleanSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(bool); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                bool boolVal = (bool)value;
                base.WriteAttribute(writer, element, attribute, boolVal ? "true" : "false", isFinal);
            }
        }

        private class StringListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestList<string>); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                QuestList<string> list = (QuestList<string>)value;
                string saveString = "[" + String.Join(", ", list.Select(s => Utility.EscapeString(s)).ToArray()) + "]";
                base.WriteAttribute(writer, element, attribute, saveString, isFinal);
            }
        }

        private abstract class DictionarySaverBase<T> : FieldSaverBase
        {
            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                QuestDictionary<T> dictionary = (QuestDictionary<T>)value;

                string result = "{";

                if (dictionary.Dictionary.Count > 0)
                {
                    int count = 0;
                    foreach (var item in dictionary.Dictionary)
                    {
                        count++;
                        result += string.Format("\"{0}\": {1}{2} ", item.Key, ValueSaver(item.Value), count == dictionary.Dictionary.Count ? "" : ",");
                    }
                }
                else
                {
                    result += string.Format("\"{0}\": {1} ", "__dummyKey", ValueSaver(default(T)));
                }

                result += "}";
                base.WriteAttribute(writer, element, attribute, result, isFinal);
            }

            protected abstract string ValueSaver(T value);
        }

        private class StringDictionarySaver : DictionarySaverBase<string>
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<string>); }
            }

            protected override string ValueSaver(string value)
            {
                return Utility.EscapeString(value);
            }
        }

        private class ObjectListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestList<Element>); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                QuestList<Element> list = (QuestList<Element>)value;

                if (list.Count == 0)
                {
                    // Just write a blank list
                    base.WriteAttribute(writer, element, attribute, "new Array()", isFinal);
                }

                foreach (Element item in list)
                {
                    writer.AddPostElementScript(element, string.Format(
                        "objectListReferences.push([\"object_{0}\", \"{1}\", \"object_{2}\"]);",
                        element.MetaFields[MetaFieldDefinitions.MappedName],
                        attribute,
                        item.Name));
                }
            }
        }

        private class ScriptSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(IScript); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                IScript script = (IScript)value;
                string savedScript = script.Save(new Context());
                if (savedScript.Trim().Length > 0)
                {
                    // TO DO: Will need to extract variables for parameters to a "useanything" script in the same way

                    if (element.Type == ObjectType.Command)
                    {
                        List<string> variables = GetCommandPatternVariableNames(element);
                        string commandVariables = string.Empty;

                        foreach (string variable in variables)
                        {
                            commandVariables += string.Format("var {0} = parameters['{0}'];\n", variable);
                        }

                        savedScript = commandVariables + savedScript;
                        base.WriteAttribute(writer, element, attribute, string.Format("function(parameters) {{ {0} }}", savedScript), isFinal);
                    }
                    else
                    {
                        string parameters = string.Empty;
                        if (attribute.StartsWith("changed"))
                        {
                            parameters = "oldvalue";
                        }
                        base.WriteAttribute(writer, element, attribute, string.Format("function({1}) {{ {0} }}", savedScript, parameters), isFinal);
                    }
                }
            }

            private static Regex s_commandPatterns = new Regex(@"\(\?\<(\w+)\>");

            private List<string> GetCommandPatternVariableNames(Element element)
            {
                List<string> result = new List<string>();
                string pattern = element.Fields[FieldDefinitions.Pattern];

                foreach (Match m in s_commandPatterns.Matches(pattern))
                {
                    string varName = m.Groups[1].Value;
                    if (!result.Contains(varName)) result.Add(varName);
                }

                if (element.Fields.FieldNames.Contains("multiple"))
                {
                    result.Add("multiple");
                }

                return result;
            }
        }

        private class IntSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(int); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                int number = (int)value;
                base.WriteAttribute(writer, element, attribute, number.ToString(), isFinal);
            }
        }

        private class DoubleSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(double); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                double number = (double)value;
                base.WriteAttribute(writer, element, attribute, number.ToString(), isFinal);
            }
        }

        private class DelegateImplementationSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(DelegateImplementation); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                DelegateImplementation impl = (DelegateImplementation)value;

                // TO DO: "function()" will need to include parameter names

                base.WriteAttribute(writer, element, attribute,
                    string.Format("function() {{ {0} }}", impl.Implementation.Fields[FieldDefinitions.Script].Save(new Context())), isFinal);
            }
        }

        private class ScriptDictionarySaver : DictionarySaverBase<IScript>
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<IScript>); }
            }

            protected override string ValueSaver(IScript value)
            {
                return string.Format("function() {{ {0} }}", value == null ? string.Empty : value.Save(new Context()));
            }
        }

        private class ObjectReferenceSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(Element); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                Element reference = (Element)value;
                if (writer.IsElementWritten(reference))
                {
                    base.WriteAttribute(writer, element, attribute, ((Element)value).MetaFields[MetaFieldDefinitions.MappedName], isFinal);
                }
                else
                {
                    writer.AddPostElementScript(element, string.Format("objectReferences.push([\"{0}\", \"{1}\", \"{2}\"]);",
                        element.MetaFields[MetaFieldDefinitions.MappedName],
                        attribute,
                        reference.MetaFields[MetaFieldDefinitions.MappedName]));
                }
            }
        }

        private class ObjectDictionarySaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<Element>); }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                QuestDictionary<Element> dictionary = (QuestDictionary<Element>)value;

                if (dictionary.Dictionary.Count == 0)
                {
                    // Just write a blank dictionary
                    base.WriteAttribute(writer, element, attribute, "new Object()", isFinal);
                }

                foreach (var item in dictionary.Dictionary)
                {
                    writer.AddPostElementScript(element, string.Format(
                        "objectDictionaryReferences.push([\"object_{0}\", \"{1}\", \"{2}\", \"object_{3}\"]);",
                        element.MetaFields[MetaFieldDefinitions.MappedName],
                        attribute,
                        item.Key,
                        item.Value.MetaFields[MetaFieldDefinitions.MappedName]));
                }
            }
        }

        private class CommandPatternSaver : StringSaver
        {
            // Match named groups within a regex e.g.
            //      look at (?<object>.*)
            // and replace with
            //      look at (.*)
            private System.Text.RegularExpressions.Regex replaceNamedGroups =
                new System.Text.RegularExpressions.Regex(@"\(\?\<(.*?)\>");

            public override Type AppliesTo
            {
                get { return null; }
            }

            public override void Save(GameWriter writer, Element element, string attribute, object value, bool isFinal)
            {
                string pattern = (string)value;
                GenerateUniqueGroupNames groupNamer = new GenerateUniqueGroupNames();
                pattern = replaceNamedGroups.Replace(pattern, groupNamer.MatchEvaluator);
                base.Save(writer, element, attribute, pattern, isFinal);
            }

            private class GenerateUniqueGroupNames
            {
                private const string prefix = "(?<";
                private const string suffix = ">";
                private List<string> groupNames = new List<string>();
                private int groupCount = 0;

                public string MatchEvaluator(Match match)
                {
                    string groupName = match.Groups[1].Value;
                    groupCount++;
                    if (!groupNames.Contains(groupName))
                    {
                        groupNames.Add(groupName);
                        return prefix + groupName + suffix;
                    }
                    else
                    {
                        return prefix + string.Format("g{0}_map_{1}", groupCount, groupName) + suffix;
                    }
                }
            }
        }
    }
}
