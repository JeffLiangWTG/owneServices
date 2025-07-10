using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class ValueRangeCalculatorGlowImporter : RateLineItemGlowImporter<ValueRangeCalculator, RatingCalculatorColumns.ValueRange>
	{
		protected override bool ImportCore(
			ValueRangeCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.ValueRange, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator, (RatingCalculatorColumns.ValueRange.ApplyTo, nameof(calculator.ApplyTo)));

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.ValueRange>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.ValueRange.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.ValueRange>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.ValueRange.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.ValueRange>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.ValueRange.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.ValueRange>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.ValueRange.AgentRate),
				new RateLineItemImportDataPair<RatingCalculatorColumns.ValueRange>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.ValueRange.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.ValueRange.RestrictedReason)
			);
		}
	}
}
