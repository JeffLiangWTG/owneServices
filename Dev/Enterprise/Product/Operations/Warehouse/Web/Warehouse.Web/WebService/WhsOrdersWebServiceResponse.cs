using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsOrdersWebServiceResponse : WebServiceResponse
	{
		public WhsDocketInfo[] Orders { get; set; }
	}
}
