using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CommercialInvoiceCollection : ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>, Integration.Customs.ICommercialInvoiceCollection
	{
		public CommercialInvoiceCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, 0);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new FetchStrategies.CommercialInvoiceCollectionFetchStrategy(this);
	}
}
