using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListLineProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return typeof(ContainerLoadListLineProcessTask);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode:
					subQuery.AddToFilter(ContainerLoadListLineSchema.CLL_LoadMode, CommonContainerLoadListLoadModeList.Codes.CY);
					break;

				case WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode:
					subQuery.AddToFilter(ContainerLoadListLineSchema.CLL_LoadMode, CommonContainerLoadListLoadModeList.Codes.CFS);
					break;
			}
		}
	}
}
