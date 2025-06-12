using System;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.EDIFACT.IFCSUM2UniversalShipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.EDIFACT.Tests
{
	[TestClass]
	public class IFCSUM2UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformIFCSUM2UniversalShipment()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "IFCSUM2UniversalShipment.TestFiles.IFCSUM_Input.xml";
			string expectedFile = "IFCSUM2UniversalShipment.TestFiles.UniversalShipment_output.xml";
			mapTester.Execute<IFCSUM2UniversalShipment>(sourceFile, expectedFile);
		}
	}
}
