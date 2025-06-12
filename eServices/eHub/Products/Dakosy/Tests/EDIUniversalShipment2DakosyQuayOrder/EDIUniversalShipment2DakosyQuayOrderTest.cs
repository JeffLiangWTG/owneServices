using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.EDIUniversalShipment2DakosyQuayOrder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class EDIUniversalShipment2DakosyQuayOrderTest
  {
    private const string filePath = "EDIUniversalShipment2DakosyQuayOrder.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2DakosyQuayOrder()
    {
      AssertMapping("Test01_Shipment_AES_input.xml", "Test01_Shipment_AES_output.xml");
      AssertMapping("Test01_Shipment_AES_LRN_input.xml", "Test01_Shipment_AES_LRN_output.xml");
      AssertMapping("Test01_Shipment_AES_LRN_noEORCDA_input.xml", "Test01_Shipment_AES_LRN_noEORCDA_output.xml");
      AssertMapping("Test01_Shipment_AES_LRN_EORCDA_equals_EORSF_input.xml", "Test01_Shipment_AES_LRN_EORCDA_equals_EORSF_output.xml");
      AssertMapping("Test02_Consol_AES_input.xml", "Test02_Consol_AES_output.xml");
      AssertMapping("Test03_MultiLine_input.xml", "Test03_MultiLine_output.xml");
      AssertMapping("Test03_MIT_MultiLine_input.xml", "Test03_MIT_MultiLine_output.xml");
      AssertMapping("Test04_DangerousGoods_input.xml", "Test04_DangerousGoods_output.xml");
      AssertMapping("Test05_Shipment_Cancellation1_input.xml", "Test05_Shipment_Cancellation1_output.xml");
      AssertMapping("Test05_Shipment_Cancellation2_input.xml", "Test05_Shipment_Cancellation2_output.xml");
      AssertMapping("Test06_Shipment_EUB_input.xml", "Test06_Shipment_EUB_output.xml");
      AssertMapping("Test07_SBF_ExemptionReasonV_input.xml", "Test07_SBF_ExemptionReasonV_output.xml");
      AssertMapping("Test07_SBF_ExemptionReasonL_input.xml", "Test07_SBF_ExemptionReasonL_output.xml");
      AssertMapping("Test08_AUS_input.xml", "Test08_AUS_output.xml");
      AssertMapping("Test09_AES_input.xml", "Test09_AES_output.xml");
      AssertMapping("Test10_DUX_input.xml", "Test10_DUX_output.xml");
      AssertMapping("Test11_AEM_input.xml", "Test11_AEM_output.xml");
      AssertMapping("Test12_Consol_Cancellation2_input.xml", "Test12_Consol_Cancellation2_output.xml");
      AssertMapping("Test13_Consol_Cancellation3_input.xml", "Test13_Consol_Cancellation3_output.xml");
      AssertMapping("Test14_A08_input.xml", "Test14_A08_output.xml");
      AssertMapping("Test15_S08_input.xml", "Test15_S08_output.xml");
      AssertMapping("Test16_BCN_input.xml", "Test16_BCN_output.xml");
      AssertMapping("Test17_ASM_AES_SubShipment_input.xml", "Test17_ASM_ASE_SubShipment_output.xml");
      AssertMapping("Test18_ASM_NonAES_SubShipment_input.xml", "Test18_ASM_NonAES_SubShipment_output.xml");
      AssertMapping("Test19_BCNSubShipment_input.xml", "Test19_BCNSubShipment_output.xml");
      AssertMapping("Test20_ZNO_input.xml", "Test20_ZNO_output.xml");
      AssertMapping("Test21_Console_MessageTooLong_input.xml", "Test21_Console_MessageTooLong_output.xml");
      AssertMapping("Test22_DOX_input.xml", "Test22_DOX_output.xml");
      AssertMapping("Test23_DOX_EoriCDAequalsEoriSF_input.xml", "Test23_DOX_EoriCDAequalsEoriSF_output.xml");
      AssertMapping("Test24_DOX_EoriCDAnotequalsEoriSF_input.xml", "Test24_DOX_EoriCDAnotequalsEoriSF_output.xml");
      AssertMapping("Test25_DUX_input.xml", "Test25_DUX_output.xml");
      AssertMapping("Test26_ROR_input.xml", "Test26_ROR_output.xml");
      AssertMapping("Test27_eoriSF_countryOfIssueAT_input.xml", "Test27_eoriSF_countryOfIssueAT_output.xml");
      AssertMapping("Test28_MITMessageAnnex_input.xml", "Test28_MITMessageAnnex_output.xml");
      AssertMapping("Test28_MITMessageNoAnnex_input.xml", "Test28_MITMessageNoAnnex_output.xml");
      AssertMapping("Test29_DakosyPortOrder_DeclarationOnPackLine_input.xml", "Test29_DakosyPortOrder_DeclarationOnPackLine_output.xml");
      AssertMapping("Test30_DakosyPortOrder_DeclarationOnShipment_input.xml", "Test30_DakosyPortOrder_DeclarationOnShipment_output.xml");
      AssertMapping("Test31_MultipleDG_input.xml", "Test31_MultipleDG_output.xml");
      AssertMapping("Test32_MultipleDG2_input.xml", "Test32_MultipleDG2_output.xml");
      AssertMapping("Test33_MultipleDG3_input.xml", "Test33_MultipleDG3_output.xml");
      AssertMapping("Test34_CoLoad_input.xml", "Test34_CoLoad_output.xml");
      AssertMapping("Test35_OneUNDG_SameWeight_input.xml", "Test35_OneUNDG_SameWeight_output.xml");
      AssertMapping("Test36_OneUNDG_DiffWeight_input.xml", "Test36_OneUNDG_DiffWeight_output.xml");
      AssertMapping("Test37_Shipment_ASM_input.xml", "Test37_Shipment_ASM_output.xml");
      AssertMapping("Test38_Shipment_BreakBulk_input.xml", "Test38_Shipment_BreakBulk_output.xml");
      AssertMapping("Test40_Shipment_AE1_input.xml", "Test40_Shipment_AE1_output.xml");
      AssertMapping("Test40_Shipment_AE1_LRN_input.xml", "Test40_Shipment_AE1_LRN_output.xml");
      AssertMapping("Test40_Shipment_AE1_LRN_noEORCDA_input.xml", "Test40_Shipment_AE1_LRN_noEORCDA_output.xml");
      AssertMapping("Test40_Shipment_AE1_LRN_EORCDA_equals_EORSF_input.xml", "Test40_Shipment_AE1_LRN_EORCDA_equals_EORSF_output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2DakosyQuayOrder_MaxLine()
    {
      try
      {
        AssertMapping("Test41_MaxLineValue_input.xml", "Test41_MaxLineValue_output.xml");
        Assert.Fail("Expect Exception should throw.");
      }
      catch (Exception ex)
      {
        var actualException = ex.InnerException ?? ex;
        Assert.AreEqual("Error: The message line count exceeds the total line count accepted by Dakosy.", actualException.Message);
      }
    }

    void AssertMapping(string inputFile, string outputFile)
    {
      var sourceFile = filePath + inputFile;
      var expectedOutputFile = filePath + outputFile;

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "HYEDDEBLN")).Return("T").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "SEIHAMHAM")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "SEIHAMTST")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "HYEDDEUAT")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "JASFRATS4")).Return("T").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "JASFRAHST")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "ContainerCode", "ContainerCode", "20RE")).Return("R2").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "ContainerCode", "ContainerCode", "20GP")).Return("C2").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "ContainerCode", "ContainerCode", "40GP")).Return("PA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "ContainerCode", "ContainerCode", "40HC")).Return("H4").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "PLT")).Return("PL").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "DRM")).Return("TR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "PKG")).Return("PA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "BOX")).Return("BO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "CAN")).Return("PA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "VE")).Return("VE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "BLC")).Return("BL").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "CTN")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "ENV")).Return("EN").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "PackingCode", "PackingCode", "CAS")).Return("CA").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "WaterHazard", "WaterHazardCode", "Y")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "OAOBREOSF")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("DAKOSYHAM", "DAKOSYHAM", "Export Declaration to Dakosy", "Participant", "TestFlag", "LAOAT0PRD")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "Dakosy.EDIUniversalShipment2DakosyQuayOrder", "@padlength", "10")).Return("0000000030").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTime("yyMMdd")).Return("151127").Repeat.Any();
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("O")).Return("2015-11-26T23:30:58.6464725Z").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "CW00151127.0000000030")).Repeat.Any();

      mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
    {
    { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
    { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
    { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor ", mockContextAccessor },
    { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor" , mockDataModelAccessor }
    };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<EDIUniversalShipment2DakosyQuayOrder>(sourceFile, expectedOutputFile);
    }
  }
}
