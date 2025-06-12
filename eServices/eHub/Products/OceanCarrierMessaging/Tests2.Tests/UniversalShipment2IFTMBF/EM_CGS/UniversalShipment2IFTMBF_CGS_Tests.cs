using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMBF.EM_CGS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMBF_CGS_Tests
  {
    const string filePath = "UniversalShipment2IFTMBF.EM_CGS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniversalShipment2IFTMBF_CGS()
    {
      AssertMapping("Test1_EM_input.xml", "Test1_EM_output.xml");
      AssertMapping("Test2_EM_input.xml", "Test2_EM_output.xml");
      AssertMapping("Test4_EM_input.xml", "Test4_EM_output.xml", "TRUE");
      AssertMapping("Test5_EM_NewForm_input.xml", "Test5_EM_NewForm_output.xml", "TRUE", true);
      AssertMapping("Test6_EM_input.xml", "Test6_EM_output.xml");

      AssertException("Test3_EM_input.xml", "Test1_EM_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string isSummary = "FALSE", bool isNewFormMessage = false)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1Client");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOSMART");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("99804516-9637-4681-8573-3E349ABDE86C");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOSMART.UNH1", "@maxlength", "14")).Return("100").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CGS", "@recipientId", "CW1Client", "@ST_ID", "CGSMSG", "@value", "S00001446_973808566/A")).Return("PreviousConsolRef").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "Package Type", "Package Type", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "Package Type", "Package Type", "PKG")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "ContainerTypeToISOCode", "Carrier Code", "45G0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R1")).Return("DRY").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R2")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART", "CARGOSMART", "eManifest IFTMBF to CARGOSMART (v2)", "CarrierHandlingAgent", "CGS Code", "C1C-0001")).Return("2345").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART", "FakeSCAC")).Return("FakeSCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART", "COSU")).Return("COSU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("SP45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45G0")).Return("SP45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R2")).Return("RRR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "CARGOSMART", "FakeSCAC")).Return("CGS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "CARGOSMART", "COSU")).Return("CGS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PKG")).Return("PKG").Repeat.Any();

      if (!isNewFormMessage)
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "1")).Return("EIN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "1")).Return("EIN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "2")).Return("LSC").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "US", "1")).Return("XX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "US", "2")).Return("YY").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "CN", "1")).Return("ZZ").Repeat.Any();
      }
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "CARGOSMART")).Return("false").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CGS"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "100"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "100"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "eManifest_CGSClient_100"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSID", "CARGOSMART", "CW1Client", "CGSClient"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "100", "S00001446_973808566/A")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "100", "S2300088127_COSU635087382012")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "CGS0000000123", "S2300088127_COSU635087382012")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "S2300088127_COSU635087382012", "CGS0000000123")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "CW1Client", "S00001446_973808566/A", "ShipmentType", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "CW1Client", "S2300088127_COSU635087382012", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "CW1Client", "S00001446_973808566/A", "APP")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "CW1Client", "S2300088127_COSU635087382012", "APP")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "CW1Client", "S2300088127_COSU635087382012", "ORG")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "100", "eManifest", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "100", "ForwardingShipment", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "100", "Shipment", "SubMessageType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "99804516-9637-4681-8573-3E349ABDE86C", "100")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "CW1Client", "PreviousConsolRef", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("CW1Client", "SHA", "CARGOSMART")).Return("CGSClient");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("CW1Client", "", "CARGOSMART")).Return("CGSClient");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARGOSMART", "@recipientId", "CW1Client", "@ST_ID", "CGSMSG", "@value", "S00001446_973808566/A")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARGOSMART", "@recipientId", "CW1Client", "@ST_ID", "CGSMSG", "@value", "S2300088127_COSU635087382012")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOSMART.BGM", "@maxlength", "14")).Return("123").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "CW1Client", "CARGOSMART")).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "CARGOSMART", "CAR")).Return("CA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "CARGOSMART", "CRD")).Return("SH").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "CARGOSMART", "QRT")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "CARGOSMART", "CTO")).Return("").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UniversalShipment2IFTMBF_CGS>(input, expectedOutput);
    }

    void AssertException(string inputFile, string outputFile)
    {
      var input = filePath + inputFile;
      var output = filePath + outputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper }
            };

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1Client");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOSMART");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "CARGOSMART")).Return("false").Repeat.Any();

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<UniversalShipment2IFTMBF_CGS>(input, "Input UniversalShipment is supposed to have one and only one SubShipment.");

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
