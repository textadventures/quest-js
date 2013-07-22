using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class QuestList<T> : IList<T>
    {
        private List<T> m_list;

        public QuestList()
            : this(null)
        {
        }

        public QuestList(IEnumerable<T> values)
        {
            if (values == null)
            {
                m_list = new List<T>();
            }
            else
            {
                m_list = new List<T>(values);
            }
        }

        public int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            m_list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                m_list[index] = value;
            }
        }

        public void Add(T item)
        {
            m_list.Add(item);
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_list.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<T>)m_list).IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return m_list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
    }
}
