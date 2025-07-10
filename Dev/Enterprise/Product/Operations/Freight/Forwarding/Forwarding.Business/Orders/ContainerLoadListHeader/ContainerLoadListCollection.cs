using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListCollection : ActiveBusinessObjectCollection<CYContainerLoadList>
	{
		public ContainerLoadListCollection(BusinessObjectFactory factory)
			: base(factory, GetContainerLoadListCollectionQuery())
		{
		}

		public ContainerLoadListCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ContainerLoadListCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static ZQuery GetContainerLoadListCollectionQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(ContainerLoadListHeaderSchema.CLH_LoadMode, CommonContainerLoadListLoadModeList.Codes.CY);
			return query;
		}
	}
}
