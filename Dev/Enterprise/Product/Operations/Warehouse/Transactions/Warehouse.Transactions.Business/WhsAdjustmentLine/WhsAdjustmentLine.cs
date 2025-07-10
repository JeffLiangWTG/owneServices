using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(WhsAdjustment), "Lines")]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventOverfillLocationWithUnitsCapacity, WhsValidationHelper.WhsCheckLinesDoNotExceedLocationCapacity, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_AdjustmentLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent), ValueToRunStoredProcWith = ValueVersion.Both)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsValidationHelper.WhsCheckStockOnHandIsBalanced_ForInsert, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_AdjustmentLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantityWhsAdjustmentLine_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsAdjustmentLine_DeferTriggerStrategy))]
	public sealed class WhsAdjustmentLine : WhsDocketLine, ILineWithCommittedPickLines, ISerialNumberParent
	{
		#region Constructors

		public WhsAdjustmentLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Type Decider

		public override Type DocketType
		{
			get { return typeof(WhsAdjustment); }
		}

		#endregion

		#region Related Entities

		#region Docket

		public new WhsAdjustment Docket
		{
			get { return (WhsAdjustment)base.Docket; }
		}

		#endregion

		#region InventoryCommitter

		WhsInventoryCommitter<WhsAdjustmentLine> InventoryCommitter
		{
			get { return inventoryCommitter ?? (inventoryCommitter = new WhsInventoryCommitter<WhsAdjustmentLine>(this)); }
		}

		WhsInventoryCommitter<WhsAdjustmentLine> inventoryCommitter;

		#endregion

		#region BOMComponentLinks

		protected override IEnumerable<WhsBOMInventoryPivot> BOMComponentLinksCore => Enumerable.Empty<WhsBOMInventoryPivot>();

		#endregion

		#endregion

		#region Overrides

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
		}

		#endregion

		protected override string DefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Available; }
		}

		#region IsInventoryLineCore

		protected override bool IsInventoryLineCore
		{
			get { return IsAdjustmentIn; }
		}

		#endregion

		#region SetDefaultValues

		protected override string DocketLineType => CodeLists.DocketType.Codes.Adjustment;

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsInDatabase)
			{
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsAdjustment>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
			}
			base.Delete();
		}

		#endregion

		#endregion

		#region SupportsHoldCodeChange

		protected override bool SupportsHoldCodeChange
		{
			get { return true; }
		}

		#endregion

		#region Properties

		// persistent

		#region WE_F3_NKPackType

		protected override ZString GetPackTypeFromProductParams(WhsProductParamsByWhsAndClient productParams)
		{
			return !productParams.W3_F3_NKReleasedPackType.IsEmpty ? productParams.W3_F3_NKReleasedPackType : productParams.W3_F3_NKReceivedPackType;
		}

		#endregion

		#region WE_ReasonCode

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member StandardReadOnly")]
		[ReadOnlyMember("StandardReadOnly")]
		[List("Lookups.AdjustmentReasonCodes")]
		public override ZString WE_ReasonCode
		{
			get { return base.WE_ReasonCode; }
			set { base.WE_ReasonCode = value; }
		}

		#endregion

		#region WE_ReasonDescription

		public ZString WE_ReasonDescription
		{
			get { return Lookups.AdjustmentReasonCodes.GetDescriptionFromCode(WE_ReasonCode); }
		}

		#endregion

		#region WE_OriginalInventoryStatus

		[List("Lookups.InventoryStatuses")]
		public override ZString WE_OriginalInventoryStatus
		{
			get { return base.WE_OriginalInventoryStatus; }
			set { base.WE_OriginalInventoryStatus = value; }
		}

		#endregion

		#region WE_CurrentInventoryStatus

		[List("Lookups.InventoryStatuses")]
		public override ZString WE_CurrentInventoryStatus
		{
			get { return base.WE_CurrentInventoryStatus; }
			set { base.WE_CurrentInventoryStatus = value; }
		}

		#endregion

		#region WE_TransactionQuantity

		public override ZDecimal WE_TransactionQuantity
		{
			get { return base.WE_TransactionQuantity; }
			set
			{
				base.WE_TransactionQuantity = value;
				if (IsAdjustmentIn)
				{
					ResetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed();
				}
			}
		}

		#endregion

		#region WE_PalletID

		public override ZString WE_PalletID
		{
			get => base.WE_PalletID;
			set
			{
				base.WE_PalletID = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_WL();
				}
			}
		}

		#endregion

		#region WE_WL

		public override ZGuid WE_WL
		{
			get => base.WE_WL;
			set
			{
				base.WE_WL = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PalletID();
				}
			}
		}

		#endregion

		#region DocketLineCanCalculateExpiryDateFromPackingDate

		protected override bool DocketLineCanCalculateExpiryDateFromPackingDate => IsAdjustmentIn;

		#endregion

		#region IsAdjustmentOut

		public bool IsAdjustmentOut => WE_TransactionQuantity < 0;

		#endregion

		#region IsAdjustmentIn

		public bool IsAdjustmentIn => WE_TransactionQuantity > 0;

		#endregion

		#region DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity

		public bool DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity { get; internal set; }

		#endregion

		#region ResetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed

		void ResetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed()
		{
			var product = Product;
			var docket = Docket;
			if (product != null && docket != null)
			{
				var client = docket.Client;
				if (client != null)
				{
					if (product.IsPartAttributeAJulianBatchNumberAndUsed(client, 1))
					{
						WE_PackingDate = product.GetPackingDateFromJulianBatchNumber(client, docket.Warehouse, WE_PartAttrib1);
						WE_ExpiryDate = product.CalculateExpiryDate(client, docket.Warehouse, WE_PartAttrib1);
					}
					else if (product.IsPartAttributeAJulianBatchNumberAndUsed(client, 2))
					{
						WE_PackingDate = product.GetPackingDateFromJulianBatchNumber(client, docket.Warehouse, WE_PartAttrib2);
						WE_ExpiryDate = product.CalculateExpiryDate(client, docket.Warehouse, WE_PartAttrib2);
					}
					else if (product.IsPartAttributeAJulianBatchNumberAndUsed(client, 3))
					{
						WE_PackingDate = product.GetPackingDateFromJulianBatchNumber(client, docket.Warehouse, WE_PartAttrib3);
						WE_ExpiryDate = product.CalculateExpiryDate(client, docket.Warehouse, WE_PartAttrib3);
					}
				}
			}
		}

		#endregion

		// calculated

		#region Readonly

		#region IsJobFinalisedOrCanceledReadOnly

		protected override bool IsJobFinalisedOrCancelledReadOnly
		{
			get { return base.IsJobFinalisedOrCancelledReadOnly || IsNewOwnershipAdjustmentChild; }
		}

		#endregion

		#region IsNewOwnershipAdjustmentChild

		bool IsNewOwnershipAdjustmentChild
		{
			get
			{
				var docket = Docket;
				return docket == null || docket.IsNewOwnershipAdjustmentChild;
			}
		}

		#endregion

		#endregion

		#region CommittedQuantity

		[ResourceStringData("WhsAdjustmentLine|CommittedQuantity", Caption = "Committed Qty")]
		public ZDecimal CommittedQuantity
		{
			get { return this.GetQtyCommittedToThisLine(); }
		}

		#endregion

		#endregion

		#region Validation

		#region ShouldSuppressDateRangeValidationErrors

		protected override bool ShouldSuppressDateRangeValidationErrors => true;

		#endregion

		protected override void BeforeRunPreSaveValidationCore()
		{
			base.BeforeRunPreSaveValidationCore();
			CommitInventory();
		}

		public new WhsAdjustmentLineValidation Validation
		{
			get { return (WhsAdjustmentLineValidation)base.Validation; }
		}

		protected override WhsDocketLineValidation GetNewValidation()
		{
			switch (CountryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return new WhsAdjustmentLineValidationUS(this);

				default:
					return new WhsAdjustmentLineValidation(this);
			}
		}

		#endregion

		#region Find Attributes

		protected override void UseChosenInventoryRowFindAttributesCore(WhsInventoryView inventory)
		{
			base.UseChosenInventoryRowFindAttributesCore(inventory);
			WE_WL = inventory.WI_WL;
			WE_PalletID = inventory.WI_PalletID;
		}

		#endregion

		#region Lookups

		public new WhsAdjustmentLineLookups Lookups
		{
			get { return (WhsAdjustmentLineLookups)base.Lookups; }
		}

		protected override WhsDocketLineLookups GetNewLookups()
		{
			return new WhsAdjustmentLineLookups(this);
		}

		#endregion

		#region CommitInventory

		public void CommitInventory()
		{
			if (!IsDocketFinalised)
			{
				if (IsAdjustmentOut) // committing inventory is only required for adjusting out
				{
					InventoryCommitter.CommitInventory();
				}
				// nothing to commit
				else
				{
					PickLines.DeleteAll();
				}
			}
		}

		#endregion

		#region UncommitOverPickedOrNotMatchingInventory

		public void UncommitOverPickedOrNotMatchingInventory()
		{
			if (!IsDocketFinalised)
			{
				InventoryCommitter.UncommitOverPickedOrNonMatchingInventory();
			}
		}

		#endregion

		// interfaces

		#region ILineWithCommittedPickLines Members

		ICommittedInventoryStrategy ILineWithCommittedPickLines.CommittedStrategy
		{
			get { return InventoryCommitter; }
		}

		ZGuid ILineWithCommittedPickLines.ParentDocketPK
		{
			get { return WE_WD; }
			set { WE_WD = value; }
		}

		WhsLocation ILineWithCommittedPickLines.LocationToCommit
		{
			get { return Location; }
		}

		ZShort ILineWithCommittedPickLines.LineNo
		{
			get { return WE_LineNo; }
		}

		ZString ILineWithCommittedPickLines.PackageGroupID
		{
			get { return WE_PackageGroupId; }
		}

		ZString ILineWithCommittedPickLines.PalletIDToCommit
		{
			get { return WE_PalletID; }
		}

		string ILineWithCommittedPickLines.Noun
		{
			get { return Res.GetString("56d0e555-6dbd-49b6-8689-dbb28a59e15f", "adjustment"); }
		}

		string ILineWithCommittedPickLines.Verb
		{
			get { return Res.GetString("9aa63cea-03c0-4d3f-9947-25eec6b6fdcd", "adjust"); }
		}

		ZDecimal ILineWithCommittedPickLines.PerPackageQty
		{
			set { WE_PerPackageQty = value; }
		}

		/// <summary>
		/// We return the opposite sign as committing logic works off positive values
		/// and we only commit negative adjustment lines (adjust out).
		/// </summary>
		ZDecimal ILineWithCommittedPickLines.TransactionQty
		{
			get { return -WE_TransactionQuantity; }
			set { WE_TransactionQuantity = -value; }
		}

		bool ILineWithCommittedPickLines.IsInTransit => false;

		#endregion

		#region ILineWithInventory Members

		ZGuid ILineWithInventory.ProductPK
		{
			get { return WE_OP; }
		}

		WhsDocket ILineWithInventory.ParentDocket
		{
			get { return Docket; }
		}

		ZDateTimeOffset ILineWithInventory.ArrivalDate
		{
			get { return WE_AdjustmentArrivalDate; }
		}

		ZString ILineWithInventory.PackType
		{
			get { return WE_F3_NKPackType; }
		}

		ZString ILineWithInventory.TransactionInventoryStatus
		{
			get { return WE_OriginalInventoryStatus; }
			set { WE_OriginalInventoryStatus = value; }
		}

		ZString ILineWithInventory.TransactionInventoryHeldCode
		{
			get { return WE_WHC_NKOriginalInventoryHeldCode; }
		}

		ZGuid ILineWithInventory.TransactionLocation
		{
			get { return WE_WL; }
		}

		ZString ILineWithInventory.TransactionPalletID
		{
			get { return WE_PalletID; }
		}

		bool ILineWithInventory.IsFinalising
		{
			get { return IsDocketFinalising; }
		}

		bool ILineWithInventory.CanCreateInventory
		{
			get { return IsDocketFinalising; }
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public override bool IsRegisteredForUniqueSerialNumberChecking => IsAdjustmentIn;

		protected override bool IsInventoryAdjustedOutOnSiblingsCore(WhsInventoryView inventory)
		{
			var result = false;
			var docket = Docket;
			if (docket != null && inventory != null)
			{
				var siblingLinesMatchingInventory = docket.Lines.SelectMany(l => l.PickLines).Where(pl => pl.WZ_WE_InventoryLine == inventory.PK && !pl.IsPicked);
				if (siblingLinesMatchingInventory.Any())
				{
					result = siblingLinesMatchingInventory.Sum(pl => pl.WZ_Units) == inventory.WI_TotalUnits;
				}
			}

			return result;
		}

		#endregion

		#region SerialNumbers

		[ChildEditable]
		public WhsSerialNumberPivotCollection SerialNumbers
		{
			get
			{
				if (serialNumbers == null)
				{
					serialNumbers = new WhsSerialNumberPivotCollection(this);
					RegisterEditableChildObject(serialNumbers);
				}
				return serialNumbers;
			}
		}

		WhsSerialNumberPivotCollection serialNumbers;

		[ChildEditableTestExclude]
		public WhsSerialNumberPivotSelectorCollection SerialNumberSelectors
		{
			get
			{
				if (serialNumberSelectors == null)
				{
					if (!IsAdjustmentOut || !HasSerialNumber)
					{
						serialNumberSelectors = WhsSerialNumberPivotSelectorCollection.Empty;
					}
					else
					{
						CommitInventory();

						serialNumberSelectors = new WhsSerialNumberPivotSelectorCollection(this, !IsInDatabase);
						serialNumberSelectors.SelectionChanged += SerialNumbers_SelectionChanged;
					}

					RegisterEditableChildObject(serialNumbers);
				}

				return serialNumberSelectors;
			}
		}

		void SerialNumbers_SelectionChanged(object sender, EventArgs e)
		{
			var selector = (ISerialNumberPivotAssigner)sender;
			if (selector.Selected)
			{
				selector.RemoveSerialNumberSelection();
				AdjustQuantity(increase: true);
			}
			else
			{
				selector.SelectSerialNumber();
				AdjustQuantity(increase: false);
			}

			void AdjustQuantity(bool increase)
			{
				var unit = increase ? 1 : -1;
				WE_TransactionQuantity += unit;
				selector.PickLineforPicking.WZ_Units -= unit;
			}
		}

		WhsSerialNumberPivotSelectorCollection serialNumberSelectors;

		public bool HasSerialNumber => WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value && IsSerialNumberUsed;

		#region ISerialNumberParent

		ZGuid ISerialNumberParent.ProductPK => WE_OP;

		bool ISerialNumberParent.SerialNumberReadOnly => SerialNumberReadOnly;

		public bool IsSerialNumberUsed => Product?.IsSerialNumberUsed(Docket.Client) ?? false;

		bool ISerialNumberParent.IsAllowedToCreateOriginalSerialNumberRecord => IsAdjustmentIn;

		bool ISerialNumberParent.IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => Docket.IsSerialNumberAlreadyInUse(pivot);

		#endregion

		#endregion

		#region Tests
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (WE_WL.IsEmpty)
			{
				var docket = Docket;
				var warehouse = docket.Warehouse;
				if (docket != null && warehouse?.DefaultOutboundDockDoorLocation != null)
				{
					WE_WL = warehouse.DefaultOutboundDockDoorLocation.PK;
				}
				else if (docket != null && warehouse?.DefaultOutboundDockDoorLocation == null)
				{
					var randomizer = new Random(warehouse.PK.GetHashCode());
					var whsRow = warehouse.Rows.AddNew();
					whsRow.WR_WW_Whs = warehouse.PK;
					whsRow.WR_Name = randomizer.Next(1000).ToString(Culture.Invariant);
					whsRow.WR_Columns = 1;
					whsRow.WR_Levels = 1;
					((IWhsWarehouseInternals)warehouse).GenerateLocations();
					WE_WL = warehouse.DefaultLocation.PK;
				}

				base.FillWithValidTestDataCore(kind, propertyPath);
			}

			WE_TransactionQuantity = 1m;
		}

#endif
		#endregion
	}
}
