using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NZ.N4PortConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NZ.Tests
{
  [TestClass]
  public class PreadviceContainerResult2UInterchangeInclude_Tests
  {
    const string filePath = "N4PortConnect.PreadviceContainerResult2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestPreadviceContainerResult2UInterchangeInclude()
    {
      AssertMapping("Test1_input_SUCCESS.xml", "Test1_output_ORG_SUCCESS.xml", "ORG");
      AssertMapping("Test1_input_SUCCESS.xml", "Test1_output_WTH_SUCCESS.xml", "WTH");
      AssertMapping("Test2_input_MULTIPLE_FAILURE.xml", "Test2_output_MULTIPLE_FAILURE.xml", "ORG");
      AssertMapping("Test3_input_SINGLE_FAILURE.xml", "Test3_output_SINGLE_FAILURE.xml", "ORG");
      AssertMapping("Test4_input_MultipleContainers_FAILURE.xml", "Test4_output_MultipleContainers_FAILURE.xml", "ORG");
      AssertMapping("Test5_input_no_container_number.xml", "Test5_output_no_container_number.xml", "ORG");
		}

    void AssertMapping(string sourceFile, string expectedFile, string purpose)
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      var serviceProvider = "PORTCONNECT";
      var serviceProviderMsgID = "PCNMSG";
      var preAdvice = "111222333";
      var forwardingType = "ForwardingConsol_C00001386";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMsgID);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", "PCIU5600395_NZTRG_CustomerCode", "@referenceType", "PreAdvice")).Return(preAdvice).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", "PCIU2222222_NZTRG_CustomerCode", "@referenceType", "PreAdvice")).Return(preAdvice).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", "_NZTRG_CustomerCode", "@referenceType", "PreAdvice")).Return(preAdvice).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", preAdvice, "@referenceType", "DocumentName")).Return("Pre-Advice Export Notificaiton (NZ)").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", preAdvice, "@referenceType", "ForwardingType")).Return(forwardingType).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMsgID, "@value", preAdvice, "@referenceType", "Purpose")).Return(purpose).Repeat.Any();

			mockDateMapper.Expect(x => x.CurrentDateTime("yyyy-MM-ddTHH:mm:ss")).Return("2024-01-05T14:48:00").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<PreadviceContainerResult2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
