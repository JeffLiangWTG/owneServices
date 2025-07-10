using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPickGroupsWebServiceResponse : WebServiceResponse
	{
		public WhsPickGroupInfoCollection PickGroups { get; set; }
	}
}
