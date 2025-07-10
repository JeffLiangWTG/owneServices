using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Module
{
	class CartageWorkflowModuleTextFilter : WorkflowModuleTextFilter
	{
		public CartageWorkflowModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list, Type bizObjType, ZString jobType, SchemaColumn schemaColumnOverride)
			: base(description, queryDelegate, list, bizObjType, jobType)
		{
			SchemaColumnOverride = schemaColumnOverride;
		}

		readonly SchemaColumn SchemaColumnOverride;

		protected override ZQuery GetQuery()
		{
			return !IsEmpty
				? WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, GetMilestoneQuery(), RelatedParentSubQueries, SchemaColumnOverride)
				: new ZQuery();
		}

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
			// no parent table code query should be added.
		}
	}
}
