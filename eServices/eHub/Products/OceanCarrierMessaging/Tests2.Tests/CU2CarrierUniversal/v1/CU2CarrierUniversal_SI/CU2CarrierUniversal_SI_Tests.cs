using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CarrierUniversal.v1;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CU2CarrierUniversal_SI_Tests
  {
    const string filePath = "CU2CarrierUniversal.v1.CU2CarrierUniversal_SI.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2CarrierUniversal_SI()
    {
      //AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "ECU", "ECULINE");
      //AssertMapping("Test2_AGT_input.xml", "Test2_AGT_output.xml", "ECU", "ECULINE");
      //AssertMapping("Test3_input.xml", "Test3_output.xml", "ECU", "ECULINE");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "ECU", "ECULINE");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierCode, string carrierName)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var mappedCarrierCode = carrierCode + "CODE";
      var mappedCarrierName = carrierCode + "NAME";
      var mappedCarrierMsg = carrierCode + "MSG";
      var destinationParty = carrierName + "_SI";

      var index = destinationParty.IndexOf("_");
      var serviceProvider = index >= 0
        ? destinationParty.Substring(0, index)
        : destinationParty;

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", mappedCarrierName)).Return("ECU").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ECU_ECULINE_SI_12345")).Repeat.Any();
      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("123").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return(mappedCarrierCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(mappedCarrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(mappedCarrierMsg).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierCode).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + mappedCarrierName, "@maxlength", "14")).Return("12345").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "", "@ST_ID", mappedCarrierMsg, "@value", "C00001000", "@referenceType", "JobNumber")).Return("000011").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", "C00001000", "@referenceType", "JobNumber")).Return("000011").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "123", "12345"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "ForwardingConsol", "ForwardingType")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00001000", "4.0.0", "FormVersion")).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "ECU", serviceProvider, "000011", "C00001000")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.ExecuteCompiled<CU2CarrierUniversal_SI>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}

