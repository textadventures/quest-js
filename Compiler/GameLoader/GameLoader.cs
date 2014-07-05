using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;

namespace TextAdventures.Quest
{
    public enum WorldModelVersion
    {
        v500,
        v510,
        v520,
        v530,
        v540,
        v550,
    }

    public partial class GameLoader
    {
        public enum LoadMode
        {
            Play,
            Edit
        }

        private struct FileData
        {
            public string Filename;
            public bool IsEditorLibrary;
        }

        private List<string> m_errors = new List<string>();
        private ElementFactory m_elementFactory;
        private ScriptFactory m_scriptFactory;
        private ImplicitTypes m_implicitTypes = new ImplicitTypes();
        private Stack<FileData> m_currentFile = new Stack<FileData>();
        private Dictionary<string, Element> m_elements = new Dictionary<string, Element>();
        private List<string> m_warnings = new List<string>();
        private WorldModelVersion m_version;
        private ElementNameMapper m_elementNameMapper = new ElementNameMapper();

        public delegate void FilenameUpdatedHandler(string filename);
        public event FilenameUpdatedHandler FilenameUpdated;

        public WorldModelVersion Version { get { return m_version; } }
        public static IEnumerable<WorldModelVersion> PossibleVersions
        {
            get
            {
                return s_versions.Values;
            }
        }

        private static Dictionary<string, WorldModelVersion> s_versions = new Dictionary<string, WorldModelVersion> {
            {"500", WorldModelVersion.v500},
            {"510", WorldModelVersion.v510},
            {"520", WorldModelVersion.v520},
            {"530", WorldModelVersion.v530},
            {"540", WorldModelVersion.v540},
            {"550", WorldModelVersion.v550},
        };

        public GameLoader()
        {
            m_elementFactory = new ElementFactory(this);
            m_scriptFactory = new ScriptFactory(this);
            m_scriptFactory.ErrorHandler += AddError;
            AddLoaders();
            AddExtendedAttributeLoaders();
            AddXMLLoaders();
        }

        public void AddElement(Element element)
        {
            m_elements.Add(element.Name, element);
        }

        protected ElementFactory ElementFactory
        {
            get { return m_elementFactory; }
        }

        protected ScriptFactory ScriptFactory
        {
            get { return m_scriptFactory; }
        }

        public bool Load(string filename)
        {
            IsCompiledFile = false;

            if (System.IO.Path.GetExtension(filename) == ".quest")
            {
                filename = LoadCompiledFile(filename);
            }

            XmlReader reader = null;

            try
            {
                reader = XmlReader.Create(filename);

                do
                {
                    reader.Read();
                } while (reader.NodeType != XmlNodeType.Element);

                if (reader.Name == "asl")
                {
                    string version = reader.GetAttribute("version");

                    if (string.IsNullOrEmpty(version))
                    {
                        AddError("No ASL version number found");
                    }

                    if (!s_versions.ContainsKey(version))
                    {
                        AddError("Unknown ASL version");
                    }
                    else
                    {
                        m_version = s_versions[version];
                        if (m_version < WorldModelVersion.v510)
                        {
                            AddError("Unsupported ASL version - must be v5.1 or later");
                        }
                    }

                    string originalFile = reader.GetAttribute("original");

                    if (!string.IsNullOrEmpty(originalFile) && System.IO.Path.GetExtension(originalFile) == ".quest")
                    {
                        LoadCompiledFile(originalFile);
                    }

                    if (!string.IsNullOrEmpty(originalFile))
                    {
                        FilenameUpdated(originalFile);
                    }
                }
                else
                {
                    AddError("File must begin with an ASL element");
                }

                LoadXML(reader);
            }
            catch (XmlException e)
            {
                AddError(string.Format("Invalid XML: {0}", e.Message));
            }
            catch (Exception e)
            {
                AddError(string.Format("Error: {0}", e.Message));
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            if (m_errors.Count == 0)
            {
                ResolveGame();
                ValidateGame();
            }

            return (m_errors.Count == 0);
        }

        private string LoadCompiledFile(string filename)
        {
            PackageReader packageReader = new PackageReader();
            var result = packageReader.LoadPackage(filename);
            ResourcesFolder = result.Folder;
            IsCompiledFile = true;
            return result.GameFile;
        }

        private void LoadXML(XmlReader reader)
        {
            Element current = null;
            IXMLLoader currentLoader = null;

            // Set the "IsEditorLibrary" flag for any library with type="editor", and its sub-libraries
            bool isEditorLibrary = false;
            if (m_currentFile.Count > 0 && m_currentFile.Peek().IsEditorLibrary) isEditorLibrary = true;
            if (reader.GetAttribute("type") == "editor") isEditorLibrary = true;

            FileData data = new FileData
            {
                Filename = reader.BaseURI,
                IsEditorLibrary = isEditorLibrary
            };

            m_currentFile.Push(data);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        currentLoader = GetLoader(reader.Name, current);
                        currentLoader.StartElement(reader, ref current);
                        break;
                    case XmlNodeType.EndElement:
                        GetLoader(reader.Name, current).EndElement(reader, ref current);
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        currentLoader.SetText(reader.ReadContentAsString(), ref current);
                        // if we've eaten the content of this element, then the reader will have gone
                        // past the EndElement already, so we need to trigger the EndElement here
                        GetLoader(reader.Name, current).EndElement(reader, ref current);
                        currentLoader = null;
                        break;
                }
            }

