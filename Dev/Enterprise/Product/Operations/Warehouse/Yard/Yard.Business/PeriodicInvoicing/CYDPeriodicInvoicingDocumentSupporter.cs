using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPeriodicInvoicingDocumentSupporter(PeriodicInvoicing invoice) : PeriodicInvoicingDocumentSupporter(invoice)
	{
		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CYDPeriodicInvoicingCustomiseDocuments;

		public override BusinessContext BusinessContext => BusinessContext.CYDPeriodicInvoicing;
	}
}
