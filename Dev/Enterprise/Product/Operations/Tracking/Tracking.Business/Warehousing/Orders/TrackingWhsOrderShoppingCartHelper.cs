using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderShoppingCartHelper
	{
		public TrackingWhsOrderShoppingCartHelper(TrackingSiteUser siteUser)
		{
			this.siteUser = siteUser;
		}
		readonly TrackingSiteUser siteUser;

		#region ShoppingCart

		public TrackingWhsOrder ShoppingCart
		{
			get
			{
				if (shoppingCart == null)
				{
					var businessObjectFactory = new BusinessObjectFactory();
					shoppingCart = TrackingHelper.Get(businessObjectFactory.New<WhsOrder>());
					if (siteUser != null && siteUser.IsLoggedIn)
					{
						shoppingCart.SiteUser = siteUser;
						shoppingCart.WhsOrder.WD_OH_Client = siteUser.LoggedInOrganisation.PK;
					}
				}
				return shoppingCart;
			}
		}
		TrackingWhsOrder shoppingCart;

		#endregion

		#region AddOrderLine

		public void AddOrderLine(TrackingWhsInventory inventory)
		{
			inventory.ShoppingCart = ShoppingCart;

			if (inventory.QuantityInfo.HasErrors())
			{
				return;
			}

			if (ShoppingCart.WhsOrder.Lines.Count > 0 && ShoppingCart.WhsOrder.WD_WW_Whs != inventory.Warehouse.PK)
			{
				inventory.Validation.ValidateQuantity();
				return;
			}

			var orderLine = GetExistingOrderLine(inventory);
			var crossDock = GetExistingCrossDock(orderLine, inventory);
			var availableToOrder = inventory.WI_AvailableForCrossDockQuantity;

			bool cannotAllocate = (inventory.Quantity > availableToOrder) ||
				(crossDock != null && (crossDock.ReservedQuantity + inventory.Quantity) > availableToOrder);
			if (cannotAllocate)
			{
				inventory.Validation.ValidateQuantity();
				return;
			}

			if (orderLine != null)
			{
				orderLine.WhsOrderLine.WE_TransactionQuantity += inventory.Quantity;
			}
			else
			{
				if (ShoppingCart.WhsOrder.Lines.Count == 0)
				{
					ShoppingCart.WhsOrder.WD_WW_Whs = inventory.Warehouse.PK;
				}

				orderLine = TrackingHelper.Get(ShoppingCart.WhsOrder.Lines.AddNew());
				orderLine.WhsOrderLine.WE_OP = inventory.WI_OP;
				if (orderLine.WhsOrderLine.SupplierPart != null)
				{
					orderLine.WhsOrderLine.SupplierPart.OP_PartNum = inventory.SupplierPart.OP_PartNum;
					orderLine.WhsOrderLine.SupplierPart.OP_Desc = inventory.SupplierPart.OP_Desc;
				}
				orderLine.WhsOrderLine.WE_ExpiryDate = inventory.WI_ExpiryDate;
				orderLine.WhsOrderLine.WE_PackingDate = inventory.WI_PackingDate;
				orderLine.WhsOrderLine.WE_PartAttrib1 = inventory.WI_PartAttrib1;
				orderLine.WhsOrderLine.WE_PartAttrib2 = inventory.WI_PartAttrib2;
				orderLine.WhsOrderLine.WE_PartAttrib3 = inventory.WI_PartAttrib3;
				orderLine.WhsOrderLine.WE_SerialNumber = inventory.WI_SerialNumber;
				orderLine.WhsOrderLine.WE_TransactionQuantity = inventory.Quantity;
			}

			var quantityToAllocate = Math.Min(inventory.WI_AvailableForCrossDockQuantity, inventory.Quantity);
			if (quantityToAllocate > 0m)
			{
				if (crossDock != null)
				{
					crossDock.ReservedQuantity += quantityToAllocate;
				}
				else
				{
					crossDock = orderLine.WhsOrderLine.ReserveStockIfAbleTo(inventory, orderLine.Factory, quantityToAllocate);
				}
			}

			inventory.Quantity = 0;
		}

		#endregion

		TrackingWhsOrderLine GetExistingOrderLine(TrackingWhsInventory inventory)
		{
			TrackingWhsOrderLine result = null;
			foreach (WhsOrderLine line in ShoppingCart.WhsOrder.Lines)
			{
				if (line.WE_OP == inventory.WI_OP &&
					line.WE_PartAttrib1 == inventory.WI_PartAttrib1 &&
					line.WE_PartAttrib2 == inventory.WI_PartAttrib2 &&
					line.WE_PartAttrib3 == inventory.WI_PartAttrib3 &&
					line.WE_SerialNumber == inventory.WI_SerialNumber &&
					IsInventoryDateMatch(line.WE_PackingDate, inventory.WI_PackingDate) &&
					IsInventoryDateMatch(line.WE_ExpiryDate, inventory.WI_ExpiryDate))
				{
					result = TrackingHelper.Get(line);
					break;
				}
			}
			return result;
		}

		bool IsInventoryDateMatch(ZDate orderLineDate, ZDate inventoryDate)
		{
			return !orderLineDate.IsValid || orderLineDate == inventoryDate;
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

		public void TryRemoveZeroLines()
		{
			var linesToRemove = new List<WhsOrderLine>();
			foreach (WhsOrderLine line in ShoppingCart.WhsOrder.Lines)
			{
				if (line.WE_TransactionQuantity == 0)
				{
					linesToRemove.Add(line);
				}
			}
			foreach (WhsOrderLine line in linesToRemove)
			{
				ShoppingCart.WhsOrder.Lines.RemoveFromRelationship(line);
			}
		}
	}
}
