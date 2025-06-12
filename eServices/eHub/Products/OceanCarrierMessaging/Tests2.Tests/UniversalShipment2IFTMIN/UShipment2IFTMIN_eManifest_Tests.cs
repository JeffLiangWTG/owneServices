using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN.EM;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UShipment2IFTMIN_eManifest_Tests
  {
    const string filePath = "UniversalShipment2IFTMIN.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UShipment2IFTMIN_eManifest_INTTRA()
    {
      AssertMapping("Test1_EM_input.xml", "Test1_EM_output.xml");
      AssertMapping("Test1_EM_input.xml", "Test1_EM_output.xml", partySenderIdentifier: "");
      AssertMapping("Test2_EM_input.xml", "Test2_EM_output.xml");
      AssertMapping("Test3_EM_input.xml", "Test3_EM_output.xml");
      AssertMapping("Test4_EM_input.xml", "Test4_EM_output.xml");
      AssertMapping("Test5_EM_input.xml", "Test5_EM_output.xml", isSummary: "TRUE");
      AssertMapping("Test6_EM_NewForm_input.xml", "Test6_EM_NewForm_output.xml", isSummary: "TRUE", isNewFormMessage: true);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string partySenderIdentifier = "CW1", string isSummary = "FALSE", bool isNewFormMessage = false)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();

      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1Client").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_EM").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "INTTRA", "@recipientId", "CW1Client", "@ST_ID", "INTMSG", "@value", "S00001446_973808566/A")).Return("PreviousConsolRef").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", "INTTRA")).Return("INTTRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_EM", "FakeSCAC")).Return("FakeSCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_EM", "INTTRA_EM", "eManifest IFTMIN to INTTRA (v2)", "Package Type", "Package Type", "PLT")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_EM", "INTTRA_EM", "eManifest IFTMIN to INTTRA (v2)", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_EM", "INTTRA_EM", "eManifest IFTMIN to INTTRA (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_EM", "INTTRA_EM", "eManifest IFTMIN to INTTRA (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R1")).Return("DRY").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_EM", "INTTRA_EM", "eManifest IFTMIN to INTTRA (v2)", "CarrierHandlingAgent", "INTTRA Code", "C1C-0001")).Return("2345").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "INTTRA_EM", "FakeSCAC")).Return("INTTRA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", "INTTRA_EM", "FakeSCAC")).Return(partySenderIdentifier);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "INTTRA_EM", "CAR")).Return("CARS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "INTTRA_EM", "QRT")).Return("QRTS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "INTTRA_EM", "CTO")).Return("CTOS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "INTTRA_EM", "CRD")).Return("CRDS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "INTTRA_EM", "CUS")).Return("CUSS").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA_EM")).Return("false").Repeat.Any();
      if (!isNewFormMessage)
      {
        //Consignor
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "1")).Return("XXX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "CN", "1")).Return("XXX_REF").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "CN", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "CN", "2")).Return("").Repeat.Any();

        //Consignee
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "1")).Return("EIN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "US", "1")).Return("EIN_REF").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "KR", "CN", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "KR", "CN", "1")).Return("").Repeat.Any();

        //NotifyParty
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "AU", "1")).Return("ABN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "AU", "1")).Return("ABN_REF").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "CN", "US", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "CN", "US", "2")).Return("").Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "CW1Client", "INTTRA")).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", "INTTRA")).Return("INTID");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", "INTTRA")).Return("INTMSG");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", "INTTRA")).Return("INT");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", "INTTRA")).Return("true");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", "INTTRA")).Return("INTBRS");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifierForContextProperty));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "INTTRA"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "eManifest_INTTRAClient_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "CW1Client", "INTTRAClient"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "S00001446_973808566/A"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "S00001446_973808566/A", "ShipmentType", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "S00001446_973808566/A", "APP", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "eManifest", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "ForwardingShipment", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "Shipment", "SubMessageType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "S00001446_973808566/A", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("CW1Client", "SHA", "INTTRA")).Return("INTTRAClient");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UniversalShipment2IFTMIN>(input, expectedOutput);
    }
  }
}
