using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test.CustomerServiceTicket.Filters
{
	[TestedType(typeof(CustomerServiceTicketOrganizationFilter))]
	class CustomerServiceTicketOrganizationFilterTest : ModuleFilterTestCase<CustomerServiceTicketOrganizationFilter>
	{
		public void TestFilter_Exact()
		{
			AssertFilter(ModuleTextFilter.ComparisonConstants.Exact, null, true, "I misspoke");
		}

		public void TestFilter_NotEqual()
		{
			AssertFilter(ModuleTextFilter.ComparisonConstants.NotEqual, null, true, "I should have said 'wouldn't'.");
		}

		public void TestFilter_FiltersMatch()
		{
			AssertFilter(ModuleTextFilter.ComparisonConstants.FiltersMatch, filter => filter.SelectedFilters.AddTextFilterStrip("Name", "V"), true, "I misspoke");
			AssertFilter(ModuleTextFilter.ComparisonConstants.FiltersMatch, filter => filter.SelectedFilters.AddTextFilterStrip("Name", "P"), false, "I should have said 'wouldn't'.");
		}

		void AssertFilter(string comparisonOperator, Action<ModuleGuidFilter> doToFilterAfterAdding, bool shouldCreateObjects, params string[] expectedTicketSummaries)
		{
			var client1OrgPk = ZGuid.Empty;

			if (shouldCreateObjects)
			{
				var ticket1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "I misspoke");
				var ticket2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "I should have said 'wouldn't'.");

				var client1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, "Vandelay Industries", "Shmlonathan", "shmlonathan@shmloostheshmloss.com");
				var client2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, "Penske", "Shmlangela", "shmlangela@shmloostheshmloss.com");

				ticket1.WKR_OC_Client = client1.PK;
				ticket2.WKR_OC_Client = client2.PK;

				Factory.Save();

				client1OrgPk = client1.Header.PK;
			}

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Organization", client1OrgPk);
			filter.ComparisonOperator = comparisonOperator;
			doToFilterAfterAdding?.Invoke(filter);

			var query = filterBizo.Filter;
			var results = Factory.Load<WorkRequest>(query);

			AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, expectedTicketSummaries, results.Select(x => x.WKR_Summary));
		}

		public void TestAllowedComparisonConstants()
		{
			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Organization");

			AssertEquals(true, filter.HasComparisonOperator);
			AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "filters match" }, filter.AllowedComparisonOperators);
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override CustomerServiceTicketOrganizationFilter GetNewModuleFilter()
		{
			return new CustomerServiceTicketOrganizationFilter(() => new OrgHeaderCollection(Factory));
		}

		protected override ZString ExpectedDescription => CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Organization;

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;

		protected override FilterCategory InitialTestCatergory => FilterCategories.Other;

		#endregion
	}
}
