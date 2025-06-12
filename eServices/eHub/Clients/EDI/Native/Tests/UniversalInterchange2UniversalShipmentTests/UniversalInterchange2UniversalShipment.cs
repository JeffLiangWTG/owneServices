using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.Native.UniversalInterchange2UniversalShipment;
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

			string sourceFile = "UniversalInterchange2UniversalShipmentTests.TestFiles.UniversalInterchange2UniversalShipment_input.xml";
			string expectedFile = "UniversalInterchange2UniversalShipmentTests.TestFiles.UniversalInterchange2UniversalShipment_output.xml";
			mapTester.Execute<UniversalInterchange2UniversalShipment>(sourceFile, expectedFile);
		}
	}
}
