using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class UShipment2ASN_ACAS_US_Test
  {
    const string filePath = "UShipment2ASN_ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2ASN_ACAS_US()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "ACAS_US_ASN_TST");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ACAS_US_ASN");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;
      var providerID = destinationParty.Contains("TST") ? "ACAS_US_TST" : "ACAS_US";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER_1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASID", providerID, "TESTSENDER_1", "ACAXXX")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASID", providerID, "TESTSENDER_1", "ACAS_ASN")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_1", "BNE", "ACAS_US")).Return("ACAS_ASN").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.US.Transforms.ACAS_US", "@maxlength", "14")).Return("29");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", providerID, "@recipientId", "TESTSENDER_1", "@ST_ID", "ACASUS", "@value", "S00001265")).Return("ACAS00000011");

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("A6A226C5-F0EE-4FBB-BD60-12448A6A5A47").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "A6A226C5-F0EE-4FBB-BD60-12448A6A5A47", "29")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Transforms.UShipment2ASN_ACAS_US.UShipment2ASN_ACAS_US>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
