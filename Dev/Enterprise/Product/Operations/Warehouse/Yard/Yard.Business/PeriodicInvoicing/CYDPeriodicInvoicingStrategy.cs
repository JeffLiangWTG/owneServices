using CargoWise.Types;
using Enterprise.Warehouse.Invoicing.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPeriodicInvoicingStrategy : IPeriodicInvoicingStrategy
	{
		public ZString GetHumanReadableName(ZString jobNumber)
		{
			return jobNumber.IsEmpty
				? Res.GetString("17191c19-be00-4570-9798-735635e21b37", "Yard Periodic Invoicing")
				: Res.GetString("f3741994-e55a-4367-b1b3-57cd1dc2d134", "Yard Periodic Invoicing {0}", jobNumber);
		}

		public PeriodicInvoicingDocumentSupporter GetPeriodicInvoicingDocumentSupporter(PeriodicInvoicing periodicInvoicing)
		{
			return new CYDPeriodicInvoicingDocumentSupporter(periodicInvoicing);
		}

		public PeriodicInvoicingInvoicingSupporter GetPeriodicInvoicingInvoicingSupporter(PeriodicInvoicing periodicInvoicing)
		{
			return new CYDPeriodicInvoicingInvoicingSupporter(periodicInvoicing);
		}

		public PeriodicInvoicingDocManagerInfo GetPeriodicInvoicingDocManagerInfo(PeriodicInvoicing periodicInvoicing)
		{
			return new PeriodicInvoicingDocManagerInfo(periodicInvoicing, DocManagerCodes.WarehouseInvoice);
		}

		public MasterFiles.Business.RatingAdaptersProvider GetRatingAdaptersProvider(PeriodicInvoicing periodicInvoicing)
		{
			return new CYDYardStorageRatingAdaptersProvider(periodicInvoicing, new CYDPeriodicInvoicingYardUnits(periodicInvoicing));
		}

		public void DeleteStorageLines(PeriodicInvoicing periodicInvoicing)
		{
			var storageLines = new CYDPeriodicInvoicingStorageLinesCollection(periodicInvoicing);
			storageLines.DeleteAll();
		}
	}
}
