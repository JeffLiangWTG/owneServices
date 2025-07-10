using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingStmNoteCollectionView : StmNoteCollectionView
	{
		public QuotedBookingStmNoteCollectionView(QuotedBooking quotedBooking)
			: base(quotedBooking, typeof(QuotedBookingStmNote))
		{
		}

		protected override bool IsRelatedNote(StmNote note)
		{
			return !note.IsDeleted && !((QuotedBooking)Parent).IsNoteParent(note);
		}
	}
}
