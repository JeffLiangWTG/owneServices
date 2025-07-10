

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Quotations
{
	public class QuoteEmailReplyBuilder : IQuoteEmailReplyBuilder
	{
		public QuoteEmailReplyBuilder(Quote quote)
		{
			Argument.NotNull(quote, nameof(quote));
			this.quote = quote;
		}

		readonly Quote quote;

		public EmailDef CreateEmailForAcceptance(OrgContact loggedInUser)
		{
			quote.TH_ClientAccepted = ZDateTime.Now;
			quote.Logs.AddNew(AutoEvents.QuotationClientAccepted);

			var propertiesProvider = GetTrackingQuotePropertiesProvider();

			var emailAddresses = propertiesProvider.GetEmailAddresses(quote);

			if (emailAddresses.Length < 1)
			{
				return null;
			}

			var def = new EmailDef();
			def.Subject = GetSubject();
			def.Body = GetBody(true, loggedInUser, quote.TH_ClientAccepted);
			def.ContentType = EmailContentTypes.HTML;
			emailAddresses.ForEach(e => def.AddRecipientForUserCommunication(e));

			quote.Factory.Save();

			return def;
		}

		public EmailDef CreateEmailForRequestFurtherDiscussion(OrgContact loggedInUser)
		{
			quote.Logs.AddNew(AutoEvents.QuotationClientRequestedDiscussion);
			quote.Factory.Save();

			var propertiesProvider = GetTrackingQuotePropertiesProvider();

			var emailAddresses = propertiesProvider.GetEmailAddresses(quote);
			if (emailAddresses.Length < 1)
			{
				return null;
			}

			var def = new EmailDef();
			def.Subject = GetSubject();
			def.Body = GetBody(false, loggedInUser, ZDateTime.Now);
			def.ContentType = EmailContentTypes.HTML;
			emailAddresses.ForEach(e => def.AddRecipientForUserCommunication(e));

			quote.Factory.Save();

			return def;
		}

		ZString GetBody(bool isQuoteAccepted, OrgContact loggedInUser, ZDateTime timeOfReply)
		{
			var propertiesProvider = GetTrackingQuotePropertiesProvider();

			var finalizedTime = quote.Logs.Find(GetFinalisedLogQuery()).FirstOrDefault()?.SL_EventTime ?? ZDateTime.Empty;
			if (finalizedTime == ZDateTime.Empty)
			{
				throw new InvalidOperationException($"Could not find Finalized log for QuotePK: '{quote.PK}'. It should have been there otherwise the {propertiesProvider.QuoteBusinessObjectName} cannot be accepted or have further discussion requested");
			}

			var emailMessageLine = ResString.GetMultilingualString("9b1c7708-6522-4cf4-a8c7-4abd3100e251", "{0} {1} is replied by {2} of {3} on {4}.", propertiesProvider.QuoteBusinessObjectName, quote.TH_QuoteNumber, loggedInUser.OC_ContactName, quote.TH_ClientCode, timeOfReply.ToLongTimeString());
			var emailLinkText = ResString.GetMultilingualString("92467916-f713-4fb8-9eb9-2265b3b46e75", "View {0}", propertiesProvider.QuoteBusinessObjectName);
			var quotationClientReply = isQuoteAccepted ? ResString.GetMultilingualString("aa0675ed-1765-4a85-836b-abf4b268143b", "{0} accepted", propertiesProvider.QuoteBusinessObjectName) : ResString.GetMultilingualString("c8d412c2-1707-4f29-916a-c4f34bdac49e", "Request further discussion");
			var emailClientReplyLine = ResString.GetMultilingualString("edfb928f-152a-4ef8-b192-60eb00ed46dd", "Client reply: {0}", quotationClientReply);

			var emailStartDateLine = ResString.GetMultilingualString("f5dc0039-20fa-4fbe-8718-5e36f46bb4eb", "{0} Start Date: {1}", propertiesProvider.QuoteBusinessObjectName, quote.TH_QuoteDate.ToShortDateString());

			var emailExpiryDateLine = quote.TH_OneTimeQuote ? ResString.GetMultilingualString("414ab436-fc4f-4aed-be8e-62614f82fc9c", "Expiry Date: {0}", quote.TH_QuoteEndDate.ToShortDateString())
				: ResString.GetMultilingualString("bfbead48-53eb-43b5-a54a-aa86d753955d", "{0} Expiry Date: {1}", propertiesProvider.QuoteBusinessObjectName, quote.TH_QuoteEndDate.ToShortDateString());

			var emailCreatedDateLine = quote.TH_OneTimeQuote ? ResString.GetMultilingualString("b66c0e5d-9714-45b5-aa71-1a638d4acfb4", "Created Time: {0}", quote.TH_SystemCreateTimeUtc.ToLongTimeString())
				: ResString.GetMultilingualString("668e042e-6635-471e-a1a8-493fe9712cd9", "{0} Created Time: {1}", propertiesProvider.QuoteBusinessObjectName, quote.TH_SystemCreateTimeUtc.ToLongTimeString());

			var emailFinalizedDateLine = quote.TH_OneTimeQuote ? ResString.GetMultilingualString("0fd95694-8000-4042-ae46-f51b940b9768", "Finalized Time: {0}", finalizedTime.ToLongTimeString())
				: ResString.GetMultilingualString("7c472f49-4a33-4381-8b9a-0fc1e074b672", "{0} Finalized Time: {1}", propertiesProvider.QuoteBusinessObjectName, finalizedTime.ToLongTimeString());

			return $@"<html><body><p>{emailMessageLine}<a href='{GetHyperlinkToViewQuote()}'>{emailLinkText}</a></p>
<p>{emailClientReplyLine}</p>
<br/>
<p>{emailStartDateLine}</p>
<p>{emailExpiryDateLine}</p>
<p>{emailCreatedDateLine}</p>
<p>{emailFinalizedDateLine}</p>
</body>
</html>"; // HTML code for an email.
		}

		ZQuery GetFinalisedLogQuery()
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuotationFinalisedPrinted.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, quote.PK);

			return query;
		}

		ZString GetSubject()
		{
			var propertiesProvider = GetTrackingQuotePropertiesProvider();

			var client = quote.Factory.Load<OrgHeader>(quote.TH_OH);
			return ResString.GetMultilingualString("ed0c644b-43f3-46ca-a9e2-d7a7c739d57d", "{0} {1} is replied by Client {2}", propertiesProvider.QuoteBusinessObjectName, quote.TH_QuoteNumber, client.OH_FullName);
		}

		string GetHyperlinkToViewQuote()
		{
			if (quote?.TH_OneTimeQuote ?? false)
			{
				return ShowViewFormUrlHandler.Instance.Create(ControllerIDs.OneOffQuotes, quote.PK.ToGuid());
			}

			return ShowViewFormUrlHandler.Instance.Create(ControllerIDs.Quotations, quote.PK.ToGuid());
		}

		ITrackingQuotePropertiesProvider GetTrackingQuotePropertiesProvider()
			=> quote.TH_OneTimeQuote
				? new TrackingOneOffQuotePropertiesProvider()
				: new TrackingQuotationPropertiesProvider();
	}
}
