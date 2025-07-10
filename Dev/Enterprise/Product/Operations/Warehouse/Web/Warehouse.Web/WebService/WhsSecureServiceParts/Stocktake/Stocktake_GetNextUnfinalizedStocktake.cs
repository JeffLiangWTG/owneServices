using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_GetNextUnfinalizedStocktake

		[WebMethod(Description = "Get stocktake data for reference")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsStocktakeWebServiceResponse Stocktake_GetNextUnfinalizedStocktake(string referenceOrEmpty, string area, string pickMethod)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeWebServiceResponse>(result => Stocktake_GetNextUnfinalizedStocktakeCore(result, referenceOrEmpty, area, pickMethod));
		}

		void Stocktake_GetNextUnfinalizedStocktakeCore(WhsStocktakeWebServiceResponse response, string referenceOrEmpty, string area, string pickMethod)
		{
			var stocktakeSessionObject = WhsStocktakeHelper.GetUnfinalizedStocktake(response, Factory, SecurityHeader.UserName, SecurityHeader.WarehouseCode,
				referenceOrEmpty, area, pickMethod,
				WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName),
				WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode),
				addLog: true);

			if (response.NoError())
			{
				response.Stocktake = stocktakeSessionObject.Stocktake;
				response.LocationsToCount = stocktakeSessionObject.GetLocationsToCount();
				if (response.LocationsToCount.Count > 0)
				{
					response.LinesToCount = stocktakeSessionObject.GetLinesToCountForLocation(response.LocationsToCount[0]);
				}
			}
		}

		#endregion
	}
}
