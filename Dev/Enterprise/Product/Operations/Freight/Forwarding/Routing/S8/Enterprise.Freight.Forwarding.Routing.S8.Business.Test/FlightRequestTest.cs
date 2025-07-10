using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class FlightRequestTest : TestCase
	{
		public void TestFlightRequestDate()
		{
			var flightRequest = new FlightRequest(new ZDate(2018, 7, 27), "QF", 1);
			AssertEquals("2018/07/27", flightRequest.Date);
		}
	}
}
