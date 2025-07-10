using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackageProductInfosWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public PackageProductInfosWebServiceResponse()
			: base()
		{
			PackageID = string.Empty;
		}

		#endregion

		#region Properties

		public WhsPackageProductInfo[] ProductInfos { get; set; }
		public string PackageID { get; set; }

		#endregion
	}
}
