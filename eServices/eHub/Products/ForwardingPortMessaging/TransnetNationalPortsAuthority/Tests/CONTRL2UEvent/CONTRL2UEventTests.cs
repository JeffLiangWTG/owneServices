using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Transforms.CONTRL2UEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.TNPA.Tests
{
  [TestClass]
  public class CONTRL2UEventTests
  {
    const string filePath = "CONTRL2UEvent.TestFiles.";


    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCONTRL2UniversalEvent()
    {
      //Accepted
      AssertMapping("Test1_input.xml", "Test1_output.xml", "NPAEDI", "C00001360_IMPORT");
      AssertMapping("Test1_input.xml", "Test1_output.xml", "NPAEDI", "", "C00001360", "Cargo Dues - Import");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "NPAEDI", "");

      //Rejected
      AssertMapping("Test2_input.xml", "Test2_output.xml", "NPACLIENT", "C00001360_LOAD");
    }

    void AssertMapping(string inputFile, string outputFile, string senderID, string subscribedConsolDocumentName, string subscribedJobNumber = "", string subscribedDocumentName = "")
    {
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();


      mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("UNB++++171123:1243+");
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "Container Discharge/Loading CONTRL from TNPA", "Event Type", "Event Type", "7")).Return("IRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "Container Discharge/Loading CONTRL from TNPA", "Event Type", "Event Type", "4")).Return("IRJ").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "TESTSENDER__1", "@ST_ID", "NPAMSG", "@value", "8074")).Return(subscribedConsolDocumentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "TESTSENDER__1", "@ST_ID", "NPAMSG", "@value", "8074", "@referenceType", "JobNumber")).Return(subscribedJobNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "TESTSENDER__1", "@ST_ID", "NPAMSG", "@value", "8074", "@referenceType", "DocumentName")).Return(subscribedDocumentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "TNPA", "@recipientId", "TESTSENDER__1", "@ST_ID", "NPAMSG", "@value", "8074", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(senderID).Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "Container Discharge/Loading CONTRL from TNPA", "Error", "Description", "2")).Return("Syntax version or level not supported").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "Container Discharge/Loading CONTRL from TNPA", "Error", "Description", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("TNPA", "TNPA", "Container Discharge/Loading CONTRL from TNPA", "Error", "Description", "12")).Return("Invalid value").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      var sourceFile = filePath + inputFile;
      var expectedFile = filePath + outputFile;
      mapTester.Execute<CONTRL2UniversalEvent>(sourceFile, expectedFile);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
