using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferLineLookups : WhsDocketLineLookups
	{
		#region Constructors

		public WhsTransferLineLookups(WhsTransferLine parent)
			: base(parent)
		{
		}

		#endregion

		#region Parent

		protected new WhsTransferLine Parent
		{
			get { return (WhsTransferLine)base.Parent; }
		}

		#endregion

		#region InventoryStatuses

		protected override CodeDescriptionPairList GetInventoryStatusesCore()
		{
			var statuses = Factory.GetCachedValue("WhsTransferLineLookups|InventoryStatuses|" + Parent.WE_WD, () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(InventoryStatus.Codes.Available, InventoryStatus.Descriptions.Available);
					result.AddPair(InventoryStatus.Codes.Held, InventoryStatus.Descriptions.Held);
					result.AddPair(InventoryStatus.Codes.InTransit, InventoryStatus.Descriptions.InTransit);

					var docket = Parent.Docket;
					if (docket != null)
					{
						if (docket.WD_IsPutawayTransfer)
						{
							result.AddPair(InventoryStatus.Codes.Received, InventoryStatus.Descriptions.Received);
							result.AddPair(InventoryStatus.Codes.Putaway, InventoryStatus.Descriptions.Putaway);
							result.AddPair(InventoryStatus.Codes.PuttingAway, InventoryStatus.Descriptions.PuttingAway);
						}
						else if (docket.IsTransferringForOrder || docket.IsReturnStockTransfer)
						{
							result.AddPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);
							result.AddPair(InventoryStatus.Codes.ReadyToPack, InventoryStatus.Descriptions.ReadyToPack);
						}
						else if (docket.IsVASOrderTransfer)
						{
							result.AddPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);
						}
					}

					return result;
				});

			return statuses;
		}

		#endregion

		#region PickedBys

		public GlbStaffCollection PickedBys
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region TransferFromLocations

		public WhsLocationCollection TransferFromLocations
		{
			get
			{
				var transferFromWarehousePK = Parent.TransferFromWarehousePK;
				return Factory.GetCachedValue("WhsTransferLineLookups|TransferFromLocations|" + transferFromWarehousePK, () =>
				{
					var warehouse = Parent.TransferFromWarehouse;
					return (warehouse != null)
						? new WhsLocationCollection(warehouse)
						: new WhsLocationCollection(Factory);
				});
			}
		}

		#endregion
	}
}
