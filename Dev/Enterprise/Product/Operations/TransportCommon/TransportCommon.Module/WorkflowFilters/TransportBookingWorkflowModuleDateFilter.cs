using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;

namespace Enterprise.TransportCommon.Module
{
	class TransportBookingWorkflowModuleDateFilter : WorkflowModuleFilter
	{
		public TransportBookingWorkflowModuleDateFilter(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType, ZString jobType, SchemaColumn schemaColumnOverride)
			: base(description, bizObjType, filterType, jobType)
		{
			SchemaColumnOverride = schemaColumnOverride;
		}

		readonly SchemaColumn SchemaColumnOverride;

		#region GetQuery

		protected override ZQuery GetQuery()
		{
			return !IsEmpty
				? WorkflowModuleFilterQueryBuilder.BuildQuery(businessObjectType, GetMilestoneQuery(), RelatedParentSubQueries, SchemaColumnOverride)
				: new ZQuery();
		}

		#endregion

		#region AddParentTableCodeQuery

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
			// no parent table code query should be added.
		}

		#endregion
	}
}
