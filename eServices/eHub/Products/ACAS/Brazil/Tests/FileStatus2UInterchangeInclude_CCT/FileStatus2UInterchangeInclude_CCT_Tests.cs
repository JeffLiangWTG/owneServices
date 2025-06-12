using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ACAS.BR.Transforms.FileStatus2UInterchangeInclude_CCT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class FileStatus2UInterchangeInclude_CCT_Tests
  {
    const string filePath = "FileStatus2UInterchangeInclude_CCT.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestFileStatus2UInterchangeInclude_CCT()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_input.xml", "Test4_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_BR").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "ACAS_BR", "@ST_ID", "ACASBR", "@value", "CCT-0000001")).Return("HYEUAT001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "ACAS_BR", "@ST_ID", "ACASBR", "@value", "CCT-0000002")).Return("HYEUAT001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "CCT-0000001", "@referenceType", "ProtocolNumber")).Return("XXX_S00010000").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "CCT-0000002", "@referenceType", "ProtocolNumber")).Return("XXX_S00010001").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "ShipmentId")).Return("S00010000").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "DocumentName")).Return("CCT Message").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "HAWB")).Return("HAWB001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "MAWB")).Return("MAWB001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010000", "@referenceType", "ActionPurpose")).Return("AMD").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "ShipmentId")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "DocumentName")).Return("CCT Message").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "HAWB")).Return("HAWB001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "MAWB")).Return("MAWB001").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYEUAT001", "@ST_ID", "ACASBR", "@value", "XXX_S00010001", "@referenceType", "ActionPurpose")).Return("AMD").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<FileStatus2UInterchangeInclude_CCT>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
