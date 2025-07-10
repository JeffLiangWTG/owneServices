namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingGUIActionQueueReversedTransaction
	{
		bool RejectReQueueForReversedTransaction(string complianceSubType);
	}
}
