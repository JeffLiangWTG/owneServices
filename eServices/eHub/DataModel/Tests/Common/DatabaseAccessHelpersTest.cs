using CargoWise.eHub.DataModel.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.DataModel.Tests.Common
{
    [TestClass]
    public class DatabaseAccessHelpersTest
    {
        [TestMethod]
        public void TestDatabaseAccessHelpers_RetryTimeLimit_DefaultValue()
        {
            Assert.AreEqual(600000, DatabaseAccessHelpers.RetryTimeLimit);
        }
    }
}
