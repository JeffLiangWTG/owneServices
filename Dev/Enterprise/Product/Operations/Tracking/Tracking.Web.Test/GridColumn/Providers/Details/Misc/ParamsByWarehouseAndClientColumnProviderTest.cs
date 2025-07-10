using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ParamsByWarehouseAndClientColumnProvider))]
	sealed class ParamsByWarehouseAndClientColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZGuidDropDownListColumn("Warehouse", WhsProductParamsByWhsAndClientSchema.W3_WW.Name, "Lookups.Warehouses") { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.Warehouse });
			AddDefaultsColumn(new ZDropDownListColumn("Stock Take Cycle", WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle.Name, "Lookups.StockTakeCycles") { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.StockTakeCycle });
			AddDefaultsColumn(new ZCalcEditColumn("Expiry Notification Period", WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.ExpiryNotificationPeriod });
			AddDefaultsColumn(new ZCalcEditColumn("Replenish Minimum", WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.ReplenMinimum });
			AddDefaultsColumn(new ZCalcEditColumn("Economic Quantity", WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity.Name) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.EconomicQuantity });
			AddDefaultsColumn(new ZTextEditColumn("UQ", WhsProductParamsByWhsAndClient.Schema.UQ) { ColumnKey = WebTracker.Grids.ParamsByWarehouseAndClient.UQ });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ParamsByWarehouseAndClientColumnProvider();
		}
	}
}
