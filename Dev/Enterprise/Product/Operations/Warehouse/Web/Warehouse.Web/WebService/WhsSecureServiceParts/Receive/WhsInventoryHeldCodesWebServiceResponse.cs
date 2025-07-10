using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsInventoryHeldCodesWebServiceResponse : WebServiceResponse
	{
		public WhsInventoryHeldCodeInfoCollection WhsInventoryHeldCodes { get; set; }
	}
}
