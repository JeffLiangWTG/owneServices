using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region PerformPackageContentsAudit

		[WebMethod(Description = "Verifies scanned product lines of a package against the package order and stores any variances found. It will hold the package in case of variance.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PerformPackageContentsAudit(Guid packagePK, WhsPackageProductInfo[] auditLines)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response =>
			{
				var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, response, auditLines, packagePK, staff);
			});
		}

		#endregion
	}
}