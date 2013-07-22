using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace CompilerTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestConvertObjectDotNotation()
        {
            List<string> objectNames = new List<string> { "myobject", "otherobject" };
            //Assert.AreEqual("test.attribute", Utility.ConvertObjectDotNotation("test.attribute", objectNames));
            //Assert.AreEqual("object_myobject.attribute", Utility.ConvertObjectDotNotation("myobject.attribute", objectNames));
            //Assert.AreEqual("object_otherobject.attribute", Utility.ConvertObjectDotNotation("otherobject.attribute", objectNames));
        }
    }
}
