using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class InvoiceCreationTestHelper
	{
		public InvoiceCreationTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;
		public AccTransactionHeader SetupTransaction(GlbBranch branch, ZString transactionNumber, OrgHeader org, ZString consolidatedInvRef, ZString ledger, ZString transactionType, JobHeader job = null)
		{
			AccTransactionHeader result = Factory.New<AccTransactionHeader>();
			result.AH_Ledger = ledger;
			result.AH_TransactionType = transactionType;
			if (job != null)
			{
				result.AH_JH = job.PK;
			}
			result.AH_GB = (branch != null) ? branch.PK : ZGuid.Empty;
			result.AH_TransactionNum = transactionNumber;
			result.AH_OH = (org != null) ? org.PK : ZGuid.Empty;
			result.AH_ConsolidatedInvoiceRef = consolidatedInvRef;
			return result;
		}
	}
}
