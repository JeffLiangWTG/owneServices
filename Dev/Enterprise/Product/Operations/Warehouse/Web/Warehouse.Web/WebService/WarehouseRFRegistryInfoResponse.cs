using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WarehouseRFRegistryInfoResponse : WebServiceResponse
	{
		public WarehouseRFRegistryInfoResponse()
			: base()
		{
		}

		public WhsRFRegistryInfo RFRegistryInfo { get; set; }
	}
}
