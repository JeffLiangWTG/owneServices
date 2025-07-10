using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsDocketsWebServiceResponse : WhsDocketWebServiceBaseResponse
	{
		public WhsDocketInfo[] Dockets { get; set; }
	}
}
