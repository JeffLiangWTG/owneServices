using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region IsValidSupplierCode

		[WebMethod(Description = "Is Valid Supplier Code")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse IsValidSupplierCode(string supplierCode)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r =>
			{
				var supplier = WebServiceHelper.GetOrgHeader(Factory, supplierCode);
				if (supplier == null || !supplier.OH_IsConsignor || !supplier.OH_IsActive)
				{
					r.LogBusinessValidationError(Res.GetString("90051693-527c-4863-8a17-997b07db614b", "Please provide a valid supplier code."));
				}
			});
		}

		#endregion
	}
}
