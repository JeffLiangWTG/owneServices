using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF_MAERSK;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMBF_MAERSK_Tests
  {
    const string filePath = "CarrierUniversal2IFTMBF_MAERSK.TestFiles.";

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_Maersk()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "MCCQ", "CARGOWISE", "MCCQU", payableElseWhere: "A");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "SAFM", "CARGOWISE", "SAFM");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "SEAU", "CARGOWISE", "SEAU");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "SEJJ", "CARGOWISE", "SEJJ");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "MAEU", "CARGOWISE", "MAEU", isSummary: "TRUE");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "MAEU", "CARGOWISE", "MAEU", isSummary: "TRUE");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test11_input.xml", "Test11_output.xml", "MAEU", "CARGOWISE", "MAEU", isSummary: "TRUE");
      AssertMapping("Test12_input_TransportModes.xml", "Test12_output_TransportModes.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test13_input_coload.xml", "Test13_output_coload.xml", "SAFM", "CARGOWISE", "SAFM");
      AssertMapping("Test14_input.xml", "Test14_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test15_input.xml", "Test15_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test16_input.xml", "Test16_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test17_input.xml", "Test17_output.xml", "MCCQ", "CARGOWISE", "MCCQU");
      AssertMapping("Test18_IsOutOfGaugeBeFalse_input.xml", "Test18_IsOutOfGaugeBeFalse_output.xml", "MCCQ", "CARGOWISE", "MCCQU");
      AssertMapping("Test19_SHP_GroupingMethod_input.xml", "Test19_SHP_GroupingMethod_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test20_FlatContainerQuality_input.xml", "Test20_FlatContainerQuality_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test21_DNG_GroupingMethod_NoShipmentsAttached_input.xml", "Test21_DNG_GroupingMethod_NoShipmentsAttached_output.xml", "MAEU", "CARGOWISE", "MAEU");
      AssertMapping("Test22_DNG_GroupingMethod_input.xml", "Test22_DNG_GroupingMethod_output.xml", "MAEU", "CARGOWISE", "MAEU");
    }

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_MAERSK_CannotFindUNB3_PartyReceiverID()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("Test1_input.xml", "Test1_output.xml", "MAEU", "MAEU", "");
      }
      catch (Exception ex)
      {
        isThrownException = true;
        var actualException = ex.InnerException ?? ex;
        if (actualException is ArgumentException)
        {
          Assert.AreEqual("Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:MAERSK_BK], [SCAC:MAEU])", actualException.Message.Trim());
        }
        else
        {
          throw;
        }
      }
      Assert.IsTrue(isThrownException);
    }

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_MAERSK_CannotFindUNB3_PartySenderID()
    {
      var isThrownException = false;
      try
      {
        AssertMapping("Test1_input.xml", "Test1_output.xml", "MAEU", "", "MAEU");
      }
      catch (Exception ex)
      {
        isThrownException = true;
        var actualException = ex.InnerException ?? ex;
        if (actualException is ArgumentException)
        {
          Assert.AreEqual("Could not found matching PartySenderID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:MAERSK_BK], [SCAC:MAEU])", actualException.Message.Trim());
        }
        else
        {
          throw;
        }
      }
      Assert.IsTrue(isThrownException);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string SCAC, string partySenderIdentifier, string partyReceiverIdentifier, string isSummary = "FALSE", string multiPickupDelivery = "FALSE", string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TestSender");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MAERSK_BK");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TestSender", "BN1", "MAERSK")).Return("HYEBNEBN1").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TestSender", "BNE", "MAERSK")).Return("HYEBNEBN1").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TestSender", "SHA", "MAERSK")).Return("HYEBNEBN1").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.MAERSK.UNH1", "@maxlength", "14")).Return("23");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.MAERSK.BGM", "@maxlength", "14")).Return("23").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "HYEBNEBN1")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "C00001003", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CODS0000682775", "AMD", "ActionPurpose")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "C00001003")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "CEBS0000682392")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000023", "CODS0000682775", "IFTMBF")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CODS0000682775", "MAE0000000023", "IFTMBF")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "CODS0000682775")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "C00001003", "23", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392", "23", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000023", "23", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000023", "C00001003_IFTMBF")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "C00001003_IFTMBF", "MAE0000000023")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000023", "CEBS0000682392_IFTMBF")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392_IFTMBF", "MAE0000000023")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "C00001003", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "CEBS0000682392", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "CEBS0000682392", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "CODS0000682775", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "Booking Request", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "C00001003", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "C00001003", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "23")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "MAERSK_BK", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "MAERSK_BK", "MAEU")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "MAERSK_BK", "MSCU")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "MAERSK_BK", "CMDU")).Return(SCAC).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "MAERSK_BK", SCAC)).Return(partyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", "MAERSK_BK", SCAC)).Return(partySenderIdentifier).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "MAERSK", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "MAERSK", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "MAERSK", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "MAERSK", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "MAERSK", "GEN")).Return("SSR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "MAERSK", "GEN")).Return("FGE").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_HYEBNEBN1_23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "MAERSK", "@recipientId", "TestSender", "@ST_ID", "MAEMSG", "@value", "C00001003", "@referenceType", "IFTMBF")).Return("C00001003").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "MAERSK", "@recipientId", "TestSender", "@ST_ID", "MAEMSG", "@value", "CEBS0000682392", "@referenceType", "IFTMBF")).Return("CEBS0000682392").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "MAERSK", "@recipientId", "TestSender", "@ST_ID", "MAEMSG", "@value", "CODS0000682775", "@referenceType", "IFTMBF")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PCE")).Return("PCE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PKG")).Return("PKG").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "20G0")).Return("20G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45G0")).Return("45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45R3")).Return("45R3").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "20U1")).Return("20U1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "42P1")).Return("42P1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TestSender", "MAERSK_BK1")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("MAERSK_BK")).Return("MAERSK").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("MAERSK")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TestSender", "HYEBNEBN1", "MAERSK", "C00001003", "C00001003")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TestSender", "HYEBNEBN1", "MAERSK", "CEBS0000682392", "CEBS0000682392")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TestSender", "HYEBNEBN1", "MAERSK", "MAE0000000023", "CODS0000682775")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/codeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/dateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/contextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/dataModelAccessor", mockDataModelAccessor},
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMBF_MAERSK>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<EFACT_D99B_IFTMBF>(expectedOutput, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
        {
          "datatype 'String' - The actual length is less than the MinLength value.",
          "datatype 'String' - The actual length is greater than the MaxLength value.",
          "C21501"  //SealParty
        };
      }
    }
  }
}