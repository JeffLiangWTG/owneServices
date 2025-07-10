using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusQuota.Loader))]
	class RefCusQuotaLoaderTest : LoaderTestCase
	{
		public void TestGetFullFilter()
		{
			var plDataGroupingCode = Core.Constants.CountryCodes.Poland;
			var euDataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(euDataGroupingCode, "EUN");
			helper.CreateNewOrGetExistingDataGrouping(plDataGroupingCode, "PL", euDataGrouping);
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = RefCusQuota.Loader.GetFullFilter(Factory, plDataGroupingCode, ZDateTime.Today, "123");
				AssertContains("Not empty DataGrouping", "ZXQ_ZZZ_NKDataGrouping ", query.FilterString);
				AssertContains("Not empty StartDate", "ZXQ_StartDate ", query.FilterString);
				AssertContains("Not empty EndDate", "ZXQ_EndDate ", query.FilterString);
				AssertContains("Not empty OrderNumber", "ZXQ_OrderNumber ", query.FilterString);
				AssertContains("DataGrouping includes parent", "ZXQ_ZZZ_NKDataGrouping in ('EUN', 'PL'))", query.LiteralTextADO);
				Assert("Not empty DataGrouping, Date, OrderNumber - query should not be no result", !query.IsNoResultQuery);
				query = RefCusQuota.Loader.GetFullFilter(Factory, ZString.Empty, ZDateTime.Today, "123");
				Assert("No Data Grouping", query.IsNoResultQuery);
				query = RefCusQuota.Loader.GetFullFilter(Factory, plDataGroupingCode, ZDateTime.Today, ZString.Empty);
				Assert("No Order Number", query.IsNoResultQuery);
				query = RefCusQuota.Loader.GetFullFilter(Factory, plDataGroupingCode, ZDateTime.Empty, "123");
				Assert("No Date", query.IsNoResultQuery);
				query = RefCusQuota.Loader.GetFullFilter(Factory, plDataGroupingCode, ZDateTime.Today, "123", false);
				AssertContains("DataGrouping does not include parent", "ZXQ_ZZZ_NKDataGrouping = 'PL'", query.LiteralTextADO);
			});
		}

		public void TestGetFirst()
		{
			var plDataGroupingCode = Core.Constants.CountryCodes.Poland;
			var euDataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(euDataGroupingCode, "EUN");
			helper.CreateNewOrGetExistingDataGrouping(plDataGroupingCode, "PL", euDataGrouping);
			Factory.Save();

			var date = ZDateTime.Now;
			var orderNumber = "123";
			UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, euDataGroupingCode, 10, 5, orderNumber, "KGM", date.AddDays(-1),
				date.AddDays(1));
			var cusQuota = new RefCusQuota.Loader(Factory).GetFirst(plDataGroupingCode, date, orderNumber);
			CombineAssertions(() =>
			{
				AssertNotNull("includeParentDataGrouping true", cusQuota);
				cusQuota = new RefCusQuota.Loader(Factory).GetFirst(plDataGroupingCode, date, orderNumber, false);
				AssertNull("includeParentDataGrouping false", cusQuota);
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefCusQuota.Loader(Factory);
	}
}
