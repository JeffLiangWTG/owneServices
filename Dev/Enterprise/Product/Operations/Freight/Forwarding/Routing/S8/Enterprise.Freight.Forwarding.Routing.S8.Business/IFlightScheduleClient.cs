using System;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public interface IFlightScheduleClient : IDisposable
	{
		string GetFlight(string token, string sset, string date, string airline, int flight);

		string SolveRouting(string token, string problem);

		string LogInS8C(string userID, string password, string computer, string loginID, string program, string clientVersion);
	}
}
