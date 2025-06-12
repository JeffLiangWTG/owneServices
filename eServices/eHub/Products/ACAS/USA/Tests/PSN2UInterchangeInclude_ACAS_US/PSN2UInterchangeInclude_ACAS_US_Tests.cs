using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class PSN2UInterchangeInclude_ACAS_US_Test
  {
    const string filePath = "PSN2UInterchangeInclude_ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestPSN2UInterchangeInclude_ACAS_US()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribeRefNo)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_US").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "WTGCODEZZZ12345678", "@referenceType", "FRI-HWB")).Return("ACAS00000188").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "WTGCODEZZZ12345678", "@referenceType", "FHL-HWB")).Return(subscribeRefNo).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "WTGCODE074-54909083", "@referenceType", "FRI-HWB")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "WTGCODE074-54909083", "@referenceType", "FRI-MAWB")).Return("ACAS00000188").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ZZZ12345678")).Return("ACAS00000188").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "074-54909083")).Return("ACAS00000188").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS00000188", "@referenceType", "ShipmentId")).Return("C00001360").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS00000188", "@referenceType", "DocumentName")).Return("ACAS Shipment Report").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "CARGOWISE", "@ST_ID", "ACASUS", "@value", "ACAS00000188", "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("ACAS_US", "ACAS_US", "ACAS System Configuration", "Event Type", "Event Type", "6H")).Return("SHL").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
      };

      List<string> exclusionXpaths = new List<string>();
      exclusionXpaths.Add("/*[local-name()='UniversalInterchangeInclude']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventTime']");
      ICompare comparer = new ExcludingComparer(exclusionXpaths);

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer, extensionObjects);
      mapTester.Execute<Transforms.PSN2UInterchangeInclude_ACAS_US.PSN2UInterchangeInclude_ACAS_US>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
