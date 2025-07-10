using System;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PutawayStockInDockDoorOrPackingStationResponse : WebServiceResponse
	{
		public PutawayStockInDockDoorOrPackingStationResponse()
		{
		}

		public string ExpectedLocationString { get; set; }

		public string ExpectedLocationString_UserFriendly { get; set; }

		public string ExpectedLocationClass { get; set; }

		public Guid ExpectedLocationPK { get; set; }
	}
}
