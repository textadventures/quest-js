using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class RunDelegateScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "rundelegate"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            if (parameters.Count < 2)
            {
                throw new Exception("Expected at least 2 parameters in rundelegate call");
            }

            List<IFunction> paramExpressions = new List<IFunction>();
            IFunction obj = null;
            int cnt = 0;
            IFunction delegateName = null;

            foreach (string param in parameters)
            {
                cnt++;
                switch (cnt)
                {
                    case 1:
                        obj = new Expression(param, GameLoader);
                        break;
                    case 2:
                        delegateName = new Expression(param, GameLoader);
                        break;
                    default:
                        paramExpressions.Add(new Expression(param, GameLoader));
                        break;
                }
            }

            return new RunDelegateScript(obj, delegateName, paramExpressions);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { }; }
        }
    }

    public class RunDelegateScript : ScriptBase
    {
        private IFunction m_delegate;
        private FunctionCallParameters m_parameters;
        private IFunction m_appliesTo = null;

        public RunDelegateScript(IFunction obj, IFunction del, IList<IFunction> parameters)
        {
            m_delegate = del;
            m_parameters = new FunctionCallParameters(parameters);
            m_appliesTo = obj;
        }

        public override string Save()
        {
            List<string> saveParameters = new List<string>();
            saveParameters.Add(m_appliesTo.Save());
            saveParameters.Add(m_delegate.Save());
            foreach (IFunction p in m_parameters.Parameters)
            {
                saveParameters.Add(p.Save());
            }

            return SaveScript("rundelegate", saveParameters.ToArray());
        }
    }
}
