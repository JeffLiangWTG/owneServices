using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using ICartageHelper = Enterprise.Freight.LocalCartage.Integration.ICartageHelper;
using NotificationType = CargoWise.ComponentModel.NotificationType;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DebuggerDisplay("DocketID={WD_DocketID}")]
	[GlowDataDefinition("IWhsDocket")]
	[UserDefinedValues]
	[UniversalCopyAssociateElement("WhsDocketLines", "Lines")]
	[UniversalCopyIgnoreElement("ParentDocket", "Split", "WhsInventoryView", "Whs")]
	[CodeProperty(WhsDocketSchema.Constants.WD_DocketID), DescriptionProperty(WhsDocketSchema.Constants.WD_ExternalReference)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.WhsDocket)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventMismatchOnDocketStatusAndDateWithLines, WhsValidationHelper.WhsCheckDocketStatusAndDateWithLines, WhsDocketSchema.Constants.PK, typeof(IWhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy))]
	public abstract class WhsDocket :
		AutoWhsDocket,
		ICancellable,
		ICanDelete,
		ICreditControlledDocumentDelivery,
		ICustomFieldProvider,
		ICustomLabelsConfigOrgProvider,
		IDocAddresses,
		IDocketInternals,
		IDocManagerSupport,
		IHaveServices,
		ISendEmailSource,
		IJobInvoicingAdditionalData,
		IJobInvoicingPlugIn,
		IRelatedJob,
		ISupportDataImporting,
		IWhsDocket,
		IWorkflowProviderTemplateCriteria,
		IWorkflowTriggerFieldChangeSource,
		IUniversalXMLNoteParent,
		IWhsLogEventParent,
		ISupportRelatedJobs,
		ILocationCapacityMaster<WhsDocketLine>,
		INumberFountainEntityWithID,
		IPropertyChecker,
		ITaskPlanningJob
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventChangeOfCriticalFieldsWhenDocketHasLines = "Attempt to change docket subtype or warehouse for a job with captured lines.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventChangeOfClientWhenDocketHasLines = "Attempt to change client for a job with captured lines.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventDetachedOrderPickLines = "Attempt to detach order/work order docket with unreserved pick lines.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventFinalisedDocketWithUnFinalisedLines = "Attempt to save Finalised Docket with Un-finalised Docket Lines or Finalised Dates of Non-Transfer Docket and Docket Lines do not match.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventSaveOnMismatchCanacelledStatusOnDocketWithLines = "Attempt to save mismatch cancel or departed status between docket and lines.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse = "Order / Work Order warehouse does not match Pick warehouse.";

		#region Constructors

		protected WhsDocket(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConfirmTypeIsCorrect(factory, row);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_WP), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_WP_ParentPickForTransfer), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_CriticalChangesVersionID), ConcurrencyPolicy.Ignore);
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoWhsDocket.Schema
		{
			public const string WD_BOLNo = "WD_BOLNo";
			public const string WD_DocketStatusDescription = "WD_DocketStatusDescription";
			public const string IsFinaliseAllowed = "IsFinaliseAllowed";
			public const string AwaitingCustomsResponseStatus = "AwaitingCustomsResponseStatus";
			public const string WD_NotInvoiced = "WD_NotInvoiced";
			public const string SplitRelationshipTypeDescription = "SplitRelationshipTypeDescription";
			public const string WD_TotalUnitsFromLines = "WD_TotalUnitsFromLines";
			public const string DocketSubType = "DocketSubType";
		}

		#endregion

		#region TypeDecider

		public static readonly WhsDocketTypeDecider TypeDecider = new WhsDocketTypeDecider();

		#endregion

		#region ConfirmDocketType

		void ConfirmTypeIsCorrect(BusinessObjectFactory factory, DataRow row)
		{
			Type docketType = TypeDecider.GetTypeForLoad(row, factory);
			if (docketType != null && !docketType.IsAssignableFrom(this.GetType())
				&& !docketType.IsSubclassOf(this.GetType())) // For Web tracker tests. Fixed in WI00071485
			{
				throw new NotSupportedException(string.Format(Culture.Invariant, "docketType did not match the type of the class. The class type was {0}, but the docketType was {1}", GetType().ToString(), docketType.ToString()));
			}
		}

		#endregion

		#region Events

		public event EventHandler ClientChanged;
		public event EventHandler WarehouseChanged;
		public event EventHandler DocketStatusChanged;
		public event EventHandler DocketSubTypeChanged;
		public event EventHandler DocketFinalisedDateChanged;

		protected virtual void OnClientChanged()
		{
			ClientChanged?.Invoke(this, EventArgs.Empty);

			// change this to Lines.RefreshBinding(); once Mykola's shelf is checked in (WI00019214)
			((IBusinessObjectState)Lines).RefreshBindingIncludingChildren();
		}

		protected virtual void OnWarehouseChanged()
		{
			WarehouseChanged?.Invoke(this, EventArgs.Empty);

			// change this to Lines.RefreshBinding(); once Mykola's shelf is checked in (WI00019214)
			((IBusinessObjectState)Lines).RefreshBindingIncludingChildren();
		}

		void OnDocketStatusChanged(string oldStatus)
		{
			if (WD_DocketStatus == DocketStatus.Codes.Cancelled)
			{
				CancelSucceeded();
			}
			else if (oldStatus == DocketStatus.Codes.Cancelled)
			{
				ReactivateSucceeded();
			}

			SetCustomsDataReadOnly();
			PropagateStatusChangeToLines(oldStatus);

			DocketStatusChanged?.Invoke(this, EventArgs.Empty);
			OnDocketStatusChangedCore();
		}

		protected virtual void OnDocketStatusChangedCore()
		{
		}

		void OnDocketSubTypeChanged(ZString previousSubType)
		{
			DocketSubTypeChanged?.Invoke(this, EventArgs.Empty);
			OnDocketSubTypeChangedCore(previousSubType);
		}

		protected virtual void OnDocketSubTypeChangedCore(ZString previousSubType)
		{
		}

		#endregion

		#region Customs Stuff

		public abstract bool IsCustomsTransaction { get; }

		public bool IsCustomsDataVisible => IsCustomsDataVisibleCore;
		protected virtual bool IsCustomsDataVisibleCore => IsCustomsTransaction;

		public bool IsBondedEntryKeyVisibleForCustomsTransactions => IsBondedEntryKeyVisibleForCustomsTransactionsCore;
		protected virtual bool IsBondedEntryKeyVisibleForCustomsTransactionsCore => false;

		public bool IsWarehouseFreeStoreEnabled => !IsDeleted && (Warehouse?.IsWarehouseFreeStoreEnabled ?? false);

		public bool IsWarehouseBondEnabled => !IsDeleted && (Warehouse?.IsWarehouseBondEnabled ?? false);

		public bool IsWarehouseExciseEnabled => !IsDeleted && (Warehouse?.IsWarehouseExciseEnabled ?? false);

		public ZString DefaultDocketSubTypeForCustomsTransaction => DefaultDocketSubTypeForCustomsTransactionCore;

		protected virtual ZString DefaultDocketSubTypeForCustomsTransactionCore => "";

		protected void SetWarehouseFKAndDefaultsForCustomsTransaction(ZGuid value)
		{
			bool wasCustomsTransaction = IsCustomsTransaction;

			using (GetValidationSuspender())
			{
				base.WD_WW_Whs = value;

				if (value.IsValid && IsWarehouseBondEnabled && !IsWarehouseFreeStoreEnabled && !wasCustomsTransaction && !DefaultDocketSubTypeForCustomsTransaction.IsEmpty)
				{
					WD_DocketSubType = DefaultDocketSubTypeForCustomsTransaction;
				}
			}

			Validation.ValidateWD_WW_Whs();
		}

		protected void SetCustomsDataReadOnly()
		{
			if (IsCustomsTransaction && IsFinalisedOrCancelled)
			{
				foreach (var docketLine in Lines)
				{
					docketLine.CustomsData.SetReadOnlyIncludingChildren(IsFinalisedOrCancelled);
				}
			}
		}

		public void SetExternalReferenceFromEntryKey(string entryKey)
		{
			string reference;

			if (WD_DocketType == DocketType.Codes.Receive)
			{
				reference = entryKey + "-INW-"; // May be identifier
			}
			else
			{
				reference = entryKey + "-AMD-"; // May be identifier
			}

			var filter = new ZQuery(WhsDocketSchema.WD_OH_Client, WD_OH_Client);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, WD_DocketType);
			filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, reference);
			var docketList = Factory.Load<WhsDocket>(filter);

			var suffixNumber = docketList.Length + 1;
			WD_ExternalReference = reference + suffixNumber.ToString("0#", Culture.Invariant);
		}

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WD_DocketID;
			set => WD_DocketID = value;
		}

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WD_DocketStatus = DocketStatus.Codes.New;
			using (new SemaphoreManager(UpdatingWeightAndVolumeSemaphore))
			{
				WD_TotalWeightUnit = Env.Registry.PackageWeightUnit;
				WD_TotalCubicUnit = Env.Registry.PackageVolumeUnit;
			}
		}

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			// This is only true if an exception was thrown halfway through Finalisation
			if (isInProgressOfFinalising)
			{
				throw new ZCannotSaveException(
					ResString.GetMultilingualString("f1e0c1f7-d9d4-4c81-aef2-cd9435bf3fb9", "Cannot save as an error occurred during Finalization. Please reload the {0}.", Description),
					UnableToSaveCaption, ExceptionType.BusinessFailure);
			}

			if (PreventSaveWithoutReload)
			{
				throw new ZCannotSaveException(
					ResString.GetMultilingualString("97e1b9bc-2bbf-4014-8a8e-832552867946", "The {0} has been updated by another job. Please reload the {0}.", Description),
					UnableToSaveCaption, ExceptionType.BusinessFailure);
			}

			DeactivateJobHeaderWhenIsCancelled();
			base.OnFactorySaving();

			CreateOperationalStatusChangeEvents();
		}

		string UnableToSaveCaption => ResString.GetMultilingualString("8808ab17-f4aa-471b-9b1c-b4e44221d3e7", "Unable to Save");

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && WD_DocketStatusInfo.HasChanges)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		#region CreateOperationalStatusChangeEvents

		void CreateOperationalStatusChangeEvents()
		{
			if (!IsInDatabase || HasChanges)
			{
				CreateOperationalStatusChangeEventsCore();
			}
		}

		protected virtual bool DoesNotHaveJobEnteredEvent
		{
			get { return !IsInDatabase; }
		}

		#endregion

		#region CreateUniqueReferenceIfRequired

		void CreateUniqueReferenceIfRequired()
		{
			if (!WD_DocketID.IsEmpty)
			{
				if (IsUniqueExternalReferenceCreatedOnSave && !IsInDatabase)
				{
					CreateUniqueReferenceIfRequiredCore();
				}

				if (WD_ExternalReference.IsEmpty)
				{
					WD_ExternalReference = WD_DocketID;
				}
			}
		}

		protected virtual void CreateUniqueReferenceIfRequiredCore()
		{
			if (WD_ExternalReference.IsEmpty)
			{
				WD_ExternalReference = WD_DocketID;
			}
			else
			{
				WD_ExternalReference = WD_ExternalReference.Left(WhsDocketSchema.WD_ExternalReference.MaxLength - (WD_DocketID.Length + 1)) + " " + WD_DocketID;
			}
		}

		#endregion

		#endregion

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				WD_DocketID = "";
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (WorkflowDescriptors.Instance.TryGetValueSafe(GetWorkflowType()) != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			CreateUniqueReferenceIfRequired();

			if (WD_DocketStatus == DocketStatus.Codes.New)
			{
				WD_DocketStatus = DocketStatus.Codes.Entered;
			}

			if (DoesNotHaveJobEnteredEvent)
			{
				CreateEventLog(Events.WarehouseJobEntered);
			}
		}

		#region Delete

		public override void Delete()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				References.RemoveAndDeleteAll();
				Containers.DeleteAll();
				Pallets.DeleteAll();
				DeleteAllLines();
				WorkflowItems.RemoveAndDeleteAll();
				DocAddresses.RemoveAndDeleteAll();
			}

			DeleteRelatedJobHeaders();
			DeleteRelatedPivots();

			base.Delete();

			DeleteCore();
		}

		protected virtual void DeleteCore()
		{
		}

		#region DeleteAllLines

		void DeleteAllLines()
		{
			if (CanCreateInventory)
			{
				var allLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, PK)).OrderBy(l => l.WE_WE_MatchingLine.IsValid ? 0 : 1); // Make sure matching lines are deleted before deleting the main docketline
				allLines.DeleteAll();
			}
			else
			{
				Lines.DeleteAll();
			}

			if (this.lines != null)
			{
				ListManager.UnhookListChangedEvent(this.lines);
			}
		}

		#endregion

		#region DeleteRelatedJobHeaders

		void DeleteRelatedJobHeaders()
		{
			if (InvoicingSupporter != null)
			{
				InvoicingSupporter.DeleteRelatedJobHeaders();
			}
		}

		#endregion

		#region DeleteRelatedPivots

		void DeleteRelatedPivots()
		{
			if (!IsDeleted)
			{
				foreach (var pivot in GetRelatedPivots())
				{
					pivot.Delete();
				}
			}
		}

		#endregion

		#endregion

		#region Clone

		#region SupportsCloneCore

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region CloneInternal

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsDocket)base.CloneInternal(args);
			clone.CopyJobDocAddressesFrom(this);
			clone.PopulateWD_BookingDateIfRequired();

			foreach (WhsDocketReference reference in References)
			{
				// When creating a Forwarding Shipment from a Warehouse Order it uses the waybill to match an existing shipment.
				// Because of this we don't want to copy across the Waybill Number when cloning the order, otherwise creating a
				// Shipment from the cloned Order would link to the same Shipment.
				if (!reference.WX_RefType.EqualsIgnoringCase(WarehouseAdditionalReferenceTypes.Codes.HouseBill))
				{
					clone.References.Add((WhsDocketReference)reference.Clone());
				}
			}

			var containerArgs = new BusinessObjectCloneArgs(new[] { WhsDocketContainerSchema.Constants.WC_WD });

			foreach (var container in Containers)
			{
				clone.Containers.Add((WhsDocketContainer)container.Clone(containerArgs));
			}

			var palletArgs = new BusinessObjectCloneArgs(new[] { WhsDocketPalletSchema.Constants.W2_WD });

			foreach (var pallet in Pallets)
			{
				clone.Pallets.Add((WhsDocketPallet)pallet.Clone(palletArgs));
			}

			return clone;
		}

		#endregion

		#region GetPropertiesToExcludeFromCloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning =
				new List<string>(base.GetPropertiesToExcludeFromCloning())
				{
					WhsDocketSchema.Constants.WD_DocketID,
					WhsDocketSchema.Constants.WD_WD_ParentDocket,
					WhsDocketSchema.Constants.WD_DocketStatus,
					WhsDocketSchema.Constants.WD_UnitsSent,
					WhsDocketSchema.Constants.WD_PackagesSent,
					WhsDocketSchema.Constants.WD_PalletsSent,
					WhsDocketSchema.Constants.WD_WeightSent,
					WhsDocketSchema.Constants.WD_WeightSentUserEntered,
					WhsDocketSchema.Constants.WD_CubicSent,
					WhsDocketSchema.Constants.WD_OrderClassification,
					WhsDocketSchema.Constants.WD_WLO_PlannedLoad,
					WhsDocketSchema.Constants.WD_GS_NKAssignedPacker,
					WhsDocketSchema.Constants.WD_BookingDate,
					WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc,
					WhsDocketSchema.Constants.WD_CanceledTimeUtc,
					WhsDocketSchema.Constants.WD_GS_NKCanceledBy,
					WhsDocketSchema.Constants.WD_HoldPalletIDPutaway,
					WhsDocketSchema.Constants.WD_FinalisedDate,
					WhsDocketSchema.Constants.WD_GS_NKFinalizedBy,
					WhsDocketSchema.Constants.WD_WP_ParentPickForReceive,
					WhsDocketSchema.Constants.WD_IsPickFaceReplenishment,
					WhsDocketSchema.Constants.WD_CriticalChangesVersionID,
					WhsDocketSchema.Constants.WD_IsInwardsProcessingJob,
					WhsDocketSchema.Constants.WD_BookedWithCBADateTimeUtc,
					WhsDocketSchema.Constants.WD_CustomsParentReference,
					WhsDocketSchema.Constants.WD_WP_PickBeingReplenished,
					WhsDocketSchema.Constants.WD_TZ_TransportZone,
					WhsDocketSchema.Constants.WD_ScreeningStatus,
					WhsDocketSchema.Constants.WD_WP_ParentPickForTransfer,
					WhsDocketSchema.Constants.WD_IsPutawayTransfer,
					WhsDocketSchema.Constants.WD_WD_Split,
					WhsDocketSchema.Constants.WD_ExWhsJobGuid,
					WhsDocketSchema.Constants.WD_ExternalReferenceSplit,
					WhsDocketSchema.Constants.WD_TaskPlanningStatus,
					WhsDocketSchema.Constants.WD_P9_PackingTask,
					WhsDocketSchema.Constants.WD_UnloadCompletedTime,
				};

			return propertiesToExcludeFromCloning;
		}

		#endregion

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsDocketFetchStrategy(this);
		}

		#endregion

		#region Notes

