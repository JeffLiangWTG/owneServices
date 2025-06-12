using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ACAS.BR.Transforms.XFNM2UInterchangeInclude_CCT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class XFNM2UInterchangeInclude_CCT_Test
  {
    const string filePath = "XFNM2UInterchangeInclude_CCT.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestXFNM2UInterchangeInclude_CCT()
    {
      AssertMapping("Test1_MPP_input.xml", "Test1_MPP_output.xml", "ForwardingConsol", "AMD");
      AssertMapping("Test2_MAA_input.xml", "Test2_MAA_output.xml", "ForwardingShipment", "AMD");
      AssertMapping("Test3_MRJ_input.xml", "Test3_MRJ_output.xml", "ForwardingConsol", "AMD");
      AssertMapping("Test4_MPP_input.xml", "Test4_MPP_output.xml", "ForwardingConsol", "AMD");
      AssertMapping("Test5_MRJ_MultipleErrors_input.xml", "Test5_MRJ_MultipleErrors_output.xml.xml", "ForwardingConsol", "AMD");
      AssertMapping("Test6_MWA_input.xml", "Test6_MWA_output.xml", "ForwardingShipment", "WTH");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribedForwardingType, string subscribedActionType)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ACAS_BR").Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOWISE").Repeat.AtLeastOnce();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "ACAS_BR", "@ST_ID", "ACASBR", "@value", "ZB00000001")).Return("WISETECH").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "WISETECH", "@ST_ID", "ACASBR", "@value", "SH093848758_ZB00000001", "@referenceType", "ShipmentId")).Return("C000010022").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "WISETECH", "@ST_ID", "ACASBR", "@value", "SH093848758_ZB00000001", "@referenceType", "DocumentName")).Return("CCT Shipment Report").Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "WISETECH", "@ST_ID", "ACASBR", "@value", "SH093848758_ZB00000001", "@referenceType", "ForwardingType")).Return(subscribedForwardingType).Repeat.Once();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "WISETECH", "@ST_ID", "ACASBR", "@value", "SH093848758_ZB00000001", "@referenceType", "FHL-HWB")).Return("123-1234567").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "WISETECH", "@ST_ID", "ACASBR", "@value", "SH093848758_ZB00000001", "@referenceType", "ActionPurpose")).Return(subscribedActionType).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "WISETECH", "20190913084815866", "SH093848758_ZB00000001", "ProtocolNumber")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<XFNM2UInterchangeInclude_CCT>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
