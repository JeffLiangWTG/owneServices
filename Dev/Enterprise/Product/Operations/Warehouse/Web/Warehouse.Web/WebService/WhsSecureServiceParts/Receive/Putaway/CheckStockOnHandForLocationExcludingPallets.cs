using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region CheckStockOnHandForLocationExcludingPallets

		[WebMethod(Description = "Check location has any stock not on specified PalletID")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CheckStockOnHandForLocationExcludingPallets(string[] palletIDs, string location)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WebServiceResponse>(r => CheckStockOnHandForLocationExcludingPallets(r, palletIDs, location));
		}

		void CheckStockOnHandForLocationExcludingPallets(WebServiceResponse response, string[] palletIDs, string location)
		{
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var inventoryLocation = whs.FindLocation(location);
			if (inventoryLocation != null)
			{
				var currentInventoriesForPallet = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WD, notIn: true);
				currentInventoriesForPallet.AddToFilter(WhsInventoryViewSchema.WI_PalletID, palletIDs);
				currentInventoriesForPallet.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, whs.PK);
				currentInventoriesForPallet.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

				var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, inventoryLocation.PK);
				inventoryQuery.AddSubQuery(WhsInventoryViewSchema.WI_WD, currentInventoriesForPallet, JoinCondition.And);
				inventoryQuery.ReLoadExistingRows = true;

				if (Factory.LoadTop1<WhsInventoryView>(inventoryQuery) != null)
				{
					response.LogBusinessValidationError(Res.GetString("5b4dc50f-a9ff-4fd4-b6be-f61d1c35fcbc", "Location {0} has stock on hand.", location));
				}
			}
		}

		#endregion
	}
}
