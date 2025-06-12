using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMCS2UniversalInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class IFTMCS2UniversalInterchange_Tests
  {
    const string filePath = "IFTMCS2UniversalInterchange.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void IFTMCS2UniversalInterchange()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "CARGOSMART", "CGSMSG", "CGS000000100001", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "CARGOSMART", "CGSMSG", "CGS000000100001", SubscriberConsolReferenceType.JobNumber, "MWA", "Bill of Lading Withdrawn");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "CARGOSMART", "CGSMSG", "CGS000000100001", SubscriberConsolReferenceType.JobNumber, "MWA", "Bill of Lading Withdrawn");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "CARGOSMART", "CGSMSG", "CGS000000100001", SubscriberConsolReferenceType.JobNumber, "MWA", "Bill of Lading Withdrawn", "N", "");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "CARGOSMART", "CGSMSG", "CGS000000100001", SubscriberConsolReferenceType.JobNumber, "MWA", "Bill of Lading Withdrawn", "N", "", shipmentType: "CLD");

      AssertMapping("Test6_input_Direct_RFF_FF.xml", "Test6_output.xml", "CARGOSMART", "CGSMSG", "C00001005", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", rffValue: "FF");
      AssertMapping("Test7_input_Direct_RFF_SI.xml", "Test7_output.xml", "CARGOSMART", "CGSMSG", "C00001005", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", rffValue: "SI");
      AssertMapping("Test8_input_CTS.xml", "Test8_output_CTS.xml", "CARGOSMART", "CGSMSG", "C00001005", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", isCTS: true);

      AssertMapping("Test9_input.xml", "Test9_output.xml", "CARGOSMART", "CGSMSG", "C02491403", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", rffValue: "SI");

      AssertMapping("Test10_input.xml", "Test10_output.xml", "CARGOSMART", "CGSMSG", "C02491403", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", rffValue: "SI", fallbackSubscriber: true);
      AssertMapping("Test11_input.xml", "Test11_output.xml", "CARGOSMART", "CGSMSG", "C02491403", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "N", rffValue: "SI", fallbackSubscriber: true);
      AssertMapping("Test12_input_Exception.xml", "Test12_output_Exception.xml", "CARGOSMART", "CGSMSG", "C02491403", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "N", rffValue: "SI", fallbackSubscriber: true, throwException: true);

      AssertMapping("INTTRA.Test1_input.xml", "INTTRA.Test1_output.xml", "INTTRA", "INTMSG", "C00001005", SubscriberConsolReferenceType.IFTMIN, "MAA", "Bill of Lading Confirmed", "Y");
      AssertMapping("INTTRA.Test2_input.xml", "INTTRA.Test2_output.xml", "INTTRA", "INTMSG", "C00001005", SubscriberConsolReferenceType.IFTMIN, "MWA", "Bill of Lading Withdrawn", "Y");
      AssertMapping("INTTRA.Test3_input.xml", "INTTRA.Test3_output.xml", "INTTRA", "INTMSG", "C00001005", SubscriberConsolReferenceType.IFTMIN, "MWA", "Bill of Lading Withdrawn", "Y");

      AssertMapping("GTNEXUS.Test1_input.xml", "GTNEXUS.Test1_output.xml", "GTNEXUS", "GTNMSG", "GTN000000100001", SubscriberConsolReferenceType.Default, "MAA", "Bill of Lading Confirmed");
      AssertMapping("GTNEXUS.Test2_input.xml", "GTNEXUS.Test2_output.xml", "GTNEXUS", "GTNMSG", "C00001428SID", SubscriberConsolReferenceType.Default, "MWA", "Bill of Lading Withdrawn", "Y");
      AssertMapping("GTNEXUS.Test3_input.xml", "GTNEXUS.Test3_output.xml", "GTNEXUS", "GTNMSG", "C00001428SID", SubscriberConsolReferenceType.Default, "MWA", "Bill of Lading Withdrawn", "Y");

      AssertMapping("MAERSK.Test1_input_Direct_RFF_SI.xml", "MAERSK.Test1_output.xml", "MAERSK", "MAEMSG", "C00001005", SubscriberConsolReferenceType.JobNumber, "MAA", "Bill of Lading Confirmed", "Y", rffValue: "SI");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderID, string ST_ID, string messageReferenceRFFSI, SubscriberConsolReferenceType subscriberConsolReferenceType,
                       string eventCode, string eventRef, string isDirectValue = "N", string expectedUNB2 = "UNB_SCAC", string rffValue = "SI", string rffFallbackValue = "ZZZ", string shipmentType = "AGT", bool isCTS = false, bool fallbackSubscriber = false, bool throwException = false)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var isDirect = isDirectValue == "Y" ? "true" : "false";

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var brs_id = ST_ID.Substring(0, 3) + "BRS";
      var recipientID = isCTS ? "CONTAINER_TRACKING" : "HYEUATBNE";
      var UNB5 = "1234567980";

      if (isCTS)
      {
        mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, recipientID)).Return("").Repeat.Any();
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();
      }
      else
      {
        mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, recipientID)).Return("OCM_BookingEngine").Repeat.Any();
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();
      }

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.SelectSubscriptionsByValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return($"UNB++++161121:0802+{UNB5}:abc:123'#####");
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB2).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMCS"), Arg<string>.Is.Equal("OCMIFTMCS"), Arg<string>.Is.Equal("OCM IFTMCS Configuration"),
                                                    Arg<string>.Is.Equal("EventType"), Arg<string>.Is.Equal("EventType"), Arg<string>.Is.Equal(senderID), Arg<string>.Is.Anything)).Return(eventCode);
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Equal("OCMIFTMCS"), Arg<string>.Is.Equal("OCMIFTMCS"), Arg<string>.Is.Equal("OCM IFTMCS Configuration"),
                                                    Arg<string>.Is.Equal("EventType"), Arg<string>.Is.Equal("EventReference"), Arg<string>.Is.Equal(senderID), Arg<string>.Is.Anything)).Return(eventRef);

      if (isDirect == "true")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", ST_ID, "@value", messageReferenceRFFSI)).Return(fallbackSubscriber ? "" : recipientID).Repeat.Any();
      }

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("", senderID, false)).Return("").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("825595", senderID, false)).Return("").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("11300121", senderID, false)).Return(throwException ? "" : recipientID).Repeat.Any();

      if (fallbackSubscriber)
      {
        mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI)).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "", "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "", "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "")).Return("").Repeat.Any();
      }
      else
      {
        mockSubscriptionHelper.Expect(x => x.GetSubscriber()).Return(throwException ? "" : recipientID).Repeat.Any();

        if (subscriberConsolReferenceType == SubscriberConsolReferenceType.JobNumber)
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "JobNumber")).Return("C00001005").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI)).Return("").Repeat.Any();
        }
        else if (subscriberConsolReferenceType == SubscriberConsolReferenceType.IFTMIN)
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "JobNumber")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "IFTMIN")).Return("C00001005").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI)).Return("").Repeat.Any();
        }
        else
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "JobNumber")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI)).Return("C00001005").Repeat.Any();
        }

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", messageReferenceRFFSI, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();

        if (messageReferenceRFFSI != "C00001005")
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "C00001005", "@referenceType", "JobNumber")).Return("C00001005").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "C00001005", "@referenceType", "IFTMIN")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "C00001005", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", ST_ID, "@value", "C00001005")).Return("").Repeat.Any();
        }
      }

      if (string.IsNullOrEmpty(shipmentType))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", brs_id, "@value", "C00001005", "@referenceType", "ShipmentType")).Return("AGT").Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(ST_ID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(brs_id);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", senderID)).Return(ST_ID.Substring(0, 3));
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect);
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMCS", "OCMIFTMCS", "OCM IFTMCS Configuration", "Reference No", "Value", senderID)).Return(rffValue); ;
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMCS", "OCMIFTMCS", "OCM IFTMCS Configuration", "Reference No", "Fallback Value", senderID)).Return(rffFallbackValue);

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDTSCAC")).Return("TDTSCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB_SCAC")).Return("UNB_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC_EMPTY")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "3333CarrierCode")).Return("3333CarrierCode").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "111111SCAC")).Return("111111SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "MAEU")).Return("MAEU").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|NADCA_SCAC", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|TDTSCAC", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|UNB_SCAC", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|NADCA_SCAC_EMPTY", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|TDT_SCAC_EMPTY", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"KKLUTW0350519|{UNB5}", "NUMNUMNUM|C00001005|111111SCAC", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"221361900|{UNB5}",     "221361900|C02491403|MAEU", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"221361900|{UNB5}",     "221361900||MAEU", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"11111MBOLNumber|{UNB5}", "22222CarrierBKRef|C00001005|3333CarrierCode", "IFTMCS")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("OCMCNF", recipientID, senderID, $"11111MBOLNumber|{UNB5}", "22222CarrierBKRef|C00001428SID|3333CarrierCode", "IFTMCS")).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      if (throwException)
      {
        mapTester.ExecuteAssertException<IFTMCS2UniversalInterchange>(input, "Unable to resolve recipient Id, message rejected.");
      }
      else
      {
        mapTester.Execute<IFTMCS2UniversalInterchange>(input, expectedOutput);
        mockCodeMapper.VerifyAllExpectations();
      }
    }

    enum SubscriberConsolReferenceType
    {
      JobNumber = 1,
      IFTMIN = 2,
      Default = 3
    }
  }
}
