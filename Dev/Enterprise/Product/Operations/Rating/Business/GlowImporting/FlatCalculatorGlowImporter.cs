using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class FlatCalculatorGlowImporter : RateLineItemGlowImporter<FlatCalculator, RatingCalculatorColumns.Flat>
	{
		protected override bool ImportCore(
			FlatCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Flat, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Flat.BasePrice, nameof(calculator.BaseRate))
			);
			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.Flat.AgentBasePrice, nameof(calculator.BaseRate))
			);

			return true;
		}
	}
}
