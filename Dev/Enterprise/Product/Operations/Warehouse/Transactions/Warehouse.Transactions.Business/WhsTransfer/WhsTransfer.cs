using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[GlowDataDefinition("IWhsTransfer")]
	[CodeProperty(WhsDocketSchema.Constants.WD_DocketID), DescriptionProperty(WhsDocketSchema.Constants.WD_ExternalReference)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.WhsTransfer)]
	public sealed class WhsTransfer : WhsDocket,
		IWhsTransfer,
		ICreateDocketLineFromInventory,
		IDocumentSupportable,
		IEDocsProvider,
		IMasterStaffAssigner,
		IProcessHandlingInfoProvider,
		IPickedStockAdjuster,
		INumberFountainConsumer,
		ICriticalChangesVersionID,
		ITaskPlanningJob
	{
		#region Constructors

		public WhsTransfer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_FinalisedDate), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Public Statics

		public static WhsTransfer New(BusinessObjectFactory factory)
		{
			return factory.
				// Split so find replace will ignore
				New<WhsTransfer>();
		}

		#endregion

		#region Customs Stuff

		public override bool IsCustomsTransaction
		{
			get { return IsWarehouseBondEnabled || IsWarehouseExciseEnabled; }
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WD_DocketType = CodeLists.DocketType.Codes.Transfer;
			WD_DocketSubType = CodeLists.TransferType.Codes.Internal;
			WD_ExternalReference = "TRANSFER";
		}

		protected override bool IsUniqueExternalReferenceCreatedOnSaveCore
		{
			get { return true; }
		}

		protected override ZString HumanReadableNameCore => WD_DocketID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WD_DocketID);

		protected override ZString HumanReadableNameWithoutID => Res.GetString("dc6a464d-8bd7-4784-8a66-0702b5ff45f8", "Warehouse Transfer");

		protected override void OnClientChanged()
		{
			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			base.OnClientChanged();
		}

		protected override void OnWarehouseChanged()
		{
			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			if (!IsInDatabase && WD_ArrivalDate.IsEmpty)
			{
				WD_ArrivalDate = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			}
			base.OnWarehouseChanged();
		}

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (NewRelatedJobsAdded)
			{
				ReloadRelatedJobs();
				NewRelatedJobsAdded = false;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (SupportsDocketPlanningStatus)
			{
				if (IsFinalisedOrCancelled)
				{
					WD_TaskPlanningStatus = string.Empty;
				}
				else if ((!IsInDatabase || WD_WW_WhsInfo.HasChanges) && (WD_TaskPlanningStatus.IsEmpty || WD_TaskPlanningStatus == TaskPlanningStatus.Codes.NotReady))
				{
					var warehouse = Warehouse;
					if (warehouse != null && warehouse.WW_GG_ReleaseGroup.IsValid
						&& ((WD_IsPickFaceReplenishment && WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.GetFallBackValueAtAllLevels(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty))
						|| (!WD_IsPickFaceReplenishment && WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.GetFallBackValueAtAllLevels(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty))))
					{
						WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
					}
					else
					{
						WD_TaskPlanningStatus = warehouse?.WW_GG_ReleaseGroup.IsValid ?? false ? TaskPlanningStatus.Codes.NotReady : string.Empty;
					}
				}
			}

			if (IsInDatabase && !IsDeleting)
			{
				CheckDidNotChangeAnyVASOrderTransferFields();
				CheckDidNotChangeAnyOutboundDockDoorTransferFields();
			}
		}

		void CheckDidNotChangeAnyVASOrderTransferFields()
		{
			if (IsVASOrderTransfer)
			{
				var areAnyPropertiesChanged = PropertiesMustNotBeChanged.Any(name => ZPropertyInfoHash[name].HasChanges);
				if (areAnyPropertiesChanged)
				{
					throw new ZCannotSaveException(Res.GetString("WhsTransfer|PropertiesMustNotBeChanged", "Cannot save as fields have been modified on a VAS Order Transfer."),
						Res.GetString("WhsTransfer|TransferMustNotBeModified", "VAS Order Transfer Must Not Be Modified."), ExceptionType.BusinessFailure);
				}
			}
		}

		void CheckDidNotChangeAnyOutboundDockDoorTransferFields()
		{
			if (IsTransferringForOrder)
			{
				var areAnyPropertiesChanged = PropertiesMustNotBeChanged.Any(name => ZPropertyInfoHash[name].HasChanges) || WD_WP_ParentPickForTransferInfo.HasChanges;
				if (areAnyPropertiesChanged)
				{
					throw new ZCannotSaveException(Res.GetString("WhsTransfer|PropertiesMustNotBeChangedDockDoor", "Cannot save as fields have been modified on an Outbound Dock Door Transfer."),
						Res.GetString("WhsTransfer|TransferMustNotBeModifiedDockDoor", "Outbound Dock Door Transfer Must Not Be Modified."), ExceptionType.BusinessFailure);
				}
			}
		}

		#endregion

		protected override void DeleteCore()
		{
			base.DeleteCore();

			var vasOrder = VASOrder;
			if (vasOrder != null)
			{
				if (vasOrder.WVO_WD_TransferIntoServiceArea == PK)
				{
					vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
				}
				else if (vasOrder.WVO_WD_TransferOutOfServiceArea == PK)
				{
					vasOrder.WVO_WD_TransferOutOfServiceArea = ZGuid.Empty;
				}
			}
		}

		IEnumerable<string> PropertiesMustNotBeChanged
		{
			get
			{
				if (propertiesMustNotBeChanged == null)
				{
					propertiesMustNotBeChanged = new[]
					{
						WhsDocketSchema.WD_OH_Client.Name,
						WhsDocketSchema.WD_WW_Whs.Name,
						WhsDocketSchema.WD_DocketSubType.Name
					};
				}
				return propertiesMustNotBeChanged;
			}
		}
		string[] propertiesMustNotBeChanged;

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return base.CanDelete && !HasAnyPickedOrFinalisedLines() && (!ServiceCommenced || Lines.Count == 0); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var reason = base.ReasonForNotAbleToDelete;

				if (HasAnyPickedOrFinalisedLines())
				{
					reason = ResString.GetMultilingualString("789b39c8-fea2-4a0f-8acf-d18cb63206fb", "There is at least one picked, transferred or finalized line on this transfer, preventing this transfer from being deleted.");
				}
				else if (ServiceCommenced && Lines.Count > 0)
				{
					reason = ResString.GetMultilingualString("5a46b2ab-da84-4a95-84c5-db6cad053d80", "This transfer has been commenced and cannot be deleted.");
				}

				return reason;
			}
		}

		bool ServiceCommenced
		{
			get
			{
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ServiceCommencedCode);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.MaximumRows = 1;

				return Logs.Find(query).Length > 0;
			}
		}

		bool HasAnyPickedOrFinalisedLines()
		{
			return Lines.Cast<WhsTransferLine>().Any(l => l.IsFinalised || l.IsHeldForTransfer);
		}

		#endregion

		#region Related Entities

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel => null;

		#endregion

		protected override WhsDocketLineCollection GetNewDocketLineCollection()
		{
			var query = new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, ZBool.True);
			query.AddToFilter(WhsDocketLineSchema.WE_WE_MatchingLine, null);

			return new WhsTransferLineCollection(this, query);
		}

		[ChildEditable(true)]
		public new WhsTransferLineCollection Lines
		{
			get { return (WhsTransferLineCollection)base.Lines; }
		}

		#region LinesToValidateWhenFinalising

		/// <summary>
		/// When only transfer lines are being finalised, the lines being finalised are stored for access during the process. However when
		/// the whole docket is finalising, it sets the lines too late, in which case we will return all the lines on the Transfer instead.
		/// </summary>
		public IEnumerable<WhsTransferLine> LinesToValidateWhenFinalising
		{
			get { return finalisingLines ?? Lines.ToArray<WhsTransferLine>(); }
		}

		#endregion

		#region FinalisingLines

		IEnumerable<WhsTransferLine> FinalisingLines => finalisingLines ?? (Array.Empty<WhsTransferLine>());

		IEnumerable<WhsTransferLine> finalisingLines;

		#endregion

		#region Notes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.O;
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.T;

				return result;
			}
		}

		#endregion

		#region GetRelatedJobsCore

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();
			if (IsInterWarehouseTransfer)
			{
				result.AddRange(GetRelatedTransfers());
			}
			return result;
		}

		IEnumerable<IRelatedJob> GetRelatedTransfers()
		{
			return Factory.Load<WhsTransfer>(GetRelatedTransfersFilter());
		}

		ZQuery GetRelatedTransfersFilter()
		{
			var condition = new ZQuery();
			condition.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
			condition.AddToFilter(JoinCondition.Or, WhsDocketSchema.PK, WD_WD_ParentDocket);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(condition);
			return query;
		}

		#endregion

		#region MasterTransfer

		public WhsTransfer MasterTransfer
		{
			get
			{
				WhsTransfer result = null;
				if (!IsMasterTransfer)
				{
					result = Factory.Load<WhsTransfer>(WD_WD_ParentDocket);
				}
				return result;
			}
		}

		#endregion

		#region ChildTransfers

		public IEnumerable<WhsTransfer> ChildTransfers
		{
			get { return Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, PK)); }
		}

		#endregion

		#region TransportCo

		protected override OrgHeader TransportCoCore => null;

		protected override int TransportCoNameOrPKMaxLength => 0;

		#endregion

		#region PickBeingReplenished

		public WhsPick PickBeingReplenished => Factory.Load<WhsPick>(WD_WP_PickBeingReplenished);

		#endregion

		#endregion

		#region Validation

		protected override WhsDocketValidation GetNewValidation()
		{
			return new WhsTransferValidation(this);
		}

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			CommittingLinesHelper.UncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended(Lines.Cast<WhsTransferLine>());
			base.RunPreSaveValidationCore();
		}

		CommittingLinesTransferHelper CommittingLinesHelper
		{
			get { return committingLinesHelper ?? (committingLinesHelper = new CommittingLinesTransferHelper(this)); }
		}

		CommittingLinesTransferHelper committingLinesHelper;

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableList(new[]
			{
				base.GetValidationDataSuspender(),
				PalletIDLocationValidationCacheManager.UseLocationCache(Factory)
			});
		}

		#endregion

		#endregion

		#region CustomizableNumber

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseDocketID;

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups()
		{
			return new WhsTransferLookups(this);
		}

		#endregion

		#region Properties

		#region HasAnyPickedLine

		public bool HasAnyPickedLine
		{
			get { return Lines.Cast<WhsTransferLine>().Any(l => l.IsPicked); }
		}

		#endregion

		#region CustomFieldsSupported

		protected override bool CustomFieldsSupportedCore => IsNotAutoCreatedTransferOrReplenishment;

		bool IsNotAutoCreatedTransferOrReplenishment => !IsAutoCreatedTransfer && !WD_IsPickFaceReplenishment;

		#endregion

		#region Description

		protected override ZString DescriptionCore
		{
			get { return Res.GetString("5a139608-6b01-4d41-bc1d-e2183a5c8eb5", "Transfer"); }
		}

		#endregion

		#region DocketSubType

		[ReadOnlyMember(nameof(DocketSubTypeReadOnly))]
		[List("Lookups.SubTypesWithPutawayType")]
		public ZString DocketSubType
		{
			get
			{
				var subType = WD_DocketSubType;

				if (WD_IsPutawayTransfer)
				{
					subType = NonPersistentTransferType.Codes.Putaway;
				}
				else if (IsTransferringForOrder)
				{
					subType = NonPersistentTransferType.Codes.OutboundDockDoor;
				}
				else if (WD_IsPickFaceReplenishment)
				{
					subType = NonPersistentTransferType.Codes.AutoCreatedReplenishment;
				}

				return subType;
			}
			set => WD_DocketSubType = value;
		}

		public ZPropertyInfo DocketSubTypeInfo
		{
			get { return DocketSubType != WD_DocketSubType ? GetZPropertyInfo(Schema.DocketSubType) : GetWrappedZPropertyInfo(Schema.DocketSubType, x => WD_DocketSubTypeInfo); }
		}

		#endregion

		#region IsAutoCreatedTransfer

		public bool IsAutoCreatedTransfer => WD_IsPutawayTransfer || IsTransferringForOrder || IsVASOrderTransfer;

		#endregion

		#region IsInterWarehouseTransfer

		public ZBool IsInterWarehouseTransfer
		{
			get
			{
				return WD_DocketSubType == CodeLists.TransferType.Codes.InterWhsSource ||
					WD_DocketSubType == CodeLists.TransferType.Codes.InterWhsDest;
			}
		}

		#endregion

		#region IsInternalTransfer

		bool IsInternalTransfer => WD_DocketSubType == TransferType.Codes.Internal;

		#endregion

		#region IsMasterTransfer

		public ZBool IsMasterTransfer
		{
			get { return WD_WD_ParentDocket.IsEmpty || IsInternalTransfer; }
		}

		#endregion

		#region IsReturnStockTransfer

		public bool IsReturnStockTransfer => !WD_WD_ParentDocket.IsEmpty && IsInternalTransfer;

		#endregion

		#region IsTransferringForOrder

		public bool IsTransferringForOrder => WD_WP_ParentPickForTransfer.IsValid;

		#endregion

		#region IsTransferLinkedToPickForReplenishment

		public bool IsTransferLinkedToPickForReplenishment => WD_WP_PickBeingReplenished.IsValid;

		#endregion

		#region IsFinalisingLines

		public bool IsFinalisingLines
		{
			get { return FinaliseDocketLineSemaphore.IsSuspended; }
		}

		#endregion

		#region IsUserAllowedToFinalise

		public ZBool IsUserAllowedToFinalise
		{
			get { return IsFinalised || !IsMasterTransfer || IsTransferringForOrder; }
		}

		#endregion

		#region IsVASOrderTransfer

		WhsVASOrder VASOrder
		{
			get
			{
				var query = new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, PK);
				query.AddToFilter(JoinCondition.Or, WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, PK);
				return Factory.Load<WhsVASOrder>(query).SingleOrDefault(); // Can't use LoadTop1 or DbHits will increase
			}
		}

		public bool IsVASOrderTransfer
		{
			get { return VASOrder != null; }
		}

		#endregion

		#region HasAtLeastOneUnFinalisedTransferLine

		public bool HasAtLeastOneUnFinalisedTransferLine(IEnumerable<WhsTransferLine> transferLines)
		{
			return transferLines.Any(o => !o.IsFinalised);
		}

		#endregion

		#region PropagatesFinalisedDateAndStatusToLines

		protected override bool PropagatesFinalisedDateAndStatusToLinesCore
		{
			get { return false; }
		}

		#endregion

		#region ReactivedDocketLineDefaultStatus

		protected override string ReactivedDocketLineDefaultStatus(WhsDocketLine line) => WhsTransferLine.TransferLineDefaultStatus;

		#endregion

		#region ShouldUpdateWeightAndVolumeOnTheFly

		protected override bool ShouldUpdateWeightAndVolumeOnTheFlyCore
		{
			get { return false; }
		}

		#endregion

		#region NewRelatedJobsAdded

		public bool NewRelatedJobsAdded
		{
			get;
			set;
		}

		#endregion

		#region OnDocketSubTypeChanged

		protected override void OnDocketSubTypeChangedCore(ZString previousSubType)
		{
			base.OnDocketSubTypeChangedCore(previousSubType);

			if ((WD_DocketSubType == TransferType.Codes.Internal && previousSubType == TransferType.Codes.InterWhsSource) ||
				(WD_DocketSubType == TransferType.Codes.InterWhsSource && previousSubType == TransferType.Codes.Internal))
			{
				SetLocationsAndWarehousesForAllLines(WD_WW_Whs, ZGuid.Empty);
			}
			else if ((WD_DocketSubType == TransferType.Codes.Internal && previousSubType == TransferType.Codes.InterWhsDest) ||
				(WD_DocketSubType == TransferType.Codes.InterWhsDest && previousSubType == TransferType.Codes.Internal))
			{
				SetLocationsAndWarehousesForAllLines(ZGuid.Empty, WD_WW_Whs);
			}
			else if ((WD_DocketSubType == TransferType.Codes.InterWhsSource && previousSubType == TransferType.Codes.InterWhsDest) ||
				(WD_DocketSubType == TransferType.Codes.InterWhsDest && previousSubType == TransferType.Codes.InterWhsSource))
			{
				SetLocationsAndWarehousesForAllLines(ZGuid.Empty, ZGuid.Empty);
			}
		}

		void SetLocationsAndWarehousesForAllLines(ZGuid transferFromWhsPK, ZGuid whsPK)
		{
			foreach (WhsTransferLine transferLine in Lines)
			{
				if (transferFromWhsPK.IsEmpty)
				{
					transferLine.TransferFromWarehousePK = transferFromWhsPK;
					transferLine.TransferFromLocationString = "";
				}
				if (whsPK.IsEmpty)
				{
					transferLine.DestinationWarehousePK = whsPK;
					transferLine.LocationString = "";
				}
			}
		}

		#endregion

		#region CanCreateInventoryCore

		protected override bool CanCreateInventoryCore
		{
			get { return true; }
		}

		#endregion

		#region SubTypeDesc

		protected override ZString SubTypeDescCore()
		{
			var subTypeDesc = base.SubTypeDescCore();

			if (WD_IsPutawayTransfer)
			{
				subTypeDesc = NonPersistentTransferType.Descriptions.Putaway;
			}
			else if (IsTransferringForOrder)
			{
				subTypeDesc = NonPersistentTransferType.Descriptions.OutboundDockDoor;
			}
			else if (WD_IsPickFaceReplenishment)
			{
				subTypeDesc = NonPersistentTransferType.Descriptions.AutoCreatedReplenishment;
			}

			return subTypeDesc;
		}

		#endregion

		#region WD_WW_Whs

		public override ZGuid WD_WW_Whs
		{
			get { return base.WD_WW_Whs; }
			set
			{
				if (WD_WW_Whs != value)
				{
					base.WD_WW_Whs = value;
					WD_WP_PickBeingReplenished = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region WD_WP_PickBeingReplenished

		[ReadOnlyMember(nameof(PickBeingReplenishedReadOnly))]
		[List("Lookups.PicksForReplenishment")]
		public override ZGuid WD_WP_PickBeingReplenished
		{
			get { return base.WD_WP_PickBeingReplenished; }
			set { base.WD_WP_PickBeingReplenished = value; }
		}

		#endregion

		#region WD_FinalisedDate

		public override ZDateTimeOffset WD_FinalisedDate
		{
			get => base.WD_FinalisedDate;
			set
			{
				base.WD_FinalisedDate = value;
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_CriticalChangesVersionID), WD_FinalisedDate.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
			}
		}

		#endregion

		#region SupportsDocketPlanningStatus

		protected override bool SupportsDocketPlanningStatus => !WD_IsPutawayTransfer && !IsTransferringForOrder;

		#endregion

		#endregion

		#region Allocate Same Destination Locations

		public void AllocateSameDestinationLocations(BusinessObject[] lines)
		{
			Argument.NotNull(lines, nameof(lines));

			if (CanPerformActionOnTransfer(this, NotificationSubscriber))
			{
				var location = GetFirstDestinationLocation(lines);
				if (!location.IsEmpty)
				{
					var message = Res.GetString("db5e2f49-b45a-4eda-af1a-cd6699e9a537", "The system will use the first Destination Location you have selected: {0}. Do you want to proceed?", location);
					var e = new QueryUserYesNoEventArgs(Res.GetString("ada3c5fd-8224-4746-87c3-cf2694aadcc5", "Warning"), message, true);
					NotificationSubscriber.QueryUser(e);

					if (e.Response)
					{
						AllocateSameDestinationLocationsCore(lines, location);
					}
				}
				else
				{
					NotificationSubscriber.Notify(new ErrorNotification(TransferErrorTypes.NoDestinationLocationSelected));
				}
			}
		}

		static bool CanPerformActionOnTransfer(WhsTransfer transfer, INotifications notifications)
		{
			bool result = false;

			if (transfer.IsFinalisedOrCancelled)
			{
				notifications.Notify(new ErrorNotification(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
			else if (transfer.IsTransferringForOrder)
			{
				notifications.Notify(new ErrorNotification(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
			}
			else
			{
				result = true;
			}

			return result;
		}

		void AllocateSameDestinationLocationsCore(BusinessObject[] lines, ZString location)
		{
			foreach (WhsTransferLine transferLine in lines)
			{
				if (transferLine != null && transferLine.LocationString.IsEmpty)
				{
					transferLine.LocationString = location;
				}
			}
		}

		ZString GetFirstDestinationLocation(BusinessObject[] lines)
		{
			ZString result = "";

			foreach (WhsTransferLine transferLine in lines)
			{
				if (transferLine != null && !transferLine.LocationString.IsEmpty)
				{
					result = transferLine.LocationString;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Clear Destination Locations

		public void ClearDestinationLocations(BusinessObject[] lines)
		{
			Argument.NotNull(lines, nameof(lines));

			if (CanPerformActionOnTransfer(this, NotificationSubscriber))
			{
				ClearDestinationLocationsCore(lines);
			}
		}

		void ClearDestinationLocationsCore(BusinessObject[] lines)
		{
			foreach (WhsTransferLine transferLine in lines)
			{
				if (transferLine != null && !transferLine.LocationStringInfo.ReadOnly && !transferLine.LocationString.IsEmpty)
				{
					transferLine.LocationString = "";
				}
			}
		}

		#endregion

		#region Generate Destination PalletIDs

		public void GenerateDestinationPalletIDs(BusinessObject[] lines)
		{
			Argument.NotNull(lines, nameof(lines));

			if (CanPerformActionOnTransfer(this, NotificationSubscriber))
			{
				if (string.IsNullOrEmpty(WD_DocketID))
				{
					NotificationSubscriber.Notify(new ErrorNotification(TransferErrorTypes.NoDocketIDSpecified));
				}
				else if (IsInternalTransfer && IsNotAutoCreatedTransferOrReplenishment)
				{
					GeneratePalletIDsHelper.GenerateIDs(PalletIDGenerator, this, lines.Cast<WhsTransferLine>(),
						(WhsDocketLine line) => line != null && !line.WE_PalletIDInfo.ReadOnly && !line.IsFinalised, ErrorMessage_InvalidLine);
				}
				else
				{
					NotificationSubscriber.Notify(new ErrorNotification(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));
				}
			}
		}

		ZString ErrorMessage_InvalidLine
		{
			get => Res.GetString(
				"c01ffcec-4b6c-4f03-93e0-2de700e136ba",
				"The system has not generated a Pallet ID for this line because it is finalized or its Pallet ID is read only");
		}

		IPalletIDGenerator PalletIDGenerator
		{
			get => palletIDGenerator ??= ObjectFactory.Get<IPalletIDGenerator>();
		}

		IPalletIDGenerator palletIDGenerator;

		#endregion

		#region Update Destination PalletIDs

		public void UpdateDestinationPalletIDs(BusinessObject[] lines)
		{
			Argument.NotNull(lines, nameof(lines));

			if (CanPerformActionOnTransfer(this, NotificationSubscriber))
			{
				UpdateDestinationPalletIDsCore(lines);
			}
		}

		void UpdateDestinationPalletIDsCore(BusinessObject[] lines)
		{
			foreach (WhsTransferLine transferLine in lines)
			{
				if (transferLine != null && !transferLine.WE_PalletIDInfo.ReadOnly && !transferLine.WE_TransferFromPalletId.IsEmpty && transferLine.WE_PalletID.IsEmpty)
				{
					transferLine.WE_PalletID = transferLine.WE_TransferFromPalletId;
				}
			}
		}

		#endregion

		#region Clear Destination PalletIDs

		public void ClearDestinationPalletIDs(BusinessObject[] lines)
		{
			Argument.NotNull(lines, nameof(lines));

			if (CanPerformActionOnTransfer(this, NotificationSubscriber))
			{
				ClearDestinationPalletIDsCore(lines);
			}
		}

		void ClearDestinationPalletIDsCore(BusinessObject[] lines)
		{
			foreach (var line in lines)
			{
				var transferLine = line as WhsTransferLine;
				if (transferLine != null && !transferLine.WE_PalletIDInfo.ReadOnly && !transferLine.WE_PalletID.IsEmpty)
				{
					transferLine.WE_PalletID = "";
				}
			}
		}

		#endregion

		#region TransferLineFromInventory

		public TransferLineFromInventoryHelper TransferLineFromInventoryHelper
		{
			get { return transferLineFromInventoryHelper ?? (transferLineFromInventoryHelper = new TransferLineFromInventoryHelper(NotificationSubscriber, this)); }
		}
		TransferLineFromInventoryHelper transferLineFromInventoryHelper;

		public WhsDocketLine CreateDocketLineFromInventory(WhsInventoryView inventory)
		{
			return TransferLineFromInventoryHelper.CreateDocketLineFromInventory(this.Lines, inventory);
		}

		public void AcceptInventoryLinesFromSearchGrid(BusinessObject[] inventory)
		{
			TransferLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(this.Lines, inventory);
		}

		#endregion

		#region Finalisation

		#region FinaliseDocket

		protected override void AddFetchHintsForPreFinaliseValidation_Core()
		{
			base.AddFetchHintsForPreFinaliseValidation_Core();

			foreach (var pk in Lines.Select(l => l.PK))
			{
				Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_MatchingLine, pk);

				var query = new ZQuery();
				query.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
				query.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

				var pickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, pk);
				pickLineQuery.AddToFilter(query);

				Factory.AddFetchHint(WhsPickLineSchema.Instance, pickLineQuery);
			}
		}

		protected override bool FinaliseDocketCore()
		{
			var successfulFinalise = true;
			var transferLines = Lines.Cast<WhsTransferLine>().ToArray();
			foreach (var line in transferLines)
			{
				Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, line.PK); // Tested in TransferEntryForm
			}

			if (HasAtLeastOneUnFinalisedTransferLine(transferLines))
			{
				FinaliseDocketLines(transferLines, false);

				var unfinalisedLines = transferLines.Where(o => !o.IsFinalised).ToArray();
				if (unfinalisedLines.Length > 0)
				{
					successfulFinalise = false;
					var unfinalisedLineNumbers = string.Join(", ", unfinalisedLines.Select(o => o.WE_LineNo));
					AddRowError(Res.GetString("3bae8623-43fe-47a8-8437-ee8452115697", "Line(s) {0} could not be finalized.", unfinalisedLineNumbers));
				}
			}
			return successfulFinalise;
		}

		protected override ZGuid FinaliseConfirmationDialogIdentifier
		{
			get { return new ZGuid("7b7c7440-544c-422b-87cd-1bfb1af92ceb"); }
		}

		protected override string FinaliseConfirmationMessage
		{
			get { return Res.GetString("a3cac078-b208-4e91-9867-c8194ba94fab", "Finalizing this Transfer will update the inventory.\r\nOn finalization, this Transfer will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\nDo you wish to Finalize this Transfer?"); }
		}

		protected override void OnFinaliseSucceeded()
		{
			base.OnFinaliseSucceeded();

			WD_TaskPlanningStatus = string.Empty;
			FinaliseChildTransfer();
			FinaliseVASOrderIfOutOfServiceAreaTransfer();
		}

		void FinaliseChildTransfer()
		{
			foreach (var childTransfer in ChildTransfers)
			{
				childTransfer.FinaliseDocketWithoutUserConfirmation();
			}
		}

		void FinaliseVASOrderIfOutOfServiceAreaTransfer()
		{
			var query = new ZQuery(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, PK);
			var vasOrder = Factory.LoadTop1<WhsVASOrder>(query);
			if (vasOrder != null)
			{
				vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			}
		}

		protected override ZDateTimeOffset GetFinalisedDate() => Provider?.GetFinalisationTimeOffset() ?? base.GetFinalisedDate();

		internal IFinalisedDateProvider GetFinalisedDateProvider()
		{
			if (Provider != null && !IsFinalising && !IsFinalisingLines)
			{
				throw new InvalidOperationException("Attempt to call GetFinalisedDateProvider() without finalising the Transfer.");
			}

			return Provider;
		}

		internal IDisposable SetFinalisedDateProvider(IFinalisedDateProvider provider)
		{
			Argument.NotNull(provider, nameof(provider));

			if (!IsTransferringForOrder)
			{
				throw new InvalidOperationException("Can only use a IFinalisedDateProvider for Outbound Dock Door Transfers.");
			}

			return new DisposableAction(
				() => Provider = provider,
				() => Provider = null);
		}

		IFinalisedDateProvider Provider;

		#endregion

		#region ValidateAndFinaliseDocketLines

		public void ValidateAndFinaliseDocketLines(IEnumerable<WhsTransferLine> transferLines, bool confirmFinalise)
		{
			if (transferLines.Any(l => l.WE_WD != PK))
			{
				throw new ArgumentException("Transfer can only finalise its own lines.");
			}

			if (HasAtLeastOneUnFinalisedTransferLine(transferLines))
			{
				var unfinalisedLines = transferLines.Where(o => !o.IsFinalised).ToArray();
				var validationMessage = WhsTransferValidationHelper.FindUNDGsOverLimit(this, unfinalisedLines);
				if (validationMessage.IsEmpty)
				{
					FinaliseDocketLines(unfinalisedLines, confirmFinalise);
				}
				else
				{
					NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, validationMessage));
				}
			}
			else
			{
				NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.CannotPerformThisOperationBecauseAllSelectedLinesAreFinalised));
			}
		}

		void FinaliseDocketLines(IEnumerable<WhsTransferLine> transferLines, bool confirmFinalise)
		{
			if (!confirmFinalise || !IsMasterTransfer || FinaliseConfirmationNotification(FinaliseLinesConfirmationMessage))
			{
				using (Provider == null ? CreateFinalisedTimeProvider() : null)
				using (new SemaphoreManager(FinaliseDocketLineSemaphore))
				{
					FinaliseDocketLinesCore(transferLines);
				}
			}

			IDisposable CreateFinalisedTimeProvider()
			{
				var finaliseTime = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				var finalisedTimeProvider = new FinalisedDateFromFinaliseEvent(finaliseTime);
				return new DisposableAction(() => Provider = finalisedTimeProvider, () => Provider = null);
			}
		}

		#region FinaliseLinesConfirmationMessage

#if DEBUG
		public
#endif
		string FinaliseLinesConfirmationMessage
		{
			get { return Res.GetString("3cbed4fb-fcbe-4cf0-8398-53109bb2af44", "Finalizing this Transfer Line(s) will update the inventory.\r\nOn finalization, this Transfer Line(s) will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\nDo you wish to Finalize this Transfer Line(s)?"); }
		}

		#endregion

		#region FinaliseDocketLineSemaphore

#if DEBUG
		public
#endif
		Semaphore FinaliseDocketLineSemaphore
		{
			get { return finaliseDocketLineSemaphore ?? (finaliseDocketLineSemaphore = new Semaphore()); }
		}

		Semaphore finaliseDocketLineSemaphore;

		#endregion

		#region FinaliseDocketLinesCore

		void FinaliseDocketLinesCore(IEnumerable<WhsTransferLine> transferLines)
		{
			try
			{
				finalisingLines = transferLines.Where(l => !l.IsFinalised).ToArray();

				// if the transfer is finalising pre-validation uncommit and commit has already occured
				if (!IsFinalising)
				{
					// tested in WhsTransferLineValidationUS
					CommittingLinesHelper.UncommitExcessInventoryAndCommitRequiredInventory(FinalisingLines);
				}

				foreach (var transferLine in FinalisingLines)
				{
					transferLine.FinaliseDocketLine();
				}

				PutawayJobHelper.RemoveIsPuttingAwayFromAssociatedPutawayLines(this, FinalisingLines);
				NotifyUserOfNewAndModifiedChildTransfers(FinalisingLines);
				RefreshBinding(); // properties readonly status is not updated fast enough, which allows user to update properties that should be readonly.
#if DEBUG
				LastFinalisingLines_ForTest = finalisingLines;
#endif
			}
			finally
			{
				finalisingLines = null;
			}
		}

#if DEBUG
		public IEnumerable<WhsTransferLine> LastFinalisingLines_ForTest;
#endif

		void NotifyUserOfNewAndModifiedChildTransfers(IEnumerable<WhsTransferLine> newFinalisedLines)
		{
			if (!HasErrors && IsMasterTransfer && IsInterWarehouseTransfer)
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.Append(Res.GetString("334c5953-2d9f-48ae-af05-7497cb219734", "The following Transfer(s) has been created or updated to reflect the stock movements to the destination warehouse(s):"));
				var dictionary = new Dictionary<ZGuid, WhsTransfer>();
				foreach (var transferLine in newFinalisedLines)
				{
					if (transferLine.IsFinalised)
					{
						if (!dictionary.TryGetValue(transferLine.ChildTransferLine.WE_WD, out _))
						{
							var transfer = transferLine.ChildTransferLine.Docket;
							dictionary.Add(transfer.PK, transfer);
							stringBuilder.Append(Res.GetString("a382df42-e51e-4542-9e6b-17ddba53b283", "Reference: {0}-{1}", transfer.WD_ExternalReference, transfer.WD_ExternalReferenceSplit));
						}
					}
				}
				NotificationSubscriber.Notify(new InfoNotification(stringBuilder.ToStringWithNewLineBetweenAppends()));
			}
		}

		#endregion

		#endregion

		protected override bool AddFetchHintsForInventory
		{
			get { return true; }
		}

		#endregion

		#region Readonly

		#region ReadOnly

		public override bool ReadOnly => base.ReadOnly || !IsMasterTransfer || IsReadyForPlanningOrPlanned;

		#endregion

		#region DocketSubTypeReadOnly

		protected override bool DocketSubTypeReadOnly
		{
			get { return IsAutoCreatedTransfer || base.DocketSubTypeReadOnly; }
		}

		#endregion

		#region StandardReadOnly

		protected override bool StandardReadOnly
		{
			get
			{
				return IsAutoCreatedTransfer || base.StandardReadOnly || !IsMasterTransfer || Lines.Cast<WhsTransferLine>().Any(l => l.IsFinalised) || HasAnyPickedLine;
			}
		}

		#endregion

		#region PickBeingReplenishedReadOnly

		bool PickBeingReplenishedReadOnly => !WD_WW_Whs.IsValid || StandardReadOnly;

		#endregion

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool RequiresCreditCheck
		{
			get { return false; }
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, "WTD");
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsTransferDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IWorkflowProvider Members

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;
		}

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection()
		{
			return new WhsTransferProcessTasksCollection(this);
		}

		#endregion

		#region IPickedStockAdjuster members

		void IPickedStockAdjuster.AdjustOutInventory(WhsInventoryView inventory, ZGuid originalInventoryPK, ZDecimal quantity)
		{
			Argument.NotNull(inventory, nameof(inventory));

			if (quantity <= 0)
			{
				throw new ArgumentException("Quantity must be positive.");
			}

			if (quantity > inventory.WI_TotalUnits)
			{
				throw new ArgumentException("Cannot return stock more than inventory TotalUnits.");
			}

			if (inventory.WI_OH_Client != WD_OH_Client)
			{
				throw new ArgumentException("Client for inventory is different.");
			}

			if (inventory.WI_WW_Whs != WD_WW_Whs)
			{
				throw new ArgumentException("Warehouse for inventory is different.");
			}

			if (!originalInventoryPK.IsValid)
			{
				throw new ArgumentException("Original Inventory must be a valid Guid.");
			}

			var originalInventory = Factory.Load<WhsDocketLine>(originalInventoryPK);
			var line = (WhsTransferLine)CreateDocketLineFromInventory(inventory);
			line.WE_OriginalInventoryStatus = inventory.WI_InventoryStatus;
			line.WE_TransactionQuantity = quantity;
			line.WE_WL_TransferFrom = inventory.WI_WL;
			line.WE_WL = originalInventory.WE_WL;
			line.WE_TransferFromPalletId = inventory.WI_PalletID;
			line.WE_PalletID = ZString.Empty;

			var pickLine = line.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_Units = quantity;
			if (inventory.PK != originalInventoryPK)
			{
				pickLine.WZ_WE_OriginalPickedInventoryLine = originalInventory.PK;
			}
			line.PickedTime = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
		}

		void IPickedStockAdjuster.LinkToDocket(WhsPickableDocket docket)
		{
			Argument.NotNull(docket, nameof(docket));
			WD_WD_ParentDocket = docket.PK;
		}

		ActionResult IPickedStockAdjuster.PrepareForSaving()
		{
			RunPreSaveValidation();

			return HasErrors ? ActionResult.Failure(this.GetErrors().ToMessageListString()) : ActionResult.Success();
		}

		#endregion

		#region IRelatedJobs Members

		protected override ControllerID JobControllerId
		{
			get { return ControllerIDs.WhsTransfer; }
		}

		#endregion

		#region ITaskPlanningJob

		protected override string SpecialCannotUpdateTaskPlanningStatusReasonCore() => SupportsDocketPlanningStatus ? string.Empty : base.SpecialCannotUpdateTaskPlanningStatusReasonCore();

		protected override void ClearFKForProcessTasksCore(ISet<ZGuid> processTaskPKs)
		{
			Lines.Where(line => processTaskPKs.Contains(line.WE_P9_Task)).ForEach(line => line.WE_P9_Task = ZGuid.Empty);
		}

		#endregion

		#region IMasterAssigner

		IEnumerable<ILineStaffAssigner> IMasterStaffAssigner.Lines
		{
			get { return Lines.ToArray<WhsTransferLine>(); }
		}

		bool IMasterStaffAssigner.IsJobAssignable => !IsFinalisedOrCancelled;

		bool IMasterStaffAssigner.CanAssignOrUnAssignAnyLines => Lines.ToArray<WhsTransferLine>().Any<ILineStaffAssigner>(l => l.CanAssignOrUnAssignLine());

		public AssignLineOptions Option { get; set; }

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new WhsTransferProcessHandlingInfo(this); }
		}

		#endregion

		#region IWhsLogEventParent Members

		protected override string GetDocketEventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Transfer; }
		}

		#endregion

		#region ICriticalChangesVersionID members

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WD_CriticalChangesVersionID = value;
			get => WD_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => IsFinalised;

		#endregion

		#region ITaskPlanningJob

		ZGuid ITaskPlanningJob.WarehousePK => WD_WW_Whs;

		ZString ITaskPlanningJob.TaskPlanningStatus
		{
			get => WD_TaskPlanningStatus;
			set => WD_TaskPlanningStatus = value;
		}

		string ITaskPlanningJob.JobID => WD_DocketID;

		ZString ITaskPlanningJob.HumanReadableNameWithoutID => HumanReadableNameWithoutID;

		#endregion
	}

	#region AssignLineOptions

	public enum AssignLineOptions
	{
		PickOnly
	}

	#endregion

	#region DocumentSupporter

	public class WhsTransferDocumentSupporter : DocumentSupporter
	{
		public WhsTransferDocumentSupporter(WhsTransfer transfer)
			: base(transfer)
		{
		}

		protected WhsTransfer Transfer
		{
			get { return (WhsTransfer)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsTransfer; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsTransferCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Core.Constants.DataContext.WhsTransfer, Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Transfer);
			}
			else
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsTransfer, Transfer) };
			}
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact result = new OrgHeaderContact(Transfer.Client, null);
			return result;
		}
	}

	#endregion
}
