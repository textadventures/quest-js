using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class FunctionCallScriptConstructor : IScriptConstructor
    {
        public IScript Create(string script, Element proc)
        {
            List<IFunction> paramExpressions = null;
            string procName, afterParameter;

            string param = Utility.GetParameter(script, out afterParameter);
            IScript paramScript = null;

            // Handle functions of the form
            //    SomeFunction (parameter) { script }
            if (afterParameter != null)
            {
                afterParameter = afterParameter.Trim();
                if (afterParameter.Length > 0)
                {
                    string paramScriptString = Utility.GetScript(afterParameter);
                    paramScript = ScriptFactory.CreateScript(paramScriptString);
                }
            }

            if (param == null && paramScript == null)
            {
                procName = script;
            }
            else
            {
                if (param != null)
                {
                    List<string> parameters = Utility.SplitParameter(param);
                    procName = script.Substring(0, script.IndexOf('(')).Trim();
                    paramExpressions = new List<IFunction>();
                    if (param.Trim().Length > 0)
                    {
                        foreach (string s in parameters)
                        {
                            paramExpressions.Add(new Expression(s, GameLoader));
                        }
                    }
                }
                else
                {
                    procName = script.Substring(0, script.IndexOfAny(new char[] { '{', ' ' }));
                }
            }

            return new FunctionCallScript(GameLoader, procName, paramExpressions, paramScript);
        }

        public IScriptFactory ScriptFactory { get; set; }
        public GameLoader GameLoader { get; set; }

        public string Keyword
        {
            get { return null; }
        }
    }

    public class FunctionCallScript : ScriptBase
    {
        private string m_procedure;
        private FunctionCallParameters m_parameters;
        private IScript m_paramFunction;
        private GameLoader m_loader;

        public FunctionCallScript(GameLoader loader, string procedure)
            : this(loader, procedure, null, null)
        {
        }

        public FunctionCallScript(GameLoader loader, string procedure, IList<IFunction> parameters, IScript paramFunction)
        {
            m_loader = loader;
            m_procedure = procedure.Replace(" ", Utility.SpaceReplacementString);
            m_parameters = new FunctionCallParameters(parameters);
            m_paramFunction = paramFunction;
        }

        public override string Save(Context c)
        {
            if (!m_loader.Elements.ContainsKey(m_procedure.Replace(Utility.SpaceReplacementString, " ")))
            {
                if (m_procedure != "requestsave" && m_procedure != "requestspeak")
                {
                    throw new Exception(string.Format("Unknown function '{0}'", m_procedure));
                }
            }

            if ((m_parameters == null || m_parameters.Parameters == null || m_parameters.Parameters.Count == 0) && m_paramFunction == null)
            {
                return m_procedure + "();";
            }

            List<IFunction> parameters = new List<IFunction>();
            foreach (IFunction p in m_parameters.Parameters)
            {
                parameters.Add(p);
            }

            List<string> saveParameters = new List<string>(parameters.Select(p => p.Save(c)));

            if (m_paramFunction != null)
            {
                saveParameters.Add(string.Format("function (result) {{ {0} }}", m_paramFunction.Save(c)));
            }

            return SaveScript(m_procedure, saveParameters.ToArray());
        }
    }
}
