using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierEmail;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierEmail_CarrierUniversal2IFTMBF_Tests
  {
    const string filePath = "CarrierEmail.CarrierUniversal2IFTMBF.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF()
    {
      //add genset case
      AssertMapping("Test1_input.xml", "Test1_output.xml", "YANGMING", "YMLA", multiPickupDelivery: "FALSE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test1_input.xml", "Test1_output_without_EDIFACT.xml", "YANGMING", "YMLA", multiPickupDelivery: "FALSE", useEDIAttachment: "N", carrierCode: "EVEMARMEL5");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", "YANGMING", "YMLA", multiPickupDelivery: "TRUE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test1_input.xml", "Test1_output_bis.xml", "YANGMING", "YMLA", partySenderIdentifier: "", carrierCode: "EVEMARMEL5");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "YANGMING", "YMLA", payableElseWhere: "A", orgPort: "BEANR", carrierCode: "EVEMARMEL5");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "YANGMING", "YMLA", carrierCode: "EVEMARMEL5");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "YANGMING", "YMLA", carrierCode: "EVEMARMEL5");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "YANGMING", "YMLA", isSummary: "TRUE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test6_input_CoLoad.xml", "Test6_output_CoLoad.xml", "YANGMING", "YMLA", shipmentType: "CLD", multiPickupDelivery: "FALSE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test6_input_CoLoad.xml", "Test6_output_CoLoad_MultiPickupDropOff.xml", "YANGMING", "YMLA", shipmentType: "CLD", multiPickupDelivery: "TRUE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test7_CLD_input.xml", "Test7_CLD_output.xml", "YANGMING", "BLAA", shipmentType: "CLD", multiPickupDelivery: "FALSE", orgPort: "AUCAR", carrierCode: "ACARRICAR");
      AssertMapping("Test7_CLD_input.xml", "Test7_CLD_output_MultiPickupDropOff.xml", "YANGMING", "BLAA", shipmentType: "CLD", multiPickupDelivery: "TRUE", orgPort: "AUCAR", carrierCode: "ACARRICAR");
      AssertMapping("Test8_LCL_CLD_input.xml", "Test8_LCL_CLD_output.xml", "YANGMING", "BLAA", shipmentType: "CLD", orgPort: "AUCAR", carrierCode: "ACARRICAR");

      AssertMapping("CMACGM.Test1_input.xml", "CMACGM.Test1_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", numberformat: "TMP=3", orgPort: "AUSYD", carrierCode: "CMACGM_AU");
      AssertMapping("CMACGM.Test2_input.xml", "CMACGM.Test2_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", orgPort: "AUSYD", carrierCode: "CMACGM_AU");
      AssertMapping("CMACGM.Test3_input.xml", "CMACGM.Test3_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", orgPort: "AUSYD", carrierCode: "CMACGM_AU");
      AssertMapping("CMACGM.Test4_input.xml", "CMACGM.Test4_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", orgPort: "AUSYD", carrierCode: "CMACGM_AU");
      AssertMapping("CMACGM.Test5_input.xml", "CMACGM.Test5_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", orgPort: "AUSYD", carrierCode: "CMACGM_AU");
      AssertMapping("CMACGM.Test6_input.xml", "CMACGM.Test6_output.xml", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", isSummary: "TRUE", orgPort: "AUSYD", carrierCode: "CMACGM_AU");

      AssertMapping("EVG.Test1_input.xml", "EVG.Test1_output.xml", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", carrierCode: "EVEMARMEL5");
      AssertMapping("EVG.Test2_input.xml", "EVG.Test2_output.xml", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", carrierCode: "EVEMARMEL5");
      AssertMapping("EVG.Test3_input.xml", "EVG.Test3_output.xml", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", carrierCode: "EVEMARMEL5");
      AssertMapping("EVG.Test4_input.xml", "EVG.Test4_output.xml", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", carrierCode: "EVEMARMEL5");
      AssertMapping("EVG.Test5_input.xml", "EVG.Test5_output.xml", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", isSummary: "TRUE", carrierCode: "EVEMARMEL5");

      AssertMapping("Test9_input.xml", "Test9_output.xml", "YANGMING", "YMLA", partySenderIdentifier: "", carrierCode: "EVEMARMEL5");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "YANGMING", "YMLA", partySenderIdentifier: "", isSummary: "TRUE", multipleMainLegs: "false", carrierCode: "EVEMARMEL5");
      AssertMapping("Test11_input_TransportModes.xml", "Test11_output_TransportModes.xml", "YANGMING", "YMLA", carrierCode: "EVEMARMEL5");
      AssertMapping("Test12_IsOutOfGauge_input.xml", "Test12_IsOutOfGauge_output.xml", "YANGMING", "YMLA", multiPickupDelivery: "FALSE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test13_IsOutOfGaugeBeFalse_input.xml", "Test13_IsOutOfGaugeBeFalse_output.xml", "YANGMING", "YMLA", multiPickupDelivery: "FALSE", carrierCode: "EVEMARMEL5");
      AssertMapping("Test14_GroupingMethod_input.xml", "Test14_GroupingMethod_output.xml", "YANGMING", "MSCU", shipmentType: "AGT", orgPort: "AUSYD");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_CannotFindUNB3()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:YANGMING_BK1], [SCAC:YMLA])";
      AssertMappingException("Test1_input.xml", exceptionMessage, "YANGMING", "YMLA", destinationPartyReceiverIdentifier: "");
      AssertMappingException("Test1_input.xml", exceptionMessage, "YANGMING", "YMLA", partySenderIdentifier: "", destinationPartyReceiverIdentifier: "");

      var exceptionCMAMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:CMACGM_BK1], [SCAC:CMDU])";
      AssertMappingException("CMACGM.Test1_input.xml", exceptionCMAMessage, "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", destinationPartyReceiverIdentifier: "");
    }

    Dictionary<string, object> SetupMappingExtensions(string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "", string multipleMainLegs = "true", string multiPickupDelivery = "FALSE", string payableElseWhere = "", string numberformat = "", string orgPort = "AUMEL", string useEDIAttachment = "Y", string carrierCode = "")
    {
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();
      var mockCarrierEmailHelper = MockRepository.GenerateStrictMock<CarrierEmailHelper>();

      var destinationParty = carrierName + "_BK1";
      var destinationPartyMappingName = string.Format("{0} Provider Configuration", "CARRIER_EMAIL");

      var index = destinationParty.IndexOf("_", StringComparison.Ordinal);
      var serviceProvider = index >= 0
                          ? destinationParty.Substring(0, index)
                          : destinationParty;

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Multiple Main Legs", "Enable", destinationParty)).Return(multipleMainLegs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Number Format", "Format", destinationParty, SCAC)).Return(numberformat).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.BGM", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00678601", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00678601")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_3"));
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLID", "CARRIER_EMAIL", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "3", "C00678601"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "EML0000000003", "C00678601", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "EML0000000003", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "Booking Request", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", shipmentType, "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00678601", "3.0.0", "FormVersion")).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", destinationParty, SCAC)).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", destinationParty, SCAC)).Return(partySenderIdentifier);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, SCAC)).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "CMDA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "EGLA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "INTD")).Return("INTT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "")).Return("").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "Package Type", "CARRIER_EMAIL" + " Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "Package Type", "CARRIER_EMAIL" + " Code", "PKG")).Return("P_O").Repeat.Any();
      // iso
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "Package Type", "CARRIER_EMAIL" + " Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      //default
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "Package Type", "CARRIER_EMAIL" + " Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "48K8")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "")).Return("").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode(serviceProvider)).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription(serviceProvider)).Return(payableElseWhere).Repeat.Any();      

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", destinationParty)).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARRIER_EMAIL", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARRIER_EMAIL", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARRIER_EMAIL", "xxx")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARRIER_EMAIL", "xxx")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARRIER_EMAIL", "yyy")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARRIER_EMAIL", "yyy")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARRIER_EMAIL", "GEN")).Return("SSR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARRIER_EMAIL", "GEN")).Return("FGE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (BK)", "Use HQ Email", orgPort, SCAC)).Return("Y").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (BK)", "EDI Attachment", orgPort, SCAC)).Return(useEDIAttachment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (BK)", "HQ Email Address", orgPort, SCAC)).Return("carrier@carrier.com").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "carrier@carrier.com"));
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("dd-MM-yyyyTHH:mm:ss")).Return("23-01-2024T11:44:33").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "OCM BE", "URL", carrierName)).Return("https://au2sp-socm-401.sand.wtg.zone:5002/v1/bookingrequest/update").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", destinationPartyMappingName, "OCM BE", "SecretKey", carrierName)).Return("12345678901234567890123456789012").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", carrierName, "EML0000000003", "C00678601")).Repeat.Any();

      var mockCarrierEmailToken = "mBunMWuXlR5olsg2W68Jk4PizVSderW/FR/qkOVSry081m1bqNgUyrM8z1AR5pHMgbD7EnmqP5TkPG2J1XQod2vUdT6lov2c66aU/EygwWFvr8rXNIiEyYWsQOvbtqcvLhwA3FP+5I3TD44VaGceeg==";
      mockCarrierEmailHelper.Expect(x => x.GenerateToken("12345678901234567890123456789012", "BR", "EML0000000003")).Return(mockCarrierEmailToken).Repeat.Any();
      mockOCMHelper.Expect(x => x.InsertClientRegistration("TESTSENDER__1", "CarrierEmail_Token", "OCMBE", mockCarrierEmailToken, 0, "", "EML0000000003", ""));

      return new Dictionary<string, object>
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/CarrierEmailHelper", mockCarrierEmailHelper }
      };
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "", string multipleMainLegs = "true", string multiPickupDelivery = "FALSE", string payableElseWhere = "", string numberformat = "", string orgPort = "AUMEL", string useEDIAttachment = "Y", string carrierCode = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(carrierName, SCAC, partySenderIdentifier, destinationPartyReceiverIdentifier, isSummary, shipmentType, multipleMainLegs, multiPickupDelivery, payableElseWhere, numberformat, orgPort, useEDIAttachment, carrierCode);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMBF>(input, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<IFTMBFEnvelope>(expectedOutput, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
        {
          "datatype 'String' - The actual length is less than the MinLength value.",
          "C21501",  //SealParty
          "DGS05",
          "C524",
          "EQDLoop1",
          "C53601",
          "NAD"
        };
      }
    }

    void AssertMappingException(string inputFile, string exceptionMessage, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "")
    {
      var input = filePath + inputFile;

      var extensionObjects = SetupMappingExtensions(carrierName, SCAC, partySenderIdentifier, destinationPartyReceiverIdentifier, isSummary, shipmentType);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<CarrierUniversal2IFTMBF>(input, exceptionMessage);
    }
  }
}