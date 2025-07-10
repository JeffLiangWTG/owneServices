using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class QuoteDocumentSupporterOneOffQuoteTest : QuoteDocumentSupporterBaseTest
	{
		protected override Quote GetQuote()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_Email = "test@wisetechglobal.com";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.Code;
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;

			Factory.Save();

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);
			var quote = (Quote)quotedBooking.Quote;
			quote.SpotQuoteChargesIncorrect += delegate
			{ };
			quote.QuotationClientAddress.OrganisationPK = orgHeader.PK;

			Factory.Save();

			return quote;
		}
	}
}
