using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    public interface IField<T>
    {
        string Property { get; }
    }

    internal class FieldDef<T> : IField<T>
    {
        private string m_property;

        public FieldDef(string property)
        {
            m_property = property;
        }

        public string Property { get { return m_property; } }
    }

    public static class FieldDefinitions
    {
        public static IField<string> Alias = new FieldDef<string>("alias");
        public static IField<QuestList<string>> DisplayVerbs = new FieldDef<QuestList<string>>("displayverbs");
        public static IField<QuestList<string>> InventoryVerbs = new FieldDef<QuestList<string>>("inventoryverbs");
        public static IField<Element> To = new FieldDef<Element>("to");
        public static IField<bool> LookOnly = new FieldDef<bool>("lookonly");
        public static IField<QuestList<string>> ParamNames = new FieldDef<QuestList<string>>("paramnames");
        public static IField<IScript> Script = new FieldDef<IScript>("script");
        public static IField<string> ReturnType = new FieldDef<string>("returntype");
        public static IField<string> Pattern = new FieldDef<string>("pattern");
        public static IField<string> Unresolved = new FieldDef<string>("unresolved");
        public static IField<string> Property = new FieldDef<string>("property");
        public static IField<string> DefaultTemplate = new FieldDef<string>("defaulttemplate");
        public static IField<bool> IsVerb = new FieldDef<bool>("isverb");
        public static IField<string> DefaultText = new FieldDef<string>("defaulttext");
        public static IField<string> GameName = new FieldDef<string>("gamename");
        public static IField<string> Text = new FieldDef<string>("text");
        public static IField<IFunction> Function = new FieldDef<IFunction>("text");
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<QuestList<string>> Steps = new FieldDef<QuestList<string>>("steps");
        public static IField<string> Element = new FieldDef<string>("element");
        public static IField<string> Type = new FieldDef<string>("type");
        public static IField<string> Src = new FieldDef<string>("src");
        public static IField<bool> Anonymous = new FieldDef<bool>("anonymous");
        public static IField<string> TemplateName = new FieldDef<string>("templatename");
        public static IField<string> OriginalPattern = new FieldDef<string>("originalpattern");
        public static IField<int> TimeElapsed = new FieldDef<int>("timeelapsed");
        public static IField<int> Trigger = new FieldDef<int>("trigger");
        public static IField<int> Interval = new FieldDef<int>("interval");
        public static IField<bool> Enabled = new FieldDef<bool>("enabled");
        public static IField<string> Background = new FieldDef<string>("defaultbackground");
        public static IField<string> Author = new FieldDef<string>("author");
        public static IField<string> Version = new FieldDef<string>("version");
        public static IField<string> VersionCode = new FieldDef<string>("versioncode");
        public static IField<string> ProductId = new FieldDef<string>("productid");
        public static IField<string> AppTabGame = new FieldDef<string>("app_tab_game");
        public static IField<string> AppTabInventory = new FieldDef<string>("app_tab_inventory");
        public static IField<string> AppTabObjects = new FieldDef<string>("app_tab_objects");
        public static IField<string> AppTabExits = new FieldDef<string>("app_tab_exits");
        public static IField<string> AppTabMore = new FieldDef<string>("app_tab_more");
        public static IField<string> AppButtonOptions = new FieldDef<string>("app_button_options");
        public static IField<string> AppButtonRestart = new FieldDef<string>("app_button_restart");
        public static IField<string> AppButtonUndo = new FieldDef<string>("app_button_undo");
        public static IField<string> AppButtonWait = new FieldDef<string>("app_button_wait");
        public static IField<string> AppTextCancel = new FieldDef<string>("app_text_cancel");
        public static IField<string> AppTextStatus = new FieldDef<string>("app_text_status");
        public static IField<string> AppTextEmpty = new FieldDef<string>("app_text_empty");
        public static IField<string> AppTextNone = new FieldDef<string>("app_text_none");
        public static IField<string> AppTextInputPlaceholder = new FieldDef<string>("app_text_inputplaceholder");
    }

    public static class MetaFieldDefinitions
    {
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<bool> Library = new FieldDef<bool>("library");
        public static IField<bool> EditorLibrary = new FieldDef<bool>("editorlibrary");
        public static IField<bool> DelegateImplementation = new FieldDef<bool>("delegateimplementation");
        public static IField<int> SortIndex = new FieldDef<int>("sortindex");
        public static IField<string> MappedName = new FieldDef<string>("mappedname");
    }

    public class Fields
    {
        #region Indexed Properties
        public string this[IField<string> field]
        {
            get
            {
                return GetAsType<string>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public QuestList<string> this[IField<QuestList<string>> field]
        {
            get
            {
                return GetAsType<QuestList<string>>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public IScript this[IField<IScript> field]
        {
            get
            {
                return GetAsType<IScript>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public Element this[IField<Element> field]
        {
            get
            {
                return GetAsType<Element>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public bool this[IField<bool> field]
        {
            get
            {
                return GetAsType<bool>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public int this[IField<int> field]
        {
            get
            {
                return GetAsType<int>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public IFunction this[IField<IFunction> field]
        {
            get
            {
                return GetAsType<IFunction>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }
        #endregion

        private Dictionary<string, object> m_attributes = new Dictionary<string, object>();
        private List<string> m_typeNames = new List<string>();
        private Stack<Element> m_types = new Stack<Element>();
        private Dictionary<string, string> m_objectReferences = new Dictionary<string, string>();
        private Dictionary<string, List<string>> m_objectLists = new Dictionary<string, List<string>>();
        private Dictionary<string, IDictionary<string, string>> m_objectDictionaries = new Dictionary<string, IDictionary<string, string>>();

        public T GetAsType<T>(string attribute)
        {
            object value = Get(attribute);
            if (value is T) return (T)value;
            return default(T);
        }

        public void Set(string attribute, object value)
        {
            m_attributes[attribute] = value;
        }

        public object Get(string attribute)
        {
            if (m_attributes.ContainsKey(attribute))
            {
                return m_attributes[attribute];
            }

            object result = null;

            foreach (Element type in m_types)
            {
                if (type.Fields.Exists(attribute))
                {
                    result = type.Fields.Get(attribute);
                    break;
                }
            }

            return result;
        }

        private bool Exists(string attribute)
        {
            if (m_attributes.ContainsKey(attribute)) return true;

            foreach (Element type in m_types)
            {
                if (type.Fields.Exists(attribute)) return true;
            }
            return false;
        }

        public IEnumerable<string> FieldNames
        {
            get
            {
                IEnumerable<string> result = m_attributes.Keys;
                foreach (Element type in m_types)
                {
                    result = result.Union(type.Fields.FieldNames);
                }
                return result.Distinct();
            }
        }

        public IEnumerable<string> TypeNames
        {
            get { return m_types.Select(t => t.Name); }
        }

        public void AddTypeName(string name)
        {
            m_typeNames.Add(name);
        }

        public void AddObjectRef(string attribute, string name)
        {
            m_objectReferences.Add(attribute, name);
        }

        public void AddObjectList(string attribute, List<string> value)
        {
            m_objectLists.Add(attribute, value);
        }

        public void AddObjectDictionary(string attribute, IDictionary<string, string> value)
        {
            m_objectDictionaries.Add(attribute, value);
        }

        public void Resolve(GameLoader loader)
        {
            foreach (string typeName in m_typeNames)
            {
                if (loader.Elements.ContainsKey(typeName))
                {
                    m_types.Push(loader.Elements[typeName]);
                }
            }

            foreach (var objectRef in m_objectReferences)
            {
                Set(objectRef.Key, loader.Elements[objectRef.Value]);
            }

            foreach (var objectList in m_objectLists)
            {
                QuestList<Element> newList = new QuestList<Element>(objectList.Value.Select(l => loader.Elements[l]));
                Set(objectList.Key, newList);
            }

            foreach (var objectDict in m_objectDictionaries)
            {
                QuestDictionary<Element> newDict = new QuestDictionary<Element>();
                foreach (var item in objectDict.Value)
                {
                    newDict.Add(item.Key, loader.Elements[item.Value]);
                }
                Set(objectDict.Key, newDict);
            }
        }
    }
}
