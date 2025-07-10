using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class MinimumOrPerUnitCalculatorGlowImporter : RateLineItemGlowImporter<MinimumOrPerUnitCalculator, RatingCalculatorColumns.MinimumOrPerUnit>
	{
		protected override bool ImportCore(
			MinimumOrPerUnitCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.MinimumOrPerUnit, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.MinimumOrPerUnit.MinimumAmount, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.MinimumOrPerUnit.PerUnitAmount, nameof(calculator.PerUnit))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.MinimumOrPerUnit.AgentMinimumAmount, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.MinimumOrPerUnit.AgentPerUnitAmount, nameof(calculator.PerUnit))
			);

			return true;
		}
	}
}
