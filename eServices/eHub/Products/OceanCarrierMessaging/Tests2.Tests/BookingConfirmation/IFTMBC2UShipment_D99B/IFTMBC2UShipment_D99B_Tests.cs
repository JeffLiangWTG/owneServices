using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class IFTMBC2UShipment_D99B_Tests
  {
    const string filePath = "BookingConfirmation.IFTMBC2UShipment_D99B.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTMBC2UShipment_D99B()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", documentName: "Shipping Instruction", formVersion: "2.0.0", enableRegexMatcher: "true");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", documentName: "Shipping Order", enableRegexMatcher: "true");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", documentName: "Booking Request", enableRegexMatcher: "true");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MWA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Withdrawn", "AP", "CLD", enableRegexMatcher: "true");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test6_input_TransportModes.xml", "Test6_output_TransportModes.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", formVersion: "3.0.0");
      AssertMapping("Test7_input_TransportModes.xml", "Test7_output_TransportModes.xml", "APP", "MAERSK", "ZAZAZA", "CGSBRS", "6", "MAA", "MAEMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", formVersion: "3.0.0");
      AssertMapping("Test8_input_defaultValues.xml", "Test8_output_defaultValues.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", formVersion: "2.0.0");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string actionPurpose, string senderID, string recipientID,
                       string brsID, string eventCode, string eventType, string msgID, string consolNo, string c10601,
                       string eventRef, string responseTypeCode, string shipmentType, string documentName = "", string formVersion = "", string isDirect = "false", string enableRegexMatcher = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      string expectedUNB = "";
      var UNB5 = "1234567890";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "Regex Matcher", "EnableRegexMatcher", senderID, recipientID)).Return(enableRegexMatcher).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "CT")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PK")).Return("PKG").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "Rail_SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, expectedUNB)).Return(expectedUNB).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "MAEU")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "1", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "2", "true")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "8", "true")).Return("InlandWaterway").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "NADCA_SCAC", "8", "false")).Return("Sea").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "MAEU", "8", "true")).Return("Sea").Repeat.Any();

      //mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "SCAC", "8", "true")).Return("InlandWaterway").Repeat.Any();
      //mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "TransportMode", "Output Code", senderID, "SCAC", "8", "false")).Return("Sea").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601)).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "ForwardingType")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "FormVersion")).Return(formVersion).Repeat.Once();
      if (string.IsNullOrEmpty(formVersion))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "FormVersion")).Return("").Repeat.Once();
      }
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID,  $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|NADCA_SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID,  $"221083218|{UNB5}", $"221083218|{consolNo}|MAEU", "IFTMBC")).Repeat.Any();

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
      mapTester.ExecuteCompiled<IFTMBC2UShipment_D99B>(input, expectedOutput);
    }
  }
}
