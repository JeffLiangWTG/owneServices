using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ACAS.MX.VUCEM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.MX.Tests
{
  [TestClass]
  public class XFNM2UInterchangeInclude_Test
  {
    const string filePath = "VUCEM.XFNM2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestXFNM2UInterchangeInclude()
    {
      AssertMapping("Test1_MAA_input.xml", "Test1_MAA_output.xml", "ForwardingConsol");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribedForwardingType)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_MX").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "ACAS_MX", "@ST_ID", "ACASMX", "@value", "123456")).Return("WISETECH").Repeat.Once();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_MX", "@recipientId", "", "@ST_ID", "ACASMX", "@value", "123456", "@referenceType", "ShipmentId")).Return("C03078216").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_MX", "@recipientId", "", "@ST_ID", "ACASMX", "@value", "123456", "@referenceType", "DocumentName")).Return("XFZB").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_MX", "@recipientId", "", "@ST_ID", "ACASMX", "@value", "123456", "@referenceType", "ForwardingType")).Return(subscribedForwardingType).Repeat.Once();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<XFNM2UInterchangeInclude>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
