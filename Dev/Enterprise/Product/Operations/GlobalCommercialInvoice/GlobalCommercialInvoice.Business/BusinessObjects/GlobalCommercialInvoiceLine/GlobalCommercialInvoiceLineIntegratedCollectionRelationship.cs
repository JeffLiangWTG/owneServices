using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	public sealed class GlobalCommercialInvoiceLineIntegratedCollectionRelationship : CollectionRelationship
	{
		protected override ZQuery RelationshipFilterCore => BuildRelationshipFilter();

		readonly GlobalCommercialInvoiceHeaderCollection headers;

		public GlobalCommercialInvoiceLineIntegratedCollectionRelationship(GlobalCommercialInvoiceHeaderCollection headers)
			: base(typeof(GlobalCommercialInvoiceLine))
		{
			this.headers = headers;
			this.headers.CountChanged += (_, e) => OnRelationshipFilterChanged(e);
		}

		ZQuery BuildRelationshipFilter()
		{
			if (headers.Count > 0)
			{
				return new ZQuery(GlobalCommercialInvoiceLineSchema.GIL_GIH_Header, headers.Select((x) => x.PK));
			}

			return ZQuery.NoResultQuery;
		}
	}
}
