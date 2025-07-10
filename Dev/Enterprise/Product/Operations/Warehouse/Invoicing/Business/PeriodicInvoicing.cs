using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Business
{
	[SystemDefinedValues]
	[CodeProperty(JobStorage.Schema.ET_StorageJobNumber), DescriptionProperty(JobStorage.Schema.ET_StorageJobNumber)]
	[UniversalDataContext(DataContextType.WarehousePeriodicInvoice)]
	public class PeriodicInvoicing :
		JobStorage,
		IPeriodicInvoicing,
		IAutoRatingSuppressJobCreationPrompt,
		IPeriodicRatingSupporter,
		IDocManagerSupport,
		IEDocsProvider,
		IDocumentSupportable,
		ISendEmailSource,
		IValidationSuspenderForAutoRating
	{
		public PeriodicInvoicing(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ET_OffBandProcessingStatus), ConcurrencyPolicy.Strict);
		}

		public new class Schema : AutoJobStorage.Schema
		{
			public const string BillingAutomationStatus = "BillingAutomationStatus";
		}

		#region Strategy

		public IPeriodicInvoicingStrategy Strategy
		{
			get => string.IsNullOrEmpty(ET_StorageType)
					? throw new ApplicationException($"No Strategy is configured for '{ET_StorageType}'.")
					: strategy ??= GetStrategy();
		}

		IPeriodicInvoicingStrategy strategy;

		IPeriodicInvoicingStrategy GetStrategy()
		{
			var providers = (Hashtable)ObjectFactory.Get("PeriodicInvoicingStrategyFactory");
			var objectHandle = (ObjectHandle)providers[ET_StorageType.ToString()];
			return objectHandle?.GetObject() as IPeriodicInvoicingStrategy;
		}

		#endregion

		#region Notifications

		public INotifications NotificationSubscriber
		{
			get { return NotificationManager.Peek; }
		}

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

		NotificationManager notificationManager;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsFinalisedAndCommitted; }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			// ET_StorageType is set after construction of new object.
			ET_BillingDate = ZDateTime.Today;
			ET_StorageToDate = ZDateTime.Today;
		}

		public void SetDefaultsForNew(ZGuid clientPK, ZGuid warehousePK)
		{
			ET_OH_Client = clientPK;
			ET_WW = warehousePK;
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Strategy.GetHumanReadableName(ET_StorageJobNumber);
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PeriodicInvoicingFetchStrategy(this);
		}

		#endregion

		#region OnFactorySaving / Saved

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			try
			{
				base.OnFactorySavingBeforeTransactionCore();
				invoiceJobHeader = GetJobHeader(PK);
			}
			finally
			{
				if (!JobStatusWithPromptlessRelatedJobUpdate.IsEmpty)
				{
					JobStatusWithPromptlessRelatedJobUpdate = ZString.Empty;
					VerifyJobHeaderNotificationErrors = false;
				}
			}
		}

		protected override void OnFactorySaving()
		{
			if (invoiceJobHeader != null)
			{
				invoiceJobHeader = null;
			}
			RollbackChangesToET_ToDateIfIncludeInvoicingFalse();
			base.OnFactorySaving();
		}

		JobHeader invoiceJobHeader;

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new PeriodicInvoicingFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class PeriodicInvoicingFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public PeriodicInvoicingFountainUniqueIndexFailureHandler(PeriodicInvoicing invoice)
				: base(JobStorageSchema.Constants.Indexes.NR_UX__ET_StorageJobNumber, invoice)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.WarehouseInvoiceNumber;
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result.Add(warehouse);
				}

				var client = Client;
				if (client != null)
				{
					result.Add(client);
				}

				return result.ToArray();
			}
		}

#if DEBUG
		public StmNoteContexts GetNoteContextsForRelatedNotes()
		{
			return NoteContextsForRelatedNotes;
		}
