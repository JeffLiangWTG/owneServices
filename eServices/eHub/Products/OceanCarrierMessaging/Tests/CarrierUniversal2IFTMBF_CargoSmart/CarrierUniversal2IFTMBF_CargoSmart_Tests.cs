using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF_CargoSmart;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMBF_CargoSmart_Tests
  {
    const string filePath = "CarrierUniversal2IFTMBF_CargoSmart.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithFRT()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", multiPickupDelivery: "TRUE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithoutFRT()
    {
      AssertMapping("Test2_input.xml", "Test2_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithFRTNoPPD()
    {
      AssertMapping("Test3_input.xml", "Test3_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithCK()
    {
      AssertMapping("Test4_input.xml", "Test4_output.xml", payableElseWhere: "A");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithPreCarriageLegType()
    {
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test9_input_coload.xml", "Test9_output_coload.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithOnForwardingLegType()
    {
      AssertMapping("Test6_input.xml", "Test6_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartWithFRT_IsSummary()
    {
      AssertMapping("Test7_input.xml", "Test7_output.xml", isSummary: "TRUE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmartTransportModes()
    {
      AssertMapping("Test8_input_TransportModes.xml", "Test8_output_TransportModes.xml");
    }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCarrierUniversal2Iftmbf_CargoSmartWithDIM()
		{
			AssertMapping("Test10_input.xml", "Test10_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCarrierUniversal2Iftmbf_CargoSmartWithDIM_IsSummary()
		{
			AssertMapping("Test11_input.xml", "Test11_output.xml", isSummary: "TRUE");
		}

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmart_IsOutOfGaugeBeTrue()
    {
      AssertMapping("Test12_input.xml", "Test12_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmart_IsOutOfGaugeBeFalse()
    {
      AssertMapping("Test13_IsOutOfGaugeBeFalse_input.xml", "Test13_IsOutOfGaugeBeFalse_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmart_GroupingMethod()
    {
      AssertMapping("Test14_SHP_GroupingMethod_input.xml", "Test14_SHP_GroupingMethod_output.xml");
      AssertMapping("Test16_DNG_GroupingMethod_NoShipmentsAttached_input.xml", "Test16_DNG_GroupingMethod_NoShipmentsAttached_output.xml");
      AssertMapping("Test17_DNG_GroupingMethod_input.xml", "Test17_DNG_GroupingMethod_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2Iftmbf_CargoSmart_FlatContainerQuality()
    {
      AssertMapping("Test15_FlatContainerQuality_input.xml", "Test15_FlatContainerQuality_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string isSummary = "FALSE", string multiPickupDelivery = "FALSE", string payableElseWhere = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOSMART_BK1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOSMART"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CargoSmart.UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "40R0")).Return("40RT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "22R0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "40P0")).Return("40P0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "40U0")).Return("40U0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "22P0")).Return("22P0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "22U0")).Return("22U0").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARGOSMART", "@recipientId", "TESTSENDER__1", "@ST_ID", "CGSMSG", "@value", "C00678677")).Return("HYEDAUTST");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CARGOSMART")).Return("CGWS");
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_3"));//CargoSmart
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "CARGOSMART")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSID", "CARGOSMART", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "3", "C00678677"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "HYEDAUTST", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "HYEDAUTST", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00678677", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00678677", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00678677", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00678677", "Booking Request", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00678677", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "3"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_BK1", "OOLA")).Return("OOLU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_BK1", "MSCU")).Return("MSCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_BK1", "")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "CARGOSMART")).Return(isSummary).Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "PKG")).Return("P_O").Repeat.Any();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "Defaults", "Package Type")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARGOSMART", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARGOSMART", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARGOSMART", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARGOSMART", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "CARGOSMART", "GEN")).Return("SSR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "CARGOSMART", "GEN")).Return("FGE").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("CARGOSMART_BK1")).Return("CARGOSMART").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("CARGOSMART")).Return(payableElseWhere).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "CARGOSMART", "HYEDAUTST", "C00678677")).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMBF_CargoSmart>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