#if DEBUG
		public StmNoteContexts GetNoteContextsForRelatedNotes()
		{
			return NoteContextsForRelatedNotes;
		}
#endif

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var client = Client;
				if (client != null)
				{
					result.Add(client);
				}

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result.Add(warehouse);
				}

				return result.ToArray();
			}
		}

		protected virtual ZString GetGoodsHandlingInstructionsNote()
		{
			var result = ZString.Empty;
			var docketNotes = Notes.FindByDescription((NoResString)"Goods Handling Instructions", false); // Note description for db query.
			var organisationNotes = Client.Notes.FindByDescription((NoResString)"Goods Handling Instructions", false); // Note description for db query.
			var notes = docketNotes.Concat(organisationNotes);

			if (notes.Count() > 1)
			{
				result = ResString.GetMultilingualString("1B49A7A9-5E74-42B0-BC66-27788928D3BF", "Multiple Goods Handling Instructions found.");
			}
			else if (notes.Any())
			{
				result = notes.First().ST_NoteText;
			}

			return result;
		}

		#endregion

		#region DocAddress Requirements

		#region DropOffDocAddressRequirement

		internal JobDocAddressRequirement DropOffDocAddressRequirement
		{
			get { return dropOffDocAddressRequirement ?? (dropOffDocAddressRequirement = GetDropOffDocAddressRequirement()); }
		}

		protected virtual JobDocAddressRequirement GetDropOffDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.DropOffAddress, ContactType.All);
		}

		JobDocAddressRequirement dropOffDocAddressRequirement;

		#endregion

		#region PickUpDocAddressRequirement

		internal JobDocAddressRequirement PickUpDocAddressRequirement
		{
			get { return pickUpDocAddressRequirement ?? (pickUpDocAddressRequirement = GetPickUpDocAddressRequirement()); }
		}

		protected virtual JobDocAddressRequirement GetPickUpDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.PickUpAddress, ContactType.Consignee);
		}

		JobDocAddressRequirement pickUpDocAddressRequirement;

		#endregion

		#region GoodsBillToDocAddressRequirement

		internal JobDocAddressRequirement GoodsBillToDocAddressRequirement
		{
			get { return goodsBillToDocAddressRequirement ?? (goodsBillToDocAddressRequirement = GetGoodsBillToDocAddressRequirement()); }
		}

		protected virtual JobDocAddressRequirement GetGoodsBillToDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.GoodsBillToAddress, ContactType.Consignee);
		}

		JobDocAddressRequirement goodsBillToDocAddressRequirement;

		#endregion

		#region SupplierDocAddressRequirement

		internal JobDocAddressRequirement SupplierDocAddressRequirement
		{
			get { return supplierDocAddressRequirement ?? (supplierDocAddressRequirement = GetSupplierDocAddressRequirement()); }
		}

		protected virtual JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.SupplierDocumentaryAddress, ContactType.Consignor);
		}

		JobDocAddressRequirement supplierDocAddressRequirement;

		#endregion

		#endregion

		#region Related Entities

		protected abstract WhsDocketLineCollection GetNewDocketLineCollection();

		#region ParentDocket

		public WhsDocket ParentDocket
		{
			get { return Factory.Load<WhsDocket>(WD_WD_ParentDocket); }
		}

		#endregion

		#region PickUp

		public virtual ZGuid PickUpPK
		{
			get { return PickUpDocAddress.OrganisationPK; }
			set
			{
				PickUpDocAddress.OrganisationPK = value;
				ValidatePickUpPK();
			}
		}

		public virtual ZPropertyInfo PickUpPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PickUpPK), x => PickUpDocAddress.OrganisationPKInfo); }
		}

		public virtual ZGuid PickUpAddressPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (!PickUpDocAddress.E2_AddressOverride)
				{
					result = PickUpDocAddress.E2_OA_Address;
				}
				return result;
			}
			set
			{
				PickUpDocAddress.E2_OA_Address = value;
				ValidatePickUpPK();
			}
		}

		public ZPropertyInfo PickUpAddressPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PickUpAddressPK), x => PickUpDocAddress.E2_OA_AddressInfo); }
		}

		public virtual JobDocAddress PickUpDocAddress
		{
			get
			{
				if (pickUpDocAddress == null || pickUpDocAddress.IsDeleted)
				{
					pickUpDocAddress = DocAddresses.FindOrCreateWithRequirement(PickUpDocAddressRequirement);
					pickUpDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(JobDocAddress_Readonly);
				}

				return pickUpDocAddress;
			}
		}

		JobDocAddress pickUpDocAddress;

		bool JobDocAddress_Readonly()
		{
			return ReadOnly || NonStandardReadOnly1;
		}

		#endregion

		#region DropOff

		public virtual ZGuid DropOffPK
		{
			get { return DropOffDocAddress.OrganisationPK; }
			set
			{
				DropOffDocAddress.OrganisationPK = value;
				ValidateDropOffPK();
			}
		}

		public virtual ZPropertyInfo DropOffPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DropOffPK), x => DropOffDocAddress.OrganisationPKInfo); }
		}

		public virtual ZGuid DropOffAddressPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (!DropOffDocAddress.E2_AddressOverride)
				{
					result = DropOffDocAddress.E2_OA_Address;
				}
				return result;
			}
			set
			{
				DropOffDocAddress.E2_OA_Address = value;
				ValidateDropOffPK();
			}
		}

		public ZPropertyInfo DropOffAddressPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DropOffAddressPK), x => DropOffDocAddress.E2_OA_AddressInfo); }
		}

		public virtual JobDocAddress DropOffDocAddress
		{
			get
			{
				if (dropOffDocAddress == null || dropOffDocAddress.IsDeleted)
				{
					dropOffDocAddress = DocAddresses.FindOrCreateWithRequirement(DropOffDocAddressRequirement);
					dropOffDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(JobDocAddress_Readonly);
				}

				return dropOffDocAddress;
			}
		}
		JobDocAddress dropOffDocAddress;

		#endregion

		#region GoodsBillTo

		public OrgHeader GoodsBillTo
		{
			get { return !GoodsBillToDocAddress.E2_AddressOverride ? GoodsBillToDocAddress.Organisation : null; }
		}

		public virtual ZGuid GoodsBillToPK
		{
			get { return GoodsBillToDocAddress.OrganisationPK; }
			set
			{
				GoodsBillToDocAddress.OrganisationPK = value;
				ValidateGoodsBillToPK();
			}
		}

		public virtual ZPropertyInfo GoodsBillToPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GoodsBillToPK), x => GoodsBillToDocAddress.OrganisationPKInfo); }
		}

		public virtual ZGuid GoodsBillToAddressPK
		{
			get { return !GoodsBillToDocAddress.E2_AddressOverride ? GoodsBillToDocAddress.E2_OA_Address : ZGuid.Empty; }
			set
			{
				GoodsBillToDocAddress.E2_OA_Address = value;
				ValidateGoodsBillToPK();
			}
		}

		public ZPropertyInfo GoodsBillToAddressPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GoodsBillToAddressPK), x => GoodsBillToDocAddress.E2_OA_AddressInfo); }
		}

		public virtual JobDocAddress GoodsBillToDocAddress
		{
			get
			{
				if (goodsBillToDocAddress == null || goodsBillToDocAddress.IsDeleted)
				{
					goodsBillToDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsBillToAddress);
					goodsBillToDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(GoodsBillToDocAddress_ReadOnly);
				}

				return goodsBillToDocAddress;
			}
		}

		protected virtual bool GoodsBillToDocAddress_ReadOnly()
		{
			return ReadOnly || NonStandardReadOnly1;
		}

		JobDocAddress goodsBillToDocAddress;

		#endregion

		#region TransportBillTo

		public OrgHeader TransportBillTo
		{
			get { return !TransportBillToDocAddress.E2_AddressOverride ? TransportBillToDocAddress.Organisation : null; }
		}

		public JobDocAddress TransportBillToDocAddress
		{
			get
			{
				if (transportBillToDocAddress == null || transportBillToDocAddress.IsDeleted)
				{
					transportBillToDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.TransportBillToAddress);
					transportBillToDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(TransportBillToDocAddress_ReadOnly);
				}

				return transportBillToDocAddress;
			}
		}

		protected virtual bool TransportBillToDocAddress_ReadOnly()
		{
			return ReadOnly || NonStandardReadOnly1;
		}

		JobDocAddress transportBillToDocAddress;

		#endregion

		#region SupplierDocAddress

		public OrgHeader Supplier
		{
			get { return !SupplierDocAddress.E2_AddressOverride ? SupplierDocAddress.Organisation : null; }
		}

		public ZGuid SupplierPK => SupplierDocAddress.OrganisationPK;

		public virtual JobDocAddress SupplierDocAddress
		{
			get
			{
				if (supplierDocAddress == null || supplierDocAddress.IsDeleted)
				{
					supplierDocAddress = DocAddresses.FindOrCreateWithRequirement(SupplierDocAddressRequirement);
					supplierDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(JobDocAddress_Readonly);
				}

				return supplierDocAddress;
			}
		}

		JobDocAddress supplierDocAddress;

		#endregion

		#region TransportCo

		public OrgHeader TransportCo => TransportCoCore;

		protected abstract OrgHeader TransportCoCore { get; }

		#endregion

		#region Lines

		[ChildEditable(true)]
		public WhsDocketLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = GetNewDocketLineCollection();

					ListManager.HookListChangedEvent(lines);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}

		public bool IsLinesInitialised => lines != null;

		internal void UpdateChangesCacheOnRemove(WhsDocketLine line)
		{
			UpdateChangesCacheOnRemove(line.SupplierPart, line.WE_TransactionQuantity, line.WE_LineNo);
		}

		internal void UpdateChangesCacheOnRemove(OrgSupplierPart part, ZDecimal quantity, ZShort lineNo)
		{
			ListManager.UpdateChangesCacheOnRemove(part, quantity, lineNo);
		}

		protected virtual bool ShouldUpdateWeightAndVolumeOnRemove => true;

		#region ListManager

		protected ListChangedManager ListManager
		{
			get { return listManager ?? (listManager = new ListChangedManager(this)); }
		}

		ListChangedManager listManager;

		protected class ListChangedManager
		{
			public ListChangedManager(WhsDocket docket)
			{
				Docket = Argument.NotNull(docket, "docket");
			}

			readonly WhsDocket Docket;

			#region Hook / Unhook

			public void HookListChangedEvent(IBindingList list)
			{
				list.ListChanged += List_ListChanged;
			}

			public void UnhookListChangedEvent(IBindingList list)
			{
				list.ListChanged -= List_ListChanged;
			}

			#endregion

			#region List_ListChanged

			void List_ListChanged(object sender, ListChangedEventArgs e)
			{
				if (!Docket.IsDeleted)
				{
					var isLinesRemoved = (e.ListChangedType == ListChangedType.Reset || e.ListChangedType == ListChangedType.ItemDeleted);
					if (isLinesRemoved)
					{
						UpdateTotalWeightAndVolumeAndMaxLineNo();
					}

					if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted)
					{
						Docket.WD_WW_WhsInfo.RefreshBinding();
						Docket.WD_OH_ClientInfo.RefreshBinding();
						Docket.WD_DocketSubTypeInfo.RefreshBinding();
					}
				}
			}

			void UpdateTotalWeightAndVolumeAndMaxLineNo()
			{
				try
				{
					foreach (var qtyDelta in QtyChangesPerProductForDeleteCache)
					{
						Docket.UpdateTotalWeightAndVolume(qtyDelta.Key, qtyDelta.Value);
					}

					if (MaxLineNoDeleted > 0)
					{
						Docket.MaxLineNoManager.LineNoDeleted(MaxLineNoDeleted);
					}
				}
				finally
				{
					QtyChangesPerProductForDeleteCache.Clear();
					MaxLineNoDeleted = ZShort.Zero;
				}

				Docket.WD_TotalUnitsFromLinesInfo.RefreshBinding(); // force recalculation.
			}

			#endregion

			#region UpdateChangesCacheOnRemove

			public void UpdateChangesCacheOnRemove(OrgSupplierPart part, ZDecimal quantity, ZShort lineNo)
			{
				if (Docket.ShouldUpdateWeightAndVolumeOnRemove && part != null && Docket.ShouldUpdateWeightAndVolumeOnTheFly && quantity > 0m)
				{
					ZDecimal result;
					if (!QtyChangesPerProductForDeleteCache.TryGetValue(part, out result))
					{
						QtyChangesPerProductForDeleteCache[part] = -quantity;
					}
					else
					{
						QtyChangesPerProductForDeleteCache[part] = result - quantity;
					}
				}

				if (MaxLineNoDeleted < lineNo)
				{
					MaxLineNoDeleted = lineNo;
				}
			}

			#endregion

			#region QtyChangesPerProductForDeleteCache

			/// <summary>
			/// Each time a line is deleted, we add that line's qty to the total sum removed for the product, as a negative.
			/// This is then stored in the cache, per product.
			/// This is because unit conversions are costly, and so they will be suspended until lines are finished deleting.
			/// Once deletion has finished, the cache is then used to unit convert each unique product qty to the Docket Unit, and deducted
			/// from the running total on the Docket.
			/// </summary>
			Dictionary<OrgSupplierPart, ZDecimal> QtyChangesPerProductForDeleteCache
			{
				get { return qtyChangesPerProductforDeleteCache ?? (qtyChangesPerProductforDeleteCache = new Dictionary<OrgSupplierPart, ZDecimal>()); }
			}

			Dictionary<OrgSupplierPart, ZDecimal> qtyChangesPerProductforDeleteCache;
			ZShort MaxLineNoDeleted;

			#endregion
		}

		#endregion

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WD_WW_Whs); }
		}

		#endregion

		#region References

		[ChildEditable(true)]
		public WhsDocketReferenceCollection References
		{
			get
			{
				if (references == null)
				{
					references = new WhsDocketReferenceCollection(this, Factory);
					references.Load();
					RegisterEditableChildObject(references);
					AfterReferencesLoaded();
				}
				return references;
			}
		}

		protected virtual void AfterReferencesLoaded() { }

		#endregion

		#region Containers

		[ChildEditable(true)]
		public WhsDocketContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new WhsDocketContainerCollection(this, Factory);
					RegisterEditableChildObject(containers);
					AfterContainersLoaded();
				}
				return containers;
			}
		}

		protected virtual void AfterContainersLoaded() { }

		#endregion

		#region Pallets

		[ChildEditable(true)]
		public WhsDocketPalletCollection Pallets
		{
			get
			{
				if (hirePallets == null)
				{
					hirePallets = new WhsDocketPalletCollection(this, Factory);
					RegisterEditableChildObject(hirePallets);
				}
				return hirePallets;
			}
		}

		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region BusinessObjectsWithRelatedEvents

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				JobHeader header = JobHeader;
				if (header != null)
				{
					result.Add(header);
				}
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region ClientCode

		public ZString GetClientCode(ZString orgCusCodeType)
		{
			ZString result = "";
			if (Client != null)
			{
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_OH, WD_OH_Client);
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, orgCusCodeType);
				OrgCusCode code = Factory.LoadTop1<OrgCusCode>(query);
				if (code != null)
				{
					result = code.OK_CustomsRegNo;
				}
			}
			return result;
		}

		#endregion

		#region Inventory Filter

		WhsInventoryViewCollection inventoryFilter;
		public WhsInventoryViewCollection InventoryFilter
		{
			get { return inventoryFilter ?? (inventoryFilter = new WhsInventoryViewCollection(Factory)); }
		}

		#endregion

		#region AccTranLines

		public AccTransactionLinesCollection AccTranLines
		{
			get
			{
				if (accTranLines == null)
				{
					var laccTranLines = new AccTransactionLinesCollection(Factory);

					var jobHeader = this.JobHeader;
					if (jobHeader != null)
					{
						ZQuery query = new ZQuery(AccTransactionLinesSchema.AL_JH, jobHeader.PK);
						query.AddToFilter(AccTransactionLinesSchema.AL_GC, jobHeader.JH_GC);
						laccTranLines.Load(query);
					}
					accTranLines = laccTranLines;
				}
				return accTranLines;
			}
		}

