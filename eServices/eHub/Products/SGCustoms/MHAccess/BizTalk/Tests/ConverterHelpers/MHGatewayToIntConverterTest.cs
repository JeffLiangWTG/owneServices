using CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    [TestClass]
    public class MHGatewayToIntConverterTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestToIntEmptyString() => TestToIntCore("", 0);

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestToIntNegativeNumber() => TestToIntCore("-1", -1);

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestToIntPositiveNumber() => TestToIntCore("1001", 1001);

        void TestToIntCore(string input, int expectedOutput)
        {
            var output = MHGatewayToIntConverter.ToInt(input);

            Assert.AreEqual(expectedOutput, output, $"Value should be {expectedOutput}");
        }
    }
}
