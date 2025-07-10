using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Web;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

#if DEBUG
using CargoWise.Common.Testing;
#endif

namespace Enterprise.Tracking.Web
{
	public class QuoteRequestHandler : DataRequestHandler<QuoteRequestHelper>
	{
		#region Instance

		public static QuoteRequestHandler Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new QuoteRequestHandler();
				}

				return fInstance;
			}
		}
#if DEBUG
		[SuppressThreadStaticFieldMessage]
#endif
		static QuoteRequestHandler fInstance;

		#endregion

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			List<QuotedBooking> spotQuotes = new List<QuotedBooking>();

			foreach (ZGuid pK in PKs)
			{
				QuotedBooking spotQuote = CreateQuotedBooking(pK);
				if (spotQuote.Quote?.Company != null)
				{
					spotQuote.Quote.SpotQuoteChargesIncorrect += new System.ComponentModel.CancelEventHandler(Quote_SpotQuoteChargesIncorrect);
					spotQuotes.Add(spotQuote);
					if (quoteBranch == null && spotQuote.Quote.Company.PK != GlbCompany.CurrentCompany.PK)
					{
						quoteBranch = spotQuote.Quote.Company.Branches[0];
					}
				}
			}

			return spotQuotes.ToArray();
		}
		GlbBranch quoteBranch;

		void Quote_SpotQuoteChargesIncorrect(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !WebDataRegistry.Instance.SaveQuotesWithoutRates.Value;
		}

		QuotedBooking CreateQuotedBooking(ZGuid quotedPK)
		{
			return QuotedBooking.New(quotedPK, ZGuid.Empty, Factory);
		}

		QuotedBooking[] Quotes
		{
			get { return BusinessObjects as QuotedBooking[]; }
		}

		public override ZBlob GetBinaryData()
		{
			ZBlob result = null;
			if (Quotes != null && Quotes.Length > 0 && quoteBranch != null)
			{
				using (WebLoginBranch spotQuoteBranch = new WebLoginBranch(quoteBranch))
				{
					lock (Quotes[0])
					{
#if DEBUG
						StopStopwatchForTesting();
#endif
						result = GetQuotationDocumentCore(Quotes);
					}
				}
			}
			else if (Quotes != null && Quotes.Length > 0)
			{
				lock (Quotes[0])
				{
#if DEBUG
					StopStopwatchForTesting();
#endif
					result = GetQuotationDocumentCore(Quotes);
				}
			}

			return result;
		}

		ZBlob GetQuotationDocumentCore(QuotedBooking[] spotQuotes)
		{
			using (DocumentPack docPack = new DocumentPack())
			{
				foreach (var spotQuote in spotQuotes)
				{
					AddToDocPack(docPack, spotQuote);
				}

				return new DocumentUtility(Factory).GetDocument(docPack, (OrgContact)AppInstance.SiteUser.LoggedInUser, ContentType);
			}
		}

		void AddToDocPack(DocumentPack docPack, QuotedBooking spotQuote)
		{
			var quoteHelper = new TrackingQuotationHelper(Factory, (OrgContact)AppInstance.SiteUser.LoggedInUser, spotQuote);
			if (quoteHelper.ChargesAreCorrectOrIgnored)
			{
				if (quoteHelper.QuotationDocumentPack != null)
				{
					docPack.AddRange(quoteHelper.QuotationDocumentPack);
				}
			}
		}

		public override string ContentType
		{
			get { return DataContentTypes.Pdf; }
		}

		public override string FileName
		{
			get { return (NoResString)"Quote" + QuoteNumbers + (NoResString)".pdf"; } // This is a filename
		}

		string QuoteNumbers
		{
			get
			{
				ZStringBuilder numbers = new ZStringBuilder();

				foreach (QuotedBooking spotQuote in Quotes)
				{
					if (spotQuote.Quote != null)
					{
						numbers.Append(spotQuote.Quote.TH_QuoteNumber);
					}
				}

				return numbers.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public override string NoDataErrorMessage
		{
			get { return Res.GetString("05E7F75B-AD6F-4CC1-B5D2-62C43289FBC6", "No matching rates were found. Please contact your sales representative to obtain a Spot Quote."); }
		}
	}
}
