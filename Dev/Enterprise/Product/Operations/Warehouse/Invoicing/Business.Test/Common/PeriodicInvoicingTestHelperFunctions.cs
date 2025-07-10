using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	public class PeriodicInvoicingTestHelperFunctions(BusinessObjectFactory factory) : WhsTestHelperFunctionsEnv(factory)
	{
		public Job CreateAccountingDataWithNoCharge(IJobInvoicingPlugIn job)
		{
			Job jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = job.PK;
			jobHeader.JH_ParentTableCode = ((job as WhsDocket) != null) ? WhsDocketSchema.Constants.Prefix : JobStorageSchema.Constants.Prefix;

			AccTransactionHeader aRHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			aRHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			aRHeader.AH_TransactionType = TransactionTypes.Invoice;
			aRHeader.AH_JH = jobHeader.PK;

			return jobHeader;
		}
	}
}
