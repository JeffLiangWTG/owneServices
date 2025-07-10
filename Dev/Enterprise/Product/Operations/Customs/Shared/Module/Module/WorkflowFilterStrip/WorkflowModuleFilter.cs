using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Customs.Module
{
	public class WorkflowModuleFilterCustoms : WorkflowModuleFilter
	{
		public WorkflowModuleFilterCustoms(ZString description, Type bizObjType, WorkflowModuleFilterTypes filterType, ZString jobType)
			: base(description, bizObjType, filterType, jobType)
		{
		}

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var milestoneSubQuery = base.GetMilestoneQuery();

			return WorkflowFilterStripsHelperCustoms.GetDeclarationAndShipmentQuery(milestoneSubQuery);
		}

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
		}
	}
}
