using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.JPCustoms.Transforms.SAS157FlatFile2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class SAS157FlatFile2UniversalEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS157FlatFile2UniversalEventTest1()
		{
			var sourceFile = "SAS157FlatFile2UniversalEvent_input.input_1.xml";
			var outputFile = "SAS157FlatFile2UniversalEvent_output.output_1.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS157FlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS157FlatFile2UniversalEventTest2()
		{
			var sourceFile = "SAS157FlatFile2UniversalEvent_input.input_2.xml";
			var outputFile = "SAS157FlatFile2UniversalEvent_output.output_2.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS157FlatFile2UniversalEvent>(sourceFile, outputFile);
		}
	}
}