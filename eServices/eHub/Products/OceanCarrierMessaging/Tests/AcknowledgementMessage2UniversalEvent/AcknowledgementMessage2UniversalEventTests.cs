using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Acknowledgement2UniversalEvent;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class AcknowledgementMessage2UniversalEventTests
  {
    const string filePath = "AcknowledgementMessage2UniversalEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void AcknowledgementMessage2UniversalEvent()
    {
      AssertMapping("GTNACK_Success.xml", "UniversalEvent_Success.xml", "C00001072");
      AssertMapping("GTNACK_Warning.xml", "UniversalEvent_Warning.xml", "C00001072");
      AssertMapping("GTNACK_Error.xml", "UniversalEvent_Error.xml");
      AssertMapping("GTNACK_Unknown.xml", "UniversalEvent_Unknown.xml");
      AssertMapping("GTNACK_Error_EventReferenceTooLong.xml", "UniversalEvent_Error_EventReferenceTooLong.xml");
      AssertMapping("Test1_BookingRequest_Warning_input.xml", "Test1_BookingRequest_Warning_output.xml");
    }

    private void AssertMapping(string inputFile, string expectedOutputFile, string subscribedJobNumber = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("OCMAPERAK").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDAULJ1").Repeat.AtLeastOnce();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("OCMAPERAK", "HYEDAULJ1")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "", "@ST_ID", "GTNMSG", "@value", "GTN000000100001", "@referenceType", "JobNumber")).Return(subscribedJobNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "", "@ST_ID", "GTNMSG", "@value", "C00001072", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "", "@ST_ID", "GTNMSG", "@value", "GTN000000100001")).Return("C00001072").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "", "@ST_ID", "GTNMSG", "@value", "C00001072")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "OCMAPERAK", "APLU")).Return("APLU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "OCMAPERAK", "AAAA")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Acknowledgement2UniversalEvent>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
