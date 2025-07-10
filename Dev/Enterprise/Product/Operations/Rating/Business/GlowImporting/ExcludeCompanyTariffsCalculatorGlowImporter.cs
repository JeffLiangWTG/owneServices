using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	internal class ExcludeCompanyTariffsCalculatorGlowImporter : RateLineItemGlowImporter<ExcludeCompanyTariffsCalculator, RatingCalculatorColumns.ExcludeCompanyTariffs>
	{
		protected override bool ImportCore(
			ExcludeCompanyTariffsCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.ExcludeCompanyTariffs, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			return true;
		}
	}
}