#if DEBUG
		internal void ClearAccTranLinesCache()
		{
			accTranLines = null;
			IsAtLeastOneAccTransactionLinePosted_Cached = null;
		}
#endif

		AccTransactionLinesCollection accTranLines;

		#endregion

		#region RelatedJobs

		public RelatedJobCollection RelatedJobs
		{
			get
			{
				if (relatedJobs == null)
				{
					relatedJobs = new RelatedJobCollection(Factory);
					relatedJobs.AddRange(GetRelatedJobsCore());
				}
				return relatedJobs;
			}
		}

		protected virtual List<IRelatedJob> GetRelatedJobsCore()
		{
			return new List<IRelatedJob>();
		}

		// We need to keep instance of Related Jobs or Binding will not work.
		protected void ReloadRelatedJobs()
		{
			if (relatedJobs != null)
			{
				RelatedJobs.RemoveAll();
				RelatedJobs.AddRange(GetRelatedJobsCore());
			}
		}

		public IEnumerable<BusinessObject> GetRelatedParents()
		{
			IEnumerable<BusinessObject> result = null;

			if (!IsDeleted)
			{
				var pivots = GetRelatedPivots();
				var parents = new List<BusinessObject>(pivots.Length);

				foreach (var pivot in pivots)
				{
					var relatedJob = Factory.Load(pivot.WV_ParentTableCode, pivot.WV_ParentId);
					if (relatedJob != null)
					{
						parents.Add(relatedJob);
					}
				}

				result = parents;
			}

			return result ?? Enumerable.Empty<BusinessObject>();
		}

		WhsDocketJobPivot[] GetRelatedPivots()
		{
			var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, PK);
			pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, WD_DocketType);

			return Factory.Load<WhsDocketJobPivot>(pivotQuery);
		}

		RelatedJobCollection relatedJobs;

		#endregion

		#region MaxLineNoManager

		public MaxLineNoManager MaxLineNoManager
		{
			get { return maxLineNoManager ?? (maxLineNoManager = new MaxLineNoManager(this)); }
		}
		MaxLineNoManager maxLineNoManager;

		#endregion

		#region InventoryToPrintPalletLabelFor

		public WhsInventoryView InventoryToPrintPalletLabelFor
		{
			get;
			set;
		}

		#endregion

		#region CarrierServiceLevel

		public abstract override OrgCarrierServiceLevel CarrierServiceLevel { get; }

		#endregion

		#region DocketDeviceStrategy

		public WhsDocketRelatedEntityOperationsStrategy DocketRelatedEntityOperationsStrategy
		{
			get { return docketRelatedEntityOperationsStrategy ?? (docketRelatedEntityOperationsStrategy = WhsEnvironment.IsRF ? new WhsDocketRelatedEntityOperationsRFStrategy(this) : new WhsDocketRelatedEntityOperationsStrategy(this)); }
		}

		WhsDocketRelatedEntityOperationsStrategy docketRelatedEntityOperationsStrategy;

		#endregion

		#endregion

		#region CheckExternalReferenceForDuplicates

		public bool CheckExternalReferenceForDuplicates()
		{
			var shouldCheck = !IsInDatabase || WD_ExternalReferenceInfo.HasChanges || WD_OH_ClientInfo.HasChanges
				|| WD_ExternalReferenceSplitInfo.HasChanges || WD_DocketTypeInfo.HasChanges;

			if (shouldCheck)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, WD_OH_Client);
				query.AddToFilter(WhsDocketSchema.WD_DocketType, WD_DocketType);
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, WD_ExternalReference);
				query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, WD_ExternalReferenceSplit);
				query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, PK);

				return Factory.LoadTop1<WhsDocket>(query) != null;
			}

			return false;
		}

		#endregion

		#region Properties

		#region Description

		public ZString Description
		{
			get { return DescriptionCore; }
		}

		protected abstract ZString DescriptionCore { get; }

		#endregion

		#region TransportZone

		[List(("Lookups.TransportZones"))]
		public override ZGuid WD_TZ_TransportZone
		{
			get { return base.WD_TZ_TransportZone; }
		}

		public RateTransportZone TransportZone
		{
			get { return TransportZoneCore; }
		}

		protected virtual RateTransportZone TransportZoneCore
		{
			get { return Factory.Load<RateTransportZone>(WD_TZ_TransportZone); }
		}

		public ZString TransportZoneName
		{
			get { return TransportZone?.TZ_ZoneName ?? ZString.Empty; }
		}

		#endregion

		#region CalculateTotalsEnabled

		public bool CalculateTotalsEnabled
		{
			get { return this.calculateTotalsEnabled; }
			set { this.calculateTotalsEnabled = value; }
		}

		bool calculateTotalsEnabled = true;

		#endregion

		#region CountryCode

		public ZString CountryCode => Warehouse?.CountryCode ?? ZString.Empty;

		#endregion

		#region ClientName

		[ResourceStringData("WhsDocket|ClientName", Caption = "Client Name")]
		public ZString ClientName
		{
			get
			{
				var client = Client;
				return client != null ? client.OH_FullNameTruncated : ZString.Empty;
			}
		}

		#endregion

		#region DistributionCentreDocAddress

		public JobDocAddress DistributionCentreDocAddress
		{
			get
			{
				if (distributionCentreDocAddress == null || distributionCentreDocAddress.IsDeleted)
				{
					distributionCentreDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.DistributionCentreAddress);
					distributionCentreDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(DistributionCentreDocAddressReadOnly);
				}

				return distributionCentreDocAddress;
			}
		}
		JobDocAddress distributionCentreDocAddress;

		protected virtual bool DistributionCentreDocAddressReadOnly()
		{
			return false;
		}

		protected OrgHeader DistributionCentre
		{
			get { return !DistributionCentreDocAddress.E2_AddressOverride ? DistributionCentreDocAddress.Organisation : null; }
		}

		#endregion

		#region PropagadesFinalisedDateAndStatusToLines

		public bool PropagatesFinalisedDateAndStatusToLines
		{
			get { return PropagatesFinalisedDateAndStatusToLinesCore; }
		}

		protected virtual bool PropagatesFinalisedDateAndStatusToLinesCore
		{
			get { return true; }
		}

		#endregion

		#region TransportCoName

		// Only here to easily share attributes between PickableDocket and Receive

		[ResourceStringData("WhsDocket|TransportCoName", Caption = "Transport Company Name", MediumCaption = "Trans. Co. Name", ShortCaption = "Trans. Name")]
		public ZString TransportCoName => TransportCoNameCore;

		protected virtual ZString TransportCoNameCore => throw new NotSupportedException();

		#endregion

		#region TransportCoNameOrPK For Grid Binding

		// Only here to easily share attributes between PickableDocket and Receive

		[BusinessObjectTestExclude]
		[List("Lookups.TransportCos")]
		[RelatedBusinessObject(nameof(TransportCo))]
		[MaxLength(nameof(TransportCoNameOrPKMaxLength))]
		[ReadOnlyMember(nameof(TransportCompanyReadOnly))]
		public ZString TransportCoNameOrPK
		{
			get => TransportCoNameOrPKCore;
			set => TransportCoNameOrPKCore = value;
		}

		protected virtual ZString TransportCoNameOrPKCore
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		protected abstract int TransportCoNameOrPKMaxLength { get; }

		#endregion

		#region SupplierName

		[ResourceStringData("WhsDocket|SupplierName", Caption = "Supplier Name", ShortCaption = "Sup. Name")]
		public ZString SupplierName
		{
			get { return SupplierDocAddress.E2_CompanyNameTruncated; }
		}

		#endregion

		#region NeedToAutoGenerateLineNumbersOnCreation

		public virtual bool NeedToAutoGenerateLineNumbersOnCreation
		{
			get
			{
				return true;
			}
		}

		#endregion

		#region Container

		#region ContainerID

		[ResourceStringData("WhsDocket|ContainerID", Caption = "Container ID")]
		public ZString ContainerID
		{
			get
			{
				var containersCollection = Containers.Cast<WhsDocketContainer>();
				ZString result = "";

				var container = containersCollection.FirstOrDefault();
				if (container != null)
				{
					result = containersCollection.All(c => c.WC_RC == container.WC_RC)
						? string.Join(", ", containersCollection.Where(c => !c.WC_ContainerNum.IsEmpty).Select(c => c.WC_ContainerNum))
						: ResString.GetMultilingualString("08f8b014-b1ea-491b-a46e-fe093560f031", "Many");
				}

				return result;
			}
		}

		#endregion

		#region ContainerType

		[ResourceStringData("WhsDocket|ContainerType", Caption = "Container Type")]
		public ZString ContainerType => Containers.Cast<WhsDocketContainer>().GetSingleValueOrManyText(x => x.Container?.RC_Code ?? ZString.Empty, x => x.WC_RC);

		#endregion

		#endregion

		#region WD_OH_Client

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(ClientReadOnly))]
		[RelatedBusinessObject("Client")]
		public override ZGuid WD_OH_Client
		{
			get { return base.WD_OH_Client; }
			set
			{
				if (base.WD_OH_Client != value)
				{
					base.WD_OH_Client = value;
					Validation.ValidateWD_DocketSubType();
					OnClientChanged();
				}
			}
		}

		#endregion

		#region WD_OH_Forwarder

		[ActionField(ReadOnly = true)]
		public override ZGuid WD_OH_Forwarder
		{
			get { return base.WD_OH_Forwarder; }
			set { base.WD_OH_Forwarder = value; }
		}

		#endregion

		#region WD_WW_Whs

		[ReadOnlyMember(nameof(WarehouseReadOnly))]
		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WD_WW_Whs
		{
			get { return base.WD_WW_Whs; }
			set
			{
				if (base.WD_WW_Whs != value)
				{
					SetWarehouseFKAndDefaultsForCustomsTransaction(value);
					Validation.ValidateWD_DocketSubType();
					Validation.ValidateWD_IsInwardsProcessingJob();
					OnWarehouseChanged();
					PopulateWD_BookingDateIfRequired();
				}
			}
		}

		void PopulateWD_BookingDateIfRequired()
		{
			if (!IsInDatabase && WD_BookingDate.IsEmpty)
			{
				WD_BookingDate = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			}
		}

		#endregion

		#region WD_F3_NKTotalPackType

		[List("Lookups.ProductUQ")]
		public override ZString WD_F3_NKTotalPackType
		{
			get { return base.WD_F3_NKTotalPackType; }
			set { base.WD_F3_NKTotalPackType = value; }
		}

		#endregion

		#region WD_DropMode

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[List("Lookups.DropModes")]
		public override ZString WD_DropMode
		{
			get { return base.WD_DropMode; }
			set { base.WD_DropMode = value; }
		}

		#endregion

		#region WD_PL_NkCarrierServiceLevel

		[ReadOnlyMember(nameof(CarrierServiceLevelReadOnly))]
		public override ZString WD_PL_NKCarrierServiceLevel
		{
			get { return base.WD_PL_NKCarrierServiceLevel; }
			set { base.WD_PL_NKCarrierServiceLevel = value; }
		}

		#endregion

		#region WD_CODPayMethod

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[List("Lookups.ShipperCODPaymentTypes")]
		public override ZString WD_CODPayMethod
		{
			get { return base.WD_CODPayMethod; }
			set { base.WD_CODPayMethod = value; }
		}

		#endregion

		#region WD_Inco

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[List("Lookups.INCOTerms")]
		public override ZString WD_INCO
		{
			get { return base.WD_INCO; }
			set { base.WD_INCO = value; }
		}

		#endregion

		#region WD_TransportMode

		[List("Lookups.TransportModes")]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_TransportMode
		{
			get { return base.WD_TransportMode; }
			set { base.WD_TransportMode = value; }
		}

		#endregion

		#region WD_ContainerMode

		[List("Lookups.ContainerModes")]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_ContainerMode
		{
			get { return base.WD_ContainerMode; }
			set { base.WD_ContainerMode = value; }
		}

		#endregion

		#region WD_DocketStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WD_DocketStatus
		{
			get { return base.WD_DocketStatus; }
			set
			{
				if (value != WD_DocketStatus)
				{
					var cancelError = ZString.Empty;
					if (value != DocketStatus.Codes.Cancelled || (cancelError = CanCancel()).IsEmpty)
					{
						var oldStatus = WD_DocketStatus;
						base.WD_DocketStatus = value;
						OnDocketStatusChanged(oldStatus);
					}
					else
					{
						CancelFailed?.Invoke(this, new TextEventArgs(cancelError));
					}
				}
			}
		}

		void PropagateStatusChangeToLines(string oldStatus)
		{
			if (((IBusinessObjectCollection)LinesToPropagateStatusAndFinDateChange).IsLoaded || ((IBusinessObjectCollection)Lines).IsLoaded)
			{
				if (IsCancelled)
				{
					foreach (var line in LinesToPropagateStatusAndFinDateChange)
					{
						line.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
					}
				}
				else if (oldStatus == DocketStatus.Codes.Cancelled)
				{
					foreach (var line in LinesToPropagateStatusAndFinDateChange)
					{
						line.WE_DocketLineStatus = ReactivedDocketLineDefaultStatus(line);
					}
				}
				else if (PropagatesFinalisedDateAndStatusToLines)
				{
					PropagatesFinalisedStatusChangeToLinesCore();
				}
			}
		}

		protected virtual void PropagatesFinalisedStatusChangeToLinesCore()
		{
			if (IsFinalised)
			{
				foreach (var line in LinesToPropagateStatusAndFinDateChange.Where(l => !l.WE_DocketLineStatus.EqualsIgnoringCase(DocketLineStatus.Codes.Finalised)))
				{
					line.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
				}
			}
			else
			{
				ResetDocketLineStatusIfRequired();
			}
		}

		protected void ResetDocketLineStatusIfRequired()
		{
			var linesToResetDocketLineStatus = LinesToPropagateStatusAndFinDateChange.Where(l => !l.WE_DocketLineStatus.IsEmpty && !l.WE_DocketLineStatus.EqualsIgnoringCase(DocketLineStatus.Codes.PickedForUnload));
			foreach (var line in linesToResetDocketLineStatus)
			{
				line.WE_DocketLineStatus = "";
			}
		}

		protected virtual string ReactivedDocketLineDefaultStatus(WhsDocketLine line)
		{
			return ZString.Empty;
		}

		protected virtual WhsDocketLineCollection LinesToPropagateStatusAndFinDateChange
		{
			get { return Lines; }
		}

		#endregion

		#region WD_DocketType

		[ActionField(ReadOnly = true)]
		public override ZString WD_DocketType
		{
			get { return base.WD_DocketType; }
			set { base.WD_DocketType = value; }
		}

		#endregion

		#region WD_DocketSubType

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(DocketSubTypeReadOnly))]
		[List("Lookups.SubTypes")]
		public override ZString WD_DocketSubType
		{
			get { return base.WD_DocketSubType; }
			set
			{
				if (WD_DocketSubType != value)
				{
					var previousSubType = WD_DocketSubType;
					base.WD_DocketSubType = value;
					Validation.ValidateWD_OH_Client();
					Validation.ValidateWD_WW_Whs();
					Validation.ValidateWD_IsInwardsProcessingJob();
					OnDocketSubTypeChanged(previousSubType);
				}
			}
		}

		#endregion

		#region WD_TaskPlanningStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WD_TaskPlanningStatus
		{
			get => base.WD_TaskPlanningStatus;
			set => base.WD_TaskPlanningStatus = value;
		}

		#endregion

		#region WD_P9_PackingTask

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WD_P9_PackingTask
		{
			get => base.WD_P9_PackingTask;
			set => base.WD_P9_PackingTask = value;
		}

		#endregion

		#region WD_IsInwardsProcessingJob

		[ActionField(ReadOnly = true)]
		public override ZBool WD_IsInwardsProcessingJob
		{
			get => base.WD_IsInwardsProcessingJob;
			set
			{
				base.WD_IsInwardsProcessingJob = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWD_WW_Whs();
					Validation.ValidateWD_DocketSubType();
				}
			}
		}

		#endregion

		#region TotalWeightUnits

		public CodeDescriptionPairList TotalWeightUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region TotalCubicUnits

		public CodeDescriptionPairList TotalCubicUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region WD_TotalUnitsFromLines

		[ReadOnly(true)]
		public ZDecimal WD_TotalUnitsFromLines
		{
			get { return WD_TotalUnitsFromLinesCore; }
		}

		protected virtual ZDecimal WD_TotalUnitsFromLinesCore
		{
			get
			{
				ZDecimal total = 0m;
				foreach (WhsDocketLine line in Lines)
				{
					total += line.WE_TransactionQuantity;
				}
				return total;
			}
		}

		public ZPropertyInfo WD_TotalUnitsFromLinesInfo
		{
			get { return GetZPropertyInfo(Schema.WD_TotalUnitsFromLines); }
		}

		#endregion

		#region WD_BOLNo

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[MaxLength(25)]
		public virtual ZString WD_BOLNo
		{
			get { return GetReference("HSB"); }
			set
			{
				SetReference("HSB", value);
				WD_BOLNoInfo.RefreshBinding();
			}
		}

		#endregion

		#region WD_WhsOrderFulfillmentRule

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.WhsOrderFulfillmentRules")]
		[BusinessObjectTestExclude]
		public override sealed ZString WD_WhsOrderFulfillmentRule
		{
			get { return base.WD_WhsOrderFulfillmentRule; }
			set
			{
				if (!SupportsWhsOrderFulfillmentRule)
				{
					throw new NotSupportedException(string.Format(Culture.Invariant, "Docket type {0} does not support setting {1}.", GetType().Name, WD_WhsOrderFulfillmentRuleInfo.Name));
				}
				base.WD_WhsOrderFulfillmentRule = value;
			}
		}

		protected virtual bool SupportsWhsOrderFulfillmentRule
		{
			get { return false; }
		}

		#endregion

		#region WD_PickOption

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.PickOptions")]
		public override ZString WD_PickOption
		{
			get { return base.WD_PickOption; }
			set { base.WD_PickOption = value; }
		}

		#endregion

		#region WD_ExternalReference

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_ExternalReference
		{
			get { return base.WD_ExternalReference; }
			set
			{
				//var oldReference = WD_ExternalReference;

				base.WD_ExternalReference = value.Trim();
				//UpdateReferenceOnCharges(oldReference);
			}
		}

		// TODO: Move this into save as it could be expensive. Uncomment when doing (Reference Split or DocketID) for autorating WI.
		//void UpdateReferenceOnCharges(ZString oldReference)
		//{
		//    var job = (Job)JobHeader;
		//    if (job != null)
		//    {
		//        foreach (JobCharge charge in job.Charges)
		//        {
		//            var docketReferenceAttribute = charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.DocketReference);
		//            if (docketReferenceAttribute != null)
		//            {
		//                docketReferenceAttribute.EC_Value = WD_ExternalReference;
		//            }
		//            charge.JR_Desc = charge.JR_Desc.Replace(oldReference, WD_ExternalReference);
		//        }
		//    }
		//}

		#endregion

		#region WD_TotalOrderValue

		public override ZDecimal WD_TotalOrderValue
		{
			get { return base.WD_TotalOrderValue; }
			set
			{
				if (WD_RX_NKTotalOrderCurrency.IsEmpty)
				{
					WD_RX_NKTotalOrderCurrency = GetMostCommonOrderCurrency();
				}

				base.WD_TotalOrderValue = value;
			}
		}

		public ZDecimal GetTotalOrderLineValue()
		{
			return Utilities.Round(Lines.Sum(l => l.WE_ExtendedLinePrice), 2);
		}

		public void UpdateTotalOrderValue(ZDecimal originalTotal)
		{
			var commonLineCurrency = GetCommonValidLineCurrency();
			if (WD_TotalOrderValue == originalTotal && !commonLineCurrency.IsEmpty && (WD_RX_NKTotalOrderCurrency.IsEmpty || commonLineCurrency == WD_RX_NKTotalOrderCurrency))
			{
				WD_TotalOrderValue = GetTotalOrderLineValue();
			}
		}

		#endregion

		#region WD_RX_NKTotalOrderCurrency

		public ZString GetMostCommonOrderCurrency()
		{
			var result = GetCommonValidLineCurrency();
			result = !result.IsEmpty ? result : WD_RX_NKTotalOrderCurrency;
			return !result.IsEmpty ? result : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		public ZString GetCommonValidLineCurrency()
		{
			var lineCurrencies = Lines.Where(l => !l.WE_RX_NKUnitPriceCurrency.IsEmpty).Select(l => l.WE_RX_NKUnitPriceCurrency).Distinct();
			return lineCurrencies.Count() == 1 ? lineCurrencies.First() : ZString.Empty;
		}

		#endregion

		#region WD_RS_NKServiceLevel

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[ResourceStringData("WhsDocket|WD_RS_NKServiceLevel", Caption = "Service Level", ShortCaption = "Svc. lvl.")]
		public override ZString WD_RS_NKServiceLevel
		{
			get { return base.WD_RS_NKServiceLevel; }
			set { base.WD_RS_NKServiceLevel = value; }
		}

		#endregion

		#region AwaitingCustomsResponseStatus

		public ZString AwaitingCustomsResponseStatus
		{
			get { return IsFinaliseAllowed ? (NoResString)"" : Res.GetString("WhsDocket|AwaitingCustomsResponseStatus", "Awaiting Customs Response"); }
		}

		public ZPropertyInfo AwaitingCustomsResponseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.AwaitingCustomsResponseStatus); }
		}

		#endregion

		#region SubTypeDesc

		public ZString SubTypeDesc
		{
			get { return SubTypeDescCore(); }
		}

		protected virtual ZString SubTypeDescCore()
		{
			return !WD_DocketSubType.IsEmpty ? Lookups.SubTypes.GetDescriptionFromCode(WD_DocketSubType) : "";
		}

		public ZPropertyInfo SubTypeDescInfo
		{
			get { return GetZPropertyInfo(nameof(SubTypeDesc)); }
		}

		#endregion

		#region Statuses

		public DocketStatus Statuses => new DocketStatus();

		public ZString WD_DocketStatusDescription => WD_DocketStatusDescriptionCore;

		protected virtual ZString WD_DocketStatusDescriptionCore
			=> Statuses.GetDescriptionFromCode(WD_DocketStatus);

		public ZPropertyInfo WD_DocketStatusDescriptionInfo
			=> GetZPropertyInfo(Schema.WD_DocketStatusDescription);

		#endregion

		#region ProductCount

		public ZInt ProductCount => ProductCountCore;

		protected virtual ZInt ProductCountCore => throw new NotSupportedException();

		#endregion

		#region DisplayAllocationKey

		public bool DocketTypeSupportsAllocationKey => DocketTypeSupportsAllocationKeyCore;

		protected virtual bool DocketTypeSupportsAllocationKeyCore => false;

		#endregion

		#region RequiredDate

		[ReadOnlyMember(nameof(RequiredDateReadOnly))]
		[BusinessObjectTestExclude] // we set to end of day
		public ZDateTime RequiredDate
		{
			get => WD_RequiredDate.ToZDateTime();
			set
			{
				WD_RequiredDate = AppendWarehouseOffset(value);
			}
		}

		protected virtual ZDateTimeOffset AppendWarehouseOffset(ZDateTime value)
		{
			var result = ZDateTimeOffset.Empty;
			if (value.IsValid && !value.IsEmpty)
			{
				var offset = Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.ToUniversalBranchTime());
				result = new ZDateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, offset.Offset);
			}
			return result;
		}

		public ZPropertyInfo RequiredDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(RequiredDate)); }
		}

		#endregion

		#endregion

		#region Property Infos

		#region WD_ArrivalDate

		[ReadOnlyMember(nameof(IsFinalisedOrCancelled))]
		public override ZDateTimeOffset WD_ArrivalDate
		{
			get { return base.WD_ArrivalDate; }
			set { base.WD_ArrivalDate = value; }
		}

		#endregion

		#region WD_BookingDate

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDateTimeOffset WD_BookingDate
		{
			get { return base.WD_BookingDate; }
			set { base.WD_BookingDate = value; }
		}

		#endregion

		#region WD_CustomerReference

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_CustomerReference
		{
			get { return base.WD_CustomerReference; }
			set { base.WD_CustomerReference = value; }
		}

		#endregion

		#region WD_DocketID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WD_DocketID
		{
			get { return base.WD_DocketID; }
			set { base.WD_DocketID = value; }
		}

		#endregion

		#region WD_ETA

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDateTimeOffset WD_ETA
		{
			get { return base.WD_ETA; }
			set { base.WD_ETA = value; }
		}

		#endregion

		#region WD_ETD

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDateTimeOffset WD_ETD
		{
			get { return base.WD_ETD; }
			set { base.WD_ETD = value; }
		}

		#endregion

		#region WD_ExternalReferenceSplit

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZByte WD_ExternalReferenceSplit
		{
			get { return base.WD_ExternalReferenceSplit; }
			set { base.WD_ExternalReferenceSplit = value; }
		}

		#endregion

		#region WD_FinalisedDate

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WD_FinalisedDate
		{
			get => base.WD_FinalisedDate;
			set
			{
				base.WD_FinalisedDate = value.IsValid ? ZDateTimeOffset.TruncateMilliseconds(value) : value;
				UpdateFinalizedByIfNecessary();
				DocketFinalisedDateChanged?.Invoke(this, EventArgs.Empty);
				PropagateFinalisedDateChangeToLines();
			}
		}

		void UpdateFinalizedByIfNecessary()
		{
			var finalizedBy = WD_FinalisedDate.IsValid ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			if (WD_GS_NKFinalizedBy != finalizedBy)
			{
				WD_GS_NKFinalizedBy = finalizedBy;
			}
		}

		void PropagateFinalisedDateChangeToLines()
		{
			if (PropagatesFinalisedDateAndStatusToLines && ((IBusinessObjectCollection)LinesToPropagateStatusAndFinDateChange).IsLoaded)
			{
				var linesToChange = LinesToPropagateStatusAndFinDateChange.Where(l => l.WE_FinalisedDate != WD_FinalisedDate).ToArray();
				if (linesToChange.Length > 0)
				{
					PropagateFinalisedDateChangeToLinesCore(linesToChange, WD_FinalisedDate);
				}
			}
		}

		protected virtual void PropagateFinalisedDateChangeToLinesCore(IEnumerable<WhsDocketLine> lines, ZDateTimeOffset docketFinalisedDate)
		{
			lines.ForEach(l => l.WE_FinalisedDate = docketFinalisedDate);
		}

		#endregion

		#region WD_LocalCartInsuranceCost

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_LocalCartInsuranceCost
		{
			get { return base.WD_LocalCartInsuranceCost; }
			set { base.WD_LocalCartInsuranceCost = value; }
		}

		#endregion

		#region WD_RequiredDate

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDateTimeOffset WD_RequiredDate
		{
			get { return base.WD_RequiredDate; }
			set { base.WD_RequiredDate = value; }
		}

		#endregion

		#region WD_TotalCubic

		[ReadOnlyMember(nameof(TotalWeightAndVolumnReadOnly))]
		public override ZDecimal WD_TotalCubic
		{
			get { return base.WD_TotalCubic; }
			set { base.WD_TotalCubic = value; }
		}

		#endregion

		#region WD_TotalCubicUnit

		[ReadOnlyMember(nameof(TotalWeightAndVolumnReadOnly))]
		[List("Lookups.CubicUnitTypes")]
		public override ZString WD_TotalCubicUnit
		{
			get { return base.WD_TotalCubicUnit; }
			set
			{
				var oldVolumeUnit = base.WD_TotalCubicUnit;
				base.WD_TotalCubicUnit = value;
				if (oldVolumeUnit != value && ShouldUpdateWeightAndVolumeOnTheFly && !IsImportingData)
				{
					WD_TotalCubic = UnitOfMeasureConverter.GetTotalQuantityFromLines(this, this.GetVolumeMeasure(), Lines.ToProductAndQuantities());
				}
			}
		}

		#endregion

		#region WD_TotalWeight

		[ReadOnlyMember(nameof(TotalWeightAndVolumnReadOnly))]
		public override ZDecimal WD_TotalWeight
		{
			get { return base.WD_TotalWeight; }
			set { base.WD_TotalWeight = value; }
		}

		#endregion

		#region WD_TotalWeightUnit

		[ReadOnlyMember(nameof(TotalWeightAndVolumnReadOnly))]
		[List("Lookups.WeightUnitTypes")]
		public override ZString WD_TotalWeightUnit
		{
			get { return base.WD_TotalWeightUnit; }
			set
			{
				var oldWeightUnit = base.WD_TotalWeightUnit;
				base.WD_TotalWeightUnit = value;
				if (oldWeightUnit != value && ShouldUpdateWeightAndVolumeOnTheFly && !IsImportingData)
				{
					WD_TotalWeight = UnitOfMeasureConverter.GetTotalQuantityFromLines(this, this.GetWeightMeasure(), Lines.ToProductAndQuantities());
				}
			}
		}

		#endregion

		#region WD_UnitsSent

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_UnitsSent
		{
			get { return base.WD_UnitsSent; }
			set { base.WD_UnitsSent = value; }
		}

		#endregion

		#region WD_BOLNoInfo

		public virtual ZPropertyInfo WD_BOLNoInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.WD_BOLNo);
				return info;
			}
		}

		#endregion

		#region WD_WP_ParentPickForTransfer

		[ReadOnly(true)]
		public override ZGuid WD_WP_ParentPickForTransfer
		{
			get { return base.WD_WP_ParentPickForTransfer; }
			set { base.WD_WP_ParentPickForTransfer = value; }
		}

		#endregion

		#region WD_ShipperCODAmount

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_ShipperCODAmount
		{
			get { return base.WD_ShipperCODAmount; }
			set { base.WD_ShipperCODAmount = value; }
		}

		#endregion

		#region WD_WeightSent

		[ActionField(ReadOnly = true)]
		public override ZDecimal WD_WeightSent
		{
			get { return base.WD_WeightSent; }
			set { base.WD_WeightSent = value; }
		}

		#endregion

		#region WD_WeightVolSetFromImport

		[ActionField(ReadOnly = true)]
		public override ZBool WD_WeightVolSetFromImport
		{
			get { return base.WD_WeightVolSetFromImport; }
			set { base.WD_WeightVolSetFromImport = value; }
		}

		#endregion

		#region WD_OrderClassification

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WD_OrderClassification
		{
			get => base.WD_OrderClassification;
			set => base.WD_OrderClassification = value;
		}

		#endregion

		#region ReadOnly

		protected virtual bool ClientReadOnly => HasLinesReadOnly;

		protected virtual bool DocketSubTypeReadOnly
		{
			get { return HasLinesReadOnly; }
		}

		protected virtual bool StandardReadOnly
		{
			get { return IsFinalisedOrCancelled; }
		}

		protected virtual bool NonStandardReadOnly1
		{
			get { return IsFinalisedOrCancelled; }
		}

		protected virtual bool WarehouseReadOnly
		{
			get { return HasLinesReadOnly; }
		}

		protected virtual bool RequiredDateReadOnly
		{
			get { return NonStandardReadOnly1; }
		}

		protected bool HasLinesReadOnly => StandardReadOnly || Lines.Count > 0;

		protected virtual bool CarrierServiceLevelReadOnly => NonStandardReadOnly1;

		protected virtual bool TransportCompanyReadOnly => false;

		protected virtual bool TotalWeightAndVolumnReadOnly => false;

		#endregion

		#endregion

		#region Flags

		public bool IsJobInError => WD_DocketStatus == DocketStatus.Codes.Error;

		public bool IsFinalised => WD_FinalisedDate.IsValid;

		#region IsFinaliseAllowed

		[ResourceStringData("WhsDocket|IsFinaliseAllowed", Caption = "Is Finalize Allowed")]
		public ZBool IsFinaliseAllowed
		{
			get
			{
				var result = true;
				if (CheckIsHeldByCustomsWhenFinalising && IsCustomsTransaction)
				{
					var warehouse = Warehouse;
					var isRealWarehouse = warehouse != null && !warehouse.WW_IsVirtualWarehouse;

					if (isRealWarehouse) // virtual warehouse bonded jobs are auto-finalised on import
					{
						result = !IsDocketHeldByCustoms;
					}
				}
				return result;
			}
		}

		protected virtual bool CanFinalizeDPS
		{
			get { return true; }
		}

		protected virtual bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return false; }
		}

		public ZPropertyInfo IsFinaliseAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.IsFinaliseAllowed); }
		}

		public bool IsDocketHeldByCustoms
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { Events.HoldTheWarehouseOrderCode, Events.WarehouseJobCanNowBeFinalisedCode });
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;

				var lastCustomStatusLog = Logs.Find(query).MaxBySafe(l => l.SL_PostedTimeUtc);
				return lastCustomStatusLog != null && lastCustomStatusLog.SL_SE_NKEvent == Events.HoldTheWarehouseOrderCode;
			}
		}

		#endregion

		public bool IsFinalising => FinaliseDocketSemaphore.IsSuspended;

		#region IsCancelled

		public ZBool IsCancelled
		{
			get { return (WD_DocketStatus == DocketStatus.Codes.Cancelled) || IsCancelEventOnJob; }
		}

		bool IsCancelEventOnJob
		{
			get
			{
				var warehouse = Warehouse;
				return warehouse != null && (warehouse.WW_IsVirtualWarehouse || IsPossibleChangeOfInventoryJob) && Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Any();
			}
		}

		bool ICancellable.IsCancelledHasChanged
		{
			get { return WD_DocketStatusInfo.HasChanges; }
		}

		public ZPropertyInfo IsCancelledInfo
		{
			get { return GetZPropertyInfo(nameof(IsCancelled)); }
		}

		#endregion

		#region IsPossibleChangeOfInventoryJob

		protected virtual bool IsPossibleChangeOfInventoryJob => false;

		#endregion

		public bool IsFinalisedOrCancelled => IsFinalised || IsCancelled || IsSameAsFinalisedOrCancelled;

		protected virtual bool IsSameAsFinalisedOrCancelled => false;

		public virtual ZBool WD_NotInvoiced => true;

		public virtual ZPropertyInfo WD_NotInvoicedInfo
			=> GetZPropertyInfo(Schema.WD_NotInvoiced);

		#region IsAtleastOneInvoiceLinePosted

