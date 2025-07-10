using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
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
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	[SystemDefinedValues]
	[CodeProperty(JobStorage.Schema.ET_StorageJobNumber), DescriptionProperty(JobStorage.Schema.ET_StorageJobNumber)]
	[UniversalDataContext(DataContextType.WarehousePeriodicInvoice)]
	public partial class WhsInvoice :
		JobStorage,
		Enterprise.Integration.Warehouse.IWhsInvoice,
		IAutoRatingSuppressJobCreationPrompt,
		IPeriodicRatingSupporter,
		IJobInvoicingPlugInAdditionalJobs,
		IJobInvoicingAdditionalData,
		IDocManagerSupport,
		IEDocsProvider,
		IDocumentSupportable,
		ISendEmailSource,
		IValidationSuspenderForAutoRating
	{
		public WhsInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ET_OffBandProcessingStatus), ConcurrencyPolicy.Strict);
		}

		public new class Schema : AutoJobStorage.Schema
		{
			public const string BillingAutomationStatus = "BillingAutomationStatus";
		}

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

		public const string StorageType = "WHS";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ET_StorageType = StorageType;
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
				return ET_StorageJobNumber.IsEmpty ?
					Res.GetString("0f3b04bb-85ca-4328-98be-83d00e839865", "Warehouse Periodic Invoice") :
					Res.GetString("55299c38-044a-4826-ab94-f7e2d27e4245", "Warehouse Periodic Invoice {0}", ET_StorageJobNumber);
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsInvoiceFetchStrategy(this);
		}

		#endregion

		#region OnFactorySaving / Saved

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			try
			{
				base.OnFactorySavingBeforeTransactionCore();
				JobsToUpdate = GetJobsToUpdate();
				invoiceJobHeader = GetJobHeader(PK);
				if (invoiceJobHeader != null)
				{
					invoiceJobHeader.JH_StatusInfo.ValueChanged -= JH_StatusInfo_ValueChanged;
					invoiceJobHeader.JH_StatusInfo.ValueChanged += JH_StatusInfo_ValueChanged;
					if (!invoiceJobHeader.IsInDatabase || invoiceJobHeader.JH_StatusInfo.HasChanges)
					{
						UpdateInvoiceStatusForRelatedJobs();
					}
				}
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
				invoiceJobHeader.JH_StatusInfo.ValueChanged -= JH_StatusInfo_ValueChanged;
				invoiceJobHeader = null;
			}
			RollbackChangesToET_ToDateIfIncludeInvoicingFalse();
			base.OnFactorySaving();
		}

		void JH_StatusInfo_ValueChanged(object sender, EventArgs e) => UpdateInvoiceStatusForRelatedJobs();
		JobHeader invoiceJobHeader;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (JobsToUpdate != null && JobsToUpdate.Count > 0)
			{
				string notificationMsg = "\r\n" + Res.GetString("183a8a86-354c-4319-a093-aad2f79683ac", "The following jobs have been successfully updated:\r\n\r\n{0}", GetJobNumberString(JobsToUpdate));
				var e = new QueryUserMsgBoxEventArgs(Res.GetString("01df5f94-3302-41fb-a15b-7ac3e715fedf", "Jobs Updated"), notificationMsg, true);
				NotificationSubscriber.QueryUser(e);

				JobsToUpdate.Clear(); // not strictly necessary as JobsToUpdate is recreated on each factory saving, but clean up for prudence.
			}
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new WhsInvoiceFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class WhsInvoiceFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public WhsInvoiceFountainUniqueIndexFailureHandler(WhsInvoice invoice)
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

		#region AdditionalDataRows

		DataRow[] AdditionalDataRows()
		{
			var docketTypeCodesToLoad = GetDocketTypesToLoad(Client);
			var filter = LoadDocketCollectionFilter(docketTypeCodesToLoad);

			return ((IBusinessObjectFactoryInternals)Factory).RowFactory.Load(WhsDocketSchema.Constants.TableName, filter);
		}

		IEnumerable<string> GetDocketTypesToLoad(OrgHeader client)
		{
			if (client != null)
			{
				var companyData = client.CompanyData;
				if (companyData.IsWhsSplitMonthBilling)
				{
					yield return DocketType.Codes.Adjustment;
				}

				if (companyData.OB_ARIncludeInwardsWhsConsolidatedInvoice)
				{
					yield return DocketType.Codes.Receive;
				}

				if (companyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice)
				{
					yield return DocketType.Codes.Order;
				}
			}
		}

		#endregion

		#region Ad Hoc Service Jobs

		DataRow[] GetDataFromRowFactory(string tableName, ZQuery query) => ((IBusinessObjectFactoryInternals)Factory).RowFactory.Load(tableName, query);

		bool InvoiceDataHasEntered => ET_OH_Client.IsValid && ET_WW.IsValid && ET_StorageFromDate.IsValid && ET_StorageToDate.IsValid;

		public WhsAdHocServiceJob[] AdHocServiceJobs()
		{
			if (!InvoiceDataHasEntered)
			{
				return Array.Empty<WhsAdHocServiceJob>();
			}
			return adHocServiceJobs ?? (adHocServiceJobs = Factory.Load<WhsAdHocServiceJob>(GetAdHocServiceJobsQuery()));
		}

		WhsAdHocServiceJob[] adHocServiceJobs;

		ZQuery GetAdHocServiceJobsQuery()
		{
			var whsServiceJobQuery = new ZDBOnlyQuery(typeof(WhsAdHocServiceJob))
				.AddToFilter(WhsAdHocServiceJobSchema.WSJ_IsFinalised, ZBool.True)
				.AddToFilter(WhsAdHocServiceJobSchema.WSJ_BillingDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ET_StorageFromDate)
				.AddToFilter(WhsAdHocServiceJobSchema.WSJ_BillingDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ET_StorageToDate)
				.AddToFilter(WhsAdHocServiceJobSchema.WSJ_WW_Whs, ET_WW)
				.AddToFilter(WhsAdHocServiceJobSchema.WSJ_OH_Client, ET_OH_Client);
			return whsServiceJobQuery;
		}

		#endregion

		#region VAS Orders

		ZQuery GetVASOrdersQuery()
		{
			var homePort = Warehouse.RelatedCompanyBranch?.HomePort;
			var dateRangeParams = new ZSqlParameterCollection {
				{ "@FromDate", ToUtcDate(ET_StorageFromDate), WhsVASOrderSchema.WVO_FinalizedTimeUtc },
				{ "@ToDate", ToUtcDate(ET_StorageToDate), WhsVASOrderSchema.WVO_FinalizedTimeUtc }
			};

			// Match warehouse by sevice area
			var areaSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsVASOrderSchema.WVO_WA_ServiceArea);
			areaSubQuery.AddToFilter(WhsAreaSchema.WA_WW_Whs, ET_WW);
			areaSubQuery.AddToFilter(WhsVASOrderSchema.WVO_OH_Client, ET_OH_Client);

			var whsVASOrderQuery = new ZDBOnlyQuery(typeof(WhsVASOrder));
			whsVASOrderQuery.AddToFilter(WhsVASOrderSchema.WVO_FinalizedTimeUtc, SQLComparisonOperator.NotEqual, ZDateTime.Empty);

			whsVASOrderQuery.AddSubQuery(areaSubQuery, JoinCondition.And);
			// filter by transfer's finalised date
			whsVASOrderQuery.AddFilterAndZSQLParameterCollection(GetFilterDocketByFinalisedDate, dateRangeParams);

			return whsVASOrderQuery;

			DateTime ToUtcDate(ZDateTime dateTime)
			{
				return dateTime.ToDateTimeOffset(homePort).ToUtcDateTime().Date;
			}
		}

		#region GetFilterDocketByFinalisedDate

		static string GetFilterDocketByFinalisedDate => @"CAST(WVO_FinalizedTimeUtc AS DATE) BETWEEN @FromDate AND @ToDate";

		#endregion

		#endregion

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

		[ResourceStringData("WhsInvoice|JobStatusWithPromptlessRelatedJobUpdate", Caption = "Job Status Update")]
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
					((WhsInvoiceValidation)Validation).ValidateVerifyJobHeaderNotificationErrors();
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

		#region InvoiceStatus

		ZString GetInvoiceStatus()
		{
			JobHeader invoiceJobHeader = GetJobHeader(this.PK);
			return (invoiceJobHeader != null) ? invoiceJobHeader.JH_Status : ZString.Empty;
		}

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
							// tested in WhsInvoiceJobStorageTest
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
			return new WhsInvoiceLookups(this);
		}

		public new WhsInvoiceLookups Lookups
		{
			get { return (WhsInvoiceLookups)base.Lookups; }
		}

		#endregion

		#region Validation

		protected override JobStorageValidation GetNewValidation()
		{
			return new WhsInvoiceValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			InvalidClientThatWasReversed = null; // When running ValidateAll() or Saving the Form we should Clear this Field to allow for proper Validation.

			base.RunPreSaveValidationCore();

			var docketTypeCodesToLoad = GetDocketTypesToLoad(Client);

			DataRow[] orderRows;

			if (docketTypeCodesToLoad.Contains(DocketType.Codes.Order))
			{
				var filter = LoadDocketCollectionFilter(new[] { DocketType.Codes.Order });
				orderRows = ((IBusinessObjectFactoryInternals)Factory).RowFactory.Load(WhsDocketSchema.Constants.TableName, filter);
			}
			else
			{
				orderRows = Array.Empty<DataRow>();
			}

			foreach (var order in orderRows)
			{
				// tested in WhsInvoiceJobStorageTest
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, new ZGuid(order[WhsDocketSchema.Constants.PK]));
			}
		}

		#endregion

		#region GetAdditionalDockets

		/// <summary>
		/// Returns all Receive and Orders that are relevant to this storage billing period.
		/// Returns Inwards / outwards based on organisation preference.
		/// Returns Adjustments if using Split Month Billing
		/// Excludes jobs that have the 'Exclude from Periodic Auto-Rating' flag ticked.
		/// </summary>
		/// <returns>Array of Dockets</returns>
		internal WhsDocket[] GetAdditionalDockets()
		{
			WhsDocket[] result;

			var cache = RatingCache.LocalSession;
			if (cache != null)
			{
				result = cache.GetCachedValue("WhsInvoice|AutoRating|Dockets|" + PK, GetAdditionalDockets);
			}
			else
			{
				result = GetAdditionalDockets();
			}

			return result;

			WhsDocket[] GetAdditionalDockets() => GetAdditionalDocketsCore().Where(docket => IncludeInPeriodicRating(docket, setJobDefaults: true)).ToArray();
		}

		internal WhsDocket[] GetAdditionalDocketsWithoutSetJobDefaults() => GetAdditionalDocketsCore().Where(docket => IncludeInPeriodicRating(docket, setJobDefaults: false)).ToArray();

		WhsDocket[] GetAdditionalDocketsCore()
		{
			var docketTypesToInclude = GetDocketTypesToLoad(Client);
			var filter = LoadDocketCollectionFilter(docketTypesToInclude);
			var allDocketsToCheck = Factory.Load<WhsDocket>(filter);

			// tested in WhsInvoiceJobStorageTest
			foreach (var docket in allDocketsToCheck)
			{
				var jobHeaderQuery = new ZQuery(JobHeaderSchema.JH_ParentID, docket.PK);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Factory.AddFetchHint(JobHeaderSchema.Instance, jobHeaderQuery);
			}

			return allDocketsToCheck;
		}

		bool IncludeInPeriodicRating(WhsDocket docket, bool setJobDefaults)
		{
			var job = new Job.Loader(docket).Load(true, setJobDefaults: setJobDefaults);
			return job == null || !job.JH_ExcludeFromPeriodicRating;
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

		#region Updating Related Dockets' Status

		#region GetJobsToUpdate

		List<Job> GetJobsToUpdate()
		{
			var result = new List<Job>();

			ZString newStatus = GetInvoiceStatus();
			var jobsNeedingUpdate = GetJobsToUpdateCore(newStatus);

			bool updateRelatedJobsWithoutAskingUser = (newStatus != JobHeaderStatus.Closed.Code) || !JobStatusWithPromptlessRelatedJobUpdate.IsEmpty;

			if (jobsNeedingUpdate.Count > 0 && (updateRelatedJobsWithoutAskingUser || AskUserToUpdateRelatedJobs(jobsNeedingUpdate)))
			{
				var jobsRequiringProfitLossReasonCode = new List<Job>();

				foreach (Job job in jobsNeedingUpdate)
				{
					if (job != null)
					{
						if (IsProfitLossReasonCodeInvalidForThisJobStatus(job, newStatus))
						{
							jobsRequiringProfitLossReasonCode.Add(job);
						}
						else
						{
							result.Add(job);
						}
					}
				}

				// if some jobs have failed validation, confirm that we want to perform a partial update
				if (JobStatusWithPromptlessRelatedJobUpdate.IsEmpty && jobsRequiringProfitLossReasonCode.Count > 0 && !AskUserToUpdateRelatedJobsWhenSomeJobsRequireProfitLossReasonCodes(result, jobsRequiringProfitLossReasonCode))
				{
					result.Clear();
				}
			}

			return result;
		}

		bool IsProfitLossReasonCodeInvalidForThisJobStatus(Job jobToValidate, ZString status)
		{
			return ((JobValidation)jobToValidate.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(status);
		}

		#region GetJobsToUpdateCore

		IReadOnlyCollection<Job> GetJobsToUpdateCore(ZString newStatus)
		{
			var jobsNeedingStatusUpdate = new List<Job>();
			var serviceJobs = AdHocServiceJobs();
			var docketTypesToInclude = GetDocketTypesToLoad(Client);
			var filter = LoadDocketCollectionFilter(docketTypesToInclude);
			var dockets = Factory.Load<WhsDocket>(filter);

			var allJobHeaders = new List<JobHeader>(dockets.Length + serviceJobs.Length);
			allJobHeaders.AddRange(dockets.Where(d => d.WD_DocketType != DocketType.Codes.Adjustment).Select(d => d.JobHeader));

			if (serviceJobs.Length > 0)
			{
				allJobHeaders.AddRange(serviceJobs.Select(ad => ad.JobHeader));
			}

			// After WI00160704 it should not be possible to save JobHeader with empty JH_Status, but WhsInvoice.GetInvoiceStatus() may return empty value if WhsInvoice.GetJobHeader(this.PK) is empty.
			if (!newStatus.IsEmpty)
			{
				foreach (var header in allJobHeaders)
				{
					if (header != null && header.JH_Status != newStatus &&
						!(header.JH_Status == JobHeaderStatus.JobInvoiced.Code && header.JH_ExcludeFromPeriodicRating))
					{
						jobsNeedingStatusUpdate.Add((Job)header);
					}
				}
			}
			return jobsNeedingStatusUpdate;
		}

		#endregion

		#region AskUserToUpdateRelatedJobs / AskUserToUpdateRelatedJobsWhenSomeJobsRequireProfitLossReasonCodes

		bool AskUserToUpdateRelatedJobs(IReadOnlyCollection<Job> jobsNeedingUpdate)
		{
			string caption = Res.GetString("d720d595-cfc7-42e9-be0b-8d7974542d51", "Update jobs");
			string message = "\r\n" + Res.GetString("c7de1153-24d6-4b5f-a4f5-db7258b37f57", "You are about to update the invoice status for the following related warehouse jobs, and reverse all related WIPs and ACRs as necessary:\r\n\r\n{0}\r\nAre you sure you want to continue?",
				GetJobNumberString(jobsNeedingUpdate));

			var e = new QueryUserYesNoEventArgs(caption, message, false);
			NotificationSubscriber.QueryUser(e);

			return e.Response;
		}

		bool AskUserToUpdateRelatedJobsWhenSomeJobsRequireProfitLossReasonCodes(List<Job> jobsToUpdate, List<Job> jobsRequiringProfitLossReasonCode)
		{
			bool result = true;

			var errorMsg = Res.GetString("7e7550fa-07e6-407d-9f8b-6350d7996a8c",
				"The following jobs require a Reason Code because their Profit Margin falls outside the tolerated margin threshold.\r\n\r\nPlease assign a Job Profit / Loss Reason Code to these jobs and then manually update them.\r\n\r\n{0}",
				GetJobNumberString(jobsRequiringProfitLossReasonCode));

			if (jobsToUpdate.Count > 0)
			{
				// there are some 'good' jobs to update, ask the user if they should perform the partial update
				errorMsg += "\r\n" + Res.GetString("800dbcdf-560b-4d6a-887f-7b27200a7c4d", "However, the following jobs are ready for updating:\r\n\r\n{0}\r\n\r\nDo you want to Continue?", GetJobNumberString(jobsToUpdate));
				var e = new QueryUserYesNoEventArgs(Res.GetString("96770348-7e4c-412c-b01b-756a8e1e2eb6", "Warning"), errorMsg, false);
				NotificationSubscriber.QueryUser(e);
				result = e.Response;
			}
			else // just inform the user
			{
				NotificationSubscriber.AddWarning(errorMsg);
			}

			return result;
		}

		#endregion

		#endregion

		#region UpdateInvoiceStatusForRelatedJobs

		void UpdateInvoiceStatusForRelatedJobs()
		{
			if (JobsToUpdate != null && JobsToUpdate.Count > 0)
			{
				var newStatus = GetInvoiceStatus();

				foreach (var job in JobsToUpdate)
				{
					job.JH_Status = newStatus;
				}
			}
		}

		#endregion

		List<Job> JobsToUpdate;

		#endregion

		#region Delete

		#region Delete

		public override void Delete()
		{
			DeleteJobHeader();
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
			get { return docManagerInfo ?? (docManagerInfo = new WhsInvoiceDocManagerInfo(this)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override JobStorageInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsInvoiceInvoicingSupporter(this);
		}

		#endregion

		#region IJobHeaderParent Members

		protected override bool AllowInvoiceDeletionCore
		{
			get { return false; }
		}

		#endregion

		#region IJobInvoicingPlugInAdditionalJobs Members

		IJobInvoicingPlugIn[] IJobInvoicingPlugInAdditionalJobs.AdditionalJobsToShowChargesFor
		{
			get
			{
				IJobInvoicingPlugIn[] result;

				var cache = RatingCache.LocalSession;
				if (cache != null)
				{
					result = cache.GetCachedValue("WhsInvoice|AutoRating|AdditionalJobs|" + PK, GetAdditionalJobs);
				}
				else
				{
					result = GetAdditionalJobs();
				}

				return result;

				IJobInvoicingPlugIn[] GetAdditionalJobs()
				{
					var invoicingJobs = new List<IJobInvoicingPlugIn>();

					if (InvoiceDataHasEntered)
					{
						var additionalJobs = WhsInvoiceJobWrapper.CreateInvoiceJobsFromDataRows(AdditionalDataRows(), WhsDocketSchema.Constants.PK, new WrappedWhsInvoiceJob<WhsDocket>());
						invoicingJobs.AddRange(additionalJobs);

						var cartageJobs = GetDataFromRowFactory(JobCartageSchema.Constants.TableName, new ZQuery(JobCartageSchema.JJ_ParentID, invoicingJobs.Select(j => j.PK)));
						cartageJobs = cartageJobs.GroupBy(c => c[JobCartageSchema.Constants.JJ_ParentID]).Where(g => g.Count() == 1).Select(g => g.Single()).ToArray();
						invoicingJobs.AddRange(WhsInvoiceJobWrapper.CreateInvoiceJobsFromDataRows(cartageJobs, JobCartageSchema.Constants.PK, WrappedInvoiceJobCartageJob.Instance));

						var adHocServiceJobRows = GetDataFromRowFactory(WhsAdHocServiceJobSchema.Constants.TableName, GetAdHocServiceJobsQuery());
						invoicingJobs.AddRange(WhsInvoiceJobWrapper.CreateInvoiceJobsFromDataRows(adHocServiceJobRows, WhsAdHocServiceJobSchema.Constants.PK, new WrappedWhsInvoiceJob<WhsAdHocServiceJob>()));

						var varOrdersRows = GetDataFromRowFactory(WhsVASOrderSchema.Constants.TableName, GetVASOrdersQuery());
						invoicingJobs.AddRange(WhsInvoiceJobWrapper.CreateInvoiceJobsFromDataRows(varOrdersRows, WhsVASOrderSchema.Constants.PK, new WrappedWhsInvoiceJob<WhsVASOrder>()));
					}

					return invoicingJobs.ToArray();
				}
			}
		}

		#endregion

		#region IJobInvoicingAdditionalDataPropertyProvider Members

		CustomPropertyContainer<JobCharge> IJobInvoicingAdditionalData.GetAdditionalProperties()
		{
			return new WhsOrderJobInvoicingAdditionalDataPropertyProvider().GetAdditionalProperties();
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
			get { return documentSupporter ?? (documentSupporter = new WhsInvoiceDocumentSupporter(this)); }
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
			get { return Res.GetString("DB7C8ACC-80E3-49b5-A35E-3BB47BAF01C9", "Periodic Invoice") + " - " + ET_StorageJobNumber; }
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
			ForceToReloadJobs();
			if (JobHeader != null)
			{
				JobHeader.Parent = this; // Forces a refresh of the Charges
			}
		}

		void ForceToReloadJobs()
		{
			adHocServiceJobs = null;
		}

		Semaphore SuspendInvalidatingCollectionsSemaphore => suspendInvalidatingCollectionsSemaphore ?? (suspendInvalidatingCollectionsSemaphore = new Semaphore());
		Semaphore suspendInvalidatingCollectionsSemaphore;

		#endregion

		#region LoadDocketCollection

		ZQuery LoadDocketCollectionFilter(IEnumerable<string> typesOfDocket)
		{
			var filter = new ZQuery(WhsDocketSchema.WD_DocketSubType, SQLComparisonOperator.NotEqual, AdjustmentType.Codes.InternalWarehouseAdjustment);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, typesOfDocket);
			filter.AddToFilter(WhsDocketSchema.WD_OH_Client, SQLComparisonOperator.Equal, ET_OH_Client);
			filter.AddToFilter(WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, ET_WW);
			filter.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForReceive, SQLComparisonOperator.Equal, null);

			if (ET_StorageFromDate.IsValid)
			{
				var storageFromOffset = Warehouse.GetWarehouseBranchLocalDateTimeOffset(ET_StorageFromDate);
				filter.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, storageFromOffset);
			}

			if (ET_StorageToDate.IsValid)
			{
				var storageToOffset = Warehouse.GetWarehouseBranchLocalDateTimeOffset(ET_StorageToDate);
				filter.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, storageToOffset);
			}

			return filter;
		}

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
			else
			{
				dynBizObjCollection = new DynamicBusinessObjectCollection(Factory);
				@params = new ZSqlParameterCollection
				{
					{ "@Client", ET_OH_Client, WhsDocketSchema.WD_OH_Client },
					{ "@Warehouse", ET_WW, WhsDocketSchema.WD_WW_Whs }
				};
				rawQuery = "select MIN({2}) as ToDate from {0} where {1} = @Client and {2} is not null and {3} = @Warehouse";

				dynBizObjCollection.Load(
					String.Format(CultureInfo.InvariantCulture,
					rawQuery,
					WhsDocketSchema.Constants.TableName,
					WhsDocketSchema.WD_OH_Client.Name,
					WhsDocketSchema.WD_FinalisedDate.Name,
					WhsDocketSchema.WD_WW_Whs.Name),
					@params);

				var finalisedDate = (ZDateTimeOffset)dynBizObjCollection[0]["ToDate"];
				if (!finalisedDate.IsEmpty)
				{
					result = finalisedDate.Date;
				}
			}

			return result;
		}

		#endregion

		#region GetJobNumberString

		string GetJobNumberString(IReadOnlyCollection<Job> jobs)
		{
			string result = "";
			if (jobs != null && jobs.Count > 0)
			{
				foreach (Job job in jobs.OrderBy(j => j.JH_JobNum))
				{
					result += "   " + job.JH_JobNum + "\r\n";
				}
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

		#endregion

		#region IAutoRatingCreateJobHeaderPromptOverride

		bool IAutoRatingSuppressJobCreationPrompt.SuppressJobCreationPrompt => true;

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsInvoiceRatingAdaptersProvider(this); }
		}

		#endregion
	}

	#region Document Supporter

	public class WhsInvoiceDocumentSupporter : DocumentSupporter
	{
		public WhsInvoiceDocumentSupporter(WhsInvoice invoice)
			: base(invoice)
		{
		}

		protected WhsInvoice Invoice
		{
			get { return (WhsInvoice)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsPeriodicBilling; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsInvoicingCustomiseDocuments;

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Invoice);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return null;
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}

	#endregion
}
