using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ElementTypeInfo : Attribute
    {
        public ElementTypeInfo(string name)
        {
            Name = name;
        }
        public string Name;
    }

    public enum ElementType
    {
        [ElementTypeInfo("include")]
        IncludedLibrary,
        [ElementTypeInfo("implied")]
        ImpliedType,
        [ElementTypeInfo("template")]
        Template,
        [ElementTypeInfo("dynamictemplate")]
        DynamicTemplate,
        [ElementTypeInfo("delegate")]
        Delegate,
        [ElementTypeInfo("object")]
        Object,
        [ElementTypeInfo("type")]
        ObjectType,
        [ElementTypeInfo("function")]
        Function,
        [ElementTypeInfo("editor")]
        Editor,
        [ElementTypeInfo("tab")]
        EditorTab,
        [ElementTypeInfo("control")]
        EditorControl,
        [ElementTypeInfo("walkthrough")]
        Walkthrough,
        [ElementTypeInfo("javascript")]
        Javascript,
        [ElementTypeInfo("timer")]
        Timer,
        [ElementTypeInfo("resource")]
        Resource,
    }

    public enum ObjectType
    {
        Object,
        Exit,
        Command,
        Game,
        TurnScript
    }

    public class Element
    {
        private static Dictionary<ObjectType, string> s_typeStrings;
        private static Dictionary<string, ObjectType> s_mapObjectTypeStringsToElementType;
        private static Dictionary<ElementType, string> s_elemTypeStrings;
        private static Dictionary<string, ElementType> s_mapElemTypeStringsToElementType;

        static Element()
        {
            s_typeStrings = new Dictionary<ObjectType, string>();
            s_typeStrings.Add(ObjectType.Object, "object");
            s_typeStrings.Add(ObjectType.Exit, "exit");
            s_typeStrings.Add(ObjectType.Command, "command");
            s_typeStrings.Add(ObjectType.Game, "game");
            s_typeStrings.Add(ObjectType.TurnScript, "turnscript");

            s_mapObjectTypeStringsToElementType = new Dictionary<string, ObjectType>();
            foreach (var item in s_typeStrings)
            {
                s_mapObjectTypeStringsToElementType.Add(item.Value, item.Key);
            }

            s_elemTypeStrings = new Dictionary<ElementType, string>();
            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                s_elemTypeStrings.Add(t, ((ElementTypeInfo)(typeof(ElementType).GetField(t.ToString()).GetCustomAttributes(typeof(ElementTypeInfo), false)[0])).Name);
            }

            s_mapElemTypeStringsToElementType = new Dictionary<string, ElementType>();
            foreach (var item in s_elemTypeStrings)
            {
                s_mapElemTypeStringsToElementType.Add(item.Value, item.Key);
            }
        }

        private ObjectType m_type;
        private ElementType m_elemType;
        private Fields m_fields = new Fields();
        private Fields m_metaFields = new Fields();
        private GameLoader m_loader;

        public Element(ElementType type, GameLoader loader)
        {
            ElemType = type;
            m_loader = loader;
        }

        public Element Parent
        {
            get { return Fields.GetAsType<Element>("parent"); }
            set { Fields.Set("parent", value); }
        }

        public string Name
        {
            get { return Fields.GetAsType<string>("name"); }
            set { Fields.Set("name", value); }
        }

        public ObjectType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                Fields.Set("type", TypeString);
            }
        }

        public ElementType ElemType
        {
            get
            {
                return m_elemType;
            }
            set
            {
                m_elemType = value;
                Fields.Set("elementtype", ElementTypeString);
            }
        }

        public Fields Fields { get { return m_fields; } }
        public Fields MetaFields { get { return m_metaFields; } }

        private string TypeString
        {
            get
            {
                return s_typeStrings[m_type];
            }
        }

        private string ElementTypeString
        {
            get
            {
                return s_elemTypeStrings[m_elemType];
            }
        }

        public GameLoader Loader { get { return m_loader; } }
    }
}
