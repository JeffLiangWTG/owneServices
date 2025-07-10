using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrder))]
	sealed class TrackingOrderBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingOrder>
	{
		protected override TrackingOrder GetNewBizOForNotification()
		{
			var order = Factory.New<TrackingOrder>();
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_Partno = "COOLRIDGE";
			orderLine.JO_Description = "water";
			orderLine.JO_InnerPacks = 1;
			orderLine.JO_OuterPacks = 1;
			orderLine.JO_Quantity = 1;
			orderLine.JO_QtyInvoiced = 1;
			orderLine.JO_QtyReceived = 1;
			orderLine.JO_F3_NKPackType = "UNT";
			orderLine.JO_ItemPrice = 1;
			orderLine.JO_LinePrice = 1;
			return order;
		}
	}
}
