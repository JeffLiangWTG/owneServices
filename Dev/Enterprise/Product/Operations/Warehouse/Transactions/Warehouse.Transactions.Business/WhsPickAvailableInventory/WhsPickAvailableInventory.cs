using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Business;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsPickAvailableInventory : NonPersistentBusinessObject, ILineAttributes, ILineCustomAttributes, IWhsPickAvailableInventory, IWhsPickAvailableInventoryInternals, ILineStaffAssigner
	{
		#region Schema

		public abstract class Schema
		{
			public const string ProductCode = nameof(ProductCode);
			public const string LocationString = nameof(LocationString);
			public const string LocationStatusDesc = nameof(LocationStatusDesc);
			public const string AreaType = nameof(AreaType);
			public const string AreaName = nameof(AreaName);
			public const string QuantityUQ = nameof(QuantityUQ);
			public const string PickLineQuantity = nameof(PickLineQuantity);
			public const string QuantityCrossDocked = nameof(QuantityCrossDocked);
			public const string QuantityAvailableToPick = nameof(QuantityAvailableToPick);
			public const string QuantityCommitted = nameof(QuantityCommitted);
			public const string QuantityTotal = nameof(QuantityTotal);
			public const string Allocate = nameof(Allocate);
			public const string AllocationLog = nameof(AllocationLog);
			public const string AllocationKey = nameof(AllocationKey);
			public const string InventoryStatusDesc = nameof(InventoryStatusDesc);
			public const string ArrivalDate = nameof(ArrivalDate);
			public const string ExpiryDate = nameof(ExpiryDate);
			public const string PackingDate = nameof(PackingDate);
			public const string PartAttrib1 = nameof(PartAttrib1);
			public const string PartAttrib2 = nameof(PartAttrib2);
			public const string PartAttrib3 = nameof(PartAttrib3);
			public const string HoldCode = nameof(HoldCode);
			public const string BondedEntryKey = nameof(BondedEntryKey);
			public const string DeclarantsReference = nameof(DeclarantsReference);
			public const string ValueForDuty = nameof(ValueForDuty);
			public const string BondedWhsQty = nameof(BondedWhsQty);
			public const string VFDPerStockUnit = nameof(VFDPerStockUnit);
			public const string PalletID = nameof(PalletID);
			public const string PackageGroupId = nameof(PackageGroupId);
			public const string PerPackageQty = nameof(PerPackageQty);
			public const string CustomAttrib1 = nameof(CustomAttrib1);
			public const string CustomAttrib2 = nameof(CustomAttrib2);
			public const string CustomAttrib3 = nameof(CustomAttrib3);
			public const string CustomAttrib4 = nameof(CustomAttrib4);
			public const string CustomAttrib5 = nameof(CustomAttrib5);
			public const string CustomAttrib6 = nameof(CustomAttrib6);
			public const string CustomDecimal1 = nameof(CustomDecimal1);
			public const string CustomDecimal2 = nameof(CustomDecimal2);
			public const string CustomDecimal3 = nameof(CustomDecimal3);
			public const string CustomDecimal4 = nameof(CustomDecimal4);
			public const string CustomDecimal5 = nameof(CustomDecimal5);
			public const string CustomDate1 = nameof(CustomDate1);
			public const string CustomDate2 = nameof(CustomDate2);
			public const string CustomDate3 = nameof(CustomDate3);
			public const string CustomDate4 = nameof(CustomDate4);
			public const string CustomDate5 = nameof(CustomDate5);
			public const string CustomFlag1 = nameof(CustomFlag1);
			public const string CustomFlag2 = nameof(CustomFlag2);
			public const string CustomFlag3 = nameof(CustomFlag3);
			public const string CustomFlag4 = nameof(CustomFlag4);
			public const string CustomFlag5 = nameof(CustomFlag5);
			public const string CustomTextBlob1 = nameof(CustomTextBlob1);
			public const string VerifiedNonEmpty = nameof(VerifiedNonEmpty);
		}

		#endregion

		#region Constructors

		public WhsPickAvailableInventory(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Business Object Overrides

		protected override void RunPreSaveValidationCore()
		{
			RebuildSplitByPickedDetailsIfNecessaryForValidation();
			RebuildUOMAvailableInventoriesIfNecessaryForValidation();

			base.RunPreSaveValidationCore();
		}

		WhsPickAvailableInventorySplitByPickedDetailsCollection RebuildSplitByPickedDetailsIfNecessaryForValidation()
		{
			return availableInventoriesSplitByPickedDetails != null && availableInventoriesSplitByPickedDetails.NeedsRefresh
				? AvailableInventoriesSplitByPickedDetails
				: null;
		}

		WhsPickAvailableInventorySplitByUOMCollection RebuildUOMAvailableInventoriesIfNecessaryForValidation()
		{
			return availableInventoriesSplitByUOM != null && availableInventoriesSplitByUOM.NeedsRefresh
				? AvailableInventoriesSplitByUOM
				: null;
		}

		public override void Delete()
		{
			base.Delete();

			// we want to delete all non reserved pick lines
			if (OrderedInventory != null)
			{
				using (((IActiveBusinessObjectCollection)OrderedInventory.PickLines).SuspendListChanged())
				{
					foreach (var pickLine in PickLines.Where(pl => !pl.IsReserveLine).ToArray())
					{
						OrderedInventory.PickLines.Delete(pickLine);
					}
				}
			}

			Deactivate();
		}

		#endregion

		#region Related Entities

		public OrgHeader Client
		{
			get { return OrderedInventory != null ? OrderedInventory.Client : null; }
		}

		public WhsPickOrderedInventory OrderedInventory
		{
			get { return orderedInventory; }
		}

		public OrgSupplierPart SupplierPart
		{
			get { return Factory.Load<OrgSupplierPart>(SupplierPartPK); }
		}

		IOrgSupplierPart IWhsPickAvailableInventory.SupplierPart => SupplierPart;

		public ZGuid SupplierPartPK
		{
			get { return (OrderedInventory != null ? OrderedInventory.SupplierPartPK : ZGuid.Empty); }
		}

		ZGuid ClientPK => Inventory.Count > 0 ? Inventory[0].WI_OH_Client : ZGuid.Empty;

		public WhsProduct WhsProduct
		{
			get { return (OrderedInventory != null ? OrderedInventory.Product : null); }
		}

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(LocationPK); }
		}

		public WhsArea Area => Location?.PickingArea;

		#region PickLines

		#region PickLines

		public IEnumerable<WhsPickLine> PickLines
		{
			get
			{
				return OrderedInventory != null
					? OrderedInventory.PickLines.Where(l => IsPickLineForInventory(l))
					: Array.Empty<WhsPickLine>();
			}
		}

		internal bool IsPickLineForInventory(WhsPickLine pickLine) => Inventory.Contains(pickLine.InventoryLinePKForAvailableInventory);

		public WhsPickLineCollectionND PickLinesCommittedFromAllPicks
		{
			get
			{
				if (pickLinesFromAllPicks == null)
				{
					pickLinesFromAllPicks = new WhsPickLineCollectionND(Factory);
					pickLinesFromAllPicks.AddRange(Inventory.Cast<WhsInventoryView>().SelectMany(i => i.CommittedPickLines));
				}
				return pickLinesFromAllPicks;
			}
		}

		WhsPickLineCollectionND pickLinesFromAllPicks;

		#endregion

		#region ClearPickLinesCache

		public void ClearPickLinesCache()
		{
			RefreshUOMAvailableInventories();
			RefreshSplitByPickedDetails();
		}

		void RefreshUOMAvailableInventories()
		{
			if (availableInventoriesSplitByUOM != null)
			{
				availableInventoriesSplitByUOM.NeedsRefresh = true;
			}
		}

		void RefreshSplitByPickedDetails()
		{
			if (availableInventoriesSplitByPickedDetails != null)
			{
				availableInventoriesSplitByPickedDetails.NeedsRefresh = true;
			}
		}

		#endregion

		#region Deactivate

		public void Deactivate()
		{
			if (OrderedInventory != null)
			{
				orderedInventory = null; // remove orderedInventory association
			}
		}

		#endregion

		#endregion

		#region Inventory

		public WhsInventoryViewCollection Inventory
		{
			get { return inventory ?? (inventory = new WhsInventoryViewCollection(Factory)); }
		}

		#endregion

		#region AvailableInventoriesSplit

		public IEnumerable<WhsPickAvailableInventorySplitBase> AvailableInventoriesSplit
		{
			get
			{
				var pickByUOMEnabled = Pick?.IsPickByUOMEnabled ?? false;
				return pickByUOMEnabled
					? AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitBase>()
					: AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitBase>();
			}
		}

		#endregion

		#region AvailableInventoriesSplitByUOM

		public WhsPickAvailableInventorySplitByUOMCollection AvailableInventoriesSplitByUOM
		{
			get
			{
				if (availableInventoriesSplitByUOM == null)
				{
					availableInventoriesSplitByUOM = new WhsPickAvailableInventorySplitByUOMCollection(Factory, this);
					RegisterEditableChildObject(availableInventoriesSplitByUOM);
					availableInventoriesSplitByUOM.NeedsRefresh = true;
				}

				if (availableInventoriesSplitByUOM.NeedsRefresh)
				{
					availableInventoriesSplitByUOM.RebuildCollection();
				}

				return availableInventoriesSplitByUOM;
			}
		}

		WhsPickAvailableInventorySplitByUOMCollection availableInventoriesSplitByUOM;

		#endregion

		#region AvailableInventoriesSplitByPickedDetails

		public WhsPickAvailableInventorySplitByPickedDetailsCollection AvailableInventoriesSplitByPickedDetails
		{
			get
			{
				if (availableInventoriesSplitByPickedDetails == null)
				{
					availableInventoriesSplitByPickedDetails = new WhsPickAvailableInventorySplitByPickedDetailsCollection(Factory, this);
					RegisterEditableChildObject(availableInventoriesSplitByPickedDetails);
					availableInventoriesSplitByPickedDetails.NeedsRefresh = true;
				}

				if (availableInventoriesSplitByPickedDetails.NeedsRefresh)
				{
					availableInventoriesSplitByPickedDetails.RebuildCollection();
				}

				return availableInventoriesSplitByPickedDetails;
			}
		}

		WhsPickAvailableInventorySplitByPickedDetailsCollection availableInventoriesSplitByPickedDetails;

		#endregion

		#endregion

		#region Validation

		public WhsPickAvailableInventoryValidation Validation
		{
			get
			{
				switch (CountryCode)
				{
					case Constants.CountryCodes.UnitedStates:
						return new WhsPickAvailableInventoryValidationUS(this);
					default:
						return new WhsPickAvailableInventoryValidation(this);
				}
			}
		}

		ZString CountryCode => Pick?.Warehouse?.CountryCode ?? ZString.Empty;

		#endregion

		#region Properties

		#region Product Code

		public ZString ProductCode => SupplierPart?.OP_PartNum ?? ZString.Empty;

		public ZPropertyInfo ProductCodeInfo => GetZPropertyInfo(Schema.ProductCode);

		#endregion

		#region LocationPK

		public ZGuid LocationPK
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_WL : ZGuid.Empty; }
		}

		#endregion

		#region VerifiedLocationIsNotEmpty

		[ReadOnlyMember(nameof(ReadOnlyForNonAllocatedLines))]
		[BusinessObjectTestExclude]
		public ZBool VerifiedNonEmpty
		{
			get => Pick != null && (Pick.VerifiedNonEmptyCacheManager?.GetVerifiedNonEmpty(ClientPK, SupplierPartPK, LocationPK) ?? IsAnyPickLineVerifiedNonEmpty());
			set
			{
				if (Pick != null)
				{
					UpdateRelatedPickLines(value);
				}

				VerifiedNonEmptyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VerifiedNonEmptyInfo => GetZPropertyInfo(Schema.VerifiedNonEmpty);

		void UpdateRelatedPickLines(bool verifiedNonEmpty)
		{
			var matchingAvailableInventory = GetMatchingAvailableInventory().ToArray();

			foreach (var pickLine in matchingAvailableInventory.SelectMany(a => a.PickLines))
			{
				pickLine.WZ_VerifiedEmpty = verifiedNonEmpty ? "N" : "Y";
			}

			matchingAvailableInventory.ForEach(l => l.VerifiedNonEmptyInfo.RefreshBinding());
		}

		bool IsAnyPickLineVerifiedNonEmpty()
		{
			var result = Pick?.Warehouse?.WW_VerifyEmptyLocations ?? false;
			if (result)
			{
				var matchingAvailableInventory = GetMatchingAvailableInventory();
				result = matchingAvailableInventory.SelectMany(a => a.PickLines).Any(pl => pl.WZ_VerifiedEmpty == "N");
			}

			return result;
		}

		IEnumerable<WhsPickAvailableInventory> GetMatchingAvailableInventory()
		{
			var clientPK = ClientPK;
			var supplierPartPK = SupplierPartPK;
			var locationPK = LocationPK;

			return Pick.OrderedInventories
				.Cast<WhsPickOrderedInventory>()
				.Where(o => o.SupplierPartPK == supplierPartPK && ((WhsPickAvailableInventory)o.AvailableInventories.FirstOrDefault())?.ClientPK == clientPK)
				.SelectMany(o => o.AvailableInventories)
				.Cast<WhsPickAvailableInventory>()
				.Where(a => a.LocationPK == locationPK);
		}

		#endregion

		#region IsLocationEmptyAfterFinalisingPick

		public bool IsLocationEmptyAfterFinalisingPick
		{
			get { return PickLines.Cast<IEmptyLocationAfterPickFinalisation>().Any(p => p.IsLocationEmptyAfterFinalisingPick); }
		}

		#endregion

		#region LocationString

		public ZString LocationString => Location?.WLV_LocationString_UserFriendly ?? ZString.Empty;

		public ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		#endregion

		#region LocationStatus

		public ZString LocationStatus => Location?.WLV_LocationStatus ?? ZString.Empty;

		public ZString LocationStatusDesc
		{
			get { return GetLocationStatusDesc(); }
		}

		public ZPropertyInfo LocationStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.LocationStatusDesc); }
		}

		#endregion

		#region InventoryStatus

		public ZString InventoryStatus
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_InventoryStatus : ZString.Empty; }
		}

		public ZString InventoryStatusDesc
		{
			get { return Inventory.Count > 0 ? Inventory[0].StatusDesc : ZString.Empty; }
		}

		public ZPropertyInfo InventoryStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InventoryStatusDesc); }
		}

		#endregion

		#region ArrivalDate

		public ZDateTimeOffset ArrivalDate
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_ArrivalDate : ZDateTimeOffset.Empty; }
		}

		public ZPropertyInfo ArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalDate); }
		}

		#endregion

		#region PalletID

		public ZString PalletID
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_PalletID : ZString.Empty; }
		}

		public ZPropertyInfo PalletIDInfo
		{
			get { return GetZPropertyInfo(Schema.PalletID); }
		}

		#endregion

		#region PackageGroupId

		public ZString PackageGroupId
		{
			get { return Inventory.Count > 0 ? Inventory[0].PackageGroupId : ZString.Empty; }
		}

		public ZPropertyInfo PackageGroupIdInfo
		{
			get { return GetZPropertyInfo(Schema.PackageGroupId); }
		}

		#endregion

		#region PerPackageQty

		public ZDecimal PerPackageQty
		{
			get { return Inventory.Count > 0 ? Inventory[0].PerPackageQty : ZDecimal.Zero; }
		}

		public ZPropertyInfo PerPackageQtyInfo
		{
			get { return GetZPropertyInfo(Schema.PerPackageQty); }
		}

		#endregion

		#region QuantityAvailableToPick

		public ZDecimal QuantityAvailableToPick
		{
			get { return PickingSlipBuilder.QuantityAvailableToPick; }
		}

		public ZPropertyInfo QuantityAvailableToPickInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityAvailableToPick); }
		}

		#endregion

		#region QuantityUnPicked

		public ZDecimal QuantityUnPicked => PickingSlipBuilder.QuantityUnPicked;

		#endregion

		#region AvailableForAllocationAlgorithm

		public bool AvailableForAllocationAlgorithm
			=> IsInventoryPickable &&
				!IsExpired &&
				IsAcceptableMinimumShelfLife &&
				(!OrderedInventory.IsComponentOrderedInventoryOnSalesOrder || Pick.IsAutoAllocatingItemsSemaphore.IsSuspended);

		#endregion

		#region QuantityTotal

		public ZDecimal QuantityTotal
		{
			get { return PickingSlipBuilder.TotalQuantity; }
		}

		public ZPropertyInfo QuantityTotalInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityTotal); }
		}

		#endregion

		#region QuantityCommitted

		public ZDecimal QuantityCommitted
		{
			get { return PickingSlipBuilder.QuantityCommitted; }
		}

		public ZPropertyInfo QuantityCommittedInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityCommitted); }
		}

		#endregion

		#region PickLineQuantity

		[ReadOnlyMember(nameof(ReadOnlyForChangingAllocations))]
		public ZDecimal PickLineQuantity
		{
			get => Pick != null ? PickingSlipBuilder.PickLineQuantity : ZDecimal.Zero;
			set => ((IWhsPickAvailableInventoryInternals)this).SetPickLineQuantity(value, orderLinePk: null);
		}

		decimal IWhsPickAvailableInventoryInternals.SetPickLineQuantity(ZDecimal value, ZGuid? orderLinePk)
		{
			// It is necessary to make sure WhsPickOrderedInventory.PickInventoryDirectFromCrossDock()
			// will do the equivalent of setting PickLineQuantity
			var quantityThatCouldNotBeAllocatedOrDeallocated = 0m;

			if (Pick != null && OrderedInventory != null)
			{
				var oldValue = PickLineQuantity;
				var quantityToAllocateOrDeallocate = value - oldValue;
				var isPickByUOMEnabled = Pick.IsPickByUOMEnabled; // we need to cache this value early - before first pick lines are created - as pickline creation may affect IsPickByUOMEnabled calculation

				quantityThatCouldNotBeAllocatedOrDeallocated = UpdatePickLineQuantityAndSent(quantityToAllocateOrDeallocate, orderLinePk);

				if (quantityToAllocateOrDeallocate != 0m)
				{
					RefreshAvailableInventorySplitBys(isPickByUOMEnabled);
					ClearPickIsAllocated(oldValue, value);
					UpdatePickTotals(quantityToAllocateOrDeallocate - quantityThatCouldNotBeAllocatedOrDeallocated);
				}
			}
			else
			{
				PickLineQuantityInfo.RefreshBinding();
			}

			return quantityThatCouldNotBeAllocatedOrDeallocated;
		}

		#region ClearPickIsAllocated

		void ClearPickIsAllocated(ZDecimal oldValue, ZDecimal value)
		{
			if (value != oldValue && (oldValue == 0m || value == 0m))
			{
				Pick?.ClearIsAllocated();
			}
		}

		#endregion

		#region UpdatePickTotals

		// Tested in WhsPickTest.TestTotals_UpdateTotals_ChangePickLineQuantity and TestUpdatePickTotals_Suspend
		void UpdatePickTotals(ZDecimal adjustQty)
		{
			if (OrderedInventory != null && Pick != null && !Pick.IsUpdatingTotalsSuspended)
			{
				Pick.UpdateTotals(adjustQty, SupplierPart, OrderedInventory);
			}
		}

		#endregion

		#region UpdatePickLineQuantityAndSent

		decimal UpdatePickLineQuantityAndSent(decimal quantityToAllocateOrDeallocate, ZGuid? orderLinePk)
		{
			var quantityThatCouldNotBeAllocatedOrDeallocated = 0m;

			try
			{
				// QuantityThatCouldNotBeAllocatedOrDeallocated is only used for validation to know what the user entered. 'try finally' will ensure it is reset to zero.
				quantityThatCouldNotBeAllocatedOrDeallocated = PickingSlipBuilder.UpdatePickLineQuantity(quantityToAllocateOrDeallocate, orderLinePk);
				QuantityThatCouldNotBeAllocatedOrDeallocated = quantityThatCouldNotBeAllocatedOrDeallocated;
				AllocateAndValidateOrderedInventory(quantityToAllocateOrDeallocate);
			}
			finally
			{
				QuantityThatCouldNotBeAllocatedOrDeallocated = 0m;
			}

			return quantityThatCouldNotBeAllocatedOrDeallocated;
		}

		internal void AllocateAndValidateOrderedInventory(decimal quantityToAllocateOrDeallocate)
		{
			var quantityAllocatedOrDeallocated = quantityToAllocateOrDeallocate - QuantityThatCouldNotBeAllocatedOrDeallocated;
			if (quantityAllocatedOrDeallocated != 0m)
			{
				Pick.CurrentPickStrategy.OnInventoryPicked(OrderedInventory, this, quantityAllocatedOrDeallocated);

				var valueThatWasSet = PickLineQuantity + QuantityThatCouldNotBeAllocatedOrDeallocated;
				var hasAllocatedFromZeroOrDeallocatedToZero = (quantityToAllocateOrDeallocate == valueThatWasSet || (quantityToAllocateOrDeallocate < 0 && valueThatWasSet == 0));
				if (Pick.WP_PercentageComplete != 0 && hasAllocatedFromZeroOrDeallocatedToZero) // very resourceful task, do not do needlessly.
				{
					Pick.ReCalculatePercentageComplete();
				}

				UpdateAllocationLog(quantityAllocatedOrDeallocated);

				foreach (var owner in OrderedInventory.Owners)
				{
					owner.ClearWE_ShortfallQuantityCached(); // if we allocate or deallocate stock, we should clear the shortfall quantity so it gets updated on next access.
				}
			}

			ValidatePickLineQuantity();
			PickLineQuantityInfo.RefreshBinding();
		}

		/// <summary>
		/// Only relevant for 'PickLineQuantity' Validation, do *not* use for anything else.
		/// This is cleared immediately after it is used in the validation.
		/// </summary>
		internal ZDecimal QuantityThatCouldNotBeAllocatedOrDeallocated
		{
			get;
			private set;
		}

		#endregion

		#region RefreshAvailableInventorySplitBys

		void RefreshAvailableInventorySplitBys(bool isPickByUOMEnabled)
		{
			if (!DeferredSplittingAndPickLineQuantityValidationSemaphore.IsSuspended)
			{
				if (isPickByUOMEnabled)
				{
					SplitByPackType();
				}
				else
				{
					if (availableInventoriesSplitByPickedDetails != null)
					{
						availableInventoriesSplitByPickedDetails.NeedsRefresh = true;
					}
					_ = AvailableInventoriesSplitByPickedDetails; // Trigger refresh
				}
			}
		}

		#endregion

		#region CanUpdatePickLineQuantity

		public bool CanUpdatePickLineQuantity
		{
			get
			{
				var pick = OrderedInventory.Pick;
				var isAutoAllocating = pick.IsAutoAllocatingItemsSemaphore.IsSuspended;

				return
					isAutoAllocating ||
					(!OrderedInventory.IsComponentOrderedInventoryOnSalesOrder // i.e. prevent allocations for Pick By BOM outside of AutoAllocate to stop allocating incomplete bom kits. 
					&& !pick.IsCartonising // Running Cartonisation in another instance, show a nice message to the user if they allocated via the GUI manually.
					&& !IsPickByBOMKitInventory);
			}
		}

		#endregion

		public bool IsCurrentlyPicking
		{
			get
			{
				return OrderedInventory.Pick?.IsPicking ?? false;
			}
		}

		public ZPropertyInfo PickLineQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.PickLineQuantity); }
		}

		void ValidatePickLineQuantity(bool validateOrderedInventory = true)
		{
			if (!IsValidationSuspended && !DeferredSplittingAndPickLineQuantityValidationSemaphore.IsSuspended)
			{
				Validation.Validate_PickLineQuantity();
				if (validateOrderedInventory && OrderedInventory != null)
				{
					OrderedInventory.Validation.ValidatePickLineQuantity();
					OrderedInventory.Validation.ValidateQuantityShort();
					OrderedInventory.RefreshBinding();
				}
			}
		}

		internal void SplitByPackType()
		{
			if (availableInventoriesSplitByUOM != null)
			{
				availableInventoriesSplitByUOM.NeedsRefresh = true;
			}

			PickingSlipBuilder.SplitByPackType();
			_ = AvailableInventoriesSplitByUOM; // Trigger refresh
			Pick?.ClearPackTypeAllocatedReadOnlyCache();
		}

		internal IDisposable DeferSplittingByPackTypeAndPickLineQuantityValidation(bool validateOrderedInventory = true)
		{
			var semaphore = new SemaphoreManager(DeferredSplittingAndPickLineQuantityValidationSemaphore);

			return new DisposableAction(() =>
			{
				semaphore.Dispose();

				RefreshAvailableInventorySplitBys(Pick.IsPickByUOMEnabled);
				ValidatePickLineQuantity(validateOrderedInventory);
			});
		}

		Semaphore DeferredSplittingAndPickLineQuantityValidationSemaphore
		{
			get { return deferredSplittingAndPickLineQuantityValidationSemaphore ?? (deferredSplittingAndPickLineQuantityValidationSemaphore = new Semaphore()); }
		}

		Semaphore deferredSplittingAndPickLineQuantityValidationSemaphore;

		#region AllowPickLineQuantityReduction

		public IDisposable AllowPickLineQuantityReduction()
		{
			return new DisposableAction(
				EnableQuantityReduction,
				() => allowQuantityReduction = false
			);

			void EnableQuantityReduction()
			{
				if (allowQuantityReduction)
				{
					throw new InvalidOperationException("You may not invoke AllowPickLineQuantityReduction more than once.");
				}
				allowQuantityReduction = true;
			}
		}

		internal bool allowQuantityReduction;

		#endregion

		#endregion

		#region QuantityCrossDocked

		[ReadOnly(true)]
		public ZDecimal QuantityCrossDocked
		{
			get { return (Pick != null ? PickingSlipBuilder.QuantityCrossDocked : ZDecimal.Zero); }
		}

		public ZPropertyInfo QuantityCrossDockedInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.QuantityCrossDocked);
				return info;
			}
		}

		#endregion

		#region Allocate

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ReadOnlyForChangingAllocations))]
		public ZBool Allocate
		{
			get { return PickLineQuantity > 0m; }
			set
			{
				AllocateQuantityByCheckBox(value);
				AllocateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AllocateInfo
		{
			get { return GetZPropertyInfo(Schema.Allocate); }
		}

		#endregion

		#region AllocationLog

		[ReadOnly(true)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString AllocationLog
		{
			get { return allocationLog; }
			set
			{
				if (allocationLog != value)
				{
					using (SuspendSettingHasChanges()) // no reason to set HasChanges when setting this Non-Persistent Log
					{
						SetNonPersistentPropertyValue(AllocationLogInfo, ref allocationLog, value);
					}
				}
			}
		}

		ZString allocationLog;

		public ZPropertyInfo AllocationLogInfo
		{
			get { return GetZPropertyInfo(Schema.AllocationLog); }
		}

		public Stack<string> AllocationLogEntryStack
		{
			get
			{
				if (allocationLogEntryStack == null)
				{
					allocationLogEntryStack = new Stack<string>();
					allocationLogEntryStack.Push(Res.GetString("dd657455-0441-4e64-8513-c7d71f338234", "user"));
				}
				return allocationLogEntryStack;
			}
		}

		Stack<string> allocationLogEntryStack;

		void IWhsPickAvailableInventoryInternals.UpdateAllocationLog(string allocationLogName, ZDecimal valueDelta)
		{
			Argument.NotNullOrEmpty(allocationLogName, nameof(allocationLogName));

			if (valueDelta == 0m)
			{
				throw new ArgumentException("Expected non-zero value delta.");
			}

			AllocationLogEntryStack.Push(allocationLogName);
			UpdateAllocationLog(valueDelta);
			AllocationLogEntryStack.Pop();
		}

		void UpdateAllocationLog(ZDecimal valueDelta)
		{
			if (WhsProduct != null && !IsUpdatingAllocationLogSuspended)
			{
				var logEntry = AllocationLogEntryStack.Peek();
				var formattedQtyAndUnit = WhsProduct.FormattedQtyAndUnit(Math.Abs(valueDelta));
				var operation = valueDelta >= 0 ? Res.GetString("d4f825ac-7bd0-4c5b-a8d2-d8b3a06e7cbb", "allocated") : Res.GetString("b4c2352b-6193-477e-bcd8-7a23b687c9f9", "deallocated");
				var logInfo = string.Empty;
				if (logEntry == Res.GetString("dd657455-0441-4e64-8513-c7d71f338234", "user"))
				{
					logInfo = Res.GetString("bfe2927c-fbca-4710-b71e-038d9a382258", "{0} manually {1} by user", formattedQtyAndUnit, operation);
				}
				else
				{
					logInfo = Res.GetString("373af36b-863d-47b1-af9a-d56eb3cb258f", "{0} {1} from {2}", formattedQtyAndUnit, operation, logEntry);
				}

				AllocationLog += logInfo + System.Environment.NewLine;
			}
		}

		bool IsUpdatingAllocationLogSuspended => Pick?.IsUpdatingAllocationLogSuspended ?? false;

		#endregion

		#region QuantityUQ

		public ZString QuantityUQ
		{
			get
			{
				ZString result = "UNT";
				if (SupplierPart != null)
				{
					result = SupplierPart.OP_StockKeepingUnit;
				}
				return result;
			}
		}

		public ZPropertyInfo QuantityUQInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityUQ); }
		}

		#endregion

		#region Area

		public ZString AreaName
		{
			get { return Area != null ? Area.WA_NameMultilingual : ZString.Empty; }
		}

		public ZString AreaType
		{
			get { return Location?.WLV_PickingAreaType ?? ZString.Empty; }
		}

		public ZPropertyInfo AreaNameInfo
		{
			get { return GetZPropertyInfo(Schema.AreaName); }
		}

		public ZPropertyInfo AreaTypeInfo
		{
			get { return GetZPropertyInfo(Schema.AreaType); }
		}

		#endregion

		#region VFD

		bool IsCustomsTransaction => Pick?.IsCustomsTransaction ?? false;

		ZDecimal ValueForDuty => IsCustomsTransaction && Inventory.Count > 0 ? Inventory[0].CustomsData.WB_ValueForDuty : ZDecimal.Zero;

		ZDecimal BondedWhsQty => IsCustomsTransaction && Inventory.Count > 0 ? Inventory[0].CustomsData.WB_BondedWhsQty : ZDecimal.Zero;

		[ResourceStringData("WhsPickAvailableInventory|VFDPerStockUnit", Caption = "VFD Per Stock Unit")]
		public ZDecimal VFDPerStockUnit
		{
			get
			{
				var result = 0m;
				var vfd = ValueForDuty;
				var bondedQty = BondedWhsQty;

				if (vfd > 0m && bondedQty > 0m)
				{
					result = vfd / bondedQty;
				}

				return result;
			}
		}

		public ZPropertyInfo VFDPerStockUnitInfo => GetZPropertyInfo(Schema.VFDPerStockUnit);

		#endregion

		#region ReadOnly

		bool ReadOnlyForChangingAllocationsCore => ReadOnlyForUnfinalizedNonAvailableStock || IsPartiallyOrFullyPickedFromPutawayLocation || Pick.IsReadyForPlanningOrPlanned;

		bool ReadOnlyForChangingAllocations => ReadOnlyForChangingAllocationsCore || HasSerialNumber;

		internal bool ReadOnlyForUnfinalizedNonAvailableStock => StandardReadOnly || (!IsInventoryPickable && PickLineQuantity == 0m) || IsPickByBOMKitInventory;

		bool ReadOnlyForNonAllocatedLines
		{
			get
			{
				var warehouse = Pick?.Warehouse;
				return warehouse == null || !warehouse.WW_VerifyEmptyLocations || ReadOnlyForUnfinalizedNonAvailableStock || !Allocate;
			}
		}

		bool StandardReadOnly => Pick?.IsFinalisedOrCancelled ?? false;

		#endregion

		#region Flags

		#region IsAllocatedToPickFace

		public bool IsAllocatedToPickFace()
		{
			var product = WhsProduct; // Lower DB hit count for OrgSupplierPart table 2 times.
			if (product != null)
			{
				var locationPK = LocationPK;
				foreach (WhsPickFace pickFace in product.PickFaces)
				{
					if (pickFace.WF_WL == locationPK)
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region IsInventoryPickable

		public bool IsInventoryPickable
		{
			get
			{
				var result = false;
				if (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value && IsChildOfHeldInventoryOrderPick)
				{
					result = InventoryStatus == CodeLists.InventoryStatus.Codes.Held;
				}
				else if (InventoryStatus == CodeLists.InventoryStatus.Codes.Available)
				{
					var location = Location;
					result = location != null && location.WLV_LocationStatus == Environment.CodeLists.LocationStatus.Codes.Normal;
				}
				return result;
			}
		}

		#endregion

		#region IsPickByBOMKitInventory

		public bool IsPickByBOMKitInventory => Inventory.Count > 0 && Inventory[0].IsPickByBOMKitInventory;

		#endregion

		#region IsChildOfHeldInventoryPick

		public bool IsChildOfHeldInventoryOrderPick => Pick?.IsHeldInventoryOrderPick ?? false;

		#endregion

		#region IsAcceptableMinimumShelfLife

		public bool IsAcceptableMinimumShelfLife
		{
			get
			{
				var minimumShelfLife = OrderedInventory.MinimumShelfLife;
				return minimumShelfLife <= 0 || ExpiryDate >= ZDateTime.Today.AddDays(minimumShelfLife);
			}
		}

		#endregion

		#region IsExpired

		public bool IsExpired => ExpiryDate <= ZDateTime.Today;

		#endregion

		#region IsPartiallyOrFullyPickedFromPutawayLocation

		bool IsPartiallyOrFullyPickedFromPutawayLocation
		{
			get { return PickLineQuantity > 0m && PickLines.Any(l => l.IsPickedFromPutawayLocation); }
		}

		#endregion

		#endregion

		#endregion

		#region ILineAttributes Members

		public ZDate ExpiryDate => Inventory.Count > 0 ? Inventory[0].WI_ExpiryDate : ZDate.Empty;

		public ZDate PackingDate => Inventory.Count > 0 ? Inventory[0].WI_PackingDate : ZDate.Empty;

		public ZString BondedEntryKey => Inventory.Count > 0 ? Inventory[0].WI_BondedEntryKey : ZString.Empty;

		public ZString DeclarantsReference
		{
			get
			{
				var declarantsReference = ZString.Empty;
				if (Inventory.Count > 0)
				{
					declarantsReference = Inventory[0].Docket?.WD_CustomerReference ?? ZString.Empty;
				}
				return declarantsReference;
			}
		}

		[ResourceStringData("WhsPickAvailableInventory|AllocationKey", Caption = "Allocation Key")]
		public ZString AllocationKey => Inventory.Count > 0 ? Inventory[0].WI_AllocationKey : ZString.Empty;

		public ZDateTime BondedEntryDate => IsCustomsTransaction && Inventory.Count > 0 ? Inventory[0].CustomsData.WB_EntryDate : ZDateTime.Empty;

		// Attribute Neutral tested in WhsPickAvailableInventoryCollectionTest
		public ZString PartAttrib1 => Inventory.Count > 0 ? Inventory[0].WI_PartAttrib1 : ZString.Empty;

		public ZString PartAttrib2 => Inventory.Count > 0 ? Inventory[0].WI_PartAttrib2 : ZString.Empty;

		public ZString PartAttrib3 => Inventory.Count > 0 ? Inventory[0].WI_PartAttrib3 : ZString.Empty;

		[ResourceStringData("WhsPickAvailableInventory|HoldCode", Caption = "Hold Code")]
		public ZString HoldCode => Inventory.Count > 0 ? Inventory[0].WI_HeldCode : ZString.Empty;

		[ResourceStringData("WhsPickAvailableInventory|SerialNumber", Caption = "Serial Number", ShortCaption = "Serial #")]
		public ZString SerialNumber => Inventory.Count > 0 && !IsAttributeNeutralUsed && !WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value ? Inventory[0].WI_SerialNumber : ZString.Empty;

		[ChildEditableTestExclude]
		public WhsSerialNumberPivotSelectorCollection SerialNumbers
		{
			get
			{
				if (serialNumbers == null)
				{
					if (!HasSerialNumber || orderedInventory == null)
					{
						serialNumbers = WhsSerialNumberPivotSelectorCollection.Empty;
					}
					else
					{
						var serialNumberPivots = Inventory.Select(i => i.InDocketLine).OfType<ISerialNumberParent>().SelectMany(i => i.SerialNumbers);
						var serialNumber = orderedInventory.SerialNumber;
						bool CannotAllocateMoreSerials(ZGuid plPK) => orderedInventory.QuantityShort == 0 && plPK.IsEmpty;
						bool IsAlreadyAllocatedToAnotherInventory(ZGuid plPK) => plPK.IsValid && !PickLines.Any(p => p.PK.Equals(plPK));
						bool shouldBeReadOnly(ZGuid pickLinePK) => ReadOnlyForChangingAllocationsCore || CannotAllocateMoreSerials(pickLinePK) || IsAlreadyAllocatedToAnotherInventory(pickLinePK);

						serialNumbers = new WhsSerialNumberPivotSelectorCollection(serialNumberPivots, Pick, serialNumber, shouldBeReadOnly);
						serialNumbers.SelectionChanged += SerialNumbers_SelectionChanged;
					}

					RegisterEditableChildObject(serialNumbers);
				}

				return serialNumbers;
			}
		}
		WhsSerialNumberPivotSelectorCollection serialNumbers;

		void SerialNumbers_SelectionChanged(object sender, EventArgs e)
		{
			var selector = (ISerialNumberPivotAssigner)sender;
			PickingSlipBuilder.UpdatePickLineQuantity(selector.Selected ? -1 : 1, null, selector);
		}

		public bool HasSerialNumber => WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value && IsSerialNumberUsed;

		bool IsSerialNumberUsed => WhsProduct?.IsSerialNumberUsed(Client) ?? false;

		bool IsAttributeNeutralUsed => WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(Factory, SupplierPartPK, ClientPK, OrderedInventory);

		public void SetAttributes(ILineAttributes src) => throw new NotSupportedException();

		public ZPropertyInfo ExpiryDateInfo => GetZPropertyInfo(Schema.ExpiryDate);

		public ZPropertyInfo PackingDateInfo => GetZPropertyInfo(Schema.PackingDate);

		public ZPropertyInfo BondedEntryKeyInfo => GetZPropertyInfo(Schema.BondedEntryKey);

		public ZPropertyInfo DeclarantsReferenceInfo => GetZPropertyInfo(Schema.DeclarantsReference);

		public ZPropertyInfo PartAttrib1Info => GetZPropertyInfo(Schema.PartAttrib1);

		public ZPropertyInfo PartAttrib2Info => GetZPropertyInfo(Schema.PartAttrib2);

		public ZPropertyInfo PartAttrib3Info => GetZPropertyInfo(Schema.PartAttrib3);

		public ZPropertyInfo HoldCodeInfo => GetZPropertyInfo(Schema.HoldCode);

		#endregion

		#region ILineCustomAttributes Members

		public ZString CustomAttrib1
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib_1 : ZString.Empty; }
		}

		public ZString CustomAttrib2
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib_2 : ZString.Empty; }
		}

		public ZString CustomAttrib3
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib_3 : ZString.Empty; }
		}

		public ZString CustomAttrib4
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib4 : ZString.Empty; }
		}

		public ZString CustomAttrib5
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib5 : ZString.Empty; }
		}

		public ZString CustomAttrib6
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomAttrib6 : ZString.Empty; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDecimal1 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDecimal2 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDecimal3 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDecimal4 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDecimal5 : ZDecimal.Zero; }
		}

		public ZDateTime CustomDate1
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDate1 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate2
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDate2 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate3
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDate3 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate4
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDate4 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate5
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomDate5 : ZDateTime.Empty; }
		}

		public ZBool CustomFlag1
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomFlag1 : ZBool.False; }
		}

		public ZBool CustomFlag2
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomFlag2 : ZBool.False; }
		}

		public ZBool CustomFlag3
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomFlag3 : ZBool.False; }
		}

		public ZBool CustomFlag4
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomFlag4 : ZBool.False; }
		}

		public ZBool CustomFlag5
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomFlag5 : ZBool.False; }
		}

		public ZString CustomTextBlob1
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_CustomTextBlob1 : ZString.Empty; }
		}

		public void SetCustomAttributes(ILineCustomAttributes a)
		{
			throw new NotSupportedException();
		}

		public ZPropertyInfo CustomAttrib1Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib1); }
		}

		public ZPropertyInfo CustomAttrib2Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib2); }
		}

		public ZPropertyInfo CustomAttrib3Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib3); }
		}

		public ZPropertyInfo CustomAttrib4Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib4); }
		}

		public ZPropertyInfo CustomAttrib5Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib5); }
		}

		public ZPropertyInfo CustomAttrib6Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttrib6); }
		}

		public ZPropertyInfo CustomDecimal1Info
		{
			get { return GetZPropertyInfo(Schema.CustomDecimal1); }
		}

		public ZPropertyInfo CustomDecimal2Info
		{
			get { return GetZPropertyInfo(Schema.CustomDecimal2); }
		}

		public ZPropertyInfo CustomDecimal3Info
		{
			get { return GetZPropertyInfo(Schema.CustomDecimal3); }
		}

		public ZPropertyInfo CustomDecimal4Info
		{
			get { return GetZPropertyInfo(Schema.CustomDecimal4); }
		}

		public ZPropertyInfo CustomDecimal5Info
		{
			get { return GetZPropertyInfo(Schema.CustomDecimal5); }
		}

		public ZPropertyInfo CustomDate1Info
		{
			get { return GetZPropertyInfo(Schema.CustomDate1); }
		}

		public ZPropertyInfo CustomDate2Info
		{
			get { return GetZPropertyInfo(Schema.CustomDate2); }
		}

		public ZPropertyInfo CustomDate3Info
		{
			get { return GetZPropertyInfo(Schema.CustomDate3); }
		}

		public ZPropertyInfo CustomDate4Info
		{
			get { return GetZPropertyInfo(Schema.CustomDate4); }
		}

		public ZPropertyInfo CustomDate5Info
		{
			get { return GetZPropertyInfo(Schema.CustomDate5); }
		}

		public ZPropertyInfo CustomFlag1Info
		{
			get { return GetZPropertyInfo(Schema.CustomFlag1); }
		}

		public ZPropertyInfo CustomFlag2Info
		{
			get { return GetZPropertyInfo(Schema.CustomFlag2); }
		}

		public ZPropertyInfo CustomFlag3Info
		{
			get { return GetZPropertyInfo(Schema.CustomFlag3); }
		}

		public ZPropertyInfo CustomFlag4Info
		{
			get { return GetZPropertyInfo(Schema.CustomFlag4); }
		}

		public ZPropertyInfo CustomFlag5Info
		{
			get { return GetZPropertyInfo(Schema.CustomFlag5); }
		}

		public ZPropertyInfo CustomTextBlob1Info
		{
			get { return GetZPropertyInfo(Schema.CustomTextBlob1); }
		}

		#endregion

		#region IWhsPickAvailableInventoryInternals Members

		void IWhsPickAvailableInventoryInternals.SetAllProperties(WhsPickOrderedInventory whsOrderedInventory, WhsInventoryView inventoryView)
		{
			this.orderedInventory = whsOrderedInventory;

			if (inventoryView != null)
			{
				this.Inventory.Add(inventoryView);
			}
		}

		#endregion

		#region Lookups

		public WhsPickAvailableInventoryLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		WhsPickAvailableInventoryLookups GetNewLookups()
		{
			return new WhsPickAvailableInventoryLookups(this);
		}

		WhsPickAvailableInventoryLookups fLookups;

		#endregion

		#region Implementation

		WhsPick Pick
		{
			get { return (OrderedInventory != null ? OrderedInventory.Pick : null); }
		}

		PickingSlipBuilder PickingSlipBuilder
		{
			get { return pickingSlipBuilder ?? (pickingSlipBuilder = new PickingSlipBuilder(this)); }
		}

		void AllocateQuantityByCheckBox(bool checkBoxValue)
		{
			if (!checkBoxValue)
			{
				if (PickLineQuantity != 0)
				{
					PickLineQuantity = 0m;
					if (HasSerialNumber)
					{
						DeallocateAllSerialNumbers();
					}
				}
			}
			else
			{
				if (OrderedInventory != null)
				{
					PickLineQuantity += Math.Min(QuantityUnPicked, OrderedInventory.QuantityShort);
				}
			}
			PickLineQuantityInfo.RefreshBinding();

			void DeallocateAllSerialNumbers()
			{
				SerialNumbers
					.Cast<ISerialNumberPivotAssigner>()
					.ForEach(s => s.RemoveSerialNumberSelection());
			}
		}

		ZString GetLocationStatusDesc()
		{
			var result = ZString.Empty;

			var location = Location;
			if (location != null)
			{
				if (SupplierPart != null)
				{
					foreach (var pickFace in WhsProduct.PickFaces)
					{
						if (pickFace.WF_WL == location.PK)
						{
							result = Res.GetString("d2442a61-959e-4317-b84a-0f8a35f1a98a", "Face");
							break;
						}
					}
				}

				if (location.WLV_LocationStatus != Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Normal)
				{
					if (!result.IsEmpty)
					{
						result += ", ";
					}

					result += location.LocationStatuses.GetDescriptionFromCode(location.WLV_LocationStatus);
				}
			}

			return result;
		}

		WhsPickOrderedInventory orderedInventory;
		WhsInventoryViewCollection inventory;
		PickingSlipBuilder pickingSlipBuilder;

		#endregion

		#region ILineAssigner Members

		void ILineStaffAssigner.AssignLine(GlbStaff staff) => AssignOrUnAssignLine((item) => item.AssignedToPK = staff?.PK ?? ZGuid.Empty);

		void ILineStaffAssigner.UnAssignLine(GlbStaff staff) => AssignOrUnAssignLine((item) =>
		{
			if (staff == null || staff.PK == item.AssignedToPK)
			{
				item.AssignedToPK = ZGuid.Empty;
			}
		});

		void AssignOrUnAssignLine(Action<WhsPickAvailableInventorySplitBase> assignOrUnAssignAction)
		{
			if (Pick != null)
			{
				foreach (var item in AvailableInventoriesSplit)
				{
					assignOrUnAssignAction(item);
				}
			}
		}

		bool ILineStaffAssigner.CanAssignOrUnAssignLine() => Allocate
			&& PickLines.Any()
			&& !PickLines.Any(pl => pl.IsPickedFromPutawayLocation)
			&& !PickLines.Any(pl => pl.WZ_IsPicking);

		#endregion
	}

	#region WhsPickAvailableInventoryInternals

	internal interface IWhsPickAvailableInventoryInternals
	{
		void SetAllProperties(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory);
		void UpdateAllocationLog(string allocationLog, ZDecimal valueDelta);
		decimal SetPickLineQuantity(ZDecimal value, ZGuid? orderLinePk);
	}

	#endregion

	#region PickingSlipBuilder

	/// <summary>
	/// This object maintains a WhsPickLineCollecion that details exactly what WhsInventoryView rows are used for the pick.
	/// This object must be cached because of quantityAllocatedOverflow
	/// </summary>
	class PickingSlipBuilder
	{
		#region Constructors

		public PickingSlipBuilder(WhsPickAvailableInventory availableInventory)
		{
			AvailableInventory = Argument.NotNull(availableInventory, nameof(availableInventory));
		}

		readonly WhsPickAvailableInventory AvailableInventory;

		#endregion

		#region Properties

		ZDecimal PickLineQuantityNotPicked => AvailableInventory.PickLines.Where(pl => !pl.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units);

		public ZDecimal PickLineQuantity => AvailableInventory.PickLines.Sum(pl => pl.WZ_Units);

		public ZDecimal QuantityCrossDocked
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsInventoryView inventory in AvailableInventory.Inventory)
				{
					result += inventory.WI_CrossDockQuantity;
				}
				return result;
			}
		}

		public ZDecimal QuantityAvailableToPick
		{
			get { return QuantityUnPicked + PickLineQuantityNotPicked; }
		}

		public ZDecimal QuantityUnPicked
		{
			get
			{
				var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;
				ZDecimal result = 0m;

				foreach (WhsInventoryView inventory in AvailableInventory.Inventory)
				{
					result += isRegistryEnableHeldGoodsForOrders && inventory.IsHeld
						? inventory.WI_AvailableToTransferQuantity
						: inventory.WI_AvailableToPickQuantity;
				}

				return result;
			}
		}

		public ZDecimal TotalQuantity
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsInventoryView inventory in AvailableInventory.Inventory)
				{
					result += inventory.WI_TotalUnits;
				}
				return result;
			}
		}

		public ZDecimal QuantityCommitted
		{
			get
			{
				var result = 0m;
				foreach (WhsInventoryView inventory in AvailableInventory.Inventory)
				{
					result += ((IWhsInventoryInternals)inventory).CommittedToTransactionQuantity;
				}
				return result;
			}
		}

		#endregion

		#region UpdatePickLineQuantity

		public ZDecimal UpdatePickLineQuantity(ZDecimal quantityToAllocateOrDeallocate, ZGuid? orderLinePk, ISerialNumberPivotAssigner specifiedSNPAssigner = null)
		{
			if (specifiedSNPAssigner != null && Math.Abs(quantityToAllocateOrDeallocate) != 1)
			{
				throw new InvalidOperationException("Allocation/Deallocation must be 1 for the serial number.");
			}

			var quantityThatCouldNotBeAllocatedOrDeallocated = quantityToAllocateOrDeallocate;

			if (AvailableInventory.CanUpdatePickLineQuantity)
			{
				var orderedInventory = AvailableInventory?.OrderedInventory;

				var specifiedOrderLine = orderLinePk != null ? (WhsPickableDocketLine)orderedInventory.Owners.FindByPK(orderLinePk.Value) : null;
				if (orderLinePk != null && specifiedOrderLine == null)
				{
					throw new ArgumentException("Attempted to UpdatePickLineQuantity for an order line from another ordered inventory!");
				}

				// We need to clear release lines if we are on the pick screen, because they will not be kept in sync anyway
				var isAlterPickProcess = orderedInventory?.Pick?.IsAlterPick ?? false;
				if (!isAlterPickProcess)
				{
					foreach (var orderLine in orderedInventory.Owners)
					{
						orderLine.ClearReleaseLines();
					}
				}
				else
				{
					foreach (var orderLine in orderedInventory.Owners.Where(x => !x.IsReleaseLineCollectionBuilt))
					{
						orderLine.BuildReleaseLines();
					}
				}

				if (quantityToAllocateOrDeallocate > 0m)
				{
					quantityThatCouldNotBeAllocatedOrDeallocated = IncrementPickingSlip(quantityToAllocateOrDeallocate, specifiedOrderLine, specifiedSNPAssigner);
				}
				else if (quantityToAllocateOrDeallocate < 0m)
				{
					quantityThatCouldNotBeAllocatedOrDeallocated = DecrementPickingSlip(quantityToAllocateOrDeallocate, specifiedOrderLine, specifiedSNPAssigner);
				}
			}

			return quantityThatCouldNotBeAllocatedOrDeallocated;
		}

		#region IncrementPickingSlip

		ZDecimal IncrementPickingSlip(ZDecimal quantityToAllocate, WhsPickableDocketLine specifiedDocketLine, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var quantityNotAllocated = TryToIncrementExistingPickLines(quantityToAllocate, specifiedDocketLine, specifiedSNPAssigner);
			if (quantityNotAllocated > 0m)
			{
				quantityNotAllocated = CreateNewPickLines(quantityNotAllocated, specifiedDocketLine, specifiedSNPAssigner);
			}

			return quantityNotAllocated;
		}

		#region TryToIncrementExistingPickLines

		ZDecimal TryToIncrementExistingPickLines(ZDecimal quantity, WhsPickableDocketLine specifiedDocketLine, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;
			var pickLines = GetPickLinesForIncrementOrDecrement(specifiedDocketLine, specifiedSNPAssigner);

			for (var index = pickLines.Length - 1; index >= 0; index--)
			{
				var pickLine = pickLines[index];
				if (!pickLine.HasReleaseCapturedAttribs // skip the PickLines with RCAs
					&& !pickLine.IsPickedFromPutawayLocation // shouldn't increase Picked Qty only Allocated Qty
					&& pickLine.WZ_GS_NKAssignedTo.IsEmpty // don't increase Pickline Qty for Assigned Lines
					&& pickLine.IsUnpacked(pickLine.Factory) // don't increase PickLine Qty for Packed Items
					&& !pickLine.WZ_IsPicking)
				{
					var pickableDocketLine = ((WhsPickableDocketLine)pickLine.DocketLine);

					var quantityNotPicked = pickableDocketLine.QuantityNotPicked;
					if (quantityNotPicked > 0m)
					{
						var quantityAvailable = isRegistryEnableHeldGoodsForOrders && pickLine.Inventory.IsHeld
							? pickLine.Inventory.WI_AvailableToTransferQuantity
							: pickLine.Inventory.WI_AvailableToPickQuantity;

						if (quantityAvailable > 0m)
						{
							var pickedByBOMPriorToIncrementing = GetPickLineQuantityFromComponents();
							var qtyThatCanBePicked = Math.Min(quantity, Math.Min(quantityAvailable, quantityNotPicked));
							pickLine.WZ_Units += qtyThatCanBePicked;

							AllocateSerialNumberIfNeeded(pickLine, qtyThatCanBePicked, specifiedSNPAssigner);

							if (!AvailableInventory.OrderedInventory.IsComponentOrderedInventoryOnSalesOrder)
							{
								ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, pickLine, RoundQtyToPickLineScale(qtyThatCanBePicked));
							}
							else
							{
								NotifyChangeForPickByBOM(pickableDocketLine.ParentLine, pickLine, pickedByBOMPriorToIncrementing);
							}

							if ((quantity -= qtyThatCanBePicked) == 0m)
							{
								break;
							}
						}
					}
				}
			}

			return quantity;
		}

		void AllocateSerialNumberIfNeeded(WhsPickLine pickLine, decimal qtyThatCanBePicked, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			if (specifiedSNPAssigner != null)
			{
				specifiedSNPAssigner.SelectSerialNumber(pickLine.PK);
			}
			else
			{
				if (AvailableInventory.HasSerialNumber)
				{
					foreach (var serialNumberPivotSelector in GetAvailableSerialNumbers(pickLine.WZ_WE_InventoryLine)
						.Take((int)qtyThatCanBePicked))
					{
						serialNumberPivotSelector.SelectSerialNumber(pickLine.PK);
					}

					IEnumerable<WhsSerialNumberPivotSelector> GetAvailableSerialNumbers(ZGuid inventoryPK)
						=> AvailableInventory.SerialNumbers.GetSerialsByInventory(inventoryPK).Where(s => !s.Selected);
				}
			}
		}

		void NotifyChangeForPickByBOM(WhsPickableDocketLine parentLine, WhsPickLine pickLine, ZDecimal pickedByBOMPriorToChanging)
		{
			if (parentLine != null)
			{
				var pickedByBOMDelta = GetPickLineQuantityFromComponents() - pickedByBOMPriorToChanging;
				ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, parentLine, AttributeParts.Empty, RoundQtyToPickLineScale(pickedByBOMDelta));
			}
		}

		ZDecimal GetPickLineQuantityFromComponents()
		{
			var orderedInv = AvailableInventory.OrderedInventory;
			return orderedInv.IsComponentOrderedInventoryOnSalesOrder ? orderedInv.Owners.Select(o => o.ParentLine).Distinct().Sum(p => p.TotalPickLineQuantityFromComponents) : 0m;
		}

		WhsPickLine[] GetPickLinesForIncrementOrDecrement(WhsPickableDocketLine specifiedDocketLine, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var relevantPickLines = (specifiedDocketLine == null ? AvailableInventory.PickLines : specifiedDocketLine.PickLines.Where(AvailableInventory.IsPickLineForInventory))
				.Where(pl => specifiedSNPAssigner == null || pl.WZ_WE_InventoryLine.Equals(specifiedSNPAssigner.InventoryPK));
			return relevantPickLines.ToArray();
		}

		#endregion

		#region CreateNewPickLines

		ZDecimal CreateNewPickLines(ZDecimal quantity, WhsPickableDocketLine specifiedDocketLine, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var owners = specifiedDocketLine == null ? AvailableInventory.OrderedInventory.Owners.Cast<WhsPickableDocketLine>() : new[] { specifiedDocketLine };
			var validOwners = owners.Where(l => !(l.Docket as WhsOrder)?.IsLoadingOrLoadedOrDeparted ?? true);

			foreach (var line in validOwners)
			{
				var quantityNotPicked = line.QuantityNotPicked;
				if (quantityNotPicked > 0m)
				{
					quantity -= CreateNewPickLines(line, Math.Min(quantity, quantityNotPicked), specifiedSNPAssigner);

					if (quantity == 0m)
					{
						break;
					}
				}
			}

			return quantity;
		}

		ZDecimal CreateNewPickLines(WhsPickableDocketLine line, ZDecimal quantity, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;
			ZDecimal result = 0m;

			foreach (var inventory in AvailableInventory.Inventory.Where(i => specifiedSNPAssigner == null || i.PK.Equals(specifiedSNPAssigner.InventoryPK)))
			{
				var availableToPickQty = isRegistryEnableHeldGoodsForOrders && inventory.IsHeld
					? inventory.WI_AvailableToTransferQuantity
					: inventory.WI_AvailableToPickQuantity;

				if (availableToPickQty > 0m)
				{
					var qtyThatCanBePicked = Math.Min(quantity, availableToPickQty);
					AddNewPickLine(line, inventory, qtyThatCanBePicked, specifiedSNPAssigner);

					result += qtyThatCanBePicked;
					if ((quantity -= qtyThatCanBePicked) == 0m)
					{
						break;
					}
				}
			}

			return result;
		}

		#region AddNewPickLine

		WhsPickLine AddNewPickLine(WhsPickableDocketLine line, WhsInventoryView inventory, ZDecimal quantity, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var pickedByBOMPriorToIncrementing = GetPickLineQuantityFromComponents();
			var pickLine = AvailableInventory.Factory.New<WhsPickLine>();

			using (((IActiveBusinessObjectCollection)AvailableInventory.OrderedInventory.PickLines).SuspendListChanged())
			using (pickLine.GetValidationSuspender())
			{
				pickLine.WZ_Units = quantity;
				pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;

				// This needs to be done last as the CollectionCountChanged event needs to fire when the PickLine has the correct quantity and Inventory set.
				pickLine.WZ_WE_TransactionLine = line.PK;

				AllocateSerialNumberIfNeeded(pickLine, quantity, specifiedSNPAssigner);
			}

			if (AvailableInventory.OrderedInventory.IsComponentOrderedInventoryOnSalesOrder)
			{
				((IActiveBusinessObjectCollection)line.PickLines).Refresh();
				NotifyChangeForPickByBOM(line.ParentLine, pickLine, pickedByBOMPriorToIncrementing);
			}
			else
			{
				ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, pickLine, RoundQtyToPickLineScale(quantity));
			}

			return pickLine;
		}

		#endregion

		#endregion

		#endregion

		#region DecrementPickingSlip

		ZDecimal DecrementPickingSlip(ZDecimal quantityToDeallocate, WhsPickableDocketLine specifiedDocketLine, ISerialNumberPivotAssigner specifiedSNPAssigner)
		{
			var pickLines = GetPickLinesForIncrementOrDecrement(specifiedDocketLine, specifiedSNPAssigner);

			for (var i = pickLines.Length - 1; i >= 0; i--)
			{
				var pickLine = pickLines[i];
				var absQuantityToDeallocate = Math.Abs(quantityToDeallocate);

				if ((AvailableInventory.allowQuantityReduction || !pickLine.WZ_IsPicking) && (!pickLine.HasReleaseCapturedAttribs || pickLine.WZ_Units <= absQuantityToDeallocate))
				{
					var qtyToReduce = Math.Min(absQuantityToDeallocate, pickLine.WZ_Units);

					var orderLine = (WhsPickableDocketLine)pickLine.DocketLine;

					if (IsPickLineUnpacked(AvailableInventory.OrderedInventory, pickLine, qtyToReduce))
					{
						if (pickLine.IsPicked && !pickLine.IsFinalised)
						{
							pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty; // if we're deallocating stock, we should unpick it if the PickLine is Picked in Memory.
						}

						if (!pickLine.IsPickedFromPutawayLocation)
						{
							var pickedByBOMPriorToDecrementing = GetPickLineQuantityFromComponents();

							pickLine.WZ_Units -= qtyToReduce;

							if (!AvailableInventory.OrderedInventory.IsComponentOrderedInventoryOnSalesOrder)
							{
								ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, pickLine, RoundQtyToPickLineScale(-qtyToReduce));
							}
							else
							{
								NotifyChangeForPickByBOM(orderLine.ParentLine, pickLine, pickedByBOMPriorToDecrementing);
							}

							specifiedSNPAssigner?.RemoveSerialNumberSelection();

							RemovePickLine(pickLine);

							if ((quantityToDeallocate += qtyToReduce) == 0m)
							{
								break;
							}
						}
					}
				}
			}

			return quantityToDeallocate;
		}

		static bool IsPickLineUnpacked(WhsPickOrderedInventory orderedInventory, WhsPickLine pickLine, ZDecimal qtyToReduce)
		{
			return (orderedInventory.IsPickLinePackedCheckSuspended && qtyToReduce == pickLine.WZ_Units) // if checking is suspended treat the pickline as unpacked as long as we are removing the whole pick line.
				|| pickLine.IsUnpacked(orderedInventory.Factory);
		}

		void RemovePickLine(WhsPickLine pickLine)
		{
			if (pickLine.WZ_Units == 0m)
			{
				AvailableInventory.SerialNumbers
					.Cast<ISerialNumberPivotAssigner>()
					.Where(s => s.PickingLinePK.Equals(pickLine.PK))
					.ForEach(s => s.RemoveSerialNumberSelection());

				if (pickLine.IsReserveLine)
				{
					pickLine.ClearOutPickingValuesForReservedLine();
				}
				else
				{
					AvailableInventory.OrderedInventory.PickLines.Delete(pickLine);
				}
			}
		}

		#endregion

		#endregion

		#region RoundPickLineDecimalScale

		ZDecimal RoundQtyToPickLineScale(ZDecimal qty)
		{
			int scale = WhsPickLineSchema.WZ_Units.Scale;
			return qty.Round(scale);
		}

		#endregion

		#region SplitByPackType

		internal void SplitByPackType()
		{
			var groupings = AvailableInventory
				.PickLines
				.Where(pl => !pl.IsPickedFromPutawayLocation)
				.GroupBy(pickLine => pickLine.DocketLine.WE_WD, pickLine => pickLine);

			foreach (var grouping in groupings)
			{
				var pickLines = grouping.ToArray();

				foreach (var pickLine in pickLines)
				{
					if (!pickLine.WZ_IsPicking)
					{
						pickLine.WZ_GS_NKAssignedTo = ZString.Empty;
					}
				}

				PickLinePackAssigner.AssignPackTypes(pickLines, AvailableInventory.OrderedInventory.Pick?.ForceWhsPickUOMTypeAllocation ?? ForceWhsPickUOMTypeAllocation.None);
			}
		}

		#endregion
	}

	#endregion
}
