using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Tests
{
  [TestClass]
  public class M1162UniEventTests
  {
    const string filePath = "M1162UniEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void M1162UniEvent()
    {
      AssertMapping("Test1_AP_input.xml", "Test1_AP_output.xml", "AP", "MAA", "ORG");
      AssertMapping("Test2_RP_input.xml", "Test2_RP_output.xml", "RP", "MRJ", "ORG");
      AssertMapping("Test3_UKN_input.xml", "Test3_UKN_output.xml", "UKN", "", "ORG");
      AssertMapping("Test4_AP_for_cancellation_input.xml", "Test4_AP_for_cancellation_output.xml", "AP", "MWA", "WTH");
    }

    void AssertMapping(string sourceFile, string expectedFile, string responseTypeCode, string eventType, string subscribedActionPurpose)
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", "100")).Return("C00001004_SHPMRN0001").Repeat.AtLeastOnce();


      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", "100", "@referenceType", "Purpose")).Return(subscribedActionPurpose);

      mockCodeMapper.Expect(x => x.GetRecipientCode("PORTBASE", "PORTBASE", "Acknowledgement Message M116 from Portbase", "Event Type", "Event Type", responseTypeCode, subscribedActionPurpose)).Return(eventType).Repeat.Once();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<M1162UniEvent>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
