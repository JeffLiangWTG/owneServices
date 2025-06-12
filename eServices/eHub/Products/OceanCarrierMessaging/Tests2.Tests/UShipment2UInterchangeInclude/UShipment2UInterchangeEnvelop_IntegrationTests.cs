using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.BookingConfirmation;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UShipment2UInterchangeInclude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UShipment2UInterchangeInclude_IntegrationTests
  {
    const string filePath = "UShipment2UInterchangeInclude.IntegrationTestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2UInterchangeInclude_IntegrationTest()
    {
      AssertMapping("Test1_input.xml", "Test1_output_BusinessLayer.xml", "Test1_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", enableRegexMatcher: "true");
      AssertMapping("Test2_input.xml", "Test2_output_BusinessLayer.xml", "Test2_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "DIRECT", "C01329220", "C01329220", "Booking Confirmed", "AP", "CLD", "Shipping Order");
      AssertMapping("Test3_input.xml", "Test3_output_BusinessLayer.xml", "Test3_output_UInterchange.xml", "CARGOSMARKT", "1", "MRJ", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test4_input.xml", "Test4_output_BusinessLayer.xml", "Test4_output_UInterchange.xml", "CARGOSMARKT", "9", "MAA", "CGSMSG", "CSG00000000029", "C01329220", "Booking Confirmed", "CA", "XYZ");
      AssertMapping("Test5_input.xml", "Test5_output_BusinessLayer.xml", "Test5_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test6_input.xml", "Test6_output_BusinessLayer.xml", "Test6_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test7_input.xml", "Test7_output_BusinessLayer.xml", "Test7_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test8_input.xml", "Test8_output_BusinessLayer.xml", "Test8_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test9_input.xml", "Test9_output_BusinessLayer.xml", "Test9_output_UInterchange.xml", "CARGOSMARKT", "1", "MWA", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test10_input.xml", "Test10_output_BusinessLayer.xml", "Test10_output_UInterchange.xml", "CARGOSMARKT", "1", "MWA", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "CLD");
      AssertMapping("Test11_input.xml", "Test11_output_BusinessLayer.xml", "Test11_output_UInterchange.xml", "CARGOSMART", "6", "MAA", "CGSMSG", "YML0000003065", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
    }

    void AssertMapping(string inputFile, string businessLayerOutputFile, string expectedOutputFile, string senderID, string eventCode, string eventType, string msgIDSubscription, string c10601, string consolNo, string eventRef, string responseTypeCode, string shipmentType, string documentName = "Booking Request", string expectedUNB = "", string enableRegexMatcher = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var expectedBusinessLayerOutput = filePath + businessLayerOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      var recipientID = "ZAZAZA";
      var brsID = "CGSBRS";
      var actionPurpose = "APP";

      var msgID = msgIDSubscription == "DIRECT"
            ? brsID.Substring(0, 3) + "MSG"
            : msgIDSubscription;
      var isDirectValue = msgIDSubscription == "DIRECT" ? "true" : "false";
      var UNB5 = "1234567890";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirectValue);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "Regex Matcher", "EnableRegexMatcher", senderID, recipientID)).Return(enableRegexMatcher).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "OOLU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "YMLU")).Return("YMLU_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, expectedUNB)).Return(expectedUNB).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("1"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("23"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("8"), Arg<string>.Is.Equal("false"))).Return("Sea").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("8"), Arg<string>.Is.Equal("true"))).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("28"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
            Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID),
            Arg<string>.Is.Anything, Arg<string>.Is.Equal("83"), Arg<string>.Is.Equal("true"))).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601)).Return(consolNo).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "CS")).Return("CS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PX")).Return("PX").Repeat.Any();
      if (c10601 != consolNo)
      {
        if (c10601 == "YML0000003065")
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "FormVersion")).Return("3.0.0").Repeat.Any();
        }
        else
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "FormVersion")).Return("2.0.0").Repeat.Any();
        }
      }
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", consolNo, "@referenceType", "FormVersion")).Return("2.0.0").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|NADCA_SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|NADCA_SCAC_EMPTY", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{consolNo}|TDT_SCAC_EMPTY", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"BL091042970|{UNB5}", $"ANT1042970|{consolNo}|OOLU", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"YMLUN488033288|{UNB5}", $"N488033288|{consolNo}|YMLU", "IFTMBC")).Repeat.Any();

      var extensionObjects1 = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects1);
      mapTester.ExecuteCompiled<IFTMBC2UShipment_D99B>(input, expectedBusinessLayerOutput);

      var extensionObjects2 = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
      mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerOutput, expectedOutput);

    }
  }

  [TestClass]
  public class UShipment2UInterchangeEnvelop_ContainerTracking_IntegrationTests
  {
    const string filePath = "UShipment2UInterchangeInclude.IntegrationTestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2UInterchangeEnvelop_IntegrationTest()
    {
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output_BusinessLayer.xml", "CTS_Test1_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MAA", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request", enableRegexMatcher: "true");
      AssertMappingCTS("CTS_Test2_input.xml", "CTS_Test2_output_BusinessLayer.xml", "CTS_Test2_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MRJ", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request");
      AssertMappingCTS("CTS_Test3_input.xml", "CTS_Test3_output_BusinessLayer.xml", "CTS_Test3_output_UInterchange.xml", "", "", "6", "MRJ", "CGSMSG", "BLANK", "", "Booking Confirmed", "CA", "", "", "Booking Request", "");
      AssertMappingCTS("CTS_Test4_input.xml", "CTS_Test4_output_BusinessLayer.xml", "CTS_Test4_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MAA", "DIRECT", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order");
      AssertMappingCTS("CTS_Test2_input.xml", "CTS_Test2_output_BusinessLayer.xml", "CTS_Test2_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MRJ", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request");
      AssertMappingCTS("CTS_Test5_input.xml", "CTS_Test5_output_BusinessLayer.xml", "CTS_Test5_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MAA", "DIRECT", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order", "C00679603", "");
      AssertMappingCTS("CTS_Test6_input.xml", "CTS_Test6_output_BusinessLayer.xml", "CTS_Test6_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MAA", "DIRECT", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order", "C00679603", "");

      //Fallback InterchangeNum
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output_BusinessLayer.xml", "CTS_Test1_output_UInterchange.xml", "APP", "HYEDAUAAA", "6", "MAA", "CGSMSG", "CMA0000147298", "", "Booking Confirmed", "CA", "XYZ", "", "Booking Request", "C00679603", enableRegexMatcher: "true");
    }

    void AssertMappingCTS(string inputFile, string businessLayerOutputFile, string expectedOutputFile, string actionPurpose,
                          string recipientID, string eventCode, string eventType, string msgIDSubscription, string c10601, string consolNo,
                          string eventRef, string responseTypeCode, string shipmentType, string directValue, string documentName, string consolNoFallback = "", string expectedUNB2 = "UNB2", string enableRegexMatcher = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var expectedBusinessLayerOutput = filePath + businessLayerOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringHelper = MockRepository.GenerateStrictMock<StringHelper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      var senderID = "CARGOSMART";
      var brsID = "CGSBRS";

      var msgID = msgIDSubscription == "DIRECT"
            ? brsID.Substring(0, 3) + "MSG"
            : msgIDSubscription;
      var isDirectValue = msgIDSubscription == "DIRECT" ? "true" : "false";
      var UNB5 = "1234567890";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CONTAINER_TRACKING").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

      if (isDirectValue == "true")
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", senderID)).Return("DIRECTID").Repeat.Once();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "DIRECTID", "@value", directValue)).Return(recipientID).Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirectValue);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "Regex Matcher", "EnableRegexMatcher", senderID, recipientID)).Return(enableRegexMatcher).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "ANNU")).Return("ANNU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "CMDU")).Return("CMDU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "AAAA")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "BBBB")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB2")).Return(expectedUNB2).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCMIFTMBC"), Arg<string>.Is.Equal("OCM IFTMBC Configuration"),
	      Arg<string>.Is.Equal("TransportMode"), Arg<string>.Is.Equal("Output Code"), Arg<string>.Is.Equal(senderID), Arg<string>.Is.NotEqual(""),
	      Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", msgID, "@value", c10601)).Return(recipientID).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601)).Return(consolNo).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "IFTMBF")).Return(consolNoFallback).Repeat.Any();

      var subscribeConsolNo = consolNo != "" ? consolNo : consolNoFallback;

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", subscribeConsolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", subscribeConsolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brsID, "@value", "", "@referenceType", "ShipmentType")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", c10601, "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "NE")).Return("NE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PKG")).Return("PKG").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2017-05-26T11:12:51").Repeat.Once();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{subscribeConsolNo}|", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{subscribeConsolNo}|AAAA", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"WAYBILL NUMBER|{UNB5}", $"CARRIER BOOKING NUMBER|{subscribeConsolNo}|SCAC", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"TYO0252161|{UNB5}", $"TYO0252161|{subscribeConsolNo}|CMDU", "IFTMBC")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"ACD0368882|{UNB5}", $"ACD0368882|{subscribeConsolNo}|ANNU", "IFTMBC")).Repeat.Any();

      var extensionObjects1 = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects1);

      if (string.IsNullOrEmpty(consolNo))
      {
        var isThrownException = false;
        try
        {
          mapTester.ExecuteCompiled<IFTMBC2UShipment_D99B>(input, expectedBusinessLayerOutput);
        }
        catch (Exception ex)
        {
          isThrownException = true;
          var actualException = ex.InnerException ?? ex;
          if (actualException is ArgumentException)
          {
            Assert.AreEqual("Error: Unable to find Data Target.", actualException.Message.Trim());
          }
          else
          {
            throw;
          }

          Assert.IsTrue(isThrownException);
        }
      }
      else
      {
        mapTester.ExecuteCompiled<IFTMBC2UShipment_D99B>(input, expectedBusinessLayerOutput);

        var extensionObjects2 = new Dictionary<string, object>() {
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
          { "http://schemas.microsoft.com/BizTalk/2003/StringHelper", mockStringHelper }
        };

        var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects2);
        mapTester2.ExecuteCompiled<UShipment2UInterchangeInclude>(expectedBusinessLayerOutput, expectedOutput);
      }
    }
  }
}