using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region ClosePackage

		[WebMethod(Description = "Close package")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ClosePackage(PackageInfo packagePackInto, bool deferClosingPackage)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ClosePackage(r, packagePackInto, deferClosingPackage));
		}

		void ClosePackage(WebServiceResponse response, PackageInfo packagePackInto, bool deferClosingPackage)
		{
			if (response.ValidateShouldNotBeNull(packagePackInto, nameof(packagePackInto)))
			{
				var package = PackageHelper.GetPackageAndValidateForClosing(response, packagePackInto.PackageID, packagePackInto.PK, Factory);
				if (package != null)
				{
					PickLineUpdater.ClosePackage(package, response);

					if (response.NoError() && !deferClosingPackage)
					{
						var concurrencyErrorMessage = Res.GetString("68FDAB73-A2FA-49DA-AD39-57DD263A0121", "Another user has changed the package while you have been working on it. Please restart the operation and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(package.Factory, response, (concurrencyException) => concurrencyErrorMessage);
					}
				}
			}
		}

		#endregion
	}
}
