using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMBF_Tests
  {
    const string filePath = "CarrierUniversal2IFTMBF.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "YML", "YANGMING", "YMLA", multiPickupDelivery: "FALSE", expectedBGM03Code: "5");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", "YML", "YANGMING", "YMLA", multiPickupDelivery: "TRUE");
      AssertMapping("Test1_input.xml", "Test1_output.xml", "YML", "YANGMING", "YMLA", partySenderIdentifier: "", expectedBGM03Code: "5");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "YML", "YANGMING", "YMLA", payableElseWhere: "A");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "YML", "YANGMING", "YMLA", enableNADCB: "true");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "YML", "YANGMING", "YMLA");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "YML", "YANGMING", "YMLA", isSummary: "TRUE");
      AssertMapping("Test6_input_CoLoad.xml", "Test6_output_CoLoad.xml", "YML", "YANGMING", "YMLA", shipmentType: "CLD", multiPickupDelivery: "FALSE");
      AssertMapping("Test6_input_CoLoad.xml", "Test6_output_CoLoad_MultiPickupDropOff.xml", "YML", "YANGMING", "YMLA", shipmentType: "CLD", multiPickupDelivery: "TRUE");
      AssertMapping("Test7_CLD_input.xml", "Test7_CLD_output.xml", "YML", "YANGMING", "BLAA", shipmentType: "CLD", multiPickupDelivery: "FALSE");
      AssertMapping("Test7_CLD_input.xml", "Test7_CLD_output_MultiPickupDropOff.xml", "YML", "YANGMING", "BLAA", shipmentType: "CLD", multiPickupDelivery: "TRUE");
      AssertMapping("Test8_LCL_CLD_input.xml", "Test8_LCL_CLD_output.xml", "YML", "YANGMING", "BLAA", shipmentType: "CLD");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "YML", "YANGMING", "YMLA", partySenderIdentifier: "");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "YML", "YANGMING", "YMLA", partySenderIdentifier: "", isSummary: "TRUE", multipleMainLegs: "false", enableNADCB: "true", enableSGPWithContainerLinkFallback: "true");
      AssertMapping("Test11_input_TransportModes.xml", "Test11_output_TransportModes.xml", "YML", "YANGMING", "YMLA");
      AssertMapping("Test12_IsOutOfGauge_input.xml", "Test12_IsOutOfGauge_output.xml", "YML", "YANGMING", "YMLA", multiPickupDelivery: "FALSE");
      AssertMapping("Test13_IsOutOfGaugeBeFalse_input.xml", "Test13_IsOutOfGaugeBeFalse_output.xml", "YML", "YANGMING", "YMLA", multiPickupDelivery: "FALSE");
      AssertMapping("Test14_DNG_GroupingMethod_input.xml", "Test14_DNG_GroupingMethod_output.xml", "YML", "YANGMING", "MSCU", shipmentType: "AGT");
      AssertMapping("Test14_SHP_GroupingMethod_input.xml", "Test14_SHP_GroupingMethod_output.xml", "YML", "YANGMING", "MSCU", shipmentType: "AGT", enableSGPWithContainerLinkFallback: "true");
      AssertMapping("Test15_MultipleTDT20_input.xml", "Test15_output_MultipleTDT20_True.xml", "MSC", "MSC", "MSCU", shipmentType: "AGT", multipleMainLegs: "true", multipleMainLegsForAllSeaLegs: "true");
      AssertMapping("Test15_MultipleTDT20_input.xml", "Test15_output_MultipleTDT20_False.xml", "MSC", "MSC", "MSCU", shipmentType: "AGT", multipleMainLegs: "true", multipleMainLegsForAllSeaLegs: "false");
      AssertMapping("Test16_input.xml", "Test16_output.xml", "YML", "YANGMING", "MSCU", shipmentType: "AGT");
      AssertMapping("Test18_input_CHVW_IsSummaryFalse.xml", "Test18_output_CHVW_IsSummaryFalse.xml", carrierCode: "YML", carrierName: "YANGMING", SCAC: "CHVW", enableSGPWithContainerLinkFallback: "true");
      AssertMapping("Test19_input_CHVW_IsSummaryTrue.xml", "Test19_output_CHVW_IsSummaryTrue.xml", carrierCode: "YML", carrierName: "YANGMING", SCAC: "CHVW", isSummary: "TRUE", enableSGPWithContainerLinkFallback: "true");
      AssertMapping("Test20_SHP_CHVW_GroupingMethod_input.xml", "Test20_SHP_CHVW_GroupingMethod_output.xml", carrierCode: "YML", carrierName: "YANGMING", SCAC: "CHVW", shipmentType: "AGT", enableSGPWithContainerLinkFallback: "true");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_CMACGM()
    {
      AssertMapping("CMACGM.Test1_input.xml", "CMACGM.Test1_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", numberformat: "TMP=3");
      AssertMapping("CMACGM.Test2_input.xml", "CMACGM.Test2_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test3_input.xml", "CMACGM.Test3_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test4_input.xml", "CMACGM.Test4_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test5_input.xml", "CMACGM.Test5_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test6_input.xml", "CMACGM.Test6_output.xml", "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", isSummary: "TRUE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_EVG()
    {
      AssertMapping("EVG.Test1_input.xml", "EVG.Test1_output.xml", "EVG", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN");
      AssertMapping("EVG.Test2_input.xml", "EVG.Test2_output.xml", "EVG", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN");
      AssertMapping("EVG.Test3_input.xml", "EVG.Test3_output.xml", "EVG", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN");
      AssertMapping("EVG.Test4_input.xml", "EVG.Test4_output.xml", "EVG", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN");
      AssertMapping("EVG.Test5_input.xml", "EVG.Test5_output.xml", "EVG", "EVERGREEN", "EGLV", partySenderIdentifier: "EVERGREEN", isSummary: "TRUE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_CannotFindUNB3()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:YANGMING_BK1], [SCAC:YMLA])";
      AssertMappingException("Test1_input.xml", exceptionMessage, "YML", "YANGMING", "YMLA", destinationPartyReceiverIdentifier: "");
      AssertMappingException("Test1_input.xml", exceptionMessage, "YML", "YANGMING", "YMLA", partySenderIdentifier: "", destinationPartyReceiverIdentifier: "");

      var exceptionCMAMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:CMACGM_BK1], [SCAC:CMDU])";
      AssertMappingException("CMACGM.Test1_input.xml", exceptionCMAMessage, "CMA", "CMACGM", "CMDU", partySenderIdentifier: "CMACGM", destinationPartyReceiverIdentifier: "");
    }

    Dictionary<string, object> SetupMappingExtensions(string carrierCode, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING",
                                                      string isSummary = "FALSE", string shipmentType = "", string multipleMainLegs = "true", string multipleMainLegsForAllSeaLegs = "false",
                                                      string multiPickupDelivery = "FALSE", string payableElseWhere = "", string numberformat = "", string enableNADCB = "false", string expectedBGM03Code = "4", string enableSGPWithContainerLinkFallback = "false")
    {
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var mappedCarrierCode = carrierCode + "CODE";
      var mappedCarrierName = carrierCode + "NAME";
      var mappedCarrierId = carrierCode + "ID";
      var mappedCarrierMsg = carrierCode + "MSG";
      var destinationParty = carrierName + "_BK1";
      var destinationPartyMappingName = string.Format("{0} Provider Configuration", mappedCarrierName);
      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      var index = destinationParty.IndexOf("_", StringComparison.Ordinal);
      var serviceProvider = index >= 0
                          ? destinationParty.Substring(0, index)
                          : destinationParty;

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifierForContextProperty));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", destinationPartyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return(mappedCarrierId).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierId", serviceProvider)).Return(mappedCarrierCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(mappedCarrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return(mappedCarrierMsg).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return(carrierCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Features Control", "Enabled", serviceProvider, "NADCB")).Return(enableNADCB).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Multiple Main Legs", "Enable", destinationParty)).Return(multipleMainLegs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Multiple Main Legs", "All Sea Legs", destinationParty)).Return(multipleMainLegsForAllSeaLegs).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Number Format", "Format", destinationParty, SCAC)).Return(numberformat).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Message Function (BGM)", "FunctionCode", serviceProvider, "AMD")).Return(expectedBGM03Code).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Message Function (BGM)", "FunctionCode", serviceProvider, "ORG")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Message Function (BGM)", "FunctionCode", serviceProvider, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMBF", "OCMIFTMBF", "OCM IFTMBF Configuration", "Features Control", "Enabled", serviceProvider, SCAC, "SGPWithContainerLinkFallback")).Return(enableSGPWithContainerLinkFallback).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + mappedCarrierName + ".UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + mappedCarrierName + ".BGM", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", "C00678601", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", mappedCarrierName, "@recipientId", "TESTSENDER__1", "@ST_ID", mappedCarrierMsg, "@value", "C00678601")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", mappedCarrierName)).Return("CGWS").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_3"));
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", mappedCarrierName)).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierId, mappedCarrierName, "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "3", "C00678601"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", carrierCode + "0000000003", "C00678601", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", carrierCode + "0000000003", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "Booking Request", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", shipmentType, "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(mappedCarrierMsg, mappedCarrierName, "TESTSENDER__1", "C00678601", "3.0.0", "FormVersion")).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", destinationParty, SCAC)).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", destinationParty, SCAC)).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, SCAC)).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "CMDA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "EGLA")).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "INTD")).Return("INTT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "")).Return("").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", mappedCarrierName)).Return("CGWS").Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "Package Type", mappedCarrierName + " Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "Package Type", mappedCarrierName + " Code", "PKG")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "Package Type", mappedCarrierName + " Code", "PLT")).Return("PLT").Repeat.Any();
      // iso
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "Package Type", mappedCarrierName + " Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      //default
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "Package Type", mappedCarrierName + " Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "48K8")).Return("48T8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(mappedCarrierName, mappedCarrierName, destinationPartyMappingName, "ContainerTypeToISOCode", mappedCarrierName + " Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "")).Return("").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode(serviceProvider)).Return(payableElseWhere).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", destinationParty)).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", mappedCarrierName, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", mappedCarrierName, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", mappedCarrierName, "xxx")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", mappedCarrierName, "xxx")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", mappedCarrierName, "yyy")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", mappedCarrierName, "yyy")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", mappedCarrierName, "GEN")).Return("SSR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", mappedCarrierName, "GEN")).Return("FGE").Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "YANGMING", "YML0000000003", "C00678601")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "CMACGM", "CMA0000000003", "C00678601")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "EVERGREEN", "EVG0000000003", "C00678601")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "MSC", "MSC0000000003", "C00678601")).Repeat.Any();

      return new Dictionary<string, object>
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierCode, string carrierName, string SCAC,
                       string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE",
                       string shipmentType = "", string multipleMainLegs = "true", string multipleMainLegsForAllSeaLegs = "false",
                       string multiPickupDelivery = "FALSE", string payableElseWhere = "", string numberformat = "", string enableNADCB = "false",
                       string expectedBGM03Code = "4", string enableSGPWithContainerLinkFallback = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(carrierCode, carrierName, SCAC, partySenderIdentifier, destinationPartyReceiverIdentifier, isSummary, shipmentType, multipleMainLegs, multipleMainLegsForAllSeaLegs, multiPickupDelivery, payableElseWhere, numberformat, enableNADCB, expectedBGM03Code, enableSGPWithContainerLinkFallback);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMBF>(input, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();

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
          "C21501",  //SealParty
          "DGS05",
          "C524",
          "EQDLoop1",
          "C53601",
          "NAD"
        };
      }
    }

    void AssertMappingException(string inputFile, string exceptionMessage, string carrierCode, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "")
    {
      var input = filePath + inputFile;

      var extensionObjects = SetupMappingExtensions(carrierCode, carrierName, SCAC, partySenderIdentifier, destinationPartyReceiverIdentifier, isSummary, shipmentType);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<CarrierUniversal2IFTMBF>(input, exceptionMessage);
    }
  }
}