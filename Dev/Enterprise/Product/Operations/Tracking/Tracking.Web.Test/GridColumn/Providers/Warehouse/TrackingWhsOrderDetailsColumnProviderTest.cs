using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingWhsOrderDetailsColumnProvider))]
	sealed class TrackingWhsOrderDetailsColumnProviderTest : WarehouseOrderLinesColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			var result = new List<DataGridColumn>(base.GetColumnsForLayoutFixNoDynamicColumns());
			result.Add(TestProvider[WebTracker.Grids.WarehouseDocketLine.ReleaseDetails]);
			return result.ToArray();
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			var releaseDetailsColumn = new ZNewRowColumn(nameof(WebTracker.Grids.WarehouseDocketLine.ReleaseDetails))
			{
				ColumnKey = WebTracker.Grids.WarehouseDocketLine.ReleaseDetails,
				HeaderText = "Release Details",
				Collapsable = true
			};

			releaseDetailsColumn.ItemTemplate = new TrackingOrderLineTemplate(releaseDetailsColumn);
			AddRequiredColumn(releaseDetailsColumn);
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.WarehouseDocketLine.Reserved
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingWhsOrderDetailsColumnProvider();
		}

		#endregion
	}
}
