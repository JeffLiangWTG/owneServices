using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public static class TrackingHelper
	{
		public static TrackingWhsOrder Get(WhsOrder order)
		{
			return order == null ? null : TrackingWhsOrder.GetTrackingWhsOrder(order);
		}

		public static TrackingWhsOrderLine Get(WhsOrderLine line)
		{
			return line == null ? null : TrackingWhsOrderLine.GetTrackingWhsOrderLine(line);
		}

		public static TrackingWhsReceive Get(WhsReceive receive)
		{
			return receive == null ? null : TrackingWhsReceive.GetTrackingWhsReceive(receive);
		}

		public static TrackingWhsReceiveLine Get(WhsReceiveLine line)
		{
			return line == null ? null : TrackingWhsReceiveLine.GetTrackingWhsReceiveLine(line);
		}
	}
}
