using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN.SO;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UShipment2IFTMIN_ShippingOrder_Tests
  {
    const string filePath = "UniversalShipment2IFTMIN.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UShipment2IFTMIN_ShippingOrder_INTTRA()
    {
      AssertMapping("Test1_SO_input.xml", "Test1_SO_output.xml");
      AssertMapping("Test1_SO_input.xml", "Test1_SO_output.xml", partySenderIdentifier: "");
      AssertMapping("Test2_SO_input.xml", "Test2_SO_output.xml");
      AssertMapping("Test3_SO_input.xml", "Test3_SO_output.xml");
      AssertMapping("Test4_SO_input.xml", "Test4_SO_output.xml", isSummary: "TRUE");

      AssertMapping("Test1_SO_input.xml", "Test1_SO_output.xml", defaultInterfaceName: "INTTRA Provider Configuration");
      AssertMapping("Test5_SO_input.xml", "Test5_SO_output.xml", isSummary: "TRUE", recipientID: "MSC_SO1");

      AssertMapping("Test7_SO_input_TransportModes.xml", "Test7_SO_output_TransportModes.xml");

      AssertMapping("Test8_SO_input_GroupingMethod.xml", "Test8_SO_output_GroupingMethod.xml");
      AssertMapping("Test9_SO_input_GroupingMethod_SHP.xml", "Test9_SO_output_GroupingMethod_SHP.xml");
      AssertMapping("Test10_SO_input_GroupingMethod_DNG.xml", "Test10_SO_output_GroupingMethod_DNG.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string recipientID = "INTTRA_SO", string partySenderIdentifier = "CW1", string isSummary = "FALSE", string defaultInterfaceName = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();

      var index = recipientID.IndexOf("_");
      var serviceProvider = index >= 0
        ? recipientID.Substring(0, index)
        : recipientID;

      var interfaceName = string.IsNullOrEmpty(defaultInterfaceName)
        ? "ShippingOrder IFTMIN to INTTRA (v2)"
        : defaultInterfaceName;

      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1Client");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("1");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "INTTRA", "@recipientId", "CW1Client", "@ST_ID", "INTMSG", "@value", "C00001136")).Return("PreviousConsolRef");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "INTT")).Return("INTT");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Default Interface Name", "Interface Name", recipientID)).Return(defaultInterfaceName).Repeat.Any();

      if (string.IsNullOrEmpty(defaultInterfaceName))
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode(recipientID, recipientID, "ShippingOrder IFTMIN to INTTRA (v2)", "Package Type", "Package Type", "PLT")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(recipientID, recipientID, "ShippingOrder IFTMIN to INTTRA (v2)", "Package Type", "Package Type", "PKG")).Return("PKG").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(recipientID, recipientID, "ShippingOrder IFTMIN to INTTRA (v2)", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(recipientID, recipientID, "ShippingOrder IFTMIN to INTTRA (v2)", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(recipientID, recipientID, "ShippingOrder IFTMIN to INTTRA (v2)", "CarrierBookingAgent", "INTTRA Code", "C1C-0001")).Return("5432").Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA", "INTTRA", interfaceName, "Package Type", "INTTRA Code", "PLT")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA", "INTTRA", interfaceName, "ContainerTypeToISOCode", "INTTRA Code", "22R0")).Return("22R0").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA", "INTTRA", interfaceName, "ContainerTypeToISOCode", "INTTRA Code", "45R0")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA", "INTTRA", interfaceName, "CarrierBookingAgent", "INTTRA Code", "C1C-0001")).Return("5432").Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", recipientID, "INTT")).Return("INTTRA");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", recipientID, "INTT")).Return(partySenderIdentifier);
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CAR")).Return("CARS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "QRT")).Return("QRTS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CTO")).Return("CTOS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CRD")).Return("CRDS").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CUS")).Return("CUSS").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return("INTID");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return("INTMSG");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return("INT");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", serviceProvider)).Return("true");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", serviceProvider)).Return("INTBRS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return("INTTRA");

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "CW1Client", "INTTRA")).Return(isSummary).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifierForContextProperty));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "INTTRA"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ShippingOrder_INTTRAClient_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "CW1Client", "INTTRAClient"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "C00001136"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "C00001136", "", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "C00001136", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "Shipping Order", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "C00001136", "Shipping Order", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "C00001136", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "C00001136", "3.0.0", "FormVersion")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "CW1Client", "1", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("CW1Client", "SHA", "INTTRA")).Return("INTTRAClient");

      var extensionObjects = new Dictionary<string, object>()
      {
        {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper},
        {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor},
        {"http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper}
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMIN>(input, expectedOutput);
      //mapTester.ExecuteCompiledWithXslDebug<UniversalShipment2IFTMIN>(input, expectedOutput, "C:\\eServices_Dev02\\eHub\\Products\\OceanCarrierMessaging\\UniversalShipment2IFTMIN\\SO\\UniversalShipment2IFTMIN.xsl");

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
