using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Transforms.APERAK2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Tests
{
  [TestClass]
  public class APERAK2UEventTests
  {
    const string filePath = "APERAK2UEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UEvent()
    {
      AssertMapping("Test_OrderCancelled_input.xml", "Test_OrderCancelled_output.xml");
      AssertMapping("Test_OrderConfirmed_input.xml", "Test_OrderConfirmed_output.xml");
      AssertMapping("Test_OrderSendToSAP_input.xml", "Test_OrderSendToSAP_output.xml");
      AssertMapping("Test_Others_input.xml", "Test_Others_output.xml");
      AssertMapping("Test_Penalty_input.xml", "Test_Penalty_output.xml");
      AssertMapping("Test_Processing_input.xml", "Test_Processing_output.xml");
      AssertMapping("Test_RevenueAdvised_input.xml", "Test_RevenueAdvised_output.xml");
      AssertMapping("Test_Quotation_input.xml", "Test_Quotation_output.xml");
      AssertMapping("Test_OrderWithResponseCode0100013_input.xml", "Test_OrderWithResponseCode0100013_output.xml");
      AssertMapping("Test_OrderWithResponseCode0200043_input.xml", "Test_OrderWithResponseCode0200043_output.xml");
      AssertMapping("Test_OrderWithResponseCode0200044_input.xml", "Test_OrderWithResponseCode0200044_output.xml");
      AssertMapping("Test_OrderWithReasonLengthGreaterThan1024_input.xml", "Test_OrderWithReasonLengthGreaterThan1024_output.xml");
      AssertMapping("Test_ValidEventTypes_input.xml", "Test_ValidEventTypes_output.xml");
      AssertMapping("Test_OneValidEventType_input.xml", "Test_OneValidEventType_output.xml");
      AssertMapping("Test_NoValidEventTypes_input.xml", "Test_NoValidEventTypes_output.xml");
      AssertMapping("Test_LineItemLevelCharges_input.xml", "Test_LineItemLevelCharges_output.xml");
      AssertMapping("Test_LineItemLevelCharges_no-type_input.xml", "Test_LineItemLevelCharges_no-type_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockDateMapper.Stub(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ssK"))).Return("2016-09-02T00:20:00Z");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPADISCHARGECOASTWISE1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPADISCHARGECOASTWISE1", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPADISCHARGECOASTWISE1", "@referenceType", "DocumentName")).Return("Cargo Dues - Discharge Coastwise").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPADISCHARGECOASTWISE1", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "QIMPORT", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAEXPORT1", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAIMPORT1", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPALOADCOASTWISE1", "@referenceType", "JobNumber")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "QIMPORT", "@referenceType", "DocumentName")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAEXPORT1", "@referenceType", "DocumentName")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAIMPORT1", "@referenceType", "DocumentName")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPALOADCOASTWISE1", "@referenceType", "DocumentName")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "QIMPORT", "@referenceType", "ForwardingType")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAEXPORT1", "@referenceType", "ForwardingType")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAIMPORT1", "@referenceType", "ForwardingType")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPALOADCOASTWISE1", "@referenceType", "ForwardingType")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "QIMPORT")).Return("C03078216_QIMPORT").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAEXPORT1")).Return("C03078216_EXPORT").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPAIMPORT1")).Return("C03078216_IMPORT").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "", "@ST_ID", "NPAMSG", "@value", "TNPALOADCOASTWISE1")).Return("C03078216_LOAD").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "CDO_CANCELED")).Return("MWA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "CDO_CANCELED")).Return("Order Cancelled").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "CDO_ORDER_CONFIRMATION_ORDERNO")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "CDO_ORDER_CONFIRMATION_ORDERNO")).Return("Order Confirmed").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "CDO_SENT_TO_SAP")).Return("MPP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "CDO_SENT_TO_SAP")).Return("Sent to SAP for Processing").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "INVALID_PORT_OF_SERVICE")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "INVALID_PORT_OF_SERVICE")).Return("Port of service is Invalid").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "PENALTY")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "PENALTY")).Return("Order Confirmed - penalties may be appplied for late submission").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "CARGO_DUES_ORDER_ACCEPTED_FOR_FURTHER_PROCESSING")).Return("MRR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "CARGO_DUES_ORDER_ACCEPTED_FOR_FURTHER_PROCESSING")).Return("Accepted for further Processing").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "REVENUE_ADVISE")).Return("STU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "REVENUE_ADVISE")).Return("Revenue Advised").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "000000")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "000000")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0010001")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0010001")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0010011")).Return("MPP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0010011")).Return("Sent to SAP for Processing").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100013")).Return("MWA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100013")).Return("Order Cancelled").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0010052")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0010052")).Return("CDO Cancelled").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0010053")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0010053")).Return("Penalties may be appplied for late submission").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100010")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100010")).Return("Order Confirmed").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100012")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100012")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100015")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100015")).Return("CDO Invalid Order Instruction").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100018")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100018")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0100019")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0100019")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0110110")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0110110")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0200001")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0200001")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0200043")).Return("MPP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0200043")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Event Type", "0200044")).Return("MPP").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "FPM APERAK from TNPA", "Error Code", "Reference", "0200044")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<APERAK2UEvent>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
