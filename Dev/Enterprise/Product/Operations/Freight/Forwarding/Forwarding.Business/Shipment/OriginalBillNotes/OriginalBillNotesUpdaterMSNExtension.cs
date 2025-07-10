using Enterprise.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	internal static class OriginalBillNotesUpdaterMSNExtension
	{
		public static void OnSelfPublish(this ForwardingShipment shipment, IStmALog log)
		{
			new OriginalBillNotesUpdaterForSelfPublish(shipment, log).PopulateOriginalBillNotes();
		}
	}
}
