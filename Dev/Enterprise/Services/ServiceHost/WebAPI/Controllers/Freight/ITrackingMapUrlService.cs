using System;
using System.Threading;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	public interface ITrackingMapUrlService
	{
		(Uri url, string errorMessage) GetActiveTransportMapUrl(TransportOrderHelper helper, OrgContact contact = null, CancellationToken ct = default);
		(Uri url, string errorMessage) GetOrderMapUrl(Order order, OrgContact contact = null, CancellationToken ct = default);
	}
}
