using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Cargowise2Cargowise_US2UI_BKC_Tests
  {
    const string filePath = "Cargowise2Cargowise.BookingConfirmation.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUS2UI_BKC()
    {
      AssertMapping("Test1_CLD_NVOCC_input.xml", "Test1_CLD_NVOCC_output.xml", "C00001002", "NVOCC", "CLD");
      AssertMapping("Test2_CLD_ShippingLine_input.xml", "Test2_CLD_ShippingLine_output.xml", "C00001002", "ShippingLine", "CLD");

      AssertMapping("Test3_AGT_NVOCC_input.xml", "Test3_AGT_NVOCC_output.xml", "C00001002", "NVOCC", "AGT");
      AssertMapping("Test4_AGT_ShippingLine_input.xml", "Test4_AGT_ShippingLine_output.xml", "C00001002", "ShippingLine", "AGT");

      AssertMapping("Test1_CLD_NVOCC_input.xml", "Test1_CLD_NVOCC_IRJ_output.xml", "C00001002", "NVOCC", "CLD", "IRJ");

      AssertMapping("Test5_input.xml", "Test5_output.xml", "C00001002", "NVOCC", "CLD");

      AssertMapping("Test6_input_EventMWA.xml", "Test6_output_EventMWA.xml", "C00001002", "NVOCC", "CLD", eventType:"MWA");

      AssertMapping("Test7_input_No_RecipientID.xml", "Test7_output_No_RecipientID.xml", "C00001002", "NVOCC", "CLD", recipientID: "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolNumber, string ehubPartyType, string shipmentType, string eventType = "MAA", string recipientID = "HYEUAT999")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEUAT001").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Once();
      mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return(recipientID).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetProvider()).Return("HYEUAT001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ActionPurpose")).Return("AMD").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "PartyType")).Return(ehubPartyType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HYEUAT001", "@recipientId", "", "@ST_ID", "CW1MSG", "@value", "WTG0000000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Type", "Booking Confirmation", "AMD", "MAA")).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "Booking Confirmation", "AMD", "MAA")).Return("Booking Accepted").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOWISE", "CARGOWISE", "CARGOWISE Provider Configuration", "Event Type", "Event Reference", "Booking Confirmation", "AMD", "IRJ")).Return("Booking Rejected").Repeat.Any();

      var currentUTCDateTime = "1741008269";
      if (recipientID != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"MEDUJ2388158|1740629900", "547IN2001425|C00001002|ECUW", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"MEDUJ2388158|{currentUTCDateTime}", "CARRIER BOOKING REF|C00001002|C1GS", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"MEDUJ2388158|{currentUTCDateTime}", "547IN2001425|C00001002|C1H2", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"MEDUJ2388158|{currentUTCDateTime}", "547IN2001425|C00001002|C1GS", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"MEDUJ2388158|{currentUTCDateTime}", "|C00001002|C1GS", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, "HYEUAT001", $"Carrier WayBill Number|{currentUTCDateTime}", "CARRIER BOOKING REF|C00001002|C1GS", "IFTMBC")).Repeat.Any();
      }

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmmss")).Return(currentUTCDateTime).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper},
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<US2UI_BKC>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockOCMHelper.VerifyAllExpectations();
      mockSubscriptionHelper.VerifyAllExpectations();
    }
  }
}