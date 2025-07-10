using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsPickableDocketLineTypeDecider

	public class WhsPickableDocketLineTypeDecider : WhsDocketLineTypeDecider
	{
		// refer to the base class for the Load TypeDecider

		public override Type GetTypeForNew()
		{
			return typeof(WhsPickableDocketLine);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(WhsPickableDocketLine);
		}
	}

	#endregion

	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy))]
	public abstract class WhsPickableDocketLine : WhsDocketLine, IWhsPickableDocketLine
	{
		protected WhsPickableDocketLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly WhsPickableDocketLineTypeDecider TypeDecider = new WhsPickableDocketLineTypeDecider();

		#region Schema

		public new abstract class Schema : WhsDocketLine.Schema
		{
			public const string WE_CrossDockQuantity = "WE_CrossDockQuantity";
			public const string WE_ShortfallQuantityCached = "WE_ShortfallQuantityCached";
			public const string SumOfUnitsMet = "SumOfUnitsMet";
			public const string WhsOrderLineToBeClearedLater = "WhsOrderLineToBeClearedLater";
			public const string CustomsClearingInProgress = "CustomsClearingInProgress";
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted) // because of tracking
			{
				ReservedPickLines.DeleteAll();

				// Unhook Release Lines collection on Delete.
				ClearReleaseLines();

				// need to refresh shortfall lines with lower priority
				SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
			}

			base.Delete();
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			var propertesToExclude = new[]
			{
				WhsDocketLineSchema.Constants.WE_LineNo,
				WhsDocketLineSchema.Constants.WE_SubLineNo,
				WhsDocketLineSchema.Constants.WE_SystemCreateUser,
				WhsDocketLineSchema.Constants.WE_SystemLastEditUser,
				WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc,
				WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc
			};

			if (!IsInDatabase || ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.IsPersistent && !propertesToExclude.Contains(p.Name) && p.HasChanges))
			{
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(Factory, this);
			}
		}

		#endregion

		#region GetProcessHandlingInfo

		protected override ProcessHandlingInfo GetProcessHandlingInfo()
		{
			return new WhsPickableDocketLineProcessHandlingInfo(this);
		}

		#endregion

		#region ReadOnly

		protected override bool StandardReadOnly => IsDocketPicking || IsDocketFinalisedOrCancelled;

		protected override bool DestLocationReadOnly => StandardReadOnly;

		protected override bool DestPalletIDReadOnly => StandardReadOnly;

		#endregion

		#region Related Entities

		#region ChildComponentLines

		public IReadOnlyCollection<WhsPickableDocketLine> ChildComponentLines => ChildComponentLinesCore;

		protected virtual IReadOnlyCollection<WhsPickableDocketLine> ChildComponentLinesCore
			=> Factory.Load<WhsPickableDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

		#endregion

		#region InventoryFilter

		public WhsInventoryViewCollection InventoryFilter => InventoryFilterCore;

		protected virtual WhsInventoryViewCollection InventoryFilterCore => new WhsInventoryViewCollection(Factory);

		#endregion

		#region ParentLine

		public WhsPickableDocketLine ParentLine => Factory.Load<WhsPickableDocketLine>(WE_WE_ParentDocketLine);

		#endregion

		#region PickableDocket

		public WhsPickableDocket PickableDocket => (WhsPickableDocket)Docket;

		#endregion

		#region PickLinesWithNonZeroUnits

		public WhsPickLineCollection PickLinesWithNonZeroUnits
		{
			get
			{
				if (pickLinesWithNonZeroUnits == null)
				{
					// touching 'PickLines' will load appropriate fetch hints for Pick Lines.
					pickLinesWithNonZeroUnits = PickLines.GetLinesWithUnitsGreaterThanZero();
				}

				return pickLinesWithNonZeroUnits;
			}
		}

		WhsPickLineCollection pickLinesWithNonZeroUnits;

		#endregion

		#region PickLinesForRelease

		/// <summary>
		/// This is ONLY used for the GUI to show Component Order Line Pick Lines as well.
		/// </summary>
		public WhsPickLineCollection PickLinesForRelease
		{
			get { return pickLinesForRelease ?? (pickLinesForRelease = GetPickLinesForRelease()); }
		}

		protected abstract WhsPickLineCollection GetPickLinesForRelease();

		WhsPickLineCollection pickLinesForRelease;

		#endregion

		#region PickLines

		protected override WhsPickLineCollection GetPickLinesCollection()
		{
			var result = new WhsPickLineCollection(this);
			result.SetReadOnlyIncludingChildren(true);
			RegisterEditableChildObject(result);
			return result;
		}

		protected override void AddFetchHintsForDocketPickLines()
		{
			var pickableDocket = PickableDocket;
			if (pickableDocket != null)
			{
				pickableDocket.AddFetchHintsForPickLines();
			}
		}

		#endregion

		#region ReleaseLines

		// There is no ChildEditable Attribute because we don't want to load this collection
		// on ValidateAll(), this is just a Wrapping Collection for the GUI.
		[ChildEditableTestExclude]
		public WhsReleaseLineCollection ReleaseLines
		{
			get { return releaseLines ?? (releaseLines = WhsReleaseLineCollection.GetNewReleaseLinesCollection(this)); }
		}

		abstract internal class ReleaseLinesWrapper
		{
			protected static void SetReleaseLineField(WhsPickableDocketLine orderLine, WhsReleaseLineCollection releaseLines)
			{
				if (orderLine.releaseLines != null)
				{
					throw new InvalidOperationException("Should not be setting the ReleaseLines Field if it already has a value.");
				}

				orderLine.releaseLines = releaseLines;
			}
		}

		internal int BuildReleaseLines()
		{
			return ReleaseLines.Count;
		}

		public void ClearReleaseLines()
		{
			if (IsReleaseLineCollectionBuilt)
			{
				releaseLines.ClearCollection();
			}
		}

		WhsReleaseLineCollection releaseLines;

		internal bool IsReleaseLineCollectionBuilt
		{
			get { return releaseLines != null && releaseLines.IsLoaded; }
		}

		#endregion

		protected override void DeleteForDataRefresh()
		{
			if (!IsDeleted) // Deleting the same line on 2 different CW1 instances causes a problem
			{
				using (PickableDocket.SuspendPackableItemParentsCountChanged())
				{
					ClearReleaseLines();
					base.DeleteForDataRefresh();
				}
			}
		}

		#region ReservedPickLines

		protected override ReservedPickLineCollection GetReservedPickLinesCollection()
		{
			return new ReservedPickLineCollection(this);
		}

		protected override void LoadReservedLines(ReservedPickLineCollection pickLines)
		{
			AddFetchHintsForDocketPickLines();
		}

		#endregion

		#region StagingLocationBOM

		public WhsLocation StagingLocationBOM
		{
			get
			{
				WhsLocation result = null;

				var docket = Docket;
				var productParams = GetProductParams(docket);
				if (productParams != null)
				{
					result = docket.WD_IsInwardsProcessingJob ? productParams.InwardProcessingStagingLocationBOM : productParams.StagingLocationBOM;
				}

				return result;
			}
		}

		#endregion

		#region BOMComponentLinks

		protected override IEnumerable<WhsBOMInventoryPivot> BOMComponentLinksCore => Enumerable.Empty<WhsBOMInventoryPivot>();

		#endregion

		#endregion

		#region SupportsHoldCodeChange

		protected override bool SupportsHoldCodeChange => false;

		#endregion

		#region Properties

		#region WE_OP

		public override ZGuid WE_OP
		{
			get => base.WE_OP;
			set
			{
				var previousValue = base.WE_OP;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_OP = value;
				if (previousValue != WE_OP)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);

					if (IsReleaseLineCollectionBuilt)
					{
						ReleaseLines.ClearCollection();
						BuildReleaseLines();
					}

					if (WE_OP.IsValid)
					{
						DefaultPriceInfoFromProduct();
					}
				}
			}
		}

		void DefaultPriceInfoFromProduct()
		{
			if (AllowToDefaultPriceInfoFromProduct)
			{
				var order = Docket;
				var product = SupplierPart;
				if (order != null && product != null)
				{
					var orgPartRelation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(order.WD_OH_Client, OrgPartRelation.RelationshipTypes.Owner);
					if (orgPartRelation != null && !string.IsNullOrEmpty(orgPartRelation.OU_RX_NKUnitPriceCurrency) && orgPartRelation.OU_UnitPrice > 0)
					{
						WE_RecommendedUnitPrice = orgPartRelation.OU_UnitPrice;
						WE_UnitPriceAfterDiscount = orgPartRelation.OU_UnitPrice;
						WE_RX_NKUnitPriceCurrency = orgPartRelation.OU_RX_NKUnitPriceCurrency;
						WE_UnitDiscountAmount = 0m;
						WE_UnitDiscountPercent = 0m;
					}
				}
			}
		}

		bool AllowToDefaultPriceInfoFromProduct
			=> AllowToDefaultPriceInfoFromProductCore &&
			(!IsPopulatingFromUniversalDataObjectReader ||
			(WE_RecommendedUnitPrice == 0m &&
			WE_UnitPriceAfterDiscount == 0m &&
			WE_UnitDiscountAmount == 0m &&
			WE_UnitDiscountPercent == 0m &&
			string.IsNullOrEmpty(WE_RX_NKUnitPriceCurrency)));

		protected virtual bool AllowToDefaultPriceInfoFromProductCore => true;

		public IDisposable SetIsPopulatingFromUniversalDataObjectReader()
		{
			return new DisposableAction(
				() => IsPopulatingFromUniversalDataObjectReader = true,
				() => IsPopulatingFromUniversalDataObjectReader = false);
		}

		/// <summary>
		/// This should only be set in SetIsPopulatingFromUniversalDataObjectReader, used when populating order line properties in the UniversalDataObjectReader.
		/// </summary>
		bool IsPopulatingFromUniversalDataObjectReader { get; set; }

		protected abstract bool CanOrderPalletID { get; }

		#endregion

		#region WE_TransactionQuantity

		public override ZDecimal WE_TransactionQuantity
		{
			get => base.WE_TransactionQuantity;
			set
			{
				var previousValue = base.WE_TransactionQuantity;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_TransactionQuantity = value;
				if (previousValue != WE_TransactionQuantity)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);

					if (!IsPicked)
					{
						CalculateExtendedLinePrice();
					}
				}
			}
		}

		#endregion

		#region WE_UnitPriceAfterDiscount

		public override ZDecimal WE_UnitPriceAfterDiscount
		{
			get => base.WE_UnitPriceAfterDiscount;
			set
			{
				base.WE_UnitPriceAfterDiscount = value;

				CalculateExtendedLinePrice();
			}
		}

		#endregion

		#region WE_PartAttrib1

		public override ZString WE_PartAttrib1
		{
			get => base.WE_PartAttrib1;
			set
			{
				var previousValue = base.WE_PartAttrib1;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_PartAttrib1 = value;
				if (previousValue != WE_PartAttrib1)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_PartAttrib2

		public override ZString WE_PartAttrib2
		{
			get => base.WE_PartAttrib2;
			set
			{
				var previousValue = base.WE_PartAttrib2;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_PartAttrib2 = value;
				if (previousValue != WE_PartAttrib2)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_PartAttrib3

		public override ZString WE_PartAttrib3
		{
			get => base.WE_PartAttrib3;
			set
			{
				var previousValue = base.WE_PartAttrib3;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_PartAttrib3 = value;
				if (previousValue != WE_PartAttrib3)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_SerialNumber

		public override ZString WE_SerialNumber
		{
			get => base.WE_SerialNumber;
			set
			{
				var previousValue = base.WE_SerialNumber;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_SerialNumber = value;
				if (previousValue != WE_SerialNumber)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_PackingDate

		public override ZDate WE_PackingDate
		{
			get => base.WE_PackingDate;
			set
			{
				var previousValue = base.WE_PackingDate;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_PackingDate = value;
				if (previousValue != WE_PackingDate)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_ExpiryDate

		public override ZDate WE_ExpiryDate
		{
			get => base.WE_ExpiryDate;
			set
			{
				var previousValue = base.WE_ExpiryDate;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_ExpiryDate = value;
				if (previousValue != WE_ExpiryDate)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_BondedEntryKey

		public override ZString WE_BondedEntryKey
		{
			get => base.WE_BondedEntryKey;
			set
			{
				var previousValue = base.WE_BondedEntryKey;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_BondedEntryKey = value;
				if (previousValue != WE_BondedEntryKey)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region IsUpdateForOrderPickingStatusAllowed

		public bool IsUpdateForOrderPickingStatusAllowed() => IsUpdateForOrderPickingStatusAllowedCore();
		protected abstract bool IsUpdateForOrderPickingStatusAllowedCore();

		#endregion

		#region OrderedQtyExcludingReservedQty

		protected ZDecimal OrderedQtyExcludingReservedQty => WE_TransactionQuantity - AvailableReservedQty;

		ZDecimal AvailableReservedQty => AvailableReservedQtyCore;

		protected virtual ZDecimal AvailableReservedQtyCore => ReservedPickLines.Where(i => (i.InventoryLine?.IsAvailable ?? false)).Sum(i => i.WZ_Units);

		#endregion

		#region SumOfUnitsMet

		protected override ZDecimal SumOfUnitsMetCore
		{
			get
			{
				// If we have not loaded the Release Lines Collection, the Sum Of Units Met should just be how many PickLines are Allocated.
				// otherwise we get the sum directly from the Release Lines (in case the Non-Persistent BizOs are different as changed by the user).
				decimal result;
				if (IsComponentLineOnSalesOrder || !IsReleaseLineCollectionBuilt)
				{
					result = PickLineQuantity;
				}
				else
				{
					result = ReleaseLines.GetTotalUnitsMet();
				}
				return result;
			}
		}

		public ZPropertyInfo SumOfUnitsMetInfo => GetZPropertyInfo(Schema.SumOfUnitsMet, Res.GetString("235d19fb-df4b-46e2-a308-e48b8af5a834", "Sum Of Units Met"));

		#endregion

		#region Total Quantity Picked / Ordered From Components

		public ZDecimal TotalPickLineQuantityFromComponents => GetQuantityFromComponents(l => l.PickLineQuantity);

		// This is ordered quantity of master products based on ordered quantities for child lines
		public ZDecimal TotalQuantityOrderedFromComponents => GetQuantityFromComponents(l => l.WE_TransactionQuantity);

		public int GetQuantityFromComponents(Func<WhsPickableDocketLine, ZDecimal> getQuantity)
		{
			var quantity = 0;

			if (IsBOMProductPickedOnSalesOrder)
			{
				var part = SupplierPart;
				if (part != null)
				{
					var childComponentLines = ChildComponentLines;
					quantity = (childComponentLines.Count > 0) ? childComponentLines.Min(childLine => GetNumberOfPossibleKitsFromChildLine(childLine, part, getQuantity)) : 0;
				}
			}

			return quantity;
		}

		int GetNumberOfPossibleKitsFromChildLine(WhsPickableDocketLine childLine, OrgSupplierPart mainProduct, Func<WhsPickableDocketLine, ZDecimal> getQuantity)
		{
			var result = 0;

			var bomPart = mainProduct.BillOfMaterials.FindByComponentPKandPackType(childLine.WE_OP, childLine.WE_F3_NKPackType);
			if (bomPart != null)
			{
				result = BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromBOMComponent(bomPart, getQuantity(childLine), childLine.WE_F3_NKPackType);
			}

			return result;
		}

		#endregion

		#region PickLineQuantity

		public ZDecimal PickLineQuantity => CanGetPickLineQuantity ? PickLines.GetQtyCommitted() : ZDecimal.Zero;

		public ZDecimal PickedPickLineQuantity => CanGetPickLineQuantity ? PickLines.GetQtyPicked() : ZDecimal.Zero;

		#endregion

		#region CanGetPickLineQuantity

		protected virtual bool CanGetPickLineQuantity => true;

		#endregion

		#region QuantityNotPicked

		public ZDecimal QuantityNotPicked => WE_TransactionQuantity - PickLineQuantity;

		#endregion

		#region WE_CrossDockQuantity

		public ZDecimal WE_CrossDockQuantity
		{
			get { return WE_CrossDockQuantityCore; }
		}

		protected abstract ZDecimal WE_CrossDockQuantityCore
		{
			get;
		}

		public ZPropertyInfo WE_CrossDockQuantityInfo => GetZPropertyInfo(Schema.WE_CrossDockQuantity);

		#endregion

		#region WE_ShortfallQuantityCached

		#region WE_ShortfallQuantityCached

		/// <summary>
		/// The shortfall quantity is cached so it is not guaranteed to be updated.
		/// </summary>
		public ZDecimal WE_ShortfallQuantityCached
		{
			get
			{
				ZDecimal result = 0m;

				var pickableDocket = PickableDocket;
				if (pickableDocket != null && WE_OP.IsValid) // shortfall calc can be expensive, avoid until SupplierPart and Units are set
				{
					if (wE_ShortfallQuantityCached.HasValue)
					{
						result = wE_ShortfallQuantityCached.Value;
					}
					else if (!Shortfall.IsShortfallCalculationSuspended)
					{
						if (pickableDocket.WD_WP.IsValid)
						{
							WE_ShortfallQuantityCached = CalculateShortfallForPickedOrder();
						}
						else // ..order not yet picked, need to look at inventory
						{
							CalculateShortfallFromDBForUnpickedOrder();
						}
						result = wE_ShortfallQuantityCached.Value;
					}
				}

				return result;
			}
			protected set
			{
				var newValue = Math.Max(0, value); // never show a negative Shortfall (can happen when a user alters the Order after picking, prior to deallocation of 'overpicked' stock)
				if (newValue != wE_ShortfallQuantityCached)
				{
					wE_ShortfallQuantityCached = newValue;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWE_ShortfallQuantityCached_WithoutUpdatingCache();

						if (!SuspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore.IsSuspended)
						{
							var pickableDocket = PickableDocket;

							if (!pickableDocket.IsCalculatingShortfallForAllLinesSemaphore.IsSuspended &&
								!((IBusinessObjectInternals)pickableDocket).IsInPreSaveValidation)
							{
								pickableDocket.Validation.ValidateWD_TotalUnits();
							}
						}
					}

					Shortfall.HasProductUnitsOrAttribsChanged = false; // reset
					WE_ShortfallQuantityCachedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo WE_ShortfallQuantityCachedInfo
		{
			get { return GetZPropertyInfo(Schema.WE_ShortfallQuantityCached, Res.GetString("6069c397-c306-4c0b-a4d1-4d437b669148", "Shortfall Qty")); }
		}

		protected bool WE_ShortfallQuantityCached_ReadOnly => true;

		ZDecimal? wE_ShortfallQuantityCached;

		internal IDisposable SuspendTotalUnitsValidationOnSettingShortfallQuantity()
		{
			return new SemaphoreManager(SuspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore);
		}

		Semaphore SuspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore
		{
			get { return suspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore ?? (suspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore = new Semaphore()); }
		}

		Semaphore suspendTotalUnitsValidationOnSettingShortfallQuantitySemaphore;

		#endregion

		#region CalculateShortfallForUnpickedOrder

		public bool CanCalculateShortfallForAllLinesFromDB => CanCalculateShortfallForAllLinesFromDBCore;

		protected virtual bool CanCalculateShortfallForAllLinesFromDBCore => true;

		void CalculateShortfallFromDBForUnpickedOrder()
		{
			if (!IsImportingData)
			{
				if (!PickableDocket.CalculateShortfallForAllLinesInOneDbHit_WasCalled && IsInDatabase && CanCalculateShortfallForAllLinesFromDB)
				{
					CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit();
				}
				else
				{
					CalculateShortfallForUnpickedOrder();
				}

				if (!wE_ShortfallQuantityCached.HasValue)
				{
					WE_ShortfallQuantityCached = 0m; // safety net
				}
			}
		}

		protected void CalculateShortfallForUnpickedOrder()
		{
			if (!IsImportingData)
			{
				WE_ShortfallQuantityCached = CalculateShortfallForUnpickedOrderCore();
			}
		}

		protected abstract decimal CalculateShortfallForUnpickedOrderCore();

		#region CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit

		void CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit()
		{
			var pickableDocket = PickableDocket;
			if (pickableDocket.Pick != null)
			{
				throw new NotSupportedException("Do not call CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit() for an Order that has a Pick.");
			}

			pickableDocket.CalculateShortfallForAllLinesInOneDbHit_WasCalled = true;

			var shouldComparePalletID = CanOrderPalletID;
			var linesToPick = pickableDocket.GetLinesToPick();
			var docketLines = new WhsPickableDocketLine[linesToPick.Count];
			var products = new HashSet<ZGuid>();
			for (var index = 0; index < linesToPick.Count; index++)
			{
				var line = linesToPick[index];
				docketLines[index] = line;
				products.Add(line.WE_OP);
			}

			var isHeldInventoryOrder = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value && docketLines.Length > 0 && !docketLines[0].WE_WHC_NKOrderedHeldCode.IsEmpty;
			var quantityAvailableList = GetQuantityAvailableForAllLinesInOneDbHit(shouldComparePalletID, products, isHeldInventoryOrder).ToDictionary(o => (ZGuid)o[WhsDocketLineSchema.Constants.PK]);
			var availableCacheByProduct = new Dictionary<string, decimal>();
			using (new SemaphoreManager(pickableDocket.IsCalculatingShortfallForAllLinesSemaphore))
			{
				foreach (var line in GetLinesSortedInShortfallPriority(docketLines).Values)
				{
					var lineKey = GetProductKeyCompare(line, shouldComparePalletID);
					var availableInfo = quantityAvailableList.ContainsKey(line.PK) ? quantityAvailableList[line.PK] : null;
					var qtyAvailable = GetAvailableQuantity(availableCacheByProduct, lineKey, availableInfo);
					var reservedQty = (ZDecimal?)availableInfo?["TotalReservedQty"] ?? ZDecimal.Zero;
					var requiredQty = line.WE_TransactionQuantity - reservedQty;
					var shortfallQuantity = Math.Max(requiredQty - qtyAvailable, 0m);
					availableCacheByProduct[lineKey] = Math.Max(availableCacheByProduct[lineKey] - requiredQty, 0m);

					using (((ISingleElementListInternal)line).SuspendListChanged())
					{
						if (line.IsBOMProductPickedOnSalesOrder)
						{
							line.WE_ShortfallQuantityCached = ShortfallQuantityHelper.ReCalculateShortFallConsideringBOMComponents(
								shortfallQuantity, SupplierPart, pickableDocket.WD_OH_Client, pickableDocket.WD_WW_Whs);
						}
						else
						{
							line.WE_ShortfallQuantityCached = shortfallQuantity;
						}
					}
				}
			}
		}

		decimal GetAvailableQuantity(Dictionary<string, decimal> availablesCache, string lineKey, DynamicBusinessObject available)
		{
			if (!availablesCache.TryGetValue(lineKey, out var result))
			{
				if (available != null)
				{
					result = (ZDecimal)available["TotalAvailableQty"];
				}
				availablesCache.Add(lineKey, result);
			}
			return result;
		}

		string GetProductKeyCompare(WhsPickableDocketLine line, bool shouldComparePalletID)
		{
			var keyParts = new string[]
			{
				line.WE_OP.ToString(),
				line.WE_PartAttrib1,
				line.WE_PartAttrib2,
				line.WE_PartAttrib3,
				line.WE_SerialNumber,
				line.WE_ExpiryDate.ToString(),
				line.WE_PackingDate.ToString(),
				line.WE_BondedEntryKey
			};

			return string.Join("|", shouldComparePalletID ? keyParts.Append((string)line.WE_PalletID) : keyParts);
		}

		DynamicBusinessObjectCollection GetQuantityAvailableForAllLinesInOneDbHit(bool shouldComparePalletID, HashSet<ZGuid> productPKs, bool isHeldInventory)
		{
			var docket = PickableDocket;

			#region SuppressResourceStringsCheckRegion

			var palletIDWhereClause = shouldComparePalletID ? @"
	AND (OrderLine.WE_PalletID = '' OR InventoryLine.WE_PalletID = OrderLine.WE_PalletID)" : "";

			var query = $@"
CREATE TABLE #Products
(
	OP_PK uniqueidentifier NOT NULL,
	OU_UseExpiryDate bit NOT NULL,
	MinShelfLife smallint NOT NULL,
	INDEX NR_RC__OP_PK CLUSTERED(OP_PK)
)

INSERT INTO #Products
SELECT
	Client.OU_OP AS OP_PK,
	Client.OU_UseExpiryDate,
	COALESCE(
		NULLIF(Consignee.OU_ConsigneeMinShelfLifeAccepted, 0),
		NULLIF(@MinShelfLifeFromOrgMiscServ, 0),
		Client.OU_ConsigneeMinShelfLifeAccepted) as MinShelfLife
FROM
	dbo.OrgPartRelation AS Client
	LEFT JOIN dbo.OrgPartRelation AS Consignee ON (
		Consignee.OU_OP = Client.OU_OP
		AND Consignee.OU_OH = @ConsigneePK
		AND Consignee.OU_Relationship = '{OrgPartRelation.RelationshipTypes.WarehouseConsignee}')
WHERE
	Client.OU_OP IN (SELECT Value FROM @ProductPKs)
	AND Client.OU_OH = @ClientPK
	AND Client.OU_Relationship IN ('{OrgPartRelation.RelationshipTypes.Owner}', '{OrgPartRelation.RelationshipTypes.Both}')

SELECT
    OrderLine.WE_PK,
    SUM(InventoryLine.WE_StockOnHand) - SUM(ISNULL(CommittedUnits,0)) as TotalAvailableQty,
    SUM(ISNULL(ReservedQty, 0)) as TotalReservedQty
FROM
    dbo.WhsDocketLine AS OrderLine
    JOIN dbo.WhsDocketLine AS InventoryLine ON InventoryLine.WE_OP = OrderLine.WE_OP
	JOIN dbo.WhsDocket AS InventoryDocket ON InventoryLine.WE_WD = InventoryDocket.WD_PK
	JOIN dbo.WhsLocationView_DoNotUse WITH(NOEXPAND) ON WLV_PK = InventoryLine.WE_WL AND WLV_WW_Whs = @WarehousePK {(isHeldInventory ? string.Empty : $"AND WLV_LocationStatus = '{LocationStatus.Codes.Normal}'")}
	JOIN #Products ON OP_PK = OrderLine.WE_OP
    CROSS APPLY
    (
        SELECT
            SUM(WZ_Units) as CommittedUnits,
            SUM(ReservedQty) as ReservedQty
        FROM
            dbo.WhsPickLine CommittedPickLine
            JOIN dbo.WhsDocketLine CommittedDocketLine ON CommittedDocketLine.WE_PK = WZ_WE_TransactionLine
            JOIN dbo.WhsDocket CommittedDocket ON CommittedDocket.WD_PK = CommittedDocketLine.WE_WD AND CommittedDocket.WD_DocketStatus <> 'CAN'
            CROSS APPLY (SELECT CASE WHEN CommittedPickLine.WZ_WE_TransactionLine = OrderLine.WE_PK THEN WZ_Units ELSE 0 END AS ReservedQty) AS ReservedQty
        WHERE
            CommittedPickLine.WZ_PickedDateTime IS NULL
            AND CommittedDocket.WD_IsPutawayTransfer = 0
            AND CommittedPickLine.WZ_WE_InventoryLine = InventoryLine.WE_PK
    ) PickLines
WHERE
    OrderLine.WE_WD = @DocketPK
    AND InventoryDocket.WD_OH_Client = @ClientPK
    AND InventoryLine.WE_CurrentInventoryStatus = '{(isHeldInventory ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Available)}'
    AND InventoryLine.WE_StockOnHand > 0{palletIDWhereClause}
    AND (OrderLine.WE_PartAttrib1 = '' OR InventoryLine.WE_PartAttrib1 = OrderLine.WE_PartAttrib1)
    AND (OrderLine.WE_PartAttrib2 = '' OR InventoryLine.WE_PartAttrib2 = OrderLine.WE_PartAttrib2)
    AND (OrderLine.WE_PartAttrib3 = '' OR InventoryLine.WE_PartAttrib3 = OrderLine.WE_PartAttrib3)
    AND (OrderLine.WE_SerialNumber = '' OR InventoryLine.WE_SerialNumber = OrderLine.WE_SerialNumber)
    AND (OrderLine.WE_PackingDate IS NULL OR InventoryLine.WE_PackingDate = OrderLine.WE_PackingDate)
    AND (OrderLine.WE_ExpiryDate IS NULL OR InventoryLine.WE_ExpiryDate = OrderLine.WE_ExpiryDate)
    AND (OrderLine.WE_BondedEntryKey = '' OR OrderLine.WE_BondedEntryKey = InventoryLine.WE_BondedEntryKey)
    AND (OU_UseExpiryDate = 0 OR (InventoryLine.WE_ExpiryDate IS NOT NULL AND InventoryLine.WE_ExpiryDate > DATEADD(day, MinShelfLife, @TodaysDate)))
	{(isHeldInventory ? "AND OrderLine.WE_WHC_NKOrderedHeldCode = InventoryLine.WE_WHC_NKCurrentInventoryHeldCode" : string.Empty )}
GROUP BY
    OrderLine.WE_PK

DROP TABLE #Products";

			#endregion

			var queryParams = new ZSqlParameterCollection
			{
				{ "@DocketPK", docket.PK, WhsDocketLineSchema.WE_WD },
				{ "@ConsigneePK", docket.ConsigneePK, OrgAddressSchema.OA_OH },
				{ "@ClientPK", docket.WD_OH_Client, WhsDocketSchema.WD_OH_Client },
				{ "@WarehousePK", docket.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs },
				{ "@MinShelfLifeFromOrgMiscServ", docket.Consignee?.MiscServ.OM_MinimumShelfLifeAccepted ?? ZShort.Zero, OrgMiscServSchema.OM_MinimumShelfLifeAccepted },
				{ "@TodaysDate", ZDateTime.Today, WhsInventoryViewSchema.WI_ExpiryDate },
				ZSqlParameter.New("@ProductPKs", productPKs.ToArray(), WhsDocketLineSchema.WE_OP, isTableValued: true)
			};

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(query, queryParams);

			return result;
		}

		#endregion

		#endregion

		#region ShortfallQuantityHelper

		WhsOrderLineShortfallQuantityHelper ShortfallQuantityHelper
		{
			get { return shortfallQuantityHelper ?? (shortfallQuantityHelper = new WhsOrderLineShortfallQuantityHelper(Factory)); }
		}
		WhsOrderLineShortfallQuantityHelper shortfallQuantityHelper;

		#endregion

		#region CalculateShortfallForPickedOrder

		protected abstract decimal CalculateShortfallForPickedOrder();

		#endregion

		#region SetShortfallForTest / GetShortfallCacheValueForTest
#if DEBUG
		public void SetShortfallForTest(ZDecimal shortfallQuantity)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("WhsPickableDocketLine.SetShortfallForTest() should only be called from a test.");
			}
			wE_ShortfallQuantityCached = shortfallQuantity;
		}

		public ZDecimal? GetShortfallCacheValueForTest()
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("WhsPickableDocketLine.GetShortfallsForTest() should only be called from a test.");
			}
			return wE_ShortfallQuantityCached;
		}
#endif

		#endregion

		#region ShortfallHelper

		public ShortfallHelper Shortfall
		{
			get { return shortfall ?? (shortfall = new ShortfallHelper(this)); }
		}

		ShortfallHelper shortfall;

		public class ShortfallHelper
		{
			public ShortfallHelper(WhsPickableDocketLine line)
			{
				Line = line;
			}

			readonly WhsPickableDocketLine Line;

			#region HasProductUnitsOrAttribsChanged

			public bool HasProductUnitsOrAttribsChanged
			{
				get;
				internal set;
			}

			#endregion

			#region SuspendShortfallCalculation

			public bool IsShortfallCalculationSuspended
			{
				get
				{
					return SuspendShortfallCalculationSemaphore.IsSuspended
						|| Line.IsImportingData
						|| Line.PickableDocket.IsCalculatingShortfallForAllLinesSemaphore.IsSuspended;
				}
			}

			public void SuspendShortfallCalculation()
			{
				SuspendShortfallCalculationSemaphore.Suspend();
			}

			public void ResumeShortfallCalculation()
			{
				SuspendShortfallCalculationSemaphore.Resume();
			}

			#region SuspendShortfallCalculationSemaphore

			SuspendableSemaphore SuspendShortfallCalculationSemaphore
			{
				get { return suspendShortfallCalculationSemaphore ?? (suspendShortfallCalculationSemaphore = new SuspendableSemaphore()); }
			}

			SuspendableSemaphore suspendShortfallCalculationSemaphore;

			#endregion

			#endregion

			#region SuspendableSemaphore

			class SuspendableSemaphore : Semaphore
			{
				public void Suspend()
				{
					SemaphoreInternals.Increment();
				}

				public void Resume()
				{
					SemaphoreInternals.Decrement();
				}

				ISemaphoreItemInternals SemaphoreInternals
				{
					get { return this; }
				}
			}

			#endregion
		}

		#endregion

		#endregion

		#region GetShortfallExistsStatus

		public bool GetShortfallExistsStatus()
		{
			if (!Shortfall.IsShortfallCalculationSuspended)
			{
				ClearWE_ShortfallQuantityCached();
			}

			return GetShortfallExistsStatus_WithoutUpdatingCache();
		}

		public bool GetShortfallExistsStatus_WithoutUpdatingCache()
		{
			return WE_ShortfallQuantityCached > 0;
		}

		public void ClearWE_ShortfallQuantityCached()
		{
			wE_ShortfallQuantityCached = null;
		}

		#endregion

		#region SetHasProductUnitsOrAttribsChangedForRelatedLines

		public IDisposable DeferSettingHasProductUnitsOrAttribsChanged()
		{
			var disposable = new SemaphoreManager(SettingHasProductUnitsOrAttribsChangedSemaphore);
			return new DisposableAction(() =>
			{
				disposable.Dispose();
				if (!SettingHasProductUnitsOrAttribsChangedSemaphore.IsSuspended)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
			});
		}

		protected void SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(bool markThisLine)
		{
			if (!IsCopying && SupportsHasProductUnitsOrAttribsChanged && !SettingHasProductUnitsOrAttribsChangedSemaphore.IsSuspended)
			{
				var pickableDocket = PickableDocket;
				if (pickableDocket != null)
				{
					if (!pickableDocket.ShortfallManager.IsMarkingLinesAsShortfallPropertiesChangedSuspended)
					{
						var linesWithSameProductLowerPriorityInShortfall = GetOtherLinesForThisProductWithLowerShortfallPrority();

						foreach (var changedLine in linesWithSameProductLowerPriorityInShortfall)
						{
							changedLine.Shortfall.HasProductUnitsOrAttribsChanged = true;
						}

						if (markThisLine)
						{
							Shortfall.HasProductUnitsOrAttribsChanged = true;
						}
					}
					else
					{
						pickableDocket.ShortfallManager.AddLineToCheckShortfallPropertiesChangedLater(this);
					}
				}
			}
		}

		protected virtual bool SupportsHasProductUnitsOrAttribsChanged => true;

		IEnumerable<WhsPickableDocketLine> GetOtherLinesForThisProductWithLowerShortfallPrority()
		{
			var lines = PickableDocket.ShortfallManager.GetSameProductLines(this) ?? Enumerable.Empty<WhsPickableDocketLine>();
			var sortedLines = GetLinesSortedInShortfallPriority(lines);
			var indexOfThisLine = sortedLines.IndexOfKey((WE_LineNo, WE_SubLineNo, PK));

			for (var index = indexOfThisLine + 1; index < sortedLines.Count; index++)
			{
				yield return sortedLines.Values[index];
			}
		}

		Semaphore SettingHasProductUnitsOrAttribsChangedSemaphore => settingHasProductUnitsOrAttribsChangedSemaphore ?? (settingHasProductUnitsOrAttribsChangedSemaphore = new Semaphore());
		Semaphore settingHasProductUnitsOrAttribsChangedSemaphore;

		#endregion

		#region ProductUsesExpiryDate

		public bool ProductUsesExpiryDate => DocketProductUsesExpiryDate(null);

		bool DocketProductUsesExpiryDate(WhsDocket docket)
		{
			var whsDocket = docket ?? Docket;
			return whsDocket?.Client?.PartAttributeManager.IsExpiryDateUsedByProduct(Product?.Parent) ?? false;
		}

		#endregion

		#region MinimumShelfLife

		public ZShort MinimumShelfLife
		{
			get
			{
				var result = ZShort.Zero;
				var docket = PickableDocket;
				if (DocketProductUsesExpiryDate(docket))
				{
					var consignee = docket?.Consignee;
					var partRelations = Product.Parent.RelatedOrganisations;
					if (!(consignee is null))
					{
						result = partRelations.FindByOrganisationPKAndRelationship(
							consignee.PK,
							OrgPartRelation.RelationshipTypes.WarehouseConsignee)?.OU_ConsigneeMinShelfLifeAccepted ?? ZShort.Zero;

						if (result == ZShort.Zero)
						{
							result = consignee.MiscServ?.OM_MinimumShelfLifeAccepted ?? ZShort.Zero;
						}
					}

					if (result == ZShort.Zero)
					{
						result = partRelations.FindByOrganisationPKAndRelationship(
							docket.WD_OH_Client,
							OrgPartRelation.RelationshipTypes.Owner)?.OU_ConsigneeMinShelfLifeAccepted ?? ZShort.Zero;
					}
				}

				return result;
			}
		}

		#endregion

		#region WE_PickGroup

		public override ZShort WE_PickGroup
		{
			get => base.WE_PickGroup;
			set
			{
				base.WE_PickGroup = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickGroup();
				}

				PickGroupForBindingInfo.RefreshBinding();
			}
		}

		#endregion

		#region PickGroupForBinding

		[BusinessObjectTestExclude]
		[List("Lookups.PickGroups")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsJobFinalisedOrCancelledReadOnly")]
		[ReadOnlyMember("IsJobFinalisedOrCancelledReadOnly")]
		[MaxLength(PickGroupHelper.PickGroupForBindingMaxLength)]
		public ZString PickGroupForBinding
		{
			get => PickGroupHelper.GetPickGroupDescription(WE_PickGroup, Lookups.PickGroups);
			set => WE_PickGroup = PickGroupHelper.GetPickGroupFromString(value);
		}

		public ZPropertyInfo PickGroupForBindingInfo => GetZPropertyInfo(nameof(PickGroupForBinding), WE_PickGroupInfo.HumanReadableName);

		#endregion

		#region ExpectedDefaultInventoryStatus

		protected override string DefaultInventoryStatus => string.Empty;

		#endregion

		#region CalculateExtendedLinePrice

		public void CalculateExtendedLinePrice()
		{
			var docket = PickableDocket;
			if (docket != null && docket.IsRecalculateOrderPricing)
			{
				WE_ExtendedLinePrice = WE_UnitPriceAfterDiscount * (docket.IsAttachedToPick ? SumOfUnitsMet : WE_TransactionQuantity);
			}
		}

		#endregion

		#region IsPicked

		bool IsPicked
		{
			get
			{
				var docket = PickableDocket;
				return docket != null && docket.IsAttachedToPick;
			}
		}

		#endregion

		#region AutoCalculatePriceReadOnly

		protected override bool AutoCalculatePriceReadOnly
		{
			get
			{
				var docket = PickableDocket;
				return base.AutoCalculatePriceReadOnly || (docket != null && docket.IsRecalculateOrderPricing);
			}
		}

		#endregion

		#endregion

		#region Flags

		#region CanCrossDockInventory

		public bool CanCrossDockInventory
		{
			get { return CanCrossDockInventoryCore; }
		}

		protected abstract bool CanCrossDockInventoryCore
		{
			get;
		}

		#endregion

		#region CanGenerateChildWorkOrder

		public bool CanGenerateChildWorkOrder
		{
			get { return CanGenerateChildWorkOrderCore; }
		}

		protected abstract bool CanGenerateChildWorkOrderCore { get; }

		#endregion

		#region IsBOMProductPickedOnSalesOrder

		public bool IsBOMProductPickedOnSalesOrder
		{
			get { return IsBOMProductPickedOnSalesOrderCore; }
		}

		protected virtual bool IsBOMProductPickedOnSalesOrderCore
		{
			get { return false; }
		}

		#endregion

		#region IsComponentLineOnSalesOrder

		public bool IsComponentLineOnSalesOrder
		{
			get { return IsComponentLineOnSalesOrderCore; }
		}

		protected abstract bool IsComponentLineOnSalesOrderCore
		{
			get;
		}

		#endregion

		#region IsDocketPicking

		public bool IsDocketPicking
		{
			get
			{
				var pickableDocket = PickableDocket;
				return pickableDocket != null && pickableDocket.IsAttachedToPickButNotFinalised;
			}
		}

		#endregion

		#region IsPickFinalising

		public bool IsPickFinalising
		{
			get
			{
				var pickableDocket = PickableDocket;
				var result = false;

				if (pickableDocket != null)
				{
					var pick = pickableDocket.Pick;
					result = pick != null && pick.IsFinalising;
				}
				return result;
			}
		}

		#endregion

		#region IsPickLinesLoaded

		public bool IsPickLinesLoaded
		{
			get { return IsPickLinesCreated; }
		}

		#endregion

		#region IsDocketUnpicked

		public bool IsDocketUnpicked
		{
			get
			{
				var docket = PickableDocket;
				return docket != null && docket.WD_WP.IsEmpty && !docket.IsCancelled;
			}
		}

		#endregion

		#region IsPartiallyOrFullyPickedFromPutawayLocation

		public bool IsPartiallyOrFullyPickedFromPutawayLocation => PickLines.Any(l => l.IsPickedFromPutawayLocation);

		#endregion

		#region IsCurrentlyBeingPickedFromPutawayLocation

		public bool IsCurrentlyBeingPickedFromPutawayLocation => PickLines.Any(l => l.WZ_IsPicking);

		#endregion

		#region IsDangerousGood

		[ResourceStringData("WhsPickableDocketLine|IsDangerousGood", Caption = "Is Dangerous Good", ShortCaption = "Is DG")]
		public ZBool IsDangerousGood => Product?.FirstUNDG != null;

		#endregion

		#endregion

		#region ReduceOverpickedStock

		/// <summary>
		/// The pick algorithm will pick as much as it can, but if one or more components are in shortfall, we may have overpicked *this* component.
		/// 
		/// For example, a Bike requires:
		///		1 frame
		///		2 wheels
		///		
		/// If we order 10 bikes, we need 10 frames + 20 wheels. If only 8 frames are in stock, 8 frames + 20 wheels will be picked.
		/// We need to return 4 wheels.
		///
		/// *** WhsWorkOrderLines tested in WhsPick.TestPickOrders_ReturnsOverpickedWorkOrderComponents() ***
		/// *** Pick By BOM Child Compone OrderLines tested in WhsPick.TestAutoAllocateItems_PickByBom_* tests() ***
		/// 
		/// </summary>
		protected void ReduceOverpickedStock(ZDecimal reduceBy)
		{
			if (reduceBy < 0)
			{
				throw new ArgumentException("Cannot reduce stock by a negative amount.");
			}

			if (!ReduceOverpickedStockAllowed)
			{
				throw new NotSupportedException("Can only reduce allocations for WhsWorkOrderLines or Pick-By-BOM Child Component Lines.");
			}

			var orderedInventory = PickableDocket.Pick.OrderedInventories.GetOrderedInventoryForLine(this);
			for (var i = orderedInventory.AvailableInventories.Count - 1; i >= 0 && reduceBy > 0m; i--)
			{
				var availableInventory = orderedInventory.AvailableInventories[i];
				var units = availableInventory.PickLineQuantity;
				if (units <= 0)
				{
					continue;
				}
				else if (units > reduceBy)
				{
					availableInventory.PickLineQuantity = units - reduceBy;
					reduceBy = 0;
				}
				else
				{
					availableInventory.PickLineQuantity = 0m;
					reduceBy -= units;
				}
			}
		}

		protected abstract bool ReduceOverpickedStockAllowed { get; }

		#endregion

		#region GetLinesSortedInShortfallPriority

		SortedList<(ZShort, ZShort, ZGuid), WhsPickableDocketLine> GetLinesSortedInShortfallPriority(IEnumerable<WhsPickableDocketLine> lines)
		{
			var sortedLines = new SortedList<(ZShort, ZShort, ZGuid), WhsPickableDocketLine>();
			foreach (var line in lines.OrderBy(l => l.WE_LineNo).ThenBy(l => l.WE_SubLineNo).ThenBy(l => l.PK))
			{
				sortedLines.Add((line.WE_LineNo, line.WE_SubLineNo, line.PK), line);
			}

			return sortedLines;
		}

		#endregion

		#region GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall

		protected ZDecimal GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall(ZDecimal currentShortfall, ZDecimal orderedQty, decimal unitPerParentProduct = 1m)
		{
			ZDecimal result;

			if (currentShortfall < orderedQty)
			{
				foreach (var higherPriorityLine in GetOtherLinesForThisProductWithHigherShortfallPrority())
				{
					currentShortfall += higherPriorityLine.OrderedQtyExcludingReservedQty * unitPerParentProduct;
					if (currentShortfall >= orderedQty)
					{
						return orderedQty;
					}
				}

				result = Math.Max(currentShortfall, 0);
			}
			else
			{
				result = orderedQty;
			}

			return result;
		}

		IEnumerable<WhsPickableDocketLine> GetOtherLinesForThisProductWithHigherShortfallPrority()
		{
			var sortedLines = GetLinesSortedInShortfallPriority(PickableDocket.ShortfallManager.GetSameProductLines(this));
			var indexOfThisLine = sortedLines.IndexOfKey((WE_LineNo, WE_SubLineNo, PK));

			for (var index = 0; index < indexOfThisLine; index++)
			{
				yield return sortedLines.Values[index];
			}
		}

		#endregion

		#region NotifyObsoleteReleaseLineChangedOnDataRefresh

		internal void NotifyObsoleteReleaseLineChangedOnDataRefresh()
		{
			ClearReleaseLines();
		}

		#endregion

		#region CancelLineCore

		protected override void CancelLineCore() => Array.ForEach(PickLines.ToArray(), l => l.Delete());

		#endregion

		#region Validation

		public new WhsPickableDocketLineValidation Validation => (WhsPickableDocketLineValidation)base.Validation;

		protected sealed override WhsDocketLineValidation GetNewValidation()
		{
			return GetNewPickableDocketLineValidation();
		}

		protected abstract WhsPickableDocketLineValidation GetNewPickableDocketLineValidation();

		#endregion

		#region Lookups

		public new WhsPickableDocketLineLookups Lookups => (WhsPickableDocketLineLookups)base.Lookups;

		protected override WhsDocketLineLookups GetNewLookups()
		{
			return new WhsPickableDocketLineLookups(this);
		}

		#endregion
	}
}
