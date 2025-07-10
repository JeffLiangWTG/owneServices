using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(NCTS.Module.NctsMovementFilterStripBusinessObject))]
	sealed class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLrnRegistrationNumberFilter()
		{
			var nctsHeader = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LrnRegistrationNumber = "LRN1111111111";
			nctsHeader.LrnRegistrationDate = ZDateTime.Today;
			Factory.Save();

			var filterStripBO = GetNewFilterStripBusinessObject();
			var lrnRegistrationNumberFilter = (ModuleTextFilter)filterStripBO["LRN"];
			AssertNotNull(lrnRegistrationNumberFilter);

			lrnRegistrationNumberFilter.IsActive = true;
			lrnRegistrationNumberFilter.Property = "LRN1111111111";
			AssertEquals("Matches filter", true, nctsHeader.MatchesFilter(filterStripBO.Filter));

			lrnRegistrationNumberFilter.Property = "LRN2222222222";
			AssertEquals("Does not match filter", false, nctsHeader.MatchesFilter(filterStripBO.Filter));

			lrnRegistrationNumberFilter.Property = "";
			lrnRegistrationNumberFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals("Does not match filter for blank", false, nctsHeader.MatchesFilter(filterStripBO.Filter));
		}

		public void TestLrnRegistrationDateFilter()
		{
			var nctsHeader = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LrnRegistrationNumber = "LRN1111111111";
			nctsHeader.LrnRegistrationDate = ZDateTime.Today;
			Factory.Save();

			var filterStripBO = GetNewFilterStripBusinessObject();
			var registrationDateFilter = (ModuleDateFilter)filterStripBO["LRN Date"];

			AssertNotNull(registrationDateFilter);
			registrationDateFilter.IsActive = true;
			registrationDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			registrationDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			registrationDateFilter.Property2 = ZDateTime.Today.AddDays(+1);

			CombineAssertions("Assert filter results", () =>
			{
				AssertEquals("departureNctsHeader1 matches filter", true, nctsHeader.MatchesFilter(filterStripBO.Filter));
			});

			registrationDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			CombineAssertions("Assert filter results for HasNoDateEntered", () =>
			{
				AssertEquals("departureNctsHeader1 does not match the filter", false, nctsHeader.MatchesFilter(filterStripBO.Filter));
			});

			registrationDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			registrationDateFilter.Property1 = ZDateTime.Today.AddDays(-10);
			registrationDateFilter.Property2 = ZDateTime.Today.AddDays(-5);

			CombineAssertions("Assert filter results for date range not including today", () =>
			{
				AssertEquals("departureNctsHeader1 does not match the filter", false, nctsHeader.MatchesFilter(filterStripBO.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new NCTS.Module.NctsMovementFilterStripBusinessObject();
		}

		NCTS.Business.NctsHeader GetNewNctsHeader(string movementType)
		{
			var nctsHeader = Factory.New<NCTS.Business.NctsHeader>();
			nctsHeader.SetMovementType(movementType);

			return nctsHeader;
		}
	}
}
