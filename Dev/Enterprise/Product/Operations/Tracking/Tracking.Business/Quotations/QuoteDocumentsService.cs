using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Web;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Business
{
	public class QuoteDocumentsService : IQuoteDocumentsService
	{
		public IWebTrackerPrintResult Print(Guid contactPK, Guid quotePK)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(QuoteDocumentsService) };

			var contact = factory.Load<OrgContact>(new ZGuid(contactPK));

			if (contact == null)
			{
				return null;
			}

			var vQuotedBooking = factory.Load<ViewQuotedBooking>(quotePK);
			if (vQuotedBooking == null)
			{
				return null;
			}

			var quote = QuotedBooking.New(vQuotedBooking, factory);
			if (quote.Quote?.Company == null)
			{
				return null;
			}

			using (quote.Quote.Company.PK != GlbCompany.CurrentCompany.PK ? new WebLoginBranch(quote.Quote.Company.Branches[0]) : null)
			{
				if (!HasAccessToQuote(contact, quote.Quote))
				{
					return null;
				}

				if (quote.Quote.TH_OneTimeQuote && !quote.Quote.HasNonZeroSellAmt && !WebDataRegistry.Instance.SaveQuotesWithoutRates.Value)
				{
					return new QuoteDocumentPrintResult
					{
						ErrorMessage = ResString.GetMultilingualString("b796664e-bfac-4cf7-a0c0-f04e9a90d348", "No matching rates were found. Please contact your sales representative to obtain a Spot Quote.")
					};
				}

				quote.Quote.SpotQuoteChargesIncorrect += (object sender, System.ComponentModel.CancelEventArgs e) => { };
				var quoteNumber = quote.Quote.TH_QuoteNumber;

				var quoteHelper = new TrackingQuotationHelper(factory, contact, quote);
				var pdfData = new DocumentUtility(factory).GetDocument(quoteHelper.QuotationDocumentPack, contact, DataContentTypes.Pdf);

				return new QuoteDocumentPrintResult
				{
					FileName = $"Quote {quoteNumber}.pdf", // File Name
					FileContents = new MemoryStream(pdfData),
				};
			}
		}

		static bool HasAccessToQuote(OrgContact contact, Quote quote)
		{
			if (contact.ParentOrg.Addresses.Contains(quote.QuotationClientAddress.Address))
			{
				return true;
			}

			string[] allowedPartyTypes =
			[
				RelatedPartyTypeList.Codes.ShipperBroker,
				RelatedPartyTypeList.Codes.ControllingCustomer,
				RelatedPartyTypeList.Codes.ManagementGrouping
			];

			return allowedPartyTypes
				.Select(partyType => quote.QuotationClientAddress.Organisation.GetRelatedParty(partyType, ZString.Empty))
				.Any(relatedParty => relatedParty != null && relatedParty.PK == contact.ParentOrg.PK);
		}
	}
}
