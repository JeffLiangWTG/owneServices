using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Orchestrations.Schemas;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierEmail;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierEmail_CarrierUniversal2IFTMIN_Tests
  {
    const string filePath = "CarrierEmail.CarrierUniversal2IFTMIN.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test1_input.xml", "Test1_output_without_EDIFACT.xml", "YML", "YangMing", "YMLA", useEDIAttachment: "N");
      AssertMapping("Test1_input.xml", "Test1_output_bis.xml", "YML", "YangMing", "YMLA", partySenderIdentifier: "", orgPort: "AUCAR");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test2_input.xml", "Test2_notIncludeRegulatingCountry_output.xml", "YML", "YangMing", "YMLA", includeRegulatingCountry: "false");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "YML", "YangMing", "YMLA", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test5_input_CoLoad.xml", "Test5_output_CoLoad.xml", "YML", "YangMing", "YMLA", shipmentType: "CLD");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE", numberFormat: "TMP=0");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE", numberFormat: "TMP=0;DGS=0");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE", useMessageReference: true);
      AssertMapping("Test9_input.xml", "Test9_output.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE", allowEmptySegment: "false");
      AssertMapping("Test10_input_Format.xml", "Test10_output_Format.xml", "YML", "YangMing", "YMLA", isSummary: "TRUE", exclusion: "C058", location: "", maxLength: "10", noOfSegment: "5", numberFormat: "");
      AssertMapping("CMACGM.Test1_input.xml", "CMACGM.Test1_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test2_input.xml", "CMACGM.Test2_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test3_input.xml", "CMACGM.Test3_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      AssertMapping("CMACGM.Test4_input.xml", "CMACGM.Test4_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      //detail - normal
      AssertMapping("CMACGM.Test5_input.xml", "CMACGM.Test5_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      //detail - one shipment's number is empty
      AssertMapping("CMACGM.Test6_input.xml", "CMACGM.Test6_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      //detail - one packline has no shipment number
      AssertMapping("CMACGM.Test7_input.xml", "CMACGM.Test7_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM");
      //summary- each group has one packline, one packline has no shipment number, one shipment's number is empty
      AssertMapping("CMACGM.Test8_input.xml", "CMACGM.Test8_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM", isSummary: "TRUE");
      //summary - a group has several packlines
      AssertMapping("CMACGM.Test9_input.xml", "CMACGM.Test9_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM", isSummary: "TRUE", numberFormat: "MOA44=0;CNT7=0;CNT15=0");
      //summary - a group has several packlines, but shipment numbers are duplicated
      AssertMapping("CMACGM.Test10_input.xml", "CMACGM.Test10_output.xml", "CMA", "CMACGM", "BLAH", destinationPartyReceiverIdentifier: "CMACGM", isSummary: "TRUE", numberFormat: "MOA44=0;CNT7=0;CNT15=0");
      AssertMapping("Test11_input.xml", "Test11_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test12_input.xml", "Test12_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test13_MSC_input.xml", "Test13_MSC_output.xml", "MSC", "MSC", "MSCU");
      AssertMapping("Test14_INDIA_input.xml", "Test14_INDIA_output.xml", "YML", "YangMing", "MAEU", shipmentType: "AGT", orgPort: "AUMEL");
      AssertMapping("Test15_INDIA_input.xml", "Test15_INDIA_output.xml", "YML", "YangMing", "HDMU", shipmentType: "AGT", orgPort: "AUMEL");
      AssertMapping("Test16_input.xml", "Test16_output.xml", "YML", "YangMing", "YMLA");
      AssertMapping("Test17_input.xml", "Test17_output.xml", "HMM", "Hyundai Merchant Marine", "HDMU", destinationPartyReceiverIdentifier: "HYUNDAI", numberFormat: "TMP=0;DGS=0", includeWayBillNumber: "true");
      AssertMapping("Test18_input.xml", "Test18_output.xml", "HMM", "Hyundai Merchant Marine", "HDMU", destinationPartyReceiverIdentifier: "HYUNDAI", includeWayBillNumber: "true");
      AssertMapping("Test19_BRAZIL_summary_input.xml", "Test19_BRAZIL_summary_output.xml", "YML", "YangMing", "WHLC", shipmentType: "AGT", isSummary: "TRUE", orgPort: "TWTPE");
      AssertMapping("Test20_BRAZIL_input.xml", "Test20_BRAZIL_output.xml", "YML", "YangMing", "WHLC", shipmentType: "AGT", orgPort: "TWTPE");
      AssertMapping("Test21_input.xml", "Test21_output_RFFGN2.xml", "YML", "YangMing", "YMLU", "HYEDCNUAT", shipmentType: "AGT", maxLength: "512", noOfSegment: "1", numberOfSegment: "2", includeRegulatingCountry: "true", orgPort: "CNSHA");
      AssertMapping("Test21_input.xml", "Test21_output_RFFGN1.xml", "YML", "YangMing", "YMLU", "HYEDCNUAT", shipmentType: "AGT", maxLength: "512", noOfSegment: "1", numberOfSegment: "1", includeRegulatingCountry: "true", orgPort: "CNSHA");
      AssertMapping("Test21_input.xml", "Test21_output_RFFGN4.xml", "YML", "YangMing", "YMLU", "HYEDCNUAT", shipmentType: "AGT", maxLength: "512", noOfSegment: "1", numberOfSegment: "4", includeRegulatingCountry: "true", orgPort: "CNSHA");
      AssertMapping("Test22_GroupingMethod_input.xml", "Test22_GroupingMethod_output.xml", "YML", "YangMing", "MSCU", shipmentType: "AGT", orgPort: "AUSYD");
      AssertMapping("Test23_input.xml", "Test23_output.xml", "YML", "YangMing", "YMLA");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string carrierCode, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "", bool useMessageReference = false, string exclusion = "", string location = "LOC57=Default", string maxLength = "", string noOfSegment = "", string numberFormat = "MOA44=2;CNT7=3;CNT15=4", string allowEmptySegment = "true", string numberOfSegment = "2", string includeRegulatingCountry = "true", string includeWayBillNumber = "false", string payableElseWhere = "", string payableElseWhereDescription = "", string orgPort = "AUCAR", string useEDIAttachment = "Y")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(carrierCode, carrierName, SCAC, partySenderIdentifier, destinationPartyReceiverIdentifier, isSummary, shipmentType, useMessageReference, exclusion, location, maxLength, noOfSegment, numberFormat, allowEmptySegment, numberOfSegment, includeRegulatingCountry, includeWayBillNumber, payableElseWhere, payableElseWhereDescription, orgPort, useEDIAttachment);

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester1.Execute<CarrierUniversal2IFTMIN>(input, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<IFTMINEnvelope>(expectedOutput, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
        {
          "datatype 'String' - The actual length is less than the MinLength value.",
          "C21501", //SealParty
          "BGM03",
          "DGS05",
        };
      }
    }

    Dictionary<string, object> SetupMappingExtensions(string carrierCode, string carrierName, string SCAC, string partySenderIdentifier = "CW1", string destinationPartyReceiverIdentifier = "YANGMING", string isSummary = "FALSE", string shipmentType = "", bool useMessageReference = false, string exclusion = "", string location = "LOC57=Default", string maxLength = "", string noOfSegment = "", string numberFormat = "MOA44=2;CNT7=3;CNT15=4", string allowEmptySegment = "true", string numberOfSegment = "2", string includeRegulatingCountry = "true", string includeWayBillNumber = "false", string payableElseWhere = "", string payableElseWhereDescription = "", string orgPort = "AUCAR", string useEDIAttachment = "Y")
    {
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      var destinationParty = carrierName + "_SI1";
      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      var index = destinationParty.IndexOf("_", StringComparison.Ordinal);
      var serviceProvider = index >= 0
                          ? destinationParty.Substring(0, index)
                          : destinationParty;

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "SID (RFF+SI) Value", "Use Message Reference", destinationParty)).Return(useMessageReference.ToString().ToLower()).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", destinationParty)).Return("false").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", destinationParty)).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", destinationParty)).Return(includeRegulatingCountry).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.UNH1", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARRIER_EMAIL.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00001111", "@referenceType", "JobNumber")).Return("");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARRIER_EMAIL", "@recipientId", "TESTSENDER__1", "@ST_ID", "EMLMSG", "@value", "C00001111")).Return("C00001111");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMIN_CGWS_1"));
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLID", "CARRIER_EMAIL", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "1", "C00001111"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", destinationParty + "0000000003", "C00001111"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", destinationParty + "0000000003"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", shipmentType, "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "Shipping Instruction WIP", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "Shipping Instruction", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("EMLMSG", "CARRIER_EMAIL", "TESTSENDER__1", "C00001111", "3.0.0", "FormVersion")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "SHA", "CARRIER_EMAIL")).Return("CGWS").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", destinationParty, SCAC)).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", destinationParty, "")).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", destinationParty, SCAC)).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", destinationParty, "")).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, SCAC)).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", destinationParty, "BLAA")).Return(SCAC).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Package Type", "CARRIER_EMAIL" + " Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Package Type", "CARRIER_EMAIL" + " Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Package Type", "CARRIER_EMAIL" + " Code", "PLT")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Package Type", "CARRIER_EMAIL" + " Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Package Type", "CARRIER_EMAIL" + " Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "40G0")).Return("40G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "ContainerTypeToISOCode", "CARRIER_EMAIL" + " Code", "45R1")).Return("45R1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", destinationParty)).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Address Format", "Exclusion", destinationParty, SCAC)).Return(exclusion).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Location Format", "Format", destinationParty, SCAC)).Return(location).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "MaxLength", destinationParty, SCAC, "BLC")).Return("26").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "NoOfSegment", destinationParty, SCAC, "BLC")).Return(noOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "MaxLength", destinationParty, SCAC, "AAI")).Return(maxLength).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "NoOfSegment", destinationParty, SCAC, "AAI")).Return(noOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "AllowEmptySegment", destinationParty, SCAC, "AAA")).Return(allowEmptySegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "IncludeWayBillNumber", destinationParty, SCAC, "CCI")).Return(includeWayBillNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Number Format", "Format", destinationParty, SCAC)).Return(numberFormat).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "INTTRA", "XXXX", "Bolero")).Return("BR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "INTTRA", "", "Bolero")).Return("BO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "INTTRA", "", "Cargo X")).Return("CX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "", "", "essDOCS")).Return("ED").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "", "", "TRADELENS")).Return("TR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "", "", "WAVE BL")).Return("WL").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", destinationParty, SCAC, "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", destinationParty, SCAC, "Cargo X")).Return("CX").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "MaxLength", destinationParty, SCAC, "ACA")).Return("20").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "FTX Format", "NoOfSegment", destinationParty, SCAC, "ACA")).Return(noOfSegment).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (SI)", "Use HQ Email", orgPort, SCAC)).Return("Y").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (SI)", "EDI Attachment", orgPort, SCAC)).Return(useEDIAttachment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARRIER_EMAIL", "CARRIER_EMAIL", "CARRIER_EMAIL Provider Configuration", "Email_Lookup (SI)", "HQ Email Address", orgPort, SCAC)).Return("carrier@carrier.com").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "carrier@carrier.com"));

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("dd-MM-yyyyTHH:mm:ss")).Return("23-01-2024T11:44:33").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetServiceProvider(destinationParty)).Return(serviceProvider).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode(serviceProvider)).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription(serviceProvider)).Return(payableElseWhereDescription).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "YangMing", "C00001111", "C00001111")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "CMACGM", "C00001111", "C00001111")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "MSC", "C00001111", "C00001111")).Repeat.Any();
      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "Hyundai Merchant Marine", "C00001111", "C00001111")).Repeat.Any();

      return new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };
    }
  }
}