namespace Enterprise.MasterFiles.Business
{
	public class GeneralLedgerDataFeatureControlModel
	{
		public bool EnableGLDComplianceReportUsingEDW { get; set; }
		public bool EnableAccountingJournalsModule { get; set; }
		public bool EnableGenerateJournalEntriesForPostedAccountingTransactions { get; set; }
		public bool EnableAJLRJLForCN { get; set; }
	}
}
