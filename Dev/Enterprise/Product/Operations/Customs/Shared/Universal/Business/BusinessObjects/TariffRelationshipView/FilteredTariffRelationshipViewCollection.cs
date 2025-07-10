using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class FilteredTariffRelationshipViewCollection : TariffRelationshipViewCollection
	{
		public FilteredTariffRelationshipViewCollection(TariffView tariffView) : base(tariffView)
		{
		}

		protected FilteredTariffRelationshipViewCollection(TariffView tariffView, ZQuery filter)
			: base(tariffView, filter, true)
		{
		}

		public static new FilteredTariffRelationshipViewCollection NewChildTariffRelationshipCollection(TariffView tariffView)
		{
			var query = NewChildTariffRelationshipCollectionQuery(tariffView);
			return new FilteredTariffRelationshipViewCollection(tariffView, query);
		}
	}
}