            m_currentFile.Pop();
        }

        private void AddedElement(Element newElement)
        {
            newElement.MetaFields[MetaFieldDefinitions.Filename] = m_currentFile.Peek().Filename;
            newElement.MetaFields[MetaFieldDefinitions.Library] = (m_currentFile.Count > 1);
            newElement.MetaFields[MetaFieldDefinitions.EditorLibrary] = (m_currentFile.Peek().IsEditorLibrary);
        }

        private IXMLLoader GetLoader(string name, Element current)
        {
            IXMLLoader loader;
            if (!m_xmlLoaders.TryGetValue(name, out loader) && current != null) loader = m_defaultXmlLoader;
            if (loader == null) throw new Exception(string.Format("Unrecognised tag '{0}' outside object definition", name));
            return loader;
        }

        private Regex m_templateRegex = new Regex(@"\[(?<name>.*?)\]");

        private string GetTemplateAttribute(XmlReader reader, string attribute)
        {
            return reader.GetAttribute(attribute);
        }

        private string GetTemplateContents(XmlReader reader)
        {
            return reader.ReadElementContentAsString();
        }

        private void ResolveGame()
        {
            foreach (Element e in m_elements.Values)
            {
                e.Fields.Resolve(this);
            }
        }

        private void ValidateGame()
        {
            foreach (Element element in m_elements.Values.Where(e => e.ElemType == ElementType.Function))
            {
                foreach (string paramName in element.Fields[FieldDefinitions.ParamNames])
                {
                    if (m_elements.ContainsKey(paramName))
                    {
                        AddWarning(string.Format("Variable '{0}' clashes with object name", paramName));
                    }
                }
            }
        }

        private void AddError(string error)
        {
            m_errors.Add(error);
        }

        public List<string> Errors
        {
            get { return m_errors; }
        }

        public List<string> Warnings
        {
            get { return m_warnings; }
        }

        // virtual so this class can be mocked
        public virtual Dictionary<string, Element> Elements
        {
            get { return m_elements; }
        }

        public virtual IEnumerable<Element> NonFunctionElements
        {
            get { return m_elements.Where(e => e.Value.ElemType != ElementType.Function).Select(e => e.Value); }
        }

        private List<Tuple<Regex, string>> m_elementNamesRegexes = null;

        public List<Tuple<Regex, string>> ElementNamesRegexes
        {
            get
            {
                if (m_elementNamesRegexes == null)
                {
                    m_elementNamesRegexes = new List<Tuple<Regex, string>>();
                    // Order by descending name length, so the longest object names are replaced first.
                    // This works around the case where we have an object called e.g. "object" and "object two",
                    // we don't want "object" to be a match for "object two".
                    foreach (Element e in NonFunctionElements.OrderByDescending(e => e.Name.Length))
                    {
                        m_elementNamesRegexes.Add(new Tuple<Regex, string>(
                            new Regex(@"\b(" + e.Name + @")\b"),
                            NameMapper.GetMappedName(e.Name)));
                    }

                }
                return m_elementNamesRegexes;
            }
        }

        private static Dictionary<string, IField<string>> s_substitutionFieldNames = new Dictionary<string, IField<string>>
        {
            { "GAMENAME", FieldDefinitions.GameName },
            { "BACKGROUND", FieldDefinitions.Background },
            { "AUTHOR", FieldDefinitions.Author },
            { "VERSION", FieldDefinitions.Version },
            { "VERSIONCODE", FieldDefinitions.VersionCode },
            { "PRODUCTID", FieldDefinitions.ProductId },
            { "TAB_GAME", FieldDefinitions.AppTabGame },
            { "TAB_INVENTORY", FieldDefinitions.AppTabInventory},
            { "TAB_OBJECTS", FieldDefinitions.AppTabObjects },
            { "TAB_EXITS", FieldDefinitions.AppTabExits },
            { "TAB_MORE", FieldDefinitions.AppTabMore },
            { "BUTTON_OPTIONS", FieldDefinitions.AppButtonOptions },
            { "BUTTON_RESTART", FieldDefinitions.AppButtonRestart },
            { "BUTTON_UNDO", FieldDefinitions.AppButtonUndo },
            { "BUTTON_WAIT", FieldDefinitions.AppButtonWait },
            { "TEXT_CANCEL", FieldDefinitions.AppTextCancel },
            { "TEXT_STATUS", FieldDefinitions.AppTextStatus },
            { "TEXT_EMPTY", FieldDefinitions.AppTextEmpty },
            { "TEXT_NONE", FieldDefinitions.AppTextNone },
            { "TEXT_INPUTPLACEHOLDER", FieldDefinitions.AppTextInputPlaceholder },
        };

