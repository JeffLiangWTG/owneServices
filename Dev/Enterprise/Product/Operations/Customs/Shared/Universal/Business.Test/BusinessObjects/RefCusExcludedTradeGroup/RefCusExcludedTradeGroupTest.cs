using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusExcludedTradeGroup))]
	public class RefCusExcludedTradeGroupTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var tradeGroup = Helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedKingdom, "GRP", ZDateTime.Now, ZDateTime.Now, "Test trade group");
			var tradeGroupNoThanks = Helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.UnitedKingdom, "XXX", ZDateTime.Now, ZDateTime.Now, "Naughty trade group");
			Factory.Save();
			var applic = Factory.New<Internal.RefCusApplicability>();
			applic.ZZT_ZZ2_Rate = TestRate.PK;
			applic.ZZT_AdditionalCode = "A";
			applic.ZZT_StartDate = ZDateTime.MinSmallDateTimeValue;
			applic.ZZT_EndDate = ZDateTime.MaxSmallDateTimeValue;
			applic.ZZT_OrderNumber = "O";
			applic.ZZT_ZZA_TradeGroup = tradeGroup.PK;
			var excludedGroup = Factory.New<RefCusExcludedTradeGroup>();
			excludedGroup.ZZC_ZZT_Applicability = applic.PK;
			excludedGroup.ZZC_ZZA_TradeGroup = tradeGroupNoThanks.PK;
			return excludedGroup;
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

		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMM3TRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
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
