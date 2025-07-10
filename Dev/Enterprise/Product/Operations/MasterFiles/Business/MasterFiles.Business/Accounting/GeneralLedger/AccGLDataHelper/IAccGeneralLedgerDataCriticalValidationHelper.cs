namespace Enterprise.MasterFiles.Business.Accounting
{
	public interface IAccGeneralLedgerDataCriticalValidationHelper
	{
		bool IsGLJournalEntriesNumberHasBeenAssigned(AccTransactionHeader target);
	}
}
