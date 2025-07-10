using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class FlatPlusPerUnitCalculatorGlowImporter : RateLineItemGlowImporter<FlatPlusPerUnitCalculator, RatingCalculatorColumns.FlatPlusPerUnit>
	{
		protected override bool ImportCore(
			FlatPlusPerUnitCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.FlatPlusPerUnit, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.FlatPlusPerUnit.BasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.FlatPlusPerUnit.PerUnitPrice, nameof(calculator.PerUnit))
			);
			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.FlatPlusPerUnit.AgentBasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.FlatPlusPerUnit.AgentPerUnitPrice, nameof(calculator.PerUnit))
			);

			return true;
		}
	}
}
