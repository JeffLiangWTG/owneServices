using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class OCMBookingConfirmation2UShipment_Tests
  {
    const string filePath = "BookingConfirmation.OCM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestOCMBookingConfirmation2UShipment()
    {
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml", formVersion:"");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml");
      AssertMapping("Test3_input.xml", "Test3_output_BusinessLayer.xml", "Test3_output_UInterchange.xml", "CLD");
      AssertMapping("Test4_input.xml", "Test4_output_BusinessLayer.xml", "Test4_output_UInterchange.xml", formVersion:"3.0.0");
      AssertMapping("Test5_input_No_RecipientID.xml", "Test5_output_No_RecipientID_BusinessLayer.xml", "Test5_output_No_RecipientID_UInterchange.xml", formVersion: "3.0.0", recipientId: "");
    }

    void AssertMapping(string inputFile, string businessLayerFile, string expectedFinalOutputFile, string subscribeShipmentType = "AGT", string formVersion = "2.0.0", string recipientId = "CARGOWISE")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFile;
      var expectedFinalOutput = filePath + expectedFinalOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ODYSSEY").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientId).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "OCMBR")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "ODYSSEY")).Return("ODSMSG").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "DocumentName")).Return("Booking Request").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "ShipmentType")).Return(subscribeShipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ODYSSEY", "@recipientId", "", "@ST_ID", "ODSMSG", "@value", "ODY00000001", "@referenceType", "FormVersion")).Return(formVersion).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "RE")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "RE")).Return("Booking Rejected").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "CA")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "CA")).Return("Booking Conditionally Accepted").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventType", "ODYSSEY", "AP")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCargowise", "OCMCargowise", "OCM Cargowise System Configuration", "BKC Event Type", "EventReference", "ODYSSEY", "AP")).Return("Booking Confirmed").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("dd-MMM-yyyy HH:mm")).Return("03-Oct-2019 09:14");

      if (recipientId != "")
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "CARGOWISE", "ODYSSEY", "C00098678|100", "C00098678|C00001360|MEAU", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "CARGOWISE", "ODYSSEY", "C00098678|100", "C00098678|C00001360|XXXX", "IFTMBC")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "CARGOWISE", "ODYSSEY", "C00098678|100", "C00098678|C00001360|", "IFTMBC")).Repeat.Any();
      }

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return("").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("ODYSSEY", recipientId)).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<OCMBookingConfirmation2UShipment>(input, expectedBusinessLayerFile);

      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var extensionObjects2 = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
      mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerFile, expectedFinalOutput);
    }
  }
}