using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingOrderLinesColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingOrderLinesColumnProviderTest : GridColumnProviderTest
	{
		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingOrderLines.LineSplitNumber],
				TestProvider[WebTracker.Grids.TrackingOrderLines.OrderLineNumber],
				TestProvider[WebTracker.Grids.TrackingOrderLines.Quantity],
				TestProvider[WebTracker.Grids.TrackingOrderLines.LineNumber]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZButtonColumn("Line", TrackingOrderLine.Schema.OrderAndOrderLineNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OrderLineNumber });
			AddDefaultsColumn(new ZTextEditColumn("Order #", TrackingOrderLine.Schema.OrderNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OrderNumber });
			AddDefaultsColumn(new ZTextEditColumn("Line #", TrackingOrderLine.Schema.JO_LineNo) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineNumber });
			AddDefaultsColumn(new ZTextEditColumn("Line Split #", TrackingOrderLine.Schema.JO_LineSplitNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineSplitNumber });
			AddDefaultsColumn(new ZTextEditColumn("Part #", TrackingOrderLine.Schema.JO_Partno) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber });
			AddDefaultsColumn(new ZGroupColumn("Quantity", new DataGridColumn[] {
				new ZCalcEditColumn("Quantity", TrackingOrderLine.Schema.JO_Quantity),
				new ZTextEditColumn("Unit of Quantity", TrackingOrderLine.Schema.JO_F3_NKPackType)
	})
			{ ColumnKey = WebTracker.Grids.TrackingOrderLines.Quantity });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingOrderLines.Quantity
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingOrderLinesColumnProvider();
		}
	}
}
