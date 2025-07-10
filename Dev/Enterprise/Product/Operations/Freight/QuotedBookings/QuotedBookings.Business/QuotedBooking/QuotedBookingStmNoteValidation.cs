using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingStmNoteValidation : StmNoteValidation
	{
		public QuotedBookingStmNoteValidation(QuotedBookingStmNote parent)
			: base(parent)
		{
		}

		public new QuotedBookingStmNote Parent
		{
			get { return (QuotedBookingStmNote)base.Parent; }
		}
	}
}
