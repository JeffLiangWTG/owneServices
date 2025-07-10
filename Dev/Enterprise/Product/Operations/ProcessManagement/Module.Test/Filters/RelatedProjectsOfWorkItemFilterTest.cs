using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(RelatedProjectsOfWorkItemFilter))]
	class RelatedProjectsOfWorkItemFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedProjectsOfWorkItemFilter, WorkItem, Project>
	{
		protected override string DescriptionFilterStripName => "Summary";

		protected override ModuleIdentifier RelatedModuleID => ModuleIDs.Project;

		protected override string RelatedEntityFilterStripName => "Related Projects";

		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new ProjectCollection(Factory);

		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new WorkItemFilterBusinessObject();

		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new ProjectFilterBusinessObject();

		protected override RelatedProjectsOfWorkItemFilter GetNewModuleFilter()
		{
			return new RelatedProjectsOfWorkItemFilter("moo", CreateRelatedEntityCollection);
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(WorkItem mainBizo)
		{
			return mainBizo.WKI_SummaryInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(Project relatedBizo)
		{
			return relatedBizo.WKP_SummaryInfo;
		}

		protected override FilterCategory ExpectedDefaultCategory => WorkItemFilterBusinessObject.RelatedItemsFilterCategory;

		public void TestRelatedProjectsOfWorkItemFilter_WhenAllMatch_ShouldHaveUniqueQueryParameterNames()
		{
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			var workItemsFilter = Filter.SelectedFilters.AddFilterStrip<RelatedWorkItemsOfProjectFilter>("Related Work Items");
			workItemsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			workItemsFilter.SelectedFilters.AddTextFilterStrip("Work Item Area", "PAV");

			var tasksFilter = Filter.SelectedFilters.AddFilterStrip<TasksModuleFilter>("Tasks");
			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			var dateFilter = tasksFilter.SelectedFilters.AddDateFilterStrip("Actual Start");
			dateFilter.PropertySearch = ModuleDateFilter.Past;

			AssertNoExceptionThrown(() => Factory.Load<WorkItem>(Filter.Query));
		}
	}
}
