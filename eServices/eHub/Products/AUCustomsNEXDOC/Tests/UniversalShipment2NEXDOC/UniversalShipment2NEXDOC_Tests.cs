using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.AUCustomsNEXDOC.Configuration;
using CargoWise.eHub.Products.AUCustomsNEXDOC.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Tests
{
	[TestClass]
	public class UniversalShipment2NEXDOC_Tests
	{
		const string filePath = "UniversalShipment2NEXDOC.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_1()
		{
			AssertMapping("Test1_ORDER_input.xml", "Test1_ORDER_output.xml", "B60001868_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_2()
		{
			AssertMapping("Test2_AMEND_input.xml", "Test2_AMEND_output.xml", "B60001868_AMEND");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_3()
		{
			AssertMapping("Test3_LODGE_input.xml", "Test3_LODGE_output.xml", "B60001868_LODGE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_4()
		{
			AssertMapping("Test4_WITHDRAW_input.xml", "Test4_WITHDRAW_output.xml", "B60001868_WITHDRAW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_5()
		{
			AssertMapping("Test5_ORDER_input.xml", "Test5_ORDER_output.xml", "B60001882_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_6()
		{
			AssertMapping("Test6_ORDER_input.xml", "Test6_ORDER_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_7()
		{
			AssertMapping("Test7_AMEND_input.xml", "Test7_AMEND_output.xml", "B60001868_AMEND");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_8()
		{
			AssertMapping("Test8_AdditionalTexts_input.xml", "Test8_AdditionalTexts_output.xml", "B60001882_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_9()
		{
			AssertMapping("Test9_Replace_input.xml", "Test9_Replace_output.xml", "B60002367_REPLACE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_10()
		{
			AssertMapping("Test10_TRFEDN_input.xml", "Test10_TRFEDN_output.xml", "B60002367_TRFEDN");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_11()
		{
			AssertMapping("Test11_CANEDN_input.xml", "Test11_CANEDN_output.xml", "B60002367_CANEDN");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_12()
		{
			AssertMapping("Test12_CANREX_input.xml", "Test12_CANREX_output.xml", "B60002367_CANREX");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_13()
		{
			AssertMapping("Test13_REISSUE_input.xml", "Test13_REISSUE_output.xml", "B60002367_REISSUE");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_14()
		{
			AssertMapping("Test14_PREVIEW_input.xml", "Test14_PREVIEW_output.xml", "B60001868_PREVIEW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_15()
		{
			AssertMapping("Test15_LOC_EU_input.xml", "Test15_LOC_EU_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_16()
		{
			AssertMapping("Test16_LOC_EU_PlaceOfDestinationOverrided_input.xml", "Test16_LOC_EU_PlaceOfDestinationOverrided_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_17()
		{
			AssertMapping("Test17_READREX_input.xml", "Test17_READREX_output.xml", "B60001868_READREX");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_EUTransit01()
		{
			AssertMapping("Test_EUTransit_01_input.xml", "Test_EUTransit_01_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_EUTransit02()
		{
			AssertMapping("Test_EUTransit_02_input.xml", "Test_EUTransit_02_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_EUTransit03()
		{
			AssertMapping("Test_EUTransit_03_input.xml", "Test_EUTransit_03_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_EUTransit04()
		{
			AssertMapping("Test_EUTransit_04_input.xml", "Test_EUTransit_04_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_fobAmount01()
		{
			AssertMapping("Test_fobAmount_01_input.xml", "Test_fobAmount_01_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_fobAmount02()
		{
			AssertMapping("Test_fobAmount_02_input.xml", "Test_fobAmount_02_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_fobAmount03()
		{
			AssertMapping("Test_fobAmount_03_input.xml", "Test_fobAmount_03_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_UseNEXDOCStaging_Fish()
		{
			AssertMapping("Test_UseNEXDOCStaging_Fish_input.xml", "Test_UseNEXDOCStaging_Fish_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_UseNEXDOC_Fish()
		{
			AssertMapping("Test_UseNEXDOC_Fish_input.xml", "Test_UseNEXDOC_Fish_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_UseNEXDOCStaging_NotFish()
		{
			AssertMapping("Test_UseNEXDOCStaging_NotFish_input.xml", "Test_UseNEXDOCStaging_NotFish_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_UseNEXDOC_NotFishFish()
		{
			AssertMapping("Test_UseNEXDOC_NotFish_input.xml", "Test_UseNEXDOC_NotFish_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_exporterDeclarationCodeExcludeSpecialCodes()
		{
			AssertMapping("Test_ExporterDeclarationCodeExcludeSpecialCodes_input.xml", "Test_ExporterDeclarationCodeExcludeSpecialCodes_output.xml", "B60001869_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_CommodityType_WOL()
		{
			AssertMapping("Test_NEXDOC_CommodityType_WOL_input.xml", "Test_NEXDOC_CommodityType_WOL_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_CommodityType_SKN()
		{
			AssertMapping("Test_NEXDOC_CommodityType_SKN_input.xml", "Test_NEXDOC_CommodityType_SKN_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_CommodityType_IME()
		{
			AssertMapping("Test_NEXDOC_CommodityType_IME_input.xml", "Test_NEXDOC_CommodityType_IME_output.xml", "B00220564_ORDER");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2NEXDOC_CommodityType_IME_incomplete()
		{
			AssertMapping("Test_NEXDOC_CommodityType_IME_incomplete_input.xml", "Test_NEXDOC_CommodityType_IME_incomplete_output.xml", "B00220564_ORDER");
		}

		private static void AssertMapping(string inputFile, string expectedOutputFile, string refID)
		{
			string input = filePath + inputFile;
			string expectedOutput = filePath + expectedOutputFile;

			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockNEXDOCDataModelAccessor = MockRepository.GenerateStrictMock<NEXDOCDataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockConfigurationAccessor = MockRepository.GenerateStrictMock<ConfigurationAccessor>();

			var senderID = "TESTSENDER";
			var recipientID = "NEXDOCS";

			if (refID.Contains("PREVIEW"))
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("NEXJOB", "NEXDOCS", "TESTSENDER", "111", "B60001868"));
			}

			mockNEXDOCDataModelAccessor.Expect(x => x.GetVendorToken(recipientID)).Return("bb955551e4af485480bfbc88f3485569");
			mockNEXDOCDataModelAccessor.Expect(x => x.GetInstallationToken(recipientID)).Return("VENDOR_ONLY_TEST_ea88dd3ea0f4415fa35786112985e055");
			mockNEXDOCDataModelAccessor.Expect(x => x.GetInstallationPassword(recipientID)).Return("Password!23");

			mockConfigurationAccessor.Expect(x => x.GetClientGroupToken(senderID)).Return("793872e03b6a497e9a1bb949bebc54a5");
			mockConfigurationAccessor.Expect(x => x.GetClientToken(senderID.Remove(3, 3).Remove(6), "BRK")).Return("a3eb42d1c80f44b19a2cc0b733c7dd08");

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", refID));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TESNDE_BRK"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", recipientID, "@recipientId", senderID,
				"@ST_ID", "NEXDOC", "@value", "111")).Return("2018-02-12");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockNEXDOCDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockConfigurationAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockDataModelAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			mapTester.ExecuteCompiled<UniversalShipment2NEXDOC>(input, expectedOutput);

			mockDataModelAccessor.VerifyAllExpectations();
			mockNEXDOCDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockConfigurationAccessor.VerifyAllExpectations();
		}
	}
}
