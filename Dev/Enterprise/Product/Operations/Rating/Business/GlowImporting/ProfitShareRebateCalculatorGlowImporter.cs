using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class ProfitShareRebateCalculatorGlowImporter : RateLineItemGlowImporter<ProfitShareRebateCalculator, RatingCalculatorColumns.ProfitShareRebate>
	{
		protected override bool ImportCore(
			ProfitShareRebateCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.ProfitShareRebate, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.ProfitShareRebate.UseZeroWhenLoss, nameof(calculator.ZeroWhenLoss)),
				(RatingCalculatorColumns.ProfitShareRebate.Percentage, nameof(calculator.Percent)),
				(RatingCalculatorColumns.ProfitShareRebate.Minimum, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.ProfitShareRebate.BasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.ProfitShareRebate.Maximum, nameof(calculator.Maximum))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.ProfitShareRebate.AgentPercentage, nameof(calculator.Percent)),
				(RatingCalculatorColumns.ProfitShareRebate.AgentMinimum, nameof(calculator.Minimum)),
				(RatingCalculatorColumns.ProfitShareRebate.AgentBasePrice, nameof(calculator.BaseRate)),
				(RatingCalculatorColumns.ProfitShareRebate.AgentMaximum, nameof(calculator.Maximum))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataPair<RatingCalculatorColumns.ProfitShareRebate>(RateLineItemsSchema.TM_Type, CalculatorConstants.Type.ApplyTo, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.ProfitShareRebate.ApplyToType)
			);
		}
	}
}
