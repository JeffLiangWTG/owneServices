using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Universal.Transforms.UniversalInterchange2NativeShipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.Native.Tests.UniversalInterchange2NativeShipmentTests
{
	[TestClass]
	public class UniversalInterchange2NativeShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2NativeShipment()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalInterchange2NativeShipment.TestFiles.UniversalInterchange2NativeShipment_input.xml";
			string expectedFile = "UniversalInterchange2NativeShipment.TestFiles.UniversalInterchange2NativeShipment_output.xml";
			mapTester.Execute<UniversalInterchange2NativeShipment>(sourceFile, expectedFile);
		}
	}
}
