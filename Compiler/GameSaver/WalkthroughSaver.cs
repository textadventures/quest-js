using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class WalkthroughSaver : ElementSaverBase, IElementSaver
    {
        public ElementType AppliesTo
        {
            get { return ElementType.Walkthrough; }
        }

        public void Save(Element e, GameWriter writer)
        {
            base.SaveElementFields(e.Name, e, writer);
        }

        protected override object ConvertField(Element e, string fieldName, object value)
        {
            if (fieldName == "steps")
            {
                QuestList<string> steps = (QuestList<string>)value;
                QuestList<string> result = new QuestList<string>();

                foreach (string step in steps)
                {
                    if (step.StartsWith("assert:"))
                    {
                        string expr = step.Substring(7);
                        Expression expression = new Expression(expr, e.Loader);
                        result.Add("assert:" + expression.Save(new Context()));
                    }
                    else
                    {
                        result.Add(step);
                    }
                }
                return result;
            }
            return value;
        }
    }
}
