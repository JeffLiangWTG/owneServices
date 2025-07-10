using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var result = typeof(WhsDocketProcessTasks);

			var parent = factory.Load<WhsDocket>(parentID);
			if (parent != null)
			{
				if (parent is WhsOrder)
				{
					result = typeof(WhsOrderProcessTasks);
				}
				else if (parent is WhsReceive)
				{
					result = typeof(WhsReceiveProcessTasks);
				}
				else if (parent is WhsAdjustment)
				{
					result = typeof(WhsAdjustmentProcessTasks);
				}
				else if (parent is WhsWorkOrder)
				{
					result = typeof(WhsWorkOrderProcessTasks);
				}
				else if (parent is WhsDynamicWorkOrder)
				{
					result = typeof(WhsDynamicWorkOrderProcessTasks);
				}
				else if (parent is WhsTransfer)
				{
					result = typeof(WhsTransferProcessTasks);
				}
			}

			return result;
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			var workflowProviderType = workflowDescriptor.WorkflowProviderType;
			var docketTypeCode = WhsDocket.TypeDecider.GetDocketTypeCodeFromType(workflowProviderType);
			subQuery.AddToFilter(WhsDocketSchema.WD_DocketType, docketTypeCode);
		}
	}
}
