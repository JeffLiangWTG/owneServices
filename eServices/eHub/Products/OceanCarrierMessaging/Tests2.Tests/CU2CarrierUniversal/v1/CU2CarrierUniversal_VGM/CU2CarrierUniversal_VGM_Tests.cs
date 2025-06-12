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
  public class CU2CarrierUniversal_VGM_Tests
  {
    const string filePath = "CU2CarrierUniversal.v1.CU2CarrierUniversal_VGM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCU2CarrierUniversal_VGM()
    {
      AssertMapping("Test1_CLD_input.xml", "Test1_CLD_output.xml", "C00001001", "ECU", "ECULINE");
      AssertMapping("Test2_AGT_input.xml", "Test2_AGT_output.xml", "C00001001", "ECU", "ECULINE");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "C00001001", "ECU", "ECULINE");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string consolID, string carrierCode, string carrierName)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      var destinationParty = carrierName + "_VM";
      var mappedCarrierName = carrierCode + "NAME";
      var mappedCarrierMsg = carrierCode + "MSG";

      var index = destinationParty.IndexOf("_");
      var serviceProvider = index >= 0
        ? destinationParty.Substring(0, index)
        : destinationParty;

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", mappedCarrierName)).Return("ECU").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ECU_ECULINE_VM_12345")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(mappedCarrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(mappedCarrierMsg).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + mappedCarrierName, "@maxlength", "14")).Return("12345").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", consolID + "_CONT1111111", "@referenceType", "JobNumber")).Return("CON0000001234").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "CON0000012345", consolID + "_CONT1111111", "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "CONT1111111_" + consolID, "CON0000012345", "JobNumber")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "12345", consolID, "InterchangeNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "Verified Gross Container Weight", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "ForwardingConsol", "ForwardingType"));

      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("123").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "123", "12345"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", consolID, "@referenceType", "PartyType")).Return("").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "ShippingLine", "PartyType"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", consolID, "2.0", "FormVersion")).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.ExecuteCompiled<CU2CarrierUniversal_VGM>(input, expectedOutput);

      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}

