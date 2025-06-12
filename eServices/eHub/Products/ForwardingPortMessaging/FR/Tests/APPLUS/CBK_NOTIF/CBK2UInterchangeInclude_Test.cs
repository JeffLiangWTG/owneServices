using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class CBK2UInterchangeInclude_Test
  {
    const string filePath = "APPLUS.CBK_NOTIF.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCBK2UInterchangeInclude()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "MGI");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "SOGET");

      //multiple equipement-cbk
      AssertMapping("Test3_input.xml", "Test3_output.xml", "SOGET");

      //multiple Messages > equipement-cbk
      AssertMapping("Test4_input.xml", "Test4_output.xml", "SOGET", eHubID: "HYEUAT001");

      // No equipement-cbk
      AssertMapping("Test5_input.xml", "Test5_output.xml", "SOGET", eHubID: "HYEUAT001");

      //Container number is blank
      AssertMapping("Test6_input.xml", "Test6_output.xml", "SOGET", eHubID: "HYEUAT001");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string eHubID = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEUAT001")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", serviceProvider, "CMACGMAG")).Return("CMACGM").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", serviceProvider, "INTTRA")).Return("INTTRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProvider + "ID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "22T1")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "42T1")).Return("42G0").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("BOLLORTR,BOLLORTR", serviceProvider)).Return(eHubID).Repeat.Any();
      if (string.IsNullOrEmpty(eHubID))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProvider + "ID", "@value", "BOLLORTR,BOLLORTR")).Return("HYEUAT001").Repeat.Once();
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CBK2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
