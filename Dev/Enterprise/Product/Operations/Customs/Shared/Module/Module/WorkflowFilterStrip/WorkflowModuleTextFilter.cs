using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class WorkflowModuleTextFilterCustoms : WorkflowModuleTextFilter
	{
		public WorkflowModuleTextFilterCustoms(ZString description, GetTextQuery queryDelegate, IList list, Type businessObjectType, string templateCode)
			: base(description, queryDelegate, list, businessObjectType, templateCode)
		{
		}

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var milestoneQuery = base.GetMilestoneQuery();

			return WorkflowFilterStripsHelperCustoms.GetDeclarationAndShipmentQuery(milestoneQuery);
		}

		protected override void AddParentTableCodeQuery(ZDBOnlySubQuery query)
		{
		}
	}
}
