using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class APERAK2UInterchangeInclude_Tests
  {
    const string filePath = "CPOINT.APERAK2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestAPERAK2UInterchangeInclude()
    {
        AssertMapping("Test1_input_example_nok.xml", "Test1_output.xml", "IFTDGN", "BE0429672881004", "CHECKPOINT123", "RE", "MRJ", "", "ORG");
        AssertMapping("Test2_input_APERAK_example_ok.xml", "Test2_output.xml", "EBADEC", "0001703993", "0001703993", "AP", "MAA", "", "ORG");
        AssertMapping("Test3_input_EBADEC_CONT_Response.xml", "Test3_output.xml", "EBADEC", "000000205414", "000000205414", "AP", "MAA", "", "ORG");
        AssertMapping("Test4_input_EBADEC_RORO_Response.xml", "Test4_output.xml", "EBADEC", "000000431780", "000000431780", "AP", "MWA", "", "WTH");
        AssertMapping("Test5_input_example_IFTDGN_CA.xml", "Test5_output.xml", "IFTDGN", "INTRXX00000262_01", "INTRXX00000262_01", "CA", "MAA", "Conditionally Accepted", "ORG");
        AssertMapping("Test6_input_example_IFTDGN_AP.xml", "Test6_output.xml", "IFTDGN", "POLYT 00505137101", "POLYT 00505137101", "AP", "MAA", "", "ORG");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string messagetype, string documentIdentifier, string checkPoint, string eventCode, string eventType, string eventRef, string subscribedActionPurpose)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "CPOINT";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", checkPoint, "@referenceType", "CheckPoint")).Return(documentIdentifier);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", checkPoint, "@referenceType", "OperationPort")).Return("BEANR");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "DocumentName")).Return("EBADEC");

      if (messagetype == "IFTDGN")
      {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId",
              serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier,
              "@referenceType", "IFTDGN")).Return("C03078216");
      }
      else
      {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "IFTDGN")).Return("");
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "EBADEC")).Return("C03078222");
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "ForwardingType")).Return("ForwardingConsol");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "Purpose")).Return(subscribedActionPurpose);

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "CPOINT System Configuration", "Event Type", "Event Type", eventCode, subscribedActionPurpose)).Return(eventType).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, "CPOINT System Configuration", "Event Type", "Event Reference", eventCode, subscribedActionPurpose)).Return(eventRef).Repeat.Any();

            var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<APERAK2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}