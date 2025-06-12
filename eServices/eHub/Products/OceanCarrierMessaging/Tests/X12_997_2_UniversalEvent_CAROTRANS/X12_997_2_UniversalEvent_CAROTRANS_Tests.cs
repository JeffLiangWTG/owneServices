using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.X12_997_2_UEvent_CAROTRANS;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class X12_997_2_UniversalEvent_CAROTRANS_Tests
  {
    const string filePath = "X12_997_2_UniversalEvent_CAROTRANS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestX12_997_2_UniversalEvent_CAROTRANS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "A", "IRA", "GS*FA*CAROTRANS*HYEBNEUAT*20170410*1340*001079523*X*004010", "C01329220");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "R", "IRJ", "GS*FA*CAROTRANS*HYEBNEUAT*20170410*1340*001079523*X*004010", "C01329220", "5", "One or More Segments in Error", "2", "Conditional required data element missing");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string eventCode, string eventType, string gsSegment, string subscribedConsolRef, string txErrorCode = "", string txErrorDes = "", string dataErrorCode = "", string dataErrorDes = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("VANGUARD").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("VANGUARD", "CARGOWISE")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Functional Acknowledgment X12_997 from CAROTRANS", "EventType", "EventType", eventCode)).Return(eventType);
      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Functional Acknowledgment X12_997 from CAROTRANS", "TxSetSyntaxError", "TxErrorDes", txErrorCode)).Return(txErrorDes);
      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Functional Acknowledgment X12_997 from CAROTRANS", "DataElementSyntaxError", "DataErrorDes", dataErrorCode)).Return(dataErrorDes).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "878500282", "@referenceType", "JobNumber")).Return(subscribedConsolRef == "" ? "878500282" : subscribedConsolRef);
      mockContextAccessor.Expect(x => x.GetContextProperty("GS_Segment", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(gsSegment);

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<X12_997_2_UEvent_CAROTRANS>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
