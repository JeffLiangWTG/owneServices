using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsCartonGroupInfoWebServiceResponse : WebServiceResponse
	{
		public WhsCartonGroupInfoCollection CartonGroups { get; set; }
	}
}
