namespace Enterprise.Rating.Business
{
	class QuotationLineListHelperClientRateTest : QuotationLineListHelperBaseTest
	{
		protected override string TestConsolidatedLines_DifferentOrigin_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
Origin Documentation Fee|||
|USD|10.00|
|USD|20.00|
|IDR|21.00|
";

		protected override string TestConsolidatedLines_DifferentDestination_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
Origin Documentation Fee|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		// Header "Origin Document Fee" is shown 2x because commodity is used as GroupBy in DocStrip "Forwarding Non-Quote Rate Table Loose (Landscape)" and "Forwarding Non-Quote Rate Table Non-Loose (Landscape)"
		protected override string TestConsolidatedLines_DifferentCommodity_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
Origin Documentation Fee|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override string TestConsolidatedLines_DifferentCarrierServiceLevel_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
|USD|10.00|

-  Carrier Service Level: AM|AUD|20.00|

-  Carrier Service Level: XX|AUD|21.00|
";

		// Header "Origin Documentation Fee|" is shown 2x because ServiceLevel is used as GroupBy in DocStrip "Forwarding Non-Quote Rate Table Loose (Landscape)" and "Forwarding Non-Quote Rate Table Non-Loose (Landscape)"
		protected override string TestConsolidatedLines_DifferentServiceLevel_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
Origin Documentation Fee|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		// Header "Origin Documentation Fee|" is shown 2x because TransportProvider is used as GroupBy in DocStrip "Forwarding Non-Quote Rate Table Loose (Landscape)" and "Forwarding Non-Quote Rate Table Non-Loose (Landscape)"
		protected override string TestConsolidatedLines_DifferentTransportProvider_ExpectedOutput => @"
International Freight|||
Origin Documentation Fee|||
Origin Documentation Fee|||
|USD|10.00|
|AUD|20.00|
|AUD|21.00|
";

		protected override RatingHeader RatingHeader => clientRate ?? (clientRate = TestHelper.NewClientRate(TestHelper.NewOrgHeader(1)));
		ClientRate clientRate;
	}
}
