using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetIsPrintPalledIdDuringUnload

		[WebMethod(Description = "Get PrintPalletIDDuringUnload configured for receive")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public IsPrintPalletIdDuringUnloadWebServiceResponse GetIsPrintPalletIdDuringUnload(Guid receivePK)
		{
			return HandleWebServiceRequest<IsPrintPalletIdDuringUnloadWebServiceResponse>(result => GetIsPrintPalledIdDuringUnloadCore(result, receivePK));
		}

		void GetIsPrintPalledIdDuringUnloadCore(IsPrintPalletIdDuringUnloadWebServiceResponse result, Guid receivePK)
		{
			var receive = Factory.Load<WhsReceive>(receivePK);
			if (receive != null)
			{
				var clientParams = WhsClientParams.GetClientParams(receive.Client);
				var whsClientParameterByWarehouseCollection = clientParams.ClientParametersByWarehouse;
				var whsClientParameterByWarehouse = whsClientParameterByWarehouseCollection.FindWithEmptyFallback(receive.WD_OH_Client, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK, receive.WD_ReceiveCategory);
				if (whsClientParameterByWarehouse != null)
				{
					result.IsPrintPalletIDDuringUnload = whsClientParameterByWarehouse.WY_PrintPalletIDDuringUnload;
				}
			}
			else
			{
				result.Error = ErrorTypes.BusinessValidationError;
				result.ErrorMessage = Res.GetString("09BEE713-BE49-4D8B-A24A-AC0274D39013", "Invalid receive PK");
			}
		}

		#endregion
	}
}
