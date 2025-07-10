using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region GetHeldCodes

		[WebMethod(Description = "Get Held Codes")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsInventoryHeldCodesWebServiceResponse GetHeldCodes()
		{
			return HandleWebServiceRequest<WhsInventoryHeldCodesWebServiceResponse>(r => r.WhsInventoryHeldCodes = GetHeldCodesCore());
		}

		WhsInventoryHeldCodeInfoCollection GetHeldCodesCore()
		{
			var heldCodes = new WhsInventoryHeldCodeCollection(Factory);
			return new WhsInventoryHeldCodeInfoCollection(heldCodes);
		}

		#endregion
	}
}
