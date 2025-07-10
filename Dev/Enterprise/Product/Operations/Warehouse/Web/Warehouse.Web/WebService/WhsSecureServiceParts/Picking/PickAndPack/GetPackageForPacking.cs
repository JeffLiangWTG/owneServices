using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get Package for Packing.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public SinglePackageForPackingWebServiceResponse GetPackageForPacking(string packageID, Guid packagePK)
		{
			return HandleWebServiceRequest<SinglePackageForPackingWebServiceResponse>(r => GetPackageForPackingCore(r, packageID, packagePK));
		}

		void GetPackageForPackingCore(SinglePackageForPackingWebServiceResponse response, string packageID, Guid packagePK)
		{
			var packageInfo = GetPackagesForPackingInfo(packageID, packagePK).SingleOrDefault();
			if (packageInfo == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("4389a4e1-0cba-492d-bb8d-f84f48b54b0e", "Package '{0}' cannot be found or is not valid for Packing.", packageID);
			}
			else
			{
				response.PackageForPackingInfo = packageInfo;
			}
		}
	}
}
