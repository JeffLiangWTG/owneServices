using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class X12_301_2UShipment_CAROTRANS_Tests
  {
    const string filePath = "BookingConfirmation.CAROTRANS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void Test_X12_301_2UShipment()
    {
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml", "A", "MAA", "Booking Confirmed");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml", "U", "MRR");
      AssertMapping("Test3_input_CoLoad.xml", "Test3_output_CoLoad_BusinessLayer.xml", "Test3_output_CoLoad_UInterchange.xml", "U", "MRR", shipmentType: "CLD");
      AssertMapping("Test4_input.xml", "Test4_output_BusinessLayer.xml", "Test4_output_UInterchange.xml", "A", "MAA", "Booking Confirmed");
      AssertMapping("Test5_input.xml", "Test5_output_BusinessLayer.xml", "Test5_output_UInterchange.xml", "A", "MAA", "Booking Confirmed", formVersion: "");
      AssertMapping("Test6_input_No_RecipientID.xml", "Test6_output_No_RecipientID_BusinessLayer.xml", "Test6_output_No_RecipientID_UInterchange.xml", "A", "MAA", "Booking Confirmed", formVersion: "", recipientID: "");
    }

    void AssertMapping(string inputFile, string businessLayerOutputFile, string expectedOutputFile, string eventCode, string eventType, string eventReference = "", string actionPurpose = "", string subscribedConsolRef = "", string shipmentType = "ABC", string formVersion = "2.0.0", string recipientID = "HYEUAT999")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerOutputFile = filePath + businessLayerOutputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CAROTRANS").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ZAZAZA").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Booking Confirmation X12_301 from CAROTRANS", "EventType", "EventType", eventCode, actionPurpose)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Booking Confirmation X12_301 from CAROTRANS", "EventType", "Reference", eventCode, actionPurpose)).Return(eventReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "CW0000000019", "@referenceType", "JobNumber")).Return(subscribedConsolRef == "" ? "C000093833" : subscribedConsolRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "CW0000000019", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "C000093833", "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRBRS", "@value", "C000093833")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "CW0000000019", "@referenceType", "FormVersion")).Return(formVersion).Repeat.Once();

      var currentDateTimeUTC = "20250304103854";
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmmss")).Return(currentDateTimeUTC).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HYEUAT999", "CAROTRANS", $"CSCBUE1515800|{currentDateTimeUTC}", "CSCBUE1515800|C000093833|CROI", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HYEUAT999", "CAROTRANS", $"CSCBUE1515800|{currentDateTimeUTC}", "CSCBUE1515800|C000093833|", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "ZAZAZA", "CAROTRANS", $"CSCBUE1515800|{currentDateTimeUTC}", "CSCBUE1515800|C000093833|CROI", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "ZAZAZA", "CAROTRANS", $"CSCBUE1515800|{currentDateTimeUTC}", "CSCBUE1515800|C000093833|", "IFTMBC")).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return(recipientID).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("CAROTRANS", "ZAZAZA")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<X12_301_2UShipment>(input, expectedBusinessLayerOutputFile);

      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var extensionObjects2 = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
      };
      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
      mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerOutputFile, expectedOutput);
    }
  }
}
