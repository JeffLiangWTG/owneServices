using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingWarehouseColumnProvider))]
	sealed class TrackingWarehouseColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouses.Code],
				TestProvider[WebTracker.Grids.TrackingWarehouses.WarehouseName],
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZButtonColumn("Warehouse Name", WhsWarehouseSchema.WW_WarehouseName.Name) { ColumnKey = WebTracker.Grids.TrackingWarehouses.WarehouseName });
			AddDefaultsColumn(new ZTextEditColumn("Code", WhsWarehouseSchema.WW_WarehouseCode.Name) { ColumnKey = WebTracker.Grids.TrackingWarehouses.Code });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingWarehouseColumnProvider();
		}

		#endregion
	}
}
