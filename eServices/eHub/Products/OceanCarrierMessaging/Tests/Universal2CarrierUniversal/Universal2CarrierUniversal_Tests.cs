using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Universal2CarrierUniversal_Tests
  {
    const string filePath = "Universal2CarrierUniversal.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversal2CarrierUniversal()
    {
      AssertMapping<Universal2CarrierUniversal>("BookingRequest To AgencyBooking", "Test1_BookingRequest_input.xml", "Test1_BookingRequest_output.xml");
      AssertMapping<Universal2CarrierUniversal>("BookingRequest To AgencyBooking(including 3PT sub-shipment)", "Test1_BookingRequest_3PT_input.xml", "Test1_BookingRequest_3PT_output.xml");
      AssertMapping<Universal2CarrierUniversal>("ShippingInstruction To BillOfLading", "Test2_ShippingInstruction_input.xml", "Test2_BillOfLading_output.xml");
      AssertMapping<Universal2CarrierUniversal>("ShippingInstruction DRT", "Test3_ShippingInstruction_DRT_input.xml", "Test3_ShippingInstruction_DRT_output.xml");
      AssertMapping<Universal2CarrierUniversal>("Verified Gross Container Weight", "Test4_VerifiedGrossContainerWeight_input.xml", "Test4_VerifiedGrossContainerWeight_output.xml");
      AssertMapping<Universal2CarrierUniversal>("ShippingInstruction BCN/STD", "Test5_ShippingInstruction_BCNSTD_input.xml", "Test5_ShippingInstruction_BCNSTD_output.xml");
      AssertMapping<Universal2CarrierUniversal>("ShippingInstruction Co-Load (CLD)", "Test6_ShippingInstruction_CLD_input.xml", "Test6_ShippingInstruction_CLD_output.xml");
      AssertMapping<Universal2CarrierUniversal>("VGM without Packing Line", "Test7_VerifiedGrossContainerWeight_input.xml", "Test7_VerifiedGrossContainerWeight_output.xml");
      AssertMapping<Universal2CarrierUniversal>("ShippingInstruction ITN", "Test10_ShippingInstruction_ITN_input.xml", "Test10_ShippingInstruction_ITN_output.xml");
      AssertMapping<Universal2CarrierUniversal>("USHipment Without SubShipmentCollection", "Test15_ShippingInstruction_Without_SubShipmentCollection_input.xml", "Test15_ShippingInstruction_Without_SubShipmentCollection_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversal2CarrierUniversal_HCWithDotsAndSpaces()
    {
      AssertMapping<Universal2CarrierUniversal>("", "Test8_input_HCDotsAndSpaces.xml", "Test8_output_HCDotsAndSpaces.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversal2CarrierUniversal_PickupDeliveryAddresss()
    {
      AssertMapping<Universal2CarrierUniversal>("Pickup Delivery Address", "Test9_PickupDeliveryAddress_input.xml", "Test9_PickupDeliveryAddress_output.xml");
      AssertMapping<Universal2CarrierUniversal>("Consignee/Consignor Pickup Delivery Address", "Test11_NoOrgCode_input.xml", "Test11_NoOrgCode_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestFormBuilderV1ShouldBeSameAsUniversal2CarrierUniversal()
    {
      // Universal2CarrierUniversal from FormBuilder v1 must be the same with Universal2CarrierUniversal until there is a nofitication from International Team.
      AssertMapping<Universal2CarrierUniversal_BK>("Test1_BookingRequest_FormBuilder_v1_input.xml", "Test1_BookingRequest_FormBuilder_v1_output.xml");
      AssertMapping<Universal2CarrierUniversal_SI>("Test2_ShippingInstruction_FormBuilder_v1_input.xml", "Test2_ShippingInstruction_FormBuilder_v1_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversal2CarrierUniversal_VehicleCollection()
    {
      AssertMapping<Universal2CarrierUniversal>("Test12_U2CU_SI_VehicleCollection_input.xml", "Test12_U2CU_SI_VehicleCollection_output.xml");
      AssertMapping<Universal2CarrierUniversal>("Test13_U2CU_BK_VehicleCollection_No_Containers_input.xml", "Test13_U2CU_BK_VehicleCollection_No_Containers_output.xml");
      AssertMapping<Universal2CarrierUniversal>("Test14_U2CU_BK_MultipleVehicles_input.xml", "Test14_U2CU_BK_MultipleVehicles_output.xml");
    }

    void AssertMapping<T>(string message, string inputFile, string expectedOutputFile) where T : Microsoft.XLANGs.BaseTypes.TransformBase
    {
      AssertMapping<T>(inputFile, expectedOutputFile);
    }

    void AssertMapping<T>(string inputFile, string expectedOutputFile) where T : Microsoft.XLANGs.BaseTypes.TransformBase
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GeneratePartialMock<ContextAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA");

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA")).Return("false").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("Gov Reference Number"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<T>(input, expectedOutput);
    }
  }
}
