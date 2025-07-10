using System;
using System.Linq;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	[Serializable]
	public class ContainerLoadListShipmentCreator : LogSubscriber
	{
		public override string Name => "ContainerLoadListShipmentCreation";

		public override string[] EventTypes => new string[] { AutoEvents.StatusUpdated.Code };

		public override string[] TableNames => new string[] { ContainerLoadListHeaderSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.Any())
			{
				ProcessLogQueueItemsCore(queuedLogs);
			}
		}

		void ProcessLogQueueItemsCore(IQueuedLog[] queuedLogs)
		{
			foreach (var queuedLog in queuedLogs.OrderBy(q => q.SJ_PostedTimeUtc))
			{
				var parameters = StmALog.GetParametersFromReference(queuedLog.SJ_Reference);
				if (parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.New, out var parameter))
				{
					if (parameter == CommonContainerLoadListStatusList.Codes.SHP)
					{
						ConvertContainerLoadListToShipment(queuedLog);
					}
				}
			}
		}

		void ConvertContainerLoadListToShipment(IQueuedLog queuedLog)
		{
			var containerLoadList = queuedLog.Factory.Load<CommonContainerLoadList>(queuedLog.SJ_ParentID);
			if (containerLoadList.CLH_Status == ContainerLoadListHeaderStatus.Shipped)
			{
				if (containerLoadList.ValidateShipmentConsolLink())
				{
					new OrderManagerConvertService().ConvertContainerLoadListToShipments(queuedLog.SJ_ParentID, queuedLog.Factory, isFinalizing: false);
				}
				else
				{
					containerLoadList.Logs.RaisePlannedShipmentValidationException();
					containerLoadList.CLH_Status =
						containerLoadList.CLH_LoadMode == CommonContainerLoadListLoadModeList.Codes.CY
							? ContainerLoadListHeaderStatus.Placed
							: ContainerLoadListHeaderStatus.Planned;
				}
			}
		}
	}
}
