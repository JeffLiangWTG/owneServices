using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Orders.ContainerLoadListHeader
{
	public class ContainerLoadListHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object result = null;

			var parent = factory.Load<CommonContainerLoadList>(parentID);

			if (parent != null && parentTablePrefix == ContainerLoadListHeaderSchema.Constants.Prefix)
			{
				if (parent is CYContainerLoadList)
				{
					result = typeof(ContainerLoadListProcessTask);
				}
				else if (parent is CFSContainerLoadList)
				{
					result = typeof(ContainerLoadPlanProcessTask);
				}
			}

			return (Type)result;
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode:
					subQuery.AddToFilter(ContainerLoadListHeaderSchema.CLH_LoadMode, CommonContainerLoadListLoadModeList.Codes.CY);
					break;

				case WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode:
					subQuery.AddToFilter(ContainerLoadListHeaderSchema.CLH_LoadMode, CommonContainerLoadListLoadModeList.Codes.CFS);
					break;
			}
		}
	}
}
