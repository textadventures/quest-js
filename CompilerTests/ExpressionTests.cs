using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;
using Moq;

namespace CompilerTests
{
    [TestClass]
    public class ExpressionTests
    {
        Mock<GameLoader> gameLoader;

        [TestInitialize]
        public void Init()
        {
            gameLoader = new Mock<GameLoader>();
            gameLoader.Setup(l => l.Elements).Returns(new Dictionary<string, Element> { { "myobject", new Element(ElementType.Object, gameLoader.Object) } });
        }

        [TestMethod]
        public void TestSave()
        {
            Expression testExpression = new Expression("myexpression", gameLoader.Object);
            Assert.AreEqual("myexpression", testExpression.Save());
        }

        [TestMethod]
        public void TestNot()
        {
            Expression testExpression = new Expression("not myexpression", gameLoader.Object);
            Assert.AreEqual("!(myexpression)", testExpression.Save());
        }

        [TestMethod]
        public void TestEquals()
        {
            Expression testExpression = new Expression("one = two", gameLoader.Object);
            Assert.AreEqual("one == two", testExpression.Save());
        }

        [TestMethod]
        public void TestNotEquals()
        {
            Expression testExpression = new Expression("one <> two", gameLoader.Object);
            Assert.AreEqual("one != two", testExpression.Save());
        }

        [TestMethod]
        public void TestGreaterEquals()
        {
            Expression testExpression = new Expression("one >= two", gameLoader.Object);
            Assert.AreEqual("one >= two", testExpression.Save());
        }

        [TestMethod]
        public void TestReplaceReservedKeywords()
        {
            Expression testExpression = new Expression("one + var", gameLoader.Object);
            Assert.AreEqual("one + variable_var", testExpression.Save());
        }

        [TestMethod]
        public void TestReplaceOverloadedFunctions()
        {
            Expression testExpression = new Expression("TypeOf(value)", gameLoader.Object);
            Assert.AreEqual("overloadedFunctions.TypeOf(value)", testExpression.Save());

            Expression testExpression2 = new Expression("(TypeOf(element[attribute]) = \"script\")", gameLoader.Object);
            Assert.AreEqual("(overloadedFunctions.TypeOf(element[attribute]) == \"script\")", testExpression2.Save());
        }

        [TestMethod]
        public void TestSpaceReplacement()
        {
            Expression testExpression = new Expression("object.my attribute", gameLoader.Object);
            Assert.AreEqual(string.Format("object.my{0}attribute", Utility.SpaceReplacementString), testExpression.Save());
        }
    }
}
