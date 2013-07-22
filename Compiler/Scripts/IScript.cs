using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public interface IScript
    {
        string Line { get; set; }
        string Save();
        string Keyword { get; }
    }

    public interface IScriptConstructor
    {
        string Keyword { get; }
        IScript Create(string script, Element proc);
        IScriptFactory ScriptFactory { set; }
        GameLoader GameLoader { set; }
    }

    public abstract class ScriptBase : IScript
    {
        private string m_line;

        public override string ToString()
        {
            return "Script: " + Line;
        }

        protected string SaveScript(string keyword, params string[] args)
        {
            return keyword + " (" + String.Join(", ", args) + ");";
        }

        protected string SaveExpressionScript(string keyword, IScript script, params string[] args)
        {
            string result;
            if (args.Length == 0)
            {
                result = keyword;
            }
            else
            {
                result = keyword + " (" + String.Join(", ", args) + ")";
            }
            string scriptString = script != null ? script.Save() : string.Empty;
            return result + " {" + Environment.NewLine + scriptString + Environment.NewLine + "}";
        }

        public Element Owner { get; set; }

        public abstract string Save();

        public virtual string Line
        {
            get { return m_line; }
            set { m_line = value; }
        }

        public virtual string Keyword
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
