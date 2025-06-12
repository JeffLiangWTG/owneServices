using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude_D04A;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class APERAK2UInterchangeInclude_D04A_Tests
  {
    const string filePath = "APERAK2UInterchangeInclude_D04A.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UInterchangeInclude_D04A()
    {
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "INTID", "CSG00000015_ABCD123456789", "CLD", "Test1_DTM137_AB_AOQ_input.xml", "Test1_DTM137_AB_AOQ_output.xml");
      AssertMapping("INTTRA", "DIRECT", "INTBRS", "INTID", "CSG00000015", "ABC", "Test2_DTM334_AP_AAQ_input.xml", "Test2_DTM334_AP_AAQ_output.xml");
      AssertMapping("PORTRIX", "PORMSG", "PORBRS", "PORID", "CSG00000015_ABCD123456789", "", "Test3_DTM182_RE_AOQ_AAQ_input.xml", "Test3_DTM182_RE_AOQ_AAQ_output.xml");
      AssertMapping("CARGOSMART", "CGSRMSG", "CGSBRS", "CGSID", "CSG00000015_ABCD123456789", "", "Test5_DTM137_AB_AOQ_input.xml", "Test5_DTM137_AB_AOQ_output.xml");
      AssertMapping("CARGOSMART", "CGSRMSG", "CGSBRS", "CGSID", "CSG00000015_ABCD123456789", "", "Test6_input.xml", "Test6_output.xml", "X0000001");
    }

    private void AssertMapping(string senderID, string st_msg, string st_brs, string st_id, string consolNumber_containerNumber, string shipmentType, string inputFile, string expectedOutputFile, string c10601 = "C03078216")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      string expectedUNB = "UNB_SCAC";
      var msgid = (st_msg == "DIRECT")
          ? st_brs.Substring(0, 3) + "MSG"
          : st_msg;
      var isDirect = (st_msg == "DIRECT") ? "TRUE" : "FALSE";

      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);

      mockDateMapper.Stub(x => x.CurrentDateTime("s")).Return("2018-10-23T12:13:20.025");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", senderID)).Return(st_id);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(st_msg);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(st_brs);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "ABC")).Return("FALSE").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "")).Return("FALSE").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "AB")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "AP")).Return("MRR");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMBF", "RE")).Return("IRJ");

      if (st_msg != "DIRECT")
      {
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "X0000001", "@referenceType", "VERMAS")).Return(consolNumber_containerNumber);
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", c10601, "@referenceType", "VERMAS")).Return("");
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", c10601, "@referenceType", "JobNumber")).Return(consolNumber_containerNumber);
      }
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", c10601, "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_brs, "@value", c10601, "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@value", st_msg == "DIRECT" ? "CGWGEODISAUBNE" : c10601, "@ST_ID", st_msg == "DIRECT" ? st_id : st_msg)).Return("Original Subscriber");

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "MAEU")).Return("Mapped_NADCA_SCAC").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://cargowise.com/Core/Transforms/Helper/ContextAccessor", mockContextAccessor },
        { "http://cargowise.com/Core/Transforms/Helper/CodeMapper", mockCodeMapper },
        { "http://cargowise.com/Core/Transforms/Helper/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude_D04A>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
