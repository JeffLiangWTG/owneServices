using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class IATACommodityCodeCollection : RefAirlineCommodityCodeCollection
	{
		public IATACommodityCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public IATACommodityCodeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(RefAirlineCommodityCodeSchema.RAC_AirlineID, SQLComparisonOperator.Equal, ZString.Empty);
			return filter;
		}
	}
}
