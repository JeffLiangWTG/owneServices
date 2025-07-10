using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WorkOrderStagingLocationHelper
	{
		public WorkOrderStagingLocationHelper(WhsWarehouse warehouse)
		{
			Warehouse = Argument.NotNull(warehouse, nameof(warehouse));
			DefaultWarehouseLocation = new Lazy<ZGuid>(() => Warehouse.DefaultLocation?.PK ?? ZGuid.Empty);
			DefaultInwardProcessingLocation = new Lazy<ZGuid>(() => Warehouse.DefaultLocationInInwardProcessingArea?.PK ?? ZGuid.Empty);
		}

		WhsWarehouse Warehouse { get; }

		public static ZGuid GetStagingLocationForWorkOrderLine(WhsWarehouse warehouse, WhsComponentOrderLine workOrderLine)
		{
			return new WorkOrderStagingLocationHelper(warehouse).GetStagingLocationForWorkOrderLine(workOrderLine);
		}

		public ZGuid GetStagingLocationForWorkOrderLine(WhsComponentOrderLine workOrderLine)
		{
			var result = ZGuid.Empty;

			Argument.NotNull(workOrderLine, nameof(workOrderLine));

			var workOrder = workOrderLine.PickableDocket;
			if (workOrder.WD_WW_Whs != Warehouse.PK)
			{
				throw new InvalidOperationException("Work Order Line must have the same Warehouse as the one passed through the Constructor.");
			}

			var product = workOrderLine.WE_WE_ParentDocketLine.IsEmpty ? workOrderLine.Product : workOrderLine.ParentLine.Product;
			var paramByWhsAndClient = product?.GetParamsByWhsAndClient(workOrder.WD_WW_Whs, workOrder.WD_OH_Client);
			if (paramByWhsAndClient != null)
			{
				WhsLocation stagingLocation;

				if (workOrder.WD_IsInwardsProcessingJob)
				{
					var inwardProcessingLocation = paramByWhsAndClient.InwardProcessingStagingLocationBOM;
					stagingLocation = inwardProcessingLocation != null && inwardProcessingLocation.IsInInwardProcessingArea ? inwardProcessingLocation : null;
				}
				else
				{
					var normalStagingLocation = paramByWhsAndClient.StagingLocationBOM;
					stagingLocation = normalStagingLocation != null && !normalStagingLocation.IsInBondedArea && !normalStagingLocation.IsInInwardProcessingArea ? normalStagingLocation : null;
				}

				if (stagingLocation != null && !stagingLocation.IsDockDoorLocation)
				{
					result = stagingLocation.PK;
				}
			}

			// currently work orders default to the Warehouse's Default Location if it cannot find a staging area on the Product
			if (result.IsEmpty)
			{
				result = workOrder.WD_IsInwardsProcessingJob ? DefaultInwardProcessingLocation.Value : DefaultWarehouseLocation.Value;
			}

			return result;
		}

		Lazy<ZGuid> DefaultWarehouseLocation { get; }
		Lazy<ZGuid> DefaultInwardProcessingLocation { get; }
	}
}
