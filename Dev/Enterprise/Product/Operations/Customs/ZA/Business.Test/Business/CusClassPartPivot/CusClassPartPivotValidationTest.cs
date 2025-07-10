using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2015, 7, 1)]
		public void TestCheckCI_TariffNum()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			Factory.Save();
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = tariff1.ZZ1_TariffCode;
			AssertNoNotifications(pivot.CI_TariffNumInfo);
			pivot.CI_TariffNum = "1234";
			AssertHasMessageError(pivot.CI_TariffNumInfo, ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExists(pivot.CI_TariffNum));
		}

		public void TestCheckCI_PrimaryPreference()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateAdditionalInformationCusCodeEntry("EUR");
			var preference = testHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", startDate, endDate);
			var rateType_ZA_REB = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Universal.Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var tradeGroup1 = testHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, description: "Standard Agreement");
			testHelper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Factory.Save();
			var tariff1Rate = testHelper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			Factory.Save();
			testHelper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			pivot.CI_TariffNum = "1020304050";
			pivot.CI_PrimaryPreference = "X";
			AssertHasMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			AssertNoMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_PrimaryPreference = "X";
			AssertHasMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_PrimaryPreference = "EUR";
			AssertNoMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
			pivot.CI_PrimaryPreference = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.CI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_VehicleFormat()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_VehicleFormat = "X";
			AssertHasMessageErrorContaining(partPivot.CI_VehicleFormatInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_VehicleFormat = VehicleFormatList.Codes.CKD;
			AssertNoMessageErrorContaining(partPivot.CI_VehicleFormatInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_VehicleFormat = ZString.Empty;
			AssertNoMessageErrorContaining(partPivot.CI_VehicleFormatInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_VehicleType()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_VehicleType = "X";
			AssertHasMessageErrorContaining(partPivot.CI_VehicleTypeInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_VehicleType = VehicleTypeList.Codes.Hauler;
			AssertNoMessageErrorContaining(partPivot.CI_VehicleTypeInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_VehicleType = ZString.Empty;
			AssertNoMessageErrorContaining(partPivot.CI_VehicleTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCI_NewUsed()
		{
			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_NewUsed = "X";
			AssertHasMessageErrorContaining(partPivot.CI_NewUsedInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_NewUsed = GoodsTypeList.Codes.U;
			AssertNoMessageErrorContaining(partPivot.CI_NewUsedInfo, ListValidation.InvalidCodeMessageError);
			partPivot.CI_NewUsed = "";
			AssertNoMessageErrorContaining(partPivot.CI_NewUsedInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
