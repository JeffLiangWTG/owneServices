using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CU2CUniveralISO8859Tests
{
	[TestClass]
	public class CCU2CUniveralISO8859_Tests
	{
		const string filePath = "CU2CUniveralISO8859.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCU2CUniveralISO8859()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
			AssertMapping("Test3_input.xml", "Test3_output.xml");
			AssertMapping("Test4_input.xml", "Test4_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CU2CUniveralISO8859>(input, expectedOutput);
		}
	}
}
