using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCountryView.Loader))]
	class CusRefTradeGroupCountryViewLoaderTest : LoaderTestCase
	{
		public void TestLoadTradeGroupCountries_FutureStartDate()
		{
			SetupTradeGroupsAndCountries();
			var result = CusRefTradeGroupCountryView.Loader.LoadTradeGroupCountries(Factory, "1010", Core.Constants.CountryCodes.Germany, ZDateTime.Today);
			AssertEquals("group 1010, ES out of Date Range", Core.Constants.CountryCodes.France, result.Single().ZZB_RN_NKTradeGroupCountryCode);
		}

		public void TestLoadTradeGroupCountries_ExpiredDate()
		{
			SetupTradeGroupsAndCountries();
			var groupExpired = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "2000", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-3));
			helper.AddCountry(groupExpired, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var result = CusRefTradeGroupCountryView.Loader.LoadTradeGroupCountries(Factory, "2000", EconomicGroupList.Codes.EuropeanUnion, ZDateTime.Today);
			AssertEquals("group 2000 is expired", 0, result.Length);
		}

		public void TestLoadTradeGroupCountries_ValuationDate()
		{
			SetupTradeGroupsAndCountries();
			var result = CusRefTradeGroupCountryView.Loader.LoadTradeGroupCountries(Factory, "1010", Core.Constants.CountryCodes.Germany, ZDateTime.Today.AddDays(3)).Select(x => x.ZZB_RN_NKTradeGroupCountryCode);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France }, result);
		}

		public void TestLoadTradeGroupCountries_DifferentTradeGroupAndDataGrouping()
		{
			SetupTradeGroupsAndCountries();
			var result = CusRefTradeGroupCountryView.Loader.LoadTradeGroupCountries(Factory, "1012", Core.Constants.CountryCodes.Germany, ZDateTime.Today);
			AssertEquals("group 1012 is AU", 0, result.Length);
		}

		public void TestLoadTradeGroupCountries_EmptyTradeGroupWithDataGroupingCode()
		{
			SetupTradeGroupsAndCountries();
			var result = CusRefTradeGroupCountryView.Loader.LoadTradeGroupCountries(Factory, ZString.Empty, Core.Constants.CountryCodes.Germany, ZDateTime.Today).Select(x => x.ZZB_RN_NKTradeGroupCountryCode);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.UnitedKingdom }, result);
		}

		public void TestGetCachedListTradeGroupCountries()
		{
			var group1010 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var groupFR = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(groupFR, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			CombineAssertions(() =>
			{
				var countryList = CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, EconomicGroupList.Codes.EuropeanUnion);
				AssertSame("Cached", countryList, CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, EconomicGroupList.Codes.EuropeanUnion));
				AssertEquals("ES, FR", countryList.CodesAsString);
			}

			);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusRefTradeGroupCountryView.Loader(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		UniversalReferenceTestDataHelper helper;
		void SetupTradeGroupsAndCountries()
		{
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia);
			var group1010 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var groupGB = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var group1011 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "1011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var group1012 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Australia, "1012", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.Spain, ZDateTime.Today.AddDays(2).Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(groupGB, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1011, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1012, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
		}
	}
}
