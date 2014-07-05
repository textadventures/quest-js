using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class CreateScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateScript : ScriptBase
    {
        private IFunction m_expr;

        public CreateScript(IFunction expr)
        {
            m_expr = expr;
        }

        public override string Save(Context c)
        {
            return SaveScript("create", m_expr.Save(c));
        }
    }

    public class CreateExitScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create exit"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 3:
                    return new CreateExitScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader));
                case 4:
                    return new CreateExitScript(new Expression(parameters[0], GameLoader), new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader), new Expression(parameters[3], GameLoader));
                case 5:
                    return new CreateExitScript(new Expression(parameters[1], GameLoader), new Expression(parameters[2], GameLoader), new Expression(parameters[3], GameLoader), new Expression(parameters[4], GameLoader), new Expression(parameters[0], GameLoader));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3, 4, 5 }; }
        }
    }

    public class CreateExitScript : ScriptBase
    {
        private IFunction m_name;
        private IFunction m_from;
        private IFunction m_to;
        private IFunction m_initialType;
        private IFunction m_id;

        public CreateExitScript(IFunction name, IFunction from, IFunction to)
        {
            m_name = name;
            m_from = from;
            m_to = to;
        }

        public CreateExitScript(IFunction name, IFunction from, IFunction to, IFunction initialType)
            : this(name, from, to)
        {
            m_initialType = initialType;
        }

        public CreateExitScript(IFunction name, IFunction from, IFunction to, IFunction initialType, IFunction id)
            : this(name, from, to, initialType)
        {
            m_id = id;
        }

        public override string Save(Context c)
        {
            if (m_initialType == null)
            {
                return SaveScript("createexit", m_name.Save(c), m_from.Save(c), m_to.Save(c));
            }
            else
            {
                // TODO: Add support for id parameter
                return SaveScript("createexit_withtype", m_name.Save(c), m_from.Save(c), m_to.Save(c), m_initialType.Save(c));
            }
        }
    }

    public class CreateTimerScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create timer"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateTimerScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTimerScript : ScriptBase
    {
        private IFunction m_expr;

        public CreateTimerScript(IFunction expr)
        {
            m_expr = expr;
        }

        public override string Save(Context c)
        {
            return SaveScript("createtimer", m_expr.Save(c));
        }
    }

    public class CreateTurnScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "create turnscript"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new CreateTurnScript(new Expression(parameters[0], GameLoader));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class CreateTurnScript : ScriptBase
    {
        private IFunction m_expr;

        public CreateTurnScript(IFunction expr)
        {
            m_expr = expr;
        }

        public override string Save(Context c)
        {
            return SaveScript("createturnscript", m_expr.Save(c));
        }
    }
}
