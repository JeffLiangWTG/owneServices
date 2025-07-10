using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsTransfer), "Lines")]
	[SystemDefinedValues]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised, WhsValidationHelper.WhsCheckInterWhsTransfersAreInSync, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_TransferLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent), ValueToRunStoredProcWith = ValueVersion.Both)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsValidationHelper.WhsCheckStockOnHandIsBalanced_ForInsert, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_TransferLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTransactionAndPickQtyIsCorrectForWhsTransferLine_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsValidationHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTransactionAndPickQtyIsCorrect_Insert_WhsTransferLine_DeferTriggerStrategy))]
	public sealed partial class WhsTransferLine : WhsDocketLine,
		IWhsTransferLine,
		ICustomsDataParent,
		ILineStaffAssigner,
		ILineWithMatchingLines<WhsTransferLine>,
		ILineWithPickAndPutawayDetails
	{
		#region Constructors

		public WhsTransferLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Schema

		public new abstract class Schema : WhsDocketLine.Schema
		{
			public const string TransferFromWarehousePK = "TransferFromWarehousePK";
			public const string TransferFromLocationString = "TransferFromLocationString";
			public const string DestinationWarehousePK = "DestinationWarehousePK";
			public const string GS_NKPickedBy = "GS_NKPickedBy";
			public const string PackQtyIncludingMatchingLines = "PackQtyIncludingMatchingLines";
			public const string PickedTime = "PickedTime";
			public const string QtyCommittedIncludingMatchingLines = "QtyCommittedIncludingMatchingLines";
			public const string QtyToMoveIncludingMatchingLines = "QtyToMoveIncludingMatchingLines";
			public const string ArrivalDateForBinding = "ArrivalDateForBinding";
		}

		#endregion

		#region Type Decider

		public override Type DocketType
		{
			get { return typeof(WhsTransfer); }
		}

		#endregion

		#region Related Business Objects

		#region CustomsData

		protected override bool IsCustomsDataReadOnly => base.IsCustomsDataReadOnly || IsTransferringForOrder;

		#endregion

		#region Docket

		public new WhsTransfer Docket
		{
			get { return (WhsTransfer)base.Docket; }
		}

		#endregion

		#region ChildTransferLine

		public WhsTransferLine ChildTransferLine
		{
			get
			{
				var query = new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, PK);
				query.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);

				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.LoadTop1<WhsTransferLine>(query);
			}
		}

		#endregion

		#region MatchingParentLine

		public WhsTransferLine MatchingParentLine
		{
			get { return Factory.Load<WhsTransferLine>(WE_WE_MatchingLine); }
		}

		#endregion

		#region InventoryCommitmentBuilder

		WhsInventoryCommitterWithMatchingLines<WhsTransferLine> InventoryCommitmentBuilder
		{
			get { return inventoryCommitmentBuilder ?? (inventoryCommitmentBuilder = new WhsInventoryCommitterWithMatchingLines<WhsTransferLine>(this)); }
		}

		WhsInventoryCommitterWithMatchingLines<WhsTransferLine> inventoryCommitmentBuilder;

		#endregion

		#region MatchingLines

		[ChildEditable(true)]
		public WhsTransferLineCollection MatchingLines
		{
			get
			{
				if (matchingLines == null)
				{
					matchingLines = new WhsTransferLineCollection(this);
					RegisterEditableChildObject(matchingLines);
				}
				return matchingLines;
			}
		}

		WhsTransferLineCollection matchingLines;

		#endregion

		#region Validation

		public new WhsTransferLineValidation Validation
		{
			get { return (WhsTransferLineValidation)base.Validation; }
		}

		protected override void ValidateUnitsProperty()
		{
			// do not call base as we do not want to validate 'WE_TransactionQuantity'
			Validation.ValidateQtyToMoveIncludingMatchingLines();
		}

		#endregion

		#endregion

		#region Business Object Overrides

		#region IsValidationEnabled

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return (!IsFinalised || !propertyInfo.ReadOnly) && this.IsMainTransactionLine() && base.IsValidationEnabledCore(propertyInfo);
		}

		#endregion

		#region SupportsClone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>(base.GetPropertiesToExcludeFromCloning()) { WhsDocketLineSchema.Constants.WE_StockOnHand };
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WE_DocketLineStatus = TransferLineDefaultStatus;
		}

		public const string TransferLineDefaultStatus = DocketLineStatus.Codes.Entered;

		protected override string DefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Available; }
		}

		protected override string DocketLineType => CodeLists.DocketType.Codes.Transfer;

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			CheckPropertiesAreNotChangedOnFinalisedTransferLine();
			CheckPropertiesAreNotChangedInVASOrderTransfer();
			CheckPropertiesAreNotChangedInOutboundDockDoorTransfer();
			CheckOutboundTransfersAreValid();
			CheckPutawayTimeIfFinalised();
			CheckCannotUpdateFinalisedVASOrderTransferInOutsideVASOrderTransferOut();

			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
		}

		#region CheckPropertiesAreNotChangedOnFinalisedTransferLine

		void CheckPropertiesAreNotChangedOnFinalisedTransferLine()
		{
			if (IsInDatabase && WE_DocketLineStatusInfo.OriginalValue.Equals(DocketLineStatus.Codes.Finalised) && IsFinalised
				&& ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(propertyInfo => !PropertiesToExclude.Contains(propertyInfo.Name) && propertyInfo.HasChanges))
			{
				throw new ZCannotSaveException(Res.GetString("cb44c3a3-5a24-40b9-b178-5ebe1db7dd59", "Cannot save as a Finalized Transfer Line has been modified."),
					Res.GetString("ae2e5d9c-1688-4ede-8c3f-27800d935fbe", "Finalized Line Modified"), ExceptionType.BusinessFailure);
			}
		}

		void CheckPutawayTimeIfFinalised()
		{
			if (IsFinalised)
			{
				var heading = Res.GetString("1561772B-9898-47F7-9F6D-527BB1E666FE", "Finalized Line Putaway Incorrect");
				if (IsPutaway && !WE_IsOriginalInventory)
				{
					throw new ZCannotSaveException(Res.GetString("F97C0E1A-D7B9-4DE9-9B32-E63A5116D870", "This transfer line cannot be finalized with putaway time because it is not Original Inventory."), heading, ExceptionType.BusinessFailure);
				}
				else if (!IsPutaway && WE_IsOriginalInventory)
				{
					throw new ZCannotSaveException(Res.GetString("0943D72E-7066-4E2C-81C9-EFD24BC52862", "This transfer line cannot be finalized without putaway time because it is Original Inventory."), heading, ExceptionType.BusinessFailure);
				}
			}
		}

		IEnumerable<string> PropertiesToExclude
		{
			get { return Factory.GetCachedValue("WhsTransferLine|PropertiesToExclude|" + this.IsMainTransactionLine(), GetPropertiesToExcludeForOnSavingCheck); }
		}

		IEnumerable<string> GetPropertiesToExcludeForOnSavingCheck()
		{
			var result = new List<string>(6);
			result.Add(WhsDocketLineSchema.Constants.WE_StockOnHand);
			result.Add(WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus);
			result.Add(WhsDocketLineSchema.Constants.WE_WHC_NKCurrentInventoryHeldCode);
			result.Add(WhsDocketLineSchema.Constants.WE_CurrentHoldReason);
			result.Add(WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc);
			result.Add(WhsDocketLineSchema.Constants.WE_SystemCreateUser);
			result.Add(WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc);
			result.Add(WhsDocketLineSchema.Constants.WE_SystemLastEditUser);

			if (!this.IsMainTransactionLine())
			{
				result.Add(WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines);
				result.Add(WhsTransferLine.Schema.PackQtyIncludingMatchingLines);
				result.Add(WhsTransferLine.Schema.QtyCommittedIncludingMatchingLines);
			}

			return result.ToArray();
		}

		#endregion

		#region CheckPropertiesAreNotChangedInVASOrderTransfer

		void CheckPropertiesAreNotChangedInVASOrderTransfer()
		{
			if (!IsFinalised && IsInDatabase && IsVASOrderTransfer)
			{
				var areAnyPropertiesChanged = PropertiesMustNotBeChangedForVASOrderTransferLine.Any(name => ZPropertyInfoHash[name].HasChanges);
				if (areAnyPropertiesChanged)
				{
					throw new ZCannotSaveException(Res.GetString("WhsTransferLine|PropertiesMustNotBeChanged", "Cannot save as fields are modified in VAS Order Transfer Line."),
						Res.GetString("WhsTransferLine|VASOrderTransferLineMustNotBeChanged", "VAS Order Transfer Line Must Not Be Modified."), ExceptionType.BusinessFailure);
				}
			}
		}

		IEnumerable<string> PropertiesMustNotBeChangedForVASOrderTransferLine
		{
			get
			{
				var cacheKey = string.Format(Culture.Invariant, "WhsTransferLine|PropertiesMustNotBeChanged|{0}|{1}", IsIntoServiceAreaTransferForVASOrder, IsInTransit); // Key for factory cache, cannot be localised
				return Factory.GetCachedValue(cacheKey, GetPropertiesMustNotBeChangedForVASOrderTransferLine);
			}
		}

		string[] GetPropertiesMustNotBeChangedForVASOrderTransferLine()
		{
			var result = new List<string>(12);
			result.Add(WhsDocketLineSchema.WE_OP.Name);
			result.Add(WhsDocketLineSchema.WE_PartAttrib1.Name);
			result.Add(WhsDocketLineSchema.WE_PartAttrib2.Name);
			result.Add(WhsDocketLineSchema.WE_PartAttrib3.Name);
			result.Add(WhsDocketLineSchema.WE_SerialNumber.Name);
			result.Add(WhsDocketLineSchema.WE_PackingDate.Name);
			result.Add(WhsDocketLineSchema.WE_ExpiryDate.Name);
			result.Add(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode.Name);
			result.Add(WhsDocketLineSchema.WE_TransactionQuantity.Name);

			if (!IsInTransit)
			{
				result.Add(WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name);
				result.Add(WhsDocketLineSchema.WE_CurrentInventoryStatus.Name);
			}

			if (IsIntoServiceAreaTransferForVASOrder)
			{
				result.Add(WhsTransferLine.Schema.WE_WL);
			}
			else
			{
				result.Add(WhsTransferLine.Schema.WE_TransferFromPalletId);
				result.Add(WhsTransferLine.Schema.WE_WL_TransferFrom);
			}

			return result.ToArray();
		}

		#endregion

		#region CheckPropertiesAreNotChangedInOutboundDockDoorTransfer

		void CheckPropertiesAreNotChangedInOutboundDockDoorTransfer()
		{
			if (!IsFinalised && IsInDatabase && IsTransferringForOrder && PropertiesMustNotBeChangedForOutboundDockDoorTransferLine.Any(name => ZPropertyInfoHash[name].HasChanges))
			{
				throw new ZCannotSaveException(Res.GetString("WhsTransferLine|PropertiesMustNotBeChangedForOutboundDockDoorTransferLine", "Cannot save as an Outbound Dock Door Transfer Line has been modified."),
					Res.GetString("WhsTransferLine|OutboundDockDoorTransferLineMustNotBeChanged", "Outbound Dock Door Transfer Line Must Not Be Modified."), ExceptionType.BusinessFailure);
			}
		}

		IEnumerable<string> PropertiesMustNotBeChangedForOutboundDockDoorTransferLine
		{
			get { return Factory.GetCachedValue("WhsTransferLine|PropertiesMustNotBeChangedForOutboundDockDoorTransferLine", GetPropertiesMustNotBeChangedForOutboundDockDoorTransferLine); }
		}

		IEnumerable<string> GetPropertiesMustNotBeChangedForOutboundDockDoorTransferLine()
		{
			return new[]
			{
				WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate,
				WhsDocketLineSchema.Constants.WE_BondedEntryKey,
				WhsDocketLineSchema.Constants.WE_ExpiryDate,
				WhsDocketLineSchema.Constants.WE_OP,
				WhsDocketLineSchema.Constants.WE_PackageGroupId,
				WhsDocketLineSchema.Constants.WE_PackingDate,
				WhsDocketLineSchema.Constants.WE_PartAttrib1,
				WhsDocketLineSchema.Constants.WE_PartAttrib2,
				WhsDocketLineSchema.Constants.WE_PartAttrib3,
				WhsDocketLineSchema.Constants.WE_SerialNumber,
				WhsDocketLineSchema.Constants.WE_PerPackageQty,
				WhsDocketLineSchema.Constants.WE_TransferFromPalletId,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_WL_TransferFrom,
			};
		}

		#endregion

		#region CheckOutboundTransfersAreValid

		void CheckOutboundTransfersAreValid()
		{
			if (IsTransferringForOrder && !IsFinalised)
			{
				RunPreSaveValidation();
				if (HasErrors)
				{
					var errors = this.GetErrors().ToUniqueMessageListString();
					throw new ZCannotSaveException(Res.GetString("WhsTransferLine|InvalidDockDoorTransferMessage", "Cannot save as Dock Door Transfers are in an invalid state: {0}", errors),
						Res.GetString("WhsTransferLine|InvalidDockDoorTransferHeading", "Invalid Dock Door Transfers Created."), ExceptionType.BusinessFailure);
				}
			}
		}

		#endregion

		#region CheckCannotUpdateFinalisedVASOrderTransferInOutsideVASOrderTransferOut

		void CheckCannotUpdateFinalisedVASOrderTransferInOutsideVASOrderTransferOut()
		{
			if (IsFinalised && !WE_DocketLineStatusInfo.HasChanges && WE_CurrentInventoryStatusInfo.HasChanges && !IsPutawayTransferLine)
			{
				var vasOrder = Factory.LoadTop1<WhsVASOrder>(new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, WE_WD));
				if (vasOrder != null && !(vasOrder.IsFinalised || IsPickedByVASTransferOut()))
				{
					throw new ZCannotSaveException(Res.GetString("WhsTransferLine|CannotUpdateVASOrderTransferIn", "You cannot modify VAS Order Transfer In inventory outside of its VAS Order Transfer Out."),
						Res.GetString("WhsTransferLine|VASOrderTransferInLineMustNotBeChanged", "VAS Order Transfer In Inventory Must Not Be Modified."), ExceptionType.BusinessFailure);
				}
			}
		}

		bool IsPickedByVASTransferOut()
		{
			var pickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, PK));
			return pickLines.IsCountEqualTo(1)
				&& pickLines.Single().DocketLine is WhsTransferLine transferLine
				&& transferLine.IsOutOfServiceAreaTransferForVASOrder;
		}

		#endregion

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			PopulateWarehouseForInterWhsTransfers();
		}

		void PopulateWarehouseForInterWhsTransfers()
		{
			var transfer = Docket;
			if (transfer != null)
			{
				if (transfer.WD_DocketSubType == TransferType.Codes.InterWhsSource)
				{
					DestinationWarehousePK = DestinationWarehousePK; // to populate local variable destinationWarehousePK to be used if related location field is cleared.
				}
				else if (transfer.WD_DocketSubType == TransferType.Codes.InterWhsDest)
				{
					TransferFromWarehousePK = TransferFromWarehousePK; // to populate local variable transferFromWarehousePK to be used if related location field is cleared.
				}
			}
		}

		#endregion

		#region CloneInternal

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsTransferLine)base.CloneInternal(args);

			if (this.IsMainTransactionLine())
			{
				using (clone.GetValidationSuspender())
				{
					if (IsInTransit)
					{
						clone.UpdateInventoryStatusByOriginalInventoryHeldCode();
					}
					clone.QtyToMoveIncludingMatchingLines = QtyToMoveIncludingMatchingLines;
				}
			}

			return clone;
		}

		#endregion

		#region GetFetchStrategyCore

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new WhsTransferLineFetchStrategy(this);

		#endregion

		#region IsInventoryLineCore

		protected override bool IsInventoryLineCore
		{
			get { return (string)Docket?.WD_DocketSubType != TransferType.Codes.InterWhsSource; }
		}

		#endregion

		#endregion

		#region Public Static Methods

		public static WhsTransferLine New(BusinessObjectFactory factory)
		{
			return factory.
				// Split so find replace will ignore
				New<WhsTransferLine>();
		}

		#endregion

		#region Lookups

		public new WhsTransferLineLookups Lookups
		{
			get { return (WhsTransferLineLookups)base.Lookups; }
		}

		protected override WhsDocketLineLookups GetNewLookups()
		{
			return new WhsTransferLineLookups(this);
		}

		#endregion

		#region Validation

		protected override WhsDocketLineValidation GetNewValidation()
		{
			switch (CountryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return new WhsTransferLineValidationUS(this);
				default:
					return new WhsTransferLineValidation(this);
			}
		}

		#region RunPreSaveValidationCore

		protected override void BeforeRunPreSaveValidationCore()
		{
			base.BeforeRunPreSaveValidationCore();
			CommitInventory();
		}

		#endregion

		#endregion

		#region Properties

		#region Flags

		bool IsInTransit => WhsInventoryView.InTransitStatusCodeList.Any(i => i == WE_OriginalInventoryStatus);
		bool IsTransferringForOrder => Docket?.IsTransferringForOrder ?? false;
		public bool IsCrossDockPutaway => IsPutawayTransferLine && ReservedPickLines.Count > 0;

		#region CanUpdateFromInventory

		protected override bool CanUpdateFromInventoryCore
		{
			get { return base.CanUpdateFromInventoryCore && !IsFinalised; }
		}

		#endregion

		#region IsFinalised

		protected override bool IsFinalisedCore
		{
			get { return base.IsFinalisedCore && MatchingLines.Cast<WhsTransferLine>().All(l => l.IsFinalised); }
		}

		#endregion

		#region IsFinalising

		public bool IsFinalising
		{
			get
			{
				var parent = Docket;
				return FinaliseDocketLineSemaphore.IsSuspended ||
					(parent != null && parent.IsFinalising) ||
					(parent != null && parent.IsFinalisingLines);
			}
		}

		#endregion

		#region IsChildTransferLine

		public ZBool IsChildTransferLine
		{
			get { return WE_WE_ParentDocketLine.IsValid && WE_IsOriginalInventory; }
		}

		#endregion

		#region IsMatchingLine

		bool IsMatchingLine => WE_WE_MatchingLine.IsValid;

		#endregion

		#region IsHeldForTransfer

		public bool IsHeldForTransfer
		{
			get { return WE_DocketLineStatus == DocketLineStatus.Codes.HeldForTransfer; }
		}

		#endregion

		#region IsPicked

		public bool IsPicked => PickedTime.IsValid;

		#endregion

		#region IsPickerPicking

		public bool IsPickerPicking => PickLines.Any(l => l.WZ_IsPicking);

		#endregion

		#region IsPutaway

		public bool IsPutaway => WE_PutawayTime.IsValid;

		#endregion

		#endregion

		#region WE_BondedEntryKey

		public override ZString WE_BondedEntryKey
		{
			get { return base.WE_BondedEntryKey; }
			set
			{
				base.WE_BondedEntryKey = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_BondedEntryKey);
			}
		}

		#endregion

		#region WE_OP

		public override ZGuid WE_OP
		{
			get { return base.WE_OP; }
			set
			{
				base.WE_OP = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_OP);
			}
		}

		#endregion

		#region WE_PackageGroupId

		public override ZString WE_PackageGroupId
		{
			get { return base.WE_PackageGroupId; }
			set
			{
				base.WE_PackageGroupId = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_PackageGroupId);
			}
		}

		#endregion

		#region WE_PerPackageQty

		[ReadOnly(true)]
		public override ZDecimal WE_PerPackageQty
		{
			get => base.WE_PerPackageQty;
			set => base.WE_PerPackageQty = value;
		}

		#endregion

		#region WE_PartAttrib1

		public override ZString WE_PartAttrib1
		{
			get { return base.WE_PartAttrib1; }
			set
			{
				base.WE_PartAttrib1 = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_PartAttrib1);
			}
		}

		#endregion

		#region WE_PartAttrib2

		public override ZString WE_PartAttrib2
		{
			get { return base.WE_PartAttrib2; }
			set
			{
				base.WE_PartAttrib2 = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_PartAttrib2);
			}
		}

		#endregion

		#region WE_PartAttrib3

		public override ZString WE_PartAttrib3
		{
			get { return base.WE_PartAttrib3; }
			set
			{
				base.WE_PartAttrib3 = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_PartAttrib3);
			}
		}

		#endregion

		#region WE_PackingDate

		public override ZDate WE_PackingDate
		{
			get { return base.WE_PackingDate; }
			set
			{
				base.WE_PackingDate = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_PackingDate);
			}
		}

		#endregion

		#region WE_ExpiryDate

		public override ZDate WE_ExpiryDate
		{
			get { return base.WE_ExpiryDate; }
			set
			{
				bool hasChanged = base.WE_ExpiryDate != value;
				base.WE_ExpiryDate = value;

				if (hasChanged)
				{
					DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_ExpiryDate);
				}
			}
		}

		#endregion

		#region ArrivalDateForBinding

		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		[ResourceStringData("WhsTransferLine|ArrivalDateForBinding", Caption = "Arrival Date")]
		public ZDateTimeOffset ArrivalDateForBinding
		{
			get
			{
				var result = WE_AdjustmentArrivalDate;

				if (this.MatchingLines.Any(l => l.WE_AdjustmentArrivalDate != WE_AdjustmentArrivalDate))
				{
					result = ZDateTimeOffset.Empty;
				}

				return result;
			}
			set
			{
				WE_AdjustmentArrivalDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateArrivalDateForBinding();
				}

				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate);
				ArrivalDateForBindingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalDateForBindingInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalDateForBinding); }
		}

		#endregion

		#region WE_TransferFromPalletId

		[ReadOnlyMember(nameof(SourceLocationOrPalletIDReadOnly))]
		public override ZString WE_TransferFromPalletId
		{
			get { return base.WE_TransferFromPalletId; }
			set
			{
				base.WE_TransferFromPalletId = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_TransferFromPalletId);
			}
		}

		#endregion

		#region Transfer From Location

		#region TransferFromLocation

		public WhsLocation TransferFromLocation => Factory.Load<WhsLocation>(WE_WL_TransferFrom);

		#endregion

		#region TransferFromWarehousePK

		[ReadOnlyMember(nameof(SourceLocationOrPalletIDReadOnly))]
		[List("Lookups.Warehouses")]
		public ZGuid TransferFromWarehousePK
		{
			get
			{
				var result = ZGuid.Empty;
				var docket = Docket;
				if (docket != null)
				{
					if (docket.WD_DocketSubType != TransferType.Codes.InterWhsDest)
					{
						result = docket.WD_WW_Whs;
					}
					else
					{
						result = TransferFromLocation?.WLV_WW_Whs ?? transferFromWarehousePK;
					}
				}
				return result;
			}
			set
			{
				if (TransferFromWarehousePK != value)
				{
					TransferFromLocationString = "";
				}
				transferFromWarehousePK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTransferFromWarehousePK();
				}

				TransferFromWarehousePKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TransferFromWarehousePKInfo => GetZPropertyInfo(Schema.TransferFromWarehousePK);

		ZGuid transferFromWarehousePK;

		#endregion

		#region TransferFromWarehouse

		public WhsWarehouse TransferFromWarehouse => Factory.Load<WhsWarehouse>(TransferFromWarehousePK);

		#endregion

		#region TransferFromLocationString

		[ReadOnlyMember(nameof(SourceLocationOrPalletIDReadOnly))]
		[List("Lookups.TransferFromLocations")]
		[MaxLength(Schema.LocationStringMaxLength)]
		public ZString TransferFromLocationString
		{
			get => TransferFromLocation?.WLV_LocationString_UserFriendly ?? transferFromLocationString;
			set
			{
				var previousValue = TransferFromLocationString;
				if (previousValue != value)
				{
					SetNonPersistentPropertyValue(TransferFromLocationStringInfo, ref transferFromLocationString, value);
					WE_WL_TransferFrom = GetLocationPK(TransferFromWarehouse, value);

					DeleteMatchingLinesIfPropertyNoLongerMatches(Schema.TransferFromLocationString);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTransferFromLocationString();
					}

					TransferFromLocationStringInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TransferFromLocationStringInfo => GetZPropertyInfo(Schema.TransferFromLocationString);

		ZString transferFromLocationString;

		#endregion

		#endregion

		#region DestinationWarehousePK

		[ReadOnlyMember(nameof(DestWarehouseReadOnly))]
		[List("Lookups.Warehouses")]
		[ResourceStringData("WhsTransferLine|DestinationWarehousePK", ShortCaption = "Whs. (Dest.)", Caption = "Destination Warehouse")]
		public ZGuid DestinationWarehousePK
		{
			get => WarehousePK; // GetWarehousePK() returns destinationWarehousePK and is used by WarehousePK
			set
			{
				SetValueOnAllMatchingLines(Schema.DestinationWarehousePK, value);

				if (DestinationWarehousePK != value)
				{
					LocationString = "";
				}

				SetNonPersistentPropertyValue(DestinationWarehousePKInfo, ref destinationWarehousePK, value);

				// SetNonPersistentPropertyValue will not set the destinationWarehousePK field if the getter of 'DestinationWarehousePK' is the same as 'value'.
				// When the transfer line is first loaded, we attempt to set the destinationWarehousePK through the setter of 'DestinationWarehousePK'.
				// Since 'value' and 'DestinationWarehousePK' will have the same value, the field is not updated, we manually set it here to solve this use case.
				destinationWarehousePK = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDestinationWarehousePK();
				}
			}
		}

		protected override ZGuid GetWarehousePK(WhsDocket docket)
		{
			return docket.WD_DocketSubType != TransferType.Codes.InterWhsSource
				? base.GetWarehousePK(docket)
				: Location?.WLV_WW_Whs ?? GetNonPersistentDestinationWarehousePK();
		}

		ZGuid GetNonPersistentDestinationWarehousePK()
		{
			ZGuid result;

			if (IsChildTransferLine)
			{
				result = MasterTransferLine?.DestinationWarehousePK ?? destinationWarehousePK;
			}
			else
			{
				result = destinationWarehousePK;
			}

			return result;
		}

		public ZPropertyInfo DestinationWarehousePKInfo
		{
			get { return GetZPropertyInfo(Schema.DestinationWarehousePK); }
		}

		bool DestWarehouseReadOnly => DestLocationReadOnly || IsPicked;

		ZGuid destinationWarehousePK;

		#endregion

		#region LocationStringCore

		protected override ZString LocationStringCore
		{
			get { return base.LocationStringCore; }
			set
			{
				using (new SemaphoreManager(SettingLocationStringSemaphore))
				{
					SetValueOnAllMatchingLines(WhsDocketLine.Schema.LocationString, value);
					SyncValueToInterWhsChildLine(WhsDocketLine.Schema.LocationString, value);
					base.LocationStringCore = value;
				}
			}
		}

		Semaphore SettingLocationStringSemaphore
		{
			get { return settingLocationStringSemaphore ?? (settingLocationStringSemaphore = new Semaphore()); }
		}

		Semaphore settingLocationStringSemaphore;

		#endregion

		#region WE_F3_NKPackType

		public override ZString WE_F3_NKPackType
		{
			get { return base.WE_F3_NKPackType; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_F3_NKPackType, value);
				base.WE_F3_NKPackType = value;
			}
		}

		protected override ZString GetPackTypeFromProductParams(WhsProductParamsByWhsAndClient productParams)
		{
			return !productParams.W3_F3_NKReleasedPackType.IsEmpty ? productParams.W3_F3_NKReleasedPackType : productParams.W3_F3_NKReceivedPackType;
		}

		#endregion

		#region WE_LineComment

		public override ZString WE_LineComment
		{
			get { return base.WE_LineComment; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_LineComment, value);
				base.WE_LineComment = value;
			}
		}

		#endregion

		#region WE_PalletID

		public override ZString WE_PalletID
		{
			get => base.WE_PalletID;
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_PalletID, value);
				SyncValueToInterWhsChildLine(WhsDocketLine.Schema.WE_PalletID, value);
				base.WE_PalletID = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_WL();
				}
			}
		}

		#endregion

		#region WE_DocketLineStatus

		[ReadOnly(true)]
		public override ZString WE_DocketLineStatus
		{
			get { return base.WE_DocketLineStatus; }
			set { base.WE_DocketLineStatus = value; }
		}

		#endregion

		#region WE_WL

		public override ZGuid WE_WL
		{
			get => base.WE_WL;
			set
			{
				if (ShouldClearPalletID(value))
				{
					WE_PalletID = "";
				}

				if (!SettingLocationStringSemaphore.IsSuspended)
				{
					SetValueOnAllMatchingLines(WhsDocketLineSchema.Constants.WE_WL, value);
					SyncValueToInterWhsChildLine(WhsDocketLine.Schema.WE_WL, value);
				}

				base.WE_WL = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PalletID();
				}
			}
		}

		[ReadOnlyMember(nameof(SourceLocationOrPalletIDReadOnly))]
		[List("Lookups.TransferFromLocations")]
		public override ZGuid WE_WL_TransferFrom
		{
			get { return base.WE_WL_TransferFrom; }
			set
			{
				base.WE_WL_TransferFrom = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransferFromLocationString();
				}
			}
		}

		bool ShouldClearPalletID(ZGuid locationPK)
		{
			var result = false;

			var docket = Docket;
			if (docket != null)
			{
				result = !Product?.PickFaces.FindByLocation(docket.WD_OH_Client, locationPK)?.Location.LocationType.WLT_RetainPalletIDsInFixedPickFaces ?? false;
			}

			return result;
		}

		#endregion

		#region WE_OriginalInventoryStatus

		[List("Lookups.InventoryStatuses")]
		public override ZString WE_OriginalInventoryStatus
		{
			get { return base.WE_OriginalInventoryStatus; }
			set
			{
				base.WE_OriginalInventoryStatus = value;

				// When inventory status change REC -> PTA, PTA -> PUT. Matching transfer lines should have the same Inventory Status.
				if (IsPutawayTransferLine)
				{
					SetValueOnAllMatchingLines(WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, value);
				}
				else
				{
					DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus);
				}
			}
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

		#region WE_GS_NKPutawayBy

		public override ZString WE_GS_NKPutawayBy
		{
			get { return base.WE_GS_NKPutawayBy; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_GS_NKPutawayBy, value);
				SyncValueToInterWhsChildLine(WhsDocketLine.Schema.WE_GS_NKPutawayBy, value);
				base.WE_GS_NKPutawayBy = value;
			}
		}

		#endregion

		#region WE_PutawayTime

		[BusinessObjectTestExclude]
		[SettingInvalidDateOnDateTimePropertyTestExclude]
		public override ZDateTimeOffset WE_PutawayTime
		{
			get { return base.WE_PutawayTime; }
			set
			{
				var isFinalising = this.IsMainTransactionLine() ? IsFinalisingOrParentFinalising : MatchingParentLine.IsFinalisingOrParentFinalising;
				if (!isFinalising && value != WE_PutawayTime)
				{
					throw new InvalidOperationException("Cannot set PutawayTime if not finalising the Transfer Line.");
				}

				SetValueOnAllMatchingLines(WhsDocketLineSchema.Constants.WE_PutawayTime, value);
				base.WE_PutawayTime = value;
				SetPutawayByIfNecessary();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_GS_NKPutawayBy();
				}
			}
		}

		bool IsFinalisingOrParentFinalising => IsChildTransferLine ? MasterTransferLine.IsFinalising : IsFinalising;

		void SetPutawayByIfNecessary()
		{
			if (WE_GS_NKPutawayBy.IsEmpty && IsPutaway)
			{
				WE_GS_NKPutawayBy = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#endregion

		#region LocalisedPutawayTime

		public ZDateTime LocalisedPutawayTime => WE_PutawayTime.ToLocalZDateTime();

		#endregion

		#region Picked By

		#region GS_NKPickedBy

		[RelatedBusinessObject("PickedBy")]
		[ReadOnlyMember(nameof(PickedByReadonly))]
		[List("Lookups.PickedBys")]
		[MaxLength(3)]
		public ZString GS_NKPickedBy
		{
			get
			{
				ZString result;

				if (!IsChildTransferLine)
				{
					result = (PickLines.Count > 0) ? PickLines.FirstOrDefault(l => !l.WZ_GS_NKAssignedTo.IsEmpty)?.WZ_GS_NKAssignedTo ?? ZString.Empty
						: pickedByCode;
				}
				else
				{
					result = MasterTransferLine?.GS_NKPickedBy ?? ZString.Empty;
				}

				return result;
			}
			set
			{
				if (IsChildTransferLine) // Only Master Transfer Line has PickLines
				{
					throw new InvalidOperationException("Cannot set Picked By on Child Transfer Line.");
				}

				CheckMaximumLength(GS_NKPickedByInfo, value);
				SetPickedByCode(value);
				SetValueOnAllMatchingLines(Schema.GS_NKPickedBy, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePickedBy();
				}
				GS_NKPickedByInfo.RefreshBinding();
			}
		}

		void SetPickedByCode(ZString pickedByUserCode)
		{
			if (PickLines.Count > 0)
			{
				foreach (var line in PickLines)
				{
					line.WZ_GS_NKAssignedTo = pickedByUserCode;
				}
			}
			else
			{
				pickedByCode = pickedByUserCode;
			}
		}

		ZString pickedByCode;

		#endregion

		#region PickedBy

		public GlbStaff PickedBy
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GS_NKPickedBy); }
		}

		#endregion

		#region PickedByInfo

		public ZPropertyInfo GS_NKPickedByInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NKPickedBy, Schema.GS_NKPickedBy); }
		}

		#endregion

		bool PickedByReadonly
		{
			get { return IsHeldForTransfer || IsPutaway || IsTransferringForOrder || (Warehouse?.WW_GG_ReleaseGroup.IsValid ?? false) || base.StandardReadOnly; }
		}

		#endregion

		#region PickedTime

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsPickedReadOnly))]
		[ResourceStringData("WhsDocketLine|PickedTime", Caption = "Picked Time")]
		public ZDateTimeOffset PickedTime
		{
			get
			{
				ZDateTimeOffset result;

				if (!IsChildTransferLine)
				{
					result = PickLines.FirstOrDefault(pl => pl.IsPicked)?.WZ_PickedDateTime ?? ZDateTimeOffset.Empty;
				}
				else
				{
					result = MasterTransferLine?.PickedTime ?? ZDateTimeOffset.Empty;
				}

				return result;
			}
			set
			{
				if (IsChildTransferLine) // Only Master Transfer Line has PickLines
				{
					throw new InvalidOperationException("Cannot set Picked Time on Child Transfer Line.");
				}

				var isFinalising = this.IsMainTransactionLine() ? IsFinalising : MatchingParentLine.IsFinalising;
				if (!isFinalising && IsFinalised)
				{
					throw new InvalidOperationException("Cannot change Picked Time of Finalised Transfer Line.");
				}

				var canSetPickedTime = CommitStock(value); // if we are picking stock we need to fully allocate pick lines to the transfer line
				if (canSetPickedTime)
				{
					var warehouseLocalisedValue = Warehouse.GetWarehouseBranchDateTimeOffset(value.ToUtcZDateTime());

					SetValueOnAllMatchingLines(Schema.PickedTime, warehouseLocalisedValue);
					SetPickedTimeOnPickLines(warehouseLocalisedValue);

					if (!IsMatchingLine) // Method below handles matching lines + children
					{
						CreateOrRemoveInTransitInventory(canRemoveInTransitInventory: true);
					}
				}

				SetDocketLineStatus();
				SetPickedTimeToCurrentUser();
				PickedTimeInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidatePickedTime(value);
					Validation.ValidatePickedBy();
				}
			}
		}

		bool CommitStock(ZDateTimeOffset value)
		{
			// attempt to Commit Stock if we are picking
			if (ShouldCommitTransferLine && value.IsValid && !IsPicked && !IsFinalising)
			{
				RunPreSaveValidation(); // this will commit stock and run validation
			}

			// when picking, you can only do so if there are no errors, otherwise you must be unpicking
			return (value.IsEmpty || !HasErrors);
		}

		void SetPickedTimeOnPickLines(ZDateTimeOffset value)
		{
			if (PickLines.Count > 0)
			{
				foreach (var pickLine in PickLines)
				{
					pickLine.WZ_PickedDateTime = value;
				}
			}
		}

		void SetDocketLineStatus()
		{
			if (WE_DocketLineStatus != DocketLineStatus.Codes.Finalised)
			{
				if (IsPicked)
				{
					WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;

					if (IsPutawayTransferLine)
					{
						PickLines.ForEach(p => p.InventoryLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload);
					}
				}
				else
				{
					WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
				}
			}
		}

		void SetPickedTimeToCurrentUser()
		{
			if (IsPicked && GS_NKPickedBy.IsEmpty)
			{
				GS_NKPickedBy = GlbStaff.CurrentUser.GS_Code;
			}
			else
			{
				// PickedBy may have been be automatically set on Pick Lines, so refresh our proxy property.
				GS_NKPickedByInfo.RefreshBinding();
			}
		}

		void ValidatePickedTime(ZDateTimeOffset value)
		{
			try
			{
				PickedTimeUserAttemptedToEnter = value;
				Validation.Validate_PickedTime();
			}
			finally
			{
				PickedTimeUserAttemptedToEnter = null;
			}
		}

		// tested in WhsTransferLineValidation
		internal ZString GetReasonUserCannotPick()
		{
			var result = ZString.Empty;

			if (PickedTimeUserAttemptedToEnter.GetValueOrDefault().IsValid && !IsChildTransferLine && !IsPicked) // if user attempted to Pick and could not
			{
				result = Res.GetString("091ef399-b61c-4a60-a412-cbaaa809cbd0", "Transfer Line must be fully Committed with no Errors before Picking.");
			}

			return result;
		}

		ZDateTimeOffset? PickedTimeUserAttemptedToEnter { get; set; }

		bool IsPickedReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly || IsHeldForTransfer || IsPutaway || IsChildTransferLine; }
		}

		public ZPropertyInfo PickedTimeInfo
		{
			get { return GetZPropertyInfo(Schema.PickedTime); }
		}

		#endregion

		#region QtyToMoveIncludingMatchingLines

		[ActionField(ReadOnly = true)]
		[ResourceStringData("WhsTransferLine|QtyToMoveIncludingMatchingLines", Caption = "Quantity", ShortCaption = "Qty")]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		public ZDecimal QtyToMoveIncludingMatchingLines
		{
			get { return InventoryCommitmentBuilder.TotalTransactionQty; }
			set
			{
				var currentQty = QtyToMoveIncludingMatchingLines;
				if (currentQty <= value) // increase qty
				{
					WE_TransactionQuantity = value - MatchingLines.Sum(l => l.WE_TransactionQuantity);
				}
				else
				{
					DecreaseTotalQty(currentQty, WhsDocketLineSchema.Constants.WE_TransactionQuantity, value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateQtyToMoveIncludingMatchingLines();
				}

				QtyToMoveIncludingMatchingLinesInfo.RefreshBinding();
			}
		}

		void DecreaseTotalQty(ZDecimal currentQty, ZString linePropertyName, ZDecimal newValue)
		{
			var qtyToUnallocate = currentQty - newValue;

			for (int i = MatchingLines.Count - 1; i >= 0; i--)
			{
				var line = MatchingLines[i];
				var linePropertyInfo = line.ZPropertyInfoHash[linePropertyName];
				var lineValue = (ZDecimal)linePropertyInfo.Value;

				// delete line if decreased to 0
				if (qtyToUnallocate >= lineValue)
				{
					qtyToUnallocate -= lineValue;
					line.Delete();
				}
				// line has enough to fulfill decrement, decrease and exit loop
				else
				{
					linePropertyInfo.Value = (ZDecimal)(lineValue - qtyToUnallocate);
					qtyToUnallocate = 0m;
					break;
				}
			}

			// after deleting all matching lines, if there is more to decrement, decrease the main line (this)
			if (qtyToUnallocate > 0m)
			{
				var mainLinePropertyInfo = ZPropertyInfoHash[linePropertyName];
				mainLinePropertyInfo.Value = (ZDecimal)((ZDecimal)mainLinePropertyInfo.Value - qtyToUnallocate);
			}
		}

		public ZPropertyInfoDecimal QtyToMoveIncludingMatchingLinesInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.QtyToMoveIncludingMatchingLines); }
		}

		#endregion

		#region PackQtyIncludingMatchingLines

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		[ResourceStringData("WhsTransferLine|PackQtyIncludingMatchingLines", Caption = "Packs")]
		public ZDecimal PackQtyIncludingMatchingLines
		{
			get { return this.GetQtyIncludingMatchingLines(l => l.WE_PackQuantity); }
			set
			{
				var currentQty = PackQtyIncludingMatchingLines;
				if (currentQty <= value) // increase qty
				{
					WE_PackQuantity = value - MatchingLines.Sum(l => l.WE_PackQuantity);
				}
				else
				{
					DecreaseTotalQty(currentQty, WhsTransferLine.Schema.WE_PackQuantity, value);
				}

				PackQtyIncludingMatchingLinesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PackQtyIncludingMatchingLinesInfo
		{
			get { return GetZPropertyInfo(Schema.PackQtyIncludingMatchingLines); }
		}

		#endregion

		#region QtyCommittedIncludingMatchingLines

		[ResourceStringData("WhsTransferLine|QtyCommittedIncludingMatchingLines", Caption = "Committed Qty")]
		public ZDecimal QtyCommittedIncludingMatchingLines
		{
			get { return InventoryCommitmentBuilder.TotalQtyCommitted; }
		}

		public ZPropertyInfo QtyCommittedIncludingMatchingLinesInfo
		{
			get { return GetZPropertyInfo(Schema.QtyCommittedIncludingMatchingLines); }
		}

		#endregion

		#region ReadOnly

		#region ReadOnly

		public override bool ReadOnly => IsReadyForPlanningOrPlanned || base.ReadOnly;

		#endregion

		#region StandardReadOnly

		protected override bool StandardReadOnly
		{
			get { return IsHeldForTransfer || IsChildTransferLine || IsPutaway || IsPutawayTransferLine || IsTransferringForOrder || base.StandardReadOnly; }
		}

		#endregion

		#region PackQuantityAndTypeAndRelatedQuantityInfoReadOnly

		protected override bool PackQuantityAndTypeAndRelatedQuantityInfoReadOnly
		{
			get { return IsHeldForTransfer || IsPutaway || IsVASOrderTransfer || IsPutawayTransferLine || base.PackQuantityAndTypeAndRelatedQuantityInfoReadOnly; }
		}

		#endregion

		#region AfterPickProductAndAttribsReadOnly

		protected override bool AfterPickProductAndAttribsReadOnly
		{
			get { return IsHeldForTransfer || IsPutaway || IsVASOrderTransfer || IsPutawayTransferLine || base.AfterPickProductAndAttribsReadOnly; }
		}

		#endregion

		protected override bool ProductReadOnly => AfterPickProductAndAttribsReadOnly;

		#region SourceLocationOrPalletIDReadOnly

		bool SourceLocationOrPalletIDReadOnly => IsJobFinalisedOrCancelledReadOnly || IsHeldForTransfer || IsPutaway || IsChildTransferLine || IsOutOfServiceAreaTransferForVASOrder || IsPutawayTransferLine || IsTransferringForOrder;

		#endregion

		#region DestLocation

		protected override bool DestLocationReadOnly
		{
			get { return IsPutaway || IsChildTransferLine || IsIntoServiceAreaTransferForVASOrder || IsTransferringForOrder || base.DestLocationReadOnly; }
		}

		#endregion

		#region DestPalletID

		protected override bool DestPalletIDReadOnly
		{
			get { return IsPutaway || IsChildTransferLine || IsPutawayTransferLine || IsTransferringForOrder || IsInTransitLineOnInventoryEditForm || base.DestPalletIDReadOnly; }
		}

		#endregion

		#region IsJobFinalisedOrCancelledReadOnly

		protected override bool IsJobFinalisedOrCancelledReadOnly
		{
			get { return IsFinalised || base.IsJobFinalisedOrCancelledReadOnly; }
		}

		#endregion

		#region IsTransferredReadOnly

		protected override bool IsTransferredReadOnly => IsPutaway || IsChildTransferLine || IsTransferringForOrder || (Warehouse?.WW_GG_ReleaseGroup.IsValid ?? false) || base.IsTransferredReadOnly;

		#endregion

		#region PartAttributes

		protected override bool CommonPartAttribReadOnly => IsHeldForTransfer || IsPutaway || base.CommonPartAttribReadOnly;

		#endregion

		#region IsPutawayTransferLine

		public bool IsPutawayTransferLine
		{
			get { return (Docket?.WD_IsPutawayTransfer) ?? false; }
		}

		#endregion

		#region AssociatedReceiveLineOfPutawayTransfer

		public WhsReceiveLine AssociatedReceiveLineOfPutawayTransfer
		{
			get
			{
				WhsReceiveLine result = null;
				if (IsPutawayTransferLine)
				{
					var firstPickLine = PickLines[0];
					if (firstPickLine != null)
					{
						result = Factory.Load<WhsReceiveLine>(firstPickLine.WZ_WE_InventoryLine);
					}
				}

				return result;
			}
		}

		#endregion

		#region IsInTransitLineOnInventoryEditForm

		bool IsInTransitLineOnInventoryEditForm
		{
			get { return IsInventoryEditForm && IsInTransit; }
		}

		#endregion

		#endregion

		#region WE_SerialNumber

		public override ZString WE_SerialNumber
		{
			get => base.WE_SerialNumber;
			set
			{
				var previousValue = base.WE_SerialNumber;
				base.WE_SerialNumber = value;
				DeleteMatchingLinesIfPropertyNoLongerMatches(WhsDocketLineSchema.Constants.WE_SerialNumber);
				if (!IsValidationSuspended && previousValue != value)
				{
					Validation.ValidateQtyToMoveIncludingMatchingLines();
				}
			}
		}

		#endregion

		#region CustomAttributes

		public override ZString WE_CustomAttrib1
		{
			get { return base.WE_CustomAttrib1; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib1, value);
				base.WE_CustomAttrib1 = value;
			}
		}

		public override ZString WE_CustomAttrib2
		{
			get { return base.WE_CustomAttrib2; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib2, value);
				base.WE_CustomAttrib2 = value;
			}
		}

		public override ZString WE_CustomAttrib3
		{
			get { return base.WE_CustomAttrib3; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib3, value);
				base.WE_CustomAttrib3 = value;
			}
		}

		public override ZString WE_CustomAttrib4
		{
			get { return base.WE_CustomAttrib4; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib4, value);
				base.WE_CustomAttrib4 = value;
			}
		}

		public override ZString WE_CustomAttrib5
		{
			get { return base.WE_CustomAttrib5; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib5, value);
				base.WE_CustomAttrib5 = value;
			}
		}

		public override ZString WE_CustomAttrib6
		{
			get { return base.WE_CustomAttrib6; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomAttrib6, value);
				base.WE_CustomAttrib6 = value;
			}
		}

		public override ZDateTime WE_CustomDate1
		{
			get { return base.WE_CustomDate1; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDate1, value);
				base.WE_CustomDate1 = value;
			}
		}

		public override ZDateTime WE_CustomDate2
		{
			get { return base.WE_CustomDate2; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDate2, value);
				base.WE_CustomDate2 = value;
			}
		}

		public override ZDateTime WE_CustomDate3
		{
			get { return base.WE_CustomDate3; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDate3, value);
				base.WE_CustomDate3 = value;
			}
		}

		public override ZDateTime WE_CustomDate4
		{
			get { return base.WE_CustomDate4; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDate4, value);
				base.WE_CustomDate4 = value;
			}
		}

		public override ZDateTime WE_CustomDate5
		{
			get { return base.WE_CustomDate5; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDate5, value);
				base.WE_CustomDate5 = value;
			}
		}

		public override ZDecimal WE_CustomDecimal1
		{
			get { return base.WE_CustomDecimal1; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDecimal1, value);
				base.WE_CustomDecimal1 = value;
			}
		}

		public override ZDecimal WE_CustomDecimal2
		{
			get { return base.WE_CustomDecimal2; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDecimal2, value);
				base.WE_CustomDecimal2 = value;
			}
		}

		public override ZDecimal WE_CustomDecimal3
		{
			get { return base.WE_CustomDecimal3; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDecimal3, value);
				base.WE_CustomDecimal3 = value;
			}
		}

		public override ZDecimal WE_CustomDecimal4
		{
			get { return base.WE_CustomDecimal4; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDecimal4, value);
				base.WE_CustomDecimal4 = value;
			}
		}

		public override ZDecimal WE_CustomDecimal5
		{
			get { return base.WE_CustomDecimal5; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomDecimal5, value);
				base.WE_CustomDecimal5 = value;
			}
		}

		public override ZBool WE_CustomFlag1
		{
			get { return base.WE_CustomFlag1; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomFlag1, value);
				base.WE_CustomFlag1 = value;
			}
		}

		public override ZBool WE_CustomFlag2
		{
			get { return base.WE_CustomFlag2; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomFlag2, value);
				base.WE_CustomFlag2 = value;
			}
		}

		public override ZBool WE_CustomFlag3
		{
			get { return base.WE_CustomFlag3; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomFlag3, value);
				base.WE_CustomFlag3 = value;
			}
		}

		public override ZBool WE_CustomFlag4
		{
			get { return base.WE_CustomFlag4; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomFlag4, value);
				base.WE_CustomFlag4 = value;
			}
		}

		public override ZBool WE_CustomFlag5
		{
			get { return base.WE_CustomFlag5; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomFlag5, value);
				base.WE_CustomFlag5 = value;
			}
		}

		public override ZString WE_CustomTextBlob1
		{
			get { return base.WE_CustomTextBlob1; }
			set
			{
				SetValueOnAllMatchingLines(WhsDocketLine.Schema.WE_CustomTextBlob1, value);
				base.WE_CustomTextBlob1 = value;
			}
		}

		#endregion

		#region SetValueOnAllMatchingLines

		void SetValueOnAllMatchingLines(ZString propertyName, IZType value)
		{
			foreach (var line in MatchingLines)
			{
				line.ZPropertyInfoHash[propertyName].Value = value;
			}
		}

		#endregion

		#region SyncValueOnChildLine

		void SyncValueToInterWhsChildLine(ZString propertyName, IZType value)
		{
			var child = ChildTransferLine;
			if (child != null)
			{
				child.ZPropertyInfoHash[propertyName].Value = value;
			}
		}

		#endregion

		#region SupportsHoldCodeChange

		protected override bool SupportsHoldCodeChange
		{
			get { return true; }
		}

		#endregion

		#region DeleteMatchingLinesIfPropertyNoLongerMatches

		void DeleteMatchingLinesIfPropertyNoLongerMatches(ZString propertyName)
		{
			if (this.IsMainTransactionLine())
			{
				var propertyValue = ZPropertyInfoHash[propertyName].Value;

				for (int i = MatchingLines.Count - 1; i >= 0; i--)
				{
					var line = MatchingLines[i];
					if (!propertyValue.Equals(line.ZPropertyInfoHash[propertyName].Value))
					{
						WE_TransactionQuantity += line.WE_TransactionQuantity;
						line.Delete(); // line no longer matches, delete it.
					}
				}
			}
		}

		#endregion

		#region ShouldSuppressDateRangeValidationErrors

		protected override bool ShouldSuppressDateRangeValidationErrors => true;

		#endregion

		#endregion

		#region TransferLineFromInventory

		TransferLineFromInventoryHelper TransferLineFromInventoryHelper
		{
			get
			{
				if (transferLineFromInventoryHelper == null)
				{
					var docket = Docket;
					transferLineFromInventoryHelper = new TransferLineFromInventoryHelper(docket.NotificationSubscriber, docket);
				}
				return transferLineFromInventoryHelper;
			}
		}
		TransferLineFromInventoryHelper transferLineFromInventoryHelper;

		#endregion

		#region AcceptInventoryLinesFromSearchGrid

		public void SetDocketLineFromInventory(WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			TransferLineFromInventoryHelper.SetDocketLineFromInventory(this, inventoryLine, exclude);
		}

		#endregion

		#region Find Attributes

		protected override void UseChosenInventoryRowFindAttributesCore(WhsInventoryView inventory)
		{
			base.UseChosenInventoryRowFindAttributesCore(inventory);
			WE_WL_TransferFrom = inventory.WI_WL;
			WE_PalletID = inventory.WI_PalletID;
			WE_AdjustmentArrivalDate = inventory.WI_ArrivalDate;
		}

		#endregion

		#region UncommitOverPickedOrNotMatchingInventory

		public void UncommitOverPickedOrNotMatchingInventory()
		{
			if (!IsFinalised && !IsInTransit && !IsChildTransferLine)
			{
				InventoryCommitmentBuilder.UncommitOverPickedOrNonMatchingInventory();
			}
		}

		#endregion

		#region CommitInventory

		public void CommitInventory()
		{
			if (ShouldCommitTransferLine)
			{
				InventoryCommitmentBuilder.CommitInventory();
			}
		}

		bool ShouldCommitTransferLine => !IsFinalised && !IsInTransit && !IsChildTransferLine && this.IsMainTransactionLine();

		#endregion

		#region CreateOrRemoveInTransitInventory

		public void CreateInTransitInventory()
		{
			if (IsFinalised)
			{
				throw new NotSupportedException("Attempt to create In-Transit Inventory for finalised transfer!");
			}

			CreateOrRemoveInTransitInventory(canRemoveInTransitInventory: false);
		}

		void CreateOrRemoveInTransitInventory(bool canRemoveInTransitInventory)
		{
			if (!IsFinalised)
			{
				if (!IsChildTransferLine) // matching lines of the child transfer line will be made in-transit by the master matching lines
				{
					// Must set matching lines first, or they will be deleted when they become out of sync
					foreach (WhsTransferLine matchingLine in MatchingLines)
					{
						matchingLine.CreateOrRemoveInTransitInventory(canRemoveInTransitInventory);
					}
				}

				if (IsPicked)
				{
					CreateInTransitInventoryCore();
				}
				else if (!canRemoveInTransitInventory)
				{
					throw new NotSupportedException("Attempt to create In-Transit Inventory for unpicked transfer line");
				}
				else if (IsInTransit)
				{
					ClearTotalUnitsAndDeleteInventoryRecord();
					UpdateInventoryStatusByOriginalInventoryHeldCode();

					var docket = Docket;
					if (docket.IsInterWarehouseTransfer && docket.IsMasterTransfer)
					{
						ChildTransferLine?.Delete();
					}
				}
			}
		}

		void CreateInTransitInventoryCore()
		{
			var isInterWarehouseSourceTransfer = Docket.WD_DocketSubType.Equals(TransferType.Codes.InterWhsSource);

			if (IsInDatabase && (ZString)WE_OriginalInventoryStatusInfo.OriginalValue == InventoryStatus.Codes.InTransit)
			{
				if (!isInterWarehouseSourceTransfer)
				{
					WE_StockOnHand = WE_TransactionQuantity;
				}
			}
			else
			{
				ClearTotalUnitsAndDeleteInventoryRecord(); // Clear any existing In-Transit inventory so it can be recreated

				WE_OriginalInventoryStatus = IsPutawayTransferLine
					 ? InventoryStatus.Codes.PuttingAway
					 : InventoryStatus.Codes.InTransit;

				if (!isInterWarehouseSourceTransfer)
				{
					using (new SemaphoreManager(IsCreatingInTransitInventorySemaphore))
					{
						IncreaseStockInDestinationLocation(makeInTransit: true);
					}
				}
			}

			var docket = Docket;
			if (docket.IsInterWarehouseTransfer && docket.IsMasterTransfer)
			{
				var childTransferLine = SynchroniseOrCreateChildTransferLine(FindOrCreateChildTransfer());
				childTransferLine.CreateInTransitInventory();
			}
		}

		void ClearTotalUnitsAndDeleteInventoryRecord()
		{
			if (IsFinalised)
			{
				throw new NotSupportedException("Attempt to clear Inventory for finalised transfer!");
			}

			WE_StockOnHand = 0m;
			WE_CurrentHoldReason = ZString.Empty;
			Inventory.RemoveAndDeleteAll();
		}

		Semaphore IsCreatingInTransitInventorySemaphore => isCreatingInTransitInventorySemaphore ?? (isCreatingInTransitInventorySemaphore = new Semaphore());
		Semaphore isCreatingInTransitInventorySemaphore;

		#endregion

		#region Finalisation

		public void FinaliseDocketLine()
		{
			if (!IsFinalised)
			{
				using (new SemaphoreManager(FinaliseDocketLineSemaphore))
				{
					ValidateAndFinaliseDocketLine();
				}
			}
			else
			{
				Docket.NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.LineIsFinalised));
			}
		}

		#region Finalise Docket Line Semaphore

