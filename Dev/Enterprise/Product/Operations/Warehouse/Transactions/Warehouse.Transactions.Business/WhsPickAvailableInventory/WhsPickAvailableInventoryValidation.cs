using System;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventoryValidation : ZValidation
	{
		public WhsPickAvailableInventoryValidation(WhsPickAvailableInventory parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly WhsPickAvailableInventory Parent;

		#region ValidatePickLineQuantity

		public void Validate_PickLineQuantity() // underscore to prevent Grid Architecture unnecessarily validating
		{
			ValidateCalculatedProperty(Parent.PickLineQuantityInfo);
		}

		protected virtual void CheckPickLineQuantity()
		{
			if (!Parent.OrderedInventory.Pick.IsFinalisedOrCancelled)
			{
				var pickLines = Parent.PickLines.ToArray();
				var pickLineQuantity = pickLines.Sum(pl => pl.WZ_Units);
				var quantityUserWantsToPick = pickLineQuantity + Parent.QuantityThatCouldNotBeAllocatedOrDeallocated;
				var hasPickedMoreThanAvailable = pickLines.DistinctBy(pl => pl.WZ_WE_InventoryLine).Any(pl => pl.Inventory.WI_AvailableToPickQuantity < 0);
				var supplierPart = Parent.SupplierPart;

				var enterValueGreaterOrEqualToZero = Res.GetString("3631c9ad-13b7-4f6b-a21f-6451314bdc9e", "Please enter a value greater than or equal to zero.");

				if (pickLineQuantity < 0) // this should never happen
				{
					Parent.PickLineQuantityInfo.AddError(enterValueGreaterOrEqualToZero);
				}
				else if (pickLineQuantity > 0 && hasPickedMoreThanAvailable && supplierPart != null)
				{
					AddMoreThanQuantityAvailableForAllocationMessage(false);
				}
				else if (quantityUserWantsToPick > 0 && Parent.InventoryStatus != InventoryStatus.Codes.Available && !Parent.IsPickByBOMKitInventory && !Parent.IsChildOfHeldInventoryOrderPick)
				{
					Parent.PickLineQuantityInfo.AddError(Res.GetString("62d73bcf-a902-4fb9-a142-6b87f64b41cd", "Only Available stock can be allocated."));
				}
				else if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value && quantityUserWantsToPick > 0 && Parent.InventoryStatus != InventoryStatus.Codes.Held && Parent.IsChildOfHeldInventoryOrderPick)
				{
					Parent.PickLineQuantityInfo.AddError(Res.GetString("71476604-EECA-428B-B104-8BEB11FDA0FA", "Only Held stock can be allocated."));
				}
				else if (quantityUserWantsToPick < 0)
				{
					Parent.PickLineQuantityInfo.AddWarning(enterValueGreaterOrEqualToZero);
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated > 0 && Parent.OrderedInventory.PickLineQuantity + Parent.QuantityThatCouldNotBeAllocatedOrDeallocated > Parent.OrderedInventory.QuantityOrdered)
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("0956f833-3d2e-47e6-98d5-68831c0a16c2", "Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock."));
				}
				else if (Parent.OrderedInventory.IsComponentOrderedInventoryOnSalesOrder && Parent.QuantityThatCouldNotBeAllocatedOrDeallocated != 0)
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("8f7bf32a-adb3-4c24-aa9d-c8449c821f12", "Quantity Allocated cannot be modified for BOM Products Allocated without Work Orders."));
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated != 0 && (Parent.OrderedInventory?.Pick?.IsCartonising ?? false))
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("37708cd8-b34f-4433-90d0-e13393599e3a", "Quantity Allocated cannot be modified as the Pick is having Package Labels allocated."));
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated > 0 && Parent.OrderedInventory.Owners.Any(l => (l.Docket as WhsOrder)?.IsLoadingOrLoadedOrDeparted ?? false))
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("45d3bb1a-fa1e-48ba-a4f0-fc6c34744559", "You cannot Allocate more Stock for a Loading, Loaded or Departed Order."));
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated > 0 && supplierPart != null) // this should never happen.
				{
					AddMoreThanQuantityAvailableForAllocationMessage(true);
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated < 0
					&& Parent.PickLines.Any(l => l.HasReleaseCapturedAttribs))
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("f02c4b49-7810-412c-9c06-38a20d375391", "You cannot Allocate less Stock than has been Release Captured."));
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated != 0 && !Parent.IsCurrentlyPicking) // must be unable to de-allocate because there are some packed pick lines.
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("b06835c5-4808-4cc9-9319-d34f9e267eda", "Quantity Allocated cannot be reduced below what is Packed across this Pick."));
				}
				else if (Parent.QuantityThatCouldNotBeAllocatedOrDeallocated != 0 && Parent.IsCurrentlyPicking)
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("4913E6FC-3E79-4987-A99A-0347F3B07614", "No changes can be made to this Pick's allocations because Picking has already commenced."));
				}

				CheckPickLineQuantity_ExpiredStockOrMinimumShelfLifeWarning(pickLineQuantity);
			}
		}

		void AddMoreThanQuantityAvailableForAllocationMessage(bool isWarning)
		{
			var onlyXUnitsAreAvailableForAllocation = Res.GetString("40f3b975-2882-4795-9244-675f91767080", "Only {0} are available for allocation.", Parent.WhsProduct.FormattedQtyAndUnit(Parent.QuantityAvailableToPick));
			if (isWarning)
			{
				Parent.PickLineQuantityInfo.AddWarning(onlyXUnitsAreAvailableForAllocation);
			}
			else
			{
				Parent.PickLineQuantityInfo.AddError(onlyXUnitsAreAvailableForAllocation);
			}
		}

		#region CheckPickLineQuantity_ExpiredStockOrMinimumShelfLifeWarning

		void CheckPickLineQuantity_ExpiredStockOrMinimumShelfLifeWarning(ZDecimal pickLineQuantity)
		{
			if (!Parent.PickLineQuantityInfo.HasErrors() && pickLineQuantity > 0m)
			{
				if (Parent.IsExpired)
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("8f5c6a42-e6d9-401e-a158-21066bb705be", "This stock is expired."));
				}

				if (!Parent.IsAcceptableMinimumShelfLife)
				{
					Parent.PickLineQuantityInfo.AddWarning(Res.GetString("922f27e6-bb0e-499f-b89b-8b0226e72616", "This stock does not satisfy the Minimum Shelf Life of the Consignee."));
				}
			}
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			Validate_PickLineQuantity();
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsPickAvailableInventoryValidation); }
		}

		#endregion
	}
}
