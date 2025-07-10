using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_SetStocktakeLineEmptyLocationConfirm

		[WebMethod(Description = "Set empty line stocktake")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsStocktakeLineWebServiceResponse Stocktake_SetStocktakeLineEmptyLocationConfirm(Guid stocktakeLinePK, bool isEmpty)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeLineWebServiceResponse>(r => Stocktake_SetStocktakeLineEmptyLocationConfirmCore(r, stocktakeLinePK, isEmpty));
		}

		void Stocktake_SetStocktakeLineEmptyLocationConfirmCore(WhsStocktakeLineWebServiceResponse response, Guid stocktakeLinePK, bool isEmpty)
		{
			var stocktakeManager = new StocktakeManager(Factory, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode), WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName));
			var line = WhsStocktakeHelper.SetStocktakeLineCount(Factory, stocktakeLinePK, response, (stocktakeLine) => stocktakeManager.SetStocktakeLineEmptyLocationConfirm(stocktakeLine, isEmpty));
			if (response.NoError())
			{
				response.StocktakeLine = isEmpty ? new WhsStocktakeLineInfo(new WhsStocktakeInfo(line.Stocktake), line) : new WhsStocktakeLineInfo();
			}
		}

		#endregion
	}
}
