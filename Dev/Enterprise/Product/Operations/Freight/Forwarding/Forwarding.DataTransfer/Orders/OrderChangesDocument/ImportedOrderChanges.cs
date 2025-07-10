using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	#region ImportedOrderState enum

	public enum ImportedOrderState
	{
		New = 0,
		Unchanged = 1,
		Cancelled = 2,
		Amended = 3,
		Attached = 4,
		Reactivated = 5
	}

	#endregion

	public class ImportedOrderChanges : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ImportedOrderChanges(List<ImportedOrder> importedOrders)
			: this(importedOrders, ZString.Empty)
		{
		}

		public ImportedOrderChanges(List<ImportedOrder> importedOrders, ZString attachedOrdersDescription)
		{
			this.allOrders = importedOrders;
			this.attachedOrdersDescription = string.IsNullOrWhiteSpace(attachedOrdersDescription)
				? ResString.GetMultilingualString("49A128CE-033F-4F48-B54A-F0FEBF591A28", "The following orders were not updated because they have attached shipments or declarations.")
				: attachedOrdersDescription;

			foreach (ImportedOrder importedOrder in importedOrders)
			{
				if (importedOrder != null)
				{
					AddOrderToCollection(importedOrder);
				}
			}
		}

		internal List<ImportedOrder> AllOrders
		{
			get { return allOrders; }
		}
		readonly List<ImportedOrder> allOrders;

		public ZBool IncludeChangesNotAppliedInReport
		{
			get { return Enterprise.Registry.Business.OrdersDataRegistry.Instance.IncludeChangesNotApplied.Value; }
		}

		void AddOrderToCollection(ImportedOrder importedOrder)
		{
			switch (importedOrder.OrderState)
			{
				case ImportedOrderState.New:
					NewOrders.Add(importedOrder);
					break;
				case ImportedOrderState.Unchanged:
					UnchangedOrders.Add(importedOrder);
					break;
				case ImportedOrderState.Cancelled:
					CancelledOrders.Add(importedOrder);
					break;
				case ImportedOrderState.Amended:
					AmendedOrders.Add(importedOrder);
					break;
				case ImportedOrderState.Attached:
					AttachedOrders.Add(importedOrder);
					break;
				case ImportedOrderState.Reactivated:
					ReactivatedOrders.Add(importedOrder);
					break;
				default:
					UnchangedOrders.Add(importedOrder);
					break;
			}
		}

		public ZString AttachedOrdersDescription
		{
			get { return attachedOrdersDescription; }
		}
		readonly ZString attachedOrdersDescription;

		#region NewOrders

		public ImportedOrderCollection NewOrders
		{
			get { return newOrders ?? (newOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection newOrders;

		public ImportedOrderLineCollection NewOrderLines
		{
			get { return newOrderLines ?? (newOrderLines = new ImportedOrderLineCollection(NewOrders)); }
		}
		ImportedOrderLineCollection newOrderLines;

		public ImportedOrderLineDeliveryCollection NewOrderLineDeliveries
		{
			get { return newOrderLineDeliveries ?? (newOrderLineDeliveries = new ImportedOrderLineDeliveryCollection(NewOrders)); }
		}
		ImportedOrderLineDeliveryCollection newOrderLineDeliveries;

		#endregion

		#region AmendedOrders

		public ImportedOrderCollection AmendedOrders
		{
			get { return amendedOrders ?? (amendedOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection amendedOrders;

		public ImportedOrderLineCollection AmendedOrderLines
		{
			get { return amendedOrderLines ?? (amendedOrderLines = new ImportedOrderLineCollection(AmendedOrders)); }
		}
		ImportedOrderLineCollection amendedOrderLines;

		public ImportedOrderLineDeliveryCollection AmendedOrderLineDeliveries
		{
			get { return amendedOrderLineDeliveries ?? (amendedOrderLineDeliveries = new ImportedOrderLineDeliveryCollection(AmendedOrders)); }
		}
		ImportedOrderLineDeliveryCollection amendedOrderLineDeliveries;

		#endregion

		#region UnchangedOrders

		public ImportedOrderCollection UnchangedOrders
		{
			get { return unchangedOrders ?? (unchangedOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection unchangedOrders;

		public ImportedOrderLineCollection UnchangedOrderLines
		{
			get { return unchangedOrderLines ?? (unchangedOrderLines = new ImportedOrderLineCollection(UnchangedOrders)); }
		}
		ImportedOrderLineCollection unchangedOrderLines;

		#endregion

		#region CancelledOrders

		public ImportedOrderCollection CancelledOrders
		{
			get { return cancelledOrders ?? (cancelledOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection cancelledOrders;

		public ImportedOrderLineCollection CancelledOrderLines
		{
			get { return cancelledOrderLines ?? (cancelledOrderLines = new ImportedOrderLineCollection(CancelledOrders)); }
		}
		ImportedOrderLineCollection cancelledOrderLines;

		#endregion

		#region AttachedOrders

		public ImportedOrderCollection AttachedOrders
		{
			get { return attachedOrders ?? (attachedOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection attachedOrders;

		public ImportedOrderLineDeliveryCollection AttachedOrderLineDeliveries
		{
			get { return attachedOrderLineDeliveries ?? (attachedOrderLineDeliveries = new ImportedOrderLineDeliveryCollection(AttachedOrders)); }
		}
		ImportedOrderLineDeliveryCollection attachedOrderLineDeliveries;

		#endregion

		#region ReactivatedOrders

		public ImportedOrderCollection ReactivatedOrders
		{
			get { return reactivatedOrders ?? (reactivatedOrders = new ImportedOrderCollection()); }
		}
		ImportedOrderCollection reactivatedOrders;

		public ImportedOrderLineDeliveryCollection ReactivatedOrderLineDeliveries
		{
			get { return reactivatedOrderLineDeliveries ?? (reactivatedOrderLineDeliveries = new ImportedOrderLineDeliveryCollection(ReactivatedOrders)); }
		}
		ImportedOrderLineDeliveryCollection reactivatedOrderLineDeliveries;

		#endregion

		public bool HasChangesToReport
		{
			get
			{
				return NewOrderLines.Count > 0 ||
					AmendedOrderLineDeliveries.Count > 0 ||
					ReactivatedOrderLineDeliveries.Count > 0 ||
					UnchangedOrders.Count > 0 ||
					CancelledOrders.Count > 0 ||
					(IncludeChangesNotAppliedInReport && AttachedOrderLineDeliveries.Count > 0);
			}
		}
	}
}

