using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NL.Portbase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NL.Tests
{
  [TestClass]
  public class M24032UInterchangeInclude_Tests
  {
    const string filePath = "Portbase.M24032UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestM24032UInterchangeInclude()
    {
      AssertMapping("Test1_input_AP.xml", "Test1_output_AP.xml");
      AssertMapping("Test2_input_RP.xml", "Test2_output_RP.xml");
      AssertMapping("Test3_input_RE.xml", "Test3_output_RE.xml");
    }

    void AssertMapping(string sourceFile, string expectedFile)
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "PORTBASE";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var documentIdentifier = "PBS0000000099I";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "DocumentName")).Return("Import Notification");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "M2400")).Return("C00679603_17DE345355864324E8");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", documentIdentifier, "@referenceType", "ForwardingType")).Return("ForwardingConsol");

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Event Type (M2403)", "Event Type", "AP")).Return("MAA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Event Type (M2403)", "Event Type", "RP")).Return("MRJ").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Event Type (M2403)", "Event Type", "RE")).Return("MRJ").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<M24032UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}