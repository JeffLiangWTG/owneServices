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
	public class RelatedWorkItemsOfProjectFilter : GenPivotDualDirectionRelatedEntityFilter
	{
		public RelatedWorkItemsOfProjectFilter(ZString description, GetList listDelegate)
			: base(description, listDelegate, typeof(Project), WorkProjectSchema.Constants.Prefix, WorkItemSchema.Constants.Prefix, ModuleIDs.WorkItem)
		{
			Category = ProjectFilterBusinessObject.RelatedWorkItemsFilterCategory;
		}

		RelatedWorkItemsOfProjectFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID)
		{
		}

		protected RelatedWorkItemsOfProjectFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override SchemaGuidColumn ParentPKColumn => WorkProjectSchema.PK;
		protected override SchemaColumn SubQueryColumn => WorkItemSchema.PK;

		protected override MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("5b9d77ab-f49b-4fc4-9b6e-b7e9ae645217", "Related Work Items");

		protected override GenPivotDualDirectionRelatedEntityFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedWorkItemsOfProjectFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}
	}
}
