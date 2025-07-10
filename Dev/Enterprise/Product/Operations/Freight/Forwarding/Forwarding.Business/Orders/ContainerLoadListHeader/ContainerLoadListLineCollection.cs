using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class ContainerLoadListLineCollection : ActiveBusinessObjectCollection<ContainerLoadListLine>
	{
		public ContainerLoadListLineCollection(CommonContainerLoadList header)
			: base(header.Factory, header, null, ContainerLoadListLineSchema.CLL_CLH_LoadListHeader)
		{
		}

		public ContainerLoadListLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
