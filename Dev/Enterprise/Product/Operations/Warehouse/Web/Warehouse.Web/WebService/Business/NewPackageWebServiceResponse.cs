using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class NewPackageWebServiceResponse : WebServiceResponse
	{
		public PackageInfo NewPackage
		{
			get;
			set;
		}
	}
}
