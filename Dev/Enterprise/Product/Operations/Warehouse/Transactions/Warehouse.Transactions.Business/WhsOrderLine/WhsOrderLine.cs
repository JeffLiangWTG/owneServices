using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsOrder), "Lines")]
	[SystemDefinedValues]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder, typeof(IWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine))]
	public class WhsOrderLine : WhsPickableDocketLine, ICustomsDataParent, IWhsOrderLineWrapperStrategy
	{
		#region Constructors

		public WhsOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Strategies

		protected IWhsOrderLineWrapperStrategy wrapperStrategy;
		public IWhsOrderLineWrapperStrategy WrapperStrategy
		{
			get
			{
				if (wrapperStrategy == null)
				{
					var builder = ObjectFactory.Get<ITrackingOrderLineStrategyBuilder>();
					wrapperStrategy = builder.Build(this);
				}

				return wrapperStrategy;
			}
		}

		#endregion

		#region Type Decider

		public override Type DocketType => typeof(WhsOrder);

		#endregion

		#region Public Static Methods

		public static WhsOrderLine New(BusinessObjectFactory factory)
		{
			return factory.
				// Split so find replace will ignore
				New<WhsOrderLine>();
		}

		#endregion

		#region Related Entities

		#region InventoryFilter

		protected override WhsInventoryViewCollection InventoryFilterCore
		{
			get
			{
				var list = base.InventoryFilterCore;

				var docket = Docket;
				if (docket != null)
				{
					if (docket.WD_WW_Whs.IsValid)
					{
						list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", docket.WD_WW_Whs));
					}

					if (docket.WD_OH_Client.IsValid)
					{
						list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", docket.WD_OH_Client));
					}
				}

				if (WE_OP.IsValid)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", WE_OP));
				}

				if (!WE_ExpiryDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property1", WE_ExpiryDate));
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property2", WE_ExpiryDate));
				}

				if (!WE_PackingDate.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property1", WE_PackingDate));
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property2", WE_PackingDate));
				}

				if (!WE_PartAttrib1.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 1", "Property", WE_PartAttrib1));
				}

				if (!WE_PartAttrib2.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 2", "Property", WE_PartAttrib2));
				}

				if (!WE_PartAttrib3.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 3", "Property", WE_PartAttrib3));
				}

				if (!WE_SerialNumber.IsEmpty)
				{
					list.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Serial Number", "Property", WE_SerialNumber));
				}

				return list;
			}
		}

		#endregion

		#region Order

		public WhsOrder Order => (WhsOrder)Docket;

		#endregion

		#region RelatedInLineForExternalProcessing

		public IWhsWarehouseTransactionLine RelatedInLineForExternalProcessing
		{
			get;
			set;
		}

		#endregion

		#region CustomData

		protected override void SetDefaultValueWhsBondedWarehouseAttribute(WhsBondedWarehouseAttribute bondedWarehouseAttribute)
		{
			if (IsCustomsTransaction)
			{
				bondedWarehouseAttribute.SetDefaultOutwardTypeIfEmpty();
			}
		}

		#endregion

		#endregion

		#region SetDefaultValues

		protected override string DocketLineType => CodeLists.DocketType.Codes.Order;

		#endregion

		#region Validation

		public new WhsOrderLineValidation Validation
		{
			get { return (WhsOrderLineValidation)base.Validation; }
		}

		protected override WhsPickableDocketLineValidation GetNewPickableDocketLineValidation()
		{
			switch (CountryCode)
			{
				case Constants.CountryCodes.UnitedStates:
					return new US.WhsOrderLineValidationUS(this);

				default:
					return new WhsOrderLineValidation(this);
			}
		}

		#endregion

		#region Lookups

		public new WhsOrderLineLookups Lookups => (WhsOrderLineLookups)base.Lookups;

		protected override WhsDocketLineLookups GetNewLookups()
		{
			return new WhsOrderLineLookups(this);
		}

		#endregion

		#region Properties

		// persistent

		#region WE_OP

		public override ZGuid WE_OP
		{
			get => base.WE_OP;
			set
			{
				var previousValue = base.WE_OP;
				base.WE_OP = value;
				DefaultPickGroupFromProductParams();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PalletID();
				}
			}
		}

		void DefaultPickGroupFromProductParams()
		{
			var order = Order;
			if (WE_PickGroup <= 0 && WE_OP.IsValid && order != null)
			{
				WE_PickGroup = PickGroupLoader.GetDefaultPickGroup(((IBusinessObjectFactoryInternals)Factory).RowFactory, WE_OP, order.WD_WW_Whs, order.WD_OH_Client);
			}
		}

		protected override bool CanOrderPalletID => true;

		#endregion

		#region WE_PalletID

		public override ZString WE_PalletID
		{
			get => base.WE_PalletID;
			set
			{
				var previousValue = base.WE_PalletID;
				if (previousValue != value)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: true);
				}
				base.WE_PalletID = value;
				if (previousValue != WE_PalletID)
				{
					SetHasProductUnitsOrAttribsChangedForRelatedLinesIfSupported(markThisLine: false);
				}
			}
		}

		#endregion

		#region WE_F3_NKPackType

		protected override ZString GetPackTypeFromProductParams(WhsProductParamsByWhsAndClient productParams)
		{
			return productParams.W3_F3_NKReleasedPackType;
		}

		#endregion

		#region WE_TransactionQuantity

		public override ZDecimal WE_TransactionQuantity
		{
			set
			{
				bool valueChanged = (value != base.WE_TransactionQuantity);
				base.WE_TransactionQuantity = value;
				WrapperStrategy.WE_TransactionQuantity_SetAfter(valueChanged);
			}

			get => base.WE_TransactionQuantity;
		}

		#endregion

		#region WE_UnitPriceAfterDiscount

		[ReadOnlyMember(nameof(IsJobFinalisedOrCancelledReadOnly))]
		public override ZDecimal WE_UnitPriceAfterDiscount
		{
			get => base.WE_UnitPriceAfterDiscount;
			set => base.WE_UnitPriceAfterDiscount = value;
		}

		#endregion

		#region WE_UnitDiscountAmount

		[ReadOnlyMember(nameof(IsJobFinalisedOrCancelledReadOnly))]
		public override ZDecimal WE_UnitDiscountAmount
		{
			get => base.WE_UnitDiscountAmount;
			set => base.WE_UnitDiscountAmount = value;
		}

		#endregion

		#region WE_UnitDiscountPercent

		[ReadOnlyMember(nameof(IsJobFinalisedOrCancelledReadOnly))]
		public override ZDecimal WE_UnitDiscountPercent
		{
			get => base.WE_UnitDiscountPercent;
			set => base.WE_UnitDiscountPercent = value;
		}

		#endregion

		#region WE_RecommendedUnitPrice

		[ReadOnlyMember(nameof(IsJobFinalisedOrCancelledReadOnly))]
		public override ZDecimal WE_RecommendedUnitPrice
		{
			get => base.WE_RecommendedUnitPrice;
			set => base.WE_RecommendedUnitPrice = value;
		}

		#endregion

		#region Client Detail

		public ZString ClientDetail => GetOrganisationDetail(Order?.Client);

		#endregion

		#region Consignee Detail

		public ZString ConsigneeDetail => GetOrganisationDetail(Order?.Consignee);

		#endregion

		ZString GetOrganisationDetail(OrgHeader org) => org != null ? ZString.Format("{0} {1}", org.OH_Code, org.OH_FullName) : ZString.Empty;

		#region ReadOnly

		public override bool ReadOnly => base.ReadOnly || IsChildComponentLine;

		bool IsChildComponentLine => !WE_WE_ParentDocketLine.IsEmpty;

		protected override bool IsJobFinalisedOrCancelledReadOnly => base.IsJobFinalisedOrCancelledReadOnly || (Order?.Pick?.IsReadyForPlanningOrPlanned ?? false);

		#region PackQuantityAndTypeAndRelatedQuantityInfoReadOnly

		protected override bool PackQuantityAndTypeAndRelatedQuantityInfoReadOnly => IsJobFinalisedOrCancelledReadOnly
			|| IsLineUpdateDisabledAfterPick
			|| IsOnMultiOrderPick
			|| IsPickedUSBondedOrder
			|| (IsInDatabase && IsPartiallyOrFullyPickedFromPutawayLocation);

		#region IsOnMultiOrderPick

		bool IsOnMultiOrderPick => Order?.Pick?.IsMultiOrderPick ?? false;

		#endregion

		#region IsPickedUSBondedOrder

		bool IsPickedUSBondedOrder
		{
			get
			{
				var order = Order;
				return order != null && order.IsAttachedToPickButNotFinalised && order.IsUSBonded;
			}
		}

		#endregion

		#region IsPickingFTZCustomsOrderLine

		bool IsPickingFTZCustomsOrderLine => Order?.IsPickingFTZCustomsOrderWithPermit ?? false;

		#endregion

		#endregion

		#region AfterPickProductAndAttribsReadOnly

		protected override bool AfterPickProductAndAttribsReadOnly => PackQuantityAndTypeAndRelatedQuantityInfoReadOnly || (IsDocketPicking && IsInDatabase);

		#endregion

		protected override bool ProductReadOnly => AfterPickProductAndAttribsReadOnly;

		#region IsLineUpdateDisabledAfterPick

		bool IsLineUpdateDisabledAfterPick
		{
			get
			{
				var order = Order;
				return order != null && order.AreLinesUpdateDisabledAfterPick;
			}
		}

		#endregion

		#region IsOrderedHoldCodeReadOnly

		protected override bool IsOrderedHoldCodeReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly || Order?.IsAttachedToPick == true; }
		}

		#endregion

		#endregion

		#region WhsOrderLineToBeClearedLater

		public ZBool WhsOrderLineToBeClearedLater
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.WhsOrderLineToBeClearedLater); }
			set { this.SetSystemDefinedValue(Schema.WhsOrderLineToBeClearedLater, value); }
		}

		#endregion

		#region CustomsClearingInProgress

		public ZBool CustomsClearingInProgress
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.CustomsClearingInProgress); }
			set { this.SetSystemDefinedValue(Schema.CustomsClearingInProgress, value); }
		}

		#endregion

		// calculated

		#region CanGetPickLineQuantity

		// tested in WhsPickableDocketLine
		protected override bool CanGetPickLineQuantity => !IsDeleted && (PickableDocket?.WD_WP.IsValid ?? false);

		#endregion

		#region GetPickLinesForRelease

		protected override WhsPickLineCollection GetPickLinesForRelease()
		{
			WhsPickLineCollection pickLines;

			if (IsChildComponentLine)
			{
				pickLines = new WhsPickLineCollection(Factory);
			}
			else
			{
				AddFetchHintsForChildDocketLines();
				AddFetchHintsForDocketPickLines();

				// this will include picklines on Child Order Lines
				pickLines = new WhsPickLineCollection(this).GetLinesWithUnitsGreaterThanZero();
			}

			return pickLines;
		}

		#endregion

		#region AddFetchHintsForChildDocketLines

		void AddFetchHintsForChildDocketLines()
		{
			var order = Order;
			if (order != null)
			{
				Factory.GetCachedValue("WhsPickableDocket|AddFetchHintsForChildDocketLines|" + order.PK, () => AddFetchHintsForChildDocketLinesCore(order), CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		bool AddFetchHintsForChildDocketLinesCore(WhsOrder order)
		{
			order.ParentLines.ForEach(ol => Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, ol.PK));
			return true;
		}

		#endregion

		#region PickGroupDescription

		public ZString PickGroupDescription => Lookups.PickGroups.GetDescriptionFromCode(WE_PickGroup.ToString());

		#endregion

		#region TempAllocated

		public ZDecimal TempAllocated
		{
			get;
			set;
		}

		#endregion

		#region WE_CrossDockQuantity

		protected override ZDecimal WE_CrossDockQuantityCore => ReservedPickLines.GetQtyCommitted();

		#endregion

		#region WE_ShortfallQuantityCached

		protected override decimal CalculateShortfallForUnpickedOrderCore()
		{
			var shortFallWithoutConsideringComponents = GetShortfallWithoutConsideringComponents();
			var docket = Docket;
			return IsBOMProductPickedOnSalesOrder
				? ShortfallQuantityHelper.ReCalculateShortFallConsideringBOMComponents(shortFallWithoutConsideringComponents, SupplierPart, docket.WD_OH_Client, docket.WD_WW_Whs)
				: shortFallWithoutConsideringComponents;
		}

		internal ZDecimal GetShortfallWithoutConsideringComponents()
		{
			var shortfall = OrderedQtyExcludingReservedQty;
			var availableToPickInventory = GetAvailableToPickInventory();
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;

			for (var i = 0; i < availableToPickInventory.Length; i++)
			{
				if (i % BatchQty == 0)
				{
					var availableInventoryFetchBatch = availableToPickInventory.Skip(i).Take(BatchQty);
					Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, availableInventoryFetchBatch.Select(a => a.WI_WE_InDocketLine)));
					Factory.AddFetchHint(WhsLocationViewSchema.Instance, new ZQuery(WhsLocationViewSchema.PK, availableInventoryFetchBatch.Select(a => a.WI_WL)));
				}

				var inventory = availableToPickInventory[i];

				shortfall -= isRegistryEnableHeldGoodsForOrders && inventory.IsHeld
					? inventory.WI_AvailableToTransferQuantity
					: inventory.WI_AvailableToPickQuantity;
			}

			return GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall(shortfall, WE_TransactionQuantity);
		}

		const int BatchQty = 100;

		WhsInventoryView[] GetAvailableToPickInventory()
		{
			var isHeldInventoryOrder = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value && !WE_WHC_NKOrderedHeldCode.IsEmpty;

			#region SuppressResourceStringsCheckRegion

			var query = new ZStringBuilder(@$"
WI_OP = @PartPK
AND WI_TotalUnits > 0
AND WI_InventoryStatus = '{(isHeldInventoryOrder ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Available)}'
AND WI_OH_Client = @ClientPK
AND EXISTS
(
	SELECT
		1
	FROM
		dbo.WhsLocationView
	WHERE
		WLV_PK = WI_WL
		AND WLV_WW_Whs = @WarehousePK
		{(isHeldInventoryOrder ? string.Empty : "AND WLV_LocationStatus = 'NOR'")}
)");

			#endregion

			var minimumExpiryDate = ZDate.Empty;
			var minimumShelfLife = MinimumShelfLife;
			if (minimumShelfLife > 0)
			{
				minimumExpiryDate = ZDate.Today.AddDays(minimumShelfLife);
			}

			var queryParams = new ZSqlParameterCollection();
			var docket = Docket;
			queryParams.Add("@ClientPK", docket.WD_OH_Client, WhsDocketSchema.WD_OH_Client);
			queryParams.Add("@WarehousePK", docket.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs);
			queryParams.Add("@PartPK", WE_OP, WhsDocketLineSchema.WE_OP);

			if (!WE_PalletID.IsEmpty)
			{
				query.Append(" AND WI_PalletID <> '' AND WI_PalletID = @PalletID"); // This is an SQL Statement.....
				queryParams.Add("@PalletID", WE_PalletID, WhsDocketLineSchema.WE_PalletID);
			}

			if (!WE_PartAttrib1.IsEmpty)
			{
				query.Append(" AND WI_PartAttrib1 <> '' AND WI_PartAttrib1 = @PartAttrib1"); // This is an SQL Statement.....
				queryParams.Add("@PartAttrib1", WE_PartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
			}

			if (!WE_PartAttrib2.IsEmpty)
			{
				query.Append(" AND WI_PartAttrib2 <> '' AND WI_PartAttrib2 = @PartAttrib2"); // This is an SQL Statement.....
				queryParams.Add("@PartAttrib2", WE_PartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
			}

			if (!WE_PartAttrib3.IsEmpty)
			{
				query.Append(" AND WI_PartAttrib3 <> '' AND WI_PartAttrib3 = @PartAttrib3"); // This is an SQL Statement.....
				queryParams.Add("@PartAttrib3", WE_PartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
			}

			if (!WE_SerialNumber.IsEmpty)
			{
				query.Append(" AND WI_SerialNumber <> '' AND WI_SerialNumber = @SerialNumber"); // This is an SQL Statement.....
				queryParams.Add("@SerialNumber", WE_SerialNumber, WhsDocketLineSchema.WE_SerialNumber);
			}

			if (!WE_BondedEntryKey.IsEmpty)
			{
				query.Append(" AND @BondedEntryKey = WI_BondedEntryKey"); // This is an SQL Statement.....
				queryParams.Add("@BondedEntryKey", WE_BondedEntryKey, WhsDocketLineSchema.WE_BondedEntryKey);
			}

			if (WE_PackingDate.IsValid)
			{
				query.Append((NoResString)" AND " + WhsInventoryViewSchema.WI_PackingDate.Name + (NoResString)" = @PackingDate"); // This is an SQL Statement.....
				queryParams.Add("@PackingDate", WE_PackingDate, WhsInventoryViewSchema.WI_PackingDate);
			}

			if (WE_ExpiryDate.IsValid)
			{
				query.Append((NoResString)" AND " + WhsInventoryViewSchema.WI_ExpiryDate.Name + (NoResString)" = @ExpiryDate"); // This is an SQL Statement......
				queryParams.Add("@ExpiryDate", WE_ExpiryDate, WhsInventoryViewSchema.WI_ExpiryDate);
			}

			if (minimumExpiryDate.IsValid)
			{
				query.Append((NoResString)" AND " + WhsInventoryViewSchema.WI_ExpiryDate.Name + (NoResString)" >= @MinimumExpiryDate"); // This is an SQL Statement.....
				queryParams.Add("@MinimumExpiryDate", minimumExpiryDate, WhsInventoryViewSchema.WI_ExpiryDate);
			}
			else if (ProductUsesExpiryDate)
			{
				query.Append((NoResString)" AND " + WhsInventoryViewSchema.WI_ExpiryDate.Name + (NoResString)" > @TodaysDate"); // This is an SQL Statement.....
				queryParams.Add("@TodaysDate", ZDateTime.Today, WhsInventoryViewSchema.WI_ExpiryDate);
			}

			if (isHeldInventoryOrder)
			{
				query.Append(" AND WI_HeldCode <> '' AND WI_HeldCode = @HeldCode");
				queryParams.Add("@HeldCode", WE_WHC_NKOrderedHeldCode, WhsInventoryViewSchema.WI_HeldCode);
			}
			var filter = new ZDBOnlyQuery(typeof(WhsInventoryView));
			filter.AddFilterAndZSQLParameterCollection(query.ToStringWithNewLineBetweenAppends(), queryParams);

			return Factory.Load<WhsInventoryView>(filter);
		}

		protected override decimal CalculateShortfallForPickedOrder() => QuantityNotMet;

		#endregion

		#region ShortfallQuantityHelper

		WhsOrderLineShortfallQuantityHelper ShortfallQuantityHelper => shortfallQuantityHelper ?? (shortfallQuantityHelper = new WhsOrderLineShortfallQuantityHelper(Factory));
		WhsOrderLineShortfallQuantityHelper shortfallQuantityHelper;

		#endregion

		#region IsUpdateForOrderPickingStatusAllowed

		protected override bool IsUpdateForOrderPickingStatusAllowedCore() => true;

		#endregion

		#endregion

		#region Flags

		#region SettingWE_TransactionQuantityShouldUpdateWeightAndVolumeOfDocket

		protected override bool ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(WhsDocket docket) => WrapperStrategy.ShouldUpdateWeightAndVolumeOfDocketFromDocketLine;

		#endregion

		#region CanCrossDockInventory

		protected override bool CanCrossDockInventoryCore => true;

		#endregion

		#region CanGenerateChildWorkOrder

		protected override bool CanGenerateChildWorkOrderCore => IsBOMProduct;

		#endregion

		#region CanUpdateFromInventory

		protected override bool CanUpdateFromInventoryCore => base.CanUpdateFromInventoryCore && !IsDocketPicking;

		#endregion

		#region IsBOMProductPickedOnSalesOrder

		protected override bool IsBOMProductPickedOnSalesOrderCore => !IsChildComponentLine && (Product?.IsBOMProductPickedOnSalesOrder ?? false);

		#endregion

		#region IsComponentLineOnSalesOrder

		protected override bool IsComponentLineOnSalesOrderCore => IsChildComponentLine;

		#endregion

		#endregion

		#region ReduceOverpickedChildStock

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
		///		*** Tested in WhsPick.TestAutoAllocateItems_PickByBom_*() tests ***
		/// 
		/// </summary>
		internal void ReduceOverpickedChildStock(int kitQuantitiesRequired)
		{
			Argument.NotNull(Order, nameof(Order));
			if (PickableDocket.Pick == null)
			{
				throw new NotSupportedException("Cannot call ReduceOverpickedChildStock() on a WhsOrderLine whose parent Order has no Pick.");
			}

			if (kitQuantitiesRequired > 0)
			{
				var pickLineQty = PickLineQuantity;
				var parentLine = ParentLine;
				if (parentLine != null && parentLine.IsBOMProductPickedOnSalesOrder && pickLineQty > 0) // we are component used to build the product
				{
					var bomPart = parentLine.SupplierPart.BillOfMaterials.FindByComponentPKandPackType(WE_OP, WE_F3_NKPackType);
					if (bomPart != null) // can be null if BOM definition changes before pick.
					{
						var packsNeeded = bomPart.OE_ComponentQty * kitQuantitiesRequired;
						var childPart = SupplierPart;
						var packsPicked = childPart.UnitConverter.Convert(pickLineQty, childPart.OP_StockKeepingUnit, WE_F3_NKPackType);
						if (packsPicked > packsNeeded)
						{
							var quantityNeeded = childPart.UnitConverter.Convert(packsNeeded, WE_F3_NKPackType, childPart.OP_StockKeepingUnit);
							ReduceOverpickedStock(pickLineQty - quantityNeeded);
						}
					}
				}
			}
		}

		protected override bool ReduceOverpickedStockAllowed => ParentLine.IsBOMProductPickedOnSalesOrder;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region CannotUseChosenInventoryError

		protected override ErrorType CannotUseChosenInventoryError => OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsFinalisedOrCancelledOrPicked;

		#endregion

		#region CheckCustomsDataMatch

		protected override void CheckCustomsDataMatch(WhsBondedWarehouseAttribute bond)
		{
			if (Order != null && bond == null && !WE_BondedEntryKey.IsEmpty && (Order.IsAttachedToPickButNotFinalised || Order.IsFinalised))
			{
				ErrorReporter.ReportOnce("DocketLineCouldNotLoadCustomsData", "This WhsDocketLine (" + this.PK.ToString() + ") has a value in WE_BondedEntryKey yet the WhsBondedWarehouseAttribute row could NOT be loaded.");
			}
		}

		#endregion

		#region Clone

		public new WhsOrderLine Clone()
		{
			return (WhsOrderLine)base.Clone();
		}

		#endregion

		#region Delete

		/// This stuff is tested in the OrderDocketLinesGridUserControl.TestDeletePickedLineInsteadReducesUnitsToZero()
		/// because the archticture calling these methods/properties is GUI-based.

		public override bool CanDelete
		{
			// We need to allow the user to press DEL on a picked OrderLine but prevent the actual delete so that we can reduce units instead.
			get
			{
				return base.CanDelete
					&& (!IsDocketPicking || !IsInDatabase)
					&& !IsPartiallyOrFullyPickedFromPutawayLocation;
			}
		}

		protected override void OnCannotDeleteCore()
		{
			base.OnCannotDeleteCore();

			// Tested by OrderLinesGridUserControlTest.TestDelete_PickingLine_ReducesUnitsToZero and OrderLinesGridUserControlTest.TestDelete_PickedLine_DoesNothing and OrderLinesGridUserControlTest.TestDelete_PickedFTZLine_DoesNothing
			if (IsDocketPicking && WE_TransactionQuantity > 0 && !IsPartiallyOrFullyPickedFromPutawayLocation && !IsOnMultiOrderPick && !IsPickingFTZCustomsOrderLine)
			{
				WE_TransactionQuantity = 0;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (IsPartiallyOrFullyPickedFromPutawayLocation)
				{
					result = ResString.GetMultilingualString("C0BE0F69-E06E-4BC2-99B7-A671E5BAC3A2", "This line has been partially or fully picked, it cannot be deleted.");
				}
				else if (IsOnMultiOrderPick)
				{
					result = ResString.GetMultilingualString("b9ada166-4025-4012-813b-256c245d3955", "This order is attached to multi-order pick, no changes to ordered content is allowed.");
				}
				else if (IsPickingFTZCustomsOrderLine)
				{
					result = ResString.GetMultilingualString("3D886869-31C5-4F1F-B4B3-B292AD0CFB3D", "This order is allocated to a Pick for an FTZ Warehouse in a Country/Region that uses Permits, it cannot be deleted.");
				}
				else if (IsDocketPicking)
				{
					result = ResString.GetMultilingualString("342e3842-3a00-48bf-b493-c5fc94765c33",
					"This Order is already Picked. Qty Ordered was reduced to 0, and the empty line will be automatically deleted when you save the Order.");
				}
				else
				{
					result = base.ReasonForNotAbleToDelete;
				}

				return result;
			}
		}

		#endregion

		#region MatchesInventoryForCrossDocking

		public bool MatchesInventoryForCrossDocking(WhsInventoryView inventory)
		{
			bool result = false;

			if (inventory != null && !inventory.IsDamaged)
			{
				var whs = inventory.Location != null ? inventory.Location.Row.Warehouse : inventory.DocketOriginal.Warehouse;
				var docket = Docket;

				if (inventory.WI_OP == WE_OP &&
					inventory.WI_OH_Client == docket.WD_OH_Client &&
					whs.PK == docket.WD_WW_Whs)
				{
					result = AttributeComparer.CompareWithIsEmptyCheck(this, inventory) && WE_PackageGroupId.EqualsIgnoringCase(inventory.PackageGroupId)
						&& (WE_PalletID.IsEmpty || WE_PalletID.EqualsIgnoringCase(inventory.WI_PalletID));
				}
			}

			return result;
		}

		#endregion

		#region ApplyPermitResponse

		public void ApplyPermitResponse(IPermitWithdrawRequestResponse permit)
		{
			Argument.NotNull(permit, nameof(permit));

			var permitResult = permit.SuccessOrFailure;
			var rowWarningPrefix = Res.GetString("007CABA3-2E2E-48AE-8B4A-6FA4B8E2596E", "No weekly estimate found for Order Line");
			var rowWarningMessageForFailure = Res.GetString("E2D7A1F3-F313-4BEA-8A1F-B91046D63DA3", "{0} {1} ({2} {3} of Product {4})",
				rowWarningPrefix, WE_LineNo, permit.Request.Qty.ToStringTrimZeros(), ProductUQ, permit.Request.ProductCode);

			ClearRowNotificationsContaining(rowWarningPrefix);

			if (permitResult != SuccessOrFailure.Success)
			{
				rowWarningMessageForFailure = rowWarningMessageForFailure + " " + Res.GetString("951C5670-5B1D-49C4-AF0E-8E72A7033BAC", "(Date: {0}, {1}).", ZDateTime.Today.ToShortDateString(), permit.FailureReason);
				if (permitResult == SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits)
				{
					rowWarningMessageForFailure = rowWarningMessageForFailure + " "
						+ Res.GetString("9DA4CA6E-A09C-443D-8A29-0FDFDE9A503E", "However the line can be fulfilled across weekly estimates.");
				}
				AddRowWarning(rowWarningMessageForFailure);
			}
		}

		#endregion

		#region ReserveStockIfAbleTo

		public WhsPickLine ReserveStockIfAbleTo(WhsInventoryView inventory)
		{
			return ReserveStockIfAbleTo(inventory, WE_TransactionQuantity);
		}

		public WhsPickLine ReserveStockIfAbleTo(WhsInventoryView inventory, ZDecimal quantityToReserve)
		{
			var factory = inventory?.Factory;
			return ReserveStockIfAbleTo(inventory, factory, quantityToReserve);
		}

		public WhsPickLine ReserveStockIfAbleTo(WhsInventoryView inventory, BusinessObjectFactory factory, ZDecimal quantityToReserve)
		{
			if (quantityToReserve < 0)
			{
				throw new ArgumentException("Should not attempt to Reserve a Negative amount.", paramName: nameof(quantityToReserve));
			}

			WhsPickLine result = null;

			if (IsDocketUnpicked // should not reserve stock for picked orders.
				&& inventory != null
				&& !inventory.IsInTransit
				&& MatchesInventoryForCrossDocking(inventory))
			{
				var availableForCrossDock = inventory.WI_AvailableForCrossDockQuantity;
				if (availableForCrossDock > 0m)
				{
					var unreservedQuantity = WE_TransactionQuantity - WE_CrossDockQuantity;
					var quantityThatCanBeReservedForOrderLine = Math.Max(unreservedQuantity, 0m); // should never be negative, but just to be safe.
					var unitsToReserve = Math.Min(Math.Min(quantityThatCanBeReservedForOrderLine, quantityToReserve), availableForCrossDock);
					if (unitsToReserve > 0m)
					{
						var reservedLine = LoadExistingReservedPickLine(inventory, factory);
						if (reservedLine != null)
						{
							// if we have an existing divot, set its qty to max that can be reserved or qty passed in.
							reservedLine.ReservedQuantity = Math.Min(quantityToReserve, reservedLine.ReservedQuantity + unitsToReserve);
						}
						else
						{
							reservedLine = WhsPickLine.ReserveInventory_Unsafe(factory, inventory.InDocketLine.PK, PK, unitsToReserve);
						}

						result = reservedLine;
					}
				}
			}

			return result;
		}

		WhsPickLine LoadExistingReservedPickLine(WhsInventoryView inventory, BusinessObjectFactory factory)
		{
			// PickLine is unique per inventory line per orderline in DB when it is reserved.
			var query = new ZQuery();
			query.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);
			query.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, PK);
			query.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);

			return factory.LoadTop1<WhsPickLine>(query);
		}

		#endregion

		#region IsValidationEnabled

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			// SumOfUnitsMetInfo should always be validated to show possible shortfall warning even for finalized orders
			return propertyInfo == SumOfUnitsMetInfo || base.IsValidationEnabledCore(propertyInfo);
		}

		#endregion

		// interfaces

		#region ICustomsDataParent Members

		bool ICustomsDataParent.IsOutwardTypeRequired => true;

		bool ICustomsDataParent.IsCustomsDataReadOnly => true;

		bool ICustomsDataParent.IsCustomsOutwardDataReadOnly
		{
			get
			{
				var order = Order;

				if (order != null &&
					order.WarehouseOrderStatus == DocketStatus.Codes.AttachedToPick &&
					order.WD_DocketSubType == OrderType.Codes.Customs &&
					Warehouse.WW_IsVirtualWarehouse == true)
				{
					return false;
				}

				return order == null ||
						order.IsAttachedToPickButNotFinalised ||
						(order.WD_DocketSubType == OrderType.Codes.CustomsReleaseWithPermit && (order.Warehouse?.IsFTZWarehouseInCountryThatUsesPermits ?? false));
			}
		}

		bool ICustomsDataParent.IsMainCustomsDataPropertiesReadOnly => false;

		#endregion

		#region Implementation of IwhsOrderLineStrategy

		void IWhsOrderLineWrapperStrategy.WE_TransactionQuantity_SetAfter(bool valueChanged)
		{
		}

		bool IWhsOrderLineWrapperStrategy.ShouldUpdateWeightAndVolumeOfDocketFromDocketLine => !IsChildComponentLine;

		#endregion

		#region Business Object Overrides

		#region Data Refresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			TransactionQuantityBeforeRefresh = WE_TransactionQuantity;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (WE_TransactionQuantity != TransactionQuantityBeforeRefresh)
			{
				ClearPackQuantity();
			}
		}

		ZDecimal TransactionQuantityBeforeRefresh;

		#endregion

		#endregion
	}
}
