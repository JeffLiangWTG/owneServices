using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(CustomerServiceTicketFilterBusinessObject))]
	class CustomerServiceTicketFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestWorkflowFilters_ShouldBePresent()
		{
			var filterBizo = new CustomerServiceTicketFilterBusinessObject();

			AssertNotNull(filterBizo[WorkflowWithExtraTasksFilterStripsHelper.FilterDescription.TaskAssignedTo]);
		}

		public void TestTextFilters()
		{
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion1, req => req.WKR_SelectionCriteria1Info);
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion2, req => req.WKR_SelectionCriteria2Info);
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion3, req => req.WKR_SelectionCriteria3Info);
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion4, req => req.WKR_SelectionCriteria4Info);
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion5, req => req.WKR_SelectionCriteria5Info);

			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Summary, req => req.WKR_SummaryInfo);
			AssertSimpleTextFilter(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Description, req => req.WKR_DescriptionInfo);
		}

		public void TestTextFilters_SaveAndReloadFilterStripLayouts_ShouldStoreDefaultNames()
		{
			var caption1Registry = ProcessManagementRegistry.Instance.SelectionCriterion1Caption;
			var caption2Registry = ProcessManagementRegistry.Instance.SelectionCriterion2Caption;
			var caption3Registry = ProcessManagementRegistry.Instance.SelectionCriterion3Caption;
			var caption4Registry = ProcessManagementRegistry.Instance.SelectionCriterion4Caption;
			var caption5Registry = ProcessManagementRegistry.Instance.SelectionCriterion5Caption;

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion1, "ABC");
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion2, "DEF");
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion3, "GHI");
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion4, "JKL");
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion5, "MNO");

			var layout = FilterStripsTestHelper.SaveFilterLayout(filterBizo, "Ooh me lucky filters", false, false, true);

			var reloadedFactory = new BusinessObjectFactory();
			var reloadedLayout = reloadedFactory.Load<StmModuleFilter>(layout.PK);

			caption1Registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 1");
			caption2Registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 2");
			caption3Registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 3");
			caption4Registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 4");
			caption5Registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 5");

			AssertEquals("Ooh me lucky filters", reloadedLayout.S9_FilterName);
			var deserialisedFilterData = reloadedLayout.S9_FilterData.ToAscii();

			CombineAssertions("The generic filter names should be saved, but not the customised ones", () =>
			{
				AssertContains(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion1, deserialisedFilterData);
				AssertContains(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion2, deserialisedFilterData);
				AssertContains(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion3, deserialisedFilterData);
				AssertContains(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion4, deserialisedFilterData);
				AssertContains(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion5, deserialisedFilterData);

				AssertNotContains(caption1Registry.Value, deserialisedFilterData);
				AssertNotContains(caption2Registry.Value, deserialisedFilterData);
				AssertNotContains(caption3Registry.Value, deserialisedFilterData);
				AssertNotContains(caption4Registry.Value, deserialisedFilterData);
				AssertNotContains(caption5Registry.Value, deserialisedFilterData);
			});

			var newFilterBizo = new CustomerServiceTicketFilterBusinessObject();
			newFilterBizo.LoadLayout(reloadedLayout);

			AssertEquals(5, newFilterBizo.ActiveModuleFilters.Count);

			CombineAssertions("The customised filter names should be displayed, but not the generic ones", () =>
			{
				AssertEquals(caption1Registry.Value, newFilterBizo[CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion1].MultilingualDescription);
				AssertEquals(caption2Registry.Value, newFilterBizo[CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion2].MultilingualDescription);
				AssertEquals(caption3Registry.Value, newFilterBizo[CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion3].MultilingualDescription);
				AssertEquals(caption4Registry.Value, newFilterBizo[CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion4].MultilingualDescription);
				AssertEquals(caption5Registry.Value, newFilterBizo[CustomerServiceTicketFilterBusinessObject.FilterDescriptions.SelectionCriterion5].MultilingualDescription);
			});
		}

		public void TestTicketNumberFilter()
		{
			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.TicketNumber, "CST00000002");

			AssertContainsExactElementsInAnyOrder(new[] { request2 }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		public void TestBranchFilter()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			request1.WKR_GB_Branch = branch1.PK;
			request2.WKR_GB_Branch = branch2.PK;

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddGuidFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Branch, branch2.PK);

			AssertContainsExactElementsInAnyOrder(new[] { request2 }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		public void TestDepartmentFilter()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			request1.WKR_GE_Department = department1.PK;
			request2.WKR_GE_Department = department2.PK;

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddGuidFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Department, department2.PK);

			AssertContainsExactElementsInAnyOrder(new[] { request2 }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		public void TestCountryFilter()
		{
			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			request1.WKR_RN_NKCountry = "AZ";
			request2.WKR_RN_NKCountry = "AU";

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddNkFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Country, "AU");

			AssertContainsExactElementsInAnyOrder(new[] { request2 }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		public void TestRelatedWorkItems()
		{
			var workRequest1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Should match on Any Match only.");
			var workRequest2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Should match on None Match only.");
			var workRequest3 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Should match on Any Match and All Match.");
			var workRequest4 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Should match on All Match and None Match.");

			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "YES");
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "NO");
			var workItem3 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "YES");
			var workItem4 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "NO");

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem2);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem2);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem4);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest3, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest3, workItem3);

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleGuidPivotFilter>("Related Work Items");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Summary", "YES");

			var query = filterBizo.Filter;
			var result = Factory.Load<WorkRequest>(query);
			AssertContainsExactElementsInAnyOrder("Any match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match only.", "Should match on Any Match and All Match." }, result.Select(x => x.WKR_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			query = filterBizo.Filter;
			result = Factory.Load<WorkRequest>(query);
			AssertContainsExactElementsInAnyOrder("None match: " + query.LiteralTextSqlFormatted, new[] { "Should match on None Match only.", "Should match on All Match and None Match." }, result.Select(x => x.WKR_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			query = filterBizo.Filter;
			result = Factory.Load<WorkRequest>(query);
			AssertContainsExactElementsInAnyOrder("All match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match and All Match.", "Should match on All Match and None Match." }, result.Select(x => x.WKR_Summary));
		}

		public void TestStatus()
		{
			var ticket_open = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticket_working = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticket_closed = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticket_cancelled = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			MasterFilesTestHelper.CreateTask(ticket_open, status: ProcessTaskStatusCodeList.Codes.Assigned);
			MasterFilesTestHelper.CreateTask(ticket_working, status: ProcessTaskStatusCodeList.Codes.Working);
			MasterFilesTestHelper.CreateTask(ticket_closed, status: ProcessTaskStatusCodeList.Codes.Closed);

			ticket_cancelled.WKR_Status = TicketStatusList.Codes.Cancelled;

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Open, ticket_open.WKR_Status);
			AssertEquals(TicketStatusList.Codes.Working, ticket_working.WKR_Status);
			AssertEquals(TicketStatusList.Codes.Closed, ticket_closed.WKR_Status);
			AssertEquals(TicketStatusList.Codes.Cancelled, ticket_cancelled.WKR_Status);

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Status);

			filter.Property = TicketStatusList.Codes.Open;
			AssertContainsExactElementsInAnyOrder(new[] { ticket_open }, Factory.Load<WorkRequest>(filterBizo.Filter));

			filter.Property = TicketStatusList.Codes.Working;
			AssertContainsExactElementsInAnyOrder(new[] { ticket_working }, Factory.Load<WorkRequest>(filterBizo.Filter));

			filter.Property = TicketStatusList.Codes.Closed;
			AssertContainsExactElementsInAnyOrder(new[] { ticket_closed }, Factory.Load<WorkRequest>(filterBizo.Filter));

			filter.Property = TicketStatusList.Codes.Cancelled;
			AssertContainsExactElementsInAnyOrder(new[] { ticket_cancelled }, Factory.Load<WorkRequest>(filterBizo.Filter));

			filter.Property = string.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { ticket_open, ticket_working, ticket_closed, ticket_cancelled }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		public void TestClient()
		{
			var ticket1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "I misspoke");
			var ticket2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "I should have said 'wouldn't'.");

			var client1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, "Vandelay Industries", "Shmlonathan", "shmlonathan@shmloostheshmloss.com");
			var client2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, "Penske", "Shmlangela", "shmlangela@shmloostheshmloss.com");

			ticket1.WKR_OC_Client = client1.PK;
			ticket2.WKR_OC_Client = client2.PK;

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			var filter = filterBizo.AddGuidFilterStrip("Client", client1.PK);
			var query = filterBizo.Filter;
			var results = Factory.Load<WorkRequest>(query);

			AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "I misspoke" }, results.Select(x => x.WKR_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Name", "Shmla");
			query = filterBizo.Filter;
			results = Factory.Load<WorkRequest>(query);

			AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "I should have said 'wouldn't'." }, results.Select(x => x.WKR_Summary));
		}

		#region Implementation

		void AssertSimpleTextFilter(string filterName, Func<WorkRequest, ZPropertyInfo> propertyInfoGetter)
		{
			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			propertyInfoGetter(request1).Value = (ZString)"DAM";
			propertyInfoGetter(request2).Value = (ZString)"AGE";

			Factory.Save();

			var filterBizo = new CustomerServiceTicketFilterBusinessObject();
			filterBizo.AddTextFilterStrip(filterName, "AGE");

			AssertContainsExactElementsInAnyOrder($"Expected results for {filterName} filter.", new[] { request2 }, Factory.Load<WorkRequest>(filterBizo.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CustomerServiceTicketFilterBusinessObject();
		}

		#endregion
	}
}
