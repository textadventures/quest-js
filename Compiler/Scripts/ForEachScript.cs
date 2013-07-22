using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class ForEachScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "foreach"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            string loop = Utility.GetScript(afterExpr);

            string[] parameters = Utility.SplitParameter(param).ToArray();
            if (parameters.Count() != 2)
            {
                throw new Exception(string.Format("'foreach' script should have 2 parameters: 'foreach ({0})'", param));
            }
            IScript loopScript = ScriptFactory.CreateScript(loop);

            string type = parameters[0];

            return new ForEachScript(parameters[0], new Expression(parameters[1], GameLoader), loopScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public GameLoader GameLoader { get; set; }
    }

    public class ForEachScript : ScriptBase
    {
        private IFunction m_list;
        private IScript m_loopScript;
        private string m_variable;

        public ForEachScript(string variable, IFunction list, IScript loopScript)
        {
            m_variable = variable;
            m_list = list;
            m_loopScript = loopScript;
        }

        public override string Save()
        {
            string result = string.Empty;
            string list = m_list.Save();
            if (list.Contains("("))
            {
                // if we're iterating over the results of a function, save the results to a variable first
                string iterateOver = list;
                list = string.Format("list_{0}", m_variable);
                result += string.Format("var {0} = {1};\n", list, iterateOver);
            }
            string arrayCheckVariable = list.Replace(".", "_") + "_isarray";
            result += string.Format("var {0} = (Object.prototype.toString.call({1}) === '[object Array]');\n", arrayCheckVariable, list);
            result += string.Format("for (var iterator_{0} in {1}) {{\n", m_variable, list);
            string variableName = Utility.ReplaceReservedVariableNames(m_variable);
            // TO DO: Need some way of warning if variable name clashes with an object name
            //if (m_elements.ContainsKey(variableName))
            //{
            //    AddWarning(string.Format("Variable '{0}' clashes with object name", variableName));
            //}
            string scriptString = string.Format("var {0} = {3} ? {1}[iterator_{2}] : iterator_{2};\n", variableName, list, m_variable, arrayCheckVariable);
            if (m_loopScript != null)
            {
                // Dictionaries contain a dummy key so that TypeOf can tell between a string dictionary and an object dictionary.
                // We add a condition here so that the dummy key is excluded from a foreach loop.
                string loopScriptAndCondition = string.Format("if ({0} || iterator_{1}!=\"__dummyKey\") {{ {2} }}", arrayCheckVariable, m_variable, m_loopScript.Save());
                scriptString += loopScriptAndCondition;
            }
            return result + scriptString + Environment.NewLine + "}";
        }

        public override string Keyword
        {
            get
            {
                return "foreach";
            }
        }
    }
}
