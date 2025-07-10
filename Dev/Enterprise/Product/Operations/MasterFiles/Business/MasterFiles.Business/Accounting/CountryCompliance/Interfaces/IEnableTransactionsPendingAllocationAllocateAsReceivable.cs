namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IEnableTransactionsPendingAllocationAllocateAsReceivable
	{
		// NOTE: This interface is for enabling Allocate as Receivable functionality in Transactions Pending Allocation module
		//       if relevant Country should be able create AR Credit Note on Allocate Transaction process.

		string GetEligibleComplianceSubTypeForAllocateAsReceivable(string payableComplianceSubType);

		bool IsComplianceSubTypeNotEligibleForAllocateAsReceivable(string complianceSubType);
	}
}
