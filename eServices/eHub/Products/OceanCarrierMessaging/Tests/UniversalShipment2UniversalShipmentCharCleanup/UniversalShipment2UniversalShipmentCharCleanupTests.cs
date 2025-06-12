using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UShipmentCharCleanup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class UniversalShipment2UniversalShipmentCharCleanupTests
	{
		const string filePath = "UniversalShipment2UniversalShipmentCharCleanup.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2UniversalShipmentCharCleanup()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
    }

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<UShipment2UShipmentCharCleanup>(input, expectedOutput);
		}
	}
}
