using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class PER2UInterchangeInclude_Tests
  {
    const string filePath = "PER2UInterchangeInclude_ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestPER2UInterchangeInclude_ACAS_US()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_US").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "WTGCODEZZZZZ0000XX", "@referenceType", "FRI-HWB")).Return("ACAS0000000888").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS0000000888", "@referenceType", "ShipmentId")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS0000000888", "@referenceType", "DocumentName")).Return("ACAS Shipment Report").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS0000000888", "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("dd-MMM-yyyy HH:mm")).Return("20-Mar-2018 05:43");
      mockCodeMapper.Expect(x => x.GetRecipientCode("ACAS_US", "ACAS_US", "ACAS System Configuration", "Error", "Description", "MISSING_CNE_CTRY")).Return("Missing Consignee Country").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<Transforms.PER2UInterchangeInclude_ACAS_US.PER2UInterchangeInclude_ACAS_US>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockDateMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
