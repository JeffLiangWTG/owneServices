using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2IFTMIN_SNT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2IFTMIN_SINOTRANS_Test
  {
    const string filePath = "UniversalShipment2IFTMIN_SINOTRANS.TestFiles.";

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2IFTMIN_SINOTRANS()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "PreviousConsolRef");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "PreviousConsolRef");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "");
      AssertMapping("Test5_input_GroupingMethod_SHP.xml", "Test5_output_GroupingMethod_SHP.xml", "");
      AssertMapping("Test6_input_GroupingMethod_DNG.xml", "Test6_output_GroupingMethod_DNG.xml", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string subscribeConsolReference)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();

      mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CW1Client");
      mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SINOTRANS");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.SINOTRANS.UNH1", "@maxlength", "14")).Return("1");
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.SINOTRANS.BGM", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SINOTRANS", "@recipientId", "CW1Client", "@ST_ID", "SNTMSG", "@value", "C00001136")).Return(subscribeConsolReference);

      var serviceProvider = "SINOTRANS";
      mockOCMHelper.Expect(x => x.GetServiceProvider(Arg<string>.Is.Anything)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return("SNTID");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return("SNTMSG");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return("SNT");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "BRSID", serviceProvider)).Return("SNTBRS");
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(serviceProvider);

      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "Package Type", "SINOTRANS Code", "PLT")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "Package Type", "SINOTRANS Code", "PKG")).Return("PKG");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "ContainerTypeToISOCode", "Carrier Code", "22R0")).Return("22R0");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "CarrierBookingAgent", "SINOTRANS Code", "C1C-0001")).Return("5432");

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "SINOTRANS", "INTT")).Return("INTT");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PLT")).Return("PLT");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "SINOTRANS", "CAR")).Return("CARS");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "SINOTRANS", "QRT")).Return("QRTS");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "SINOTRANS", "CTO")).Return("CTOS");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "SINOTRANS", "CRD")).Return("CRDS");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", "SINOTRANS", "CUS")).Return("CUSS");

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Default Interface Name", "Interface Name", "SINOTRANS")).Return("SINOTRANS Provider Configuration").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "SINOTRANS", "INTT")).Return("SINOTRANS").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "CarrierBookingAgent", "SINOTRANS Code", "C1C-0001")).Return("5432").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "ContainerTypeToISOCode", "SINOTRANS Code", "22R0")).Return("22R0").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SINOTRANS", "SINOTRANS", "SINOTRANS Provider Configuration", "ContainerTypeToISOCode", "SINOTRANS Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "PKG")).Return("PKG").Repeat.Any();

      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Stub(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Stub(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Stub(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "SINOTRANS"));
      mockContextAccessor.Stub(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Stub(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "ShippingOrder_SINOTRANSClient_1"));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Stub(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Stub(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTID", "SINOTRANS", "CW1Client", "SINOTRANSClient"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "1", "C00001136"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "SNT0000000001", "C00001136")).Repeat.Any();
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "C00001136", "SNT0000000001")).Repeat.Any();
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "C00001136", "", "ShipmentType"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "C00001136", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "1", "Shipping Order", "DocumentName"));
      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "1", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", subscribeConsolReference, "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("SNTMSG", "SINOTRANS", "CW1Client", "SNT0000000001", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Stub(x => x.GetClientRegistrationCode("CW1Client", "SHA", serviceProvider)).Return("SINOTRANSClient");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniversalShipment2IFTMIN_SNT>(input, expectedOutput);
    }
  }
}
