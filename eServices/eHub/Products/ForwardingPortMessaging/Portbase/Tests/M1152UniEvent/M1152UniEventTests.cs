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
  public class M1152UniEventTests
  {
    private const string filePath = "M1152UniEvent.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void M1152UniEvent()
    {
      var containerMRNNumbers = new Dictionary<string, string>
      {
        { "CGMU7864099", "DOC09849083" }
      };

      AssertMapping("Test1_input.xml", "Test1_output.xml", containerMRNNumbers, "C00001985");

      containerMRNNumbers = new Dictionary<string, string>
      {
        { "TRLU2456893", "DOC09849083"},
        { "TGMU3039480", "DOC09849083"}
      };
      AssertMapping("Test2_input.xml", "Test2_output.xml", containerMRNNumbers, "C00001985");

      containerMRNNumbers = new Dictionary<string, string>
      {
        { "CGMU7864099", "DOC09849083" }
      };
      AssertMapping("Test3_input.xml", "Test3_output.xml", containerMRNNumbers);
    }

    public void AssertMapping(string inputFile, string expectedOutputFile, Dictionary<string, string> containerMRNNumbers, string expectedConsolNumber = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("PORTBASE", "PORTBASE", "Notification Message M115 from Portbase", "Defaults", "Event Type")).Return("STU").Repeat.Once();

      foreach (var containerMRN in containerMRNNumbers)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", containerMRN.Key + containerMRN.Value)).Return(expectedConsolNumber).Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "", "@ST_ID", "PBSMSG", "@value", containerMRN.Key + containerMRN.Value, "@referenceType", "EquipmentID")).Return(expectedConsolNumber).Repeat.Any();
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockDataModelAccessor }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<M1152UniEvent>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
