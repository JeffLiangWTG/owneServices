using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class CombinedCalculatorGlowImporter : RateLineItemGlowImporter<CombinedCalculator, RatingCalculatorColumns.Combined>
	{
		protected override bool ImportCore(
			CombinedCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Combined, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Combined.UseInclusiveBreaks, nameof(calculator.UseInclusiveBreaks)),
				(RatingCalculatorColumns.Combined.UseCumulativeBreaks, nameof(calculator.IsAccumulated)),
				(RatingCalculatorColumns.Combined.UseHigherBreakLowerRate, nameof(calculator.UseHigherChargeableLowerRateRule))
			);

			return ImportRateLineItem(
			   calculator.RateLineBizO,
			   context,
			   directValues,
			   relationshipValues,

			   new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.Combined.Operator),
			   new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.Combined.Break),
			   new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.Combined.Rate),
			   new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.Combined.AgentRate),
			   new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.Combined.FlatAmount),
			   new RateLineItemImportDataPair<RatingCalculatorColumns.Combined>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.Combined.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Combined.RestrictedReason)
		   );
		}
	}
}
