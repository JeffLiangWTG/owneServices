using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.UniversalShipment2UniversalInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.Universal.Tests
	{
	[TestClass]
	public class UniversalShipment2UniversalInterchangeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2UniversalInterchange()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment2UniversalInterchange.TestFiles.UniversalShipment_input.xml";
			string expectedFile = "UniversalShipment2UniversalInterchange.TestFiles.UniversalInterchange_output.xml";
			mapTester.Execute<UniversalShipment2UniversalInterchange>(sourceFile, expectedFile);

			expectedFile = "UniversalShipment2UniversalInterchange.TestFiles.UniversalInterchange_output_2012.xml";
			mapTester.Execute<CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalShipment2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}
