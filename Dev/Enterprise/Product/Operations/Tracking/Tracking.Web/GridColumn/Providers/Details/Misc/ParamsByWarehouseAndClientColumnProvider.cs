using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ParamsByWarehouseAndClientColumnProvider : GridColumnProvider
	{
		public ParamsByWarehouseAndClientColumnProvider()
		{
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZGuidDropDownListColumn(Res.GetString("b3918825-d747-4f36-a71e-d1b8bcbb60ae", "Warehouse"), WhsProductParamsByWhsAndClientSchema.W3_WW.Name, "Lookups.Warehouses") { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.Warehouse });
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("46d12c84-fb77-48ab-80fd-6ab8fa07b1be", "Stock Take Cycle"), WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle.Name, "Lookups.StockTakeCycles") { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.StockTakeCycle });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("af20d186-07f3-4a6b-9605-84f3c08ea31d", "Expiry Notification Period"), WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.ExpiryNotificationPeriod });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("566a1354-4f86-4b6a-a6b3-99de58d95157", "Replenish Minimum"), WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.ReplenMinimum });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("aa819441-787c-49bf-b866-542745b2da75", "Economic Quantity"), WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.EconomicQuantity });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9941c88a-cccf-48eb-9cf3-ce0fc95c1c64", "UQ"), WhsProductParamsByWhsAndClient.Schema.UQ) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.UQ });
		}
	}
}
