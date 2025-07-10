using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	public class TrackingQuoteClientReplyController : ITrackingQuoteClientReplyController
	{
		public void ClientResponseIsAcceptedQuotation(IQuoteEmailReplyBuilder quoteEmailReplyBuilder, OrgContact loggedInUser)
		{
			var emailDefinition = quoteEmailReplyBuilder.CreateEmailForAcceptance(loggedInUser);
			if (emailDefinition != null)
			{
				Env.OutgoingMailManager.CreateAndSave(emailDefinition);
			}
		}

		public void ClientResponseIsRequestFurtherDiscussion(IQuoteEmailReplyBuilder quoteEmailReplyBuilder, OrgContact loggedInUser)
		{
			var emailDefinition = quoteEmailReplyBuilder.CreateEmailForRequestFurtherDiscussion(loggedInUser);
			if (emailDefinition != null)
			{
				Env.OutgoingMailManager.CreateAndSave(emailDefinition);
			}
		}

		public bool IsClientReplyAllowed(Quote quote, TrackingSiteUser trackingSiteUser)
		{
			return quote.QuoteStatus == Quote.QuoteStatusOptions.Finalized
				&& trackingSiteUser?.LoggedInUser != null
				&& trackingSiteUser.CanViewQuotations;
		}
	}
}
