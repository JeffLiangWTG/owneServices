using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider : ModuleGuidForeignCollectionFilter
	{
		protected ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
			ParentBusinessObjectType = parentBusinessObjectType;
		}

		public ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
			ParentBusinessObjectType = parentBusinessObjectType;
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (AlternativeParentColumn == null)
			{
				var subQueryForParent = GetSubQueryForPrimaryKeyBusinessObjects(subModuleFilter);
				return base.GetQueryForSelectedFiltersCore(filterBusinessObject, subQueryForParent);
			}
			else
			{
				var parentQuery = new ZDBOnlySubQuery(ParentBusinessObjectType, FilterColumn, UsesNotInQuery);
				var filterQuery = new ZDBOnlySubQuery(filterBusinessObject.QueryObjectType, SubQueryColumn);
				var subQueryForParent = GetSubQueryForPrimaryKeyBusinessObjects(subModuleFilter);
				filterQuery.AddToFilter(subQueryForParent);

				parentQuery.AddSubQuery(filterQuery, JoinCondition.And);

				var topQuery = GetNewQueryForSelectedFilters();
				topQuery.IgnoreActiveFilter = true;

				var alternateParentQuery = new ZDBOnlySubQuery(ParentBusinessObjectType, FilterColumn, UsesNotInQuery);

				if (TryGetSubQueryForForeignKeyBusinessObjects(subModuleFilter, out ZQuery subQueryForAlternativeParent) || IsAlternativeSearchOnly)
				{
					var alternateFilterQuery = new ZDBOnlySubQuery(filterBusinessObject.QueryObjectType, SubQueryColumn);
					alternateFilterQuery.AddToFilter(subQueryForAlternativeParent);

					if (AlternativeParentColumn.IsNullable)
					{
						alternateFilterQuery.AddToFilter(UsesNotInQuery ? JoinCondition.Or : JoinCondition.And, AlternativeParentColumn, UsesNotInQuery ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
					}

					alternateParentQuery.AddSubQuery(AlternativeParentColumn, alternateFilterQuery, JoinCondition.And);

					if (IsAlternativeSearchOnly)
					{
						topQuery.AddSubQuery(alternateParentQuery, JoinCondition.And);
						return topQuery;
					}

					parentQuery.AddAsUnionQuery(alternateParentQuery, true);
				}

				topQuery.AddSubQuery(parentQuery, JoinCondition.And);
				return topQuery;
			}
		}

		public bool IsAlternativeSearchOnly { get; set; }

		public SchemaGuidColumn AlternativeParentColumn { get; set; }

		public string AlternativeParentTableCode { get; set; }

		public Type ParentBusinessObjectType { get; private set; }
	}
}
