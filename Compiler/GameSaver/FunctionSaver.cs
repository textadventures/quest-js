using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class FunctionSaver : IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Function; }
        }

        public void Save(Element e, GameWriter writer)
        {
            string paramNames = string.Join(", ", e.Fields[FieldDefinitions.ParamNames]);
            paramNames = Utility.ReplaceReservedVariableNames(paramNames);
            writer.AddLine("function " + e.Name.Replace(" ", Utility.SpaceReplacementString) + "(" + paramNames + ")");
            writer.AddLine("{");
            var context = new Context();
            context.AddLocalVariable(e.Fields[FieldDefinitions.ParamNames].ToArray());
            writer.AddLine(e.Fields[FieldDefinitions.Script].Save(context));
            writer.AddLine("}");
        }
    }
}
