using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2OCMBR;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2OCMBR_Tests
  {
    const string filePath = "CarrierUniversal2OCMBR.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2OCMBR()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "ODS", "ODS");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "ODS", "ODS", payableElseWhere: "ElseWhere");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "ODS", "ODS");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "ODS", "ODS");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "ODS", "ODS");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "ODS", "ODS");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "ODS", "ODS", payableElseWhere: "ElseWhere");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "ODS", "ODS");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierCode, string carrierName, string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var mappedCarrierCode = carrierCode + "CODE";
      var mappedCarrierName = carrierCode + "NAME";
      var mappedCarrierId = carrierCode + "ID";
      var mappedCarrierMsg = carrierCode + "MSG";
      var mappedCarrierBrs = carrierCode + "BRS";
      var destinationParty = carrierName + "_BK1";
      var mappingId = "OCMCargowise";
      var mappingDescription = "OCM Cargowise System Configuration";
      var internalTrackingID = "666";
      var sourceParty = "CARGOWISE";

      var index = destinationParty.IndexOf("_");
      var serviceProvider = index >= 0
        ? destinationParty.Substring(0, index)
        : destinationParty;

      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceParty);
      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Stub(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(internalTrackingID);
      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "BR_ODYBN1_100")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(mappedCarrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return(mappedCarrierId).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(mappedCarrierMsg).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", serviceProvider)).Return(mappedCarrierBrs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierCode).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + mappedCarrierName, "@maxlength", "14")).Return("100");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2OCMBR", "@maxlength", "14")).Return("100");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", sourceParty, "@ST_ID", mappedCarrierMsg, "@value", "C00001003", "@referenceType", "OCMBR")).Return("");

      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, "ODSNAME Provider Configuration", "ContainerTypeToISOCode", "ODSNAME Code", "45G0")).Return("45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, "ODSNAME Provider Configuration", "ContainerTypeToISOCode", "ODSNAME Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, "ODSNAME Provider Configuration", "ContainerTypeToISOCode", "ODSNAME Code", "45H0")).Return("45H0").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, "ODSNAME Provider Configuration", "Package Type", "ODSNAME Code", "PCE")).Return("P2").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, "ODSNAME Provider Configuration", "Package Type", "ODSNAME Code", "PLT")).Return("P1").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("false").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("true").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "C00001003", "OCMBR"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "C00001003", "ODS0000000100", "OCMBR"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, internalTrackingID, "100"));
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(sourceParty, "BN1", mappedCarrierName)).Return("ODYBN1").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierId, mappedCarrierName, sourceParty, "ODYBN1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "WTH", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "Booking Request", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, sourceParty, "ODS0000000100", "1.0.0", "FormVersion")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "1")).Return("EIN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", destinationParty)).Return("true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "US", "1")).Return("USSSSSS").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "BR", "BR", "1")).Return("CJN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "BR", "BR", "1")).Return("BRRRRRR").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "BR", "US", "1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "BR", "1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "BR", "GB", "1")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "BR", "GB", "2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "GB", "1")).Return("").Repeat.Any();


      mockCodeMapper.Expect(x => x.GetRecipientCode(mappingId, mappingId, mappingDescription, "MessageParty", "SenderID", destinationParty)).Return("K1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappingId, mappingId, mappingDescription, "MessageParty", "RecipientID", destinationParty)).Return("K2").Repeat.Any();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss")).Return("2019-08-21T23:05:38").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode(serviceProvider)).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription(sourceParty, "ODYBN1", serviceProvider, "ODS0000000100", "C00001003")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2OCMBR>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}