#endif

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.P;

				return result;
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

				return notes;
			}
		}

		#endregion

		#region Related Business Objects

		#region JobHeader

		public Job JobHeader
		{
			get
			{
				var loaded = false;
				var isValidationResultOfValueChanged = false;

				if (jobHeader == null)
				{
					jobHeader = new Job.Loader(this).Load();
					loaded = true;
					if (jobHeader != null)
					{
						jobHeader.JH_StatusInfo.ValueChanged += (s, e) => RefreshBinding();
						jobHeader.JH_OA_LocalChargesAddrInfo.ValueChanged += OnLocalChargesClientChanged_SetClientAndSetFlagForValueChanged;
						jobHeader.JH_OA_LocalChargesAddrInfo.AdditionalValidation += AfterLocalChargesClientValidated;
					}
				}

				if (jobHeader != null && !jobHeader.IsDeleted)
				{
					if (!jobHeader.IsInDatabase && ET_OH_Client.IsValid &&
						(loaded || jobHeader.LocalChargesPK.IsEmpty))
					{
						jobHeader.LocalChargesPK = ET_OH_Client;
					}
					if (jobHeader.Parent == null)
					{
						jobHeader.Parent = this;
					}
				}
				return jobHeader;

				void OnLocalChargesClientChanged_SetClientAndSetFlagForValueChanged(object sender, EventArgs e)
				{
					var previousLocalClientAddressPK = e is ValueChangedEventArgs args ? (ZGuid?)args.OldValue : null;

					try
					{
						PreviousLocalClientAddress = previousLocalClientAddressPK;
						ET_OH_Client = JobHeader.LocalChargesPK;
					}
					finally
					{
						PreviousLocalClientAddress = null;
					}

					isValidationResultOfValueChanged = true;
				}

				void AfterLocalChargesClientValidated()
				{
					// if we have validation as a result of Local Client being changed, we can reset this flag
					if (isValidationResultOfValueChanged)
					{
						isValidationResultOfValueChanged = false;
					}
					// Otherwise since this is a result of tabbing off, we can reset the Invalid Client value.
					// This results in the next Validation not considering the Value that was reversed.
					else
					{
						InvalidClientThatWasReversed = null;
					}
				}
			}
		}

		Job jobHeader;

		#endregion

		#region TryCreateJobHeader

		public void TryCreateJobHeader()
		{
			new Job.Loader(this).TryCreate();
		}

		#endregion

		#endregion

		#region Properties

		#region Include in Invoicing

		public ZBool IncludeInInvoicing
		{
			get { return includeInInvoicing; }
			set
			{
				SetNonPersistentPropertyValue(IncludeInInvoicingInfo, ref includeInInvoicing, value);
				RollbackChangesToET_ToDateIfIncludeInvoicingFalse();

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
					RefreshBinding();
				}
			}
		}

		void RollbackChangesToET_ToDateIfIncludeInvoicingFalse()
		{
			if (!IncludeInInvoicing && ET_StorageToDateInfo.HasChanges && IsInDatabase)
			{
				Reload();
			}
		}

		ZBool includeInInvoicing = true;

		public ZPropertyInfo IncludeInInvoicingInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInInvoicing)); }
		}

		#endregion

		#region Is Finalised

		public ZBool IsFinalised => JobHeader?.IsClosed ?? false;

		public ZPropertyInfo IsFinalisedInfo
		{
			get { return GetZPropertyInfo(nameof(IsFinalised)); }
		}

		#endregion

		#region IsFinalisedAndCommitted

		ZBool IsFinalisedAndCommitted => IsFinalised && JobHeader.JH_StatusInfo.OriginalValue.Equals(JobHeaderStatus.Closed.Code);

		#endregion

		#region ET_BillingDate

		[ReadOnlyMember(nameof(ContainsChargePostedFromThisJob))]
		public override ZDateTime ET_BillingDate
		{
			get { return base.ET_BillingDate; }
			set { base.ET_BillingDate = value; }
		}

		#endregion

		#region ET_StorageJobNumber

		protected override ZString NewStorageJobNumber()
		{
			return Env.NumberFountains.WarehouseInvoiceNumber.GetNextFormatted(Factory);
		}

		[ReadOnly(true)]
		public override ZString ET_StorageJobNumber
		{
			get { return base.ET_StorageJobNumber; }
			set { base.ET_StorageJobNumber = value; }
		}

		#endregion

		#region JobStatusWithPromptlessRelatedJobUpdate

		[ResourceStringData("PeriodicInvoicing|JobStatusWithPromptlessRelatedJobUpdate", Caption = "Job Status Update")]
		[ActionField(CollectionType = typeof(JobHeaderStatusList))]
		public ZString JobStatusWithPromptlessRelatedJobUpdate
		{
			get { return jobStatusWithPromptlessRelatedJobUpdate; }
			set
			{
				SetNonPersistentPropertyValue(JobStatusWithPromptlessRelatedJobUpdateInfo, ref jobStatusWithPromptlessRelatedJobUpdate, value);

				var job = !value.IsEmpty ? JobHeader : null;
				if (job != null)
				{
					job.JH_Status = value;
					VerifyJobHeaderNotificationErrors = true;
				}
			}
		}

		public ZPropertyInfo JobStatusWithPromptlessRelatedJobUpdateInfo
		{
			get
			{
				var job = JobHeader;
				return job != null
					? GetWrappedZPropertyInfo(nameof(JobStatusWithPromptlessRelatedJobUpdate), x => job.JH_StatusInfo)
					: GetZPropertyInfo(nameof(JobStatusWithPromptlessRelatedJobUpdate));
			}
		}

		ZString jobStatusWithPromptlessRelatedJobUpdate = ZString.Empty;

		#endregion

		#region VerifyJobHeaderNotificationErrors

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public ZBool VerifyJobHeaderNotificationErrors
		{
			get { return verifyJobHeaderNotificationErrors; }
			set
			{
				SetNonPersistentPropertyValue(VerifyJobHeaderNotificationErrorsInfo, ref verifyJobHeaderNotificationErrors, value);

				if (!IsValidationSuspended)
				{
					((PeriodicInvoicingValidation)Validation).ValidateVerifyJobHeaderNotificationErrors();
				}
			}
		}

		ZBool verifyJobHeaderNotificationErrors;

		#endregion

		public ZPropertyInfo VerifyJobHeaderNotificationErrorsInfo => GetZPropertyInfo(nameof(VerifyJobHeaderNotificationErrors));

		#region ET_OH_Client

		[ReadOnlyMember(nameof(CustomReadOnly))]
		[List("Lookups.Clients")]
		public override ZGuid ET_OH_Client
		{
			get { return base.ET_OH_Client; }
			set
			{
				// Setting Client might set Local Charges back which will fire this
				// setter again, so we add this flag to skip invoking the below again.
				if (!IsSettingClient)
				{
					IsSettingClient = true;

					try
					{
						SetClientCore(value);
					}
					finally
					{
						IsSettingClient = false;
					}
				}
			}
		}

		void SetClientCore(ZGuid value)
		{
			var invalidClientWasSet = InvalidClientThatWasReversed.HasValue;
			InvalidClientThatWasReversed = null;

			var previousValue = ET_OH_Client;
			if (previousValue != value)
			{
				var isValidClient = value == (ZGuid)ET_OH_ClientInfo.OriginalValue || !ContainsChargePostedFromThisJob;
				if (isValidClient)
				{
					base.ET_OH_Client = value;

					// Setting the Storage From Date will Invalidate the Collections,
					// so instead we suspend it and manually call it once at the end.
					using (new SemaphoreManager(SuspendInvalidatingCollectionsSemaphore))
					{
						SetDefaultStorageFromDate();
					}

					SetToDateAndInvalidateCollections();
				}
				else
				{
					// Unfortunately whenever a ZAddress is changed in the GUI, the Architecture will Validate the Property bound to the Control,
					// even though property setters already call Validation. In any case, this is impossible to circumvent currently.
					// This means that even though the Client property is validated with the Client the user was trying to set before
					// being reverted, as soon as the control is tabbed off the Validation Error will be cleared because Tabbing off will re-validate
					// the Client after it's been reverted. This means we have to persist the Reverted Client for Validation purposes Yuck!
					// This also means the user cannot remove the error without tabbing off again or running validate all.
					InvalidClientThatWasReversed = value;

					if (PreviousLocalClientAddress.HasValue)
					{
						// Validation is going to run from the Original setting of Local Charges + GUI Tabbing off
						using (JobHeader.GetValidationSuspender())
						{
							JobHeader.JH_OA_LocalChargesAddr = PreviousLocalClientAddress.Value; // revert Local Client Address
						}
					}
				}
			}
			else if (invalidClientWasSet && !IsValidationSuspended)
			{
				// reset validation error
				JobHeader.Validation.ValidateJH_OA_LocalChargesAddr();
			}
		}

		bool IsSettingClient;

		/// <summary>
		/// DO NOT USE! Read ET_OH_Client setter to see the reason this property exists.
		/// </summary>
		internal ZGuid? InvalidClientThatWasReversed { get; private set; }
		ZGuid? PreviousLocalClientAddress;

		void SetToDateAndInvalidateCollections()
		{
			// No need to Invalidate Collections twice, so prevent it when setting To Date
			using (new SemaphoreManager(SuspendInvalidatingCollectionsSemaphore))
			{
				SetToDateFromClientRatingPeriod();
			}

			InvalidateCollections();
		}

		public ZGuid JobHeaderClient
		{
			get { return JobHeader != null ? JobHeader.LocalChargesPK : ZGuid.Empty; }
		}

		public ZString ClientName
		{
			get { return Client?.OH_FullNameTruncated ?? ZString.Empty; }
		}

		public ZPropertyInfo ClientNameInfo
		{
			get { return GetZPropertyInfo(nameof(ClientName)); }
		}

		#endregion

		#region ET_StorageFromDate

		[ReadOnlyMember(nameof(StorageFromDateInfoReadOnly))]
		public override ZDateTime ET_StorageFromDate
		{
			get { return base.ET_StorageFromDate; }
			set
			{
				bool wasChanged = ET_StorageFromDate != value;
				if (wasChanged)
				{
					var validSmallDateTimeValue = ZDateTime.GetValidSmallDateTime(value);
					base.ET_StorageFromDate = validSmallDateTimeValue;
					SetToDateAndInvalidateCollections();
				}
			}
		}

		void SetDefaultStorageFromDate()
		{
			if (Client != null && Warehouse != null)
			{
				var defaultStorageFrom = GetDefaultStorageFromDate();
				var isFirstBillingForClientWarehouse = defaultStorageFrom.IsEmpty;

				ET_StorageFromDate = isFirstBillingForClientWarehouse ? ZDateTime.Today.AddYears(-1) : defaultStorageFrom;
				SetFromDateReadOnlyIfFirstBilling(isFirstBillingForClientWarehouse);
			}
		}

		public void SetFromDateReadOnlyIfFirstBilling()
		{
			SetFromDateReadOnlyIfFirstBilling(IsFirstBillingForClientWarehouse);
		}

		void SetFromDateReadOnlyIfFirstBilling(bool isFirstBillingForClientWarehouse)
		{
			storageFromDateInfoReadOnly = !isFirstBillingForClientWarehouse;
		}

		bool StorageFromDateInfoReadOnly
		{
			get { return storageFromDateInfoReadOnly || ContainsChargePostedFromThisJob; }
		}

		bool storageFromDateInfoReadOnly;

		bool IsFirstBillingForClientWarehouse
		{
			get { return GetDefaultStorageFromDate().IsEmpty; }
		}

		#endregion

		#region ET_StorageToDate

		[ReadOnlyMember(nameof(ContainsChargePostedFromThisJob))]
		public override ZDateTime ET_StorageToDate
		{
			get { return base.ET_StorageToDate; }
			set
			{
				bool wasChanged = ET_StorageToDate != value;
				if (wasChanged)
				{
					base.ET_StorageToDate = value;
					InvalidateCollections();
				}
			}
		}

		void SetToDateFromClientRatingPeriod()
		{
			var client = Client;
			ET_StorageToDate = client != null && ET_StorageFromDate.IsValid
				? GetStorageToDate()
				: ZDateTime.Today;

			ZDateTime GetStorageToDate()
			{
				var clientStoragePeriod = client.CompanyData.GetWarehouseRatingPeriod();
				switch (clientStoragePeriod)
				{
					case Core.Constants.StorageCalculationPeriods.Weekly:
						return ET_StorageFromDate.AddDays(6);
					case Core.Constants.StorageCalculationPeriods.Fortnightly:
						return ET_StorageFromDate.AddDays(13);
					case Core.Constants.StorageCalculationPeriods.Monthly:
						return ET_StorageFromDate.AddMonths(1).AddDays(-1);
					case Core.Constants.StorageCalculationPeriods.BillingPeriod:
						return ZDateTime.Today;
					default:
						return ET_StorageFromDate;
				}
			}
		}

		#endregion

		#region ET_WW

		[ReadOnlyMember(nameof(CustomReadOnly))]
		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid ET_WW
		{
			get { return base.ET_WW; }
			set
			{
				bool wasChanged = ET_WW != value;
				if (wasChanged)
				{
					base.ET_WW = value;

					// Setting the Storage From Date will Invalidate the Collections,
					// so instead we suspend it and manually call it once at the end.
					using (new SemaphoreManager(SuspendInvalidatingCollectionsSemaphore))
					{
						ET_StorageFromDate = ZDateTime.Empty;
						SetDefaultStorageFromDate();
					}

					InvalidateCollections();
				}
			}
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(ET_WW); }
		}

		public ZString WarehouseName
		{
			get { return Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty; }
		}

		public ZPropertyInfo WarehouseNameInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseName)); }
		}

		#endregion

		#region BillingAutomationStatus

		public ZString BillingAutomationStatus
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.BillingAutomationStatus);
			set
			{
				if (!BillingAutomationStatusCodes.AllStatuses.ToList().Contains(value))
				{
					throw new InvalidOperationException($"'{value}' is not valid {Schema.BillingAutomationStatus} code.");  // Diagnostic Only
				}
				this.SetSystemDefinedValue(Schema.BillingAutomationStatus, value);
			}
		}

		#endregion

		#region CustomReadOnly

		public ZBool CustomReadOnly
		{
			get { return customReadOnly || ContainsChargePostedFromThisJob; }
			set { customReadOnly = value; }
		}
		bool customReadOnly;

		#endregion

		#region ContainsChargePostedFromThisJob

		bool ContainsChargePostedFromThisJob
		{
			get
			{
				bool result;

				var cache = RatingCache.LocalSession;
				if (cache != null)
				{
					result = cache.GetCachedValue("WhsInvoice|AutoRating|ContainsPostedCharge|" + PK, HasPostedChargeFromThisJob);
				}
				else
				{
					result = HasPostedChargeFromThisJob();
				}

				return result;

				bool HasPostedChargeFromThisJob()
				{
					bool hasPostedCharge;

					if (IncludeInInvoicing)
					{
						var job = JobHeader;
						if (job != null)
						{
							var query = new ZQuery();
							query.AddToFilter(JobHeaderSchema.JH_JH_ParentJob, job.PK);
							query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
							Factory.AddFetchHint(JobHeaderSchema.Instance, query);
						}

						hasPostedCharge = job?.Charges.Cast<Charge>().Any(c => c.IsRevenuePosted && c.JR_JobNumber == job.JH_JobNum) ?? false;
					}
					else
					{
						hasPostedCharge = true;
					}

					return hasPostedCharge;
				}
			}
		}

		#endregion

		#region ET_OffBandProcessingStatus

		[List("Lookups.OffBandProcessingStatus")]
		public override ZString ET_OffBandProcessingStatus
		{
			get => base.ET_OffBandProcessingStatus;
			set => base.ET_OffBandProcessingStatus = value;
		}

		#endregion

		#region ProcessingStatusDesc

		public ZString ProcessingStatusDesc => Lookups.OffBandProcessingStatus.GetDescriptionFromCode(ET_OffBandProcessingStatus);

		#endregion

		#region HasCharges

		public bool HasCharges => JobHeader != null && JobHeader.Charges.Count > 0;

		#endregion

		#endregion

		#region Lookups

		protected override JobStorageLookups GetNewLookups()
		{
			return new PeriodicInvoicingLookups(this);
		}

		public new PeriodicInvoicingLookups Lookups
		{
			get { return (PeriodicInvoicingLookups)base.Lookups; }
		}

		#endregion

		#region Validation

		protected override JobStorageValidation GetNewValidation()
		{
			return new PeriodicInvoicingValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			InvalidClientThatWasReversed = null; // When running ValidateAll() or Saving the Form we should Clear this Field to allow for proper Validation.

			base.RunPreSaveValidationCore();
		}

		#endregion

		#region StoragePeriod

		internal StorageDates[] GetStorageDates()
		{
			if (!ET_StorageFromDate.IsValid || !ET_StorageToDate.IsValid)
			{
				return new StorageDates[] { new StorageDates(ZDateTime.Empty, ZDateTime.Empty) };
			}

			var result = new List<StorageDates>();
			var from = ET_StorageFromDate;
			var period = StorageCalculationPeriod;

			ZDateTime to;
			do
			{
				int periodLength;
				if (period > 0)
				{
					periodLength = (int)period;
				}
				else if (period == CalculationPeriod.Month)
				{
					periodLength = (int)(from.AddMonths(1) - from).TotalDays;
				}
				else
				{
					periodLength = (int)(ET_StorageToDate - ET_StorageFromDate).TotalDays + 1;
				}

				to = from.AddDays(periodLength - 1);
				if (to > ET_StorageToDate)
				{
					to = ET_StorageToDate;
				}

				result.Add(new StorageDates(from, to));

				from = to.AddDays(1);
			}
			while (from <= ET_StorageToDate);

			return result.ToArray();
		}

		CalculationPeriod StorageCalculationPeriod
		{
			get
			{
				var warehouseRatingPeriod = (string)Client?.CompanyData.GetWarehouseRatingPeriod() ?? Env.Registry.Rating.StorageCalculationPeriod;
				switch (warehouseRatingPeriod)
				{
					case Core.Constants.StorageCalculationPeriods.Daily:
						return CalculationPeriod.Day;

					case Core.Constants.StorageCalculationPeriods.Weekly:
						return CalculationPeriod.Week;

					case Core.Constants.StorageCalculationPeriods.Fortnightly:
						return CalculationPeriod.Fortnight;

					case Core.Constants.StorageCalculationPeriods.Monthly:
						return CalculationPeriod.Month;

					default:
						return CalculationPeriod.None;
				}
			}
		}

		enum CalculationPeriod
		{
			None = 0,
			Day = 1,
			Week = 7,
			Fortnight = 14,
			Month = -30
		}

		internal struct StorageDates
		{
			public StorageDates(ZDateTime from, ZDateTime to)
			{
				this.From = from;
				this.To = to;
			}

			public readonly ZDateTime From;
			public readonly ZDateTime To;
		}

		#endregion

		#region AutoRateJobHeader

		public void AutoRateJobHeader(IAutoRatingGUIInteractor interactor)
		{
			if (JobHeader == null)
			{
				jobHeader = new Job.Loader(this).TryCreateWithMutex();
				jobHeader.PlugInData = this;
			}

			var options = AutoRateOptions.AutorateCostsRevenue.With(triggerSource: AutoRateTriggerSource.Menu);
			new AutoRatingStarter(this, interactor).ExecuteAutorating(options);
			((IAutoRatingAccountingUtils)JobHeader).ReloadChargesFromAdditionalJobs();
		}

		#endregion

		#region IsInvoiceBillingCheckSuspended

		public bool IsInvoiceBillingCheckSuspended => isInvoiceBillingCheckLockSemaphore != null && isInvoiceBillingCheckLockSemaphore.IsSuspended;
		Semaphore IsInvoiceBillingCheckLockSemaphore => isInvoiceBillingCheckLockSemaphore ?? (isInvoiceBillingCheckLockSemaphore = new Semaphore());
		Semaphore isInvoiceBillingCheckLockSemaphore;

		public IDisposable InvoiceBillingCheckLockSuspender() => new SemaphoreManager(IsInvoiceBillingCheckLockSemaphore);

		#endregion

		#region PostInvoice

		public InvoicingBaseCollection PostInvoice()
		{
			InvoicingBaseCollection postedInvoices = null;
			if (!HasErrors && JobHeader.Charges.Count > 0)
			{
				var postManager = new InvoicingPostManager(JobHeader);
				Factory.Save();

				ZDateTime postDate = GetDefaultPostDate(ET_BillingDate);

				postManager.CreateTransactions(JobInvoicingPostingOption.All);
				postManager.Poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(ET_BillingDate, postDate);

				postedInvoices = postManager.Poster.PostedInvoices;
			}
			return postedInvoices;
		}

		ZDateTime GetDefaultPostDate(ZDateTime billingDate)
		{
			var postDate = ZDateTime.Empty;

			if (AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate &&
				AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate)
			{
				postDate = billingDate;
			}

			return postDate;
		}

		#endregion

		#region Delete

		#region Delete

		public override void Delete()
		{
			DeleteJobHeader();
			DeleteStorageLines();
			base.Delete();
		}

		#endregion

		#region DeleteJobHeader

		void DeleteJobHeader()
		{
			var header = JobHeader;
			if (header != null)
			{
				header.Delete();
			}
		}

		#endregion

		#region DeleteStorageLines

		void DeleteStorageLines()
		{
			Strategy.DeleteStorageLines(this);
		}

		#endregion

		#region CanDelete

		public override bool CanDelete
		{
			get
			{
				var header = JobHeader;
				return (header == null) || header.CanDelete;
			}
		}

		#endregion

		#region ReasonForNotAbleToDelete

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var header = JobHeader;
				return (header != null) ? header.ReasonForNotAbleToDelete : (NoResString)string.Empty;
			}
		}

		#endregion

		#endregion

		#region IAutoRatingProcessDeferrer Members

		void IValidationSuspenderForAutoRating.OnValidationResumed()
		{
			JobHeader?.Validation.ValidateAll();
		}

		bool IValidationSuspenderForAutoRating.ShouldRunPreSaveValidationBeforeAutorating(IBusiness hostEntity) => hostEntity == this;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ??= Strategy.GetPeriodicInvoicingDocManagerInfo(this); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override JobStorageInvoicingSupporter GetNewInvoicingSupporter()
		{
			return Strategy.GetPeriodicInvoicingInvoicingSupporter(this);
		}

		#endregion

		#region IJobHeaderParent Members

		protected override bool AllowInvoiceDeletionCore
		{
			get { return false; }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ??= Strategy.GetPeriodicInvoicingDocumentSupporter(this); }
		}

		DocumentSupporter documentSupporter;

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(Client);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("7bd58b08-6c89-42ac-ad21-6a92578c4ca6", "Periodic Invoice") + " - " + ET_StorageJobNumber; }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.WarehousePeriodicBilling; }
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

		#region Implementation

		#region InvalidateCollections

		void InvalidateCollections()
		{
			if (!SuspendInvalidatingCollectionsSemaphore.IsSuspended)
			{
				InvalidateCollectionsCore();
			}
		}

		void InvalidateCollectionsCore()
		{
			if (JobHeader != null)
			{
				JobHeader.Parent = this; // Forces a refresh of the Charges
			}
		}

		Semaphore SuspendInvalidatingCollectionsSemaphore => suspendInvalidatingCollectionsSemaphore ?? (suspendInvalidatingCollectionsSemaphore = new Semaphore());
		Semaphore suspendInvalidatingCollectionsSemaphore;

		#endregion

		#region HasOverlappingInvoices

		internal bool HasOverlappingInvoices()
		{
			if (!(TypeValidation.IsInSmallDateTimeRange(ET_StorageFromDateInfo) && TypeValidation.IsInSmallDateTimeRange(ET_StorageToDateInfo)))
			{
				return false;
			}

			var qry = new ZQuery();
			qry.AddToFilter(JobStorageSchema.PK, SQLComparisonOperator.NotEqual, PK);
			qry.AddToFilter(JobStorageSchema.ET_StorageType, ET_StorageType);
			qry.AddToFilter(JobStorageSchema.ET_OH_Client, ET_OH_Client);
			qry.AddToFilter(JobStorageSchema.ET_WW, ET_WW);
			qry.AddToFilter(JobStorageSchema.ET_StorageFromDate, SQLComparisonOperator.LessThanOrEqualTo, ET_StorageToDate);
			qry.AddToFilter(JobStorageSchema.ET_StorageToDate, SQLComparisonOperator.GreaterThanOrEqualTo, ET_StorageFromDate);

			return Factory.LoadTop1<JobStorage>(qry) != null;
		}

		#endregion

		#region DefaultStorageFromDateCalculated

		public bool DefaultStorageFromDateCalculated { get; private set; }

		ZDateTime GetDefaultStorageFromDate()
		{
			DefaultStorageFromDateCalculated = true;

			var dynBizObjCollection = new DynamicBusinessObjectCollection(Factory);
			var @params = new ZSqlParameterCollection
			{
				{ "@StorageType", ET_StorageType, JobStorageSchema.ET_StorageType },
				{ "@ThisPK", PK, JobStorageSchema.PK },
				{ "@Client", ET_OH_Client, JobStorageSchema.ET_OH_Client },
				{ "@Warehouse", ET_WW, JobStorageSchema.ET_WW }
			};

			var rawQuery = "select MAX({4}) as ToDate from {0} where {1} = @StorageType and {2} <> @ThisPK and {3} = @Client and {5} = @Warehouse";

			dynBizObjCollection.Load(
				String.Format(CultureInfo.InvariantCulture,
				rawQuery,
				JobStorageSchema.Constants.TableName,
				JobStorageSchema.ET_StorageType.Name,
				JobStorageSchema.PK.Name,
				JobStorageSchema.ET_OH_Client.Name,
				JobStorageSchema.ET_StorageToDate.Name,
				JobStorageSchema.ET_WW.Name),
				@params);

			var result = (ZDateTime)dynBizObjCollection[0]["ToDate"];
			if (!result.IsEmpty)
			{
				result = result.AddDays(1);
			}

			return result;
		}

		#endregion

		#region GetJobHeader

		JobHeader GetJobHeader(ZGuid invoicePK)
		{
			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, invoicePK);
			return Factory.LoadTop1<JobHeader>(jobQuery);
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ET_StorageType = PeriodicInvoicingStorageTypes.Codes.ContainerYard;
		}
#endif

		#endregion

		#region IAutoRatingCreateJobHeaderPromptOverride

		bool IAutoRatingSuppressJobCreationPrompt.SuppressJobCreationPrompt => true;

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return Strategy.GetRatingAdaptersProvider(this); }
		}

		#endregion

		#region AuditLogging

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		#endregion
	}
}
