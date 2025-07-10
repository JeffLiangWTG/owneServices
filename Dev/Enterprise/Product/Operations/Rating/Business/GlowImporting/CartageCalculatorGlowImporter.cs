using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class CartageCalculatorGlowImporter : RateLineItemGlowImporter<CartageCalculator, RatingCalculatorColumns.Cartage>
	{
		protected override bool ImportCore(
			CartageCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Cartage, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Cartage.DropMode, nameof(calculator.EquipmentType)),
				(RatingCalculatorColumns.Cartage.UseInclusiveBreaks, nameof(calculator.UseInclusiveBreaks)),
				(RatingCalculatorColumns.Cartage.UseCumulativeBreaks, nameof(calculator.IsAccumulated)),
				(RatingCalculatorColumns.Cartage.UseHigherBreakLowerRate, nameof(calculator.UseHigherChargeableLowerRateRule))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.Cartage.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.Cartage.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.Cartage.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.Cartage.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.Cartage.FlatAmount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_BreakWeightVolume, RatingCalculatorColumns.Cartage.Units),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_BreakMinimum, RatingCalculatorColumns.Cartage.BreakMinimum),
				new RateLineItemImportDataPair<RatingCalculatorColumns.Cartage>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.Cartage.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Cartage.RestrictedReason)
			);
		}
	}
}
