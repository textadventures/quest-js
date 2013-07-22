using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    // TO DO: Redundant class, should simply be a field in FunctionCallScript

    internal class FunctionCallParameters
    {
        private IList<IFunction> m_parameters;

        public FunctionCallParameters(IList<IFunction> parameters)
        {
            m_parameters = parameters;
        }

        public IList<IFunction> Parameters
        {
            get { return m_parameters; }
        }
    }
}
