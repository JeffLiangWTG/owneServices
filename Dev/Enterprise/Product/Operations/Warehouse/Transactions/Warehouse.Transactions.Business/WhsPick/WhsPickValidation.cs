using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickValidation : AutoWhsPickValidation
	{
		public WhsPickValidation(AutoWhsPick parent)
			: base(parent)
		{
		}

		public new WhsPick Parent
		{
			get { return (WhsPick)base.Parent; }
		}

		#region CheckWP_WW_Whs

		protected override void CheckWP_WW_Whs()
		{
			base.CheckWP_WW_Whs();
			if (Parent.Orders.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WP_WW_WhsInfo);

				if (Parent.WP_WW_Whs.IsValid)
				{
					foreach (WhsPickableDocket order in Parent.Orders)
					{
						if (order.WD_WW_Whs != Parent.WP_WW_Whs)
						{
							Parent.WP_WW_WhsInfo.AddError(Res.GetString("443792a3-cf7d-40b8-94d1-ce48508ac455", "One or more orders are attached for a different warehouse. Either change this warehouse or detach the Order(s)"));
							break;
						}
					}
				}
			}
		}

		#endregion

		#region CheckWP_PickOption

		protected override void CheckWP_PickOption()
		{
			base.CheckWP_PickOption();
			ListValidation.ErrorIfInvalidCode(Parent.WP_PickOptionInfo, new WhsPickOption());

			if (Parent.Orders.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.WP_PickOptionInfo);
				if (Parent.Orders.Cast<WhsPickableDocket>().Any(IsOrderWithMisalignedPickOption))
				{
					Parent.WP_PickOptionInfo.AddError(Res.GetString("53d88d08-b2d3-4c80-9d22-f9e3ebc210c5", "One or more orders are attached which have a different Pick Option. Either change this Pick's Option or detach the Order(s)"));
				}
			}

			bool IsOrderWithMisalignedPickOption(WhsPickableDocket order) => (order.WD_PickOption != Parent.WP_PickOption && (!order.WD_WPInfo.Value.Equals(order.WD_WPInfo.OriginalValue) || !order.IsInDatabase));
		}

		#endregion

		#region ValidateDockDoorPK

		public void ValidateDockDoorPK()
		{
			ValidateCalculatedProperty(Parent.DockDoorPKInfo);
		}

		protected void CheckDockDoorPK()
		{
			var isWhsUsingDockDoorLocation = Parent.Warehouse?.IsUsingDockDoorLocation ?? false;
			var isDockDoorLocationMandatory = !Parent.IsWorkOrderPick && isWhsUsingDockDoorLocation;
			if (isDockDoorLocationMandatory)
			{
				if (Parent.Orders.Count > 0)
				{
					MandatoryValidation.CheckEntered(Parent.DockDoorPKInfo);

					if (!Parent.DockDoorPKInfo.Value.IsValid)
					{
						Parent.DockDoorPKInfo.AddError(Res.GetString("F6DBD064-97CD-4016-BEC0-0FEC563BC00D", "Enter a valid Dock Door."));
					}
				}
				CheckDockDoorPK_IsInCorrectWarehouseAndDDLAndLocationStatusIsCorrect();
			}

			CheckDockDoorPK_IsNotChangedIfPickingStartedOrTrolleyExists();
		}

		void CheckDockDoorPK_IsInCorrectWarehouseAndDDLAndLocationStatusIsCorrect()
		{
			if (Parent.DockDoorPK.IsValid && !Parent.DockDoorPKInfo.HasErrors())
			{
				var dockDoorLocation = Parent.DockDoorLocation;
				if (dockDoorLocation == null || !dockDoorLocation.IsDockDoorLocation)
				{
					Parent.DockDoorPKInfo.AddError(Res.GetString("FFF29A32-AA2C-41AA-A68C-F2AED5341127", "Please enter a Dock Door."));
				}
				else if (dockDoorLocation.WLV_WW_Whs != Parent.WP_WW_Whs)
				{
					Parent.DockDoorPKInfo.AddError(Res.GetString("81FA8DAB-BA34-44F4-AAAC-A60FBA39F4D0", "This Dock Door does not belong to Warehouse on the Pick."));
				}
				else if (dockDoorLocation.WLV_LocationStatus != LocationStatus.Codes.Normal)
				{
					Parent.DockDoorPKInfo.AddError(Res.GetString("1ee654e5-8a20-4af7-b550-e1e8635f22a4", "Location must have a status of {0}.", LocationStatus.Codes.Normal));
				}
			}
		}

		void CheckDockDoorPK_IsNotChangedIfPickingStartedOrTrolleyExists()
		{
			CheckInfo_IsNotChangedIfPickingStartedOrTrolleyExistsCore(
				Parent.DockDoorPKInfo,
				Parent.WP_WL_DockDoorInfo,
				Res.GetString("a3fc8324-fd64-4556-b241-40e2d5a2da11", "Cannot change Dock Door as Picking has been started or a Trolley has been created for this Pick."));
		}

		void CheckInfo_IsNotChangedIfPickingStartedOrTrolleyExistsCore(ZPropertyInfo info, ZPropertyInfo originalValueInfo, string errorMessage)
		{
			if (!info.HasErrors()
				&& !originalValueInfo.OriginalValue.IsEmpty
				&& originalValueInfo.HasChanges
				&& (Parent.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.GetLinesToPick()).SelectMany(l => l.PickLines).Any(pl => pl.IsPickedFromPutawayLocation || pl.WZ_IsPicking) || Parent.AnyPackagesOnOrdersHasActiveTrolleyJob))
			{
				info.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckWP_IsAwaitingReplenishment

		protected override void CheckWP_IsAwaitingReplenishment()
		{
			base.CheckWP_IsAwaitingReplenishment();

			if (Parent.WP_IsAwaitingReplenishment && Parent.WP_PickType == PickType.Codes.HeldInventoryOrder)
			{
				Parent.WP_IsAwaitingReplenishmentInfo.AddError(Res.GetString("c73e9ee1-d3ec-43ea-bb9f-dd043ab63b11", "Pick with orders containing held inventory cannot be waiting for replenishment."));
			}
		}

		#endregion

		#region WP_WA_DynamicPickAreaOverride

		protected override void CheckWP_WA_DynamicPickAreaOverride()
		{
			base.CheckWP_WA_DynamicPickAreaOverride();

			CheckWP_WA_DynamicPickAreaOverride_IsInCorrectWarehouseAndDynamicAreaAndAreaHasLocations();
			CheckWP_WA_DynamicPickAreaOverride_IsNotChangedIfPickingStartedOrTrolleyExists();
			CheckWP_WA_DynamicPickAreaOverride_IsNotSetIfOrderLineWithPalletIDAttached();
		}

		void CheckWP_WA_DynamicPickAreaOverride_IsInCorrectWarehouseAndDynamicAreaAndAreaHasLocations()
		{
			if (!Parent.WP_WA_DynamicPickAreaOverrideInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.WP_WA_DynamicPickAreaOverrideInfo);
			}

			if (!Parent.WP_WA_DynamicPickAreaOverrideInfo.HasErrors() && Parent.WP_WA_DynamicPickAreaOverride.IsValid)
			{
				var overrideArea = Parent.DynamicPickAreaOverride;
				if (overrideArea.PickLocations.Count == 0)
				{
					Parent.WP_WA_DynamicPickAreaOverrideInfo.AddError(Res.GetString("89928147-2b11-4fd4-9360-b06a5cdca69b", "Assigned Dynamic Area Override should have at least 1 Pick Location."));
				}
			}
		}

		void CheckWP_WA_DynamicPickAreaOverride_IsNotChangedIfPickingStartedOrTrolleyExists()
		{
			CheckInfo_IsNotChangedIfPickingStartedOrTrolleyExistsCore(
				Parent.WP_WA_DynamicPickAreaOverrideInfo,
				Parent.WP_WA_DynamicPickAreaOverrideInfo,
				Res.GetString("ef7d8780-c589-4c9f-b63a-42880f9de0a7", "Cannot change Dynamic Area Override as Picking has been started or a Trolley has been created for this Pick."));
		}

		void CheckWP_WA_DynamicPickAreaOverride_IsNotSetIfOrderLineWithPalletIDAttached()
		{
			var info = Parent.WP_WA_DynamicPickAreaOverrideInfo;
			if (!info.HasErrors()
				&& info.OriginalValue.IsEmpty
				&& info.HasChanges
				&& Parent.Orders.Cast<WhsPickableDocket>().Where(d => d.WD_DocketType.Equals(DocketType.Codes.Order)).SelectMany(o => o.Lines).Any(l => !l.WE_PalletID.IsEmpty))
			{
				info.AddError(Res.GetString("df459e23-800d-40d6-93ab-894eb823a7a0", "Cannot set Dynamic Area Override as there is an Order with Pallet ID on the Line attached to this Pick."));
			}
		}

		#endregion

		#region ValidateAll

		// Tested in WhsPick.cs by TestCancelPick_NoPickLinesAttachedToCanceledPick() and 
		// TestFinalisePick_FailsOnIncorrectCommittedStockQuantities()
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDockDoorPK();

			var pick = Parent;
			pick.Factory.SynchroniseCachedBusinessObjectsWithDB<WhsPickLine>(false, false); // Update cached pick lines to determine Deleted and Updated pick lines by other instances of Enterprise.
			pick.Factory.ClearQueryCache(WhsPickLineSchema.Constants.TableName); // Clear query cache to load pick lines Added by other instances of Enterprise.

			AddFetchHints(pick);

			foreach (WhsPickOrderedInventory orderedInventory in pick.OrderedInventories)
			{
				foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
				{
					availableInventory.Validation.ValidateAll();
				}
			}
		}

		static void AddFetchHints(WhsPick pick)
		{
			// Since we cleared all the query caches for PickLine we need to re-fetch all PickLines on all Orders.
			pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines).ForEach(l => pick.Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, l.PK));
			var allPickLines = pick.GetAllPickLines();

			// Tested in ReleaseEntryForm.cs TestFinaliseOrderAndPickButtonPerformance_Click_WithStockCommittedToAdjustments()
			// These are required to fetch all the PickLines that belong to the Picked Inventory plus their respective Inventory Lines.
			foreach (var pickLine in allPickLines)
			{
				pick.Factory.AddFetchHint(WhsDocketLineSchema.PK, pickLine.WZ_WE_InventoryLine);
				pick.Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, pickLine.WZ_WE_InventoryLine);
				pick.Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, pickLine.WZ_WE_TransactionLine);
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsPickSchema.Constants.WP_WL_DockDoor, WhsPickSchema.Constants.WP_WA_DynamicPickAreaOverride);

		#endregion
	}
}
