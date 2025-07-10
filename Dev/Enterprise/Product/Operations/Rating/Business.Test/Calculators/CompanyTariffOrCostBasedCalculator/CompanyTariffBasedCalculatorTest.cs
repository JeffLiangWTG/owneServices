namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CompanyTariffBasedCalculatorTest : CompanyTariffOrCostBasedCalculatorBaseTest
	{
		#region Implementation

		protected override RatingHeader RatingHeader => ratingHeader ?? (ratingHeader = Helper.NewCompanyTariff());
		RatingHeader ratingHeader;

		protected override string CalculatorCode => CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

		protected override SimpleArInfo[] ExpectedChargesForNotMatchingRatesButMatchingOriginRate => new[] { new SimpleArInfo { Amount = 10m, CalculationSingleLineDescription = @"ODOC: 1 Container(s) @ AUD 10.00/Container", InvoiceLineDesc = "Origin Documentation Fee" } };

		protected override SimpleArInfo[] ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate => new[] { new SimpleArInfo { Amount = 0m, CalculationSingleLineDescription = @"ODOC: Calculation failed due to charge code is using the Company Tariff Based Calculator, however there are conflicting or no tariff rates found.", InvoiceLineDesc = "Origin Documentation Fee" } };

		#endregion
	}
}
