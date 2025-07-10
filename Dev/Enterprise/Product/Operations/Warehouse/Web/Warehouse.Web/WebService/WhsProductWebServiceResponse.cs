using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsProductWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public WhsProductInfo Product { get; set; }

		public WhsProductPartAttributesInfo ProductPartAttributes { get; set; }

		#endregion

	}
}
