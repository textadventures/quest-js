using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class IfScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "if"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string expr = Utility.GetParameter(script, out afterExpr);
            string then = Utility.GetScript(afterExpr);

            IScript thenScript = ScriptFactory.CreateScript(then, proc);

            return new IfScript(new Expression(expr, GameLoader), thenScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }

        public void AddElse(IScript script, string elseScript, Element proc)
        {
            IScript add = GetElse(elseScript, proc);
            ((IfScript)script).SetElse(add);
        }

        public void AddElseIf(IScript script, string elseIfScript, Element proc)
        {
            IScript add = GetElse(elseIfScript, proc);

            // GetElse uses the ScriptFactory to parse the "else if" block, so it will return
            // a MultiScript containing an IfScript with one expression and one "then" script block.

            IfScript elseIf = (IfScript)((MultiScript)add).Scripts.First();

            ((IfScript)script).AddElseIf(elseIf.Expression, elseIf.ThenScript);
        }

        private IScript GetElse(string elseScript, Element proc)
        {
            elseScript = Utility.GetTextAfter(elseScript, "else");
            return ScriptFactory.CreateScript(elseScript, proc);
        }
    }

    public class IfScript : ScriptBase
    {
        public class ElseIfScript
        {
            private IfScript m_parent;

            public ElseIfScript(IFunction expression, IScript script, IfScript parent, string id)
            {
                Expression = expression;
                Script = script;
                m_parent = parent;
                Id = id;
            }

            internal IFunction Expression { get; private set; }
            public IScript Script { get; private set; }
            public string Id { get; private set; }
        }

        private IFunction m_expression;
        private IScript m_thenScript;
        private IScript m_elseScript;
        private List<ElseIfScript> m_elseIfScript = new List<ElseIfScript>();

        public IfScript(IFunction expression, IScript thenScript)
            : this(expression, thenScript, null)
        {
        }

        public IfScript(IFunction expression, IScript thenScript, IScript elseScript)
        {
            m_expression = expression;
            m_thenScript = thenScript;
            m_elseScript = elseScript;
        }

        public void SetElse(IScript elseScript)
        {
            m_elseScript = elseScript;
        }

        private int m_lastElseIfId = 0;

        private string GetNewElseIfID()
        {
            m_lastElseIfId++;
            return "elseif" + m_lastElseIfId;
        }

        public void AddElseIf(IFunction expression, IScript script)
        {
            ElseIfScript elseIfScript = new ElseIfScript(expression, script, this, GetNewElseIfID());
            m_elseIfScript.Add(elseIfScript);
        }

        public override string Save(Context c)
        {
            string result = SaveExpressionScript("if", m_thenScript, c, m_expression.Save(c));
            if (m_elseIfScript != null)
            {
                foreach (ElseIfScript elseIf in m_elseIfScript)
                {
                    result += Environment.NewLine + SaveExpressionScript("else if", elseIf.Script, c, elseIf.Expression.Save(c));
                }
            }
            if (m_elseScript != null) result += Environment.NewLine + "else {" + Environment.NewLine + m_elseScript.Save(c) + Environment.NewLine + "}";
            return result;
        }

        internal IFunction Expression
        {
            get { return m_expression; }
        }

        public IScript ThenScript
        {
            get { return m_thenScript; }
            set { m_thenScript = value; }
        }
    }
}
