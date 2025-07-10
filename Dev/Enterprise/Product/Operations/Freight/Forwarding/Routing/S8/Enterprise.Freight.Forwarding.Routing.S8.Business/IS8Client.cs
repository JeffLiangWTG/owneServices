using System;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public interface IS8Client : IDisposable
	{
		MethodCallResult<FlightInformation> GetFlight(FlightRequest request);
		MethodCallResult<string> SolveRouting(RoutingRequest request);
	}
}
