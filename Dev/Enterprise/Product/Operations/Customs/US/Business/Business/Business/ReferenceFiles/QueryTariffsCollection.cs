using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class QueryTariffsCollection : NonPersistentBusinessObjectCollection<QueryTariff>
	{
		public QueryTariffsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QueryTariff();
		}

		public IReadOnlyList<TariffDate> QueryTariffCollection
		{
			get
			{
				if (queryTariffCollection == null)
				{
					queryTariffCollection = System.Array.Empty<TariffDate>();
				}

				return queryTariffCollection;
			}
		}
		public TariffDate[] queryTariffCollection;
	}
}
