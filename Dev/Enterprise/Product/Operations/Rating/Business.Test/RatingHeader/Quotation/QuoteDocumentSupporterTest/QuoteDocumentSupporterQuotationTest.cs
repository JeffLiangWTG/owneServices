namespace Enterprise.Rating.Business.Testing
{
	internal sealed class QuoteDocumentSupporterQuotationTest : QuoteDocumentSupporterBaseTest
	{
		protected override Quote GetQuote()
		{
			var quotation = Helper.NewQuote(Helper.NewOrgHeader());
			quotation.AddRateEntryWithFlatRateLine("ORG", "ALL", "AUSYD", "USLAX", "BAF", 100m);
			Factory.Save();

			return quotation;
		}
	}
}
