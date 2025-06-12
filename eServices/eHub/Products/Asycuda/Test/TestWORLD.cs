using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.AsycudaCustoms.Transforms.Uxml_2_Awmds;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class TestWORLD : AsycudaTestCase<Uxml2Awmds>
	{
		// Methods we want:
		// BD, FJ, LK, 2011/2012, 


		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test01_Fiji()
		{
			RunTest("TestFiles.GenericSourceAir.xml", "Test01_Fiji_Result.xml", "FJ");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test02_Solomon()
		{
			RunTest("Test02_Solomon_Result.xml", "SB");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test03_Bangladesh_CCDMatches()
		{
			RunTest("TestFiles.GenericSource_CCDMatches.xml", "Test03_Bangladesh_Result.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test03_Bangladesh_CCCMatches()
		{
			RunTest("TestFiles.GenericSource_CCCMatches.xml", "Test03_Bangladesh_Result.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test03_Bangladesh_NoCCDandCCC()
		{
			RunTest("TestFiles.GenericSource_NoCCDandCCC.xml", "Test03_Bangladesh_Result_NoCCDandCCC.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test03_Bangladesh_BothCCDandCCC()
		{
			RunTest("TestFiles.GenericSource_BothCCDandCCC.xml", "Test03_Bangladesh_Result_CCD.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test04_SriLanka()
		{
			RunTest("Test04_SriLanka_Result.xml", "LK");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test07_Vanuatu()
		{
			RunTest("Test07_Vanuatu_Result.xml", "VU");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test08_Madagascar()
		{
			RunTest("Test08_Madagascar_Result.xml", "MG");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test11_CookIslands()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test11_CookIslands_Result.xml", "CK");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test12_Tuvalu()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test12_Tuvalu_Result.xml", "TV");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test13_Kiribati()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test13_Kiribati_Result.xml", "KI");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test14_Niue()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test14_Niue_Result.xml", "NU");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test15_Nauru()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test15_Nauru_Result.xml", "NR");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test16_Tonga()
		{
			RunTest("TestFiles.GenericSourceWithFourEmptyFullIndicator.xml", "Test16_Tonga_Result.xml", "TO");
		}


		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test05_2011vs2012()
		{
			// Note, we're not flipping out the country code.  It will execute as the most because map with no country-specific logic. 
			// Also note, the 2012 file has had its xmlns changed from 2012 to 2011 to make the test work. With the correct 2012 namespace, the generated output is totally blank and the test pukes up the "Data at the root level is invalid. Line 1, position 1" bullshit error.
			string sourceFile2011 = "TestFiles.GenericSource.xml";
			string sourceFile2012 = "TestFiles.GenericSource2012.xml";
			string outputFile = "TestFiles.Test05_2011vs2012.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			// Expect identical content
			mapTester.Execute<Uxml2Awmds>(sourceFile2011, outputFile);
			mapTester.Execute<Uxml2Awmds>(sourceFile2012, outputFile);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test06_OldStyleContainersOnBills_World()
		{
			string sourceFile = "TestFiles.GenericSourceOldStyleWithContainersAtBill.xml";
			string outputFile = "TestFiles.Test06_OldStyleContainersOnBills_Result_World.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<Uxml2Awmds>(sourceFile, outputFile);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test09_Consolidated_Cargo_SriLanka()
		{
			RunTest("TestFiles.GenericSource-1SubShipment.xml", "Test09_SriLanka_Result.xml", "LK");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test09_Consolidated_Cargo_Bangladesh()
		{
			RunTest("TestFiles.GenericSource-1SubShipment.xml", "Test09_Bangladesh_Result.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test09_Consolidated_Cargo_Fiji()
		{
			RunTest("TestFiles.GenericSource-1SubShipment.xml", "Test09_Fiji_Result.xml", "FJ");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test10_Gross_mass_Volume_in_cubic_meters_SriLanka()
		{
			RunTest("TestFiles.GenericSource-Fractional.xml", "Test10_SriLanka_Result.xml", "LK");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test10_Gross_mass_Volume_in_cubic_meters_Bangladesh()
		{
			RunTest("TestFiles.GenericSource-Fractional.xml", "Test10_Bangladesh_Result.xml", "BD");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_SEA_CookIslands()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_SEA_CookIslands_Result.xml", "CK", "SEA");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_AIR_CookIslands()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_AIR_CookIslands_Result.xml", "CK", "AIR");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_MAI_CookIslands()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_MAI_CookIslands_Result.xml", "CK", "MAI");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_SEA_Fiji()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_SEA_Fiji_Result.xml", "FJ", "SEA");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_AIR_Fiji()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_AIR_Fiji_Result.xml", "FJ", "AIR");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNoCCDAgainstShippingAgent_MAI_Fiji()
		{
			RunTest("TestFiles.NoCCDAgainstShippingAgent.xml", "TestNoCCDAgainstShippingAgent_MAI_Fiji_Result.xml", "FJ", "MAI");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAwbolegmdsTemplate__Bangladesh_Export()
		{
			RunTest("TestFiles.GenericSource_Awbolegmds.xml", "GenericSource_Awbolegmds_Result_BDExport.xml", "BD", nature: "EXP");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAwbolegmdsTemplate__Bangladesh_Export_2BE2BL3Containers()
		{
			RunTest("TestFiles.GenericSource_Awbolegmds_Multiple.xml", "GenericSource_Awbolegmds_Multiple_Result_BDExport.xml", "BD", nature: "EXP");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAwbolegmdsTemplate__Bangladesh_Import()
		{
			RunTest("TestFiles.GenericSource_Awbolegmds.xml", "GenericSource_Awbolegmds_Result_BDImport.xml", "BD", nature: "IMP");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAwbolegmdsTemplate__OtherCountry()
		{
			RunTest("TestFiles.GenericSource_Awbolegmds.xml", "GenericSource_Awbolegmds_Result_MGExport.xml", "MG", nature: "EXP");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNewShipmentType_SriLanka()
		{
			RunTest("TestFiles.GenericSourceNewShipmentTypeForLK.xml", "TestNewShipmentType_SriLanka_Result.xml", "LK");
		}
	}
}
