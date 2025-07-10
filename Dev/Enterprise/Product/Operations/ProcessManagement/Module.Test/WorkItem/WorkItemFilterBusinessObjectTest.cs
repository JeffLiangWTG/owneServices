using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.ProcessManagement.Module.SearchFieldConstants;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(WorkItemFilterBusinessObject))]
	class WorkItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WorkItemFilterBusinessObject();
		}

		WorkItemFilterBusinessObject FilterBizO
		{
			get { return (WorkItemFilterBusinessObject)CachedBusinessObject; }
		}

		#endregion

		#region Related Customer Service Ticket

		public void TestRelatedWorkRequestFilter_Category()
		{
			var filterBizo = new WorkItemFilterBusinessObject();
			var filter = filterBizo["Related Tickets"];

			AssertEquals(WorkItemFilterBusinessObject.RelatedItemsFilterCategory, filter.Category);
		}

		public void TestRelatedWorkRequestFilter()
		{
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Should match on Any Match only.");
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Should match on None Match only.");
			var workItem3 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Should match on Any Match and All Match.");
			var workItem4 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Should match on All Match and None Match.");

			var workRequest1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "YES");
			var workRequest2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "NO");
			var workRequest3 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "YES");
			var workRequest4 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "NO");

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem1);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem2);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest4, workItem2);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem3);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest3, workItem3);

			Factory.Save();

			var filterBizo = new WorkItemFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleGuidPivotFilter>("Related Tickets");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Summary", "YES");

			var query = filterBizo.Filter;
			var result = Factory.Load<WorkItem>(query);
			AssertContainsExactElementsInAnyOrder("Any match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match only.", "Should match on Any Match and All Match." }, result.Select(x => x.WKI_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			query = filterBizo.Filter;
			result = Factory.Load<WorkItem>(query);
			AssertContainsExactElementsInAnyOrder("None match: " + query.LiteralTextSqlFormatted, new[] { "Should match on None Match only.", "Should match on All Match and None Match." }, result.Select(x => x.WKI_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			query = filterBizo.Filter;
			result = Factory.Load<WorkItem>(query);
			AssertContainsExactElementsInAnyOrder("All match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match and All Match.", "Should match on All Match and None Match." }, result.Select(x => x.WKI_Summary));
		}

		public void TestGetSearchFieldOverride()
		{
			using (GetIndexSearchRegistryMock())
			using (GetGlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				Assert(filterStrip[PortOrCountry] is IIndexSearchModuleFilter);
				Assert(filterStrip[Company] is IIndexSearchModuleFilter);
				Assert(filterStrip[Department] is IIndexSearchModuleFilter);
				Assert(filterStrip[WorkItemType] is IIndexSearchModuleFilter);
				Assert(filterStrip[WorkItemArea] is IIndexSearchModuleFilter);
				Assert(filterStrip[ActivityType] is IIndexSearchModuleFilter);
				Assert(filterStrip[ActivitySubtype] is IIndexSearchModuleFilter);
				Assert(filterStrip[WorkItemPriority] is IIndexSearchModuleFilter);
			}
		}

		IDisposable GetIndexSearchRegistryMock()
		{
			var registryMock = new Mock<IGlowRegistry>();
			_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
			_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
			return ObjectFactory.Substitute(registryMock.Object);
		}

		IDisposable GetGlowIndexQueryEngineMock()
		{
			var mock = new Mock<IGlowIndexQueryEngine>();
			_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(new SearchFieldCollection(null, Array.Empty<SearchField>()));
			_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IWorkItem" });
			return ObjectFactory.Substitute(mock.Object);
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create(PortOrCountry, "Port/Country");
			var field2 = SearchField.Create(Company, "Company");
			var field3 = SearchField.Create(Department, "Department");
			var field4 = SearchField.Create(WorkItemType, "WorkItemType");
			var field5 = SearchField.Create(WorkItemArea, "WorkItemArea");
			var field6 = SearchField.Create(ActivityType, "ActivityType");
			var field7 = SearchField.Create(ActivitySubtype, "ActivitySubtype");
			var field8 = SearchField.Create(WorkItemPriority, "WorkItemPriority");
			var ret = new SearchFieldCollection(null, new SearchField[] { field1, field2, field3, field4, field5, field6, field7, field8 });
			return ret;
		}

		#endregion

		public void TestSingleExactOrStartsWithWITypeFilterShowsRelatedBranchesOnly()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			ModuleTextFilter typeFilter = (ModuleTextFilter)FilterBizO["Work Item Type"];
			typeFilter.IsActive = true;
			typeFilter.Property = "1AA";
			typeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			typeFilter.Property = "ZZZ";
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);

			typeFilter.Property = "1AA";
			typeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			typeFilter.Property = "ZZZ";
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);

			typeFilter.Property = "1AA";
			typeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);

			typeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);
		}

		public void TestSingleExactOrStartsWithWITypeAndAreaFilterShowsRelatedBranchesOnly()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			ModuleTextFilter typeFilter = (ModuleTextFilter)FilterBizO["Work Item Type"];
			ModuleTextFilter areaFilter = (ModuleTextFilter)FilterBizO["Work Item Area"];
			areaFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			typeFilter.IsActive = false;

			areaFilter.IsActive = true;
			areaFilter.Property = "2AA";
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);

			typeFilter.IsActive = true;
			typeFilter.Property = "1AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA");

			areaFilter.Property = "ZZZ";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			areaFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			typeFilter.IsActive = false;
			areaFilter.IsActive = true;
			areaFilter.Property = "2AA";
			AssertAllBranchesForAnyFilterInWorkItemTree(tree);

			typeFilter.IsActive = true;
			typeFilter.Property = "1AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA");

			areaFilter.Property = "ZZZ";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			areaFilter.Property = "2AA";
			areaFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			areaFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");
		}

		public void TestFiltersInWorkItemTreeShowsRelatedBranchesOnly()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			ModuleTextFilter typeFilter = (ModuleTextFilter)FilterBizO["Work Item Type"];
			typeFilter.IsActive = true;
			typeFilter.Property = "1AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA");

			ModuleTextFilter areaFilter = (ModuleTextFilter)FilterBizO["Work Item Area"];
			areaFilter.IsActive = true;
			areaFilter.Property = "2AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA");

			ModuleTextFilter activityTypeFilter = (ModuleTextFilter)FilterBizO["Activity Type"];
			activityTypeFilter.IsActive = true;
			activityTypeFilter.Property = "3AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA", "3AA");

			ModuleTextFilter activitySubtypeFilter = (ModuleTextFilter)FilterBizO["Activity Subtype"];
			activitySubtypeFilter.IsActive = true;
			activitySubtypeFilter.Property = "4AA";
			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA", "3AA", "4AA");
		}

		void AssertRelatedBranchesForExactOrStartsWithFilter(CodeDescriptionBoolTreeNodeCollection tree, string type)
		{
			AssertWorkItemAreaList(tree.GetChildren(true, type));
			AssertActivityTypeList(tree.GetChildren(true, "", ""));
			AssertActivitySubtypeList(tree.GetChildren(true, "", "", ""));
			AssertPriorityList(tree.GetChildren(true, "", "", "", ""));
		}

		void AssertRelatedBranchesForExactOrStartsWithFilter(CodeDescriptionBoolTreeNodeCollection tree, string type, string subType)
		{
			AssertWorkItemAreaList(tree.GetChildren(true, type));
			AssertActivityTypeList(tree.GetChildren(true, type, subType));
			AssertActivitySubtypeList(tree.GetChildren(true, "", "", ""));
			AssertPriorityList(tree.GetChildren(true, "", "", "", ""));
		}

		void AssertRelatedBranchesForExactOrStartsWithFilter(CodeDescriptionBoolTreeNodeCollection tree, string type, string subType, string activityType)
		{
			AssertWorkItemAreaList(tree.GetChildren(true, type));
			AssertActivityTypeList(tree.GetChildren(true, type, subType));
			AssertActivitySubtypeList(tree.GetChildren(true, type, subType, activityType));
			AssertPriorityList(tree.GetChildren(true, "", "", "", ""));
		}

		void AssertRelatedBranchesForExactOrStartsWithFilter(CodeDescriptionBoolTreeNodeCollection tree, string type, string subType, string activityType, string activitySubType)
		{
			AssertWorkItemAreaList(tree.GetChildren(true, type));
			AssertActivityTypeList(tree.GetChildren(true, type, subType));
			AssertActivitySubtypeList(tree.GetChildren(true, type, subType, activityType));
			AssertPriorityList(tree.GetChildren(true, type, subType, activityType, activitySubType));
		}

		public void TestMultipleWITypeFiltersShowAllBranches()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			ModuleTextFilter typeFilter1 = (ModuleTextFilter)FilterBizO["Work Item Type"];
			typeFilter1.IsActive = true;
			typeFilter1.Property = "1AA";

			ModuleTextFilter typeFilter2 = (ModuleTextFilter)FilterBizO.CreateDuplicateFor("Work Item Type");
			typeFilter2.IsActive = true;
			typeFilter2.Property = "1BB";

			AssertAllBranchesForAnyFilterInWorkItemTree(tree);
		}

		public void TestMultipleWIActivityTypeFiltersShowAllBranches()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			ModuleTextFilter typeFilter = (ModuleTextFilter)FilterBizO["Work Item Type"];
			typeFilter.IsActive = true;
			typeFilter.Property = "1AA";

			ModuleTextFilter areaFilter = (ModuleTextFilter)FilterBizO["Work Item Area"];
			areaFilter.IsActive = true;
			areaFilter.Property = "2AA";

			ModuleTextFilter activityTypeFilter1 = (ModuleTextFilter)FilterBizO["Activity Type"];
			activityTypeFilter1.IsActive = true;
			activityTypeFilter1.Property = "3AA";

			ModuleTextFilter activityTypeFilter2 = (ModuleTextFilter)FilterBizO.CreateDuplicateFor("Activity Type");
			activityTypeFilter2.IsActive = true;
			activityTypeFilter2.Property = "";

			AssertRelatedBranchesForExactOrStartsWithFilter(tree, "1AA", "2AA");
		}

		void AssertAllBranchesForAnyFilterInWorkItemTree(CodeDescriptionBoolTreeNodeCollection tree)
		{
			AssertWorkItemAreaList(tree.GetChildren(true, ""));
			AssertActivityTypeList(tree.GetChildren(true, "", ""));
			AssertActivitySubtypeList(tree.GetChildren(true, "", "", ""));
			AssertPriorityList(tree.GetChildren(true, "", "", "", ""));
		}

		void AssertWorkItemAreaList(CodeDescriptionPairList expectedList)
		{
			var filter = (ModuleTextFilter)FilterBizO["Work Item Area"];
			AssertContainsExactElementsInAnyOrder("Work Item Area List",
				new CodeDescriptionComparer(),
				expectedList.ToArray(),
				filter.List.Cast<ICodeDescription>().ToArray());
		}

		void AssertActivityTypeList(CodeDescriptionPairList expectedList)
		{
			var filter = (ModuleTextFilter)FilterBizO["Activity Type"];
			AssertContainsExactElementsInAnyOrder("Activity Type List",
				new CodeDescriptionComparer(),
				expectedList.ToArray(),
				filter.List.Cast<ICodeDescription>().ToArray());
		}

		void AssertActivitySubtypeList(CodeDescriptionPairList expectedList)
		{
			var filter = (ModuleTextFilter)FilterBizO["Activity Subtype"];
			AssertContainsExactElementsInAnyOrder("Activity Subtype",
				new CodeDescriptionComparer(),
				expectedList.ToArray(),
				filter.List.Cast<ICodeDescription>().ToArray());
		}

		void AssertPriorityList(CodeDescriptionPairList expectedList)
		{
			var filter = (ModuleTextFilter)FilterBizO["Priority"];
			AssertContainsExactElementsInAnyOrder("Priority",
				new CodeDescriptionComparer(),
				expectedList.ToArray(),
				filter.List.Cast<ICodeDescription>().ToArray());
		}
	}
}
