using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class TimeCalculatorGlowImporter : RateLineItemGlowImporter<TimeCalculator, RatingCalculatorColumns.Time>
	{
		protected override bool ImportCore(
			TimeCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Time, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Time.UseCumulativeBreaks, nameof(calculator.IsAccumulated)),
				(RatingCalculatorColumns.Time.UseHigherBreakLowerRate, nameof(calculator.UseHigherChargeableLowerRateRule)),
				(RatingCalculatorColumns.Time.HolidaysToExclude, nameof(calculator.ExcludeHolidays))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.Time.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.Time.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.Time.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.Time.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_BreakWeightVolume, RatingCalculatorColumns.Time.Units),
				new RateLineItemImportDataPair<RatingCalculatorColumns.Time>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.Time.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Time.RestrictedReason)
			);
		}
	}
}