#if DEBUG
		public
#endif
		Semaphore FinaliseDocketLineSemaphore
		{
			get { return finaliseDocketLineSemaphore ?? (finaliseDocketLineSemaphore = new Semaphore()); }
		}

		Semaphore finaliseDocketLineSemaphore;

		#endregion

		#region ValidateAndFinaliseDocketLine

		void ValidateAndFinaliseDocketLine()
		{
			var validationPassed = RunPreFinaliseValidation();
			if (validationPassed)
			{
				FinaliseDocketLineCore();
			}
		}

		#region RunPreFinaliseValidation

		bool RunPreFinaliseValidation()
		{
			if (!Docket.IsFinalising)
			{
				RunPreSaveValidation();
			}
			return !HasErrors;
		}

		#endregion

		#region FinaliseDocketLineCore

		void FinaliseDocketLineCore()
		{
			UpdateInventory();
			UpdateInventoryStatusForPutawayTransferLines();
			CreateChildInterWarehouseRecord();
			FinaliseMatchingLines();
			UpdateStatusFromInTransit();
			FinalValidation();
			UpdateWhenStockOnHandChanged();
			RemoveIsPuttingAwayFromAssociatedPutawayLine();
		}

		#region UpdateStatusFromInTransit

		void UpdateStatusFromInTransit()
		{
			// Keep matching lines in sync to prevent them being deleted
			foreach (var matchingLine in MatchingLines.Cast<WhsTransferLine>())
			{
				matchingLine.UpdateStatusFromInTransit();
			}

			if (IsInTransit)
			{
				UpdateInventoryStatusByOriginalInventoryHeldCode();
				SetStatusToStagedOrReadyToPackIfNecessary();
			}
		}

		void SetStatusToStagedOrReadyToPackIfNecessary()
		{
			var docket = Docket;
			if (docket.IsTransferringForOrder)
			{
				WE_CurrentInventoryStatus = GetInventoryStatus(Location); // Pick Available, create Staged/ReadyToPack
			}
			else if (docket.IsReturnStockTransfer)
			{
				// Tested in WhsReleaseLine_ReturnStockTest
				WE_OriginalInventoryStatus = GetInventoryStatus(TransferFromLocation); // Edge Case: Pick Staged/ReadyToPack, return as Available
				WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			}
			else if (IsIntoServiceAreaTransferForVASOrder)
			{
				WE_CurrentInventoryStatus = InventoryStatus.Codes.Staged;
			}

			string GetInventoryStatus(WhsLocation location)
			{
				return location.IsPackingStationLocation || location.IsPackingConsolidationLocation ? InventoryStatus.Codes.ReadyToPack : InventoryStatus.Codes.Staged;
			}
		}

		#region UpdateInventoryStatusByOriginalInventoryHeldCode

		void UpdateInventoryStatusByOriginalInventoryHeldCode()
		{
			if (!WE_WHC_NKOriginalInventoryHeldCode.IsEmpty)
			{
				WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
				WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			}
			else
			{
				WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
				WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			}
		}

		#endregion

		#endregion

		#region UpdateInventoryStatusForPutawayTransferLines

		void UpdateInventoryStatusForPutawayTransferLines()
		{
			if (IsPutawayTransferLine && IsFinalising)
			{
				WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
				WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			}
		}

		#endregion

		#region UpdateInventory

		void UpdateInventory()
		{
			DecreaseStockInSourceLocationIfNotAlreadyPicked();
			SetDocketLineStatus();

			// For case when one finalises a transfer with picking it first.
			var notInTransitAndHasNoInventory = !IsInTransit && Inventory.Count == 0;
			if (notInTransitAndHasNoInventory)
			{
				IncreaseStockInDestinationLocation(makeInTransit: false);
			}
		}

		#region DecreaseStockInSourceLocationIfNotAlreadyPicked

		void DecreaseStockInSourceLocationIfNotAlreadyPicked()
		{
			this.DecreaseStockInLocation(QtyToMoveIncludingMatchingLinesInfo);
		}

		#endregion

		#region IncreaseStockInDestinationLocation

		void IncreaseStockInDestinationLocation(bool makeInTransit)
		{
			var isInterWarehouseSourceTransfer = Docket.WD_DocketSubType.Equals(TransferType.Codes.InterWhsSource);

			if (!isInterWarehouseSourceTransfer) // InterWhsSource Transfers have no inventory attached to them.
			{
				var pickLines = (!IsChildTransferLine) ? PickLines : MasterTransferLine.PickLines;
				foreach (WhsPickLine pickLine in pickLines)
				{
					IncreaseStockInDestinationLocation(pickLine, makeInTransit);
				}

				var holdReason = pickLines.Count == 1 ? pickLines[0].InventoryLine.WE_CurrentHoldReason : ZString.Empty;
				WE_CurrentHoldReason = holdReason;
			}
		}

		#region MasterTransferLine

		WhsTransferLine MasterTransferLine
		{
			get { return Factory.Load<WhsTransferLine>(WE_WE_ParentDocketLine); }
		}

		#endregion

		#region CreateNewInventory

		void IncreaseStockInDestinationLocation(WhsPickLine pickLine, bool makeInTransit)
		{
			var sourceInventory = pickLine.Inventory;
			if (WE_AdjustmentArrivalDate.IsEmpty)
			{
				WE_AdjustmentArrivalDate = sourceInventory.WI_ArrivalDate;
			}

			// this will prevent weird behavior related to unit conversions in IncreaseStockInLocation method
			PokePackQuantity();

			var originalInDocketLineForRating = Docket.WD_DocketSubType == TransferType.Codes.InterWhsDest ? PK : sourceInventory.WI_WE_OriginalInDocketLineForRating;

			if (makeInTransit && !IsPutawayTransferLine)
			{
				this.IncreaseStockInLocation(originalInDocketLineForRating, pickLine.WZ_Units, InventoryStatus.Codes.InTransit);
			}
			// Stock transferred into the Service Area should not be pickable.
			else if (IsIntoServiceAreaTransferForVASOrder)
			{
				this.IncreaseStockInLocation(originalInDocketLineForRating, pickLine.WZ_Units, InventoryStatus.Codes.Staged);
			}
			// Stock transferred out of the Service Area should be pickable.
			else if (IsOutOfServiceAreaTransferForVASOrder)
			{
				this.IncreaseStockInLocation(originalInDocketLineForRating, pickLine.WZ_Units, InventoryStatus.Codes.Available);
			}
			else
			{
				this.IncreaseStockInLocation(originalInDocketLineForRating, pickLine.WZ_Units);
			}
		}

		ZDecimal PokePackQuantity() => WE_PackQuantity;

		internal bool IsIntoServiceAreaTransferForVASOrder => !IsPutawayTransferLine
			&& Factory.LoadTop1<WhsVASOrder>(new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, WE_WD)) != null;

		internal bool IsOutOfServiceAreaTransferForVASOrder => !IsPutawayTransferLine
			&& Factory.LoadTop1<WhsVASOrder>(new ZQuery(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, WE_WD)) != null;

		bool IsVASOrderTransfer => IsIntoServiceAreaTransferForVASOrder || IsOutOfServiceAreaTransferForVASOrder;

		#endregion

		#endregion

		#endregion

		#region CreateChildInterWarehouseRecord

		void CreateChildInterWarehouseRecord()
		{
			var docket = Docket;
			if (docket.IsInterWarehouseTransfer && docket.IsMasterTransfer)
			{
				var childTransfer = FindOrCreateChildTransfer();
				SynchroniseOrCreateAndFinaliseChildTransferLine(childTransfer);
			}
		}

		#region FindOrCreateChildTransfer

		WhsTransfer FindOrCreateChildTransfer()
			=> FindChildTransfer()
				?? CreateChildTransfer();

		#region FindChildTransfer

		WhsTransfer FindChildTransfer()
		{
			var docket = Docket;
			foreach (var transfer in Docket.ChildTransfers)
			{
				// find the related transfer
				if ((docket.WD_DocketSubType == TransferType.Codes.InterWhsSource && transfer.WD_WW_Whs == DestinationWarehousePK) ||
					(docket.WD_DocketSubType == TransferType.Codes.InterWhsDest && transfer.WD_WW_Whs == TransferFromWarehousePK))
				{
					return transfer;
				}
			}
			return null;
		}

		#endregion

		#region CreateChildTransfer

		WhsTransfer CreateChildTransfer()
		{
			var docket = Docket;
			docket.NewRelatedJobsAdded = true;

			var transfer = Factory.New<WhsTransfer>();
			transfer.WD_OH_Client = docket.WD_OH_Client;
			transfer.WD_ExternalReference = docket.WD_ExternalReference;
			transfer.WD_ExternalReferenceSplit = GetNextSplitNumber(docket);
			transfer.WD_DocketSubType = (docket.WD_DocketSubType == TransferType.Codes.InterWhsSource) ? TransferType.Codes.InterWhsDest : TransferType.Codes.InterWhsSource;
			transfer.WD_WD_ParentDocket = docket.PK;
			transfer.WD_WW_Whs = (docket.WD_DocketSubType == TransferType.Codes.InterWhsSource) ? DestinationWarehousePK : TransferFromWarehousePK;

			// custom attributes
			transfer.WD_CustomAttrib1 = docket.WD_CustomAttrib1;
			transfer.WD_CustomAttrib2 = docket.WD_CustomAttrib2;
			transfer.WD_CustomAttrib3 = docket.WD_CustomAttrib3;
			transfer.WD_CustomAttrib4 = docket.WD_CustomAttrib4;
			transfer.WD_CustomAttrib5 = docket.WD_CustomAttrib5;
			transfer.WD_CustomDate1 = docket.WD_CustomDate1;
			transfer.WD_CustomDate2 = docket.WD_CustomDate2;
			transfer.WD_CustomDecimal1 = docket.WD_CustomDecimal1;
			transfer.WD_CustomDecimal2 = docket.WD_CustomDecimal2;
			transfer.WD_CustomDecimal3 = docket.WD_CustomDecimal3;
			transfer.WD_CustomDecimal4 = docket.WD_CustomDecimal4;
			transfer.WD_CustomDecimal5 = docket.WD_CustomDecimal5;
			transfer.WD_CustomFlag1 = docket.WD_CustomFlag1;
			transfer.WD_CustomFlag2 = docket.WD_CustomFlag2;
			transfer.WD_CustomFlag3 = docket.WD_CustomFlag3;
			transfer.WD_CustomFlag4 = docket.WD_CustomFlag4;
			transfer.WD_CustomFlag5 = docket.WD_CustomFlag5;

			return transfer;
		}

		ZByte GetNextSplitNumber(WhsTransfer transfer)
		{
			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, transfer.WD_ExternalReference);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, transfer.WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, transfer.WD_DocketType);
			query.OrderBy = WhsDocketSchema.WD_ExternalReferenceSplit.Name + " DESC";

			ZByte result = 1;
			var highestSplitDocket = Factory.LoadTop1<WhsDocket>(query);
			if (highestSplitDocket != null)
			{
				result = (ZByte)(highestSplitDocket.WD_ExternalReferenceSplit + 1);
			}
			return result;
		}

		#endregion

		#endregion

		#region FindOrCreate_AndFinaliseChildTransferLine

		void SynchroniseOrCreateAndFinaliseChildTransferLine(WhsTransfer childTransfer)
		{
			var childTransferLine = SynchroniseOrCreateChildTransferLine(childTransfer);
			childTransferLine.FinaliseDocketLine();
		}

		WhsTransferLine SynchroniseOrCreateChildTransferLine(WhsTransfer childTransfer)
		{
			var childTransferLine = ChildTransferLine ?? childTransfer.Lines.AddNew();
			childTransferLine.WE_OP = WE_OP;
			childTransferLine.WE_F3_NKPackType = WE_F3_NKPackType;
			childTransferLine.WE_TransactionQuantity = WE_TransactionQuantity;
			childTransferLine.DestinationWarehousePK = DestinationWarehousePK;
			childTransferLine.LocationString = LocationString;
			childTransferLine.WE_PalletID = WE_PalletID;
			childTransferLine.TransferFromWarehousePK = TransferFromWarehousePK;
			childTransferLine.TransferFromLocationString = TransferFromLocationString;
			childTransferLine.WE_TransferFromPalletId = WE_TransferFromPalletId;
			childTransferLine.WE_LineComment = WE_LineComment;
			childTransferLine.WE_BondedEntryKey = WE_BondedEntryKey;
			childTransferLine.WE_ExpiryDate = WE_ExpiryDate;
			childTransferLine.WE_PackingDate = WE_PackingDate;
			childTransferLine.WE_PartAttrib1 = WE_PartAttrib1;
			childTransferLine.WE_PartAttrib2 = WE_PartAttrib2;
			childTransferLine.WE_PartAttrib3 = WE_PartAttrib3;
			childTransferLine.WE_SerialNumber = WE_SerialNumber;
			childTransferLine.WE_WE_ParentDocketLine = PK;
			childTransferLine.WE_GS_NKPutawayBy = WE_GS_NKPutawayBy;
			childTransferLine.WE_OriginalInventoryStatus = WE_OriginalInventoryStatus;
			childTransferLine.WE_WHC_NKOriginalInventoryHeldCode = WE_WHC_NKOriginalInventoryHeldCode;
			childTransferLine.SetDocketLineStatus();

			var matchingParentLine = MatchingParentLine;
			if (matchingParentLine != null)
			{
				var matchingParentLine_ChildTransferLine = matchingParentLine.ChildTransferLine;

				if (matchingParentLine_ChildTransferLine == null)
				{
					matchingParentLine_ChildTransferLine = matchingParentLine.SynchroniseOrCreateChildTransferLine(childTransfer);

					// Matching lines get picked prior to parent lines and ChildTransferLines never get picked, so we must copy the HFT status over
					matchingParentLine_ChildTransferLine.WE_DocketLineStatus = childTransferLine.WE_DocketLineStatus;
				}

				childTransferLine.WE_WE_MatchingLine = matchingParentLine_ChildTransferLine.PK;
				childTransferLine.WE_GS_NKPutawayBy = matchingParentLine_ChildTransferLine.WE_GS_NKPutawayBy;
			}

			// Custom Attributes
			childTransferLine.WE_CustomAttrib1 = WE_CustomAttrib1;
			childTransferLine.WE_CustomAttrib2 = WE_CustomAttrib2;
			childTransferLine.WE_CustomAttrib3 = WE_CustomAttrib3;
			childTransferLine.WE_CustomAttrib4 = WE_CustomAttrib4;
			childTransferLine.WE_CustomAttrib5 = WE_CustomAttrib5;
			childTransferLine.WE_CustomAttrib6 = WE_CustomAttrib6;
			childTransferLine.WE_CustomDecimal1 = WE_CustomDecimal1;
			childTransferLine.WE_CustomDecimal2 = WE_CustomDecimal2;
			childTransferLine.WE_CustomDecimal3 = WE_CustomDecimal3;
			childTransferLine.WE_CustomDecimal4 = WE_CustomDecimal4;
			childTransferLine.WE_CustomDecimal5 = WE_CustomDecimal5;
			childTransferLine.WE_CustomDate1 = WE_CustomDate1;
			childTransferLine.WE_CustomDate2 = WE_CustomDate2;
			childTransferLine.WE_CustomDate3 = WE_CustomDate3;
			childTransferLine.WE_CustomDate4 = WE_CustomDate4;
			childTransferLine.WE_CustomDate5 = WE_CustomDate5;
			childTransferLine.WE_CustomFlag1 = WE_CustomFlag1;
			childTransferLine.WE_CustomFlag2 = WE_CustomFlag2;
			childTransferLine.WE_CustomFlag3 = WE_CustomFlag3;
			childTransferLine.WE_CustomFlag4 = WE_CustomFlag4;
			childTransferLine.WE_CustomFlag5 = WE_CustomFlag5;
			childTransferLine.WE_CustomTextBlob1 = WE_CustomTextBlob1;

			OnChildTransferLineCreated_ForTest(childTransferLine);

			return childTransferLine;
		}

		#endregion

		#endregion

		#region FinaliseMatchingLines

		void FinaliseMatchingLines()
		{
			if (!IsChildTransferLine) // The master is responsible for finalising its child (i.e. Child doesn't finalise child matching lines, parent matching lines do)
			{
				var finalisedAllMatchingLines = true;

				foreach (WhsTransferLine matchingLine in MatchingLines)
				{
					matchingLine.FinaliseDocketLine();

					if (!matchingLine.IsFinalised)
					{
						finalisedAllMatchingLines = false;
						break;
					}
				}

				if (!finalisedAllMatchingLines)
				{
					AddRowErrorForFinalisationError();
				}
			}
		}

		#endregion

		#region FinalValidation

		void FinalValidation()
		{
			var childTransferLine = ChildTransferLine;
			if (!HasErrors && (childTransferLine == null || childTransferLine.IsFinalised))
			{
				OnFinalisationSucceeded(childTransferLine);
			}
			else
			{
				CopyErrorsToOriginalLine();
				AddRowErrorForFinalisationError();
			}
		}

		void OnFinalisationSucceeded(WhsTransferLine childTransferLine)
		{
			WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;

			var docket = Docket;
			var docketForFinalisedDateProvider = docket.IsInterWarehouseTransfer && docket.WD_WD_ParentDocket.IsValid ? (WhsTransfer)docket.ParentDocket : docket;
			var finalisationTime =
				docketForFinalisedDateProvider.GetFinalisedDateProvider()?.GetFinalisationTimeOffset()
				?? Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);

			WE_FinalisedDate = (childTransferLine == null)
				? finalisationTime
				: childTransferLine.WE_FinalisedDate; // to ensure that both transfer lines have same finalised date.

			if (!IsPutaway)
			{
				WE_PutawayTime = WE_FinalisedDate;
			}

			if (this.IsMainTransactionLine()) // Matching lines will have values set by the MainTransactionLine
			{
				var currentUser = GlbStaff.CurrentUser.GS_Code;

				if (!IsChildTransferLine) // Only Master Transfer Lines have PickLines
				{
					if (GS_NKPickedBy.IsEmpty)
					{
						GS_NKPickedBy = currentUser;
					}

					if (!IsPicked)
					{
						PickedTime = WE_FinalisedDate;
					}
					// Synchronise Picked Times if necessary. Since the Matching Lines get finalised separately
					// to the Main Transfer Line, the Picked Times can potentially be different.
					else if (PickLines.Any(pl => !pl.IsFinalised))
					{
						PickedTime = PickedTime;
					}

					if (IsPutawayTransferLine)
					{
						PickLines[0].InventoryLine.Docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
					}

					if (WE_GS_NKPutawayBy.IsEmpty)
					{
						WE_GS_NKPutawayBy = currentUser;
					}
				}
			}
		}

		void CopyErrorsToOriginalLine()
		{
			var childTransferLine = ChildTransferLine;
			if (childTransferLine != null)
			{
				foreach (var notification in childTransferLine.Notifications)
				{
					if (notification.Type.EnumValueName == nameof(NotificationTypes.Error) || notification.Type.EnumValueName == nameof(NotificationTypes.MessageError))
					{
						AddRowError(Res.GetString("9028224a-f6de-4cf9-b2ed-c133f247892b", "Transfer line does not match between warehouses and cannot be finalized.\r\n{0}", notification.Message));
					}
				}
			}

			this.AddErrorsFromCreatedInventory();
		}

		void AddRowErrorForFinalisationError()
		{
			AddRowError(Res.GetString("e394c868-9e4f-4260-a2a3-5f87b4a50d54", "Error occurred during finalization. Close the form without saving and try again."));
		}

		#endregion

		#region UpdateWhenStockOnHandChanged

		void UpdateWhenStockOnHandChanged()
		{
			if (IsFinalised)
			{
				Location.OnStockOnHandChanged();
			}
		}

		#endregion

		#region RemoveIsPuttingAwayFromAssociatedPutawayLine

		void RemoveIsPuttingAwayFromAssociatedPutawayLine()
		{
			var parent = Docket;
			if (parent != null && !parent.IsFinalisingLines)
			{
				PutawayJobHelper.RemoveIsPuttingAwayFromAssociatedPutawayLines(parent, new[] { this });
			}
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return base.CanDelete && !IsFinalised && !IsPutaway && !IsHeldForTransfer && !IsVASOrderTransfer && !IsTransferringForOrder; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason;
				if (IsFinalised)
				{
					reason = ResString.GetMultilingualString("313f823c-b79e-4cdf-8f2b-2e4a32485e86", "You cannot delete finalized Transfer Lines.");
				}
				else if (IsPutaway)
				{
					reason = ResString.GetMultilingualString("06B4B102-C158-49CF-88D6-690F627F44E1", "You cannot delete Transfer Lines that have been partially or fully transferred.");
				}
				else if (IsTransferringForOrder)
				{
					reason = ResString.GetMultilingualString("93d6436d-cb2b-4113-9950-9df4cb18ef21", "You cannot delete Outbound Dock Door Transfer Lines.");
				}
				else if (IsHeldForTransfer)
				{
					reason = ResString.GetMultilingualString("4C464598-D049-4D07-8853-2B70423BFD02", "You cannot delete Transfer Lines that have been partially or fully picked.");
				}
				else if (IsVASOrderTransfer)
				{
					reason = ResString.GetMultilingualString("9ead3f45-5f4c-418d-b1ff-b0e6f5ce0643", "You cannot delete VAS Order Transfer Lines.");
				}
				else
				{
					reason = base.ReasonForNotAbleToDelete;
				}
				return reason;
			}
		}

		public override void Delete()
		{
			if (IsPutawayTransferLine)
			{
				foreach (WhsPickLine pickLine in PickLines)
				{
					var receiveLine = pickLine.InventoryLine;
					receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
				}
			}

			MatchingLines.DeleteAll();
			ChildTransferLine?.Delete();

			if (IsInDatabase)
			{
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
			}
			base.Delete();
		}

		#endregion

		// interfaces

		#region ICustomsDataParent Members

		bool ICustomsDataParent.IsOutwardTypeRequired => false;

		bool ICustomsDataParent.IsCustomsDataReadOnly => IsTransferringForOrder;

		bool ICustomsDataParent.IsMainCustomsDataPropertiesReadOnly => IsHeldForTransfer;

		bool ICustomsDataParent.IsCustomsOutwardDataReadOnly => IsTransferringForOrder;

		#endregion

		#region ILineAssigner Members

		void ILineStaffAssigner.AssignLine(GlbStaff staff) => AssignOrUnassignLine(() =>
		{
			GS_NKPickedBy = staff.GS_Code;
			HasChanges = true;
		});

		void ILineStaffAssigner.UnAssignLine(GlbStaff staff) => AssignOrUnassignLine(() =>
		{
			if (staff == null || GS_NKPickedBy == staff.GS_Code)
			{
				GS_NKPickedBy = ZString.Empty;
				HasChanges = true;
			}
		});

		void AssignOrUnassignLine(Action assignOrUnAssignLineAction)
		{
			switch (Docket.Option)
			{
				case AssignLineOptions.PickOnly:
					assignOrUnAssignLineAction();
					break;
				default:
					throw new NotImplementedException("Invalid AssignLineOption");
			}
		}

		bool ILineStaffAssigner.CanAssignOrUnAssignLine()
		{
			bool result = !IsFinalised && !Docket.IsFinalisedOrCancelled;
			switch (Docket.Option)
			{
				case AssignLineOptions.PickOnly:
					result &= !IsPicked && !IsPickerPicking;
					break;
				default:
					throw new NotImplementedException("Invalid AssignLineOption");
			}
			return result;
		}

		#endregion

		#region ILineWithInventory

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
			get { return IsOutOfServiceAreaTransferForVASOrder && !IsFinalised ? new ZString(InventoryStatus.Codes.Staged) : WE_OriginalInventoryStatus; }
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

		bool ILineWithInventory.CanCreateInventory => IsFinalising || IsCreatingInTransitInventorySemaphore.IsSuspended;

		#endregion

		#region ILineWithCommittedPickLines

		ICommittedInventoryStrategy ILineWithCommittedPickLines.CommittedStrategy
		{
			get { return InventoryCommitmentBuilder; }
		}

		ZGuid ILineWithCommittedPickLines.ParentDocketPK
		{
			get { return WE_WD; }
			set { WE_WD = value; }
		}

		WhsLocation ILineWithCommittedPickLines.LocationToCommit
		{
			get { return TransferFromLocation; }
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
			get { return WE_TransferFromPalletId; }
		}

		string ILineWithCommittedPickLines.Noun
		{
			get { return Res.GetString("63de9164-8441-4169-821d-f75ac9d1e6e9", "transfer"); }
		}

		string ILineWithCommittedPickLines.Verb
		{
			get { return Res.GetString("63de9164-8441-4169-821d-f75ac9d1e6e9", "transfer"); }
		}

		ZDecimal ILineWithCommittedPickLines.PerPackageQty
		{
			set { WE_PerPackageQty = value; }
		}

		ZDecimal ILineWithCommittedPickLines.TransactionQty
		{
			get { return WE_TransactionQuantity; }
			set { WE_TransactionQuantity = value; }
		}

		bool ILineWithCommittedPickLines.IsInTransit => IsInTransit;

		#endregion

		#region ILineWithMatchingLines

		ZDecimal ILineWithMatchingLines<WhsTransferLine>.PerPackageQty
		{
			get { return WE_PerPackageQty; }
		}

		ZGuid ILineWithMatchingLines<WhsTransferLine>.MatchingLinePK
		{
			get { return WE_WE_MatchingLine; }
			set { WE_WE_MatchingLine = value; }
		}

		IEnumerable<WhsTransferLine> ILineWithMatchingLines<WhsTransferLine>.MatchingLines
		{
			get { return MatchingLines.Cast<WhsTransferLine>(); }
		}

		#endregion

		#region ILineWithPickAndPutawayDetails

		ZString ILineWithPickAndPutawayDetails.PickedBy
		{
			get { return GS_NKPickedBy; }
		}

		ZString ILineWithPickAndPutawayDetails.PutawayBy
		{
			get { return WE_GS_NKPutawayBy; }
			set { WE_GS_NKPutawayBy = value; }
		}

		#endregion

		#region ILocationCapacityLine

		protected override ZDecimal GetQuantityCore(WhsLocation location)
		{
			var sourceLocation = TransferFromLocation;
			var destLocation = Location;
			var result = 0m;

			if (sourceLocation != destLocation)
			{
				var takeOut = sourceLocation?.PK == location.PK;
				result = takeOut ? -QtyToMoveIncludingMatchingLines : QtyToMoveIncludingMatchingLines;
			}

			return result;
		}

		#endregion

		partial void OnChildTransferLineCreated_ForTest(WhsTransferLine childLine);
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Business
{
	partial class WhsTransferLine
	{
		partial void OnChildTransferLineCreated_ForTest(WhsTransferLine childLine)
		{
			if (ChildTransferLineChanged_ForTest != null)
			{
				ChildTransferLineChanged_ForTest(childLine, EventArgs.Empty);
			}
		}

		public event EventHandler ChildTransferLineChanged_ForTest;
	}
}
#endif
#endregion
