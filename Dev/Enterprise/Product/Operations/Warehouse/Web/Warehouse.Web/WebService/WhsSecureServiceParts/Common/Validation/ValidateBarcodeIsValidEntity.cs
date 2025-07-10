using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region ValidateBarcodeIsValidEntity

		[WebMethod(Description = "Validate Barcode Is Valid Entity")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ValidateBarcodeIsValidEntity(string barcode, string clientCode)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ValidateBarcodeIsValidEntityCore(r, barcode, clientCode));
		}

		void ValidateBarcodeIsValidEntityCore(WebServiceResponse response, string barcode, string clientCode)
		{
			var result = Product_GetByPartNumOrBarcode(barcode, clientCode).Product != null
					 || !string.IsNullOrEmpty(GetLocation(barcode).Location)
					 || ValidatePalletID(barcode)
					 || ValidateInTransitPalletID(barcode);

			if (!result)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("fe9daba6-2656-45a3-b32d-5afa5ea027ee", "Barcode is not valid entity.");
			}
		}

		bool ValidatePalletID(string palletID)
		{
			var criteria = new WhsInventorySearchCriteriaInfo(string.Empty, string.Empty, string.Empty, palletID);
			var inventoryLoader = new WhsInventoryLoader(Factory, SecurityHeader.WarehouseCode);
			var result = inventoryLoader.LoadWhsInventory(criteria, JoinCondition.And, excludeIntransit: false);
			return result.Length > 0;
		}

		bool ValidateInTransitPalletID(string barcode)
		{
			var result = 0;
			if (!string.IsNullOrWhiteSpace(barcode))
			{
				var query = new ZDBOnlyQuery(typeof(WhsTransferLine));
				query.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, SQLComparisonOperator.Equal, barcode);
				query.AddToFilter(WhsDocketLineSchema.WE_CurrentInventoryStatus, SQLComparisonOperator.Equal, InventoryStatus.Codes.InTransit);
				query.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0);

				result = Factory.Load<WhsTransferLine>(query).Length;
			}

			return result > 0;
		}

		#endregion
	}
}
