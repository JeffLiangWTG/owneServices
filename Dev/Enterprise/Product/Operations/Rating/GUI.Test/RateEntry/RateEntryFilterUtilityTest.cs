using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Rating.GUI.RateEntryFilterUtility;

namespace Enterprise.Rating.GUI.Testing
{
	internal sealed class RateEntryFilterUtilityTest : RatingTestCase
	{
		public void TestContractNumberLinkedFilters_FiltersCorrectly()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var rateContractNumberLinked1 = AddRateEntryWithContractNumber(costing, "L12345", true);
			var rateContractNumberLinked2 = AddRateEntryWithContractNumber(costing, "L54321", true);
			var rateContractNumberNotlinked1 = AddRateEntryWithContractNumber(costing, "NL12345", false);
			var rateContractNumberNotlinked2 = AddRateEntryWithContractNumber(costing, "NL54321", false);
			Factory.Save();

			var collection = costing.EntryCollections[RatingConstants.RateCategory.FCL];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var transportModeFilter = (ModuleFlagsFilter)filterBizO[Constants.Codes.ContractNumberLinked];
			transportModeFilter.IsActive = true;

			transportModeFilter.Property0 = true;
			AssertFilteredCollectionMode($"Unable to correctly filter for linked contract number", filterBizO, new[] { rateContractNumberLinked1, rateContractNumberLinked2 });

			transportModeFilter.Property0 = false;
			AssertFilteredCollectionMode($"Unable to correctly filter for linked contract number", filterBizO, new[] { rateContractNumberNotlinked1, rateContractNumberNotlinked2 });
		}

		public void TestTransportModeFilters_FilterCorrectly_BLK() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.BLK);
		public void TestTransportModeFilters_FilterCorrectly_BBK() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.BBK);
		public void TestTransportModeFilters_FilterCorrectly_ROR() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.ROR);
		public void TestTransportModeFilters_FilterCorrectly_BCN() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.BCN);
		public void TestTransportModeFilters_FilterCorrectly_COU() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.COU);
		public void TestTransportModeFilters_FilterCorrectly_OBC() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.OBC);
		public void TestTransportModeFilters_FilterCorrectly_UNA() => AssertTransportModeFilterCorrectlyFiltersFor(Core.Constants.RateMode.UNA);

		public void TestAddTransportModeFilterQuery_BBK() => AssertTransportModeQuerySQL("BBK",
			"TI_Mode = 'BBK' or ((TI_RateCategory in ('CDS', 'COR', 'CST', 'DST', 'ORG', 'PAC', 'SDE', 'SOR', 'TBC', 'TRN', 'UNP')) and TI_Mode = 'BBK')");

		public void TestAddTransportModeFilterQuery_BLK() => AssertTransportModeQuerySQL("BLK",
			"TI_Mode = 'BLK' or ((TI_RateCategory in ('CDS', 'COR', 'CST', 'DST', 'ORG', 'PAC', 'SDE', 'SOR', 'TBC', 'TRN', 'UNP')) and TI_Mode = 'BLK')");

		public void TestAddTransportModeFilterQuery_ROR() => AssertTransportModeQuerySQL("ROR",
			"TI_Mode = 'ROR' or ((TI_RateCategory in ('CDS', 'COR', 'CST', 'DST', 'ORG', 'PAC', 'SDE', 'SOR', 'TBC', 'TRN', 'UNP')) and TI_Mode = 'ROR')");

		public void TestAddTransportModeFilterQuery_BCN() => AssertTransportModeQuerySQL("BCN",
			"TI_Mode = 'BCN' or ((TI_RateCategory in ('CDS', 'COR', 'CST', 'DST', 'ORG', 'PAC', 'SDE', 'SOR', 'TBC', 'TRN', 'UNP')) and TI_Mode = 'BCN')");

		public void TestAddContainterTypeFilter_AllowsSubFiltersMatch()
		{
			var filtersCollection = new ModuleFilterCollection();
			var containerList = new Mock<IBusinessObjectCollection>().Object;

			var filter = AddContainterTypeFilter(filtersCollection, containerList);

			Assert(filter.HasComparisonOperator);
			Assert(filter.SupportsFiltersMatchComparisonOperator);
		}

		#region helpers

		RateEntry AddRateEntryWithContractNumber(Costing costing, string contractNumber, bool isLinked = false)
		{
			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NKAKL", "WAR", 100);
			entry.TI_ContractNumber = contractNumber;
			entry.TI_ContractNumberLinked = isLinked;

			return entry;
		}

		void AssertTransportModeFilterCorrectlyFiltersFor(string mode)
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var category = RatingConstants.RateCategory.ORG;

			clientRate.AddRateEntry(category, Core.Constants.RateMode.AIR);
			clientRate.AddRateEntry(category, Core.Constants.RateMode.SEA);
			clientRate.AddRateEntry(category, Core.Constants.RateMode.ROA);
			clientRate.AddRateEntry(category, Core.Constants.RateMode.RAI);

			var expected = clientRate.AddRateEntry(category, mode);

			Factory.Save();

			var collection = clientRate.EntryCollections[category];
			var filterBizO = new RateEntryFilterStripBusinessObject(collection);
			var transportModeFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.TransportMode];

			transportModeFilter.IsActive = true;
			transportModeFilter.Property = mode;

			AssertFilteredCollectionMode($"Unable to correctly filter for {mode}", filterBizO, new[] { expected });
		}

		void AssertTransportModeQuerySQL(string mode, string expectedSqlQuery)
		{
			var filterCollection = new ModuleFilterCollection();
			var transportModeList = GetTransportModeList();
			var filter = AddTransportModeFilter(filterCollection, transportModeList);

			filter.IsActive = true;
			filter.Property = mode;

			var query = filter.Query.LiteralTextADO;
			AssertEquals($"Unable to correctly produce query for {mode}", query, expectedSqlQuery);
		}

		static void AssertFilteredCollectionMode(string message, RateEntryFilterStripBusinessObject filterBizO, IEnumerable<RateEntry> expected)
		{
			filterBizO.Collection.LoadAndSortForGUI();

			AssertContainsExactElementsInAnyOrder(
				message,
				BusinessObjectEqualityComparer<RateEntry>.InstanceComparer,
				(entry) => entry.TI_Mode,
				expected,
				filterBizO.Collection.Cast<RateEntry>());
		}

		#endregion helpers
	}
}
