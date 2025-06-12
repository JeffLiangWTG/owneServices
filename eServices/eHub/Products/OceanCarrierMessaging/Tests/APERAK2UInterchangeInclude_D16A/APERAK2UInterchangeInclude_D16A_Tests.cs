using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude_D16A;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class APERAK2UInterchangeInclude_D16A_Tests
  {
    const string filePath = "APERAK2UInterchangeInclude_D16A.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UInterchangeInclude_D16A()
    {
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test1_DTM137_AB_AOQ_input.xml", "Test1_DTM137_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "CSG00000015", "ABC", "Shipping Instruction", "C00001188", "Test2_DTM334_AP_AAQ_input.xml", "Test2_DTM334_AP_AAQ_output.xml");
      AssertMapping("PORTRIX", "PORMSG", "PORBRS", "C00001188_ABCD123456789", "", "Booking Request", "MAE00000001", "Test3_DTM182_RE_AOQ_AAQ_input.xml", "Test3_DTM182_RE_AOQ_AAQ_output.xml");
      AssertMapping("PORTRIX", "PORMSG", "PORBRS", "C00001188", "", "Shipping Instruction", "MAE00000001", "Test4_EventTime_FromUNB_input.xml", "Test4_EventTime_FromUNB_output.xml");
      AssertMapping("CARGOSMART", "CGSRMSG", "CGSBRS", "C00001188_ABCD123456789", "", "Verified Gross Container Weight", "MAE00000001", "Test5_DTM137_AB_AOQ_input.xml", "Test5_DTM137_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test6_input.xml", "Test6_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test7_input_FTX+AAI_SingleError.xml", "Test7_output_FTX+AAI_SingleError.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test8_input_FTX+AAI_MultipleError.xml", "Test8_output_FTX+AAI_MultipleError.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test9_input_FTX+ZZZ+++R_SingleError.xml", "Test9_output_FTX+ZZZ+++R_SingleError.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "C00001188_ABCD123456789", "CLD", "Verified Gross Container Weight", "C00001188", "Test10_input_FTX+ZZZ+++R_MultipleError.xml", "Test10_output_FTX+ZZZ+++R_MultipleError.xml");
    }

    private void AssertMapping(string senderID, string st_msg, string st_brs, string consolNumber_containerNumber, string shipmentType, string documentName, string bgmMessageReference, string inputFile, string expectedOutputFile)
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

      ExpectAssignment(senderID, mockContextAccessor, mockCodeMapper, mockOCMHelper);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgid).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(st_brs).Repeat.Once();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Once();

      var consolNumber = consolNumber_containerNumber.Contains("_")
              ? consolNumber_containerNumber.Substring(0, consolNumber_containerNumber.IndexOf('_'))
              : consolNumber_containerNumber;

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference, "@referenceType", "InterchangeNum")).Return("280").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference, "@referenceType", "IFTMBF")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference, "@referenceType", "VERMAS")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference, "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", bgmMessageReference)).Return(consolNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "280", "@referenceType", "ForwardingType")).Return(documentName).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", "280", "@referenceType", "DocumentName")).Return(documentName).Repeat.Once();

      var messageReference = st_msg == "DIRECT" ? bgmMessageReference : consolNumber;
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();
      if (string.IsNullOrEmpty(shipmentType))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "HYEDAULJ1", "@ST_ID", st_brs, "@value", messageReference, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Once();
      }

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
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude_D16A>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }

    private static void ExpectAssignment(string senderID, ContextAccessor mockContextAccessor, CodeMapper mockCodeMapper, OCMHelper mockOCMHelper)
    {
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDAULJ1").Repeat.AtLeastOnce();
      mockContextAccessor.Stub(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB+UNOC:3+INTTRA:ZZZ+CARGOWISE:ZZZ+160902:0020+170457");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "AB")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "AP")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMBF", "RE")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "ERROR DESCRIPTION")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "", "ACCEPTED")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "VERMAS", "AB")).Return("ACKNOWLEDGED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMIN", "AP")).Return("ACCEPTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, "IFTMBF", "RE")).Return("REJECTED").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
    }
  }
}
