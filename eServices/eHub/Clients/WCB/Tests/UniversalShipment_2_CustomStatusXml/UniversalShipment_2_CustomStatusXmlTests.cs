using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.WCB.Transforms.UniversalShipment_2_CustomStatusXml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.WCB.Tests
{
	[TestClass]
	public class UniversalShipment_2_CustomStatusXmlTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomStatusXml_Air()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomStatusXml.TestFiles.input_AIR.xml";
			string expectedFile = "UniversalShipment_2_CustomStatusXml.TestFiles.output_AIR.xml";
			mapTester.Execute<UniversalShipment_2_CustomStatusXml>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomStatusXml_Sea()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomStatusXml.TestFiles.input_SEA.xml";
			string expectedFile = "UniversalShipment_2_CustomStatusXml.TestFiles.output_SEA.xml";
			mapTester.Execute<UniversalShipment_2_CustomStatusXml>(sourceFile, expectedFile);
		}
	}
}
