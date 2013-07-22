using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    interface IElementSaver
    {
        ElementType AppliesTo { get; }
        void Save(Element e, GameWriter writer);
    }

    public class GameSaver
    {
        private Dictionary<string, Element> m_elements;
        private Dictionary<ElementType, IElementSaver> m_elementSavers = new Dictionary<ElementType, IElementSaver>();

        public class ProgressEventArgs : EventArgs
        {
            public int PercentComplete { get; set; }
        }

        public event EventHandler<ProgressEventArgs> Progress;

        public GameSaver(Dictionary<string, Element> elements)
        {
            m_elements = elements;

            // Use Reflection to create instances of all IElementSavers (save individual elements)
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IElementSaver)))
            {
                AddElementSaver((IElementSaver)Activator.CreateInstance(t));
            }
        }

        private void AddElementSaver(IElementSaver saver)
        {
            m_elementSavers.Add(saver.AppliesTo, saver);
        }

        public string Save()
        {
            int total = m_elements.Count;
            int done = 0;

            GameWriter writer = new GameWriter();
            foreach (Element e in m_elements.Values)
            {
                if (Progress != null)
                {
                    Progress(this, new ProgressEventArgs { PercentComplete = (int)(((double)done / total) * 100) });
                }

                IElementSaver saver;
                if (m_elementSavers.TryGetValue(e.ElemType, out saver))
                {
                    saver.Save(e, writer);
                }
                else
                {
                    throw new Exception("ERROR: No ElementSaver for type " + e.ElemType.ToString());
                }
                done++;
            }
            return writer.Save();
        }
    }
}
