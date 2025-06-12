using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GCT.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GCT.Tests
{
  [TestClass]
  public class Acknowledgement2UInterchangeEnvelopeTest
  {
    const string filePath = "Acknowledgement2UInterchangeEnvelope.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void Acknowledgement2UInterchangeEnvelope()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_Discard_input.xml", "Test3_Discard_output.xml");
      AssertMapping("Test4_Discard_input.xml", "Test4_Discard_output.xml");
    }

    private void AssertMapping(string inputFile, string expectedOutputFile, string expectedUNB = "UNB_SCAC")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GTNEXUS");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "GTNEXUS", "@value", "GTN0000000402", "@ST_ID", "GTNMSG")).Return("HYEDAUTST");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GTNEXUS", "@recipientId", "HYEDAUTST", "@ST_ID", "GTNMSG", "@value", "GTN0000000402")).Return("C00001072");
      mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2018-02-20T09:54:30");

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "GTNEXUS", "HDMU")).Return("Mapped_NADCA_SCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMCARGOWISE", "OCMCARGOWISE", "OCM Cargowise System Configuration", "SCAC (Inbound)", "SCAC", "GTNEXUS", "EMPTY_SCAC")).Return("").Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(expectedUNB).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://cargowise.com/Core/Transforms/Helper/ContextAccessor", mockContextAccessor },
        { "http://cargowise.com/Core/Transforms/Helper/CodeMapper", mockCodeMapper },
        { "http://cargowise.com/Core/Transforms/Helper/DateMapper", mockDateMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<Acknowledgement2UInterchangeEnvelope>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
