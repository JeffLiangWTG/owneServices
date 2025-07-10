using System;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class BeginRFDirectedPackingTaskWebServiceResponseTest : WebServiceResponseTestCase
	{
		public void TestBeginRFDirectedPackingTaskWebServiceResponse()
		{
			var response = new BeginRFDirectedPackingTaskWebServiceResponse();
			Assert("Response should implement appropriate response type.", response is WhsOrdersWebServiceResponse);

			AssertEquals(null, response.PackingStation);
			AssertEquals(Guid.Empty, response.WhsPickPK);

			var packingStationPK = Guid.NewGuid();
			var pickPK = Guid.NewGuid();

			response.PackingStation = new WhsLocationInfo(packingStationPK, "Packing Station", "Packing Station User Friendly", LocationClasses.Codes.PST);
			response.WhsPickPK = pickPK;

			AssertEquals(packingStationPK, response.PackingStation.LocationPK);
			AssertEquals("Packing Station", response.PackingStation.LocationString);
			AssertEquals("Packing Station User Friendly", response.PackingStation.LocationString_UserFriendly);
			AssertEquals(LocationClasses.Codes.PST, response.PackingStation.LocationClass);
			AssertEquals(pickPK, response.WhsPickPK);
		}
	}
}
