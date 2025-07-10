using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Update Transport Company For Receive")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse UpdateTransportCompanyForWhsReceive(Guid receivePK, string transportCompanyCode)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => { UpdateTransportCompanyCore(response, receivePK, transportCompanyCode); });
		}

		void UpdateTransportCompanyCore(WebServiceResponse response, Guid receivePK, string transportCompanyCode)
		{
			var receive = Factory.Load<WhsReceive>(new ZGuid(receivePK));
			if (receive != null)
			{
				if (string.IsNullOrEmpty(transportCompanyCode))
				{
					receive.TransportCoPK = Guid.Empty;
				}
				else
				{
					var transportCo = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, transportCompanyCode.Trim());
					if (transportCo == null)
					{
						response.LogBusinessValidationError(Res.GetString("c3f1a0b2-4d8e-4f5b-9a6c-7d1e0f2f3a5b", "Transport company could not be found. Please scan the correct company code."));
					}
					else if (!transportCo.OH_IsShippingProvider)
					{
						response.LogBusinessValidationError(Res.GetString("f2a0b1c3-4d8e-4f5b-9a6c-7d1e0f2f3a5b", "The company is not a Carrier. Please scan the correct company code."));
					}
					else
					{
						receive.TransportCoPK = transportCo.PK;
					}
				}
				if (response.NoError())
				{
					var concurrencyErrorMessage = Res.GetString("73636107-eac5-4ae6-b5ad-4661dbfff2a6", "Another user has made changes while you're updating the receive. Please restart the operation and try again.");
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("85d61a7b-dcb8-43d8-ac3f-c6da6f226fcf", "Receive could not be found. Please restart the Unload and try again."));
			}
		}
	}
}
