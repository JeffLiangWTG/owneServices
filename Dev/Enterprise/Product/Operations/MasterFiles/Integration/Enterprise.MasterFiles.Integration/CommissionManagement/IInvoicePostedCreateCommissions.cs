namespace Enterprise.MasterFiles.Integration
{
	public interface IInvoicePostedCreateCommissions
	{
		void PostQueueItemOrCreateCommissions();
		void CreateCommissions();
		ICommissionCreator CommissionCreatorOverride { get; set; }
	}
}
