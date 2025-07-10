namespace Enterprise.TransportCommon.Shared
{
	public class BookingInstructionStatuses : BookingStatuses
	{
		public BookingInstructionStatuses()
		{
			RemoveCode(Codes.Deactivated);
			RemoveCode(Codes.DeliveredEmptyNotReturned);
			RemoveCode(Codes.ActionRequired);
			RemoveCode(Codes.Held);
			RemoveCode(Codes.Quote);
			RemoveCode(Codes.ServiceCommenced);
			// BookingInstructionStatuses should not have any codes that aren't mentioned in BookingStatuses or TransportStatuses
			// Rationale: If that happens, then methods providing a description of a given booking instruction status will have to be modified
		}
	}
}
