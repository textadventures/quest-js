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
            if (e.Name.Replace(" ", Utility.SpaceReplacementString) == "PrintCentered")
            {
                writer.AddLine("function PrintCentered(text)");
                writer.AddLine("{");
                writer.AddLine("msg('<center>'+text+'</center>');");
                writer.AddLine("}");
            }
            else if (e.Name.Replace(" ", Utility.SpaceReplacementString) == "RequestSave" || e.Name.Replace(" ", Utility.SpaceReplacementString) == "requestsave" || e.Name.Replace(" ", Utility.SpaceReplacementString) == "requestspeak")
            {
                // Do nothing because the new setup breaks the games!  The old functions shall be in game.js!
            }
            else if (e.Name.Replace(" ", Utility.SpaceReplacementString) != "SetTimeout")
            {
                writer.AddLine("function " + e.Name.Replace(" ", Utility.SpaceReplacementString) + "(" + paramNames + ")");
                writer.AddLine("{");
                var context = new Context();
                context.AddLocalVariable(e.Fields[FieldDefinitions.ParamNames].ToArray());
                var s = e.Fields[FieldDefinitions.Script].Save(context);
                if (e.Name.Replace(" ", Utility.SpaceReplacementString) == "ResolveNameFromList")
                {
                    s = s.Replace("fullmatches + partialmatches", "ListCombine(fullmatches, partialmatches)");
                }
                writer.AddLine(s);
                writer.AddLine("}");
            }
        }
    }
}
