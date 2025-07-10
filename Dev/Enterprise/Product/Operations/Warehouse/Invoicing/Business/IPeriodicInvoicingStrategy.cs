using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public interface IPeriodicInvoicingStrategy
	{
		ZString GetHumanReadableName(ZString jobNumber);

		PeriodicInvoicingDocumentSupporter GetPeriodicInvoicingDocumentSupporter(PeriodicInvoicing periodicInvoicing);

		PeriodicInvoicingInvoicingSupporter GetPeriodicInvoicingInvoicingSupporter(PeriodicInvoicing periodicInvoicing);

		PeriodicInvoicingDocManagerInfo GetPeriodicInvoicingDocManagerInfo(PeriodicInvoicing periodicInvoicing);

		RatingAdaptersProvider GetRatingAdaptersProvider(PeriodicInvoicing periodicInvoicing);

		void DeleteStorageLines(PeriodicInvoicing periodicInvoicing);
	}
}
