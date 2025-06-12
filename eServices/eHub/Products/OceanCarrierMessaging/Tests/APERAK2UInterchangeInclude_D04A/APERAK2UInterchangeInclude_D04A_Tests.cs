using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude_D04A;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class APERAK2UInterchangeInclude_D04A_Tests
  {
    const string filePath = "APERAK2UInterchangeInclude_D04A.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UInterchangeInclude_D04A()
    {
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "CSG00000015_ABCD123456789", "CLD", "Test1_DTM137_AB_AOQ_input.xml", "Test1_DTM137_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "CSG00000015", "ABC", "Test2_DTM334_AP_AAQ_input.xml", "Test2_DTM334_AP_AAQ_output.xml");
      AssertMapping("PORTRIX", "PORMSG", "PORBRS", "CSG00000015_ABCD123456789", "", "Test3_DTM182_RE_AOQ_AAQ_input.xml", "Test3_DTM182_RE_AOQ_AAQ_output.xml", "");
      AssertMapping("PORTRIX", "PORMSG", "PORBRS", "CSG00000015", "", "Test4_EventTime_FromUNB_input.xml", "Test4_EventTime_FromUNB_output.xml");
      AssertMapping("CARGOSMART", "CGSRMSG", "CGSBRS", "CSG00000015_ABCD123456789", "", "Test5_DTM137_AB_AOQ_input.xml", "Test5_DTM137_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "CSG00000015_ABCD123456789", "CLD", "Test6_DTM9_AB_AOQ_input.xml", "Test6_DTM9_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "CSG00000015_ABCD123456789", "CLD", "Test6_DTM9_AB_AOQ_input.xml", "Test6_DTM9_AB_AOQ_output.xml");

      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000015_ABCD123456789", "CLD", "Test7_input.xml", "Test7_output.xml");
      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000015_ABCD123456789", "CLD", "Test8_input.xml", "Test8_output.xml");
      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000016_ABCD123456789", "CLD", "Test9_input.xml", "Test9_output.xml");

      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000016_ABCD123456789", "CLD", "Test10_input_FTX+AAI_SingleError.xml", "Test10_output_FTX+AAI_SingleError.xml");
      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000016_ABCD123456789", "CLD", "Test11_input_FTX+AAI_MultipleError.xml", "Test11_output_FTX+AAI_MultipleError.xml");

      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000016_ABCD123456789", "CLD", "Test12_input_FTX+ZZZ+++R_SingleError.xml", "Test12_output_FTX+ZZZ+++R_SingleError.xml");
      AssertMapping("YANGMING", "YMLMSG", "YMLBRS", "CSG00000016_ABCD123456789", "CLD", "Test13_input_FTX+ZZZ+++R_MultipleError.xml", "Test13_output_FTX+ZZZ+++R_MultipleError.xml");
    }

    private void AssertMapping(string senderID, string st_msg, string st_brs, string consolNumber_containerNumber, string shipmentType, string inputFile, string expectedOutputFile, string expectedUNB2 = "UNB2")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var msgId = st_msg != "DIRECT" ? st_msg : (st_brs.Substring(0, 3) + "MSG");
      var isDirect = (st_msg == "DIRECT") ? "TRUE" : "FALSE";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      ExpectAssignment(senderID, mockContextAccessor, mockCodeMapper, mockOCMHelper, expectedUNB2);

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgId).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(st_brs).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();

      if (st_msg != "DIRECT")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "C03078216", "@referenceType", "VERMAS")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "CSG00000016", "@referenceType", "VERMAS")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "C03078216", "@referenceType", "JobNumber")).Return(consolNumber_containerNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "CSG00000016", "@referenceType", "JobNumber")).Return(consolNumber_containerNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "X00000006", "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "X00000006", "@referenceType", "VERMAS")).Return(consolNumber_containerNumber).Repeat.Any();
      }
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "X00000006", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "CSG00000016", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "C03078216", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", st_brs, "@value", "C03078216", "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "X00000006", "@referenceType", "DocumentName")).Return("Verified Gross Container Weight").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "C03078216", "@referenceType", "DocumentName")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgId, "@value", "CSG00000016", "@referenceType", "DocumentName")).Return("Booking Request").Repeat.Any();

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
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude_D04A>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }

    private static void ExpectAssignment(string senderID, ContextAccessor mockContextAccessor, CodeMapper mockCodeMapper, OCMHelper mockOCMHelper, string expectedUNB2)
    {
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDAULJ1").Repeat.AtLeastOnce();
      mockContextAccessor.Stub(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB+UNOC:3+INTTRA:ZZZ+CARGOWISE:ZZZ+160902:0020+170457");
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "AB")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "AP")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMBF", "RE")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "MP")).Return("MPP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "ERROR DESCRIPTION")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "ACCEPTED")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "MP")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "VERMAS", "AB")).Return("ACKNOWLEDGED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMIN", "AP")).Return("ACCEPTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMBF", "RE")).Return("REJECTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "VERMAS", "MP")).Return("REJECTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "", "MP")).Return("REJECTED").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "MAEU")).Return("MAEU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "AAAA")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "UNB2")).Return(expectedUNB2).Repeat.Any();
    }
  }
}
