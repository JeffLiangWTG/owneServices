using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingOrderLinesColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingOrderLine)null).OrderAndOrderLineNumber);
			AddButtonColumn(Res.GetString("a3ca84da-4977-4f05-a86a-d51799cb47bb", "Line"), TrackingOrderLine.Schema.OrderAndOrderLineNumber, WebTracker.Grids.TrackingOrderLines.OrderLineNumber);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrderLine)null).OrderNumber);
			ZTextEditColumn orderColumn = new ZTextEditColumn(Res.GetString("79175de6-674f-49bf-bbfa-5f94e3ab4c1d", "Order #"), TrackingOrderLine.Schema.OrderNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OrderNumber };
			AddToDictionaryAsDefault(orderColumn);

			ZBindToChecker.CheckBindTo((ZInt)((TrackingOrderLine)null).JO_LineNo);
			ZTextEditColumn orderNumberColumn = new ZTextEditColumn(Res.GetString("0a72e70f-46da-46d2-a00c-f9f481dd7785", "Line #"), TrackingOrderLine.Schema.JO_LineNo) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineNumber };
			AddToDictionaryAsDefault(orderNumberColumn);

			ZBindToChecker.CheckBindTo((ZShort)((TrackingOrderLine)null).JO_LineSplitNumber);
			ZTextEditColumn lineSplitNumberColumn = new ZTextEditColumn(Res.GetString("a9f35b74-b9ab-48c4-b6bf-8fae85dcd79f", "Line Split #"), TrackingOrderLine.Schema.JO_LineSplitNumber) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineSplitNumber };
			AddToDictionaryAsDefault(lineSplitNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrderLine)null).JO_Partno);
			ZTextEditColumn partNumberColumn = new ZTextEditColumn(Res.GetString("1bee378d-5514-47f6-86fe-1d4339808de2", "Part #"), TrackingOrderLine.Schema.JO_Partno) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber };
			AddToDictionaryAsDefault(partNumberColumn);

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingOrderLine)null).JO_Quantity);
			ZBindToChecker.CheckBindTo((ZString)((TrackingOrderLine)null).JO_F3_NKPackType);
			ZCalcEditColumn quantityColumn = new ZCalcEditColumn(Res.GetString("07b89f68-fc68-4a7b-9f37-4c30045c87f5", "Quantity"), TrackingOrderLine.Schema.JO_Quantity);
			ZTextEditColumn uQColumn = new ZTextEditColumn(Res.GetString("f019d384-3c02-4ca0-8f31-190efe6726ec", "Unit of Quantity"), TrackingOrderLine.Schema.JO_F3_NKPackType);
			AddGroupColumn(Res.GetString("07b89f68-fc68-4a7b-9f37-4c30045c87f5", "Quantity"), WebTracker.Grids.TrackingOrderLines.Quantity, true, quantityColumn, uQColumn);
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingOrderLines.OrderLineNumber);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.OrderNumber);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.LineNumber);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.LineSplitNumber);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.PartNumber);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.Quantity);
			result.Add((int)WebTracker.Grids.TrackingOrderLines.UnitOfQuantity);
			return result;
		}
	}
}
