using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class APERAK2UEvent_Tests
  {
    const string filePath = "APERAK2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UEvent()
    {
      AssertMapping("CARGOSMART", "Test1_input.xml", "Test1_output.xml", "CGSMSG", "CGSBRS", "CGSID", "CLD");
      AssertMapping("INTTRA", "Test2_input.xml", "Test2_output.xml", "DIRECT", "INTBRS", "INTID", "");
      AssertMapping("CARGOSMART", "Test3_input.xml", "Test3_output.xml", "CGSMSG", "CGSBRS", "CGSID", "ABC");
      AssertMapping("INTTRA", "Test4_input.xml", "Test4_output.xml", "DIRECT", "INTBRS", "INTID", "", "");
      AssertMapping("CARGOSMART", "Test5_input.xml", "Test5_output.xml", "CGSMSG", "CGSBRS", "CGSID", "CLD");
      AssertMapping("INTTRA", "Test6_input_Doc_MultipleErrors.xml", "Test6_output_Doc_MultipleErrors.xml", "DIRECT", "INTBRS", "INTID", "", "");
    }

    private void AssertMapping(string senderID, string inputFile, string expectedOutputFile, string st_msg, string st_brs, string st_id, string shipmentType, string expectedUNB = "UNB_SCAC")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      var msgid = (st_msg == "DIRECT")
        ? st_brs.Substring(0, 3) + "MSG"
        : st_msg;
      var isDirect = (st_msg == "DIRECT") ? "TRUE" : "FALSE";

      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);

      mockDateMapper.Stub(x => x.CurrentDateTime("s")).Return("2018-10-23T12:13:20.025");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("FALSE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "ABC")).Return("TRUE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "")).Return("FALSE");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("Mapped_NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "INTTRA", "UNB_SCAC")).Return(expectedUNB).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC_EMPTY")).Return("").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMBF", "ACCEPTED")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "IFTMIN", "A")).Return("MAA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, "VERMAS", "R")).Return("MRR");

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", senderID)).Return(st_id);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(st_msg);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", senderID)).Return(st_brs);
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect);

      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "CSG00000015", "@referenceType", "JobNumber")).Return("C03078216_ABCD12345678");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "CSG00000015", "@referenceType", "VERMAS")).Return("");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "X0000001", "@referenceType", "VERMAS")).Return("C03078216_ABCD12345678");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "X0000001", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "C03078216", "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_brs, "@value", "CSG00000015", "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "Original Subscriber", "@ST_ID", st_msg, "@value", "ConsolNumberC00001188", "@referenceType", "ShipmentType")).Return(shipmentType);

      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@value", st_msg == "DIRECT" ? "HYEDAUTST" : "ConsolNumberC00001188", "@ST_ID", st_msg == "DIRECT" ? st_id : st_msg)).Return("Original Subscriber");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@value", st_msg == "DIRECT" ? "HYEDAUTST" : "CSG00000015", "@ST_ID", st_msg == "DIRECT" ? st_id : st_msg)).Return("Original Subscriber");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@value", st_msg == "DIRECT" ? "HYEDAUTST" : "X0000001", "@ST_ID", st_msg == "DIRECT" ? st_id : st_msg)).Return("Original Subscriber");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://cargowise.com/Core/Transforms/Helper/ContextAccessor", mockContextAccessor },
        { "http://cargowise.com/Core/Transforms/Helper/CodeMapper", mockCodeMapper },
        { "http://cargowise.com/Core/Transforms/Helper/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
