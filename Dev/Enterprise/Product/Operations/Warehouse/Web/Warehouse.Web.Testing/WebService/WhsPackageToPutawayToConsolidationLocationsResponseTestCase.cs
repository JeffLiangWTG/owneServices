using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsPackageToPutawayToConsolidationLocationsResponseTestCase : WebServiceResponseTestCase
	{
		#region Test Cases

		public void TestWhsPackageToPutawayToConsolidationLocationsResponse()
		{
			var response = new WhsPackageToPutawayToConsolidationLocationsResponse();
			AssertNotNull(response.PackagesToConsolidationLocationGroupingInfos);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ConsolidationLocationToPackagesGroupingInfo>(), response.PackagesToConsolidationLocationGroupingInfos);
			AssertEquals(false, response.RequiresDockDoorPutaway);

			var info = new ConsolidationLocationToPackagesGroupingInfo();
			response.PackagesToConsolidationLocationGroupingInfos = new[] { info };
			AssertContainsExactElementsInAnyOrder(new[] { info }, response.PackagesToConsolidationLocationGroupingInfos);

			response.RequiresDockDoorPutaway = true;
			AssertEquals(true, response.RequiresDockDoorPutaway);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsPackageToPutawayToConsolidationLocationsResponse();
		}

		protected new WhsPackageToPutawayToConsolidationLocationsResponse Response
		{
			get
			{
				return (WhsPackageToPutawayToConsolidationLocationsResponse)base.Response;
			}
		}

		#endregion
	}
}
