using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_SetStocktakeLineCount

		[WebMethod(Description = "Set stocktake line count")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsStocktakeLineWebServiceResponse Stocktake_SetStocktakeLineCount(Guid stocktakeLinePK, string packType, decimal count)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeLineWebServiceResponse>(r => Stocktake_SetStocktakeLineCountCore(r, stocktakeLinePK, packType, count));
		}

		void Stocktake_SetStocktakeLineCountCore(WhsStocktakeLineWebServiceResponse response, Guid stocktakeLinePK, string packType, decimal count)
		{
			var stocktakeManager = new StocktakeManager(Factory, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode), WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName));
			var line = WhsStocktakeHelper.SetStocktakeLineCount(Factory, stocktakeLinePK, response, (stocktakeLine) => stocktakeManager.SetStocktakeLineCountQuantity(stocktakeLine, packType, count));
			if (response.NoError())
			{
				response.StocktakeLine = new WhsStocktakeLineInfo(new WhsStocktakeInfo(line.Stocktake), line);
			}
		}

		#endregion
	}
}
