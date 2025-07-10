namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CostBasedCalculatorTest : CompanyTariffOrCostBasedCalculatorBaseTest
	{
		#region Implementation

		protected override RatingHeader RatingHeader => ratingHeader ?? (ratingHeader = Helper.NewCosting(TransportProvider1));
		RatingHeader ratingHeader;

		protected override string CalculatorCode => CompanyTariffOrCostBasedCalculator.CostBasedCode;

		protected override SimpleArInfo[] ExpectedChargesForNotMatchingRatesButMatchingOriginRate => System.Array.Empty<SimpleArInfo>();

		protected override SimpleArInfo[] ExpectedChargesForMatchingClientRateButNotMatchingOriginalRate => new[] { new SimpleArInfo { Amount = 0m, CalculationSingleLineDescription = @"ODOC: Calculation failed due to charge code is using the Cost Based Calculator, however there are conflicting or no costing rates found.", InvoiceLineDesc = "Origin Documentation Fee" } };

		#endregion
	}
}
