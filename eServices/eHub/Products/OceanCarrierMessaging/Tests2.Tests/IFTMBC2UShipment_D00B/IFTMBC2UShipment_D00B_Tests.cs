using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMBC2UShipment_D00B;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class IFTMBC2UniversalShipment_D00B_Tests
  {
    const string filePath = "IFTMBC2UShipment_D00B.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTMBC2UniversalShipment_D00B_CTS()
    {
      AssertMappingCTS("EventCode='MAA'", "CTS_Test1_input.xml", "CTS_Test1_output.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MAA", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
      AssertMappingCTS("EventCode='MRR'", "CTS_Test1_input.xml", "CTS_Test1_output.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MRR", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
      AssertMappingCTS("EventCode='MRR'", "CTS_Test1_input.xml", "CTS_Test1_output.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MRR", "DIRECT", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
      AssertMappingCTS("ReceiverID=blank, should raise exception", "CTS_Test1_input.xml", "CTS_Test1_output_Exception.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MAA", "HAPMSG", "CSG00000000029", "", "Booking Confirmed", "XYZ");

      //Should generate empty output
      AssertMappingCTS("EventCode='XXX'", "CTS_Test1_input.xml", "CTS_TestX_output_Blank.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "XXX", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
      AssertMappingCTS("isCoLoad='TRUE'", "CTS_Test2_input.xml", "CTS_TestX_output_Blank.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MAA", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "CLD");
      AssertMappingCTS("RFF+BN=blank", "CTS_Test3_input.xml", "CTS_TestX_output_Blank.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MAA", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
      AssertMappingCTS("RFF+BM=blank", "CTS_Test4_input.xml", "CTS_TestX_output_Blank.xml", "APP", "HAPAG_LLOYD", "CW1", "HAPBRS", "9", "MAA", "HAPMSG", "CSG00000000029", "C00679603", "Booking Confirmed", "XYZ");
    }

    void AssertMappingCTS(string testDescription, string inputFile, string expectedOutputFile, string actionPurpose, string senderID, string receiverID, string brsID, string eventCode, string eventType, string msgID, string messageReferenceNumber, string consolNo, string eventRef, string shipmentType, string documentName = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      var msgIDForDocumentNameSubscription = msgID == "DIRECT"
                ? brsID.Substring(0, 3) + "MSG"
                : msgID;
      var isDirectValue = msgID == "DIRECT" ? "true" : "false";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(receiverID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirectValue);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, "")).Return(eventType).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, "")).Return(eventRef).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "ZAZAZA", "@ST_ID", msgID, "@value", messageReferenceNumber)).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", receiverID, "@ST_ID", brsID, "@value", consolNo)).Return(actionPurpose).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "ZAZAZA", "@ST_ID", msgIDForDocumentNameSubscription, "@value", messageReferenceNumber, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", receiverID, "@ST_ID", brsID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      if (receiverID == "")
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", receiverID)).Repeat.Any();
      }

      mockDateMapper.Stub(x => x.CurrentDateTimeWithTimeZone()).Return("2014-07-04T05:43:59").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      try
      {
        mapTester.ExecuteCompiled<IFTMBC2UShipment_D00B>(input, expectedOutput);
      }
      catch (Exception ex)
      {
        if (string.IsNullOrWhiteSpace(receiverID))
        {
          Assert.AreEqual(String.Format("Cannot find subscription base on @senderId: {0}, @recipientId: {1}, @ST_ID: {2}, @value: {3}", senderID, "", msgID, messageReferenceNumber), ex.Message);
        }
      }
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTMBC2UniversalShipment_D00B()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", documentName: "Shipping Instruction");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ", documentName: "Shipping Order");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "1", "XXX", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test3_input.xml", "Test3_output_Direct.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "1", "XXX", "DIRECT", "CEFS0000681229", "CEFS0000681229", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ", "Booking Request", "");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ", "Booking Request", "");

      AssertMapping("Test1_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test2_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test3_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "1", "XXX", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test3_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "1", "XXX", "DIRECT", "CEFS0000681229", "CEFS0000681229", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test4_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ", "Booking Request", "");
      AssertMapping("Test5_input.xml", "Test_Empty_output.xml", "APP", "HAPAG_LLOYD", "", "HAPBRS", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ", "Booking Request", "");

      AssertMapping("Test6_input.xml", "Test6_output.xml", "APP", "HAPAG_LLOYD", "ZAZAZA", "HAPBRS", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string actionPurpose, string senderID, string recipientID, string brsID, string eventCode, string eventType, string msgIDSubscription, string c10601, string consolNo, string eventRef, string responseTypeCode, string shipmentType, string documentName = "", string expectedUNB2 = "UNB_SCAC")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      var msgID = msgIDSubscription == "DIRECT"
                ? brsID.Substring(0, 3) + "MSG"
                : msgIDSubscription;
      var isDirectValue = msgIDSubscription == "DIRECT" ? "true" : "false";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirectValue);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Any();

      //mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB_SCAC")).Return(expectedUNB2).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<IFTMBC2UShipment_D00B>(input, expectedOutput);
    }
  }
}
