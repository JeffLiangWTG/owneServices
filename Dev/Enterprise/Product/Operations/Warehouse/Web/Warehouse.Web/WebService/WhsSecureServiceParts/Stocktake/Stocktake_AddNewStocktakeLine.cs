using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region Stocktake_AddNewStocktakeLine

		[WebMethod(Description = "Add a new stocktake line for a specific stocktake")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public WhsStocktakeLineWebServiceResponse Stocktake_AddNewStocktakeLine(Guid stocktakePk, Guid supplierPartPk, string locationString, Guid clientPK, string packType,
			string attr1, string attr2, string attr3, string serial, DateTime? expiryDate, DateTime? packingDate, string palletID, decimal countQuantity, string stocktakeInventoryStatus)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WhsStocktakeLineWebServiceResponse>(r => Stocktake_AddNewStocktakeLineCore(r, stocktakePk, supplierPartPk, locationString, clientPK, packType,
				attr1, attr2, attr3, serial, expiryDate, packingDate, palletID, countQuantity, stocktakeInventoryStatus));
		}

		void Stocktake_AddNewStocktakeLineCore(WhsStocktakeLineWebServiceResponse response, Guid stocktakePk, Guid supplierPartPk, string locationString, Guid clientPK, string packType,
			string attr1, string attr2, string attr3, string serial, DateTime? expiryDate, DateTime? packingDate, string palletID, decimal countQuantity, string stocktakeInventoryStatus)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (response.ValidateShouldNotBeNull(warehouse, nameof(warehouse)))
			{
				var location = WebServiceHelper.GetLocationByLocationString(Factory, warehouse, locationString);
				if (location != null)
				{
					var stocktakeManager = new StocktakeManager(Factory, ZString.Empty, ZString.Empty, warehouse, WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName));
					var stocktakeLineResult = stocktakeManager.AddNewStocktakeLine(new ZGuid(stocktakePk), new ZGuid(supplierPartPk), location.PK, new ZGuid(clientPK), packType, attr1, attr2, attr3, serial, new ZDate(expiryDate),
						new ZDate(packingDate), palletID, countQuantity, stocktakeInventoryStatus);
					if (string.IsNullOrEmpty(stocktakeLineResult.ErrorMessage))
					{
						response.StocktakeLine = new WhsStocktakeLineInfo(new WhsStocktakeInfo(stocktakeLineResult.StocktakeLine.Stocktake), stocktakeLineResult.StocktakeLine);
					}
					else
					{
						response.LogBusinessValidationError(stocktakeLineResult.ErrorMessage);
					}
				}
			}
		}

		#endregion
	}
}
