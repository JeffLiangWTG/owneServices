using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingTransportCollection : BusinessObjectCollection<Transport>
	{
		public ForwardingTransportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ForwardingTransportCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
