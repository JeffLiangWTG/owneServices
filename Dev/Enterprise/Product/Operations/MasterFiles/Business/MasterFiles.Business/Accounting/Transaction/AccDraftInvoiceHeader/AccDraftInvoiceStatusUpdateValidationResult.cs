namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceStatusUpdateValidationResult
	{
		public bool CanUpdate { get; set; }

		public string[] ValidationErrors { get; set; }
	}
}
