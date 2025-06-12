using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
    public class UShipment2FHL_ACAS_US_Test
  {
    const string filePath = "UShipment2FHL_ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2FHL_ACAS_US()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "ACAS_US_FHL_TST", "S00001265");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ACAS_US_FHL", "ACAS0000000029");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty, string consolRef)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;
      var providerId = destinationParty.Contains("TST") ? "ACAS_US_TST" : "ACAS_US";

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER_1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_1", "BNE", "ACAS_US")).Return("ACAS_US");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.US.Transforms.ACAS_US", "@maxlength", "14")).Return("29");

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", providerId, "@recipientId", "TESTSENDER_1", "@ST_ID", "ACAS_US", "@value", "S00001265")).Return("");

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", "29", "S00001265")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", "S00001265", consolRef)).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", "ACAS_USABC12345678", consolRef, "FHL-HWB")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", consolRef, "S00001265", "ShipmentId")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", consolRef, "ACAS Shipment Report", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", consolRef, "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", consolRef, "Shipment", "SubMessageType")).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("D939C3E0-7C75-47D0-A176-1CD88FCFB55F").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASUS", providerId, "TESTSENDER_1", "D939C3E0-7C75-47D0-A176-1CD88FCFB55F", "29")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<Transforms.UShipment2FHL_ACAS_US.UShipment2FHL_ACAS_US>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
