using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_SetZeroCountForAllLinesToCountWithPalletId

		[WebMethod(Description = "Set 0 count on all stocktake line for a pallet Id")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsStocktakeWebServiceResponse Stocktake_SetZeroCountForAllLinesToCountWithPalletId(string reference, string area, string pickMethod, string location, string palletId)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeWebServiceResponse>(r => Stocktake_SetZeroCountForAllLinesToCountWithPalletIdCore(r, reference, area, pickMethod, location, palletId));
		}

		void Stocktake_SetZeroCountForAllLinesToCountWithPalletIdCore(WhsStocktakeWebServiceResponse response, string reference, string area, string pickMethod, string location, string palletId)
		{
			if (string.IsNullOrEmpty(palletId))
			{
				response.LogBusinessValidationError(Res.GetString("4f67d7d9-7544-40f0-b4f0-732ea82930a6", "Pallet Id should not be null or empty."));
			}
			else
			{
				SetZeroCountForAllLinesToCountWithPalletId(response, reference, area, pickMethod, location, palletId);
			}
		}

		void SetZeroCountForAllLinesToCountWithPalletId(WhsStocktakeWebServiceResponse response, string reference, string area, string pickMethod, string location, string palletId)
		{
			var stocktakeSessionObject = WhsStocktakeHelper.GetUnfinalizedStocktake(response, Factory, SecurityHeader.UserName, SecurityHeader.WarehouseCode, reference, area, pickMethod, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName), WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode));
			if (response.NoError())
			{
				response.Stocktake = stocktakeSessionObject.Stocktake;
				response.LocationsToCount = stocktakeSessionObject.GetLocationsToCount();

				foreach (var line in stocktakeSessionObject.GetLinesToCountForLocation(location))
				{
					if (palletId.Equals(line.PalletID))
					{
						var stocktakeManager = new StocktakeManager(Factory, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode), WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName));
						WhsStocktakeHelper.SetStocktakeLineCount(Factory, line.PK, response, (stocktakeLine) => stocktakeManager.SetStocktakeLineCountQuantity(stocktakeLine, line.PackType, 0));

						if (!response.NoError())
						{
							break;
						}
					}
					else
					{
						response.LinesToCount.Add(line);
					}
				}
			}
		}

		#endregion
	}
}
