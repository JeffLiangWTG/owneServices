using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CONTRL2UEvent;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CONTRL2UniversalEvent_Tests
  {
    const string filePath = "CONTRL2UniversalEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCONTRL2UniversalEvent()
    {
      AssertMapping("Test4_input.xml", "Test4_output.xml", "EASIPASS", "ESPMSG", "false", "ESP", "ForwardingShipment", "eManifest", "Shipment", expectedConsolContainerNumber:"C00001360_CGMU93830983");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "GTNEXUS", "GTXMSG", "false", "GTX", "ForwardingConsole", "Verified Gross Container Weight", "Container", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "INTTRA", "INTMSG", "false", "INT", "", "Booking Request", "", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test7_input_HapagLloyd.xml", "Test7_output_HapagLloyd.xml", "HAPAG_LLOYD", "HAPMSG", "false", "HAP", "ForwardingConsol", "Booking Request", "", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "EASIPASS", "ESPMSG", "false", "ESP", "ForwardingShipment", "eManifest", "Shipment", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "HAPAG_LLOYD", "HAPMSG", "false", "HAP", "ForwardingConsol", "Booking Request", "", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "HAPAG_LLOYD", "HAPMSG", "false", "HAP", "ForwardingConsol", "Booking Request", expectedConsolContainerNumber:"C00001360_CGMU93830983", isEmptyBody:true);
      AssertMapping("Test11_input.xml", "Test11_output.xml", "HAPAG_LLOYD", "HAPMSG", "false", "HAP", "ForwardingConsol", "Booking Request", expectedConsolContainerNumber:"C00001360_CGMU93830983", isEmptyBody: true);
      AssertMapping("Test12_input.xml", "Test12_output.xml", "EASIPASS", "ESPMSG", "false", "ESP", "ForwardingShipment", "eManifest", "Shipment", expectedConsolContainerNumber: "C00001360_CGMU93830983");
      AssertMapping("Test13_input.xml", "Test13_output.xml", "EASIPASS", "ESPMSG", "false", "ESP", "ForwardingShipment", "eManifest", "Shipment", expectedConsolContainerNumber: "C00001360_CGMU93830983");

      AssertMapping("Test14_input.xml", "Test14_output.xml", "SINOTRANS_EASTERN", "ESTMSG", "true", "EST", "ForwardingConsol", "Booking Request", "", "C00001360", "EST00000001");
      AssertMapping("Test15_input.xml", "Test15_output.xml", "SINOTRANS_EASTERN", "ESTMSG", "true", "EST", "ForwardingConsol", "Booking Request", "", "C00001360", "EST00000001");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCONTRL2UniversalEvent_AcceptReject()
    {
      TestCONTRL2UniversalEvent_Accepted("CMACGM", "CMAMSG");
      TestCONTRL2UniversalEvent_Rejected("CMACGM", "CMAMSG");

      TestCONTRL2UniversalEvent_Accepted("PORTRIX", "PORMSG");
      TestCONTRL2UniversalEvent_Rejected("PORTRIX", "PORMSG");

      TestCONTRL2UniversalEvent_Accepted("INTTRA", "INTMSG");
      TestCONTRL2UniversalEvent_Rejected("INTTRA", "INTMSG");
    }

    void TestCONTRL2UniversalEvent_Accepted(string senderID, string st_id)
    {
      AssertMapping(string.Format("Test1_input_{0}.xml", senderID), string.Format("Test1_output_SI.xml", senderID), senderID, st_id, expectedConsolContainerNumber: "C00001360");
      AssertMapping(string.Format("Test3_input_{0}.xml", senderID), string.Format("Test3_output_BK.xml", senderID), senderID, st_id, expectedConsolContainerNumber: "C00001360");
    }

    void TestCONTRL2UniversalEvent_Rejected(string senderID, string st_id, string forwardingType = "", string documentName = "", string subMessageType = "")
    {
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "4")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "6")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Error", "Description", senderID, "")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Error", "Description", senderID, "12")).Return("Invalid value");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(st_id).Repeat.AtLeastOnce();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", senderID)).Return("XXX").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "6")).Return("C00001360_CGMU93830983").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "6", "@referenceType", "JobNumber")).Return("C00001360_CGMU93830983").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "6", "@referenceType", "ForwardingType")).Return(forwardingType).Repeat.AtLeastOnce();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "6", "@referenceType", "DocumentName")).Return(documentName).Repeat.AtLeastOnce();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "6", "@referenceType", "SubMessageType")).Return(subMessageType).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.AtLeastOnce();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, "TESTSENDER__1")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      var sourceFile = filePath + string.Format("Test2_input_{0}.xml", senderID);
      var expectedFile = filePath + string.Format("Test2_output_VM.xml", senderID);
      mapTester.ExecuteCompiled<CONTRL2UniversalEvent>(sourceFile, expectedFile);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string senderID, string st_id,
                       string isDirect = "false", string prefix = "false",
                       string forwardingType = "", string documentName = "",
                       string subMessageType = "", string messageReference = "",
                       string expectedConsolContainerNumber = "", bool isEmptyBody = false)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++161121:0802+");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "7")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "8")).Return("IRA");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "4")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "6")).Return("IRJ");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Event Type", "Event Type", senderID, "12")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Error", "Description", senderID, "")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMCONTRL", "OCMCONTRL", "OCM CONTRL Configuration", "Error", "Description", senderID, "12")).Return("Invalid value");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(st_id).Repeat.AtLeastOnce();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", senderID)).Return(prefix).Repeat.Any();

      if (isDirect.ToUpper() == "TRUE")
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", messageReference, "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", messageReference)).Return(expectedConsolContainerNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", messageReference, "@referenceType", "ForwardingType")).Return(forwardingType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", messageReference, "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", messageReference, "@referenceType", "SubMessageType")).Return(subMessageType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", expectedConsolContainerNumber, "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", expectedConsolContainerNumber)).Return(messageReference).Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "65")).Return(expectedConsolContainerNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "65", "@referenceType", "JobNumber")).Return(expectedConsolContainerNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "65", "@referenceType", "ForwardingType")).Return(forwardingType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "65", "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "65", "@referenceType", "SubMessageType")).Return(subMessageType).Repeat.Any();
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "66")).Return("C00001366_CGMU93830966").Repeat.Any();
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "66", "@referenceType", "JobNumber")).Return("C00001366_CGMU93830966");
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "66", "@referenceType", "ForwardingType")).Return(forwardingType);
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "66", "@referenceType", "DocumentName")).Return(documentName);
        mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "TESTSENDER__1", "@ST_ID", st_id, "@value", "66", "@referenceType", "SubMessageType")).Return(subMessageType);
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.AtLeastOnce();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID(senderID, "TESTSENDER__1")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CONTRL2UniversalEvent>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();

      if (!isEmptyBody)
      {
        mockCodeMapper.VerifyAllExpectations();
      }
    }
  }
}
