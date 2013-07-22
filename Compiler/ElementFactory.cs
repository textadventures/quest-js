using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class ElementFactory
    {
        private GameLoader m_loader;
        private Dictionary<ObjectType, string> m_defaultTypeNames = new Dictionary<ObjectType, string>();

        public ElementFactory(GameLoader loader)
        {
            m_loader = loader;
            m_defaultTypeNames.Add(ObjectType.Object, "defaultobject");
            m_defaultTypeNames.Add(ObjectType.Exit, "defaultexit");
            m_defaultTypeNames.Add(ObjectType.Command, "defaultcommand");
            m_defaultTypeNames.Add(ObjectType.Game, "defaultgame");
            m_defaultTypeNames.Add(ObjectType.TurnScript, "defaultturnscript");
        }

        public Element CreateCommand(string name)
        {
            return CreateObject(name, null, ObjectType.Command);
        }

        public Element CreateObject(string name)
        {
            return CreateObject(name, null, ObjectType.Object);
        }

        public Element CreateObject(string name, Element parent, ObjectType type)
        {
            Element result = CreateElement(ElementType.Object, name);
            result.Parent = parent;
            result.Type = type;
            result.Fields.AddTypeName(m_defaultTypeNames[type]);
            return result;
        }

        public Element CreateGame()
        {
            return CreateObject("game", null, ObjectType.Game);
        }

        public Element CreateTurnScript(string name, Element parent)
        {
            return CreateObject(name, parent, ObjectType.TurnScript);
        }

        public Element CreateFunction(string name)
        {
            return CreateElement(ElementType.Function, name);
        }

        public Element CreateElement(ElementType type, string name)
        {
            string mappedName = m_loader.NameMapper.AddToMap(name);
            Element result = new Element(type, m_loader);
            result.Name = name;
            result.MetaFields[MetaFieldDefinitions.MappedName] = mappedName;
            m_loader.AddElement(result);
            return result;
        }

        public Element CreateTemplate(string name, string text, bool isCommandTemplate)
        {
            string id = GetUniqueID("template");
            Element template = CreateElement(ElementType.Template, id);
            template.Fields[FieldDefinitions.TemplateName] = name;
            template.Fields[FieldDefinitions.Text] = text;
            if (isCommandTemplate)
            {
                template.Fields[FieldDefinitions.IsVerb] = true;
            }
            return template;
        }

        private Dictionary<string, int> m_nextUniqueID = new Dictionary<string, int>();

        public string GetUniqueID()
        {
            return GetUniqueID(null);
        }

        public string GetUniqueID(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "k";
            if (!m_nextUniqueID.ContainsKey(prefix))
            {
                m_nextUniqueID.Add(prefix, 0);
            }

            m_nextUniqueID[prefix]++;
            return prefix + m_nextUniqueID[prefix].ToString();
        }

    }
}
