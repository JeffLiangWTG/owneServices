using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class PercentageCalculatorGlowImporter : RateLineItemGlowImporter<PercentageCalculator, RatingCalculatorColumns.Percentage>
	{
		protected override bool ImportCore(
			PercentageCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Percentage, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Percentage.IncludeGST, nameof(calculator.IncludeGST)),
				(RatingCalculatorColumns.Percentage.UseTakeHighestCharge, nameof(calculator.GreaterCharge)),
				(RatingCalculatorColumns.Percentage.UsePartThereof, nameof(calculator.IsPartThereof)),

				(RatingCalculatorColumns.Percentage.PartThereofRate, nameof(calculator.Rate)),
				(RatingCalculatorColumns.Percentage.PartThereofValue, nameof(calculator.ValueOrPartThereOf)),
				(RatingCalculatorColumns.Percentage.Percentage, nameof(calculator.Percent)),
				(RatingCalculatorColumns.Percentage.Minimum, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.Percentage.BasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.Percentage.Maximum, nameof(calculator.Maximum))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.Percentage.AgentPartThereofRate, nameof(calculator.Rate)),
				(RatingCalculatorColumns.Percentage.AgentPartThereofValue, nameof(calculator.ValueOrPartThereOf)),
				(RatingCalculatorColumns.Percentage.AgentPercentage, nameof(calculator.Percent)),
				(RatingCalculatorColumns.Percentage.AgentMinimum, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.Percentage.AgentBasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.Percentage.AgentMaximum, nameof(calculator.Maximum))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataPair<RatingCalculatorColumns.Percentage>(RateLineItemsSchema.TM_Type, CalculatorConstants.Type.ApplyTo, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Percentage.ApplyToType)
			);
		}
	}
}
