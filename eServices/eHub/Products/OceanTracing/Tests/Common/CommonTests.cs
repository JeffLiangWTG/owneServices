using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanTracing.Transforms.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanTracing.Tests
{
	[TestClass]
	public class Common_Tests
	{
		const string filePath = "Common.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonUShipmentCleanup()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<UShipmentCleanup>(input, expectedOutput);
		}
	}
}
