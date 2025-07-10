using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region IsValidClientCode

		[WebMethod(Description = "Is Valid Client Code")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse IsValidClientCode(string clientCode)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r =>
			{
				if (WebServiceHelper.GetOrgHeader(Factory, clientCode) == null)
				{
					r.LogBusinessValidationError(Res.GetString("c939b317-081f-4ba8-9afd-18bf06247f35", "Please provide a valid client code."));
				}
			});
		}

		#endregion
	}
}
