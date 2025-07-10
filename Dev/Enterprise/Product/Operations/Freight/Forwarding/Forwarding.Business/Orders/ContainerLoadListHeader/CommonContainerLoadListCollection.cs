using CargoWise.EntityFramework;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class CommonContainerLoadListCollection : ActiveBusinessObjectCollection<CommonContainerLoadList>, IContainerLoadListHeaderCollection
	{
		public CommonContainerLoadListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommonContainerLoadListCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public CommonContainerLoadListCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
