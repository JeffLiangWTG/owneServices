using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsManyProductWebServiceResponse : WebServiceResponse
	{
		public WhsProductInfo[] Products { get; set; }
		public WhsProductPartAttributesInfo[] ProductPartAttributes { get; set; }
	}
}
