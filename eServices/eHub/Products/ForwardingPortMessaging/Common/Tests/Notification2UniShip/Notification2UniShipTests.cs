using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Common.Transforms.Notification2UniShip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Common.Tests
{
  [TestClass]
  public class Notification2UniShipTests
  {
    const string filePath = "Notification2UniShip.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestNotification2UniShip()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
      AssertMapping("Test7_input.xml", "Test7_output.xml");
      AssertMapping("Test8_input_NoContainerNumber.xml", "Test8_output_NoContainerNumber.xml");
      AssertMapping("Test9_input_IRJ.xml", "Test9_output_IRJ.xml");
      AssertMapping("Test10_input_SystemError.xml", "Test10_output_SystemError.xml");
      AssertMapping("Test11_input_.xml", "Test11_output.xml");
      AssertMapping("Test12_input_IRJ.xml", "Test12_output_IRJ.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
      mockContextAccessor.Stub(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("IRJ");
      mockContextAccessor.Stub(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("Department=WiseTechGlobal|Reason=You are not registered with this service. Contact WTG to register.");
      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Subscription Type ID", "Message Reference ST ID", "NGBEDI")).Return("NGBMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "NGBEDI", "@recipientId", "", "@ST_ID", "NGBMSG", "@value", "C00681546")).Return("C03078216").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "NGBEDI", "@recipientId", "", "@ST_ID", "NGBMSG", "@value", "9e238311-2885-4cd5-82da-150af33e65a7")).Return("88_BBBBB").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Subscription Type ID", "Message Reference ST ID", "MGI")).Return("MGIMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "MGI", "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "5210e0eb-567e-4349-85d3-34599e92dd9f")).Return("88_BBBBB").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Subscription Type ID", "Message Reference ST ID", "PORTCONNECT")).Return("PRCMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTCONNECT", "@recipientId", "", "@ST_ID", "PRCMSG", "@value", "5671e9e3-5a18-4dde-879f-a364a3e99b36")).Return("88_BBBBB").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Notification2UniShip>(input, expectedOutput);
    }
  }
}
