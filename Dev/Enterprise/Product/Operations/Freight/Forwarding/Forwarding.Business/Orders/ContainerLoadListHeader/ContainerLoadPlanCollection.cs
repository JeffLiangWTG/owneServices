using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadPlanCollection : ActiveBusinessObjectCollection<CFSContainerLoadList>
	{
		public ContainerLoadPlanCollection(BusinessObjectFactory factory)
			: base(factory, GetContainerLoadPlanCollectionQuery())
		{
		}

		public ContainerLoadPlanCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ContainerLoadPlanCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static ZQuery GetContainerLoadPlanCollectionQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(ContainerLoadListHeaderSchema.CLH_LoadMode, CommonContainerLoadListLoadModeList.Codes.CFS);
			return query;
		}
	}
}
