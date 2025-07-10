using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PutawayStockInDockDoorOrPackingStationResponseTestCase : WebServiceResponseTestCase
	{
		public void TestNewProperties()
		{
			var response = new PutawayStockInDockDoorOrPackingStationResponse();
			AssertNull(response.ExpectedLocationString);
			AssertNull(response.ExpectedLocationString_UserFriendly);
			AssertNull(response.ExpectedLocationClass);
			AssertEquals(Guid.Empty, response.ExpectedLocationPK);

			var testPK = Guid.NewGuid();
			response.ExpectedLocationPK = testPK;
			response.ExpectedLocationString = "ABC-1-1-1";
			response.ExpectedLocationString_UserFriendly = "ABC";
			response.ExpectedLocationClass = "PST";

			AssertEquals(nameof(response.ExpectedLocationPK), testPK, response.ExpectedLocationPK);
			AssertEquals(nameof(response.ExpectedLocationString), "ABC-1-1-1", response.ExpectedLocationString);
			AssertEquals(nameof(response.ExpectedLocationString_UserFriendly), "ABC", response.ExpectedLocationString_UserFriendly);
			AssertEquals(nameof(response.ExpectedLocationClass), "PST", response.ExpectedLocationClass);
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse() => new PutawayStockInDockDoorOrPackingStationResponse();

		#endregion
	}
}
