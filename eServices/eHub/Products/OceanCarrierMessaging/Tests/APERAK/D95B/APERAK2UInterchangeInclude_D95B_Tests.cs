using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class APERAK2UInterchangeInclude_D95B_Tests
  {
    const string filePath = "APERAK.D95B.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void APERAK2UEvent()
    {
      AssertMapping("MARFRET", "Test1_input.xml", "Test1_output.xml", "UNB++++999999:9999+", false, "MFUMSG", "MFU00000000099", "AGT", "ConsolNumberC00001188", "Booking Request");
      AssertMapping("MARFRET", "Test2_IsDirect_input.xml", "Test2_IsDirect_output.xml", "UNB++++999999:9999+", true, "MFUMSG", "MFU00000000099", "AGT", "ConsolNumberC00001188", "Booking Request");
      AssertMapping("MARFRET", "Test3_IsCoLoad_input.xml", "Test3_IsCoLoad_output.xml", "UNB++++999999:9999+", false, "MFUMSG", "MFU00000000099", "CLD", "ConsolNumberC00001188", "Booking Request");
      AssertMapping("MARFRET", "Test4_input.xml", "Test4_output.xml", "UNB++++999999:9999+", false, "MFUMSG", "MFU00000000099", "CLD", "ConsolNumberC00001188", "Verified Gross Container Weight", "VERMAS");
    }

    private void AssertMapping(string senderID, string inputFile, string expectedOutputFile, string unbSegment, bool isDirect, string st_msg, string messageReference, string shipmentType, string consolNumber, string documentName, string messageType = "IFTMBF")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;
      var msgid = isDirect
              ? st_msg.Substring(0, 3) + "MSG"
              : st_msg;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDAULJ1").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(unbSegment).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB2").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgid).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect ? "TRUE" : "FALSE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, messageType, "AP")).Return("ACCEPTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, messageType, "AP")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Reference", senderID, messageType, "RE")).Return("REJECTED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMAPERAK", "OCMAPERAK", "OCM APERAK Configuration", "Event Type", "Event Type", senderID, messageType, "RE")).Return("IRJ").Repeat.Any();

      if (isDirect)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", consolNumber, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", consolNumber, "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", consolNumber, "@referenceType", "InterchangeNum")).Return("288").Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "IFTMBF")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "VERMAS")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", consolNumber, "@referenceType", "ShipmentType")).Return(shipmentType).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "JobNumber")).Return(consolNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", messageReference, "@referenceType", "InterchangeNum")).Return("288").Repeat.Any();
      }
      
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", msgid, "@value", "288", "@referenceType", "DocumentName")).Return(documentName).Repeat.Any();

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
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude_D95B>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
