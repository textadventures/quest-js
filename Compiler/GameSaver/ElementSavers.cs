using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    internal abstract class ElementSaverBase
    {
        FieldSaver fieldSaver = new FieldSaver();

        protected void SaveElementFields(string name, Element e, GameWriter writer)
        {
            string mappedName = e.MetaFields[MetaFieldDefinitions.MappedName];

            writer.AddLine(mappedName + " = {");
            e.Fields.Set("_js_name", mappedName);
            e.Fields.Set("_types", new QuestList<string>(e.Fields.TypeNames));

            int count = 0;
            int length = e.Fields.FieldNames.Count();

            foreach (string field in e.Fields.FieldNames)
            {
                count++;
                object value = ConvertField(e, field, e.Fields.Get(field));
                fieldSaver.Save(writer, e, field, value, count == length);
            }

            writer.AddLine("};");
            writer.AddLine(string.Format("elementsNameMap[\"{0}\"] = {1};", e.Name, e.MetaFields[MetaFieldDefinitions.MappedName]));
            writer.MarkElementWritten(e);
        }

        protected virtual object ConvertField(Element e, string fieldName, object value)
        {
            return value;
        }
    }

    internal abstract class ObjectSaverBase : ElementSaverBase
    {
        private Dictionary<ObjectType, string> allObjectsArray = new Dictionary<ObjectType, string> {
            { ObjectType.Object, "allObjects" },
            { ObjectType.Command, "allCommands" },
            { ObjectType.Exit, "allExits" },
            { ObjectType.TurnScript, "allTurnScripts" }
        };

        public void Save(Element e, GameWriter writer)
        {
            base.SaveElementFields(e.Name, e, writer);
            string postElementScript = writer.GetPostElementScript(e);
            if (postElementScript.Length > 0) writer.AddLine(postElementScript);
            if (allObjectsArray.ContainsKey(e.Type))
            {
                writer.AddLine(string.Format("{0}.push({1});", allObjectsArray[e.Type], e.MetaFields[MetaFieldDefinitions.MappedName]));
            }
            writer.AddLine(string.Format("objectsNameMap[\"{0}\"] = {1};", e.Name, e.MetaFields[MetaFieldDefinitions.MappedName]));
        }
    }

    internal class ObjectSaver : ObjectSaverBase, IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Object; }
        }
    }

    internal class TemplateSaver : IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Template; }
        }

        public void Save(Element e, GameWriter writer)
        {
            if (e.Fields[FieldDefinitions.TemplateName] == "EditorVerbDefaultExpression") return;
            writer.AddLine(string.Format("templates.t_{0} = \"{1}\"", e.Fields[FieldDefinitions.TemplateName], e.Fields[FieldDefinitions.Text].Replace("\n", "").Replace("\r", "")));
        }
    }

    internal class DynamicTemplateSaver : IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.DynamicTemplate; }
        }

        public void Save(Element e, GameWriter writer)
        {
            string expression = e.Fields[FieldDefinitions.Function].Save(new Context());
            expression = Utility.ReplaceDynamicTemplateVariableNames(expression);
            writer.AddLine(string.Format("dynamicTemplates.{0} = function(params) {{ return {1}; }};", e.Name, expression));
        }
    }

    internal class DelegateSaver : IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Delegate; }
        }

        public void Save(Element e, GameWriter writer)
        {
            // Delegate definitions don't need saving in Javascript
        }
    }

    internal class ObjectTypeSaver : ObjectSaverBase, IElementSaver
    {
        // TO DO: This will simply save the type - we need more logic to actually include the type in objects that inherit it

        public ElementType AppliesTo
        {
            get { return ElementType.ObjectType; }
        }
    }

    internal class JavacriptSaver : IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Javascript; }
        }

        public void Save(Element e, GameWriter writer)
        {
            // Do nothing, .js files will be picked up from the resources folder and embedded in game.js automatically
        }
    }

    internal class TimerSaver : ElementSaverBase, IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Timer; }
        }

        public void Save(Element e, GameWriter writer)
        {
            base.SaveElementFields(e.Name, e, writer);
            writer.AddLine(string.Format("allTimers.push({0});", e.MetaFields[MetaFieldDefinitions.MappedName]));
            writer.AddLine(string.Format("objectsNameMap[\"{0}\"] = {1};", e.Name, e.MetaFields[MetaFieldDefinitions.MappedName]));
        }
    }
}
