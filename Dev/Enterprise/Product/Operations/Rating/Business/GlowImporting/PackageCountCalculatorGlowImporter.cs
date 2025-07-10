using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class PackageCountCalculatorGlowImporter : RateLineItemGlowImporter<PackageCountCalculator, RatingCalculatorColumns.PackageCount>
	{
		protected override bool ImportCore(
			PackageCountCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.PackageCount, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.PackageCount.BasicCharge, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.PackageCount.AdditionalPackage, nameof(calculator.AddtionalPackageRate)),
				(RatingCalculatorColumns.PackageCount.FirstPackage, nameof(calculator.FirstPackageRate)),
				(RatingCalculatorColumns.PackageCount.RatePerKg, nameof(calculator.PerKG))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.PackageCount.AgentBasicCharge, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.PackageCount.AgentAdditionalPackage, nameof(calculator.AddtionalPackageRate)),
				(RatingCalculatorColumns.PackageCount.AgentFirstPackage, nameof(calculator.FirstPackageRate)),
				(RatingCalculatorColumns.PackageCount.AgentRatePerKg, nameof(calculator.PerKG))
			);

			return true;
		}
	}
}
