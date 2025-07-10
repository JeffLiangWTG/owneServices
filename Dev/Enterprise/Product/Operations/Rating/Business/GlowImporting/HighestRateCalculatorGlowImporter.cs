using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class HighestRateCalculatorGlowImporter : RateLineItemGlowImporter<HighestRateCalculator, RatingCalculatorColumns.HighestRate>
	{
		protected override bool ImportCore(
			HighestRateCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.HighestRate, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.HighestRate.UnitAsFreighted, nameof(calculator.RatePickRule))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.HighestRate.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_BreakWeightVolume, RatingCalculatorColumns.HighestRate.Units),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.HighestRate.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.HighestRate.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.HighestRate.FlatAmount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HighestRate>(RateLineItemsSchema.TM_UnitMultiple, RatingCalculatorColumns.HighestRate.Multiple)
			);
		}
	}
}
