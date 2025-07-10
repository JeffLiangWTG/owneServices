using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Check Receive ASN Quantity Unloaded")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CheckReceiveASNQuantityMet(Guid receivePK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => CheckReceiveASNQuantityMetCore(response, receivePK));
		}

		void CheckReceiveASNQuantityMetCore(WebServiceResponse response, Guid receivePK)
		{
			var receive = Factory.Load<WhsReceive>(receivePK);
			if (receive == null)
			{
				response.LogBusinessValidationError(Res.GetString("a883e145-cc1a-4ca8-98fb-afad28bf395b", "Receive not found."));
			}
			else if (receive.WD_UnloadCompletedTime.IsEmpty && TaskManagementHelper.PlannedReceiveCanSetUnloadCompleteTime(receive, Factory, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName)))
			{
				if (receive.AsnLines.Count > 0)
				{
					var asnQtyMet = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>().All(l => l.ReceivedQuantity >= l.ExpectedQuantity);
					if (asnQtyMet)
					{
						response.LogError(ErrorTypes.YesNoEnquiry, Res.GetString("0340e727-0bc1-4a54-8772-484a192c57a4", "Has the unload been completed for Receive {0}?", receive.WD_ExternalReference));
					}
				}
			}
		}
	}
}
