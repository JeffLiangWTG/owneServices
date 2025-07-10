using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RateViewCollection))]
	class RateViewCollectionTest : ActiveBusinessObjectCollectionTestCase<RateViewCollection>
	{
		public void TestGetRatesFor()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var rate1 = helper.CreateRate(cusTariff, ZGuid.Empty, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var rate2 = helper.CreateRate(cusTariff, ZGuid.Empty, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 5, 1));
			var rate3 = helper.CreateRate(cusTariff, ZGuid.Empty, new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 1));
			var rate4 = helper.CreateRate(cusTariff, ZGuid.Empty, new ZDateTime(2016, 5, 1), new ZDateTime(2016, 7, 1));
			var rates = cusTariff.Rates.GetRatesFor(new ZDateTime(2016, 6, 1)).ToArray();
			AssertEquals(2, rates.Length);
			AssertCollectionContains(rate1, rates);
			AssertCollectionContains(rate4, rates);
		}

		public void TestTariffNationalCodeRelatedData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var rate1 = helper.CreateRate(tariff1, ZGuid.Empty, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var rate2 = helper.CreateRate(tariff1, ZGuid.Empty, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 5, 1));
			var rate3 = helper.CreateRate(tariff1, ZGuid.Empty, new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 1));
			var rate4 = helper.CreateRate(tariff1, ZGuid.Empty, new ZDateTime(2016, 5, 1), new ZDateTime(2016, 7, 1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var tariff2rate = helper.CreateRate(tariff2, ZGuid.Empty, new ZDateTime(2016, 5, 1), new ZDateTime(2016, 7, 1));
			var tariffNationalCode = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, tariff1.PK, "TNC", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), new ZDate(2010, 12, 9), "dummy Description 0", ensureDataGroupingExists: false);
			Factory.Save();
			var rate5 = helper.CreateRate(tariffNationalCode, ZGuid.Empty, new ZDateTime(2016, 1, 2), new ZDateTime(2016, 12, 1));
			var rate6 = helper.CreateRate(tariffNationalCode, ZGuid.Empty, new ZDateTime(2016, 1, 2), new ZDateTime(2016, 5, 1));
			var rate7 = helper.CreateRate(tariffNationalCode, ZGuid.Empty, new ZDateTime(2016, 7, 2), new ZDateTime(2016, 12, 1));
			var rate8 = helper.CreateRate(tariffNationalCode, ZGuid.Empty, new ZDateTime(2016, 5, 2), new ZDateTime(2016, 7, 1));
			var rates = tariffNationalCode.Rates;
			AssertEquals(8, rates.Count);
			AssertCollectionContains(rate1, rates);
			AssertCollectionContains(rate2, rates);
			AssertCollectionContains(rate3, rates);
			AssertCollectionContains(rate4, rates);
			AssertCollectionContains(rate5, rates);
			AssertCollectionContains(rate6, rates);
			AssertCollectionContains(rate7, rates);
			AssertCollectionContains(rate8, rates);
		}

		protected override RateViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new RateViewCollection(cusTariff);
		}
	}
}
