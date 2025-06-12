using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  public partial class Universal2CarrierUniversal_V1_Tests
  {
    [TestClass]
    public class Universal2CarrierUniversal_BK_Tests
    {
      const string filePath = "Universal2CarrierUniversal.v1.BK.TestFiles.";

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestCarrierUniversal_FormBuilder_v1_BK()
      {
        AssertMapping<UniversalInterchange2UniversalShipment_BK, Universal2CarrierUniversal_BK>("Test01_UniversalInterchange_BK.xml", "Test01_UniversalShipment_BK.xml", "Test01_CarrierUniversal_BK_Output.xml");
        AssertMapping<UniversalInterchange2UniversalShipment_BK, Universal2CarrierUniversal_BK>("Test02_NoContainers_UniversalInterchange_BK.xml", "Test02_NoContainers_UniversalShipment_BK.xml", "Test02_NoContainers_CarrierUniversal_BK_Output.xml");
        AssertMapping<UniversalInterchange2UniversalShipment_BK, Universal2CarrierUniversal_BK>("Test19_UniversalInterchange_GroupingMethod_BK.xml", "Test19_UniversalShipment_GroupingMethod_BK.xml", "Test19_CarrierUniversal_GroupingMethod_BK_Output.xml");
      }

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestUniversal2CarrierUniversal_v1_BK()
      {
        AssertMapping("Test03_U2CU_BK_FindPOLandPOD_input.xml", "Test03_U2CU_BK_FindPOLandPOD_output.xml");
        AssertMapping("Test04_U2CU_BK_PickupDeliveryAddress_input.xml", "Test04_U2CU_BK_PickupDeliveryAddress_output.xml");
        AssertMapping("Test05_U2CU_BK_PackingLineCollection_input.xml", "Test05_U2CU_BK_PackingLineCollection_output.xml");
        AssertMapping("Test06_U2CU_BK_SubShipmentCollection_input.xml", "Test06_U2CU_BK_SubShipmentCollection_output.xml");
        AssertMapping("Test07_U2CU_BK_FindITNUsingMinLegOrder_input.xml", "Test07_U2CU_BK_FindITNUsingMinLegOrder_output.xml");
        AssertMapping("Test08_U2CU_BK_LegType_input.xml", "Test08_U2CU_BK_LegType_output.xml");
        AssertMapping("Test09_U2CU_BK_NoOrgCode_input.xml", "Test09_U2CU_BK_NoOrgCode_output.xml");

        AssertMapping("Test10_U2CU_BK_VehicleCollection_No_Containers_3PT_input.xml", "Test10_U2CU_BK_VehicleCollection_No_Containers_3PT_output.xml");
        AssertMapping("Test11_U2CU_BK_VehicleCollection_No_Containers_input.xml", "Test11_U2CU_BK_VehicleCollection_No_Containers_output.xml");
        AssertMapping("Test12_U2CU_BK_MultipleVehicles_input.xml", "Test12_U2CU_BK_MultipleVehicles_output.xml");
        AssertMapping("Test13_U2CU_BK_Without_SubShipmentCollection_input.xml", "Test13_U2CU_BK_Without_SubShipmentCollection_output.xml");
        AssertMapping("Test14_U2CU_BK_tempControl_packingLine_input.xml", "Test14_U2CU_BK_tempControl_packingLine_output.xml");
        AssertMapping("Test15_U2CU_BK_tempControl_shipment_input.xml", "Test15_U2CU_BK_tempControl_shipment_output.xml");
        AssertMapping("Test16_U2CU_BK_NoMainLeg_Input.xml", "Test16_U2CU_BK_NoMainLeg_Output.xml");
        AssertMapping("Test17_U2CU_BK_PreallocatedUNDGCollection_Input.xml", "Test17_U2CU_BK_PreallocatedUNDGCollection_Output.xml");
        AssertMapping("Test18_U2CU_BK_WithoutPreallocatedUNDGCollection_Input.xml", "Test18_U2CU_BK_WithoutPreallocatedUNDGCollection_Output.xml");

        //add genset testing
        AssertMapping("Test20_UniversalShipment_GroupingMethod_BK.xml", "Test20_CarrierUniversal_GroupingMethod_BK_Output.xml");
        AssertMapping("Test21_UniversalShipment_GroupingMethod_BK.xml", "Test21_CarrierUniversal_GroupingMethod_BK_Output.xml");
      }

      public void AssertMapping(string inputFile, string expectedOutputFile)
      {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "STD")).Return("FALSE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");
        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper}
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.Execute<Universal2CarrierUniversal_BK>(input, expectedOutput);
      }

      public virtual void AssertMapping<T1, T2>(string universalInterchangeFile, string universalShipmentFile, string carrierUniversalFile)
        where T1 : Microsoft.XLANGs.BaseTypes.TransformBase
        where T2 : Microsoft.XLANGs.BaseTypes.TransformBase
      {
        var universalInterchange = filePath + universalInterchangeFile;
        var universalShipment = filePath + universalShipmentFile;
        var carrierUniversal = filePath + carrierUniversalFile;

        var mapTester = new MapTester(Assembly.GetExecutingAssembly());
        mapTester.Execute<T1>(universalInterchange, universalShipment);

        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");

        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper}
        };

        var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester2.Execute<T2>(universalShipment, carrierUniversal);
      }
    }
  }
}
