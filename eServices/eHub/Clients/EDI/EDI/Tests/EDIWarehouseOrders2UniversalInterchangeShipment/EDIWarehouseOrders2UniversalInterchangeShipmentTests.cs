using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.EDIWarehouseOrders2UniversalInterchangeShipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace CargoWise.eHub.Clients.EDI.Tests
{
	[TestClass]
	public class EDIWarehouseOrders2UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImplusOrder2UniversalShipment()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "EDIWarehouseOrders2UniversalInterchangeShipment.TestFiles.EDIWarehouseOrders2UniversalInterchangeShipment_input.xml";
			string expectedFile = "EDIWarehouseOrders2UniversalInterchangeShipment.TestFiles.EDIWarehouseOrders2UniversalInterchangeShipment_output.xml";

			mapTester.Execute<EDIWarehouseOrders2UniversalInterchangeShipment>(sourceFile, expectedFile);
		}
	}
}
