namespace Enterprise.TransportCommon.Shared
{
	public class BookingStatuses : TransportStatuses
	{
		public BookingStatuses()
		{
			RemoveCode(Codes.Allocated);
			RemoveCode(Codes.Booked);
			RemoveCode(Codes.DeliveryAllocated);
			RemoveCode(Codes.Incomplete);
			RemoveCode(Codes.PickUpAllocated);
			RemoveCode(Codes.PickUpCommenced);
			RemoveCode(Codes.PickUpConfirmed);
			// BookingStatuses should not have any codes that aren't mentioned in TransportStatuses
			// Rationale: If that happens, then methods providing a description of a given booking status will have to be modified, like DtbTransport.StatusDescription
		}
	}
}
