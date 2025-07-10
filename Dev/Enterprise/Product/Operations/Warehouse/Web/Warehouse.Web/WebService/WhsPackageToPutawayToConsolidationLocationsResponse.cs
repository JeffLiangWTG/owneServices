using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPackageToPutawayToConsolidationLocationsResponse : WebServiceResponse
	{
		public WhsPackageToPutawayToConsolidationLocationsResponse()
		{
			PackagesToConsolidationLocationGroupingInfos = Array.Empty<ConsolidationLocationToPackagesGroupingInfo>();
			RequiresDockDoorPutaway = false;
		}

		public ConsolidationLocationToPackagesGroupingInfo[] PackagesToConsolidationLocationGroupingInfos { get; set; }
		public bool RequiresDockDoorPutaway { get; set; }
	}
}
