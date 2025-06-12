using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.JPCustoms.Transforms.SAS108FlatFile2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class SAS108FlatFile2UniversalEventTest
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFile2UniversalEventTest1()
		{
			string sourceFile = "SAS108FlatFile2UniversalEvent_input.input_1.xml";
			string outputFile = "SAS108FlatFile2UniversalEvent_output.output_1.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS108FlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFile2UniversalEventTest2()
		{
			string sourceFile = "SAS108FlatFile2UniversalEvent_input.input_2.xml";
			string outputFile = "SAS108FlatFile2UniversalEvent_output.output_2.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<SAS108FlatFile2UniversalEvent>(sourceFile, outputFile);
		}
	}
}