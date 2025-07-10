using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.DocumentWrappers
{
	[CodeAlive("This is used by reflection code in US.Business.")]
	public class USExportCommercialInvoiceWrapper : CommercialInvoiceWrapper
	{
		public static USExportCommercialInvoiceWrapper New(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory) => new USExportCommercialInvoiceWrapper(invoiceHeader, factory);
		public USExportCommercialInvoiceWrapper(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory) : base(invoiceHeader, factory)
		{
			invoiceHeaderBO = invoiceHeader ?? factory.GetNull<JobComInvoiceHeader>();
		}
		readonly JobComInvoiceHeader invoiceHeaderBO;

		protected override OrganisationWrapper GetSupplier()
		{
			return new OrganisationWrapper(OrganisationUsageType.Supplier, invoiceHeaderBO.USPPIDocAddress, Factory);
		}

		protected override OrganisationWrapper GetImporter()
		{
			return new OrganisationWrapper(OrganisationUsageType.Importer, invoiceHeaderBO.UltimateConsigneeDocAddress, Factory);
		}
	}
}
