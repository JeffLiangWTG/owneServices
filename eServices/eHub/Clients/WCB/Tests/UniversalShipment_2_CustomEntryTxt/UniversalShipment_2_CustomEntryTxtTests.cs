using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.WCB.Transforms.UniversalShipment_2_CustomEntryTxt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.WCB.Tests
{
	[TestClass]
	public class UniversalShipment_2_CustomEntryTxtTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomEntryTxt_MessageType_EXW()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.input_MessageType_EXW.xml";
			string expectedFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.output_MessageType_EXW.xml";
			mapTester.Execute<UniversalShipment_2_CustomEntryTxt>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomEntryTxt_IsPackToBondForLine()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.input_IsPackToBondForLine.xml";
			string expectedFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.output_IsPackToBondForLine.xml";
			mapTester.Execute<UniversalShipment_2_CustomEntryTxt>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomEntryTxt_PackCountForNature10()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.input_PackCountForNature10.xml";
			string expectedFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.output_PackCountForNature10.xml";
			mapTester.Execute<UniversalShipment_2_CustomEntryTxt>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment_2_CustomEntryTxt_PackCountForBond()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.input_PackCountForBond.xml";
			string expectedFile = "UniversalShipment_2_CustomEntryTxt.TestFiles.output_PackCountForBond.xml";
			mapTester.Execute<UniversalShipment_2_CustomEntryTxt>(sourceFile, expectedFile);
		}
	}
}
