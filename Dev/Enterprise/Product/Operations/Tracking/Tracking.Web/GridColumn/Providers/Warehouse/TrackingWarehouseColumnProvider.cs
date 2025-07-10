using System.Collections.Generic;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingWarehouseColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddButtonColumn(Res.GetString("5635b352-b3eb-4970-a6df-138586d55080", "Warehouse Name"), WhsWarehouseSchema.WW_WarehouseName.Name, WebTracker.Grids.TrackingWarehouses.WarehouseName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("76a9e86b-e619-4217-8677-05525824753c", "Code"), WhsWarehouseSchema.WW_WarehouseCode.Name) { ColumnKey = WebTracker.Grids.TrackingWarehouses.Code });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingWarehouses.WarehouseName);
			result.Add((int)WebTracker.Grids.TrackingWarehouses.Code);
			return result;
		}
	}
}
