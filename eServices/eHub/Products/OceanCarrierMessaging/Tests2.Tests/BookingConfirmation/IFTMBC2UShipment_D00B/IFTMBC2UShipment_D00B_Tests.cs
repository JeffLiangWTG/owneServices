using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class BookingConfirmation_IFTMBC2UniversalShipment_D00B_Tests
  {
    const string filePath = "BookingConfirmation.IFTMBC2UShipment_D00B.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTMBC2UniversalShipment_D00B()
    {
      recipientID = "ZAZAZA";
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "AP", documentName: "Shipping Instruction");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "AJ", documentName: "Shipping Order");
      AssertMapping("Test3_input.xml", "Test3_output_BusinessLayer.xml", "Test3_output_UInterchange.xml", "1", "XXX", "HAPMSG", "CEFS0000681229", "CW00000000461", "AJ", documentName: "Booking Request");
      AssertMapping("Test4_input.xml", "Test4_output_BusinessLayer.xml", "Test4_output_UInterchange.xml", "1", "XXX", "HAPMSG", "CEFS0000681229", "CEFS0000681229", "AJ", isDirect: "true");
      AssertMapping("Test5_input.xml", "Test5_output_BusinessLayer.xml", "Test5_output_UInterchange.xml", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "AJ", "");
      AssertMapping("Test6_input.xml", "Test6_output_BusinessLayer.xml", "Test6_output_UInterchange.xml", "1", "MRR", "HAPMSG", "CEFS0000681229", "CW00000000461", "AJ", "");
      AssertMapping("Test7_input.xml", "Test7_output_BusinessLayer.xml", "Test7_output_UInterchange.xml", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "AP");
      AssertMapping("Test8_input_TransportModes.xml", "Test8_output_BusinessLayer_TransportModes.xml", "Test8_output_UInterchange_TransportModes.xml", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "AP", formVersion: "3.0.0");
      AssertMapping("Test9_input_defaultValues.xml", "Test9_output_BusinessLayer_defaultValues.xml", "Test9_output_UInterchange_defaultValues.xml", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "AP");

      recipientID = "";
      AssertMapping("Test1_input.xml", "Test_Empty_output_BusinessLayer.xml", "Test_Empty_output_UInterchange.xml", "6", "MAA", "HAPMSG", "C00679603", "CSG00000000036", "AP");
    }

    string recipientID = string.Empty;
    void AssertMapping(string inputFile, string businessLayerFiler, string expectedFinalOutputFile,
                       string eventCode, string eventType, string msgID, string c10601,
                       string consolNo, string responseTypeCode, string expectedUNB2 = "UNB_SCAC",
                       string documentName= "", string formVersion = "2.0.0.0", string isDirect = "false")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFiler;
      var expectedFinalOutput = filePath + expectedFinalOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var actionPurpose = "APP";
      var senderID = "HAPAG_LLOYD";
      var brsID = "HAPBRS";

      var shipmentType = "XYZ";
      var eventRef = responseTypeCode == "AJ" ? "Booking Cancelled" : "Booking Confirmed";
      var UNB5 = "1234567890";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "CR")).Return("PKG").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB_SCAC")).Return(expectedUNB2).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "8", "true")).Return("Sea").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "8", "false")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "SCAC", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "TDT_SCAC", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "TDT_SCAC_EMPTY", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "UNB_SCAC", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "23", "true")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601)).Return(consolNo).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", consolNo)).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", c10601)).Return(actionPurpose).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "FormVersion")).Return(formVersion).Repeat.Once();
      if (c10601 != consolNo)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "FormVersion")).Return(formVersion).Repeat.Once();
      }
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2014 -07-04T05:43:59").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|NADCA_SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|TDT_SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|TDT_SCAC_EMPTY", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|UNB_SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, "BL091042970", "Booking Number", "IFTMBC")).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, recipientID)).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<IFTMBC2UShipment_D00B>(input, expectedBusinessLayerFile);

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

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTMBC2UniversalShipment_D00B_CTS()
    {
      recipientID = "HYEBNEUAT";
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output_BusinessLayer.xml", "CTS_Test1_output_UInterchange.xml", "9", "MAA", "C00679603");
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output_BusinessLayer.xml", "CTS_Test1_output_UInterchange.xml", "9", "MAA", "C00679603");
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output_BusinessLayer.xml", "CTS_Test1_output_UInterchange.xml", "9", "MAA", "C00679603", isDirect: "true");

      ////Should generate empty output
      //EventCode='XXX'
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_TestX_output_BusinessLayer_EmptyBody_Blank.xml", "CTS_TestX_output_UInterchange.xml", "9", "XXX", "C00679603");
      //"sCoLoad='TRUE'
      AssertMappingCTS("CTS_Test2_input.xml", "CTS_TestX_output_BusinessLayer_EmptyBody_Blank.xml", "CTS_TestX_output_UInterchange.xml", "9", "MAA", "C00679603", shipmentType: "CLD");
      //RFF+BN = blank and RFF+BM = blank
      AssertMappingCTS("CTS_Test3_input.xml", "CTS_Test3_output_BusinessLayer.xml", "CTS_TestX_output_UInterchange.xml", "9", "MAA", "C00679603");

      //ReceiverID=blank, should raise exception
      AssertMappingCTS("CTS_Test4_input.xml", "CTS_Test4_output_BusinessLayer.xml", "CTS_Test1_output_Exception.xml", "9", "MAA", "");
    }

    void AssertMappingCTS(string inputFile, string businessLayerFile, string expectedFinalOutputFile, string eventCode, string eventType, string consolNo, string documentName = "", string shipmentType = "XYZ", string isDirect = "false")
    {
      var input = filePath + inputFile;
      var expectedBusinessLayerFile = filePath + businessLayerFile;
      var expectedFinalOutput = filePath + expectedFinalOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var senderID = "HAPAG_LLOYD";
      var msgID = "HAPMSG";
      var brsID = "HAPBRS";
      var actionPurpose = "APP";

      var messageReferenceNumber = "CSG00000000029";
      var eventRef = "Booking Confirmed";
      var UNB5 = "1234567890";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CONTAINER_TRACKING").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB_SCAC").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, "")).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, "")).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "HLCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "CR")).Return("PKG").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
                                                    Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything,
                                                    Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", msgID, "@value", messageReferenceNumber)).Return("HYEBNEUAT").Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", messageReferenceNumber, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", messageReferenceNumber)).Return(consolNo).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", messageReferenceNumber, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", messageReferenceNumber, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", consolNo)).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", messageReferenceNumber, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", messageReferenceNumber)).Return(actionPurpose).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2014-07-04T05:43:59").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", "HYEBNEUAT", senderID, $"BL091042970|{UNB5}", $"ANT1042970|{consolNo}|HLCU", "IFTMBC")).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, "CONTAINER_TRACKING")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      try
      {
        var extensionObjects = new Dictionary<string, object>()
        {
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
          { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
        };

        MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.ExecuteCompiled<IFTMBC2UShipment_D00B>(input, expectedBusinessLayerFile);

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
      catch (Exception ex)
      {
        if (string.IsNullOrWhiteSpace(consolNo))
        {
          Assert.AreEqual("Error: Unable to find Data Target.", ex.Message);
        }
      }
    }
  }
}
