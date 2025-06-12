using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.X12_301_2CU_CAROTRANS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;


namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class X12_301_2CU_CAROTRANS_Tests
  {
    const string filePath = "X12_301_2CU_CAROTRANS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void Test_X12_301_2CU_CAROTRANS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "A", "MAA", "Booking Confirmed");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "U", "MRR");
      AssertMapping("Test3_input_CoLoad.xml", "Test3_output_CoLoad.xml", "U", "MRR", shipmentType: "CLD");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "A", "MAA", "Booking Confirmed");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "A", "MAA", "Booking Confirmed");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string eventCode, string eventType, string eventReference = "", string actionPurpose = "", string subscribedConsolRef = "", string shipmentType = "ABC")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Booking Confirmation X12_301 from CAROTRANS", "EventType", "EventType", eventCode, actionPurpose)).Return(eventType);
      mockCodeMapper.Expect(x => x.GetRecipientCode("CAROTRANS", "CAROTRANS", "Booking Confirmation X12_301 from CAROTRANS", "EventType", "Reference", eventCode, actionPurpose)).Return(eventReference);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "CW0000000019", "@referenceType", "JobNumber")).Return(subscribedConsolRef == "" ? "C000093833" : subscribedConsolRef);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "CW0000000019", "@referenceType", "ShipmentType")).Return(shipmentType);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRMSG", "@value", "C000093833", "@referenceType", "ActionPurpose")).Return(actionPurpose);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CAROTRANS", "@recipientId", "", "@ST_ID", "CTRBRS", "@value", "C000093833")).Return("");

      mockOCMHelper.Expect(x => x.IsCoLoad("ABC")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<X12_301_2CU_CAROTRANS>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
