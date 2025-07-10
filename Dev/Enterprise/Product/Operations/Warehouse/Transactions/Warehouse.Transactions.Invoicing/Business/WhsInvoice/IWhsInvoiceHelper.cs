using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	#region IWhsInvoiceHelper

	public interface IWhsInvoiceHelper
	{
		WhsInvoice GetByPK(ZGuid pk);
		IEnumerable<ZGuid> GetQueuedInvoiceValidToProcessPKs();
		WhsInvoice LoadInNewFactory(ZGuid pk);
		bool IsStillInQueueInDB(ZGuid pK);
		bool AutoRateJobHeader(IAutoRatingServiceLogger logger, WhsInvoice invoice);
		bool PostInvoice(IAutoRatingServiceLogger logger, WhsInvoice invoice);
		bool DeliverInvoice(IAutoRatingServiceLogger logger, WhsInvoice invoice);
		IZGlobalMutex GetInvoiceBillingAutomationMutex(ZGuid pk);
		InvoiceSaveSafeResult InvoiceSaveSafe(WhsInvoice invoice, ILogger logger, int attempt);
		string AttemptToStrConvert(int attemptNumber);
		WhsInvoice GetOldestUnpostedInvoice(BusinessObjectFactory factory, OrgWarehousePair orgWarehousePair);
		bool OldestUnpostedInvoiceExists(OrgWarehousePair orgWarehousePair);
		IEnumerable<OrgWarehousePair> GetCandidateInvoices();
		IEnumerable<OrgWarehousePair> GetCandidateInvoices(BusinessObjectFactory factory, bool isAutoCreate);
		WhsInvoice CreateInvoiceFromOrgWarehousePair(OrgWarehousePair pair);
		string GetInvoiceReferNumber(WhsInvoice invoice);
		bool IsNotLockByOtherProcess(ZGuid pk);
		void DisposeLoadedJobHeaders(BusinessObjectFactory factory);
		bool CloseBillingAndRelatedJob(IAutoRatingServiceLogger logger, WhsInvoice invoice);
	}

	#endregion

	#region InvoiceSaveSafeResult

	public enum InvoiceSaveSafeResult
	{
		SaveSuccessful,
		SaveFailedRetry,
		SaveFailedNoRetry,
	}

	#endregion
}
