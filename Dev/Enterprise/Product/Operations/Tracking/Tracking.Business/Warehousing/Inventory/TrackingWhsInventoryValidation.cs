using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsInventoryValidation : WhsInventoryViewValidation
	{
		public TrackingWhsInventoryValidation(TrackingWhsInventory parent)
			: base(parent)
		{
		}

		protected new TrackingWhsInventory Parent
		{
			get { return (TrackingWhsInventory)base.Parent; }
		}

		public override void ValidateAll()
		{
			ValidateQuantity();
			base.ValidateAll();
		}

		public virtual void ValidateQuantity()
		{
			ValidateCalculatedProperty(Parent.QuantityInfo);
		}

		protected void CheckQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.QuantityInfo, 18, 3);
			if (Parent.ShoppingCart != null)
			{
				if (Parent.ShoppingCart.WhsOrder.Lines.Count > 0 && Parent.ShoppingCart.WhsOrder.WD_WW_Whs != Parent.Warehouse.PK)
				{
					Parent.QuantityInfo.AddError(Res.GetString("a4ed63f2-b545-44f8-82c2-b5b4aab02d3d", "You can only select inventory from a single warehouse"));
					return;
				}

				var reservedQuantity = GetCrossDockReservedQuantity();
				var availableToOrder = Parent.WI_AvailableForCrossDockQuantity;
				if (reservedQuantity + Parent.Quantity > availableToOrder)
				{
					Parent.QuantityInfo.AddError(Res.GetString("c82072eb-953a-42dc-89f4-d5928e816864", "You cannot allocate more than the Available Quantity of") + " " + availableToOrder.ToString(Parent.Product.QtyToStringFormat));
					return;
				}
			}
		}

		ZDecimal GetCrossDockReservedQuantity()
		{
			var orderLine = GetExistingOrderLine(Parent);
			var crossDock = GetExistingCrossDock(orderLine, Parent);
			return crossDock?.ReservedQuantity ?? 0;
		}

		TrackingWhsOrderLine GetExistingOrderLine(TrackingWhsInventory inventory)
		{
			TrackingWhsOrderLine result = null;
			foreach (WhsOrderLine line in inventory.ShoppingCart.WhsOrder.Lines)
			{
				if (line.WE_OP == inventory.WI_OP &&
					line.WE_PartAttrib1 == inventory.WI_PartAttrib1 &&
					line.WE_PartAttrib2 == inventory.WI_PartAttrib2 &&
					line.WE_PartAttrib3 == inventory.WI_PartAttrib3 &&
					line.WE_SerialNumber == inventory.WI_SerialNumber)
				{
					result = TrackingHelper.Get(line);
					break;
				}
			}
			return result;
		}

		WhsPickLine GetExistingCrossDock(TrackingWhsOrderLine orderLine, TrackingWhsInventory inventory)
		{
			if (orderLine != null)
			{
				foreach (var pickLine in orderLine.WhsOrderLine.ReservedPickLines)
				{
					if (pickLine.WZ_WE_InventoryLine == inventory.WI_WE_InDocketLine)
					{
						return pickLine;
					}
				}
			}

			return null;
		}
	}
}
