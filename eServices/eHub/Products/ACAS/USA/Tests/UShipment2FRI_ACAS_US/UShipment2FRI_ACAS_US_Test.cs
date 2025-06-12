using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class UShipment2FRI_ACAS_US_Test
  {
    const string filePath = "UShipment2FRI_ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2FRI_ACAS_US()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "ACAS_US_FRI_TST");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ACAS_US_FRI");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "ACAS_US_FRI");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "ACAS_US_FRI");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "ACAS_US_FRI");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;
      var providerID = destinationParty.Contains("TST") ? "ACAS_US_TST" : "ACAS_US";

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER_1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ACAX")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ACAS_US")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_1", "BNE", "ACAS_US")).Return("ACAS_US").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_1", "BNE", "ACAX")).Return("ACAS_US").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.US.Transforms.ACAS_US", "@maxlength", "14")).Return("29");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASID", providerID, "TESTSENDER_1", "ACAX")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASID", providerID, "TESTSENDER_1", "ACAS_US")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "S00098873")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "S00098873", "ACAS0000000029")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "S00098873", "ShipmentId")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAXWAYBILLS0000", "ACAS0000000029", "FRI-HWB")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS_USWAYBILLS0000", "ACAS0000000029", "FRI-HWB")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS_US123-12345678", "ACAS0000000029", "FRI-MAWB")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", providerID, "@recipientId", "TESTSENDER_1", "@ST_ID", "ACASUS", "@value", "S00098873")).Return("");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "S00098873", "ACAS Shipment Report", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "S00098873", "ForwardingShipment", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "S00098873", "Shipment", "SubMessageType")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "ACAS Shipment Report", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "HVLVConsignment", "ForwardingType")).Repeat.Any();
	  mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "ForwardingShipment", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "ACAS0000000029", "Shipment", "SubMessageType")).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("A6A226C5-F0EE-4FBB-BD60-12448A6A5A47").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerID, "TESTSENDER_1", "A6A226C5-F0EE-4FBB-BD60-12448A6A5A47", "29")).Repeat.Any();


      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Transforms.UShipment2FRI_ACAS_US.UShipment2FRI_ACAS_US>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
