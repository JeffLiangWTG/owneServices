using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	public interface ITrackingQuoteClientReplyController
	{
		/// <summary>
		/// When the client responds that they wish to accept the quotation
		/// </summary>
		/// <param name="quoteEmailReplyBuilder">Email builder which has the quote details</param>
		/// <param name="loggedInUser">The user who clicked on their email.</param>
		void ClientResponseIsAcceptedQuotation(IQuoteEmailReplyBuilder quoteEmailReplyBuilder, OrgContact loggedInUser);
		/// <summary>
		/// When the client responds that they wish to request further discussion about the quotation
		/// </summary>
		/// <param name="quoteEmailReplyBuilder">Email builder which has the quote details</param>
		/// <param name="loggedInUser">The user who clicked on their email.</param>
		void ClientResponseIsRequestFurtherDiscussion(IQuoteEmailReplyBuilder quoteEmailReplyBuilder, OrgContact loggedInUser);

		bool IsClientReplyAllowed(Quote quote, TrackingSiteUser trackingSiteUser);
	}
}
