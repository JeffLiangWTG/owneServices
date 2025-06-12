using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class APERAK2UEvent_Tests
  {
    const string filePath = "APERAK2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UEvent()
    {
      AssertMapping("CARGOSMART", "Test1_input.xml", "Test1_output.xml", "UNB++++999999:9999+", "CGSMSG", "CGSBRS", "CSG00000015", "CLD", "ConsolNumberC00001188_ABCD12345678", "Shipping Instruction");
      AssertMapping("INTTRA", "Test2_input.xml", "Test2_output.xml", "UNB++++20130303:0030+", "DIRECT", "INTBRS", "CSG00000015", "", "ConsolNumberC00001188", "Booking Request");
      AssertMapping("CARGOSMART", "Test3_input.xml", "Test3_output.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test3_input.xml", "Test3_output.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "");
      AssertMapping("MAERSK", "Test4_input.xml", "Test4_output.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "", "");
      AssertMapping("CARGOSMART", "Test5_input.xml", "Test5_output.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("INTTRA", "Test6_input_DOC_IRA.xml", "Test6_output_DOC_IRA.xml", "UNB++++20130303:0030+", "DIRECT", "INTBRS", "CSG00000015", "", "ConsolNumberC00001188_ABCD12345678", "Booking Request");
      AssertMapping("CARGOSMART", "Test7_input_MultipleErrors.xml", "Test7_output_MultipleErrors.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test8_input_SingleError.xml", "Test8_output_SingleError.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test9_input_FTX+AAI_SingleError.xml", "Test9_output_FTX+AAI_SingleError.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test10_input_FTX+AAI_MultipleErrors.xml", "Test10_output_FTX+AAI_MultipleErrors.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test11_input_FTX+ZZZ+++R_SingleError.xml", "Test11_output_FTX+ZZZ+++R_SingleError.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
      AssertMapping("CARGOSMART", "Test12_input_FTX+ZZZ+++R_MultipleErrors.xml", "Test12_output_FTX+ZZZ+++R_MultipleErrors.xml", "UNB++++161211:+", "CGSMSG", "CGSBRS", "CSG00000015", "ABC", "ConsolNumberC00001188_ABCD12345678", "Verified Gross Container Weight");
    }

    private void AssertMapping(string senderID, string inputFile, string expectedOutputFile, string unbSegment, string st_msg, string st_brs, string bgmReferenceNumber, string shipmentType, string consolNumber, string documentName, string expectedUNB2 = "UNB2")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var msgid = (st_msg == "DIRECT")
        ? st_brs.Substring(0, 3) + "MSG"
        : st_msg;
      var isDirect = (st_msg == "DIRECT") ? "TRUE" : "FALSE";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDAULJ1").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(unbSegment).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMBF", "ACCEPTED")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "A")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "REJECTED")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "R")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "R")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "E")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "AP")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "")).Return("");

      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMBF", "ACCEPTED")).Return("ACCEPTED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMIN", "REJECTED")).Return("REJECTED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMIN", "A")).Return("ACCEPTED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "", "")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "VERMAS", "R")).Return("REJECTED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMIN", "R")).Return("REJECTED");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "", "E")).Return("ACCEPTED IN INTERCHANGE LEVEL WITH ERRORS/WARNING");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "", "AP")).Return("ACCEPTED");

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgid).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(st_brs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "MAEU")).Return("MAEU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "AAAA")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB2")).Return(expectedUNB2).Repeat.Any();

      if (st_msg != "DIRECT")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber, "@referenceType", "IFTMBF")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber, "@referenceType", "VERMAS")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "ConsolNumberC00001188", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", st_brs, "@value", "ConsolNumberC00001188", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", st_brs, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "ConsolNumberC00001188", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", st_brs, "@value", "ConsolNumberC00001188", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber, "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber)).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmReferenceNumber, "@referenceType", "InterchangeNum")).Return("288").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "288", "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "CSG00000015", "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, "HYEDAULJ1")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
