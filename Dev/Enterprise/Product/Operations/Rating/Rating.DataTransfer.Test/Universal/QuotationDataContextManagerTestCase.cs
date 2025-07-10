using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(QuotationDataContextManager))]
	public class QuotationDataContextManagerTestCase : RatingHeaderDataContextManagerTestCase<QuotationDataContextManager, Quote>
	{
		public override void TestDataContextKey_GlobalRate()
		{
			Assert("Quotation cannot be a global rate. Should not test this case.", condition: true);
		}

		public override void TestLoadBusinessObjectFromDataSource_GlobalRate()
		{
			Assert("Quotation cannot be a global rate. Should not test this case.", condition: true);
		}

		protected override string CreateAndSetQuoteNumberForTestData(RatingHeader bizO)
		{
			bizO.TH_QuoteNumber = "QTE1234";
			// Why `QTE1234` with  `/A`? See Quote.GetNewQuoteNumberForAmendment which is eventually called when saving the quote bizO.
			return "QTE1234/A";
		}

		protected override Quote GetBusinessObjectForTesting() => Factory.NewWithValidTestData<Quote>();

		protected override RatingHeaderDataContextManager<Quote> GetNewContextManagerForTesting() => new QuotationDataContextManager();
	}
}
