using CargoWise.EntityFramework;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;

namespace Enterprise.Freight.Business
{
	public class RoutingLegsOrderHelper : IRoutingLegsOrderHelper
	{
		public void InitializeFrom(BusinessObject parentWithTransports)
		{
			var orderHelper = CreateTransportOrderHelper(parentWithTransports);

			FirstLeg = orderHelper.FirstLeg;
			LastLeg = orderHelper.LastLeg;
		}

		TransportOrderHelper CreateTransportOrderHelper(BusinessObject parentWithTransports)
		{
			if (parentWithTransports is IRoutingSupport routingSupport)
			{
				return new TransportOrderHelper(routingSupport.TransportsIncludingRelated);
			}
			else if (parentWithTransports is ITransportParentCommon standaloneTransportParent)
			{
				var transportCollection = new TransportCollection(standaloneTransportParent);
				transportCollection.Load();

				return new TransportOrderHelper(transportCollection);
			}

			return new TransportOrderHelper(System.Array.Empty<Transport>());
		}

		public ITransport FirstLeg { get; private set; }

		public ITransport LastLeg { get; private set; }
	}
}
