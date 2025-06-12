using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class DTI2UInterchangeInclude_Test
  {
    const string filePath = "APPLUS.DTI.TestFiles_DTI_NOTIF.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestDTI2UInterchangeInclude()
    {
      AssertMapping("Test1_input_OK.xml", "Test1_output_OK.xml");
      AssertMapping("Test2_input_NOK.xml", "Test2_output_NOK.xml", eHubID: "DHL");
      AssertMapping("Test3_input_OK.xml", "Test3_output_OK.xml", eHubID: "DHL");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string eHubID = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var recipientID = "DHL";
      var senderID = "MGI";
      var serviceProvider = "MGI";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var serviceProviderID = serviceProviderPrefix + "ID";
      var serviceProviderMSGID = serviceProviderPrefix + "MSG";
      var serviceProviderSICID = serviceProviderPrefix + "SIC";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("BOLLORTR,BOLLORTR", serviceProvider)).Return(eHubID).Repeat.Any();
      if (string.IsNullOrEmpty(eHubID))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProviderID, "@value", "BOLLORTR,BOLLORTR")).Return(recipientID).Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProviderID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", serviceProvider)).Return(serviceProviderPrefix).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "MGI00000001", "@referenceType", "JobNumber")).Return("C00001234").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "MGI00000001", "@referenceType", "DocumentName")).Return("DTI Notification").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderSICID, "@value", "DTI_LPD61000678")).Return("MGI00000001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderSICID, "@value", "DTI_DTI61002306")).Return("MGI00000001").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "Acknowledgment Status", "Output Code", "OK")).Return("OK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "ContainerTypeToISOCode", "MGI Code", "45G1")).Return("ISO45G1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", senderID, "CCGM")).Return("SCACCCGM").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<DTI2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
