using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get Transport Company Code From RMA Linked Order")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public OrgHeadersWebServiceResponse GetTransportCompanyFromRMALinkedOrder(Guid receivePK)
		{
			return HandleWebServiceRequest<OrgHeadersWebServiceResponse>(response => GetTransportCompanyFromRMALinkedOrderCore(response, receivePK));
		}

		void GetTransportCompanyFromRMALinkedOrderCore(OrgHeadersWebServiceResponse response, Guid receivePK)
		{
			var receive = Factory.Load<WhsReceive>(new ZGuid(receivePK));
			if (receive == null)
			{
				response.LogBusinessValidationError(Res.GetString("4d39cae1-6d9a-441b-aeb0-043162dc6fbf", "Receive could not be found. Please restart the operation and try again."));
			}
			else
			{
				var order = Factory.Load<WhsOrder>(new ZGuid(receive.WD_WD_ParentDocket));
				if (order == null)
				{
					response.LogBusinessValidationError(Res.GetString("3da94867-a27d-4e7a-b886-88fa6768b71f", "Linked order could not be found. Please restart the operation and try again."));
				}
				else
				{
					var transportCompany = order.TransportCo;
					if (transportCompany == null)
					{
						response.LogBusinessValidationError(Res.GetString("f97aaae1-c4df-4672-92b3-c636fb17fd1b", "Linked order does not have transport company info. Please scan / enter the company code directly."));
					}
					else
					{
						response.Organisations = new[] { new OrgHeaderInfo(transportCompany) };
					}
				}
			}
		}
	}
}
