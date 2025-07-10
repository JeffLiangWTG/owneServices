using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.US;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ActionFieldFollow(false)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_TransactionAndPickedQtyIsCorrect, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect, WhsPickLineSchema.Constants.WZ_WE_TransactionLine, typeof(IWhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Both)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventUnPickedPickLinesOnFinalisedJobs, WhsValidationHelper.WhsCheckFinalisedJobsWithUnPickedPicklines, WhsPickLineSchema.Constants.PK, typeof(IWhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventOverCommitOfStockViaPickLine, WhsValidationHelper.WhsCheckPickLinesAreNotOverCommitting, WhsPickLineSchema.Constants.WZ_WE_InventoryLine, typeof(IOverCommitPickLinesDeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsPickLineSchema.Constants.WZ_WE_InventoryLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_PickLineInsertUpdate), ValueToRunStoredProcWith = ValueVersion.Both)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsPickLineSchema.Constants.WZ_WE_InventoryLine, typeof(IDeferTriggerOnDeleteConditionStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits, WhsValidationHelper.WhsCheckSerialNumberMatchWithPickLineUnits, WhsPickLineSchema.Constants.PK, typeof(IWhsCheckSerialNumberMatchWithPickLineUnits_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory, WhsValidationHelper.WhsCheckTransferSerialNumberMatchWithnventory, WhsPickLineSchema.Constants.PK, typeof(IWhsCheckTransferSerialNumberMatchWithnventory_DeferTriggerStrategy))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
	public class WhsPickLine : AutoWhsPickLine, IEmptyLocationAfterPickFinalisation, Integration.IWhsPickLine, IWhsPickLineInternals, ICartonisableItem, IReducibleItem
	{
		public const string PreventOverCommitOfStockViaPickLineTriggerID = "Over-pick attempt.";
		public const string PreventPickingReservedLinesOnUnAllocatedJobsTriggerID = "Attempt to pick Reserved PickLines that are not allocated to a Pick.";
		public const string PreventReassigningPickLineTriggerID = "Attempt to re-assigning pick line that is being picked.";
		public const string PreventPickLineFromLinkingToIncorrectTransactionLine = "Attempt to link Pick Line to incorrect Transaction Line.";
		public const string PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID = "Attempt to change critical data for a finalized Pick Line.";
		public const string PreventTransactionsPickingFromDifferentWarehouseTriggerID = "Attempt to pick from a Warehouse different from where the transaction was registered.";
		public const string PreventAttemptToPutStockOnHandOutOfBalance = "Attempt to put Stock On Hand out of balance.";
		public const string PreventAttemptToAlllocateUnallocateableInventory = "Attempt to link Pick Line to an unallocateable inventory.";
		public const string PreventDeleteOfFinalisedPickLineTriggerTriggerID = "Attempt to delete dbo.WhsPickLine on a FIN Pick.";

		public new abstract class Schema : AutoWhsPickLine.Schema
		{
			public const string ConsigneeNameOrPK = "ConsigneeNameOrPK";
			public const string ConsigneeFieldType = "ConsigneeFieldType";
			public const string ReservedQuantity = "ReservedQuantity";
		}

		#region Constructors

		public WhsPickLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WZ_Units), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WZ_WE_InventoryLine), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WZ_WE_OriginalPickedInventoryLine), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WZ_IsPicking), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Business Object Overrides

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsPickLineFetchStrategy(this);
		}

		#endregion

		#region LightValidation

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			RemoveReservedQtyOnPickedPickLine();

			// Need to do this in OnFactorySaving() because otherwise we don't call OnFactorySaving() for the newly created Transfers
			CreateDockDoorTransfers();
		}

		#region RemoveReservedQtyOnPickedPickLine

		void RemoveReservedQtyOnPickedPickLine()
		{
			if (IsReserveLine && IsPickedInMemory && !IsNotAllocatedToPick)
			{
				isReserveLine = null;
				base.WZ_OriginalReservedQty = 0m;
			}
		}

		#endregion

		#region CreateDockDoorTransfers

		void CreateDockDoorTransfers()
		{
			ObjectFactory.Get<IOutboundDockDoorTransferCreator>().CreateOutboundDockDoorTransfer(this);
		}

		#endregion

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsPickedInMemory && WZ_WE_InventoryLine.IsValid && !IsFinalised)
			{
				var inventoryLine = Factory.Load<WhsDocketLine>(WZ_WE_InventoryLine);
				if (inventoryLine.WE_WL.IsValid)
				{
					var location = Factory.Load<WhsLocation>(inventoryLine.WE_WL);
					location.UpdateWLV_LastAllocatedOrChangedDateUtc();
				}
			}
			UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(Factory, this);
		}

		#endregion

		#endregion

		#region Related Entities

		#region Pick

		public WhsPick Pick
		{
			get
			{
				var pickableDocketLine = DocketLine as WhsPickableDocketLine;
				if (pickableDocketLine != null)
				{
					var pickableDocket = pickableDocketLine.PickableDocket;
					if (pickableDocket != null)
					{
						return pickableDocket.Pick;
					}
				}
				return null;
			}
		}

		#endregion

		#region Inventory

		/// <summary>
		/// This is the Inventory that is committed to the pick.
		/// </summary>
		public WhsInventoryView Inventory
		{
			get
			{
				var inventoryLine = InventoryLine;
				return inventoryLine != null ? (WhsInventoryView)inventoryLine.Inventory.FirstOrDefault() : null;
			}
		}

		#endregion

		#region InventoryLine

		/// <summary>
		/// This is the InventoryLine that is committed to the pick.
		/// </summary>
		public WhsDocketLine InventoryLine
		{
			get { return Factory.Load<WhsDocketLine>(WZ_WE_InventoryLine); }
		}

		#endregion

		#region DocketLine

		/// <summary>
		/// This is the Work Order, Order or Transfer Line that the Committed Inventory is allocated to.
		/// </summary>
		public WhsDocketLine DocketLine
		{
			get { return Factory.Load<WhsDocketLine>(WZ_WE_TransactionLine); }
		}

		#endregion

		#region Product

		public WhsProduct Product
		{
			get { return InventoryLine?.Product; }
		}

		#endregion

		#region SupplierPart

		public OrgSupplierPart SupplierPart
		{
			get
			{
				var inventory = InventoryLine;
				return (inventory != null) ? inventory.SupplierPart : null;
			}
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region WZ_Units

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDecimal WZ_Units
		{
			get { return base.WZ_Units; }
			set
			{
				if (IsPickedFromPutawayLocation && !AllowChangingPickedQuantitySemaphore.IsSuspended)
				{
					throw new InvalidOperationException("Cannot change quantity on a Picked PickLine (i.e. you cannot change the amount that was Picked).");
				}

				base.WZ_Units = value;
			}
		}

		#endregion

		#region WZ_PickedDateTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WZ_PickedDateTime
		{
			get { return base.WZ_PickedDateTime; }
			set
			{
				if (IsFinalised && (!AllowUnpickingPickLineWithNoStockChange.IsSuspended || !value.IsEmpty))
				{
					throw new InvalidOperationException("You cannot change the Picked Time of a PickLine that is already Picked in the DB.");
				}

				var previousValue = WZ_PickedDateTime;
				base.WZ_PickedDateTime = value.IsValid ? ZDateTimeOffset.TruncateMilliseconds(value) : value;

				if (!value.IsEmpty)
				{
					WZ_IsPicking = false;
				}

				if (!AllowUnpickingPickLineWithNoStockChange.IsSuspended)
				{
					SetPickerToCurrentUserIfPicked();
					UpdateStockIfPickingOrUnpicking(previousValue);
					UpdateOrderPickingStatus(previousValue);
					Pick?.ReCalculatePercentageComplete();
				}
			}
		}

		void SetPickerToCurrentUserIfPicked()
		{
			if (IsPicked && WZ_GS_NKAssignedTo.IsEmpty)
			{
				WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#region UpdateStockIfPickingOrUnpicking

		void UpdateStockIfPickingOrUnpicking(ZDateTimeOffset previousPickedTime)
		{
			if (!IsFinalised) // Cannot change stock for a Finalised PickLine
			{
				bool wasPreviouslyPicked = previousPickedTime.IsValid;
				if (wasPreviouslyPicked && !IsPicked) // we are unpicking
				{
					var inventory = InventoryLine;
					if (inventory != null)
					{
						inventory.WE_StockOnHand += WZ_Units;
						UpdateChangeLastInventoryChangeDate(inventory, inventory.Location);
					}
				}
				else if (!wasPreviouslyPicked && IsPicked && (!DoNotReduceStockWhenPicking || IsUnfinalisedPutawayTransferLine)) // we are picking and we are allowed to reduce stock
				{
					ReduceStockIfPickLineIsPicked();
				}
			}
		}

		void UpdateChangeLastInventoryChangeDate(WhsDocketLine inventory, WhsLocation location)
		{
			if (inventory.WE_StockOnHandInfo.HasChanges)
			{
				location?.OnStockOnHandChanged();
			}
			else
			{
				location?.RollbackChangeLastInventoryChangeDate();
			}
		}

		void ReduceStockIfPickLineIsPicked()
		{
			var inventory = InventoryLine;
			if (inventory != null)
			{
				var newTotalUnits = inventory.WE_StockOnHand - WZ_Units;
				if (newTotalUnits >= 0)
				{
					inventory.WE_StockOnHand = newTotalUnits;

					var location = inventory.Location;
					UpdateChangeLastInventoryChangeDate(inventory, location);

					if (WZ_WE_OriginalPickedInventoryLine.IsEmpty && (location?.IsFixedLocation ?? false))
					{
						RegisterInventoryForReplenishmentNudging();
					}
				}
				else
				{
					using (((IWhsPickLineInternals)this).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
					{
						WZ_PickedDateTime = ZDateTimeOffset.Empty; // No longer valid to Pick
					}

					// We make the pick line units equal the inventory sum because the inventory is overcommitted.
					// Since they cannot save the job in this scenario, and since the inventory is overcommitted anyway
					// there should be no issue with reducing the committed quantity.
					WZ_Units = inventory.WE_StockOnHand;
				}
			}

			void RegisterInventoryForReplenishmentNudging()
			{
				var afterCommittedService = Factory.ServiceContainer.GetAfterCommittedService<WhsPickLineInPickFaceAfterPickingService>();
				if (afterCommittedService == null)
				{
					afterCommittedService = new WhsPickLineInPickFaceAfterPickingService(Factory);
					Factory.ServiceContainer.AddAfterCommittedService(afterCommittedService);
				}
				afterCommittedService.RegisterInventory(WZ_WE_InventoryLine);
			}
		}

		void UpdateOrderPickingStatus(ZDateTimeOffset previousPickedTime)
		{
			var pickableDocketLine = DocketLine as WhsPickableDocketLine;
			if (pickableDocketLine != null
				&& pickableDocketLine.IsUpdateForOrderPickingStatusAllowed())
			{
				var pickableDocket = pickableDocketLine.PickableDocket;
				var wasPreviouslyPicked = previousPickedTime.IsValid;
				if (wasPreviouslyPicked
					&& !IsPicked  // we are unpicking
					&& ((ZString)pickableDocket.WD_DocketStatusInfo.OriginalValue) != DocketStatus.Codes.Picking
					&& NoPickLinesOnOrderArePicked(pickableDocket))
				{
					pickableDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				}
				else if (
					!wasPreviouslyPicked
					&& IsPicked // we are picking
					&& pickableDocket.WD_DocketStatus != DocketStatus.Codes.Picking)
				{
					pickableDocket.WD_DocketStatus = DocketStatus.Codes.Picking;
				}
			}
		}

		bool NoPickLinesOnOrderArePicked(WhsPickableDocket pickableDocket)
		{
			var pickableDocketLinePKs = pickableDocket.AllLines.Select(l => l.PK);
			var localLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, pickableDocketLinePKs);
			localLineQuery.FetchOnlyFromLocalCache = true;
			var pickLinesOnOrder = Factory.Load<WhsPickLine>(localLineQuery);
			return pickLinesOnOrder.All(pl => !pl.IsPicked);
		}

		#endregion

		#region PickWithoutReducingStock_DoNotUse

		void IWhsPickLineInternals.PickWithoutReducingStock_DoNotUse(ZDateTimeOffset pickTime)
		{
			if (!pickTime.IsValid)
			{
				throw new ArgumentException("You must supply a valid Pick Time.", nameof(pickTime));
			}

			try
			{
				DoNotReduceStockWhenPicking = true;
				WZ_PickedDateTime = pickTime;
			}
			finally
			{
				DoNotReduceStockWhenPicking = false;
			}
		}

		bool DoNotReduceStockWhenPicking;

		#endregion

		bool IsUnfinalisedPutawayTransferLine => DocketLine is WhsTransferLine transferLine && !transferLine.IsFinalised && transferLine.IsPutawayTransferLine;

		#endregion

		#region WZ_OriginalReservedQty

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		[ActionField(ReadOnly = true)]
		public sealed override ZDecimal WZ_OriginalReservedQty
		{
			get { return base.WZ_OriginalReservedQty; }
			set { throw new NotSupportedException("Do not call the setter of WZ_OriginalReservedQty directly."); } // TestSettingValueCallsRefreshBinding() ignores a property if it throws a NotSupportedException.
		}

		#endregion

		#region WZ_WE_InventoryLine

		[ReadOnly(true)]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject InventoryLine (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject("InventoryLine")]
		public override ZGuid WZ_WE_InventoryLine
		{
			get { return base.WZ_WE_InventoryLine; }
			set
			{
				if (!WZ_PickedDateTime.IsEmpty)
				{
					throw new InvalidOperationException("You cannot change inventory for a picked pickline.");
				}
				base.WZ_WE_InventoryLine = value;
			}
		}

		#endregion

		#region WZ_WE_TransactionLine

		[ReadOnly(true)]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject DocketLine (WhsDocketLine) is an abstract class")]
		[RelatedBusinessObject("DocketLine")]
		public override ZGuid WZ_WE_TransactionLine
		{
			get { return base.WZ_WE_TransactionLine; }
			set { base.WZ_WE_TransactionLine = value; }
		}

		#endregion

		#region WZ_WE_OriginalOrderLine

		[ReadOnly(true)]
		public override ZGuid WZ_WE_OriginalOrderLine
		{
			get => base.WZ_WE_OriginalOrderLine;
			set => base.WZ_WE_OriginalOrderLine = value;
		}

		#endregion

		#region WZ_WE_OriginalPickedInventoryLine

		[ReadOnly(true)]
		public override ZGuid WZ_WE_OriginalPickedInventoryLine
		{
			get { return base.WZ_WE_OriginalPickedInventoryLine; }
			set { base.WZ_WE_OriginalPickedInventoryLine = value; }
		}

		#endregion

		#region WZ_P9_Task 

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WZ_P9_Task
		{
			get { return base.WZ_P9_Task; }
			set { base.WZ_P9_Task = value; }
		}

		#endregion

		#region WZ_VerifiedEmpty

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_VerifiedEmpty
		{
			get { return base.WZ_VerifiedEmpty; }
			set { base.WZ_VerifiedEmpty = value; }
		}

		#endregion

		#region WZ_F3_NKAllocatedPackType

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_F3_NKAllocatedPackType
		{
			get { return base.WZ_F3_NKAllocatedPackType; }
			set { base.WZ_F3_NKAllocatedPackType = value; }
		}

		#endregion

		#region WZ_GS_NKAssignedTo

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_GS_NKAssignedTo
		{
			get { return base.WZ_GS_NKAssignedTo; }
			set { base.WZ_GS_NKAssignedTo = value; }
		}

		#endregion

		#region WZ_IsPicking

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool WZ_IsPicking
		{
			get { return base.WZ_IsPicking; }
			set { base.WZ_IsPicking = value; }
		}

		#endregion

		#region WZ_ReleaseCapturedPartAttrib1

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_ReleaseCapturedPartAttrib1
		{
			get { return base.WZ_ReleaseCapturedPartAttrib1; }
			set { base.WZ_ReleaseCapturedPartAttrib1 = value; }
		}

		#endregion

		#region WZ_ReleaseCapturedPartAttrib2

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_ReleaseCapturedPartAttrib2
		{
			get { return base.WZ_ReleaseCapturedPartAttrib2; }
			set { base.WZ_ReleaseCapturedPartAttrib2 = value; }
		}

		#endregion

		#region WZ_ReleaseCapturedPartAttrib3

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_ReleaseCapturedPartAttrib3
		{
			get { return base.WZ_ReleaseCapturedPartAttrib3; }
			set { base.WZ_ReleaseCapturedPartAttrib3 = value; }
		}

		#endregion

		#region WZ_ReleaseCapturedSerialNumber

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WZ_ReleaseCapturedSerialNumber
		{
			get { return base.WZ_ReleaseCapturedSerialNumber; }
			set { base.WZ_ReleaseCapturedSerialNumber = value; }
		}

		#endregion

		#region ReleaseCaptured

		public bool HasReleaseCapturedAttribs => !(WZ_ReleaseCapturedPartAttrib1.IsEmpty && WZ_ReleaseCapturedPartAttrib2.IsEmpty && WZ_ReleaseCapturedPartAttrib3.IsEmpty && WZ_ReleaseCapturedSerialNumber.IsEmpty);

		public ZDecimal ReleaseCapturedQty => HasReleaseCapturedAttribs ? WZ_Units : ZDecimal.Zero;

		public ZDecimal UnreleaseCapturedQty => HasReleaseCapturedAttribs ? ZDecimal.Zero : WZ_Units;

		public void SetReleaseCapturedAttributes(ZString attrib1, ZString attrib2, ZString attrib3, ZString serialNumber)
		{
			WZ_ReleaseCapturedPartAttrib1 = attrib1;
			WZ_ReleaseCapturedPartAttrib2 = attrib2;
			WZ_ReleaseCapturedPartAttrib3 = attrib3;
			WZ_ReleaseCapturedSerialNumber = serialNumber;
		}

		public void ClearReleaseCapturedAttributes()
		{
			SetReleaseCapturedAttributes(string.Empty, string.Empty, string.Empty, string.Empty);
		}

		#endregion

		// calculated

		#region ConsigneeNameOrPK

		[List("DocketLine.Docket.Lookups.Consignees")]
		public ZString ConsigneeNameOrPK
		{
			get
			{
				var pickableDocketLine = DocketLine as WhsPickableDocketLine;
				var pickableDocket = pickableDocketLine != null ? pickableDocketLine.PickableDocket : null;
				return (pickableDocket != null) ? pickableDocket.ConsigneeNameOrPK : ZString.Empty;
			}
		}

		public ZPropertyInfo ConsigneeNameOrPKInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeNameOrPK); }
		}

		#endregion

		#region ConsigneeFieldType

		public ZString ConsigneeFieldType
		{
			get
			{
				var pickableDocketLine = DocketLine as WhsPickableDocketLine;
				var pickableDocket = pickableDocketLine != null ? pickableDocketLine.PickableDocket : null;
				return (pickableDocket != null) ? pickableDocket.ConsigneeFieldType : (ZString)nameof(FieldType.Text);
			}
		}

		#endregion

		#region ProductCode

		public ZString ProductCode
		{
			get { return SupplierPart?.OP_PartNum ?? ZString.Empty; }
		}

		#endregion

		#region InventoryLineForAvailableInventory

		public WhsDocketLine InventoryLineForAvailableInventory => Factory.Load<WhsDocketLine>(InventoryLinePKForAvailableInventory);

		#endregion

		#region InventoryLinePKForAvailableInventory

		public ZGuid InventoryLinePKForAvailableInventory => WZ_WE_OriginalPickedInventoryLine.IsEmpty ? WZ_WE_InventoryLine : WZ_WE_OriginalPickedInventoryLine;

		#endregion

		#region ReservedQuantity

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("WhsPickLine|ReservedQuantity", Caption = "Cross Dock Quantity", ShortCaption = "Quantity")]
		public ZDecimal ReservedQuantity
		{
			get { return WZ_Units; }
			set
			{
				if (IsReserveLine && ReservedQuantity != value && IsNotAllocatedToPick)
				{
					WZ_Units = value;

					if (!IsInDatabase)
					{
						// We set the original reserved value so that when a user cancels a pick, we can attempt to re-reserve the original stock.
						// Do not call the overriden setter as it will throw an exception.
						base.WZ_OriginalReservedQty = WZ_Units;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateReservedQuantity();
					}

					ReservedQuantityInfo.RefreshBinding();
				}
			}
		}

		bool IsNotAllocatedToPick
		{
			get
			{
				var pickableDocketLine = DocketLine as WhsPickableDocketLine;
				return pickableDocketLine != null && pickableDocketLine.IsDocketUnpicked;
			}
		}

		public ZPropertyInfo ReservedQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.ReservedQuantity); }
		}

		#endregion

		#region WZ_UnitsUQ

		[MaxLength(3)]
		public ZString WZ_UnitsUQ
		{
			get { return SupplierPart?.OP_StockKeepingUnit ?? Constants.PkgUnit.Unit; }
		}

		#endregion

		#endregion

		#region Flags

		#region IsPickedFromPutawayLocation

		public bool IsPickedFromPutawayLocation => IsPicked || WZ_WE_OriginalPickedInventoryLine.IsValid;

		#endregion

		#region IsFinalised

		/// <summary>
		/// A PickLine is Finalised if it is in the database *and* has a picked date time.
		/// 
		/// There is one case where we consider the PickLine finalised, even if it is *not* in the DB. This is the case
		/// where we split an already (in DB) finalised PickLine. Even though the newly split PickLine is not yet saved, its
		/// source was a Finalised PickLine, so we must treat the newly split PickLine as also finalised.
		/// </summary>
		public bool IsFinalised => IsInDatabase ? WZ_PickedDateTimeInfo.OriginalValue.IsValid : IsFinalisedOverride;

		bool IsFinalisedOverride
		{
			get { return isFinalisedOverride; }
			set
			{
				if (IsInDatabase && value)
				{
					throw new InvalidOperationException("Cannot Override Finalised Status of PickLine if it is in the DB.");
				}

				isFinalisedOverride = value;
			}
		}

		bool isFinalisedOverride;

		#endregion

		#region IsPicked

		/// <summary>
		/// This will return true if the PickedDateTime is set, however to know whether the stock this PickLine represents
		/// is physically Picked from its Location you should instead use 'IsPickedFromPutawayLocation'.
		/// </summary>
		public bool IsPicked => WZ_PickedDateTime.IsValid;

		#endregion

		#region IsPickedInMemory

		public bool IsPickedInMemory => IsPicked && (!IsInDatabase || WZ_PickedDateTimeInfo.HasChanges);

		#endregion

		#region WasPickedInMemoryForStocktake

		public bool WasPickedInMemoryForStocktake
		{
			get
			{
				return !IsInDatabase
				|| (WZ_WE_OriginalPickedInventoryLine.IsEmpty ? WZ_PickedDateTimeInfo.HasChanges : WZ_WE_OriginalPickedInventoryLineInfo.HasChanges);
			}
		}

		#endregion

		#region IsPickFinalised

		public bool IsPickFinalised
		{
			get
			{
				var pick = Pick;
				return pick != null && pick.IsFinalised;
			}
		}

		#endregion

		#region IsPickCancelled

		public bool IsPickCancelled
		{
			get
			{
				var pick = Pick;
				return pick != null && pick.IsCancelled;
			}
		}

		#endregion

		#region IsPickFinalisedOrCancelled

		public bool IsPickFinalisedOrCancelled
		{
			get { return IsPickFinalised || IsPickCancelled; }
		}

		#endregion

		#region IsReserveLine

		public bool IsReserveLine
		{
			get { return isReserveLine ?? (isReserveLine = IsInDatabase && WZ_OriginalReservedQty > 0m).Value; }
			internal set
			{
				if (IsInDatabase || !value)
				{
					throw new InvalidOperationException("Cannot un-reserve a reserve pick line or change a saved pick line.");
				}

				isReserveLine = value;
			}
		}

		bool? isReserveLine;

		#endregion

		#endregion

		#region ClearOutPickingValuesForReservedLine

		public void ClearOutPickingValuesForReservedLine()
		{
			if (!IsReserveLine)
			{
				throw new InvalidOperationException("ClearOutPickingValuesForReservedLine() should only be called on Reserved Pick Lines.");
			}

			// we attempt to unpick the reserve line
			if (IsPicked)
			{
				WZ_PickedDateTime = ZDateTimeOffset.Empty;
			}

			WZ_IsPicking = false;
			WZ_GS_NKAssignedTo = string.Empty;
			WZ_VerifiedEmpty = string.Empty;
			WZ_Units = 0m;
			WZ_F3_NKAllocatedPackType = string.Empty;
			ClearReleaseCapturedAttributes();
		}

		#endregion

		#region IsPickByBOMKitPickLine

		public bool IsPickByBOMKitPickLine() => InventoryLineForAvailableInventory.IsCreatedFromPickByBOM;

		#endregion

		#region Split

		public WhsPickLine Split(ZDecimal qtyToAssign)
		{
			return SplitCore(qtyToAssign);
		}

		WhsPickLine SplitCore(ZDecimal qtyToAssign)
		{
			if (qtyToAssign <= 0m)
			{
				throw new ArgumentException("Cannot Split by a Negative or Zero amount.");
			}

			if (qtyToAssign >= WZ_Units)
			{
				throw new InvalidOperationException("Attempted to split pick line by a quantity greater than or equal to WZ_Units.");
			}

			var excludedColumns = new[] { WhsPickLineSchema.Constants.WZ_WE_TransactionLine, WhsPickLineSchema.Constants.WZ_Units };
			var args = new BusinessObjectCloneArgs(excludedColumns, performRowCopyWithoutTriggeringValidationAndSetter: true);
			var newPickLine = (WhsPickLine)Clone(args);

			using (GetValidationSuspender())
			using (newPickLine.GetValidationSuspender())
			{
				// We are merely splitting the PickLine and not changing Picked Qty
				TransferQtyAcrossPickLines(newPickLine, this, qtyToAssign);

				// Set WZ_WE_TransactionLine *after* the Quantity and Release Captured Attribs were set.
				// This is because the Non-Persistent ReleaseLinesCollection will update its internal pickline
				// cache when setting WZ_WE_TransactionLine, and this update relies on the Quantity and Release Captured Attribs.
				newPickLine.WZ_WE_TransactionLine = WZ_WE_TransactionLine;
			}

			return newPickLine;
		}

		#endregion

		#region SplitForPacking

		WhsPickLine SplitForPackingCore(ZDecimal qtyToSplit)
		{
			if (qtyToSplit > WZ_Units)
			{
				throw new ArgumentException("Cannot Split more than is Available on the PickLine.");
			}

			return SplitCore(qtyToSplit);
		}

		#endregion

		#region TransferQtyAcrossPickLines

		public static void TransferQtyAcrossPickLines(WhsPickLine pickLineToTransferTo, WhsPickLine pickLineToTransferFrom, ZDecimal qtyToMove)
		{
			if (pickLineToTransferTo.IsFinalised != pickLineToTransferFrom.IsFinalised)
			{
				throw new InvalidOperationException("Can only transfer between PickLines that are either both finalised or both unfinalized.");
			}

			if (qtyToMove <= 0m)
			{
				throw new ArgumentException(string.Format(Culture.Invariant, "Cannot transfer {0} Units.", (qtyToMove == 0m ? "zero" : "negative")), nameof(qtyToMove));
			}

			if (qtyToMove > pickLineToTransferFrom.WZ_Units)
			{
				throw new ArgumentException("Cannot transfer more than the PickLine has allocated.", nameof(qtyToMove));
			}

			if (pickLineToTransferFrom.WZ_WE_InventoryLine != pickLineToTransferTo.WZ_WE_InventoryLine)
			{
				throw new ArgumentException("Cannot transfer to pick lines for other Inventory.");
			}

			if (pickLineToTransferFrom.WZ_WE_OriginalPickedInventoryLine != pickLineToTransferTo.WZ_WE_OriginalPickedInventoryLine)
			{
				throw new ArgumentException("Cannot transfer to pick lines for other OriginalPickedInventory.");
			}

			if (!pickLineToTransferFrom.IsAllowedToSplitPickedOutboundTransferPickLine || !pickLineToTransferTo.IsAllowedToSplitPickedOutboundTransferPickLine)
			{
				throw new InvalidOperationException("Attempted to split pick line when already picked on Outbound Transfer.");
			}

			// Since we cannot normally change the Picked Qty we need to Temporarily allow this behaviour as in this
			// case we are not changing the Picked Qty but merely transferring the picked amount to another Order Line.
			using (pickLineToTransferTo.TemporarilyAllowChangingPickedQuantity_DoNotUse())
			using (pickLineToTransferFrom.TemporarilyAllowChangingPickedQuantity_DoNotUse())
			{
				pickLineToTransferTo.WZ_Units += qtyToMove;
				pickLineToTransferFrom.WZ_Units -= qtyToMove;
			}
		}

		IDisposable TemporarilyAllowSplittingPickedOutboundTransferPickLine() => new SemaphoreManager(AllowSplittingPickedOutboundTransferPickLineSemaphore);

		bool IsAllowedToSplitPickedOutboundTransferPickLine => !WZ_WE_OriginalOrderLine.IsValid ||
			(allowSplittingPickedOutboundTransferPickLineSemaphore != null && allowSplittingPickedOutboundTransferPickLineSemaphore.IsSuspended);

		Semaphore AllowSplittingPickedOutboundTransferPickLineSemaphore => allowSplittingPickedOutboundTransferPickLineSemaphore ??= new Semaphore();
		Semaphore allowSplittingPickedOutboundTransferPickLineSemaphore;

		/// <summary>
		/// DO NOT USE THIS UNLESS YOU KNOW WHAT YOU ARE DOING! This method will allow
		/// you to change the WZ_Units on a WhsPickLine even if the Picked Time is set. This
		/// is not normally valid to do.
		/// </summary>
		IDisposable TemporarilyAllowChangingPickedQuantity_DoNotUse() => new SemaphoreManager(AllowChangingPickedQuantitySemaphore);

		Semaphore AllowChangingPickedQuantitySemaphore => allowChangingPickedQuantitySemaphore ?? (allowChangingPickedQuantitySemaphore = new Semaphore());
		Semaphore allowChangingPickedQuantitySemaphore;

		#endregion

		#region Validation

		protected override WhsPickLineValidation GetNewValidation()
		{
			WhsPickLineValidation result = null;

			// US Validation is irrevelant for non-reserve lines
			if (IsReserveLine)
			{
				var docketLine = DocketLine;
				if (docketLine != null && docketLine.CountryCode == Constants.CountryCodes.UnitedStates)
				{
					result = new WhsPickLineValidationUS(this);
				}
			}

			return result ?? base.GetNewValidation();
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			bool result = base.IsValidationEnabledCore(propertyInfo);
			if (result)
			{
				bool isReservedAndUnpicked = IsReserveLine && IsNotAllocatedToPick;

				switch (propertyInfo.Name)
				{
					case WhsPickLineSchema.Constants.WZ_Units:
						result = !isReservedAndUnpicked;
						break;
					case WhsPickLine.Schema.ReservedQuantity:
						result = isReservedAndUnpicked;
						break;
				}
			}

			return result;
		}

		#endregion

		#region Cloning

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning = new List<string>(base.GetPropertiesToExcludeFromCloning());
			propertiesToExcludeFromCloning.Add(WhsPickLineSchema.Constants.WZ_OriginalReservedQty);
			return propertiesToExcludeFromCloning;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			// force Cloner to do a Row Copy always, we never want to use the setters when cloning a Pick Line.
			var newArgs = args.PerformRowCopyWithoutTriggeringValidationAndSetter
				? args
				: new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, args.GetExcludedColumns(), args.TypeToCloneAs, true, args.CopyDecider);

			var result = (WhsPickLine)base.CloneInternal(newArgs);
			result.IsFinalisedOverride = IsFinalised; // This is necessary for when splitting an already finalised Pick Line.

			return result;
		}

		#endregion

		#region Delete

		protected override void BeforeSuccessfulDelete()
		{
			// no point running trigger for records that were never in DB
			if (IsInDatabase)
			{
				TransactionLinePKForDelete = (ZGuid)this.WZ_WE_TransactionLineInfo.OriginalValue;
			}

			base.BeforeSuccessfulDelete();
		}
		internal ZGuid TransactionLinePKForDelete;

		public override void Delete()
		{
			// we should not delete In-Transit Pick Lines
			if (WZ_WE_OriginalPickedInventoryLine.IsValid && WZ_Units > 0m)
			{
				throw new InvalidOperationException("You cannot Delete a Pick Line with Originally Picked Inventory set.");
			}

			// undo picking if we're deleting the PickLine.
			if (IsPicked && WZ_Units > 0m)
			{
				WZ_PickedDateTime = ZDateTimeOffset.Empty;
			}

			if (IsInDatabase)
			{
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(Factory, this);
			}
			base.Delete();
		}

		protected override void OnBeforeDeletedByDataRefresh()
		{
			base.OnBeforeDeletedByDataRefresh();

			if (!IsDeleted)
			{
				(DocketLine as WhsOrderLine)?.ClearReleaseLines();
			}
		}

		#endregion

		#region CreateReserveLine_Unsafe

		/// <summary>
		/// This function was only exists to allow reserveLines to be created in WhsWebservice. 
		/// For most other situations is HIGHLY suggested that any needed reserveLines are created through OrderLine.ReserveIfAbleTo().
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="inventoryPK"></param>
		/// <param name="orderLinePK"></param>
		/// <param name="unitsToReserve"></param>
		/// <returns></returns>
		public static WhsPickLine ReserveInventory_Unsafe(BusinessObjectFactory factory, ZGuid inventoryPK, ZGuid orderLinePK, decimal unitsToReserve)
		{
			if (!inventoryPK.IsValid)
			{
				throw new ArgumentException("inventoryPK is not valid");
			}
			if (!orderLinePK.IsValid)
			{
				throw new ArgumentException("orderLinePK is not valid");
			}
			Argument.NotNull(factory, "Factory");
			if (unitsToReserve <= 0)
			{
				throw new ArgumentException("UnitsToReserve is not greater than zero.");
			}

			// add pick line to represent reservation of stock.
			var result = factory.New<WhsPickLine>();
			using (result.GetValidationSuspender())
			{
				// A new reserve pickline has an original qty of 0, which is technically NOT a reserve pickline.
				// To know that it is a reserve pickline prior to setting the original qty to > 0, set state in memory.
				result.IsReserveLine = true;
				result.WZ_WE_TransactionLine = orderLinePK;
				result.WZ_WE_InventoryLine = inventoryPK;
				result.ReservedQuantity = unitsToReserve;
			}

			return result;
		}

		#endregion

		// interfaces

		#region IEmptyLocationAfterPickFinalisation Members

		bool IEmptyLocationAfterPickFinalisation.IsLocationEmptyAfterFinalisingPick
		{
			get;
			set;
		}

		decimal IEmptyLocationAfterPickFinalisation.AllocatedQty
		{
			get
			{
				return !IsPickedFromPutawayLocation || WasPickedInMemoryForStocktake ? (decimal)WZ_Units : 0m;
			}
		}

		WhsDocketLine IEmptyLocationAfterPickFinalisation.Inventory => InventoryLineForAvailableInventory;

		#endregion

		#region ICartonisableItem Members

		Guid ICartonisableItem.PK => PK.ToGuid();

		Guid ICartonisableItem.LocationPK => InventoryLine.WE_WL.IsValid ? InventoryLine.WE_WL.ToGuid() : Guid.Empty;

		decimal ICartonisableItem.Quantity => WZ_Units;

		ICartonisableItemDefinition ICartonisableItem.ItemDefinition => Product;

		#endregion

		#region IReducibleItem Members

		bool IReducibleItem.CanBeReduced => IsPickedFromPutawayLocation && this.IsUnpacked(Factory);

		ActionResult IReducibleItem.ReduceStock(ZDecimal quantityToReduce, IPickedStockAdjuster adjuster)
		{
			Argument.NotNull(adjuster, nameof(adjuster));

			if (quantityToReduce > WZ_Units)
			{
				throw new ArgumentException("quantityToLose must not be greater than WZ_Units.");
			}

			if (quantityToReduce <= 0)
			{
				throw new ArgumentException("quantityToLose must be positive.");
			}

			if (!((IReducibleItem)this).CanBeReduced)
			{
				throw new InvalidOperationException("Should not try to Reduce Stock when the PickLine is not Picked or does not have Unpacked item.");
			}

			using (TemporarilyAllowChangingPickedQuantity_DoNotUse()) // Until we have a proper way of recording Lost Stock, we have to reduce what was Picked
			{
				WZ_Units -= quantityToReduce;
			}

			InventoryAndActionResult inventoryAndResult;
			ZGuid originalInventory;

			if (IsPicked)
			{
				InventoryLine.WE_StockOnHand += quantityToReduce; // Since we need stock to adjust out, we temporarily increase Total Units.

				var inventoryToAdjust = Inventory;
				inventoryAndResult = InventoryAndActionResult.Success(inventoryToAdjust);
				originalInventory = inventoryToAdjust.PK;
			}
			// Must be Picked from Putaway Location Transfer Line since CanBeReduced is true
			else
			{
				inventoryAndResult = FinaliseTransferLineWithQuantityToReduce(quantityToReduce);
				originalInventory = inventoryAndResult.IsSuccess
					? WZ_WE_OriginalPickedInventoryLine
					: ZGuid.Empty;
			}

			// if we didn't succeed with the above, there's no point adjusting out the stock
			if (inventoryAndResult.IsSuccess)
			{
				adjuster.AdjustOutInventory(inventoryAndResult.InventoryToAdjust, originalInventory, quantityToReduce);

				if (WZ_Units == 0)
				{
					Delete();
				}
			}

			return inventoryAndResult.Result;
		}

		InventoryAndActionResult FinaliseTransferLineWithQuantityToReduce(ZDecimal quantityToReduce)
		{
			InventoryAndActionResult result;

			var inTransitTransferLine = (WhsTransferLine)InventoryLine;
			// if transfer line is already finalised then we can simply adjust it directly.
			if (inTransitTransferLine.IsFinalised)
			{
				result = InventoryAndActionResult.Success(inTransitTransferLine.Inventory[0]);
			}
			// if we are reducing stock for the whole transferred quantity, we can finalise the Transfer Line entirely.
			else if (inTransitTransferLine.WE_StockOnHand == quantityToReduce)
			{
				// we make this a main Transfer Line as we are finalising it separately to its Main Transfer Line
				inTransitTransferLine.WE_WE_MatchingLine = ZGuid.Empty;
				result = FinaliseTransferLine(inTransitTransferLine);
			}
			// we should split off the stock we are reducing and finalise the split transferline only
			else
			{
				var splitTransferLineToAdjustOut = SplitTransferLine(quantityToReduce, inTransitTransferLine);
				result = FinaliseTransferLine(splitTransferLineToAdjustOut);
			}

			return result;
		}

		static WhsTransferLine SplitTransferLine(ZDecimal quantityToReduce, WhsTransferLine inTransitTransferLine)
		{
			var mainTransferLine = inTransitTransferLine.IsMainTransactionLine()
				? inTransitTransferLine
				: inTransitTransferLine.MatchingParentLine;

			if (quantityToReduce >= mainTransferLine.WE_TransactionQuantity)
			{
				throw new ArgumentException("quantityToReduce must be less than the Transaction Qty.", nameof(quantityToReduce));
			}

			var splitTransferLineToAdjustOut = mainTransferLine.CreateMatchingLineWithQuantity(quantityToReduce);
			// we make this a main Transfer Line as we are finalising it separately to its Main Transfer Line
			splitTransferLineToAdjustOut.WE_WE_MatchingLine = ZGuid.Empty;

			// move quantities off Original Transfer Line
			inTransitTransferLine.WE_TransactionQuantity -= quantityToReduce;
			inTransitTransferLine.WE_StockOnHand -= quantityToReduce;

			// split picked pick line by amount we are reducing
			var pickLine = inTransitTransferLine.PickLines.Single();
			var newPickLine = (WhsPickLine)pickLine.Clone(new BusinessObjectCloneArgs(new[] { WhsPickLineSchema.Constants.WZ_Units, WhsPickLineSchema.Constants.WZ_WE_TransactionLine }));

			// Tested in WhsReleaseLine_AdjustOutQuantityMet/ReturnStockTest
			using (pickLine.TemporarilyAllowSplittingPickedOutboundTransferPickLine())
			using (newPickLine.TemporarilyAllowSplittingPickedOutboundTransferPickLine())
			{
				TransferQtyAcrossPickLines(newPickLine, pickLine, quantityToReduce);
			}

			newPickLine.WZ_WE_TransactionLine = splitTransferLineToAdjustOut.PK;

			// Recreate In-Transit inventory on the two lines
			inTransitTransferLine.CreateInTransitInventory();
			splitTransferLineToAdjustOut.CreateInTransitInventory();

			return splitTransferLineToAdjustOut;
		}

		static InventoryAndActionResult FinaliseTransferLine(WhsTransferLine transferLine)
		{
			InventoryAndActionResult result;

			// default a Destination Location if not set, to allow finalisation.
			if (transferLine.WE_WL.IsEmpty)
			{
				transferLine.WE_WL = transferLine.WE_WL_TransferFrom;
			}

			transferLine.FinaliseDocketLine();

			if (!transferLine.IsFinalised)
			{
				result = InventoryAndActionResult.Failure(transferLine.GetErrors().ToMessageListString());
			}
			else
			{
				result = InventoryAndActionResult.Success(transferLine.Inventory[0]);
			}

			return result;
		}

		class InventoryAndActionResult
		{
			InventoryAndActionResult(WhsInventoryView inventoryToAdjust, ActionResult result)
			{
				InventoryToAdjust = inventoryToAdjust;
				Result = result;
			}

			public static InventoryAndActionResult Success(WhsInventoryView inventoryToAdjust)
			{
				Argument.NotNull(inventoryToAdjust, nameof(inventoryToAdjust));
				return new InventoryAndActionResult(inventoryToAdjust, ActionResult.Success());
			}

			public static InventoryAndActionResult Failure(string errorMessage)
			{
				Argument.NotNullOrEmpty(errorMessage, nameof(errorMessage));
				return new InventoryAndActionResult(null, ActionResult.Failure(errorMessage));
			}

			public WhsInventoryView InventoryToAdjust { get; }
			public ActionResult Result { get; }

			public bool IsSuccess => Result.IsSuccess;
		}

		#endregion

		#region IPackableItem Members

		ZDecimal IPackableItem.Quantity => WZ_Units;

		GroupingKey IPackableItem.Key => InventoryGroupingKey.New(this);

		IPackableItem IPackableItem.Split(ZDecimal qtyToSplit) => SplitForPackingCore(qtyToSplit);

		void IPackableItem.ReMerge()
		{
			if (!SuspendReMergeSemaphore.IsSuspended && !IsFinalised)
			{
				var docketLine = DocketLine;
				if (docketLine != null)
				{
					Func<WhsPickLine, bool> isRCAsTheSame = (pl) => pl.WZ_ReleaseCapturedPartAttrib1.EqualsIgnoringCase(WZ_ReleaseCapturedPartAttrib1)
						&& pl.WZ_ReleaseCapturedPartAttrib2.EqualsIgnoringCase(WZ_ReleaseCapturedPartAttrib2)
						&& pl.WZ_ReleaseCapturedPartAttrib3.EqualsIgnoringCase(WZ_ReleaseCapturedPartAttrib3)
						&& pl.WZ_ReleaseCapturedSerialNumber.IsEmpty
						&& WZ_ReleaseCapturedSerialNumber.IsEmpty;

					var sameInventoryQuery = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, WZ_WE_InventoryLine);
					var pickLineToMerge = docketLine.PickLines.Find(sameInventoryQuery).FirstOrDefault(p => PK != p.PK
						&& p.WZ_WE_OriginalPickedInventoryLine == WZ_WE_OriginalPickedInventoryLine
						&& p.WZ_PickedDateTime.IsEmpty
						&& p.WZ_Units > 0m
						&& p.WZ_F3_NKAllocatedPackType.EqualsIgnoringCase(WZ_F3_NKAllocatedPackType)
						&& p.WZ_GS_NKAssignedTo.EqualsIgnoringCase(WZ_GS_NKAssignedTo)
						&& p.IsUnpacked(Factory)
						&& isRCAsTheSame(p));

					if (pickLineToMerge != null)
					{
						TransferQtyAcrossPickLines(pickLineToMerge, this, WZ_Units);
						Delete();
					}
				}
			}
		}

		public IDisposable SuspendReMerge() => new SemaphoreManager(SuspendReMergeSemaphore);

		Semaphore SuspendReMergeSemaphore => suspendReMergeSemaphore ?? (suspendReMergeSemaphore = new Semaphore());
		Semaphore suspendReMergeSemaphore;

		#endregion

		#region IWhsPickLineInternals

		IDisposable IWhsPickLineInternals.TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse() => new SemaphoreManager(AllowUnpickingPickLineWithNoStockChange);

		Semaphore AllowUnpickingPickLineWithNoStockChange => allowUnpickingPickLineWithNoStockChange ?? (allowUnpickingPickLineWithNoStockChange = new Semaphore());

		Semaphore allowUnpickingPickLineWithNoStockChange;

		void ISyncWithDB.SafeDeleteForBizOAlreadyDeletedInDatabase_DoNotUse()
		{
			using (((IWhsPickLineInternals)this).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
			{
				WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty; // Allow delete for PickLines that had OriginalPickedInventoryLine set.
				Delete();
			}
		}

		#endregion

		#region IWhsPickLine

		Integration.IWhsDocketLine Integration.IWhsPickLine.InventoryLine => InventoryLine;

		#endregion

		//

		#region Test
#if DEBUG
		#region FillWithValidTestData

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper(Factory);
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			public MyBusinessObjectTestDataHelper(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			protected override void PopulateFK(ZPropertyInfo fKProperty, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
			{
				if (fKProperty.Name == Schema.WZ_WE_TransactionLine || fKProperty.Name == Schema.WZ_WE_InventoryLine)
				{
					var transferLine = Factory.NewWithValidTestData<WhsTransferLine>();
					transferLine.WE_TransactionQuantity = 1m;
					fKProperty.Value = transferLine.PK;
				}
				else
				{
					base.PopulateFK(fKProperty, kind, propertyPath);
				}
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

#endif
		#endregion
	}
}
