namespace Enterprise.MasterFiles.Business
{
	public interface IAccDraftInvoiceStatusChangeValidator
	{
		AccDraftInvoiceStatusUpdateValidationResult CanChangeStatusTo(AccDraftInvoiceHeader accDraftInvoice, string newStatus);
	}
}
