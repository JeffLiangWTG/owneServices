using System.Collections.Generic;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingWhsOrderDetailsColumnProvider : WarehouseOrderLinesColumnProvider
	{
		#region CustomizeDictionaryCore

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			var releaseDetailsColumn = new ZNewRowColumn(nameof(WebTracker.Grids.WarehouseDocketLine.ReleaseDetails))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.ReleaseDetails,
				HeaderText = Res.GetString("ddad7ac7-c89b-4106-9dac-2b2d854285e4", "Release Details"),
				Collapsable = true
			};

			releaseDetailsColumn.ItemTemplate = new TrackingOrderLineTemplate(releaseDetailsColumn);
			AddToDictionaryAsRequired(releaseDetailsColumn);
		}

		#endregion

		#region GetOldColumnsOrder

		protected override List<int> GetOldColumnsOrder()
		{
			var result = base.GetOldColumnsOrder();
			result.Add((int)WebTracker.Grids.WarehouseDocketLine.ReleaseDetails);
			return result;
		}

		#endregion
	}
}
