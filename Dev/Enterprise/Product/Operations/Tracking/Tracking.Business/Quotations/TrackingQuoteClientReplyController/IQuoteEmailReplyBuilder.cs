using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Quotations
{
	public interface IQuoteEmailReplyBuilder
	{
		EmailDef CreateEmailForAcceptance(OrgContact loggedInUser);

		EmailDef CreateEmailForRequestFurtherDiscussion(OrgContact loggedInUser);
	}
}
