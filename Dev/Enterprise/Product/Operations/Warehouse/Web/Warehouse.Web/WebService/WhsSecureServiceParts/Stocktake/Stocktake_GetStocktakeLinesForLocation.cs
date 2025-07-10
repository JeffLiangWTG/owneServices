using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_GetStocktakeLinesForLocation

		[WebMethod(Description = "Get stocktake lines for a location")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsStocktakeWebServiceResponse Stocktake_GetStocktakeLinesForLocation(string reference, string area, string pickMethod, string location)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeWebServiceResponse>(r => Stocktake_GetStocktakeLinesForLocationCore(r, reference, area, pickMethod, location));
		}

		void Stocktake_GetStocktakeLinesForLocationCore(WhsStocktakeWebServiceResponse response, string reference, string area, string pickMethod, string location)
		{
			if (response.ValidateShouldNotBeNull(reference, nameof(reference)))
			{
				var stocktakeSessionObject = WhsStocktakeHelper.GetUnfinalizedStocktake(response, Factory, SecurityHeader.UserName, SecurityHeader.WarehouseCode, reference, area, pickMethod, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName), WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode));
				if (response.NoError())
				{
					response.Stocktake = stocktakeSessionObject.Stocktake;
					response.LocationsToCount = stocktakeSessionObject.GetLocationsToCount();
					response.LinesToCount.AddRange(stocktakeSessionObject.GetLinesToCountForLocation(location));
				}
			}
		}

		#endregion
	}
}
