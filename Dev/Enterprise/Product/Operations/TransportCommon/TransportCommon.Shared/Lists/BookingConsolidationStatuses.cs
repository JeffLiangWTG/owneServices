namespace Enterprise.TransportCommon.Shared
{
	public class BookingConsolidationStatuses : BookingStatuses
	{
		public BookingConsolidationStatuses()
		{
			RemoveCode(Codes.Deactivated);
			RemoveCode(Codes.Quote);
		}
	}
}
