using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMBC2UShipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class IFTMBC2UniversalShipment_Tests
  {
    const string filePath = "IFTMBC2UniversalShipment.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void IFTMBC2UniversalShipment()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", documentName: "Shipping Instruction");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C01329220", "C01329220", "Booking Confirmed", "AP", "CLD", "Shipping Order", isDirect: "true");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "APP", "CARGOSMARKT", "ZAZAZA", "CGSBRS", "1", "MRJ", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "APP", "CARGOSMARKT", "ZAZAZA", "CGSBRS", "9", "MAA", "CGSMSG", "CSG00000000029", "C01329220", "Booking Confirmed", "CA", "XYZ");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "APP", "CARGOSMARKT", "ZAZAZA", "CGSBRS", "1", "MWA", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");

      AssertMapping("Test1_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test2_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C01329220", "C01329220", "Booking Confirmed", "AP", "CLD", "Shipping Order", isDirect:"true");
      AssertMapping("Test3_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMARKT", "", "CGSBRS", "1", "MRJ", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");
      AssertMapping("Test4_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMARKT", "", "CGSBRS", "9", "MAA", "CGSMSG", "CSG00000000029", "C01329220", "Booking Confirmed", "CA", "XYZ");
      AssertMapping("Test5_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test6_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test7_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
      AssertMapping("Test8_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMART", "", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ", "Booking Request", "UNB_SCAC");
      AssertMapping("Test9_input.xml", "Test_Empty_output.xml", "APP", "CARGOSMARKT", "", "CGSBRS", "1", "MWA", "CGSMSG", "CEFS0000681229", "CW00000000461", "Booking Cancelled", "AJ", "XYZ");

      AssertMapping("Test10_input.xml", "Test10_output.xml", "APP", "CARGOSMART", "ZAZAZA", "CGSBRS", "6", "MAA", "CGSMSG", "C00679603", "CSG00000000036", "Booking Confirmed", "AP", "XYZ");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string actionPurpose, string senderID, string recipientID,
                       string brsID, string eventCode, string eventType, string msgID, string c10601, string consolNo,
                       string eventRef, string responseTypeCode, string shipmentType, string documentName = "", string expectedUNB = "", string isDirect = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Once();

      //mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "OOLU")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, expectedUNB)).Return(expectedUNB).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601)).Return(consolNo).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ActionPurpose")).Return(actionPurpose).Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgID, "@value", c10601, "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", recipientID, "@ST_ID", msgID, "@value", consolNo, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<IFTMBC2UniversalShipment>(input, expectedOutput);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void IFTMBC2UniversalShipment_CTS()
    {
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MAA", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request");
      AssertMappingCTS("CTS_Test2_input.xml", "CTS_Test2_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MRJ", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request");
      AssertMappingCTS("CTS_Test3_input.xml", "CTS_Test3_output.xml", "", "CARGOSMART", "", "CGSBRS", "6", "MRJ", "CGSMSG", "BLANK", "", "Booking Confirmed", "CA", "", "", "Booking Request", "");
      AssertMappingCTS("CTS_Test4_input.xml", "CTS_Test4_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MAA", "CGSMSG", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order", isDirect:"true");
      AssertMappingCTS("CTS_Test2_input.xml", "CTS_Test2_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MRJ", "CGSMSG", "CMA0000147298", "C00679603", "Booking Confirmed", "CA", "XYZ", "", "Booking Request");
      AssertMappingCTS("CTS_Test5_input.xml", "CTS_Test5_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MAA", "CGSMSG", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order", "C00679603", "", isDirect: "true");
      AssertMappingCTS("CTS_Test6_input.xml", "CTS_Test6_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MAA", "CGSMSG", "C01329220", "C01329220", "Booking Confirmed", "AP", "XYZ", "CGWS", "Shipping Order", "C00679603", "", isDirect: "true");

      //Fallback InterchangeNum
      AssertMappingCTS("CTS_Test1_input.xml", "CTS_Test1_output.xml", "APP", "CARGOSMART", "HYEDAUAAA", "CGSBRS", "6", "MAA", "CGSMSG", "CMA0000147298", "", "Booking Confirmed", "CA", "XYZ", "", "Booking Request", "C00679603");
    }

    void AssertMappingCTS(string inputFile, string expectedOutputFile, string actionPurpose, string senderID,
                          string recipientID, string brsID, string eventCode, string eventType, string msgID, string c10601, string consolNo,
                          string eventRef, string responseTypeCode, string shipmentType, string directValue, string documentName, 
                          string consolNoFallback = "", string expectedUNB2 = "UNB2", string isDirect = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      //var msgID = msgIDSubscription == "DIRECT"
      //      ? brsID.Substring(0, 3) + "MSG"
      //      : msgIDSubscription;

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CONTAINER_TRACKING").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+").Repeat.Once();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

      if (isDirect == "true")
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", senderID)).Return("DIRECTID").Repeat.Once();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "DIRECTID", "@value", directValue)).Return(recipientID).Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brsID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "EventType", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBC", "OCMIFTMBC", "OCM IFTMBC Configuration", "EventType", "Reference", senderID, eventCode, actionPurpose, responseTypeCode)).Return(eventRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "ANNU")).Return("ANNU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "CMDU")).Return("CMDU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "AAAA")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "BBBB")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "SCAC")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB2")).Return(expectedUNB2).Repeat.Any();

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

      mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2017-05-26T11:12:51").Repeat.Once();

      mockOCMHelper.Expect(x => x.IsCoLoad("XYZ")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<IFTMBC2UniversalShipment>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
