using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms.IFTMCS2UInterchangeEnvelope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class IFTMCS2UInterchangeEnvelope_Tests
  {
    const string filePath = "IFTMCS2UInterchangeEnvelope.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void IFTMCS2UniversalInterchange()
    {
      AssertMapping("All requirements are matched (Event Code MAA)", "Test1_input.xml", "Test1_output_MAA.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.JobNumber, "TRUE");
      AssertMapping("All requirements are matched (Event Code MRR)", "Test1_input.xml", "Test1_output_MRR.xml", "INTTRA", "INTMSG", "MRR", SubscriberConsolReferenceType.JobNumber, "TRUE");
      AssertMapping("Should not generate UEvent, invalid EventCode", "Test1_input.xml", "Test1_output_EmptyBody.xml", "INTTRA", "INTMSG", "XXX", SubscriberConsolReferenceType.JobNumber, "TRUE");

      AssertMapping("Should generate UEvent, RFF+BN has value, RFF+BM is missing", "Test2_input.xml", "Test2_output.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.IFTMIN, "TRUE");
      AssertMapping("Should not generate UEvent, RFF+BM  has value, RFF+BN is missing", "Test3_input.xml", "Test3_output.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.IFTMIN, rffValue: "INT0001000099");

      AssertMapping("Should not generate UEvent, RFF+BN And RFF+BM are blank", "Test4_input.xml", "Test1_output_EmptyBody.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.Default, rffValue: "INT0001000099");
      
      AssertMapping("Is Direct", "Test5_input_Direct_RFF_FF.xml", "Test5_output_Direct_RFF_FF.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.JobNumber, "TRUE", rffCode: "FF", destinationParty: "NADHI_SCAC");
      AssertMapping("Is Direct", "Test5_input_Direct_RFF_SI.xml", "Test5_output_Direct_RFF_SI.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.JobNumber, "TRUE");

      AssertMapping("Is Direct", "Test6_input_Direct_RFF_FF_MAERSK.xml", "Test6_output_Direct.xml", "MAERSK", "MAEMSG", "MAA", SubscriberConsolReferenceType.JobNumber, "NO", rffCode: "FF");

      AssertMapping("Empty Consol", "Test7_input_empty_consol.xml", "Test7_output_empty_consol.xml", "INTTRA", "INTMSG", "MAA", SubscriberConsolReferenceType.JobNumber, "TRUE", "", "", "", "", "CW1XXXXX");
    }

    void AssertMapping(string testMessage, string inputFile, string expectedOutputFile, string senderID, string msgID, string eventCode, SubscriberConsolReferenceType subscriberConsolReferenceType, string isDirect = "NO", string rffCode = "SI", string rffFallbackValue = "ZZZ", string rffValue = "C00001005", string consol = "C00001005", string destinationParty = "CW1XXXXX")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(senderID, msgID, eventCode, subscriberConsolReferenceType, isDirect, rffCode, rffFallbackValue, rffValue, destinationParty, consol);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<IFTMCS2UInterchangeEnvelope>(input, expectedOutput);

      extensionObjects["http://cargowise.com/Core/Transforms/Helper/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://cargowise.com/Core/Transforms/Helper/ContextAccessor"].VerifyAllExpectations();
    }

    Dictionary<string, object> SetupMappingExtensions(string senderID, string msgID, string eventCode, SubscriberConsolReferenceType subscriberConsolReferenceType, string isDirect = "N", string rffCode = "SI", string rffFallbackValue = "ZZZ", string rffValue = "C00001005", string destinationParty = "CW1XXXXX", string consol = "C00001005")
    {
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", destinationParty)).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMCS", "OCMIFTMCS", "OCM IFTMCS Configuration", "EventType", "EventType", senderID, "22")).Return(eventCode);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", senderID)).Return(msgID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", senderID)).Return(isDirect).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMCS", "OCMIFTMCS", "OCM IFTMCS Configuration", "Reference No", "Value", senderID)).Return(rffCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMCS", "OCMIFTMCS", "OCM IFTMCS Configuration", "Reference No", "Fallback Value", senderID)).Return(rffFallbackValue).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@value", rffValue, "@ST_ID", msgID)).Return("CW1XXXXX").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("", senderID, false)).Return("").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("NADHI_SCAC", senderID, false)).Return("NADHI_SCAC").Repeat.Any();

      if (subscriberConsolReferenceType == SubscriberConsolReferenceType.JobNumber)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "JobNumber")).Return(consol).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue)).Return("").Repeat.Any();
      }
      else if (subscriberConsolReferenceType == SubscriberConsolReferenceType.IFTMIN)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "IFTMIN")).Return(consol).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue)).Return("").Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "JobNumber")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue, "@referenceType", "IFTMIN")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "CW1XXXXX", "@ST_ID", msgID, "@value", rffValue)).Return(consol).Repeat.Any();
      }

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss.fff")).Return("2018-10-26T11:59:59.999").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "NADCA_SCAC")).Return("Mapped_NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", senderID, "TDT_SCAC")).Return("Mapped_TDT_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "INTTRA", "UNB_SCAC")).Return("UNB_SCAC").Repeat.Any();

      return new Dictionary<string, object>()
      {
        { "http://cargowise.com/Core/Transforms/Helper/ContextAccessor", mockContextAccessor },
        { "http://cargowise.com/Core/Transforms/Helper/CodeMapper", mockCodeMapper },
        { "http://cargowise.com/Core/Transforms/Helper/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };
    }

    enum SubscriberConsolReferenceType
    {
      JobNumber = 1,
      IFTMIN = 2,
      Default = 3
    }
  }
}
