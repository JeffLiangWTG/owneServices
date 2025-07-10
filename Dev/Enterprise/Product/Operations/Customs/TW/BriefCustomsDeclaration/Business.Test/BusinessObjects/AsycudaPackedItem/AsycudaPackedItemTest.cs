using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TransportTypeList = Enterprise.Customs.Business.TransportTypeList;
using UniversalReferenceConstants = Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateLinePriceOrQuantityWhenUnitPriceChanges()
		{
			packedItem.API_GoodsValue = 0m;
			packedItem.API_CustomsQty = 1m;
			packedItem.API_UnitPrice = 10m;
			AssertEquals("Defaulting Goods Value", 10m, packedItem.API_GoodsValue);

			packedItem.API_UnitPrice = 0m;
			packedItem.API_CustomsQty = 0;
			packedItem.API_GoodsValue = 100m;
			packedItem.API_UnitPrice = 33.33m;
			AssertEquals("Defaulting Quantity", 3.0003m, packedItem.API_CustomsQty);

			packedItem.API_UnitPrice = 20m;
			CombineAssertions("Not Recalculated when has value", () =>
			{
				AssertEquals("Goods Value", 100m, packedItem.API_GoodsValue);
				AssertEquals("Quantity", 3.0003m, packedItem.API_CustomsQty);
			});
		}

		public void TestUpdateLinePriceOrUnitPriceWhenQuantityChanges()
		{
			packedItem.API_GoodsValue = 0m;
			packedItem.API_UnitPrice = 105.4545m;
			packedItem.API_CustomsQty = 550m;
			AssertEquals("Defaulting Goods Value", 57999.98m, packedItem.API_GoodsValue);

			packedItem.API_CustomsQty = 0m;
			packedItem.API_UnitPrice = 0m;
			packedItem.API_GoodsValue = 10m;
			packedItem.API_CustomsQty = 3m;
			AssertEquals("Defaulting Unit Price", 3.333333m, packedItem.API_UnitPrice);

			packedItem.API_CustomsQty = 4m;
			CombineAssertions("Not Recalculated when has value", () =>
			{
				AssertEquals("Goods Value", 10m, packedItem.API_GoodsValue);
				AssertEquals("Unit Price", 3.333333m, packedItem.API_UnitPrice);
			});
		}

		public void TestUpdateUnitPriceOrQuantityWhenUnitPriceChanges()
		{
			packedItem.API_UnitPrice = 0m;
			packedItem.API_CustomsQty = 3m;
			packedItem.API_GoodsValue = 100m;
			AssertEquals("Defaulting Unit Price", 33.333333m, packedItem.API_UnitPrice);

			packedItem.API_GoodsValue = 0m;
			packedItem.API_CustomsQty = 0m;
			packedItem.API_UnitPrice = 33.33m;
			packedItem.API_GoodsValue = 100m;
			AssertEquals("Defaulting Quantity", 3.0003m, packedItem.API_CustomsQty);

			packedItem.API_GoodsValue = 99.99m;
			CombineAssertions("Not Recalculated when has value", () =>
			{
				AssertEquals("Unit Price", 33.33m, packedItem.API_UnitPrice);
				AssertEquals("Quantity", 3.0003m, packedItem.API_CustomsQty);
			});
		}

		public void TestValidationType()
		{
			AssertType<AsycudaPackedItemValidation>(packedItem.Validation);
		}

		public void TestAPI_Tariff_Attribute()
		{
			AssertEquals("MaxLength", 11, packedItem.API_TariffInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestPreference()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = Core.Constants.CountryCodes.Taiwan;
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupStandard = universalReferenceTestDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			universalReferenceTestDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);
			universalReferenceTestDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Taiwan, date1, date4);
			Factory.Save();

			var hsnTariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "HSN");
			Factory.Save();
			var dutyRateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCodeDTA = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", dutyRateType.PK);
			Factory.Save();

			var preferencePRE = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Preference, "Preference", "TW");
			var preferenceSTD = universalReferenceTestDataHelper.CreatePreferenceForCountry(Constants.PreferenceCodes.Standard, "Standard", "TW");

			Factory.Save();

			var cusTariff = universalReferenceTestDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "123456789", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = universalReferenceTestDataHelper.CreateRate(cusTariff, rateCodeDTA.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			universalReferenceTestDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4);
			Factory.Save();

			header.DeclarationDate = new ZDateTime(2023, 9, 25);
			AssertEquals("API_Preference", ZString.Empty, packedItem.API_Preference);

			packedItem.API_Tariff = "123456789";
			packedItem.API_RN_NKGoodsOrigin = "AU";
			AssertEquals("API_Preference", Constants.PreferenceCodes.Standard, packedItem.API_Preference);

			Factory.Save();
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, packedItem.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, AsycudaPackedItem.Schema.API_Preference);
			var genAddOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertEquals(Constants.PreferenceCodes.Standard, genAddOnColumn.XA_Data);
		}

		public void TestAdValoremDuty()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = universalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffBoth = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariffBothRateDTS = universalTestHelper.CreateRate(tariffBoth, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffBothRateDTA = universalTestHelper.CreateRate(tariffBoth, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(tariffBothRateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			packedItem.API_Tariff = "21039090200";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedAdValoremDutyRate", "15%", packedItem.FormattedAdValoremDutyRate);
				AssertEquals("AdValoremDuty", "0.15", packedItem.AdValoremRateFormulaDerivedFrom);
			});

			packedItem.API_Tariff = "21039090201";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedAdValoremDutyRate", ZString.Empty, packedItem.FormattedAdValoremDutyRate);
				AssertEquals("AdValoremDuty", ZString.Empty, packedItem.AdValoremRateFormulaDerivedFrom);
			});

			packedItem.API_Tariff = "21039090202";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedAdValoremDutyRate", "15%", packedItem.FormattedAdValoremDutyRate);
				AssertEquals("AdValoremDuty", "0.15", packedItem.AdValoremRateFormulaDerivedFrom);
			});
		}

		public void TestSpecificDuty()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "US", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "US", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090201", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = universalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "150 * [LTR]", preference.PK, "150/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariff21039090202 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "21039090202", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate21039090202 = universalTestHelper.CreateRate(tariff21039090202, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "160 * [LTR]", preference.PK, "160/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rate21039090202, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			packedItem.API_Tariff = "21039090200";
			packedItem.API_RN_NKGoodsOrigin = "US";
			packedItem.API_Preference = "PR1";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedSpecificDutyRate", ZString.Empty, packedItem.FormattedSpecificDutyRate);
				AssertEquals("SpecificDuty", ZString.Empty, packedItem.SpecificDutyRateFormulaDerivedFrom);
			});

			packedItem.API_Tariff = "21039090202";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedSpecificDutyRate", "160/LTR", packedItem.FormattedSpecificDutyRate);
				AssertEquals("SpecificDuty", "160/LTR", packedItem.SpecificDutyRateFormulaDerivedFrom);
			});

			packedItem.API_Preference = "STD";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedSpecificDutyRate", ZString.Empty, packedItem.FormattedSpecificDutyRate);
				AssertEquals("SpecificDuty", ZString.Empty, packedItem.SpecificDutyRateFormulaDerivedFrom);
			});

			packedItem.API_Tariff = "21039090201";
			packedItem.API_Preference = "PR1";
			CombineAssertions(() =>
			{
				AssertEquals("FormattedSpecificDutyRate", "150/LTR", packedItem.FormattedSpecificDutyRate);
				AssertEquals("SpecificDuty", "150/LTR", packedItem.SpecificDutyRateFormulaDerivedFrom);
			});
		}

		public void TestSetUpdateCustomsUQ2AfterAPI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "00000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "00000000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "00000000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "SET");
			helper.CreateTariffUOM(tariff2, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "MWHORAXX");
			helper.CreateTariffUOM(tariff3, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			Factory.Save();

			packedItem.API_Tariff = "00000000001";
			AssertEquals("SET", packedItem.API_CustomsUQ2);

			packedItem.API_CustomsQty2 = 1m;
			packedItem.API_Tariff = "00000000002";
			CombineAssertions(() =>
			{
				AssertEquals("MWHORA", packedItem.API_CustomsUQ2);
				AssertEquals(1m, packedItem.API_CustomsQty2);
			});

			packedItem.API_Tariff = "00000000003";
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, packedItem.API_CustomsUQ2);
				AssertEquals(0m, packedItem.API_CustomsQty2);
			});
		}

		public void TestUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.Import);
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "11081990009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, impTariffType.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			packedItem.API_Tariff = "11081990009";

			AssertNotNull("HSN", packedItem.UniversalTariff);

			packedItem.API_Tariff = "01012100006";
			AssertNull("non-HSN", packedItem.UniversalTariff);
		}

		public void TestAPI_LineNo()
		{
			packedItem.API_LineNo = 1;
			packedItem.API_LineNo = 0;
			AssertEquals("Sequence cannot be 0.", (ZShort)1, packedItem.API_LineNo);
		}

		public void TestAPI_GoodsDescription_Attribute()
		{
			AssertEquals("MaxLength", 512, packedItem.API_GoodsDescriptionInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestPackedItemFormattedTariff_Attributes()
		{
			AssertEquals("MaxLength", 15, packedItem.API_FormattedTariffInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestAPI_FormattedTariff_Format()
		{
			packedItem.API_FormattedTariff = "11081990009";
			AssertEquals("1108.19.90.00-9", packedItem.API_FormattedTariff);
		}

		public void TestITariffFormatProvider()
		{
			AssertType<TaiwanTariffFormatter>(((ITariffFormatProvider)packedItem).TariffFormatter);
		}

		public void TestModeOfStatisticsOrDutyTreatment()
		{
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			AssertEquals(Constants.ProcedureCodes._31, packedItem.ModeOfStatisticsOrDutyTreatment);

			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals(Constants.ProcedureCodes._02, packedItem.ModeOfStatisticsOrDutyTreatment);
		}

		public void TestAPI_NetWeightInKG()
		{
			packedItem.API_NetWeight = 1000m;
			packedItem.API_NetWeightUQ = "G";
			AssertEquals(1m, packedItem.PackedItemNetWeightInKG);
		}

		public void TestAPI_CustomsValue_DecimalPlaces()
		{
			AssertEquals("DecimalPlaces", 0, packedItem.API_CustomsValueInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestDefaultDutyWhenRelateFieldChanged()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			bill.ABL_CustomsValue = 0m;
			packedItem.API_FormattedTariff = "87031000002";
			packedItem.API_Preference = "PR1";
			packedItem.API_RN_NKGoodsOrigin = "US";
			var dtaTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTA).FirstOrDefault();
			var dtsTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTS).FirstOrDefault();

			CombineAssertions("ABL_CustomsValue < 2000, DTA RateFormula: 0.15 * VFD, DTS RateFormula: 170 * [LTR]", () =>
			{
				AssertNull(dtaTax);
				AssertNull(dtsTax);
			});

			bill.ABL_CustomsValue = 2000m;
			packedItem.API_RN_NKGoodsOrigin = "TW";
			dtaTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTA).FirstOrDefault();
			dtsTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTS).FirstOrDefault();
			CombineAssertions("ABL_CustomsValue >= 2000, DTA RateFormula: 0.15 * VFD, DTS RateFormula: 170 * [LTR]", () =>
			{
				AssertEquals("DTA Of Rate", 0.15m, dtaTax.AET_Rate);
				AssertEquals("DTA Of MethodOfCalculation", "%", dtaTax.AET_MethodOfCalculation);
				AssertEquals("DTS Of Rate", 170m, dtsTax.AET_Rate);
				AssertEquals("DTS Of MethodOfCalculation", "LTR", dtsTax.AET_MethodOfCalculation);
			});
			packedItem.API_Preference = "PR2";
			dtaTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTA).FirstOrDefault();
			dtsTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTS).FirstOrDefault();
			CombineAssertions("ABL_CustomsValue >= 2000, DTA RateFormula: 0.16 * VFD, DTS RateFormula: 180 * [KGM]", () =>
			{
				AssertEquals("DTA Of Rate", 0.16m, dtaTax.AET_Rate);
				AssertEquals("DTA Of MethodOfCalculation", "%", dtaTax.AET_MethodOfCalculation);
				AssertEquals("DTS Of Rate", 180m, dtsTax.AET_Rate);
				AssertEquals("DTS Of MethodOfCalculation", "KGM", dtsTax.AET_MethodOfCalculation);
			});
			packedItem.API_FormattedTariff = "87031000003";
			dtaTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTA).FirstOrDefault();
			dtsTax = packedItem.AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTS).FirstOrDefault();
			CombineAssertions("ABL_CustomsValue >= 2000, DTA RateFormula: 0.17 * VFD, DTS RateFormula: 190 * [LTR]", () =>
			{
				AssertEquals("DTA Of Rate", 0.17m, dtaTax.AET_Rate);
				AssertEquals("DTA Of MethodOfCalculation", "%", dtaTax.AET_MethodOfCalculation);
				AssertEquals("DTS Of Rate", 190m, dtsTax.AET_Rate);
				AssertEquals("DTS Of MethodOfCalculation", "LTR", dtsTax.AET_MethodOfCalculation);
			});
		}

		public void TestVATBaseAmount()
		{
			bill.ABL_CustomsValue = 2000m;
			packedItem.API_CustomsValue = 500000m;
			AssertEquals(500000m, packedItem.VATBaseAmount);

			var asycudaTaxes = packedItem.AsycudaTaxes;
			AddTax(ChargeTypeOtherList.Codes.AT, 100m);
			AddTax(ChargeTypeOtherList.Codes.CT, 200m);
			AddTax(ChargeTypeOtherList.Codes.TT, 300m);
			AddTax(ChargeTypeOtherList.Codes.HWS, 400m);
			AddTax(ChargeTypeOtherList.Codes.DTA, 500m);
			AddTax(ChargeTypeOtherList.Codes.DTS, 600m);
			AssertEquals(501600m, packedItem.VATBaseAmount);

			AddTax(ChargeTypeOtherList.Codes.DTA, 200m);
			AssertEquals(501700m, packedItem.VATBaseAmount);

			void AddTax(ZString type, ZDecimal amount)
			{
				var tax = asycudaTaxes.AddNew();
				tax.AET_ChargeType = type;
				tax.AET_ChargeAmount = amount;
			}
		}

		[TestDate(2025, 4, 21)]
		public void TestTaxesAmountWhenAPI_CustomsValueChanged()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			AsycudaPackedItemTaxHelperForTest.CreateTPFRate(Factory);
			AsycudaPackedItemTaxHelperForTest.CreateVATRate(Factory);
			Factory.ClearCachedValue<ZDecimal>($"TW|TPF|{ZDateTime.Today}");
			Factory.ClearCachedValue<ZDecimal>($"TW|VAT|{ZDateTime.Today}");
			bill.ABL_CustomsValue = 2000m;
			packedItem.API_CustomsValue = 500000m;
			var tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			tax.AET_Tariff = "ASUS";
			var asycudaTaxes = packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>();
			var vatTax = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT);
			var tpfTax = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.TPF);
			CombineAssertions("ZZ2_RateFormula is IF(UnitCustomsValue >= 3000000,0.1 * VFD,0) and CustomsValue = 500,000", () =>
			{
				AssertEquals(0m, tax.AET_Rate);
				AssertEquals(500000m, tax.AET_BaseValue);
				AssertEquals(0m, tax.AET_ChargeAmount);
				AssertEquals(25000m, vatTax.AET_ChargeAmount);
				AssertEquals(200m, tpfTax.AET_ChargeAmount);
			});

			packedItem.API_CustomsValue = 3000000m;
			asycudaTaxes = packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>();
			vatTax = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT);
			tpfTax = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.TPF);
			CombineAssertions("ZZ2_RateFormula is IF(UnitCustomsValue >= 3000000,0.1 * VFD,0) and CustomsValue = 3,000,000", () =>
			{
				AssertEquals(0.1m, tax.AET_Rate);
				AssertEquals(3000000m, tax.AET_BaseValue);
				AssertEquals(300000m, tax.AET_ChargeAmount);
				AssertEquals(150000m, vatTax.AET_ChargeAmount);
				AssertEquals(1200m, tpfTax.AET_ChargeAmount);
			});
		}

		public void TestTaxesAmountWhenAPI_CustomsQty2Changed()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			bill.ABL_CustomsValue = 2000m;
			packedItem.API_FormattedTariff = "87031000003";
			packedItem.API_CustomsUQ2 = "LTR";
			var asycudaTaxes = packedItem.AsycudaTaxes.Cast<AsycudaPackedItemTax>().ToList();
			var taxAT = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.AT);
			taxAT.AET_Tariff = "ATTariff";
			var taxTT = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.TT);
			taxTT.AET_Tariff = "TTTariff";
			var taxHWS = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.HWS);
			packedItem.API_CustomsQty2 = 5m;
			AssertEquals(5m, taxAT.AET_BaseValue);
			AssertEquals(0m, taxTT.AET_BaseValue);
			AssertEquals(0m, taxHWS.AET_BaseValue);

			packedItem.API_NetWeight = 6m;
			taxAT = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.AT);
			taxTT = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.TT);
			taxHWS = asycudaTaxes.FirstOrDefault(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.HWS);
			AssertEquals(5m, taxAT.AET_BaseValue);
			AssertEquals(6m, taxTT.AET_BaseValue);
			AssertEquals(6m, taxHWS.AET_BaseValue);
		}

		public void TestTaxesAmountWhenAPI_NetWeightChanged()
		{
			var tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			tax.AET_Rate = 10m;
			tax.AET_MethodOfCalculation = "KGM";
			packedItem.API_NetWeight = 140m;
			packedItem.API_NetWeightUQ = "KG";
			CombineAssertions(() =>
			{
				AssertEquals(10m, tax.AET_Rate);
				AssertEquals(140m, tax.AET_BaseValue);
				AssertEquals(1400m, tax.AET_ChargeAmount);
			});

			packedItem.API_NetWeight = 150m;
			CombineAssertions(() =>
			{
				AssertEquals(10m, tax.AET_Rate);
				AssertEquals(150m, tax.AET_BaseValue);
				AssertEquals(1500m, tax.AET_ChargeAmount);
			});
		}

		public void TestTaxesAmountWhenAPI_CustomsQtyChanged()
		{
			var tax = packedItem.AsycudaTaxes.AddNew();
			tax.AET_ChargeType = ChargeTypeOtherList.Codes.SS;
			tax.AET_Rate = 10m;
			tax.AET_MethodOfCalculation = "DZN";
			packedItem.API_CustomsQty = 120m;
			packedItem.API_CustomsUQ = "SET";
			CombineAssertions(() =>
			{
				AssertEquals(10m, tax.AET_Rate);
				AssertEquals(10m, tax.AET_BaseValue);
				AssertEquals(100m, tax.AET_ChargeAmount);
			});

			packedItem.API_CustomsQty = 240m;
			CombineAssertions(() =>
			{
				AssertEquals(10m, tax.AET_Rate);
				AssertEquals(20m, tax.AET_BaseValue);
				AssertEquals(200m, tax.AET_ChargeAmount);
			});
		}

		public void TestDefaultPackedItemTax()
		{
			AsycudaPackedItemTaxHelperForTest.CreateTariffData(Factory);
			AsycudaPackedItemTaxHelperForTest.CreateTPFRate(Factory);
			bill.ABL_CustomsValue = 0m;
			packedItem.API_CustomsValue = 100m;
			packedItem.AsycudaTaxes.RemoveAndDeleteAll();
			packedItem.API_FormattedTariff = "87031000002";
			AssertEquals("ABL_CustomsValue < 2000", 0, packedItem.AsycudaTaxes.Count);

			bill.ABL_CustomsValue = 2000m;
			packedItem.API_FormattedTariff = "87031000003";
			AssertContainsExactElementsInAnyOrder("The corresponding CustomsRequirements are B and C", new string[] { "VAT", "AT", "TT", "HWS" }, packedItem.AsycudaTaxes.Where(c => c.AET_RateOverrideReasonCode.IsEmpty).Select(t => t.AET_ChargeType));

			packedItem.API_FormattedTariff = "87031000002";
			AssertContainsExactElementsInAnyOrder("The corresponding CustomsRequirements are L* and T", new string[] { "VAT", "CT", "SS" }, packedItem.AsycudaTaxes.Where(c => c.AET_RateOverrideReasonCode.IsEmpty).Select(t => t.AET_ChargeType));

			var vatOverride = packedItem.AsycudaTaxes.AddNew();
			vatOverride.AET_ChargeType = "VAT";
			vatOverride.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;
			packedItem.API_FormattedTariff = "87031000003";
			AssertContainsExactElementsInAnyOrder("The corresponding CustomsRequirements are B and C with VAT Override", new string[] { "AT", "TT", "HWS" }, packedItem.AsycudaTaxes.Where(c => c.AET_RateOverrideReasonCode.IsEmpty).Select(t => t.AET_ChargeType));
		}

		public void TestAPI_CustomsQty()
		{
			var targetInfo = packedItem.API_CustomsQtyInfo;
			CombineAssertions(() =>
			{
				AssertEquals("DecimalPlaces", 5, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
				AssertEquals("DecimalPrecision", 11, targetInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			});
		}

		public void TestAPI_UnitPrice_DecimalPlaces()
		{
			var targetInfo = packedItem.API_UnitPriceInfo;
			CombineAssertions(() =>
			{
				AssertEquals("DecimalPlaces", 6, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
				AssertEquals("DecimalPrecision", 13, targetInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			});
		}

		public void TestAPI_NetWeight()
		{
			var targetInfo = packedItem.API_NetWeightInfo;
			CombineAssertions(() =>
			{
				AssertEquals("DecimalPlaces", 3, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
				AssertEquals("DecimalPrecision", 11, targetInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			});

			packedItem.API_NetWeight = 1m;
			AssertEquals(1m, packedItem.API_NetWeight);

			packedItem.API_NetWeight = -1m;
			AssertEquals(0m, packedItem.API_NetWeight);
		}

		public void TestAPI_CustomsQty2_DecimalPlaces()
		{
			var targetInfo = packedItem.API_CustomsQty2Info;
			CombineAssertions(() =>
			{
				AssertEquals("DecimalPlaces", 4, targetInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
				AssertEquals("DecimalPrecision", 9, targetInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			});
		}

		public void TestAPI_Preference_ReadOnly()
		{
			var targetInfo = packedItem.API_PreferenceInfo;
			AssertEquals(false, targetInfo.ReadOnly);
			packedItem.Bill.Header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestAPI_CustomsQty2_ReadOnly()
		{
			var targetInfo = packedItem.API_CustomsQty2Info;
			AssertEquals(true, targetInfo.ReadOnly);
			packedItem.API_CustomsUQ2 = "PKG";
			AssertEquals(false, targetInfo.ReadOnly);
			packedItem.Bill.Header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals(true, targetInfo.ReadOnly);
		}

		public void TestAPI_CustomsUQ2_ReadOnly()
		{
			Assert(packedItem.API_CustomsUQ2Info.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestCaption()
		{
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_TariffInfo, "Tariff Code", "Tariff");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_LineNoInfo, "Sequence", "Seq.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_GoodsDescriptionInfo, "Goods Description", "Desc.");
			BusinessObjectCaptionTestHelper.AssertCaptions(packedItem.API_ModelInfo, "Model");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_BrandInfo, "Brand Name", "Brand");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(packedItem.API_RemarksInfo, "Specification", "Identifies the element or component element in a commodity including description, percentage, quantity, name, active ingredient and yield amount.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_FormattedTariffInfo, "Tariff Code", "Tariff");
			BusinessObjectCaptionTestHelper.AssertCaptions(packedItem.API_CustomsValueInfo, "Customs Value");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsQtyInfo, "Quantity", "Qty");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsUQInfo, "Quantity Unit", "UQ");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_UnitPriceInfo, "Unit Price", "Unit Price");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_GoodsValueInfo, "Goods Value", "Goods Value");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_NetWeightInfo, "Net Weight", "NW");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_NetWeightUQInfo, "Net Weight Unit", "UQ");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsQty2Info, "Statistical Quantity", "Stats. Qty");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsUQ2Info, "Statistical Quantity Unit", "UQ");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_RN_NKGoodsOriginInfo, "Goods Origin", "Origin");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_PreferenceInfo, "Preference", "Preference");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.FormattedAdValoremDutyRateInfo, "Ad-Valorem Duty Rate", "Ad-Valorem Duty");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.FormattedSpecificDutyRateInfo, "Specific Duty Rate", "Specific Duty");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsBuyerPartNoInfo, "Buyer Part No.", "Buyer Part No.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_CustomsSupplierPartNoInfo, "Supplier Part No.", "Supplier Part No.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_PreviousEntryNoInfo, "Previous Bonded Entry No.", "Entry No.");
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(packedItem.API_PreviousEntryLineNoInfo, "Previous Bonded Entry Line No.", "Line No.");
		}

		public void TestPackedItemCustomsBuyerPartNo_ReadOnly() => AssertPropertyInfoIsReadOnlyWhenImport(packedItem.API_CustomsBuyerPartNoInfo);

		public void TestPackedItemCustomSupplierPartNo_ReadOnly() => AssertPropertyInfoIsReadOnlyWhenImport(packedItem.API_CustomsSupplierPartNoInfo);

		public void TestPackedItemPreviousEntryNo_ReadOnly() => AssertPropertyInfoIsReadOnlyWhenImport(packedItem.API_PreviousEntryNoInfo);

		public void TestPackedItemPreviousEntryLineNo_ReadOnly() => AssertPropertyInfoIsReadOnlyWhenImport(packedItem.API_PreviousEntryLineNoInfo);

		void AssertPropertyInfoIsReadOnlyWhenImport(ZPropertyInfo propertyInfo)
		{
			var header = packedItem.Bill.Header;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			AssertEquals(false, propertyInfo.ReadOnly);
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			AssertEquals(true, propertyInfo.ReadOnly);
		}

		public void TestLookupType()
		{
			AssertType<AsycudaPackedItemLookups>(packedItem.Lookups);
		}

		public void TestIsAir()
		{
			var header = packedItem.Bill.Header;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsAir is true", true, packedItem.IsAir);

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsAir is false", false, packedItem.IsAir);
		}

		public void TestIsExportIsImport()
		{
			var header = packedItem.Bill.Header;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			CombineAssertions(() =>
			{
				AssertEquals("IsExport is true", true, packedItem.IsExport);
				AssertEquals("IsImport is false", false, packedItem.IsImport);
			});
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			CombineAssertions(() =>
			{
				AssertEquals("IsExport is false", false, packedItem.IsExport);
				AssertEquals("IsImport is true", true, packedItem.IsImport);
			});
		}

		public void TestDelete()
		{
			var tax = packedItem.AsycudaTaxes.AddNew();
			packedItem.Delete();
			AssertEquals(true, tax.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = "IMP";
			header.AMA_JobReference = "C4321";
			header.SuspendCheckBusinessObjectType();
			bill = header.Bills.AddNew();
			bill.ABL_CustomsValue = 2000m;
			return bill.PackedItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			header.AMA_JobReference = "AB123";
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		AsycudaPackedItem packedItem;
	}
}
