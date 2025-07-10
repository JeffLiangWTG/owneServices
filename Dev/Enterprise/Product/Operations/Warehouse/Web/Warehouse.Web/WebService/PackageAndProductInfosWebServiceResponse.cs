using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackageAndProductInfosWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public PackageAndProductInfosWebServiceResponse()
			: base()
		{
		}

		#endregion

		#region Properties

		public WhsPackageProductInfo[] ProductInfos { get; set; }
		public PackageForPackingInfo Package { get; set; }
		public bool IsInvalidToteId { get; set; }

		#endregion
	}
}
