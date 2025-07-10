using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region FinaliseDocket

		[WebMethod(Description = "Finalise Docket")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketWebServiceResponse FinaliseDocket(Guid pk, RFDocketType docketType)
		{
			return HandleWebServiceRequest<WhsDocketWebServiceResponse>(response => FinaliseDocketCore(response, pk, docketType));
		}

		void FinaliseDocketCore(WebServiceResponse response, Guid pk, RFDocketType docketType)
		{
			switch (docketType)
			{
				case RFDocketType.WhsReceive:
					FinaliseDocketCore<WhsReceive>(response, pk, Res.GetString("51b5481e-96b8-4145-b042-7b51f9fe0ac0", "Receive"), DocketType.Codes.Receive);
					break;
				case RFDocketType.WhsTransfer:
					FinaliseDocketCore<WhsTransfer>(response, pk, Res.GetString("29022982-0055-40fa-832b-fc176334a665", "Transfer"), DocketType.Codes.Transfer);
					break;
				default:
					response.LogBusinessValidationError(Res.GetString("07672d85-0fea-456e-a40a-d623c4e0347e", "Docket type '{0}' is not supported.", docketType.ToString()));
					break;
			}
		}

		void FinaliseDocketCore<TDocket>(WebServiceResponse response, Guid pk, string docketDescription, string docketType)
			where TDocket : WhsDocket
		{
			var docket = Factory.Load<TDocket>(WebServiceHelper.GetUnfinalisedDocketByPKQuery(Factory, SecurityHeader.WarehouseCode, pk)).SingleOrDefault();

			if (docket == null)
			{
				response.LogBusinessValidationError(Res.GetString("549AD4A8-3DBB-41CF-B034-0842932ECEB0", "Can't find un-finalized {0}.", docketDescription));
			}
			else if (ShouldRunFinalise(docket))
			{
				if (docketType == DocketType.Codes.Transfer)
				{
					docket.AddEvents(ZArchitecture.Business.AutoEvents.ServiceCompleted);
				}

				WebServiceHelper.FinaliseAndSaveDocket(Factory, docket, docketDescription);
			}
		}

		static bool ShouldRunFinalise(WhsDocket docket) => docket is not WhsTransfer transfer || transfer.Lines.All(line => line.IsFinalised);

		#endregion
	}
}
