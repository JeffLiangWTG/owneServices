using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed partial class WhsPickOrderedInventory : NonPersistentBusinessObject, ILineAttributes, ILineCustomAttributes, IWhsPickOrderedInventoryInternals
	{
		#region Schema

		public abstract class Schema
		{
			public const string QuantityOrdered = nameof(QuantityOrdered);
			public const string PickQuantity = nameof(PickQuantity);
			public const string PickLineQuantity = nameof(PickLineQuantity);
			public const string QuantityCrossDocked = nameof(QuantityCrossDocked);
			public const string QuantityShort = nameof(QuantityShort);
			public const string ExpiryDate = nameof(ExpiryDate);
			public const string PackingDate = nameof(PackingDate);
			public const string PartAttrib1 = nameof(PartAttrib1);
			public const string PartAttrib2 = nameof(PartAttrib2);
			public const string PartAttrib3 = nameof(PartAttrib3);
			public const string SerialNumber = nameof(SerialNumber);
			public const string OrderedHeldCode = nameof(OrderedHeldCode);
			public const string PackageGroupId = nameof(PackageGroupId);
			public const string BondedEntryKey = nameof(BondedEntryKey);
			public const string AllocationKey = nameof(AllocationKey);
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
			public const string ClientCode = nameof(ClientCode);
			public const string ProductCode = nameof(ProductCode);
			public const string ProductDesc = nameof(ProductDesc);
			public const string CommodityCode = nameof(CommodityCode);
			public const string PalletIDOrdered = nameof(PalletIDOrdered);
		}

		#endregion

		#region Constructors

		public WhsPickOrderedInventory(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region ClearPickLinesCache

		public void ClearPickLinesCache()
		{
			if (availableInventories != null)
			{
				foreach (WhsPickAvailableInventory availableInventory in AvailableInventories)
				{
					availableInventory.ClearPickLinesCache();
				}
			}
		}

		internal void ClearPickLinesCacheForInventory(ZGuid inventoryLinePK)
		{
			if (availableInventories != null)
			{
				availableInventories.GetAvailableInventoryFromInventoryPK(inventoryLinePK).ClearPickLinesCache();
			}
		}

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			base.Delete();

			AvailableInventories.RemoveAndDeleteAll();
		}

		#endregion

		#region Related Entities

		#region AvailableInventories

		public WhsPickAvailableInventoryCollection AvailableInventories
		{
			get
			{
				if (availableInventories == null)
				{
					availableInventories = GetAvailableInventoriesCore(availInvs => availInvs.LoadForOrderedInventory(this));
					RegisterEditableChildObject(availableInventories);

					if (!AvailableInventoryValidationSuspendedSemaphore.IsSuspended && availableInventories.Count < MaximumNumberOfLinesForWhichValidationOnGridStillShouldBeRun)
					{
						Validation.ValidatePickLineQuantity();
						Validation.ValidateQuantityShort();
					}
				}
				return availableInventories;
			}
		}

		public WhsPickAvailableInventory[] AvailableInventoriesWithAllocationLogs
		{
			get
			{
				return availableInventories?.Cast<WhsPickAvailableInventory>().Where(inv => !inv.AllocationLog.IsEmpty).ToArray() ?? [];
			}
		}

		WhsPickAvailableInventoryCollection availableInventories;

		const int MaximumNumberOfLinesForWhichValidationOnGridStillShouldBeRun = 1000;

		public WhsPickAvailableInventoryCollection GetAvailableInventoriesForPalletIDs(IEnumerable<string> palletIDs)
		{
			Argument.NotNull(palletIDs, nameof(palletIDs));
			return GetAvailableInventoriesCore(availInvs => availInvs.LoadSpecificPalletIDsForOrderedInventory(this, palletIDs));
		}

		WhsPickAvailableInventoryCollection GetAvailableInventoriesCore(Action<WhsPickAvailableInventoryCollection> loadForOrderedInventory)
		{
			var availableInventories = new WhsPickAvailableInventoryCollection(Factory);

			using (availableInventories.SuspendListChanged())
			{
				loadForOrderedInventory(availableInventories);
			}

			availableInventories.Sort(new SortPickInventoryForPicking());

			return availableInventories;
		}

		#endregion

		#region AvailableInventoryValidationSuspendedSemaphore

		public Semaphore AvailableInventoryValidationSuspendedSemaphore
		{
			get { return availableInventoryValidationSuspendedSemaphore ?? (availableInventoryValidationSuspendedSemaphore = new Semaphore()); }
		}

		Semaphore availableInventoryValidationSuspendedSemaphore;

		#endregion

		#region ClearAvailableInventoriesCache

		public void ClearAvailableInventoriesCache()
		{
			if (availableInventories != null)
			{
				UnRegisterEditableChildObject(availableInventories);

				foreach (WhsPickAvailableInventory availableInventory in availableInventories)
				{
					availableInventory.Deactivate();
				}

				availableInventories = null;
			}
		}

		#endregion

		#region Client

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(ClientPK); }
		}

		public ZGuid ClientPK => Owners.FirstOrDefault()?.Docket?.WD_OH_Client ?? ZGuid.Empty;

		#endregion

		#region Owners

		public WhsPickableDocketLineCollectionAdHoc Owners => owners ?? (owners = new WhsPickableDocketLineCollectionAdHoc(Factory));
		WhsPickableDocketLineCollectionAdHoc owners;

		#endregion

		#region Pick

		public WhsPick Pick
		{
			get;
			private set;
		}

		#endregion

		#region Product

		public WhsProduct Product
		{
			get { return product ?? (product = WhsProduct.GetWhsProduct(Factory, SupplierPartPK)); }
		}

		WhsProduct product;

		#endregion

		#region SupplierPart

		public OrgSupplierPart SupplierPart
		{
			get { return Factory.Load<OrgSupplierPart>(SupplierPartPK); }
		}

		public ZGuid SupplierPartPK => (ZGuid)(supplierPartPK ?? (supplierPartPK = Owners.FirstOrDefault()?.WE_OP ?? ZGuid.Empty));

		ZGuid? supplierPartPK;

		#endregion

		public WhsPickLineCollection PickLines
		{
			get { return pickLines ?? (pickLines = new WhsPickLineCollection(this)); }
		}

		WhsPickLineCollection pickLines;

		#endregion

		#region Validation

		public WhsPickOrderedInventoryValidation Validation
		{
			get { return GetNewValidation(); }
		}

		WhsPickOrderedInventoryValidation GetNewValidation()
		{
			return new WhsPickOrderedInventoryValidation(this);
		}

		#endregion

		#region Properties

		#region GetClientPKFast

		public ZGuid GetClientPKFast(WhsPickableDocketLine firstOwner)
		{
			if (firstOwner != null)
			{
				foreach (WhsPickableDocket order in Pick.Orders)
				{
					if (order.PK == firstOwner.WE_WD) // comparing Guids is much faster than comparing to Docket as there is no Factory.Load() (significant when doing 1000's of iterations when picking)
					{
						return order.WD_OH_Client;
					}
				}
			}

			return ZGuid.Empty;
		}

		#endregion

		#region Client Code

		public ZString ClientCode
		{
			get { return (Client != null ? Client.OH_Code : ZString.Empty); }
		}

		public ZPropertyInfo ClientCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ClientCode); }
		}

		#endregion

		#region Product Code

		public ZString ProductCode
		{
			get { return (SupplierPart != null ? SupplierPart.OP_PartNum : ZString.Empty); }
		}

		public ZPropertyInfo ProductCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProductCode); }
		}

		#endregion

		#region Product Desc

		public ZString ProductDesc
		{
			get { return (SupplierPart != null ? SupplierPart.OP_Desc : ZString.Empty); }
		}

		public ZPropertyInfo ProductDescInfo
		{
			get { return GetZPropertyInfo(Schema.ProductDesc); }
		}

		#endregion

		#region CommodityCode

		public ZString CommodityCode
		{
			get { return (SupplierPart != null ? SupplierPart.OP_RH_NKCommodityCode : ZString.Empty); }
		}

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CommodityCode); }
		}

		#endregion

		#region PackageGroupId

		public ZString PackageGroupId
		{
			get { return Owners.Count > 0 ? Owners[0].WE_PackageGroupId : ZString.Empty; }
		}

		public ZPropertyInfo PackageGroupIdInfo
		{
			get { return GetZPropertyInfo(Schema.PackageGroupId); }
		}

		#endregion

		#region Quantity Ordered

		public ZDecimal QuantityOrdered
		{
			get
			{
				ZDecimal result = 0m;
				foreach (var line in Owners)
				{
					result += line.WE_TransactionQuantity;
				}
				return result;
			}
		}

		public ZPropertyInfo QuantityOrderedInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityOrdered); }
		}

		#endregion

		#region PickQuantity

		[ResourceStringData("WhsPickOrderedInventory|2eb53490-3edd-4bc8-bf78-34bbf0417498", Caption = "Picked Quantity", ShortCaption = "Picked Qty")]
		public ZDecimal PickQuantity
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var docketLine in Owners)
				{
					result += docketLine.PickLines.Where(pickLine => pickLine.IsPickedFromPutawayLocation).Sum(pl => pl.WZ_Units);
				}
				return result;
			}
		}

		public ZPropertyInfo PickQuantityInfo => GetZPropertyInfo(Schema.PickQuantity);

		#endregion

		#region PickLineQuantity

		[ResourceStringData("WhsPickOrderedInventory|254b1049-97e1-4b07-a548-32609a935a33", Caption = "Allocated Quantity", ShortCaption = "Allocated Qty")]
		public ZDecimal PickLineQuantity => Owners.Sum(o => o.PickLineQuantity);

		public ZPropertyInfo PickLineQuantityInfo => GetZPropertyInfo(Schema.PickLineQuantity);

		#endregion

		#region Quantity CrossDocked

		public ZDecimal QuantityCrossDocked
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsPickAvailableInventory availableInventory in AvailableInventories)
				{
					result += availableInventory.QuantityCrossDocked;
				}
				return result;
			}
		}

		public ZPropertyInfo QuantityCrossDockedInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityCrossDocked); }
		}

		#endregion

		#region Quantity Short

		public ZDecimal QuantityShort
		{
			get { return Math.Max(0m, QuantityOrdered - PickLineQuantity); }
		}

		public ZPropertyInfo QuantityShortInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityShort); }
		}

		public ZString ValidationQuantityShortWarningMessage
		{
			get { return validationQuantityShortWarningMessage; }
			set
			{
				if (value != validationQuantityShortWarningMessage)
				{
					validationQuantityShortWarningMessage = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateQuantityShort();
					}
				}
			}
		}
		ZString validationQuantityShortWarningMessage = "";

		#endregion

		#region WI_UnitsUQ

		public ZString UnitsUQ
		{
			get { return SupplierPart?.OP_StockKeepingUnit ?? Constants.PkgUnit.Unit; }
		}

		public ZPropertyInfo UnitsUQInfo
		{
			get { return GetZPropertyInfo(nameof(UnitsUQ)); }
		}

		#endregion

		#region OnlyShowAvailableStock

		public bool OnlyShowAvailableStock
		{
			get { return onlyShowAvailableStock; }
			set { onlyShowAvailableStock = value; }
		}
		bool onlyShowAvailableStock;

		#endregion

		#region MinimumShelfLife

		[ResourceStringData("WhsPickOrderedInventory|MinimumShelfLife", Caption = "Minimum Shelf Life", ShortCaption = "Min. Shelf Life")]
		public ZShort MinimumShelfLife
		{
			get { return (Owners.Count > 0) ? Owners[0].MinimumShelfLife : ZShort.Zero; }
		}

		#endregion

		#region PalletIDOrdered

		[ResourceStringData("WhsPickOrderedInventory|PalletIDOrdered", Caption = "Ordered Pallet ID", ShortCaption = "Ordered Plt. ID")]
		public ZString PalletIDOrdered
		{
			get { return (Owners.Count > 0) ? Owners[0].WE_PalletID : ZString.Empty; }
		}

		public ZPropertyInfo PalletIDOrderedInfo
		{
			get { return GetZPropertyInfo(Schema.PalletIDOrdered); }
		}
		#endregion

		#region IsBOMProductPickedOnSalesOrder

		public bool IsBOMProductPickedOnSalesOrder
		{
			get { return Owners.FirstOrDefault()?.IsBOMProductPickedOnSalesOrder ?? false; }
		}

		#endregion

		#region IsComponentOrderedInventoryOnSalesOrder

		public ZBool IsComponentOrderedInventoryOnSalesOrder
		{
			get { return Owners.FirstOrDefault()?.IsComponentLineOnSalesOrder ?? false; }
		}

		#endregion

		#region IsCustomsTransaction

		public bool IsCustomsTransaction => Owners.FirstOrDefault()?.IsCustomsTransaction ?? false;

		#endregion

		#region IsInwardProcessingJob

		public bool IsInwardProcessingJob => Owners.FirstOrDefault()?.Docket?.WD_IsInwardsProcessingJob ?? false;

		#endregion

		#region IsShortfall

		bool IsShortfall
		{
			get { return QuantityShort > 0m; }
		}

		#endregion

		#endregion

		#region Picking

		#region IsWaitingReplenishment

		internal bool IsWaitingReplenishment()
		{
			if (IsShortfall && OrderedHeldCode.IsEmpty)
			{
				var clientPk = ClientPK;
				var supplierPartPk = SupplierPartPK;
				var warehousePk = Pick.WP_WW_Whs;
				var whsProduct = Product;

				if (whsProduct != null)
				{
					var locationsToExclude = new HashSet<WhsLocation>();
					var paramsByWhsAndClient = whsProduct.GetParamsByWhsAndClient(warehousePk, clientPk);
					var pickDynamicAreaOverride = Pick.DynamicPickAreaOverride;

					var allPickFaceLocations = whsProduct.PickFaces.FindAllPickFaceLocations(clientPk, supplierPartPk, warehousePk);
					locationsToExclude.UnionWith(allPickFaceLocations);

					var dynamicPickFaceArea = paramsByWhsAndClient?.DynamicPickFaceArea;
					if (dynamicPickFaceArea != null)
					{
						var dynamicLocations = dynamicPickFaceArea.PickLocations.Where(l => l.WLV_LocationClass == LocationClasses.Codes.DPF);
						locationsToExclude.UnionWith(dynamicLocations);
					}

					if (pickDynamicAreaOverride != null)
					{
						locationsToExclude.UnionWith(pickDynamicAreaOverride.PickLocations);
					}

					return locationsToExclude.Count > 0 && AvailableInventories.GetTotalAvailableToPickUnits(locationsToExclude) > 0;
				}
			}
			return false;
		}

		#endregion

		#region ClearAllocations

		public void ClearAllocations()
		{
			foreach (var owner in Owners)
			{
				owner.ClearReleaseLines();
			}

			var iBusinessOwners = (IBusiness)Owners;
			iBusinessOwners.SuspendValidation();

			try
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory)) // for performance
				using (AvailableInventories.SuspendListChanged())
				using (SuspendCheckIsPickLinePacked()) // Allow Deletion of Packed Pick Lines when Clearing Allocations
				{
					foreach (WhsPickAvailableInventory availableInventory in AvailableInventories)
					{
						using (availableInventory.GetValidationSuspender()) // no need to validate anything when we're clearing Allocations
						{
							availableInventory.PickLineQuantity = 0m;
						}
					}
				}
			}
			finally
			{
				iBusinessOwners.ResumeValidation();
			}

			ValidateWE_ShortfallQuantityCached_ForAllOwners();
		}

		internal bool IsPickLinePackedCheckSuspended => CheckIsPickLinePackedSemaphore.IsSuspended;

		IDisposable SuspendCheckIsPickLinePacked() => new SemaphoreManager(CheckIsPickLinePackedSemaphore);

		void ValidateWE_ShortfallQuantityCached_ForAllOwners()
		{
			foreach (var owner in Owners)
			{
				owner.Validation.ValidateWE_ShortfallQuantityCached();
			}
		}

		Semaphore CheckIsPickLinePackedSemaphore => checkIsPickLinePackedSemaphore ?? (checkIsPickLinePackedSemaphore = new Semaphore());
		Semaphore checkIsPickLinePackedSemaphore;

		#endregion

		#endregion

		#region AdjustDownPickAndAttributeMetLinesToMatchOrderQty

		/// <summary>
		/// 
		/// This method will deallocate all stock then reallocate it, this is used when the user has, *after picking*,
		/// reduced the quantities ordered.
		/// 
		/// A better method is to simply deallocate only what we need, but to do that we cannot use pickLineAvailable.PickLineQuantity
		/// because we cannot control the exact line it deallocates from. For example, if we have the following OrderLines:
		/// 
		///       Ordered  Picked
		/// Part1   10       10
		/// Part1    5        5
		/// 
		/// ..and the user then changes the qtys to:
		/// 
		///       Ordered  Picked
		/// Part1  [7]       10
		/// Part1   5         5
		/// 
		/// Setting pickLineAvailable.PickLineQuantity will 'unpick' the first line it finds, which could result in:
		/// 
		///       Ordered  Picked
		/// Part1   7        10
		/// Part1   5        [2]
		/// 
		/// </summary>                         
		void IWhsPickOrderedInventoryInternals.AdjustDownPickAndAttributeMetLinesToMatchOrderQty()
		{
			if (IsAnyLineOverAllocated)
			{
				var dictionary = new Dictionary<WhsPickAvailableInventory, decimal>();
				var quantityStillToPick = QuantityOrdered;
				// save list of current allocations and deallocate all stock.
				foreach (WhsPickAvailableInventory availableInventory in AvailableInventories)
				{
					ZDecimal originalPickLineQuantity = availableInventory.PickLineQuantity;
					if (originalPickLineQuantity > 0m)
					{
						availableInventory.PickLineQuantity = 0m;
						var quantityDeallocated = originalPickLineQuantity - availableInventory.PickLineQuantity;
						quantityStillToPick -= availableInventory.PickLineQuantity;
						dictionary.Add(availableInventory, quantityDeallocated);
					}
				}

				// reallocate saved stock to other lines that have the same product, but do not ever go over the original qty ordered.
				foreach (WhsPickAvailableInventory availableInventory in dictionary.Keys)
				{
					var previousQuantityDeallocated = dictionary[availableInventory];
					var min = Math.Min(previousQuantityDeallocated, quantityStillToPick);

					availableInventory.PickLineQuantity += min;
					quantityStillToPick -= availableInventory.PickLineQuantity;
					if (quantityStillToPick <= 0m)
					{
						break;
					}
				}
			}
		}

		bool IsAnyLineOverAllocated
		{
			get
			{
				foreach (WhsPickableDocketLine line in Owners)
				{
					if (line.PickLineQuantity > line.WE_TransactionQuantity || line.SumOfUnitsMet > line.WE_TransactionQuantity)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region ILineAttributes Members

		public ZDate ExpiryDate => Owners.Count > 0 ? Owners[0].WE_ExpiryDate : ZDate.Empty;

		public ZDate PackingDate => Owners.Count > 0 ? Owners[0].WE_PackingDate : ZDate.Empty;

		public ZString BondedEntryKey => Owners.Count > 0 ? Owners[0].WE_BondedEntryKey : ZString.Empty;

		public ZString PartAttrib1 => Owners.Count > 0 ? Owners[0].WE_PartAttrib1 : ZString.Empty;

		public ZString PartAttrib2 => Owners.Count > 0 ? Owners[0].WE_PartAttrib2 : ZString.Empty;

		public ZString PartAttrib3 => Owners.Count > 0 ? Owners[0].WE_PartAttrib3 : ZString.Empty;

		[ResourceStringData("WhsPickOrderedInventory|SerialNumber", Caption = "Serial Number", ShortCaption = "Serial #")]
		public ZString SerialNumber => Owners.Count > 0 ? Owners[0].WE_SerialNumber : ZString.Empty;

		[ResourceStringData("WhsPickOrderedInventory|AllocationKey", Caption = "Allocation Key")]
		public ZString AllocationKey => Owners.Count > 0 ? Owners[0].WE_AllocationKey : ZString.Empty;

		[ResourceStringData("WhsPickOrderedInventory|OrderedHeldCode", Caption = "Hold Code")]
		public ZString OrderedHeldCode => Owners.Count > 0 ? Owners[0].WE_WHC_NKOrderedHeldCode : ZString.Empty;

		public void SetAttributes(ILineAttributes src) => throw new NotSupportedException();

		public ZPropertyInfo ExpiryDateInfo => GetZPropertyInfo(Schema.ExpiryDate);

		public ZPropertyInfo PackingDateInfo => GetZPropertyInfo(Schema.PackingDate);

		public ZPropertyInfo BondedEntryKeyInfo => GetZPropertyInfo(Schema.BondedEntryKey);

		public ZPropertyInfo PartAttrib1Info => GetZPropertyInfo(Schema.PartAttrib1);

		public ZPropertyInfo PartAttrib2Info => GetZPropertyInfo(Schema.PartAttrib2);

		public ZPropertyInfo PartAttrib3Info => GetZPropertyInfo(Schema.PartAttrib3);

		public ZPropertyInfo SerialNumberInfo => GetZPropertyInfo(Schema.SerialNumber);

		public ZPropertyInfo AllocationKeyInfo => GetZPropertyInfo(Schema.AllocationKey);

		public ZPropertyInfo OrderedHeldCodeInfo => GetZPropertyInfo(Schema.OrderedHeldCode);

		#endregion

		#region ILineCustomAttributes Members

		public ZString CustomAttrib1
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib1 : ZString.Empty; }
		}

		public ZString CustomAttrib2
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib2 : ZString.Empty; }
		}

		public ZString CustomAttrib3
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib3 : ZString.Empty; }
		}

		public ZString CustomAttrib4
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib4 : ZString.Empty; }
		}

		public ZString CustomAttrib5
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib5 : ZString.Empty; }
		}

		public ZString CustomAttrib6
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomAttrib6 : ZString.Empty; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDecimal1 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDecimal2 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDecimal3 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDecimal4 : ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDecimal5 : ZDecimal.Zero; }
		}

		public ZDateTime CustomDate1
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDate1 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate2
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDate2 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate3
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDate3 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate4
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDate4 : ZDateTime.Empty; }
		}

		public ZDateTime CustomDate5
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomDate5 : ZDateTime.Empty; }
		}

		public ZBool CustomFlag1
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomFlag1 : ZBool.False; }
		}

		public ZBool CustomFlag2
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomFlag2 : ZBool.False; }
		}

		public ZBool CustomFlag3
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomFlag3 : ZBool.False; }
		}

		public ZBool CustomFlag4
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomFlag4 : ZBool.False; }
		}

		public ZBool CustomFlag5
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomFlag5 : ZBool.False; }
		}

		public ZString CustomTextBlob1
		{
			get { return Owners.Count > 0 ? Owners[0].WE_CustomTextBlob1 : ZString.Empty; }
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

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				ConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider { get; }

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(WhsPickOrderedInventory), configOrg, ResString.GetMultilingualString("62b98fde-6dc1-4092-905a-e9624b4d9960", "the client"), factory);

				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute1, Schema.CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute2, Schema.CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute3, Schema.CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute4, Schema.CustomAttrib4, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute5, Schema.CustomAttrib5, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute6, Schema.CustomAttrib6, Constants.CustomLabels.Descriptions.CustomAttribute(6));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate1, Schema.CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate2, Schema.CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate3, Schema.CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate4, Schema.CustomDate4, Constants.CustomLabels.Descriptions.CustomDate(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate5, Schema.CustomDate5, Constants.CustomLabels.Descriptions.CustomDate(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal1, Schema.CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal2, Schema.CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal3, Schema.CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal4, Schema.CustomDecimal4, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal5, Schema.CustomDecimal5, Constants.CustomLabels.Descriptions.CustomNumber(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag1, Schema.CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag2, Schema.CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag3, Schema.CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag4, Schema.CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag5, Schema.CustomFlag5, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, Schema.CustomTextBlob1, Constants.CustomLabels.Descriptions.CustomText(1));

				return result;
			}
		}

		#endregion

		#region IWhsPickOrderedInventoryInternals Members

		void IWhsPickOrderedInventoryInternals.SetAllProperties(WhsPick pick, WhsPickableDocketLine line)
		{
			Pick = pick;
			availableInventories = null;

			if (line != null)
			{
				product = null;
				supplierPartPK = null;

				if (!Owners.Contains(line))
				{
					Owners.Add(line);
				}
			}
		}

		#endregion
	}

	#region IWhsPickOrderedInventoryInternals

	internal interface IWhsPickOrderedInventoryInternals
	{
		void SetAllProperties(WhsPick pick, WhsPickableDocketLine line);
		void AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
	}

	#endregion
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed partial class WhsPickOrderedInventory
	{
		internal void SetAvaliableInventoryForTest(WhsPickAvailableInventory inventory) => SetAvaliableInventory(inventory);

		void SetAvaliableInventory(WhsPickAvailableInventory inventory)
		{
			availableInventories = new WhsPickAvailableInventoryCollection(Factory);
			availableInventories.Add(inventory);
		}
	}
}

#endif
#endregion
