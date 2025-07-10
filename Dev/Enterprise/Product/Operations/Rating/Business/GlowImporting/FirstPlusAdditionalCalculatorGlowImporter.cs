using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class FirstPlusAdditionalCalculatorGlowImporter : RateLineItemGlowImporter<FirstPlusAdditionalCalculator, RatingCalculatorColumns.FirstPlusAdditional>
	{
		protected override bool ImportCore(
			FirstPlusAdditionalCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.FirstPlusAdditional, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.FirstPlusAdditional.FirstItem, nameof(calculator.First)),
				(RatingCalculatorColumns.FirstPlusAdditional.AdditionalItem, nameof(calculator.Additional))
			);
			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.FirstPlusAdditional.AgentFirstItem, nameof(calculator.First)),
				(RatingCalculatorColumns.FirstPlusAdditional.AgentAdditionalItem, nameof(calculator.Additional))
			);

			return true;
		}
	}
}
