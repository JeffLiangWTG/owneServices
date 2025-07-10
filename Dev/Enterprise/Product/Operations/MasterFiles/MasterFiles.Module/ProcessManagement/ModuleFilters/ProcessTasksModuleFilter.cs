using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public abstract class ProcessTasksModuleFilter : ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider
	{
		protected ProcessTasksModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ProcessTasksModuleFilter(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public ProcessTasksModuleFilter(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, moduleID, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}

		protected override FilterStripBusinessObject GetNewSelectedFilters(StmModuleFilter layoutToLoad)
		{
			var filterBizo = base.GetNewSelectedFilters(layoutToLoad);
			filterBizo.ParentType = ParentBusinessObjectType;
			return filterBizo;
		}

		public override ZQuery GetQueryForParentBusinessObjects(IEnumerable<BusinessObject> parents)
		{
			using (SetUpFilterForParentBusinessObjects(parents))
			{
				return base.GetQueryForParentBusinessObjects(parents);
			}
		}

		protected override ZQuery GetSubQueryForPrimaryKeyBusinessObjects(ZQuery subQuery)
		{
			var result = subQuery.DeepClone();
			if (ShouldUseParentBusinessObjectsForQuery)
			{
				if (!ParentBusinessObjectsForQuery.Any())
				{
					return ZQuery.NoResultQuery;
				}

				var pks = ParentBusinessObjectsForQuery.Select(x => x.PK);
				if (FilterColumn is not null)
				{
					var primaryKeyColumn = BusinessObjectFactory.GetTableSchemaFromType(ParentBusinessObjectType).PK;
					if (FilterColumn != primaryKeyColumn && FilterColumn.TableName == primaryKeyColumn.TableName)
					{
						var parentIdQuery = new ZDBOnlyQuery(typeof(ProcessTask));
						var computedColumnSubQuery = new ZDBOnlySubQuery(ParentBusinessObjectType, FilterColumn);
						computedColumnSubQuery.AddToFilter(BusinessObjectFactory.GetTableSchemaFromType(ParentBusinessObjectType).PK, pks);
						parentIdQuery.AddSubQuery(ProcessTasksSchema.P9_ParentID, computedColumnSubQuery, JoinCondition.And);
						result.AddToFilter(parentIdQuery);
						return result;
					}
				}

				result.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(ParentBusinessObjectType));
				result.AddToFilter(ProcessTasksSchema.P9_ParentID, pks);
			}

			return result;
		}

		protected override bool TryGetSubQueryForForeignKeyBusinessObjects(ZQuery subQuery, out ZQuery result)
		{
			result = subQuery.DeepClone();
			if (ShouldUseParentBusinessObjectsForQuery && AlternativeParentTableCode != null && AlternativeParentColumn != null)
			{
				var pks = ParentBusinessObjectsForQuery.Select(x => (ZGuid)x[AlternativeParentColumn]).Where(pk => pk.IsValid);

				if (!pks.Any())
				{
					return false;
				}

				result.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, AlternativeParentTableCode);
				result.AddToFilter(ProcessTasksSchema.P9_ParentID, pks);
			}

			return true;
		}

		IDisposable SetUpFilterForParentBusinessObjects(IEnumerable<BusinessObject> parents)
		{
			if (parents != null && SelectedFilters is ProcessTaskFilterBusinessObject filterBizo)
			{
				var initialValue = filterBizo.ShouldAddNonTemplateFilter;
				filterBizo.ShouldAddNonTemplateFilter = false;
				return new DisposableAction(() => filterBizo.ShouldAddNonTemplateFilter = initialValue);
			}

			return null;
		}
	}
}
