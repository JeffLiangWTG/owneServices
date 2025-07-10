using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceExRateCollection : DependentBusinessObjectCollection<AccDraftInvoiceExRate, AccDraftInvoiceHeader>
	{
		public AccDraftInvoiceExRateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccDraftInvoiceExRateCollection(AccDraftInvoiceHeader parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => AccDraftInvoiceExRateSchema.AIE_AIH_Header;
	}
}
