using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RateView.Loader))]
	public class RateViewLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RateView.Loader(Factory);
		}

		[TestDate(2019, 01, 01)]
		public void TestLoadMostRecentCachedRate()
		{
			RateView rateView;
			CombineAssertions(() =>
			{
				var tariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, "EXC");
				var rateType = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Singapore, Constants.RateTypes.Excise, "Excise");
				var rateCode = Helper.LoadOrCreateNewCusRateCode(Factory, "EXC", rateType.PK);
				Factory.Save();
				var tariff = Helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "10011001", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "dummy Description 0");
				var tariff2 = Helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "22022022", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "dummy Description 0");
				var expiredRate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-1), "0", dataGrouping: Core.Constants.CountryCodes.Singapore);
				var lessRecentRate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(1), "0", dataGrouping: Core.Constants.CountryCodes.Singapore);
				var validRate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddYears(1), "0", dataGrouping: Core.Constants.CountryCodes.Singapore);
				var futureRate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddYears(1), ZDateTime.Today.AddYears(2), "0", dataGrouping: Core.Constants.CountryCodes.Singapore);
				Factory.Save();
				rateView = RateView.Loader.LoadMostRecentCachedRate(tariff, "EXC", ZDateTime.Today);
				AssertEquals("Valid Rate Found", validRate.PK, rateView.PK);
				rateView = RateView.Loader.LoadMostRecentCachedRate(tariff, "ABC", ZDateTime.Today);
				AssertNull("Invalid RateType", rateView);
				rateView = RateView.Loader.LoadMostRecentCachedRate(tariff, "EXC", ZDateTime.Today.AddYears(5));
				AssertNull("No rates for date", rateView);
				rateView = RateView.Loader.LoadMostRecentCachedRate(tariff2, "EXC", ZDateTime.Today);
				AssertNull("No rates for tariff", rateView);
				rateView = RateView.Loader.LoadMostRecentCachedRate(tariff, "EXC", ZDateTime.Today.AddYears(-1));
				AssertEquals("ExpiredValid Rate Found", expiredRate.PK, rateView.PK);
			}

			);
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
