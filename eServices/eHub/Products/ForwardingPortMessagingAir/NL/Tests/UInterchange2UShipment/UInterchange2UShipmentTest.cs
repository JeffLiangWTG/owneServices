using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests
{
	[TestClass]
	public class UInterchange2UShipmentTest
	{
		const string filePath = "UInterchange2UShipment.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUInterchange2UShipment()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<UInterchange2UShipment>(input, expectedOutput);
		}
	}
}
