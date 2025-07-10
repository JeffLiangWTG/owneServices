using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCI_RN_NKCountryOfOrigin()
		{
			cusClassPartPivot.CI_RN_NKCountryOfOrigin = "TW";
			var info = cusClassPartPivot.CI_RN_NKCountryOfOriginInfo;
			AssertNoMessageErrors(info);
			cusClassPartPivot.CI_RN_NKCountryOfOrigin = "XX";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_RN_NKCountryOfOrigin = ZString.Empty;
			AssertNoMessageErrors(info);
		}

		public void TestCheckCI_PartPivotUOM()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			var info = cusClassPartPivot.CI_PartPivotUOMInfo;
			cusClassPartPivot.CI_PartPivotUOM = "XX";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_PartPivotUOM = cusClassPartPivot.Lookups.PartPivotUOMList[0].Code;
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(info);
		}

		public void TestCheckCI_DeclGoodsDescMode()
		{
			var targetInfo = cusClassPartPivot.CI_DeclGoodsDescModeInfo;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
				ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "XX1", DeclarationGoodsDescriptionModeList.Codes.BTH);
				ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "XX2", DeclarationGoodsDescriptionModeList.Codes.CHT);
				ValidationTestHelper.AssertErrorIfInvalidCode(targetInfo, "XX3", DeclarationGoodsDescriptionModeList.Codes.ENG);
			});
		}

		public void CheckCI_Price()
		{
			var maxMoney = 922337203685477.5807M;
			var minMoney = -922337203685477.5808M;
			var expectError = string.Format("The number 999,999,999,999,999 is too large, the value's range of Decimal is between {0} and {1}.", minMoney, maxMoney);
			cusClassPartPivot.CI_Price = 999999999999999M;
			AssertHasError(cusClassPartPivot.CI_PriceInfo, expectError);
			cusClassPartPivot.CI_Price = 99999999999999M;
			AssertNoErrors(cusClassPartPivot.CI_PriceInfo);
		}

		public void TestCheckCI_PriceCurr()
		{
			cusClassPartPivot.CI_PriceCurr = "TWD";
			AssertNoMessageErrors(cusClassPartPivot.CI_PriceCurrInfo);
			cusClassPartPivot.CI_PriceCurr = "ZZ";
			AssertHasMessageError(cusClassPartPivot.CI_PriceCurrInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_PriceCurr = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_PriceCurrInfo);
		}

		public void TestCheckCI_ModeOfStatistics()
		{
			var pivot = setupPivotWhithCusprocedure();
			pivot.CI_ModeOfStatistics = "90";
			AssertNoMessageErrors(pivot.CI_ModeOfStatisticsInfo);
			pivot.CI_ModeOfStatistics = "ZZ";
			AssertHasMessageError(pivot.CI_ModeOfStatisticsInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_ModeOfStatistics = ZString.Empty;
			AssertNoMessageErrors(pivot.CI_ModeOfStatisticsInfo);
		}

		public void TestCheckCI_DutyTreatment()
		{
			var pivot = setupPivotWhithCusprocedure();
			pivot.CI_DutyTreatment = "5E";
			AssertNoMessageErrors(pivot.CI_DutyTreatmentInfo);
			pivot.CI_DutyTreatment = "ZZ";
			AssertHasMessageError(pivot.CI_DutyTreatmentInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_DutyTreatment = ZString.Empty;
			AssertNoMessageErrors(pivot.CI_DutyTreatmentInfo);
		}

		CusClassPartPivot setupPivotWhithCusprocedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("TW", "EX", "90", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G3,");
			helper.CreateRefCusProcedure("TW", "EX", "94", ZString.Empty, ZString.Empty, "國貨出口供經營國際貿易之非本國籍船舶、航空器或其他運輸工具專用之物料、物品。", "EXP", group: "D1,");
			helper.CreateRefCusProcedure("TW", "EX", "9G", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G7,");
			helper.CreateRefCusProcedure("TW", "IM", "5E", ZString.Empty, ZString.Empty, "外交郵袋", "IMP", group: "G3,D2,");
			helper.CreateRefCusProcedure("TW", "IM", "65", ZString.Empty, ZString.Empty, "預估稅捐", "IMP", group: "F3,");
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			return orgSupplierPart.PivotsForBinding.AddNew();
		}

		public void TestCheckCI_CarType()
		{
			cusClassPartPivot.CI_CarType = CarTypeCodeList.Codes.A1;
			AssertNoMessageErrors(cusClassPartPivot.CI_CarTypeInfo);
			cusClassPartPivot.CI_CarType = "ZZ";
			AssertHasMessageError(cusClassPartPivot.CI_CarTypeInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_CarType = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_CarTypeInfo);
		}

		public void TestCheckCI_Transmission()
		{
			cusClassPartPivot.CI_Transmission = TransmissionCodeList.Codes.Auto;
			AssertNoMessageErrors(cusClassPartPivot.CI_TransmissionInfo);
			cusClassPartPivot.CI_Transmission = "Z";
			AssertHasMessageError(cusClassPartPivot.CI_TransmissionInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_Transmission = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_TransmissionInfo);
		}

		public void TestCheckCI_EngineType()
		{
			cusClassPartPivot.CI_EngineType = EngineTypeCodeList.Codes.CG;
			AssertNoMessageErrors(cusClassPartPivot.CI_EngineTypeInfo);
			cusClassPartPivot.CI_EngineType = "ZZ";
			AssertHasMessageError(cusClassPartPivot.CI_EngineTypeInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_EngineType = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_EngineTypeInfo);
		}

		public void TestCheckCI_LHD()
		{
			cusClassPartPivot.CI_LHD = LeftSideSteeringCodeList.Codes.Left;
			AssertNoMessageErrors(cusClassPartPivot.CI_LHDInfo);
			cusClassPartPivot.CI_LHD = "Z";
			AssertHasMessageError(cusClassPartPivot.CI_LHDInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_LHD = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_LHDInfo);
		}

		public void TestCheckCI_HasCatalystConverter()
		{
			cusClassPartPivot.CI_HasCatalystConverter = CatalystConverterPrintModeList.Codes.Yes;
			AssertNoMessageErrors(cusClassPartPivot.CI_HasCatalystConverterInfo);
			cusClassPartPivot.CI_HasCatalystConverter = "Z";
			AssertHasMessageError(cusClassPartPivot.CI_HasCatalystConverterInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_HasCatalystConverter = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_HasCatalystConverterInfo);
		}

		public void TestCheckCI_EquipmentPrintMode()
		{
			cusClassPartPivot.CI_EquipmentPrintMode = EquipmentPrintModeList.Codes.EEC;
			AssertNoMessageErrors(cusClassPartPivot.CI_EquipmentPrintModeInfo);
			cusClassPartPivot.CI_EquipmentPrintMode = "ZZZ";
			AssertHasMessageError(cusClassPartPivot.CI_EquipmentPrintModeInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_EquipmentPrintMode = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_EquipmentPrintModeInfo);
		}

		public void TestCheckCI_CarCondition()
		{
			cusClassPartPivot.CI_CarCondition = CarConditionCodeList.Codes.NewTruck;
			AssertNoMessageErrors(cusClassPartPivot.CI_CarConditionInfo);
			cusClassPartPivot.CI_CarCondition = "Z";
			AssertHasMessageError(cusClassPartPivot.CI_CarConditionInfo, ListValidation.InvalidCodeMessageError);
			cusClassPartPivot.CI_CarCondition = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_CarConditionInfo);
		}

		public void TestCheckCI_ModelYear()
		{
			cusClassPartPivot.CI_ModelYear = 2019;
			AssertNoMessageErrors(cusClassPartPivot.CI_ModelYearInfo);
			cusClassPartPivot.CI_ModelYear = 105;
			AssertHasMessageError(cusClassPartPivot.CI_ModelYearInfo, "Model Year should be 1000-9999.");
			cusClassPartPivot.CI_ModelYear = ZShort.Zero;
			AssertNoMessageErrors(cusClassPartPivot.CI_ModelYearInfo);
		}

		[ExpectNoExceptions]
		public void TestCheckCI_Displacement()
		{
			NUnit.Framework.Assert.That(cusClassPartPivot.CI_DisplacementInfo.MaxLength, NUnit.Framework.Is.EqualTo(9), "Pre-requisite to test");
			cusClassPartPivot.CI_Displacement = "12345678";
			AssertNoMessageErrors(cusClassPartPivot.CI_DisplacementInfo);
			cusClassPartPivot.CI_Displacement = "123456789";
			AssertHasMessageErrorContaining("The field should not store 9 numbers, it's max length is 9 such that we can store an 8 number value with 2 decimal places (as the decimal place takes up a character)", cusClassPartPivot.CI_DisplacementInfo, "The value should be between");
			cusClassPartPivot.CI_Displacement = "1234567.8";
			AssertNoMessageErrors(cusClassPartPivot.CI_DisplacementInfo);
			cusClassPartPivot.CI_Displacement = "123456.78";
			AssertNoMessageErrors(cusClassPartPivot.CI_DisplacementInfo);
			cusClassPartPivot.CI_Displacement = "123456";
			AssertNoMessageErrors(cusClassPartPivot.CI_DisplacementInfo);
			cusClassPartPivot.CI_Displacement = "123456..8";
			AssertHasMessageError(cusClassPartPivot.CI_DisplacementInfo, "The value should only contain numeric characters.");
			cusClassPartPivot.CI_Displacement = "abc";
			AssertHasMessageError(cusClassPartPivot.CI_DisplacementInfo, "The value should only contain numeric characters.");
			cusClassPartPivot.CI_Displacement = ZString.Empty;
			AssertNoMessageErrors(cusClassPartPivot.CI_DisplacementInfo);
		}

		public void TestCheckCI_EPTDigit()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			var pivot = part.PivotsForBinding.AddNew();
			AssertNoMessageErrors(pivot.CI_EPTDigit1Info);
			AssertNoMessageErrors(pivot.CI_EPTDigit2Info);
			AssertNoMessageErrors(pivot.CI_EPTDigit3Info);
			pivot.CI_TariffNum = "5432102345";
			AssertHasMessageErrorContaining(pivot.CI_EPTDigit1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(pivot.CI_EPTDigit2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(pivot.CI_EPTDigit3Info, MandatoryValidation.YouHaveNotEntered);
			pivot.CI_EPTDigit1 = "1";
			pivot.CI_EPTDigit2 = "A";
			pivot.CI_EPTDigit3 = "A";
			AssertHasMessageError(pivot.CI_EPTDigit1Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(pivot.CI_EPTDigit2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(pivot.CI_EPTDigit3Info, ListValidation.InvalidCodeMessageError);
			pivot.CI_EPTDigit1 = "A";
			pivot.CI_EPTDigit2 = "1";
			pivot.CI_EPTDigit3 = "1";
			AssertNoErrors(pivot.CI_EPTDigit1Info);
			AssertNoErrors(pivot.CI_EPTDigit2Info);
			AssertNoErrors(pivot.CI_EPTDigit3Info);
			pivot.CI_TariffNum = "5432102346";
			pivot.CI_EPTDigit1 = "1";
			pivot.CI_EPTDigit2 = "A";
			pivot.CI_EPTDigit3 = "A";
			AssertNoErrors(pivot.CI_EPTDigit1Info);
			AssertNoErrors(pivot.CI_EPTDigit2Info);
			AssertNoErrors(pivot.CI_EPTDigit3Info);
		}

		void SetUpEnvironmentalProtectionTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "5432102346", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "5432102345", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpEnvironmentalProtectionTariff();
			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			cusClassPartPivot = part.PivotsForBinding.AddNew();
		}

		OrgSupplierPart part;
		CusClassPartPivot cusClassPartPivot;
	}
}