#if DEBUG
		internal
#endif
		protected bool IsAtleastOneInvoiceLinePosted => IsAtLeastOneAccTransactionLinePosted;

		#region IsAtLeastOneAccTransactionLinePosted

		bool IsAtLeastOneAccTransactionLinePosted
		{
			get
			{
				return IsAccTransactionLinesLoadedByTheFactory
					? IsAtLeastOneAccTransactionLinePosted_WhenLinesAreLoaded
					: IsAtLeastOneAccTransactionLinePosted_WhenLinesAreNotLoaded;
			}
		}

		#region IsAccTransactionLinesLoadedByTheFactory

		bool IsAccTransactionLinesLoadedByTheFactory
		{
			get
			{
				bool result = false;

				var jobHeader = this.JobHeader;
				if (jobHeader != null)
				{
					var query = new ZQuery(AccTransactionLinesSchema.AL_JH, jobHeader.PK);
					query.FetchOnlyFromLocalCache = true;
					result = (Factory.LoadTop1<AccTransactionLines>(query) != null);
				}
				return result;
			}
		}

		#endregion

		#region IsAtLeastOneAccTransactionLinePosted_WhenLinesAreLoaded

		bool IsAtLeastOneAccTransactionLinePosted_WhenLinesAreLoaded
		{
			get
			{
				foreach (AccTransactionLines line in AccTranLines)
				{
					if (line.AL_LineType == TransactionLineTypes.Revenue)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region IsAtLeastOneAccTransactionLinePosted_WhenLinesAreNotLoaded

		bool IsAtLeastOneAccTransactionLinePosted_WhenLinesAreNotLoaded
		{
			get
			{
				if (!IsAtLeastOneAccTransactionLinePosted_Cached.HasValue)
				{
					var jobHeader = this.JobHeader;
					if (jobHeader != null)
					{
						var query = new ZQuery(AccTransactionLinesSchema.AL_JH, jobHeader.PK);
						query.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Revenue);
						query.AddToFilter(AccTransactionLinesSchema.AL_GC, jobHeader.JH_GC);

						IsAtLeastOneAccTransactionLinePosted_Cached = Factory.ExistsInDatabase(AccTransactionLinesSchema.Constants.TableName, query);
					}
					else
					{
						IsAtLeastOneAccTransactionLinePosted_Cached = false;
					}
				}
				return IsAtLeastOneAccTransactionLinePosted_Cached.Value;
			}
		}

		bool? IsAtLeastOneAccTransactionLinePosted_Cached;

		#endregion

		#endregion

		#endregion

		#region CanCreateInventory

		public bool CanCreateInventory
		{
			get { return CanCreateInventoryCore; }
		}

		protected abstract bool CanCreateInventoryCore { get; }

		#endregion

		public virtual bool CanDeleteRelatedData => !IsFinalisedOrCancelled;

		#region IsUniqueExternalReferenceCreatedOnSave

		public bool IsUniqueExternalReferenceCreatedOnSave
		{
			get { return isUniqueExternalReferenceCreatedOnSave ?? IsUniqueExternalReferenceCreatedOnSaveCore; }
			set { isUniqueExternalReferenceCreatedOnSave = value; }
		}

		protected virtual bool IsUniqueExternalReferenceCreatedOnSaveCore
		{
			get { return false; }
		}

		bool? isUniqueExternalReferenceCreatedOnSave;

		#endregion

		#region IsUSBonded

		public bool IsUSBonded
		{
			get { return IsCustomsTransaction && CountryCode == Constants.CountryCodes.UnitedStates; }
		}

		#endregion

		#region IsForChangeOfInventory

		public bool IsImportingForChangeOfInventory
		{
			get;
			private set;
		}

		public IDisposable MarkAsImportingForChangeOfInventory()
		{
			return new DisposableAction(
				() => IsImportingForChangeOfInventory = true,
				() => IsImportingForChangeOfInventory = false);
		}

		#endregion

		#region IsSalesChannelAllowed

		public bool IsSalesChannelAllowed => SalesChannelAllowed;
		protected virtual bool SalesChannelAllowed => false;

		#endregion

		protected bool IsNotDPSScreened => !IsInDatabase || WD_ScreeningStatus == ScreeningStatusesList.Codes.NotScreened;

		#region IsCreatedFromPickByBOM

		public bool IsCreatedFromPickByBOM => IsCreatedFromPickByBOMCore;

		protected virtual bool IsCreatedFromPickByBOMCore => false;

		#endregion

		#endregion

		#region DGContact

		public virtual ZString DGContact
		{
			get
			{
				ZString result = "";
				if (Client != null)
				{
					OrgContact contact = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, Client.MiscServ.OM_OC_EXDefaultDGContact));
					if (contact != null)
					{
						result = contact.OC_ContactName;
					}
				}
				return result;
			}
		}

		public virtual ZPropertyInfo DGContactInfo
		{
			get { return GetZPropertyInfo(nameof(DGContact)); }
		}

		#endregion

		#region IsDomesticFreight

		public virtual bool IsDomesticFreight
		{
			get { return true; }
		}

		#endregion

		#region Post Finalize Edit

		public bool IsPostFinalizeEditAllowed
		{
			get { return IsPostFinalizeEditAllowedCore; }
		}

		protected virtual bool IsPostFinalizeEditAllowedCore
		{
			get { return false; }
		}

		#endregion

		#region Finalisation

		public void FinaliseDocket()
		{
			ValidateAndFinaliseDocket(true);
		}

		public void FinaliseDocketWithoutUserConfirmation()
		{
			ValidateAndFinaliseDocket(false);
		}

		void ValidateAndFinaliseDocket(bool withUserConfirmations)
		{
			try
			{
				isInProgressOfFinalising = true;

				if (PreventFinaliseWithoutReload)
				{
					NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.CannotFinaliseWithoutReload));
				}
				else if (!IsFinalised)
				{
					using (new SemaphoreManager(FinaliseDocketSemaphore))
					{
						ValidateAndFinaliseDocketCore(withUserConfirmations);
					}
				}
				else
				{
					NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.JobIsFinalised));
				}

				isInProgressOfFinalising = false;
			}
			catch (AbortFinalizationException ex)
			{
				NotificationSubscriber.Notify(new Notification(NotificationType.Error, ex.Message));
			}
		}

		// NOTE: This is used to prevent saving if a docket has been partially finalised, but an exception thrown.
		// Without this, the form will catch the exception and the user may be able to save a docket in a partially finalised state.
		// A real "Semaphore" cannot be used as it will be disposed when an exception is thrown.
		bool isInProgressOfFinalising;

		void ValidateAndFinaliseDocketCore(bool withUserConfirmations)
		{
			var finaliseOk = RunPreFinaliseValidation();
			if (finaliseOk)
			{
				using (GetFinalisationDataCache())
				{
					var finaliseWithActiveTasks = CanFinaliseWithActiveWorkingTasks(withUserConfirmations);
					if (finaliseWithActiveTasks && (!withUserConfirmations || FinaliseConfirmationNotification(FinaliseConfirmationMessage)))
					{
						finaliseOk = FinaliseDocketCore();
						if (finaliseOk && !HasErrors) // Successful finalisation
						{
							((IDocketInternals)this).FlagDocketAsFinalised();
							OnFinaliseSucceeded();
						}
						else
						{
							OnFinaliseFailed();
						}
					}
				}
			}

			if (!finaliseOk)
			{
				NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.FinaliseZErrorMessageBox));
			}
		}

		protected virtual bool CanFinaliseWithActiveWorkingTasks(bool withUserConfirmations) => true;

		protected virtual IDisposable GetFinalisationDataCache() => new DisposableObject();

		/// <summary>
		/// Override for Docket Type Specific Finalization
		/// All PreFinaliseValidation is done before this method is called. 
		/// This method will not be called if PreFinaliseValidation failed.
		/// Do not put any validation or notifications into FinaliseDocketCore, put it in RunPreFinaliseValidationCore
		/// </summary>
		protected abstract bool FinaliseDocketCore();

		protected virtual void OnFinaliseSucceeded()
		{
		}

		protected virtual void OnFinaliseFailed()
		{
		}

		/// <summary>
		/// Run validations to ensure docket is in a finalisable state
		/// </summary>
		/// <returns>
		/// true = Successful validation
		/// false = Validation failure
		/// </returns>
		bool RunPreFinaliseValidation()
		{
			var validationFailure = true;
			if (IsFinaliseAllowed)
			{
				var dpsError = Res.GetString("2B33C6C9-6D14-4F7F-AC1B-B79066ACCF2D", "Cannot finalize when Denied Party Screening is matched.");
				ClearRowNotificationsContaining(dpsError);

				if (CanFinalizeDPS)
				{
					AddFetchHintsForPreFinaliseValidation();

					MarkAsNeedingValidationIncludingChildren();
					RunPreSaveValidationWithFetchHints();
					validationFailure = HasErrors;
					if (!validationFailure)
					{
						if (Lines.Count <= 0)
						{
							NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.NoLinesEntered));
							validationFailure = true;
						}
						else
						{
							validationFailure = !RunPreFinaliseValidationCore();
						}
					}
				}
				else
				{
					AddRowError(dpsError);
				}
			}
			else
			{
				AddRowError(Res.GetString("421d38dc-bbe1-49dd-95ef-b7129cd6dc1e", "Cannot be finalized until Customs send an ACCEPT event."));
			}

			return !validationFailure;
		}

		void AddFetchHintsForPreFinaliseValidation()
		{
			AddFetchHintsForWhsInventoryView();
			AddFetchHintsForJobDocAddress();

			AddFetchHintsForPreFinaliseValidation_Core();
		}

		protected virtual void AddFetchHintsForPreFinaliseValidation_Core()
		{
		}

		protected abstract bool AddFetchHintsForInventory { get; }

		protected override void RunPreSaveValidationCore()
		{
			AddFetchHintsForWhsInventoryView();
			AddFetchHintsForJobDocAddress();
			Lines.ForEach(l => Factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, l.WE_OP));

			base.RunPreSaveValidationCore();

			SynchroniseOffsetsOnDocketAndLinesToWarehouseTime();
		}

		void SynchroniseOffsetsOnDocketAndLinesToWarehouseTime()
		{
			if (WD_WW_Whs.IsValid)
			{
				var warehouse = Warehouse;
				if (warehouse != null)
				{
					var warehouseTimeZoneSet = warehouse.RelatedCompanyBranch.HomePort?.TimeZoneSet;
					var calculationTimeZone = warehouseTimeZoneSet?.GetCalculationTimeZone();

					SynchroniseOffsetsToWarehouseTime(calculationTimeZone);

					foreach (var line in Lines.Where(l => !l.IsInDatabase || l.HasChanges))
					{
						line.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);
					}
				}
			}
		}

		void SynchroniseOffsetsToWarehouseTime(ITimeZone calculationTimeZone)
		{
			SynchroniseOffsetsToWarehouseTimeCore(calculationTimeZone);
		}

		protected virtual void SynchroniseOffsetsToWarehouseTimeCore(ITimeZone calculationTimeZone)
		{
			WD_BookingDateInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
		}

		void AddFetchHintsForJobDocAddress()
		{
			if (!jobDocAddressFetchHintsAdded)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, PK);
				Lines.ForEach(l => Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, l.PK));
				jobDocAddressFetchHintsAdded = true;
			}
		}

		bool jobDocAddressFetchHintsAdded;

		void AddFetchHintsForWhsInventoryView()
		{
			if (!isFetchHintsForWhsInventoryViewAdded && AddFetchHintsForInventory)
			{
				Lines.ForEach(l =>
				{
					Factory.AddFetchHint(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WE_InDocketLine, l.PK);
				});
				isFetchHintsForWhsInventoryViewAdded = true;
			}
		}

		bool isFetchHintsForWhsInventoryViewAdded;

		/// <summary>
		/// Override for Docket Type Specific Validation
		/// </summary>
		/// <returns>
		/// true = Successful validation
		/// false = Validation failure
		/// </returns>
		protected virtual bool RunPreFinaliseValidationCore()
		{
			return true;
		}

		protected virtual bool FinaliseConfirmationNotification(ZString finaliseConfirmationMessage)
		{
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: FinaliseConfirmationDialogIdentifier,
													caption: Res.GetString("c43298d0-0419-40fc-93cf-90b9b4830686", "Finalization Confirmation"),
													buttons: ZMessageBoxButtons.YesNoCancel,
													icon: ZMessageBoxIcon.Warning,
													context: null,
													resultsNotToSave: new[] { ZDialogResult.No, ZDialogResult.Cancel },
													showCheckboxOnly: true);

			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, finaliseConfirmationMessage, false);
			NotificationSubscriber.QueryUser(e);
			return e.Response;
		}

		protected virtual ZGuid FinaliseConfirmationDialogIdentifier
		{
			get { throw GetExceptionIfFinaliseConfirmationDetailsNotProvided(nameof(FinaliseConfirmationDialogIdentifier)); }
		}

		protected virtual string FinaliseConfirmationMessage
		{
			get { throw GetExceptionIfFinaliseConfirmationDetailsNotProvided(nameof(FinaliseConfirmationMessage)); }
		}

		NotSupportedException GetExceptionIfFinaliseConfirmationDetailsNotProvided(string methodName)
		{
			return new NotSupportedException($"Override to implement confirmation message details (see WhsReceive, WhsTransfer or WhsAdjustment). If no confirmation prompt required, override {methodName} to do nothing and return true (see WhsOrder).");
		}

		#region Finalise Docket Semaphore

