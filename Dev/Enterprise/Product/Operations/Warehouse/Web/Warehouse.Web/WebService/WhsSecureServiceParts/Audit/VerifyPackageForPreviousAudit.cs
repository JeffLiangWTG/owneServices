using System;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region VerifyPackageForPreviousAudit

		[WebMethod(Description = "Verifies if the Package has been audited previously and returns a YesNoEnquiry with a passed/failed message in case a previous audit was found.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse VerifyPackageForPreviousAudit(Guid packagePK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response =>
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForPreviousAudit(Factory, response, packagePK);
			});
		}

		#endregion
	}
}