using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class UnitCalculatorGlowImporter : RateLineItemGlowImporter<UnitCalculator, RatingCalculatorColumns.Unit>
	{
		protected override bool ImportCore(
			UnitCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Unit, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Unit.PerUnitPrice, nameof(calculator.PerUnit))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.Unit.AgentPerUnitPrice, nameof(calculator.PerUnit))
			);

			return true;
		}
	}
}
