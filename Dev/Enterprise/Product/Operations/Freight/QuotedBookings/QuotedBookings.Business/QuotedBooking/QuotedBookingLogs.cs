using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[System.Diagnostics.DebuggerDisplay("Count = {ElementsInternal.Count}")]
	public class QuotedBookingLogs : Logs
	{
		public QuotedBookingLogs(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		public new QuotedBooking Parent
		{
			get { return (QuotedBooking)base.Parent; }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			var additionalQuery = new ZQuery(StmALogSchema.SL_Table, ViewQuotedBookingSchema.Constants.TableName);
			var logs = new StmALogDependentCollection(Parent, additionalQuery);
			logs.CountChanged += QuotedBookingLogs_CountChanged;
			return logs;
		}

		void QuotedBookingLogs_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded
				&& e.BizObject is StmALog log
				&& !log.IsInDatabase
				&& log.SL_SE_NKEvent == Events.DocumentAllocatedCode
				&& sender is BusinessObjectCollection collection
				&& !collection.IsLoading
				&& Parent.Booking is ForwardingShipment shipment
				&& shipment.JS_IsForwardRegistered)
			{
				ErrorReporter.ReportOnce(
					"Attempting to add new event log for quoted booking which already has been converted to forwarding shipment",
					$"Event Log is created for quoted booking which has been converted to Shipment#: {shipment.JS_UniqueConsignRef}\nEvent: {log.SL_SE_NKEvent} - {log.SL_TableFriendlyName}");
			}
		}
	}
}