        private static Dictionary<string, string> s_substitutionFieldDefaults = new Dictionary<string, string>
        {
            { "AUTHOR", "Anonymous" },
            { "PRODUCTID", "" },
            { "VERSIONCODE", "1" },
            { "TAB_GAME", "Game" },
            { "TAB_INVENTORY", "Inventory" },
            { "TAB_OBJECTS", "Objects" },
            { "TAB_EXITS", "Exits" },
            { "TAB_MORE", "More" },
            { "BUTTON_OPTIONS", "Options" },
            { "BUTTON_RESTART", "Restart" },
            { "BUTTON_UNDO", "Undo" },
            { "BUTTON_WAIT", "Wait" },
            { "TEXT_CANCEL", "Cancel" },
            { "TEXT_STATUS", "Status" },
            { "TEXT_EMPTY", "Empty" },
            { "TEXT_NONE", "None" },
            { "TEXT_INPUTPLACEHOLDER", "Tap links or type here..." },
        };

        public string GetSubstitutionText(string name)
        {
            string result = m_elements["game"].Fields[s_substitutionFieldNames[name]];
            if (string.IsNullOrEmpty(result))
            {
                result = s_substitutionFieldDefaults[name];
            }
            return result;
        }

        public IEnumerable<string> GetSubstitutionFieldNames()
        {
            return s_substitutionFieldNames.Keys;
        }

        public void AddWarning(string text)
        {
            m_warnings.Add(text);
        }

        private class RequiredAttribute
        {
            public string Name { get; set; }
            public bool UseTemplate { get; set; }
            public bool AllowBlank { get; set; }
            public bool Required { get; set; }

            public RequiredAttribute(string name, bool useTemplate, bool allowBlank, bool required)
                : this(name, useTemplate, allowBlank)
            {
                Required = required;
            }

            public RequiredAttribute(string name, bool useTemplate, bool allowBlank)
                : this(name, useTemplate)
            {
                AllowBlank = allowBlank;
            }

            public RequiredAttribute(string name, bool useTemplate)
                : this(name)
            {
                UseTemplate = useTemplate;
            }

            public RequiredAttribute(bool required, string name)
                : this(name)
            {
                Required = required;
            }

            public RequiredAttribute(string name)
            {
                Name = name;
                Required = true;
            }
        }

        private class RequiredAttributes
        {
            public List<RequiredAttribute> Attributes { get; set; }
            public RequiredAttributes(params RequiredAttribute[] attribs)
            {
                Attributes = new List<RequiredAttribute>(attribs);
            }
            public RequiredAttributes(bool canUseTemplates, params string[] attribs)
            {
                Attributes = new List<RequiredAttribute>();
                foreach (string attrib in attribs)
                {
                    Attributes.Add(new RequiredAttribute(attrib, canUseTemplates, false));
                }
            }
        }

        private Dictionary<string, string> GetRequiredAttributes(XmlReader reader, RequiredAttributes attribs)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (RequiredAttribute attrib in attribs.Attributes)
            {
                string value;
                if (attrib.UseTemplate)
                {
                    value = GetTemplateAttribute(reader, attrib.Name);
                }
                else
                {
                    value = reader.GetAttribute(attrib.Name);
                }

                if (attrib.Required && (value == null || (!attrib.AllowBlank && value.Length == 0)))
                {
                    throw new Exception(string.Format("Missing required attribute '{0}' in '{1}' tag", attrib.Name, reader.LocalName));
                }

                result.Add(attrib.Name, value);
            }

            return result;
        }

        private class ImplicitTypes
        {
            private Dictionary<string, string> m_implicitTypes = new Dictionary<string, string>();

            public void Add(string element, string property, string type)
            {
                m_implicitTypes.Add(GetKey(element, property), type);
            }

            public string Get(string element, string property)
            {
                string result;
                if (m_implicitTypes.TryGetValue(GetKey(element, property), out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }

            private string GetKey(string element, string property)
            {
                return element + "~" + property;
            }
        }

        public string ResourcesFolder { get; set; }
        public bool IsCompiledFile { get; private set; }

        public ElementNameMapper NameMapper { get { return m_elementNameMapper; } }

        ~GameLoader()
        {
            try
            {
                System.IO.Directory.Delete(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Quest"), true);
            }
            catch { }
        }
    }
}
