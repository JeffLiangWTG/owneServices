using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Schemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.Schemas.OCM;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2OCMSI;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2OCMSI_Tests
  {
    const string filePath = "CarrierUniversal2OCMSI.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2OCMSI()
    {
      AssertMapping("Test1_AGT_input.xml", "Test1_AGT_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "CEBS0000682392_ODY0000000012");
      AssertMapping("Test2_CLD_CY_CY_input.xml", "Test2_CLD_CY_CY_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "CEBS0000682392_ODY0000000012", payableElseWhere: "ElseWhere");
      AssertMapping("Test3_CLD_CFS_CFS_input.xml", "Test3_CLD_CFS_CFS_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "");
      AssertMapping("Test4_CLD_CFS_CFS_input.xml", "Test4_CLD_CFS_CFS_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "");
      AssertMapping("Test5_GROUP_input.xml", "Test5_GROUP_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "ODYSSEY", "ODY", "CEBS0000682392", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierName, string carrierPrefixCode, string shipmentNumber, string previousConsolReference, string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mappedCarrierCode = carrierName + " Code";
      var mappedCarrierId = carrierPrefixCode + "ID";
      var mappedCarrierMsg = carrierPrefixCode + "MSG";
      var mappedCarrierBrs = carrierPrefixCode + "BRS";
      var destinationParty = carrierName + "_SI1";
      var carrierMappingName = carrierName + " Provider Configuration";
      var mappingId = "OCMCargowise";
      var mappingDescription = "OCM Cargowise System Configuration";

      var sourceParty = "CARGOWISE";
      var clientID = carrierPrefixCode + "_ClientID";

      var subscribeShipmentRef = !string.IsNullOrEmpty(previousConsolReference)
                    ? previousConsolReference
                    : "ODY0000000012";

      var index = destinationParty.IndexOf("_");
      var serviceProvider = index >= 0
        ? destinationParty.Substring(0, index)
        : destinationParty;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "SI_ODY_ClientID_12")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "12"));
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", carrierName)).Return(clientID).Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss")).Return("2019-01-23T12:01:23").Repeat.Any();

      if (string.IsNullOrEmpty(previousConsolReference))
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, sourceParty, subscribeShipmentRef, "CEBS0000682392", "OCMSI")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, sourceParty, "CEBS0000682392", subscribeShipmentRef, "OCMSI")).Repeat.Any();
      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierId, carrierName, "TESTSENDER__1", clientID)).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "WTH", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, carrierName, "TESTSENDER__1", subscribeShipmentRef, "ForwardingConsol", "ForwardingType")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(mappingId, mappingId, mappingDescription, "MessageParty", "SenderID", destinationParty)).Return(carrierPrefixCode + "_SenderID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappingId, mappingId, mappingDescription, "MessageParty", "RecipientID", destinationParty)).Return(carrierPrefixCode + "_RecipientID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(carrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return(mappedCarrierId).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(mappedCarrierMsg).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", serviceProvider)).Return(mappedCarrierBrs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierPrefixCode).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "40G0")).Return("40G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("false").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "USLU")).Return("USLU").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", shipmentNumber, "@referenceType", "OCMSI")).Return(subscribeShipmentRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + carrierName, "@maxlength", "14")).Return("12").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierMappingName, "Package Type", mappedCarrierCode, "PLT")).Return("PT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierMappingName, "ContainerTypeToISOCode", mappedCarrierCode, "40G0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierMappingName, "ContainerTypeToISOCode", mappedCarrierCode, "22G0")).Return("20G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierMappingName, "ContainerTypeToISOCode", mappedCarrierCode, "40R0")).Return("40R0").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription(serviceProvider)).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "ODY_ClientID", serviceProvider, "CEBS0000682392_ODY0000000012", "CEBS0000682392")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "ODY_ClientID", serviceProvider, "ODY0000000012", "CEBS0000682392")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CarrierUniversal2OCMSI>(input, expectedOutput);

      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<ShippingInstruction_v1>(expectedOutput, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
            {
                "datatype 'String' - The actual length is less than the MinLength value.",
                "C21501"  //SealParty
            };
      }
    }
  }
}
