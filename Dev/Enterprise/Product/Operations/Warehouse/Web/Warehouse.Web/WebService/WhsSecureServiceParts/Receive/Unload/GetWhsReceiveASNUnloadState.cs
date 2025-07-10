using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get Receive ASN Unload State")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public WhsDocketWebServiceResponse GetWhsReceiveASNUnloadState(Guid receivePK)
		{
			return HandleWebServiceRequest<WhsDocketWebServiceResponse>(r => GetWhsReceiveASNUnloadStateCore(r, receivePK));
		}

		void GetWhsReceiveASNUnloadStateCore(WhsDocketWebServiceResponse response, Guid receivePK)
		{
			var receive = WhsGroupedUnloadHelper.LoadValidNotFinalizedReceive(response, Factory, receivePK);
			if (response.Error == ErrorTypes.None)
			{
				var docketInfo = new WhsDocketInfo();
				docketInfo.PK = receive.PK.ToGuid();
				docketInfo.ASNUnloadState = GetASNStateFromReceive(receive);
				response.Docket = docketInfo;
			}
		}
	}
}
