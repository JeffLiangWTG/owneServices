using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ACAS.BR.Transforms.AsyncPolling2UInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class AsyncPolling2UInterchangeInclude_Tests
  {
    const string filePath = "AsyncPolling2UInterchange.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestAsyncPolling2UInterchange()
    {
      AssertMapping("Test1_WithoutStateDescription_input.xml", "Test1_WithoutStateDescription_output.xml");
      AssertMapping("Test2_WithStateDescription_input.xml", "Test2_WithStateDescription_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      string inputPath = filePath + inputFile;
      string expectedOutputPath = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2019-10-01T14:59:00");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_BR").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "ACAS_BR", "@ST_ID", "ACASBR", "@value", "20200107043718")).Return("HYEUAT001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "S2154644451_FZB0000001138", "@referenceType", "DocumentName")).Return("XXX Document").Repeat.Any();
      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper}
      };


      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<AsyncPolling2UInterchange>(inputPath, expectedOutputPath);
    }
  }
}
