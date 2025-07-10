using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Set Receive Unload Completed Time")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse SetReceiveUnloadCompletedTime(Guid receivePK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => SetReceiveUnloadCompletedTimeCore(response, receivePK));
		}

		void SetReceiveUnloadCompletedTimeCore(WebServiceResponse response, Guid receivePK)
		{
			var receive = Factory.Load<WhsReceive>(receivePK);
			if (receive != null)
			{
				if (receive.WD_UnloadCompletedTime.IsEmpty)
				{
					if (!TaskManagementHelper.PlannedReceiveCanSetUnloadCompleteTime(receive, Factory, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName)))
					{
						response.Error = ErrorTypes.Information;
						response.ErrorMessage = Res.GetString("03d3a9aa-d6f7-4257-9417-6a179ac52e19", "Attempted to complete unload but another user is still working on it.");
					}
					else
					{
						receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
						TaskManagementHelper.CloseRelatedTasks(Factory, receivePK, WarehouseTaskFormFlowTypes.UnloadJob);
						var concurrencyErrorMessage = Res.GetString("f8dccd7e-028b-4bf2-8006-ace4599f37cd", "Unable to save unload completed time to Receive. Another user made changes. Please refresh and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
					}
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("a883e145-cc1a-4ca8-98fb-afad28bf395b", "Receive not found."));
			}
		}
	}
}
