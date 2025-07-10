using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IInvoicesProvider : Customs.Business.IInvoicesProvider
	{
		ZBool ShouldElectronicInvoicesBeVisible { get; }
		bool UseScheduleB { get; }
		bool IsExWarehouse { get; }
		bool IsImportByExternalBroker { get; }
		bool IsLineGroupingSupported { get; }
		bool IsInwardBondedWarehousingEnabled { get; }
		bool IsOutwardBondedWarehousingEnabled { get; }
		InvoiceHeaderFilteredActiveCollection FilteredInvoices { get; }
		void AddDefaultInvoice();
		ZBool IsImport { get; }
		new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines { get; }
		ZGuid SelectedOriginalEntry { get; }
		bool IsConsumptionFTZ { get; }
		JobComInvoiceGroupHeader TopGroupInvoice { get; }
	}
}
