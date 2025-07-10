using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingGUIActionQueuePendingInvoice
	{
		ZString QueuePendingInvoiceMenuName { get; }

		ZString[] PivotStatusesEligibleForQueuing { get; }

		ZString PivotStatusesEligibleForQueuingErrorMessage { get; }
	}
}
