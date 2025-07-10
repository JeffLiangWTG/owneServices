using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class RelatedProjectsOfWorkItemFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedProjectsOfWorkItemFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(WorkItem), WorkItemSchema.Constants.Prefix, WorkProjectSchema.Constants.Prefix, ModuleIDs.Project)
		{
			Category = WorkItemFilterBusinessObject.RelatedItemsFilterCategory;
		}

		RelatedProjectsOfWorkItemFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedProjectsOfWorkItemFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => WorkItemSchema.PK;
		protected override SchemaColumn SubQueryColumn => WorkProjectSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("a2d70a06-2b5a-4b32-8acd-e2672e3bdc84", "Related Projects");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedProjectsOfWorkItemFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
