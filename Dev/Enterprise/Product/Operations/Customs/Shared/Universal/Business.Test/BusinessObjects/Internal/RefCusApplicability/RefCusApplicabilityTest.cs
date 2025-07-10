using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusApplicability))]
	internal class RefCusApplicabilityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZT_TradeGroupDataGrouping()
		{
			var eutrade = Helper.CreateTradeGroup(Core.Constants.CountryCodes.Eritrea, "EUTRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "EU Trade Agreement Jan 2000");
			var testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			var applic = Helper.CreateCusApplicability(testRate, eutrade, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "DJC", "O");
			AssertEquals("ZZT_TradeGroupDataGrouping", Core.Constants.CountryCodes.Eritrea, applic.ZZT_TradeGroupDataGrouping);
		}

		public void TestIsApplicable()
		{
			var mercosurquota = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "MERCOSURQUOTA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "MERCOSUR Quota trade agreement");
			var py = mercosurquota.TradeGroupCountries.AddNew();
			py.ZZB_RN_NKTradeGroupCountryCode = "PY";
			var uy = mercosurquota.TradeGroupCountries.AddNew();
			uy.ZZB_RN_NKTradeGroupCountryCode = "UY";
			var br = mercosurquota.TradeGroupCountries.AddNew();
			br.ZZB_RN_NKTradeGroupCountryCode = "BR";
			var ar = mercosurquota.TradeGroupCountries.AddNew();
			ar.ZZB_RN_NKTradeGroupCountryCode = "AR";
			var group1 = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "BR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "BR");
			var grp1BR = group1.TradeGroupCountries.AddNew();
			grp1BR.ZZB_RN_NKTradeGroupCountryCode = "BR";
			var group2 = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "AR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AR");
			var grp2AR = group2.TradeGroupCountries.AddNew();
			grp2AR.ZZB_RN_NKTradeGroupCountryCode = "AR";
			var applicability = TestRate.RateApplicabilities.AddNew();
			applicability.ZZT_AdditionalCode = "A";
			applicability.ZZT_StartDate = ZDateTime.MinSmallDateTimeValue;
			applicability.ZZT_EndDate = ZDateTime.MaxSmallDateTimeValue;
			applicability.ZZT_OrderNumber = "O";
			applicability.ZZT_ZZA_TradeGroup = mercosurquota.PK;
			var ex1 = applicability.ExcludedTradeGroups.AddNew();
			ex1.ZZC_ZZA_TradeGroup = group1.PK;
			var ex2 = applicability.ExcludedTradeGroups.AddNew();
			ex2.ZZC_ZZA_TradeGroup = group2.PK;
			Assert(applicability.IsApplicable("PY", ZDateTime.Today));
			Assert(applicability.IsApplicable("UY", ZDateTime.Today));
			Assert(!applicability.IsApplicable("BR", ZDateTime.Today));
			Assert(!applicability.IsApplicable("AR", ZDateTime.Today));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var tradeGroup = Helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedKingdom, "GRP", ZDateTime.Now, ZDateTime.Now, "Test trade group");
			Factory.Save();
			var applic = Helper.CreateNewOrLoadExistingCusApplicability(TestRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, additionalCode: "A", orderNumber: "O");
			return applic;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			dutyRateType = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			djcRateCode = Helper.LoadOrCreateNewCusRateCode(Factory, "DJC", dutyRateType.PK);
			Factory.Save();
		}

		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMM1TRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		RateView TestRate => testRate ?? (testRate = Helper.CreateRate(CusTariff, djcRateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0"));
		RateView testRate;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
		RefCusTariffType s1p1TariffType;
		RefCusRateType dutyRateType;
		CusRefRateCodeView djcRateCode;
	}
}
