using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Set package status to held.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse SetWhsPackageStatusToHeld(Guid packagePK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => SetWhsPackageStatusToHeldCore(r, packagePK));
		}

		void SetWhsPackageStatusToHeldCore(WebServiceResponse response, Guid packagePK)
		{
			var result = Factory.Load<PkgPackage>(packagePK);

			if (result != null)
			{
				result.KP_IsHeld = true;
				Factory.Save();
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("7A23BEC7-2979-4030-B0C2-C0B6DEAA8B6A", "Package does not exist.");
			}
		}
	}
}
