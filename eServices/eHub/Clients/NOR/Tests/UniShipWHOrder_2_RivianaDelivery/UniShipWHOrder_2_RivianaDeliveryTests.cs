using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.UniShipWHOrder_2_RivianaDelivery;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class UniShipWHOrder_2_RivianaDeliveryTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShipWHOrder_2_RivianaDelivery()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniShipWHOrder_2_RivianaDelivery.TestFiles.UniShipWHOrder_2_RivianaDelivery_input.xml";
			string expectedFile = "UniShipWHOrder_2_RivianaDelivery.TestFiles.UniShipWHOrder_2_RivianaDelivery_output.xml";
			mapTester.Execute<UniShipWHOrder_2_RivianaDelivery>(sourceFile, expectedFile);
		}
	}
}
