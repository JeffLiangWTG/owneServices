using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrder))]
	sealed class TrackingWhsOrderBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingWhsOrder>
	{
		protected override TrackingWhsOrder GetNewBizOForNotification()
		{
			var trackingOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());

			var order = trackingOrder.WhsOrder;
			var reference = order.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "Reference Text";

			var line = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;
			order.Lines.Add(line);
			line.SupplierPart.OP_PartNum = "PARTNUM";
			line.WE_PackQuantity = 1;
			line.WE_F3_NKPackType = "UNT";
			line.WE_TransactionQuantity = 1;
			line.WE_PartAttrib1 = "one";
			line.WE_PartAttrib2 = "two";
			line.WE_PartAttrib3 = "three";

			return trackingOrder;
		}
	}
}
