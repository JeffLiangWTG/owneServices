using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class OrgHeadersWebServiceResponse : WebServiceResponse
	{
		public OrgHeaderInfo[] Organisations
		{
			get;
			set;
		}
	}
}
