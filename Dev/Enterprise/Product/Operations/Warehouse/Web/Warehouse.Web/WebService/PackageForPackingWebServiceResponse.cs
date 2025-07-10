using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PackageForPackingWebServiceResponse : WebServiceResponse
	{
		public PackageForPackingInfoCollection PackagesForPackingInfo
		{
			get => packagesForPackingInfo ?? (packagesForPackingInfo = new PackageForPackingInfoCollection());
			set => packagesForPackingInfo = value;
		}

		PackageForPackingInfoCollection packagesForPackingInfo;
	}
}
