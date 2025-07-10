using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	abstract class BaseApplicableRateLoaderAbstractTest : TestCaseWithFactory
	{
		protected void AssertCriteriaSetCached(string criteriaDesc, RateLoadTariffCriteriaSet criteriaSet, bool expected)
		{
			AssertEquals($"Is criteria [{criteriaDesc}] cached?", expected, criteriaSet.IsCached());
		}

		protected void AssertLoadedRateCount(IEnumerable<RateView> loadedRates, string rateCode, int expected)
		{
			AssertEquals($"{rateCode} count", expected, loadedRates.Count(r => r.RateCode == rateCode));
		}

		protected RateView CreateRate(TariffView tariff, RefCusRateType rateType, string rateCode, ZGuid? preferencePk = null, string orderNumber = "", string additionalCode = "", CusRefTradeGroupView secondTradeGroup = null)
		{
			var cusRateCode = helper.CreateCusRateCode(Factory, rateCode, rateType.PK);
			var rate = helper.CreateRate(tariff, cusRateCode.PK, startDate, endDate, preferencePk: preferencePk, dataGrouping: EunGroupCode);
			helper.CreateCusApplicability(rate, stdTradeGroup, startDate, endDate, additionalCode, orderNumber, secondTradeGroup: secondTradeGroup);
			return rate;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(EunGroupCode);
			tariffType = helper.CreateTariffType(EunGroupCode, "T1");
			stdTradeGroup = helper.CreateTradeGroup(EunGroupCode, "STANDARD", startDate, endDate);
			Factory.Save();
		}

		protected string EunGroupCode => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
		protected RefCusTariffType tariffType;
		protected CusRefTradeGroupView stdTradeGroup;
		protected ZDateTime startDate = ZDateTime.Today.AddYears(-1);
		protected ZDateTime endDate = ZDateTime.Today.AddYears(1);
		protected ZDateTime testDate = ZDateTime.Today;
		protected UniversalReferenceTestDataHelper helper;
	}
}
