using System;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region VerifyPackageForAuditForPK

		[WebMethod(Description = "Verifies if the Package can be audited or not with a given PK")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageWebServiceResponse VerifyPackageForAuditForPK(Guid packagePK)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<PackageWebServiceResponse>(r => WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, r, packagePK));
		}

		#endregion
	}
}