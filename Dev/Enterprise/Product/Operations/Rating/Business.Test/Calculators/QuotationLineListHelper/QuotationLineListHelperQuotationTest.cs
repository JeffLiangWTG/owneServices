namespace Enterprise.Rating.Business
{
	internal class QuotationLineListHelperQuotationTest : QuotationLineListHelperBaseTest
	{
		protected override string TestConsolidatedLines_DifferentOrigin_ExpectedOutput => @"
International Freight|||
|USD|10.00|
|USD|20.00|
|IDR|21.00|
";

		protected override string TestConsolidatedLines_DifferentDestination_ExpectedOutput => @"
International Freight|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override string TestConsolidatedLines_DifferentCommodity_ExpectedOutput => @"
International Freight|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override string TestConsolidatedLines_DifferentCarrierServiceLevel_ExpectedOutput => @"
International Freight|||
|USD|10.00|

-  Carrier Service Level: AM|AUD|20.00|

-  Carrier Service Level: XX|AUD|21.00|
";

		protected override string TestConsolidatedLines_DifferentServiceLevel_ExpectedOutput => @"
International Freight|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override string TestConsolidatedLines_DifferentTransportProvider_ExpectedOutput => @"
International Freight|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override RatingHeader RatingHeader => quote ?? (quote = TestHelper.NewQuote(TestHelper.NewOrgHeader(1)));
		Quote quote;
	}
}
