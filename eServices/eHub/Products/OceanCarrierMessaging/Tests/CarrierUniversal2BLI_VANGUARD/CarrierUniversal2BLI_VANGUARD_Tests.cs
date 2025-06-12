using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2BLI_VANGUARD;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class CarrierUniversal2BLI_VANGUARD_Tests
	{
		const string filePath = "CarrierUniversal2BLI_VANGUARD.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCarrierUniversal2BLI_VANGUARD()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "ABC");
			AssertMapping("Test2_input_CoLoad.xml", "Test2_output_CoLoad.xml", "CLD", payableElseWhere: "A");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "ABC", isSummary: "TRUE");
			AssertMapping("Test4_input.xml", "Test4_output.xml", "ABC");
      AssertMapping("Test5_input_GroupingMethod.xml", "Test5_output_GroupingMethod.xml", "AGT");
    }

		void AssertMapping(string inputFile, string expectedOutputFile, string shipmentType = "", string isSummary = "FALSE", string payableElseWhere = "")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("edi_test_prod");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VANGUARD_SI1");
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
			mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd_mmssms")).Return("20180101_115059_100");
			mockDateMapper.Expect(x => x.CurrentDateTime("yyyy-MM-dd")).Return("2019-05-17");
			mockDateMapper.Expect(x => x.CurrentDateTime("HH:mm:ss")).Return("10:21:13");


			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VANGUARD", "@recipientId", "edi_test_prod", "@ST_ID", "VGDMSG", "@value", "CEIS0000680261", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any(); ;

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "11", "CEIS0000680261"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "11"));
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("edi_test_prod", "BN1", "VANGUARD")).Return("CGWS");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.VANGUARD", "@maxlength", "14")).Return("11");
			mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "ContainerTypeToISOCode", "VANGUARD Code", "22G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "ContainerTypeToISOCode", "VANGUARD Code", "45R1")).Return("45R1").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "Receiver ID", "Receiver")).Return("vanguard");
			mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("VANGUARD", "VANGUARD", "VANGUARD Provider Configuration", "Sender ID", "Sender")).Return("edi_cargowise_prod");

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", "AMD", "ActionPurpose")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("VGDMSG", "VANGUARD", "edi_test_prod", "C03078216", shipmentType, "ShipmentType")).Repeat.Any();

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VGD_SI_0000000011"));

			mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "edi_test_prod", "VANGUARD")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("VANGUARD_SI1")).Return("VANGUARD").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("VANGUARD")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("edi_test_prod", "CGWS", "VANGUARD", "C03078216", "CEIS0000680261")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

			MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly());
			MapTester mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester2.Execute<CarrierUniversal2BLI_VANGUARD>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
