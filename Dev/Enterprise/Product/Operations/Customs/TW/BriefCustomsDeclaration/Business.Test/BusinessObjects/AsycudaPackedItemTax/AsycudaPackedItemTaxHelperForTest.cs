using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Constants = Enterprise.Customs.TW.Business.Constants;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	public static class AsycudaPackedItemTaxHelperForTest
	{
		public static void CreateTariffData(BusinessObjectFactory factory)
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "DTA", rateType.PK);
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "DTS", rateType.PK);
			var preference1 = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var preference2 = universalTestHelper.CreatePreferenceForCountry("PR2", "PR2", Core.Constants.CountryCodes.Taiwan);
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var ctTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			var ssTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var ttTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TT");
			var tstTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TST");

			var ssgRateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "SSG");
			var comRateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var tatRateCode = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "TAT", comRateType.PK);
			var hwsRateCode = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "HWS", comRateType.PK);
			var catRateCode = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "CAT", comRateType.PK);
			var ssgRateCode = universalTestHelper.LoadOrCreateNewCusRateCode(factory, "SSG", ssgRateType.PK);
			var tradeGroupAllCountry = universalTestHelper.LoadOrCreateTradeGroup("TW", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
			var tariff1 = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateDTA1 = universalTestHelper.CreateRate(tariff1, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference1.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var rateDTA2 = universalTestHelper.CreateRate(tariff1, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.16 * VFD", preference2.PK, "0.16", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var rateDTS1 = universalTestHelper.CreateRate(tariff1, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "170 * [LTR]", preference1.PK, "170/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var rateDTS2 = universalTestHelper.CreateRate(tariff1, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "180 * [KGM]", preference2.PK, "180/KGM", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariff2 = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateDTA3 = universalTestHelper.CreateRate(tariff2, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.17 * VFD", preference2.PK, "0.17", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA3, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var rateDTS3 = universalTestHelper.CreateRate(tariff2, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "190 * [LTR]", preference2.PK, "190/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS3, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			universalTestHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariff1);
			universalTestHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariff1);
			universalTestHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "B", tariff2);
			universalTestHelper.CreateNewOrGetExistingTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "C", tariff2);
			factory.Save();
			var atChildtariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "ATTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(atChildtariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(atChildtariff.PK, hsnTariffType.PK, "87031000003");
			var atRate1 = universalTestHelper.CreateRate(atChildtariff, tatRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "26 * [LTR]", rateFormulaDeriveFrom: "26/LTR");
			universalTestHelper.CreateCusApplicability(atRate1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "ATDoesNotBelongToMainTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var ctChildtariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ctTariffType.PK, "CTTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Test Desc");
			universalTestHelper.CreateTariffRelationship(ctChildtariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(ctChildtariff.PK, hsnTariffType.PK, "87031000003");
			var ctRate1 = universalTestHelper.CreateRate(ctChildtariff, catRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2 * VFD", rateFormulaDeriveFrom: "0.2");
			universalTestHelper.CreateCusApplicability(ctRate1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ctTariffType.PK, "CTDoesNotBelongToMainTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var ssChildtariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SSTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(ssChildtariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(ssChildtariff.PK, hsnTariffType.PK, "87031000003");
			var ssRate1 = universalTestHelper.CreateRate(ssChildtariff, ssgRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "150 * [LTR]", rateFormulaDeriveFrom: "150/LTR");
			universalTestHelper.CreateCusApplicability(ssRate1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var sedanChildTariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(sedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(sedanChildTariff.PK, hsnTariffType.PK, "87031000003");
			var ssRate2 = universalTestHelper.CreateRate(sedanChildTariff, ssgRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.05*VFD", rateFormulaDeriveFrom: "0.05");
			universalTestHelper.CreateCusApplicability(ssRate2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var asusChildTariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "ASUS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(asusChildTariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(asusChildTariff.PK, hsnTariffType.PK, "87031000003");
			var ssRate3 = universalTestHelper.CreateRate(asusChildTariff, ssgRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "IF(UnitCustomsValue >= 3000000,0.1 * VFD,0)", rateFormulaDeriveFrom: "IF(UnitCustomsValue >= 3000000,0.1 * VFD,0)");
			universalTestHelper.CreateCusApplicability(ssRate3, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var oppoChildTariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "OPPO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(oppoChildTariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(oppoChildTariff.PK, hsnTariffType.PK, "87031000003");
			var ssRate4 = universalTestHelper.CreateRate(oppoChildTariff, ssgRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "IF(UnitCustomsValue >= 500000,0.1 * VFD,0)", rateFormulaDeriveFrom: "IF(UnitCustomsValue >= 500000,0.1 * VFD,0)");
			universalTestHelper.CreateCusApplicability(ssRate4, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SSDoesNotBelongToMainTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var ttChildtariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ttTariffType.PK, "TTTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Test Desc");
			universalTestHelper.CreateTariffRelationship(ttChildtariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(ttChildtariff.PK, hsnTariffType.PK, "87031000003");
			var ttRate1 = universalTestHelper.CreateRate(ttChildtariff, tatRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "1590 * [KGM]", rateFormulaDeriveFrom: "1590/KGM");
			universalTestHelper.CreateCusApplicability(ttRate1, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var ttRate2 = universalTestHelper.CreateRate(ttChildtariff, hwsRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "1000 * [KGM]", rateFormulaDeriveFrom: "1000/KGM");
			universalTestHelper.CreateCusApplicability(ttRate2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, ttTariffType.PK, "TTDoesNotBelongToMainTariff", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var testSedanChildTariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, tstTariffType.PK, "TESTSEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Test Desc");
			universalTestHelper.CreateTariffRelationship(testSedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			universalTestHelper.CreateTariffRelationship(testSedanChildTariff.PK, hsnTariffType.PK, "87031000003");
			factory.Save();
		}

		public static void CreateTPFRate(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tpfFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			tpfFee.ZZF_Threshold = 100m;
			factory.Save();
		}

		public static void CreateVATRate(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var vatFee = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, 0, 0, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			vatFee.ZZF_Description = "Business tax1";
			factory.Save();
		}
	}
}