#if DEBUG
		public
#endif
		Semaphore FinaliseDocketSemaphore
		{
			get { return finaliseDocketSemaphore ?? (finaliseDocketSemaphore = new Semaphore()); }
		}

		Semaphore finaliseDocketSemaphore;

		#endregion

		#endregion

		#region GetProductParams

		public WhsProductParamsByWhsAndClient GetProductParams(WhsProduct product) => product?.GetParamsByWhsAndClient(WD_WW_Whs, WD_OH_Client);

		#endregion

		#region Notifications

		public NotificationManager NotificationManager
		{
			get
			{
				if (notificationManager == null)
				{
					notificationManager = new NotificationManager();
					notificationManager.Push(new NotificationBuffer());
				}
				return notificationManager;
			}
		}

		public INotifications NotificationSubscriber
		{
			get { return NotificationManager.Peek; }
		}

		NotificationManager notificationManager;

		#endregion

		#region Cancel Docket

		public event EventHandler<TextEventArgs> CancelFailed;

		public void CancelReactivateDocket()
		{
			WD_DocketStatus = IsCancelled ? DocketStatus.Codes.Entered : DocketStatus.Codes.Cancelled;
		}

		void CancelSucceeded()
		{
			WD_CanceledTimeUtc = ZDateTime.UtcNow;
			WD_GS_NKCanceledBy = GlbStaff.CurrentUser.GS_Code;
			WD_TaskPlanningStatus = string.Empty;
			LinesToPropagateStatusAndFinDateChange.ForEach(l => l.CancelLine());
		}

		void ReactivateSucceeded()
		{
			WD_CanceledTimeUtc = ZDateTime.Empty;
			WD_GS_NKCanceledBy = ZString.Empty;
			LinesToPropagateStatusAndFinDateChange.ForEach(l => l.ReactivateLine());
		}

		#endregion

		#region Validation

		#region WhsLocationCapacityValidationManager

		internal WhsLocationCapacityValidationManager<WhsDocket, WhsDocketLine> LocationCapacityValidationManager
		{
			get { return locationCapacityValidationManager ?? (locationCapacityValidationManager = new WhsLocationCapacityValidationManager<WhsDocket, WhsDocketLine>(this)); }
		}
		WhsLocationCapacityValidationManager<WhsDocket, WhsDocketLine> locationCapacityValidationManager;

		#endregion

		public void ValidateGoodsBillToPK()
		{
			if (!IsValidationSuspended && !GoodsBillToDocAddress.IsValidationSuspended)
			{
				GoodsBillToDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		public void ValidatePickUpPK()
		{
			if (!IsValidationSuspended && !PickUpDocAddress.IsValidationSuspended)
			{
				PickUpDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		public void ValidateDropOffPK()
		{
			if (!IsValidationSuspended && !DropOffDocAddress.IsValidationSuspended)
			{
				DropOffDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		#region LocationProductPalletValidationCache

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableList(new[]
			{
				base.GetValidationDataSuspender(),
				new DisposableAction(
					() => productPalletValidationCache = new LocationProductPalletValidationCache(Factory, Lines.ToArray(), PK),
					() => productPalletValidationCache = null)
		});
		}

		internal LocationProductPalletValidationCache GetProductPalletValidationCache() => productPalletValidationCache;

		LocationProductPalletValidationCache productPalletValidationCache;

		#endregion

		#endregion

		#region UpdatingTotalWeightandVolume

		public void UpdateTotalWeightAndVolume(OrgSupplierPart part, ZDecimal delta)
		{
			if (part != null && delta != 0m && ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var lineWithProductAndQty = new LineWithProductAndQuantity(part.PK, delta);
				WD_TotalWeight += UnitOfMeasureConverter.GetQuantityFromLine(this, this.GetWeightMeasure(), lineWithProductAndQty);
				WD_TotalCubic += UnitOfMeasureConverter.GetQuantityFromLine(this, this.GetVolumeMeasure(), lineWithProductAndQty);

				if (WD_TotalCubic < 0m)
				{
					WD_TotalCubic = 0m; // safeguard
				}

				if (WD_TotalWeight < 0m)
				{
					WD_TotalWeight = 0m; // safeguard
				}
			}
		}

		public bool ShouldUpdateWeightAndVolumeOnTheFly
		{
			get { return !UpdatingWeightAndVolumeSemaphore.IsSuspended && ShouldUpdateWeightAndVolumeOnTheFlyCore; }
		}

		protected virtual bool ShouldUpdateWeightAndVolumeOnTheFlyCore
		{
			get { return !IsDeleted && !WD_WeightVolSetFromImport; }
		}

		#region UpdatingWeightAndVolumeSemaphore;

		public Semaphore UpdatingWeightAndVolumeSemaphore
		{
			get { return updatingWeightAndVolumeSemaphore ?? (updatingWeightAndVolumeSemaphore = new Semaphore()); }
		}

		Semaphore updatingWeightAndVolumeSemaphore;

		#endregion

		#region RefreshTotalUnitsFromLinesSemaphore;

		public Semaphore RefreshTotalUnitsFromLinesSemaphore
		{
			get { return refreshTotalUnitsFromLinesSemaphore ?? (refreshTotalUnitsFromLinesSemaphore = new Semaphore()); }
		}

		Semaphore refreshTotalUnitsFromLinesSemaphore;

		#endregion

		#endregion

		#region TaskPlanningStatusPrompt

		public ZString TaskPlanningStatusPrompt
		{
			get
			{
				var result = string.Empty;

				if (SupportsDocketPlanningStatus)
				{
					if (WD_TaskPlanningStatus == TaskPlanningStatus.Codes.Ready)
					{
						result = Res.GetString("cb7a6925-db8e-4968-82ac-64c8db382bb4", "This {0} is read only because it is ready for planning.", Description);
					}					
					else if (WD_TaskPlanningStatus == TaskPlanningStatus.Codes.Planned)
					{
						result = Res.GetString("dde5a0d7-5c66-4854-8f26-14efca53d875", "This {0} is read only because it is planned.", Description);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo TaskPlanningStatusPromptInfo => GetZPropertyInfo(nameof(TaskPlanningStatusPrompt));

		#endregion

		#region SupportsDocketPlanningStatus

		protected virtual bool SupportsDocketPlanningStatus => false;

		#endregion

		#region IsReadyForPlanningOrPlanned

		public bool IsReadyForPlanningOrPlanned => WD_TaskPlanningStatus == TaskPlanningStatus.Codes.Ready || WD_TaskPlanningStatus == TaskPlanningStatus.Codes.Planned;

		#endregion

		// interfaces

		#region ICreditControlledDocumentDelivery

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback
		{
			get;
			set;
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (GetDocumentLogin != null)
			{
				GetDocumentLogin(this, e);
			}
		}

		public event EventHandler<SecurityLoginEventArgs> GetDocumentLogin;

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("f95f5174-66c0-44a3-9622-9888830395de", "Warehouse Client"); }
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted => IsDPSMovementRestrictedCore();

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		protected virtual bool IsDPSMovementRestrictedCore() => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties() => GetScreeningPartiesCore();

		protected virtual ScreeningParty[] GetScreeningPartiesCore() => Array.Empty<ScreeningParty>();

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var orgs = new List<OrgHeader>();
				if (RequiresCreditCheck)
				{
					if (Client != null && this.IsCreditLimitCheckRequired(OrgCodes.Consignor))
					{
						orgs.Add(Client);
					}
				}

				return orgs.ToArray();
			}
		}

		string[] IRelatedJobNumber.JobNumber => Array.Empty<string>();

		protected virtual bool RequiresCreditCheck
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return WD_DocketID; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			if (job != null && this is IDtbBookingParent)
			{
				// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromChildPortTransportJob() and TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildPortTransportJob()
				CartageHelper.AttachCartageJobsToParentJob(job, PK);
				// tested by IDtbBookingParentTestCase<T>.TestIfJobInvoicingPluginAndCanCreateInvoicingJobThenAddsChargesFromGrandchildLandTransportConsignment()
				ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return InvoicingSupporter; }
		}

		public WhsDocketInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		protected virtual WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsDocketInvoicingSupporter(this);
		}

		internal void PostedStateChanged()
		{
			accTranLines = null;
			RefreshBindingIncludingChildren();
		}

		WhsDocketInvoicingSupporter invoicingSupporter;

		#endregion

		#region IJobInvoicingAdditionalData Members

		CustomPropertyContainer<JobCharge> IJobInvoicingAdditionalData.GetAdditionalProperties()
		{
			return GetAdditionalProperties();
		}

		protected virtual CustomPropertyContainer<JobCharge> GetAdditionalProperties()
		{
			return new WhsJobInvoicingAdditionalDataPropertyProvider().GetAdditionalProperties();
		}

		#endregion

		#region IDocAddresses

		protected void CopyJobDocAddressesFrom(WhsDocket source)
		{
			DocAddresses.RemoveAndDeleteAll();
			foreach (JobDocAddress address in source.DocAddresses)
			{
				JobDocAddress newAddress = (JobDocAddress)address.Clone();
				DocAddresses.Add(newAddress);
			}
		}

		protected virtual WhsDocketDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new WhsDocketDocAddressValidation(addressToValidate);
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return PiggyBackedDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return SupportedAddressTypesCore(); }
		}

		protected virtual DocAddressType[] SupportedAddressTypesCore()
		{
			return new DocAddressType[]
			{
				DocAddressType.GoodsBillToAddress,
				DocAddressType.PickUpAddress,
				DocAddressType.DropOffAddress,
				DocAddressType.SupplierDocumentaryAddress
			};
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirementCore(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.GoodsBillToAddress:
					return GoodsBillToDocAddressRequirement;
				case DocAddressType.PickUpAddress:
					return PickUpDocAddressRequirement;
				case DocAddressType.DropOffAddress:
					return DropOffDocAddressRequirement;
				case DocAddressType.SupplierDocumentaryAddress:
					return SupplierDocAddressRequirement;
				default:
					return null;
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.ConsigneeAddress:
					ConsigneeOrgAddressBeforeChangeCore(docAddress);
					break;
			}
		}

		protected virtual void ConsigneeOrgAddressBeforeChangeCore(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideAddressCheckpoint(docAddress);
		}

		protected virtual SecurityCheckpoint GetCanOverrideAddressCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		#region DocAddresses

		[ChildEditable(true)]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region CanDeleteAddress

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public abstract DocManagerInfo DocManagerInfo
		{
			get;
		}

		#endregion

		#region IDocketInternals Members

		void IDocketInternals.FlagDocketAsFinalised()
		{
			FlagDocketAsFinalisedCore();
			CreateEventLog(Events.ItemDocumentJobFinalised);
		}

		protected virtual void FlagDocketAsFinalisedCore()
		{
			// Setting finalised date first to allow propagation of docket status to docket lines
			WD_FinalisedDate = GetFinalisedDate();
			WD_DocketStatus = DocketStatus.Codes.Finalised;
		}

		protected virtual ZDateTimeOffset GetFinalisedDate() => Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);

		#endregion

		#region IAllowUserToCancel Members

		public static string CantCancelReasonMsg
		{
			get { return Res.GetString("1059287f-597f-4566-abdb-18a159aa538b", "You can only cancel dockets with {0} status", DocketStatus.Descriptions.Entered); }
		}
		public static string CantReactivateReasonMsg
		{
			get { return Res.GetString("e1d1aeee-6368-48d4-9edd-c3d3acf3062f", "Only canceled dockets can be re-activated."); }
		}

		public string CanCancel()
		{
			return CanCancelCore();
		}

		protected virtual string CanCancelCore()
		{
			var result = CanCancel(WD_DocketStatus) ? string.Empty : CantCancelReasonMsg;

			if (result == ZString.Empty && WD_DocketStatus != DocketStatus.Codes.Cancelled)
			{
				result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
			}

			return result;
		}

		public static bool CanCancel(ZString status)
		{
			var canCancelStatus = new ZString[]
			{
				DocketStatus.Codes.New, DocketStatus.Codes.Entered, DocketStatus.Codes.Cancelled, DocketStatus.Codes.Error, DocketStatus.Codes.Held
			};

			return canCancelStatus.Contains(status);
		}

		public string CanReactivate()
		{
			string result;
			if (WD_DocketStatus == DocketStatus.Codes.Cancelled)
			{
				result = null;
			}
			else
			{
				result = CantReactivateReasonMsg;
			}
			return result;
		}

		bool ICancellable.IsCancelled
		{
			get
			{
				return this.IsCancelled;
			}
			set
			{
				if (value)
				{
					this.CancelReactivateDocket();
				}
				else
				{
					if (this.IsCancelled)
					{
						WD_DocketStatus = DocketStatus.Codes.Entered;
					}
				}
			}
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return CanDeleteCore; }
		}

		bool CanDeleteCore
		{
			get { return WD_DocketStatus == DocketStatus.Codes.New || WD_DocketStatus == DocketStatus.Codes.Entered || WD_DocketStatus == DocketStatus.Codes.Held; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return CanDelete ? null : CantDeleteReasonMsg; }
		}

		public static MultilingualString CantDeleteReasonMsg
		{
			get
			{
				return ResString.GetMultilingualString("fb6e128b-4b33-4745-938f-54ab4de71722", "You can only delete Dockets that are {0}, {1} or {2}.",
					DocketStatus.Descriptions.New, DocketStatus.Descriptions.Entered, DocketStatus.Descriptions.Held);
			}
		}

		#endregion

		#region IWorkflowProvider Members

		protected abstract ZString GetWorkflowType();

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return GetWorkflowType(); }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewProcessTasksCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		protected abstract WhsDocketProcessTasksCollection GetNewProcessTasksCollection();

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, WD_OH_Client, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_WW, WD_WW_Whs, ZGuid.Empty);

			GetTemplateSelectionCriteriaCore(result);

			return result;
		}

		protected virtual void GetTemplateSelectionCriteriaCore(ColumnValueRanker result)
		{
		}

		ZGuid IWorkflowProviderTemplateCriteria.CompanyPK
			=> Warehouse?.RelatedCompanyBranch?.GB_GC ?? ZGuid.Empty;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		#endregion

		#region IWorkflowTriggerFieldChangeSource

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				List<IWorkflowProvider> result = new List<IWorkflowProvider>();
				result.Add(this);
				return result.ToArray();
			}
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg => CustomFieldsSupported ? Client : null;

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				if (CustomFieldsSupported)
				{
					WD_OH_ClientInfo.ValueChanged += value;
				}
			}

			remove
			{
				WD_OH_ClientInfo.ValueChanged -= value;
			}
		}

		bool CustomFieldsSupported => !IsDeleted && CustomFieldsSupportedCore;
		protected virtual bool CustomFieldsSupportedCore => true;

		#endregion

		#region IPropertyChecker Members

		bool IPropertyChecker.IsPropertyUpdatableViaXueAdditionalFields(PropertyInfo propertyInfo, object proposedValue, out string errorMessage)
		{
			errorMessage = null;
			var result = true;

			if (propertyInfo.Name == nameof(WD_DocketStatus))
			{
				errorMessage = Res.GetString("264918c1-5f32-4073-8bb2-5994f16cbca0", "Docket Status is read-only field, it can not be set.");
				result = false;
			}
			else
			{
				result = IsPropertyUpdatableViaXueAdditionalFieldsCore(propertyInfo, proposedValue, out errorMessage);
			}

			return result;
		}

		protected virtual bool IsPropertyUpdatableViaXueAdditionalFieldsCore(PropertyInfo propertyInfo, object proposedValue, out string errorMessage)
		{
			errorMessage = null;
			return true;
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
				CustomLabelInfoList result;
				if (configOrg != null)
				{
					var key = string.Format(Culture.Invariant, "WhsDocket.CustomLabelsProvider:{0}", configOrg.PK.ToStringKey()); // Key used for Factory Cache
					result = configOrg.Factory.GetCachedValue(key, () => GetCustomFieldsNoCache(configOrg, factory));
				}
				else
				{
					result = GetCustomFieldsNoCache(configOrg, factory);
				}

				return result;
			}

			CustomLabelInfoList GetCustomFieldsNoCache(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(WhsDocket), configOrg, ResString.GetMultilingualString("6119f9ff-4363-4818-b9a5-e6b86d0314d4", "the client"), factory);
				result.Capacity = 17;

				result.Add(Constants.CustomLabels.WhsDocket.CustomAttribute1, WhsDocketSchema.WD_CustomAttrib1.Name, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.WhsDocket.CustomAttribute2, WhsDocketSchema.WD_CustomAttrib2.Name, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.WhsDocket.CustomAttribute3, WhsDocketSchema.WD_CustomAttrib3.Name, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.WhsDocket.CustomAttribute4, WhsDocketSchema.WD_CustomAttrib4.Name, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.WhsDocket.CustomAttribute5, WhsDocketSchema.WD_CustomAttrib5.Name, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDate1, WhsDocketSchema.WD_CustomDate1.Name, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDate2, WhsDocketSchema.WD_CustomDate2.Name, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDecimal1, WhsDocketSchema.WD_CustomDecimal1.Name, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDecimal2, WhsDocketSchema.WD_CustomDecimal2.Name, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDecimal3, WhsDocketSchema.WD_CustomDecimal3.Name, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDecimal4, WhsDocketSchema.WD_CustomDecimal4.Name, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.WhsDocket.CustomDecimal5, WhsDocketSchema.WD_CustomDecimal5.Name, Constants.CustomLabels.Descriptions.CustomNumber(5));
				result.Add(Constants.CustomLabels.WhsDocket.CustomFlag1, WhsDocketSchema.WD_CustomFlag1.Name, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.WhsDocket.CustomFlag2, WhsDocketSchema.WD_CustomFlag2.Name, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.WhsDocket.CustomFlag3, WhsDocketSchema.WD_CustomFlag3.Name, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.WhsDocket.CustomFlag4, WhsDocketSchema.WD_CustomFlag4.Name, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.WhsDocket.CustomFlag5, WhsDocketSchema.WD_CustomFlag5.Name, Constants.CustomLabels.Descriptions.CustomFlag(5));

				return result;
			}
		}

		#endregion

		#region IHaveServices Members

		[ChildEditable(true)]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new WhsJobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
				}
				return services;
			}
		}

		ZString IHaveServices.ContainerMode
		{
			get { return ""; }
		}

		IHaveServices[] IHaveServices.DependentServiceParents => Array.Empty<IHaveServices>();

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		ZString IHaveServices.TableCode
		{
			get { return WhsDocketSchema.Constants.Prefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return ""; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return false; }
		}

		WhsJobServiceDependentCollection services;

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Warehouse?.RelatedCompanyBranch;

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobDescription
		{
			get { return WD_ExternalReference; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return WD_DocketID; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return WD_DocketStatus; }
		}

		#endregion

		#region ITaskPlanningJob

		protected virtual ZString HumanReadableNameWithoutID => null;

		ZGuid ITaskPlanningJob.PK => PK;

		ZGuid ITaskPlanningJob.WarehousePK => WD_WW_Whs;

		ZString ITaskPlanningJob.TaskPlanningStatus
		{
			get => WD_TaskPlanningStatus;
			set => WD_TaskPlanningStatus = value;
		}

		string ITaskPlanningJob.JobID => WD_DocketID;

		ZString ITaskPlanningJob.HumanReadableNameWithoutID => HumanReadableNameWithoutID;

		string ITaskPlanningJob.SpecialCannotUpdateTaskPlanningStatusReason => SpecialCannotUpdateTaskPlanningStatusReasonCore();

		protected virtual string SpecialCannotUpdateTaskPlanningStatusReasonCore() => Res.GetString("e21f8401-def9-45ec-8db7-823ecf31f9d7", "The job does not support Task Planning Status.");

		void ITaskPlanningJob.ClearFKForProcessTasks(ISet<ZGuid> processTaskPKs) => ClearFKForProcessTasksCore(processTaskPKs);

		protected virtual void ClearFKForProcessTasksCore(ISet<ZGuid> processTaskPKs) { }

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return JobControllerId; }
		}

		protected abstract ControllerID JobControllerId { get; }

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region Properties for Subclasses' ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		protected virtual AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Client);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Description + " - " + WD_DocketID; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return GetTemplateCategory(); }
		}

		protected virtual string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.None;
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return null; }
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			AddOrganisationCustomFields(properties);
			return new CustomBusinessObject(Factory, this, properties);
		}

		void AddOrganisationCustomFields(UserDefinedPropertyCollection properties)
		{
			var customLabelsProvider = new CustomLabelsProvider(this);
			var list = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, Factory);
			foreach (CustomLabelInfo field in list)
			{
				if (field.IsEnabled)
				{
					properties.Add(field.PropertyName, field.Caption);
				}
			}
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return IsImportingData; }
			set { IsImportingData = value; }
		}

		public bool IsImportingData
		{
			set;
			get;
		}

		#endregion

		#region IWhsLogEventParent Members

		ZString IWhsLogEventParent.EventFreeTextReference
		{
			get { return WD_DocketID; }
		}

		string IWhsLogEventParent.EventReferenceParameterType
		{
			get { return GetDocketEventReferenceParameterType; }
		}
		protected abstract string GetDocketEventReferenceParameterType { get; }

		#endregion

		#region ILocationCapacityMaster Members

		ZGuid ILocationCapacityMaster<WhsDocketLine>.PKToExclude => PK;

		IEnumerable<WhsDocketLine> ILocationCapacityMaster<WhsDocketLine>.GetValidationLines(WhsLocation location)
		{
			return Lines.Where(l => l.WE_WL_TransferFrom == location.PK || l.WE_WL == location.PK);
		}

		ZBool ILocationCapacityMaster<WhsDocketLine>.ShowRFMessage => true;

		ZString ILocationCapacityMaster<WhsDocketLine>.GetLocationQuantityExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("b5a300c7-f7c3-499c-9505-e86063b14f5b",
											"Total required Quantity ({0}) exceeds the maximum available Quantity ({1}) for this location.",
											requiredValue,
											availableValue);
		}

		ZString ILocationCapacityMaster<WhsDocketLine>.GetLocationWeightExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("8afe272b-7fe4-4378-8df6-8840dd8ab999",
											"Total required Weight ({0} {2}) exceeds the maximum available Weight ({1} {2}) for this location.",
											requiredValue.ToString(2), // 2 Decimal Places
											availableValue.ToString(2), // 2 Decimal Places
											location.WLV_MaxWeightUnit);
		}

		ZString ILocationCapacityMaster<WhsDocketLine>.GetLocationVolumeExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location)
		{
			return Res.GetString("498c4326-4ee7-45d7-952e-aa92088242f4",
											"Total required Volume ({0} {2}) exceeds the maximum available Volume ({1} {2}) for this location.",
											requiredValue.ToString(3), // 3 Decimal Places
											availableValue.ToString(3), // 3 Decimal Places
											location.WLV_MaxCubicUnit);
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DocketIDFountainUniqueIndexFailureHandler(this)); }
		}

		class DocketIDFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DocketIDFountainUniqueIndexFailureHandler(WhsDocket docket)
				: base(WhsDocketSchema.Constants.Indexes.NR_UX__WD_DocketID, docket)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.WarehouseDocketID; }
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();

			if (WD_DocketType != DocketType.Codes.Order)
			{
				CriticalChangesVersionIDCache = WD_CriticalChangesVersionID;
				if (IsFinalised && WD_FinalisedDateInfo.HasChanges)
				{
					FinalisedDateCache = WD_FinalisedDate;
				}
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			PreventFinaliseWithoutReload = CriticalChangesVersionIDCache.HasValue && CriticalChangesVersionIDCache.Value != WD_CriticalChangesVersionID;
			if (FinalisedDateCache.HasValue && !IsFinalised)
			{
				PreventSaveWithoutReload = true;
				WD_FinalisedDate = FinalisedDateCache.Value;
			}

			FinalisedDateCache = null;
			CriticalChangesVersionIDCache = null;
		}

		bool PreventFinaliseWithoutReload;
		bool PreventSaveWithoutReload;
		ZGuid? CriticalChangesVersionIDCache;
		ZDateTimeOffset? FinalisedDateCache;

		#endregion

		#region Implementation

		protected void CancelEvents(Event eventToClear)
		{
			var logsToCancel = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventToClear.Code));
			foreach (var log in logsToCancel)
			{
				log.Cancel();
			}
		}

		protected ZString GetReference(ZString referenceType)
		{
			ZString result = ZString.Empty;
			foreach (WhsDocketReference reference in References)
			{
				if (reference.WX_RefType == referenceType)
				{
					result = reference.WX_Reference;
					break;
				}
			}
			return result;
		}

		protected void SetReference(ZString referenceType, ZString value)
		{
			WhsDocketReference foundReference = null;
			foreach (WhsDocketReference reference in References)
			{
				if (reference.WX_RefType == referenceType)
				{
					foundReference = reference;
					break;
				}
			}

			if (foundReference != null)
			{
				if (value.IsEmpty)
				{
					foundReference.Delete();
				}
				else
				{
					foundReference.WX_Reference = value;
				}
			}
			else
			{
				if (!value.IsEmpty)
				{
					WhsDocketReference reference = References.AddNew();
					reference.WX_RefType = referenceType;
					reference.WX_Reference = value;
				}
			}
		}

		ZBool refreshProxyPropertiesWasRun;
		public ZBool RefreshProxyPropertiesWasRun
		{
			get { return refreshProxyPropertiesWasRun; }
			set { refreshProxyPropertiesWasRun = value; }
		}

		public virtual void RefreshProxyProperties()
		{
			RefreshProxyPropertiesWasRun = true;
			WD_BOLNoInfo.RefreshBinding();
		}

		protected ZBool HasErrorsIncludingLines
		{
			get
			{
				bool hasErrors = (Notifications.GetErrors().Count() != 0 || Notifications.GetMessageErrors().Count() != 0);

				if (!hasErrors)
				{
					foreach (WhsDocketLine line in Lines)
					{
						if (line.Notifications.GetErrors().Count() != 0 || line.Notifications.GetMessageErrors().Count() != 0)
						{
							hasErrors = true;
							break;
						}
					}
				}
				return hasErrors;
			}
		}

		protected virtual void CreateOperationalStatusChangeEventsCore()
		{
		}

		#region Logs

		protected void CreateEventLog(Event eventType, ZDateTimeOffset? dateTime = null)
		{
			CreateEventLog(eventType, ZString.Empty, dateTime);
		}

		protected void CreateEventLog(Event eventType, ZString reference, ZDateTimeOffset? dateTime = null)
		{
			Logs.AddNew(eventType, reference, dateTime ?? ZDateTimeOffset.Now);
		}

		protected bool EventLogExists(ZString code)
		{
			return DocketRelatedEntityOperationsStrategy.EventLogExists(code);
		}

		#endregion

		public static BusinessObject[] GetDockets(BusinessObjectFactory factory, Type objType, ZGuid client, ZGuid whs, ZString docketType, ZString reference)
		{
			ZQuery filter = new ZQuery(WhsDocketSchema.WD_OH_Client, SQLComparisonOperator.Equal, client);
			filter.AddToFilter(WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, whs);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, SQLComparisonOperator.Equal, docketType);
			filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.Equal, reference);
			return factory.Load(objType, filter);
		}

		WhsDocketLineCollection lines;
		WhsDocketPalletCollection hirePallets;
		WhsDocketContainerCollection containers;
		WhsDocketReferenceCollection references;

		ICartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = ObjectFactory.Get<ICartageHelper>()); }
		}
		ICartageHelper cartageHelper;

		IConsignmentJobHelper ConsignmentJobHelper
		{
			get { return consignmentJobHelper ?? (consignmentJobHelper = ObjectFactory.Get<IConsignmentJobHelper>()); }
		}
		IConsignmentJobHelper consignmentJobHelper;

		#endregion
	}

	#region IDocketInternals

	internal interface IDocketInternals
	{
		void FlagDocketAsFinalised();
	}

	#endregion

	#region MaxLineNoManager

	public class MaxLineNoManager
	{
		public MaxLineNoManager(WhsDocket docket)
		{
			Argument.NotNull(docket, "Docket");

			this.docket = docket;
			maxLineNo = 0;
		}

		#region CurrentMaxLineNo

		public ZShort CurrentMaxLineNo
		{
			get { return docket.DocketRelatedEntityOperationsStrategy.GetCurrentMaxLineNo(maxLineNo); }
		}

		#endregion

		#region LineNoDeleted

		public void LineNoDeleted(ZShort lineNo)
		{
			if (lineNo == CurrentMaxLineNo)
			{
				maxLineNo = 0;
			}
		}

		#endregion

		#region LineNoSet

		public void LineNoSet(ZShort lineNo)
		{
			if (lineNo > CurrentMaxLineNo)
			{
				maxLineNo = lineNo;
			}
		}

		#endregion

		#region Implementation

		readonly WhsDocket docket;
		ZShort maxLineNo;

		#endregion
	}

	#endregion
}
