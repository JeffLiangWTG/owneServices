using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF_HAPAG;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversalShipment2IFTMBF_HAPAG_Tests
  {
    const string filePath = "CarrierUniversal2IFTMBF_HAPAG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_HAPAG()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", NADCB: "true");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", multiPickupDelivery: "TRUE");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "A", documentName: "Shipping Order");
      AssertMapping("Test3_input_USExport.xml", "Test3_output_USExport.xml");
      AssertMapping("Test4_UAT_input.xml", "Test4_UAT_output.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml", isSummary: "TRUE");
      AssertMapping("Test6_input.xml", "Test6_output.xml");
      AssertMapping("Test7_input.xml", "Test7_output.xml", isSummary: "TRUE");
      AssertMapping("Test8_input_TransportModes.xml", "Test8_output_TransportModes.xml");
      AssertMapping("Test9_input.xml", "Test9_output.xml", isSummary: "TRUE");
      AssertMapping("Test10_input_coload.xml", "Test10_output_coload.xml", isSummary: "TRUE");
      AssertMapping("Test11_input.xml", "Test11_output.xml");
      AssertMapping("Test12_IsOutOfGaugeBeFalse_input.xml", "Test12_IsOutOfGaugeBeFalse_output.xml");
      AssertMapping("Test13_SHP_GroupingMethod_input.xml", "Test13_SHP_GroupingMethod_output.xml", NADCB: "true");
      AssertMapping("Test14_FlatContainerQuality_input.xml", "Test14_FlatContainerQuality_output.xml");
      AssertMapping("Test15_DNG_GroupingMethod_input.xml", "Test15_DNG_GroupingMethod_output.xml");
      AssertMapping("Test16_SGP_GroupingMethod_input.xml", "Test16_SGP_GroupingMethod_output.xml", NADCB: "true");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string isSummary = "FALSE", string multiPickupDelivery = "FALSE", string payableElseWhere = "", string documentName = "Booking Request", string NADCB = "false")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HAPAG_LLOYD_BK1");

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "HLCU"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.HAPAG_LLOYD.UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HAPAG_LLOYD", "@recipientId", "TESTSENDER__1", "@ST_ID", "HAPMSG", "@value", "C00678677")).Return("HYEDAUTST");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "HAPAG_LLOYD")).Return("HLCU");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "NGB", "HAPAG_LLOYD")).Return("HLCU");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_HLCU_3"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "HLCU"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "3", "C00678677"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "WTH", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", documentName, "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID12345678").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "TrackingID12345678", "3")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "3.0.0", "FormVersion")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_BK1", "HLCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_BK1", "MSCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_BK1", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "HAPAG_LLOYD_BK1", "HLCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "HAPAG_LLOYD_BK1", "MSCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "40R0")).Return("40RT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22R0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22R1")).Return("22R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "45G0")).Return("45G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "PLT")).Return("PI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "P_I")).Return("PI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("DI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "HAPAG_LLOYD")).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "HAPAG_LLOYD", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "HAPAG_LLOYD", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "HAPAG_LLOYD", "GEN")).Return("FGE").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Features Control", "Enabled", "HAPAG_LLOYD", "NADCB")).Return(NADCB).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "160311"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "0851"));
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyMMddHHmm")).Return("1603110851").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("HAPAG_LLOYD_BK1")).Return("HAPAG_LLOYD").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("HAPAG_LLOYD")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "HLCU", "HAPAG_LLOYD", "HYEDAUTST", "C00678677")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMBF_HAPAG>(input, expectedOutput);
      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
