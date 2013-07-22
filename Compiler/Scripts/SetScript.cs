using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class SetScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "="; }
        }

        public IScript Create(string script, Element proc)
        {
            bool isScript = false;
            int offset = 0;
            int eqPos;

            // hide text within string expressions
            string obscuredScript = Utility.ObscureStrings(script);
            int bracePos = obscuredScript.IndexOf('{');
            if (bracePos != -1)
            {
                // only want to look for = and => before any other scripts which may
                // be defined on the same line, for example procedure calls of type
                //     MyProcedureCall (5) { some other script }

                obscuredScript = obscuredScript.Substring(0, bracePos);
            }

            eqPos = obscuredScript.IndexOf("=>");
            if (eqPos != -1)
            {
                isScript = true;
                offset = 1;
            }
            else
            {
                eqPos = obscuredScript.IndexOf('=');
            }

            if (eqPos != -1)
            {
                string appliesTo = script.Substring(0, eqPos);
                string value = script.Substring(eqPos + 1 + offset).Trim();

                string variable;
                IFunction expr = GetAppliesTo(appliesTo, out variable);

                if (!isScript)
                {
                    return new SetExpressionScript(this, expr, variable, new Expression(value, GameLoader), GameLoader);
                }
                else
                {
                    return new SetScriptScript(this, expr, variable, ScriptFactory.CreateScript(value), GameLoader);
                }
            }

            return null;
        }

        internal IFunction GetAppliesTo(string value, out string variable)
        {
            string var = value.Trim();
            string obj;
            Utility.ResolveVariableName(ref var, out obj, out variable);
            return (obj == null) ? null : new Expression(obj, GameLoader);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }
    }

    public abstract class SetScriptBase : ScriptBase
    {
        private IFunction m_appliesTo;
        private string m_property;
        private SetScriptConstructor m_constructor;
        private GameLoader m_loader;

        internal SetScriptBase(SetScriptConstructor constructor, IFunction appliesTo, string property, GameLoader loader)
        {
            m_constructor = constructor;
            AppliesTo = appliesTo;
            Property = property;
            m_loader = loader;
        }

        protected IFunction AppliesTo
        {
            get { return m_appliesTo; }
            private set
            {
                m_appliesTo = value;
            }
        }

        protected string Property
        {
            get { return m_property; }
            private set
            {
                m_property = value;
            }
        }

        public override string Save()
        {
            string result = string.Empty;

            if (AppliesTo != null)
            {
                result = string.Format("set({0}, \"{1}\", {2});", AppliesTo.Save(), Property, GetSaveString());
            }
            else
            {
                string varName = Utility.ReplaceReservedVariableNames(Property);
                result = "var " + varName;
                if (m_loader.Elements.ContainsKey(varName))
                {
                    m_loader.AddWarning(string.Format("Variable '{0}' clashes with object name", varName));
                }
                result += " = " + GetSaveString() + ";";
            }

            return result;
        }

        protected abstract string GetSaveString();
    }

    public class SetExpressionScript : SetScriptBase
    {
        private Expression m_expr;

        public SetExpressionScript(SetScriptConstructor constructor, IFunction appliesTo, string property, Expression expr, GameLoader loader)
            : base(constructor, appliesTo, property, loader)
        {
            m_expr = expr;
        }

        protected override string GetSaveString()
        {
            return m_expr.Save();
        }
    }

    public class SetScriptScript : SetScriptBase
    {
        private IScript m_script;
        private IScriptFactory m_scriptFactory;

        public SetScriptScript(SetScriptConstructor constructor, IFunction appliesTo, string property, IScript script, GameLoader loader)
            : base(constructor, appliesTo, property, loader)
        {
            m_script = script;
            m_scriptFactory = constructor.ScriptFactory;
        }

        protected override string GetSaveString()
        {
            return string.Format("function() {{ {0} }}", m_script.Save());
        }
    }
}
