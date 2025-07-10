using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class PeriodicInvoicingHelper : IPeriodicInvoicingHelper
	{
		#region Contractor

		public PeriodicInvoicingHelper()
		{
		}

		#endregion

		#region GetByPK

		public PeriodicInvoicing GetByPK(ZGuid pk) => Factory.Load<PeriodicInvoicing>(pk);

		#endregion

		#region GetQueuedInvoiceValidToProcessPKs

		public IEnumerable<ZGuid> GetQueuedInvoiceValidToProcessPKs()
		{
			var sql = $@"
select
	ET_PK
from
	dbo.JobStorage
where
	ET_OffBandProcessingStatus = '{StorageOffBandProcessingStatus.Codes.QUE}' and
	ET_StorageToDate < @Today";

			var factory = new BusinessObjectFactory();

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Today", ZDateTime.Today, JobStorageSchema.ET_StorageToDate);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, sqlParams);
			return collection.Select(g => (ZGuid)g[JobStorageSchema.Constants.PK]);
		}

		#endregion

		#region LoadInNewFactory

		public PeriodicInvoicing LoadInNewFactory(ZGuid pk) => new BusinessObjectFactory().Load<PeriodicInvoicing>(pk);

		#endregion

		#region IsStillInQueueInDB

		public bool IsStillInQueueInDB(ZGuid pk)
		{
			var query = new ZQuery(JobStorageSchema.ET_OffBandProcessingStatus, StorageOffBandProcessingStatus.Codes.QUE);
			return Factory.ExistsInDatabase(JobStorageSchema.Constants.TableName, query);
		}

		#endregion

		#region InvoiceSaveSafe

		public InvoiceSaveSafeResult InvoiceSaveSafe(PeriodicInvoicing invoice, ILogger logger, string additionalLogMessageWhenHasError)
		{
			var result = InvoiceSaveSafeResult.SaveSuccessful;
			var factory = invoice.Factory;
			try
			{
				using (ProcessTask.Loader.SuppressTemplateApplication())
				{
					factory.Save();
				}
			}
			catch (ZCannotSaveException ex)
			{
				result = InvoiceSaveSafeResult.SaveFailedRetry;
				LogErrorAndDisposeLoadedJobHeaders(factory, logger, ex.Message, additionalLogMessageWhenHasError);
			}
			catch (ZSaveException ex)
			{
				result = InvoiceSaveSafeResult.SaveFailedRetry;
				LogErrorAndDisposeLoadedJobHeaders(factory, logger, ex.Message, additionalLogMessageWhenHasError);
			}
			catch (OnSavingCriticalCheckException ex)
			{
				result = InvoiceSaveSafeResult.SaveFailedNoRetry;
				LogErrorAndDisposeLoadedJobHeaders(factory, logger, ex.Message, additionalLogMessageWhenHasError);
			}
			return result;
		}

		public InvoiceSaveSafeResult InvoiceSaveSafe(PeriodicInvoicing invoice, ILogger logger, int attempt)
		{
			return InvoiceSaveSafe(invoice, logger, AttemptToStrConvert(attempt));
		}

		void LogErrorAndDisposeLoadedJobHeaders(BusinessObjectFactory factory, ILogger logger, params string[] messages)
		{
			var msg = new ZStringBuilder(messages);
			logger.Error(msg.ToStringWithNewLineBetweenAppends());
			DisposeLoadedJobHeaders(factory);
		}

		#endregion

		#region DisposeLoadedJobHeaders

		public void DisposeLoadedJobHeaders(BusinessObjectFactory factory) => DisposeLoadedJobHeadersCore(factory);

		public static void DisposeLoadedJobHeadersCore(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var loadedJobs = factory.Load<IJobHeader>(query);

			loadedJobs.ForEach(job => job.Dispose());
		}

		#endregion

		#region AutoRateJobHeader

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public bool AutoRateJobHeader(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice)
		{
			var isValid = false;
			invoice.AutoRateJobHeader(null);
			invoice.RunPreSaveValidation();
			if (invoice.HasErrors)
			{
				logger.Error(Res.GetString("8697d098-2d8d-4089-ae59-d8b9121f1e21",
					"Invoice for client {0} warehouse/facility {1} could not be auto rated due to following errors: {2}",
					invoice.ClientName,
					invoice.WarehouseName,
					invoice.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString()));
			}
			else if (!invoice.HasCharges)
			{
				logger.Information(Res.GetString("0ded9311-5c66-45fb-80b8-7ff23929f3cc",
					"Invoice does not have charges for client {0} warehouse/facility {1}.", invoice.ClientName, invoice.WarehouseName));
			}
			else
			{
				invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
				logger.Information(Res.GetString("e482b040-dde2-409c-be04-2a455388f8ff",
					"Autorated Invoice for client {0} warehouse/facility {1}.", invoice.ClientName, invoice.WarehouseName));
				isValid = true;
			}

			return isValid;
		}

		#endregion

		#region PostInvoice

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public bool PostInvoice(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice)
		{
			var isValid = false;
			if (invoice.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated)
			{
				throw new InvalidOperationException("When status isn't autorated, it shouldn't be called.");
			}

			if (invoice.Client.CompanyData.OB_WhsAutoPostPeriodicInvoice)
			{
				var prepostValidation = CreatePostManagerValidation(new[] { invoice.JobHeader });
				var validationResult = prepostValidation.Validate();
				if (validationResult != null && validationResult.Type == CargoWise.ComponentModel.NotificationType.Error)
				{
					logger.Error(Res.GetString("2bb33d82-9615-4448-95ed-6fbeea2b37fc",
						"Invoice for client {0} warehouse/facility {1} could not be posted due to following errors: {2}",
						invoice.ClientName,
						invoice.WarehouseName,
						validationResult.Message));
				}
				else
				{
					invoice.PostInvoice();
					invoice.RunPreSaveValidation();
					if (invoice.HasErrors)
					{
						logger.Error(Res.GetString("f4e8390e-9f4c-4372-9eca-dc8255eb7fa2",
							"Invoice for client {0} warehouse/facility {1} could not be posted due to following errors: {2}",
							invoice.ClientName,
							invoice.WarehouseName,
							invoice.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString()));
					}
					else
					{
						invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
						logger.Information(Res.GetString("f297054e-b09e-4019-aa2b-2d6cedb845bb",
							"Posted Invoice for client {0} warehouse/facility {1}.",
							invoice.ClientName, invoice.WarehouseName));
						isValid = true;
					}
				}
			}
			else
			{
				logger.Information(Res.GetString("af36c169-e210-4d01-99a7-e372e49b7f80",
					"Automatically Post Periodic Invoices is not enabled for client {0} warehouse/facility {1}.", invoice.ClientName, invoice.WarehouseName));
				isValid = true;
			}

			return isValid;
		}

		PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs) => new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);

		#endregion

		#region DeliverInvoice

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public bool DeliverInvoice(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice)
		{
			if (invoice.Client.CompanyData.OB_WhsAutoPostPeriodicInvoice && invoice.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationPosted)
			{
				throw new InvalidOperationException("When status isn't posted, it shouldn't be called.");
			}

			if (invoice.Client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice)
			{
				var postedInvoices = invoice.Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, invoice.JobHeader.PK.ToGuid())).Cast<InvoicingBase>().ToArray();
				if (postedInvoices.Length > 0)
				{
					using (var printTask = new PeriodicInvoicingPrintTask(postedInvoices, invoice.Factory))
					{
						printTask.RunSilentlyEmailOnly(false);
					}

					invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationDelivered;
					logger.Information(Res.GetString("4497029c-79cf-4055-8689-7b5a83b2c0ab",
						"Delivered Invoice for client {0} warehouse/facility {1}.",
						invoice.ClientName, invoice.WarehouseName));
				}
				else
				{
					logger.Information(Res.GetString("86b5822d-0a83-47b0-9989-74403414f0ea",
						"There are no Posted Periodic Invoices to Deliver for client {0} warehouse/facility {1}.", invoice.ClientName, invoice.WarehouseName));
				}
			}
			else
			{
				logger.Information(Res.GetString("e90be9d0-31a6-4f85-b15c-50c4546bd6d6",
					"Automatically Deliver Periodic Invoices is not enabled for client {0} warehouse/facility {1}.", invoice.ClientName, invoice.WarehouseName));
			}
			return true;
		}

		#endregion

		#region SetUserContextForInvoice

		public static IDisposable SetUserContextForInvoice(PeriodicInvoicing invoice)
		{
			return WarehouseUserContextHelper.SetUserContextForWarehouse(invoice.Warehouse);
		}

		#endregion

		#region GetInvoiceReferNumber

		public string GetInvoiceReferNumber(PeriodicInvoicing invoice) => $"[{invoice.HumanReadableName}]";

		#endregion

		#region GetInvoiceBillingAutomationMutex

		public IZGlobalMutex GetInvoiceBillingAutomationMutex(ZGuid pK) => new ZGlobalMutex(MutexIDs.WhsInvoiceAutoRate, pK.ToString());

		#endregion

		#region IsNotLockByOtherProcess

		/// <summary>
		/// This method is only used in the PeriodicInvoicingInvoicingSupporter to give a Reason to not autorate if the Periodic Invoicing is in progress.
		/// This is to prevent Autorating from the Job Invoicing module. 
		/// </summary>
		public bool IsNotLockByOtherProcess(ZGuid pK)
		{
			using (var mutex = GetInvoiceBillingAutomationMutex(pK))
			{
				return mutex.IsLocked ? (bool)mutex.HasLock : mutex.Lock();
			}
		}

		#endregion

		#region AttemptToStrConvert

		public string AttemptToStrConvert(int attemptNumber) => AttemptToStrConvertCore(attemptNumber);

		static string AttemptToStrConvertCore(int attemptNumber)
		{
			switch (attemptNumber)
			{
				case 1:
					return (NoResString)"(First attempt).";
				case 2:
					return (NoResString)"(Second attempt).";
				case 3:
					return (NoResString)"(When attempting to change status to error).";
				default:
					throw new ArgumentException("We expected to call method always between 1 to 3.");
			}
		}

		#endregion

		#region GetOldestUnpostedInvoice

		public PeriodicInvoicing GetOldestUnpostedInvoice(BusinessObjectFactory factory, OrgWarehousePair orgWarehousePair)
		{
			var query = GetOldestUnpostedInvoiceQuery(orgWarehousePair);

			return factory.LoadTop1<PeriodicInvoicing>(query);
		}

		public bool OldestUnpostedInvoiceExists(OrgWarehousePair orgWarehousePair)
		{
			var query = GetOldestUnpostedInvoiceQuery(orgWarehousePair);

			return Factory.Exists(typeof(PeriodicInvoicing), query);
		}

		static ZDBOnlyQuery GetOldestUnpostedInvoiceQuery(OrgWarehousePair orgWarehousePair)
		{
			// job header (billing job)
			var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.NotEqual, JobHeaderStatus.Closed.Code);

			// where JobHeader is not posted 
			AddJobHeaderHasNoAccHeadersSubQuery(jobHeaderQuery, LedgerTypes.AccountsReceivable);
			AddJobHeaderHasNoAccLinesSubQuery(jobHeaderQuery, LedgerTypes.AccountsReceivable);
			AddJobHeaderHasNoAccHeadersSubQuery(jobHeaderQuery, LedgerTypes.AccountsPayable);
			AddJobHeaderHasNoAccLinesSubQuery(jobHeaderQuery, LedgerTypes.AccountsPayable);

			// periodic job
			var query = new ZDBOnlyQuery(typeof(JobStorage));
			query.AddToFilter(JobStorageSchema.ET_OH_Client, orgWarehousePair.OrgPK);
			query.AddToFilter(JobStorageSchema.ET_WW, orgWarehousePair.WarehousePK);
			query.OrderBy = JobStorageSchema.ET_StorageFromDate.Name; // to find oldest unposted
			query.AddSubQuery(jobHeaderQuery, JoinCondition.And);
			return query;
		}

		static void AddJobHeaderHasNoAccHeadersSubQuery(ZDBOnlySubQuery jobHeaderQuery, string ledger)
		{
			var accHeaderQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_JH, true);
			AddToAccHeaderFilter(accHeaderQuery, ledger);
			jobHeaderQuery.AddSubQuery(accHeaderQuery, JoinCondition.And);
		}

		static void AddJobHeaderHasNoAccLinesSubQuery(ZDBOnlySubQuery jobHeaderQuery, string ledger)
		{
			var accHeaderQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			AddToAccHeaderFilter(accHeaderQuery, ledger);
			var accLinesQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH, true);
			accLinesQuery.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, DBNull.Value);
			accLinesQuery.AddSubQuery(AccTransactionLinesSchema.AL_AH, accHeaderQuery, JoinCondition.And);
			jobHeaderQuery.AddSubQuery(accLinesQuery, JoinCondition.And);
		}

		static void AddToAccHeaderFilter(ZDBOnlySubQuery accHeaderQuery, string ledger)
		{
			accHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			accHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			accHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new[] { "INV", "CRD", "ADJ" });
		}

		#endregion

		#region GetCandidateInvoices

		public IEnumerable<OrgWarehousePair> GetCandidateInvoices() => GetCandidateInvoices(Factory, true);

		public IEnumerable<OrgWarehousePair> GetCandidateInvoices(BusinessObjectFactory factory, bool isAutoCreate)
		{
			string joinCompanyAndActiveClient;
			if (isAutoCreate)
			{
				joinCompanyAndActiveClient = @"
			join dbo.WhsWarehouse on WarehousePK = WW_PK
			join dbo.GlbBranch on WW_GB_RelatedCompanyBranch = GB_PK
			join dbo.OrgCompanyData on OB_OH = ClientPK and GB_GC = OB_GC and OB_WhsAutoCreateAndRatePeriodicInvoice = 1
			join dbo.OrgHeader on OH_PK = ClientPK
		where
			OH_IsActive = 1
";
			}
			else
			{
				joinCompanyAndActiveClient = @"join dbo.OrgHeader on OH_PK = ClientPK
	where
		OH_IsActive = 1
;";
			}

			string sQL = $@"
select
	ET_OH_Client
	,ET_WW
	,convert(date, max(ET_StorageToDate)) LastStorageDate
into #Invoices
from
	dbo.JobStorage
	join dbo.JobHeader on ET_PK = JH_ParentID and JH_Status in ('CLS', 'CMP')
	join dbo.OrgHeader on OH_PK = ET_OH_Client
where
	OH_IsActive = 1
group by
	ET_OH_Client,
	ET_WW

DECLARE @MinDate date = (SELECT MIN(LastStorageDate) FROM #Invoices)

select
    WD_OH_Client,
    WD_WW_Whs,
    convert(date, max(WE_FinalisedDate)) as WE_FinalisedDate
into #DocketLines
from
    dbo.WhsDocket
    join #Invoices on WD_OH_Client = ET_OH_Client and WD_WW_Whs = ET_WW
    join dbo.WhsDocketLine on WD_PK = WE_WD and WE_FinalisedDate IS NOT NULL and WE_FinalisedDate > @MinDate
group by
	WD_OH_Client,
	WD_WW_Whs

create clustered index TempIndex on #DocketLines (WE_FinalisedDate)

select
	ClientPK,
	WarehousePK,
	cast((case when exists (select null from dbo.JobStorage where ET_OH_Client = ClientPK and ET_WW = WarehousePK) then 1 else 0 end) as bit) HasInvoice
from
	(
		select distinct
			WSJ_OH_Client as ClientPK,
			WSJ_WW_Whs as WarehousePK
		from
			dbo.WhsAdHocServiceJob
			join dbo.JobStorage on ET_WW = WSJ_WW_Whs and ET_OH_Client = WSJ_OH_Client
		where
			WSJ_BillingDate > CONVERT(date, ET_StorageToDate) and
			WSJ_BillingDate <= getdate() and
			WSJ_IsFinalised = 1

		union

		select distinct
			WSJ_OH_Client as ClientPK,
			WSJ_WW_Whs as WarehousePK
		from
			dbo.WhsAdHocServiceJob
		where
			WSJ_BillingDate <= getdate() and
			WSJ_IsFinalised = 1 and
			not exists
			(
				select
					null
				from
					dbo.JobStorage 
				where
					ET_WW = WSJ_WW_Whs and
					ET_OH_Client = WSJ_OH_Client
			)

		union

		{(isAutoCreate ? $@"
		select
			WD_OH_Client as ClientPK,
			WW_PK as WarehousePK
		from
			dbo.WhsDocket
			join dbo.WhsWarehouse on WW_PK = WW_PK
			join dbo.GlbBranch on WW_GB_RelatedCompanyBranch = GB_PK
			join dbo.OrgCompanyData on GB_GC = OB_GC and OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = 1 and OB_OH = WD_OH_Client

		union
			" : "")}
		select distinct
            WD_OH_Client,
            WD_WW_Whs
        from
            #DocketLines
            join #Invoices on WD_OH_Client = ET_OH_Client and
				WD_WW_Whs = ET_WW and
				WE_FinalisedDate > @MinDate and
				WE_FinalisedDate > COALESCE(LastStorageDate, WE_FinalisedDate)

		union

		select distinct
			WD_OH_Client as ClientPK,
			WD_WW_Whs as WarehousePK
		from
			dbo.WhsDocket
			join dbo.WhsDocketLine on WD_PK = WE_WD and
				WE_FinalisedDate IS NOT NULL and
				not exists
				(
					select
						null
					from
						#Invoices
					where
						WD_OH_Client = ET_OH_Client and
						WD_WW_Whs = ET_WW
				)

		union
		
		select distinct
			WD_OH_Client as ClientPK,
			WD_WW_Whs as WarehousePK
		from
			dbo.WhsDocket
			join dbo.WhsDocketLine on WD_PK = WE_WD and WE_DocketLineStatus = 'FIN'
		where
			WE_StockOnHand > 0

	) result
	{joinCompanyAndActiveClient}

DROP TABLE #Invoices;
DROP TABLE #DocketLines;
";

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);

			dynamicCollection.Load(sQL);
			foreach (var row in dynamicCollection.Cast<DynamicBusinessObject>())
			{
				var pair = new OrgWarehousePair
				{
					OrgPK = (ZGuid)row["ClientPK"],
					WarehousePK = (ZGuid)row["WarehousePK"],
					HasInvoice = isAutoCreate && (ZBool)row["HasInvoice"]
				};
				yield return pair;
			}
		}

		#endregion

		#region CreateInvoiceFromOrgWarehousePair

		public PeriodicInvoicing CreateInvoiceFromOrgWarehousePair(OrgWarehousePair pair)
		{
			var factory = new BusinessObjectFactory();
			var invoice = factory.New<PeriodicInvoicing>();
			invoice.ET_OH_Client = pair.OrgPK;
			invoice.ET_WW = pair.WarehousePK;
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.ET_BillingDate = ZDateTime.Today;
			invoice.TryCreateJobHeader();

			return invoice;
		}

		#endregion

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public bool CloseBillingAndRelatedJob(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice)
		{
			var closeApplied = false;
			if (invoice.Client?.CompanyData.OB_WhsAutoPostPeriodicInvoice ?? false)
			{
				if (invoice.JobHeader == null)
				{
					throw new InvalidOperationException("shouldn't be called when there is no job to close.");
				}
				else if (invoice.JobHeader.JH_Status != JobHeaderStatus.Closed.Code)
				{
					invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
					invoice.RunPreSaveValidation();
					if (invoice.HasErrors)
					{
						logger.Error(Res.GetString("b099e6c6-3cba-4302-be33-7cffccdcaf7c",
							"Invoice for client {0} warehouse/facility {1} could not be closed. it must be manually closed due to following errors: {2}",
								invoice.ClientName,
								invoice.WarehouseName,
								invoice.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString()));
					}
					else
					{
						logger.Information(Res.GetString("6ba2ddd0-af8d-45f0-9edf-aa971dc0cac4",
							"Periodic Billing job status for client {0} warehouse/facility {1} was successfully closed.",
							invoice.ClientName,
							invoice.WarehouseName));
						closeApplied = true;
					}
				}
			}
			return closeApplied;
		}

		#region Factory

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;

		#endregion
	}
}
