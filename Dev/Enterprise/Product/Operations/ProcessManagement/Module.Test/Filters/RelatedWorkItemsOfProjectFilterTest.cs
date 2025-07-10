using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(RelatedWorkItemsOfProjectFilter))]
	class RelatedWorkItemsOfProjectFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedWorkItemsOfProjectFilter, Project, WorkItem>
	{
		protected override string DescriptionFilterStripName => "Summary";

		protected override ModuleIdentifier RelatedModuleID => ModuleIDs.WorkItem;

		protected override string RelatedEntityFilterStripName => "Related Work Items";

		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new WorkItemCollection(Factory);

		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new ProjectFilterBusinessObject();

		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new WorkItemFilterBusinessObject();

		protected override RelatedWorkItemsOfProjectFilter GetNewModuleFilter()
		{
			return new RelatedWorkItemsOfProjectFilter("moo", () => CreateRelatedEntityCollection());
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(Project mainBizo)
		{
			return mainBizo.WKP_SummaryInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(WorkItem relatedBizo)
		{
			return relatedBizo.WKI_SummaryInfo;
		}

		protected override FilterCategory ExpectedDefaultCategory => ProjectFilterBusinessObject.RelatedWorkItemsFilterCategory;
	}
}
