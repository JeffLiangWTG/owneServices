using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingStmNoteCollectionWithRelatedElements : StmNoteCollectionWithRelatedElements
	{
		public QuotedBookingStmNoteCollectionWithRelatedElements(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		protected override bool IsRelatedNote(StmNote note)
		{
			return !((QuotedBooking)Master).IsNoteParent(note);
		}
	}
}
