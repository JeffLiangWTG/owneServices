using System;
using System.Linq;
using System.Text;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class QuoteClientReplyRequestHandler : DataRequestHandler<QuoteClientReplyRequestHelper>
	{
		/// <summary>
		/// The nameof this enum and its values are used as the request
		/// parameter name and value. Except for None, which is only used to
		/// detect if the wrong GetHandlerURL method was called.
		/// </summary>
		public enum ClientReply
		{
			None,
			AcceptQuotation,
			RequestFurtherDiscussion
		}

		public override ZBlob GetBinaryData()
		{
			var currentUser = HttpContext.Current.Session["SiteUser"] as OrgContactWebUser;
			var loggedInUser = currentUser?.LoggedInUser;

			var quotePK = PKs.FirstOrDefault();
			Argument.NotNull(quotePK, nameof(PKs), "Expected at least one PK provided. Found zero");
			var quote = Factory.Load<Quote>(quotePK);
			Argument.NotNull(quote, nameof(quote), "Expected the quote to exist in the db");

			var requestKind = GetRequestKind(QueryString.Get(nameof(ClientReply)));

			if (!clientReplyController.IsClientReplyAllowed(quote, currentUser as TrackingSiteUser))
			{
				return CouldNotHandle(quote, requestKind);
			}

			var response = string.Empty;
			var quoteEmailReplyBuilder = new QuoteEmailReplyBuilder(quote);
			if (requestKind == ClientReply.AcceptQuotation)
			{
				clientReplyController.ClientResponseIsAcceptedQuotation(quoteEmailReplyBuilder, loggedInUser);
				return Handled(quote, requestKind);
			}
			else if (requestKind == ClientReply.RequestFurtherDiscussion)
			{
				clientReplyController.ClientResponseIsRequestFurtherDiscussion(quoteEmailReplyBuilder, loggedInUser);
				return Handled(quote, requestKind);
			}
			else
			{
				return CouldNotHandle(quote, requestKind);
			}
		}

		ZBlob Handled(Quote quote, ClientReply requestKind)
		{
			var quoteBusinessObjectName = GetQuoteBusinessObjectName(quote);

			string response;
			if (requestKind == ClientReply.AcceptQuotation)
			{
				response = ResString.GetMultilingualString("7c9c40a8-6180-49b3-a386-9bdf6540c81e", "{0} {1} is accepted. Thank you.", quoteBusinessObjectName, quote.TH_QuoteNumber);
			}
			else /* (requestKind == ClientReply.RequestFurtherDiscussion) */
			{
				response = ResString.GetMultilingualString("66e4451e-8f18-4516-83bb-2cd74d3bb82c", "Request for discussion is sent for {0} {1}. Thank you.", quoteBusinessObjectName, quote.TH_QuoteNumber);
			}

			return new ZBlob(Encoding.ASCII.GetBytes(response));
		}

		ZBlob CouldNotHandle(Quote quote, ClientReply requestKind)
		{
			var response = string.Empty;
			var quoteBusinessObjectName = GetQuoteBusinessObjectName(quote);

			if (requestKind == ClientReply.AcceptQuotation)
			{
				response = quote.TH_OneTimeQuote
					? ResString.GetMultilingualString("8b2bdd7b-9afd-4e49-8559-a15752c23122", "You will need to be granted Web Quoting security access to accept One Off Quote through e-mail. Please check with your Administrator.")
					: ResString.GetMultilingualString("f60b9839-1305-4c12-89cc-0b6c176245f6", "{0} {1} could NOT be accepted at this time.", quoteBusinessObjectName, quote.TH_QuoteNumber);
			}
			else /* (requestKind == ClientReply.RequestFurtherDiscussion) */
			{
				response = quote.TH_OneTimeQuote
					? ResString.GetMultilingualString("e0ddb97d-0c41-4a36-af02-80ff9dfe2cb2", "You will need to be granted Web Quoting security access to request for discussion via e-mail. Please check with your Administrator.")
					: ResString.GetMultilingualString("ac5b7489-6b0f-425a-92a9-28b2fc055acc", "Request for discussion could NOT be sent for {0} {1} at this time.", quoteBusinessObjectName, quote.TH_QuoteNumber);
			}

			return new ZBlob(Encoding.ASCII.GetBytes(response));
		}

		ClientReply GetRequestKind(string clientReplyValue)
		{
			if (clientReplyValue == nameof(ClientReply.AcceptQuotation))
			{
				return ClientReply.AcceptQuotation;
			}
			else if (clientReplyValue == nameof(ClientReply.RequestFurtherDiscussion))
			{
				return ClientReply.RequestFurtherDiscussion;
			}
			else
			{
				throw new InvalidOperationException($"Received a client reply of {clientReplyValue} when only 'AcceptQuotation' and 'RequestFurtherDiscussion' are allowed");
			}
		}

		string GetQuoteBusinessObjectName(Quote quote) => quote.TH_OneTimeQuote
			? ResString.GetMultilingualString("e53dac37-e980-4497-b347-9d7ac7fbe2c9", "One Off Quote")
			: ResString.GetMultilingualString("92fe8eec-01d6-4a83-91ad-bd73e9047e82", "Quotation");

		public override string ContentType => "text/plain"; // Content type.

		protected override ContentDispositionType ContentDispositionType => ContentDispositionType.Inline;

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return null;
		}

		public override string FileName => "response.txt";

		ITrackingQuoteClientReplyController clientReplyController = new TrackingQuoteClientReplyController();

#if DEBUG
		internal void SetControllerForTest(ITrackingQuoteClientReplyController controller)
		{
			clientReplyController = controller;
		}
#endif
	}
}
