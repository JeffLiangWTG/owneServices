using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.Native.Tests.UniversalInterchange2UniversalShipmentTests
{
	[TestClass]
	public class UniversalInterchange2UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2UniversalShipment()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalInterchange2UniversalShipment.TestFiles.UniversalInterchange2UniversalShipment_input.xml";
			string expectedFile = "UniversalInterchange2UniversalShipment.TestFiles.UniversalInterchange2UniversalShipment_output.xml";
			mapTester.Execute<CargoWise.eHub.Clients.EDI.Universal.Transforms.UniversalInterchange2UniversalShipment.UniversalInterchange2UniversalShipment>(sourceFile, expectedFile);

			expectedFile = "UniversalInterchange2UniversalShipment.TestFiles.UniversalInterchange2UniversalShipment_output_2012.xml";
			mapTester.Execute<CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchange2UniversalShipment>(sourceFile, expectedFile);
		}
	}
}
