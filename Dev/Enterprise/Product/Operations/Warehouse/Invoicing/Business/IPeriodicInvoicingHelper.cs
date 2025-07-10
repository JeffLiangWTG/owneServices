using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public interface IPeriodicInvoicingHelper
	{
		PeriodicInvoicing GetByPK(ZGuid pk);
		IEnumerable<ZGuid> GetQueuedInvoiceValidToProcessPKs();
		PeriodicInvoicing LoadInNewFactory(ZGuid pk);
		bool IsStillInQueueInDB(ZGuid pK);
		bool AutoRateJobHeader(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice);
		bool PostInvoice(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice);
		bool DeliverInvoice(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice);
		IZGlobalMutex GetInvoiceBillingAutomationMutex(ZGuid pk);
		InvoiceSaveSafeResult InvoiceSaveSafe(PeriodicInvoicing invoice, ILogger logger, int attempt);
		string AttemptToStrConvert(int attemptNumber);
		PeriodicInvoicing GetOldestUnpostedInvoice(BusinessObjectFactory factory, OrgWarehousePair orgWarehousePair);
		bool OldestUnpostedInvoiceExists(OrgWarehousePair orgWarehousePair);
		IEnumerable<OrgWarehousePair> GetCandidateInvoices();
		IEnumerable<OrgWarehousePair> GetCandidateInvoices(BusinessObjectFactory factory, bool isAutoCreate);
		PeriodicInvoicing CreateInvoiceFromOrgWarehousePair(OrgWarehousePair pair);
		string GetInvoiceReferNumber(PeriodicInvoicing invoice);
		bool IsNotLockByOtherProcess(ZGuid pk);
		void DisposeLoadedJobHeaders(BusinessObjectFactory factory);
		bool CloseBillingAndRelatedJob(IAutoRatingServiceLogger logger, PeriodicInvoicing invoice);
	}
}
