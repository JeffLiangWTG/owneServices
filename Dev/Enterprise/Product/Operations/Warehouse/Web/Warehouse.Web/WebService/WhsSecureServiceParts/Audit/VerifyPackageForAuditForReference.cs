using System.Web.Services;
using System.Web.Services.Protocols;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region VerifyPackageForAuditForReference

		[WebMethod(Description = "Verifies if the Package can be audited or not receiving with a given PackageId, if there are several of them all will be returned")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageWebServiceResponse VerifyPackageForAuditForReference(string packageId)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<PackageWebServiceResponse>(r =>
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, r, packageId));
		}

		#endregion
	}
}