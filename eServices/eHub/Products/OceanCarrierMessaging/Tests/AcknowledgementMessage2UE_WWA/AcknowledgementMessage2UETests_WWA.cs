using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Acknowledgement2UE_WWA;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class AcknowledgementMessage2UETests_WWA
  {
    const string filePath = "AcknowledgementMessage2UE_WWA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void AcknowledgementMessage2UE_WWA()
    {
      AssertMapping("Test1_BL_input.xml", "Test1_BL_output.xml");
      AssertMapping("Test2_BL_input.xml", "Test2_BL_output.xml");
      AssertMapping("Test3_BOOK_input.xml", "Test3_BOOK_output.xml");
      AssertMapping("Test4_BOOK_input.xml", "Test4_BOOK_output.xml");
      AssertMapping("Test5_NoAckStatus_input.xml", "Test5_NoAckStatus_output.xml");
    }

    private void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WWA").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.GetOCMBEClientID("WWA", "CARGOWISE")).Return("OCM_BookingEngine").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "OCM_BookingEngine")).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "WWA", "@recipientId", "", "@ST_ID", "WWAMSG", "@value", "283789ebjfb", "@referenceType", "JobNumber")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA_Acknowledgement", "Event Type", "Event Type", "true")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA_Acknowledgement", "Event Type", "Event Type", "A")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA_Acknowledgement", "Event Type", "Event Type", "R")).Return("IRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("WWA", "WWA", "WWA_Acknowledgement", "Event Type", "Event Type", "*")).Return("IRJ").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("s"))).Return("2018-02-20T09:54:30");
      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Acknowledgement2UE_WWA>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
