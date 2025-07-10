using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class CartageZoneDistanceCalculatorGlowImporter : RateLineItemGlowImporter<CartageZoneDistanceCalculator, RatingCalculatorColumns.CartageZoneDistance>
	{
		protected override bool ImportCore(
			CartageZoneDistanceCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.CartageZoneDistance, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.CartageZoneDistance.DropMode, nameof(calculator.EquipmentType)),
				(RatingCalculatorColumns.CartageZoneDistance.UseInclusiveBreaks, nameof(calculator.UseInclusiveBreaks)),
				(RatingCalculatorColumns.CartageZoneDistance.UseAciZones, nameof(calculator.UseACIZones)),
				(RatingCalculatorColumns.CartageZoneDistance.UseCumulativeBreaks, nameof(calculator.IsAccumulated)),
				(RatingCalculatorColumns.CartageZoneDistance.UseHigherBreakLowerRate, nameof(calculator.UseHigherChargeableLowerRateRule))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.CartageZoneDistance.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.CartageZoneDistance.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.CartageZoneDistance.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.CartageZoneDistance.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.CartageZoneDistance.FlatAmount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_BreakWeightVolume, RatingCalculatorColumns.CartageZoneDistance.Units),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_BreakMinimum, RatingCalculatorColumns.CartageZoneDistance.BreakMinimum),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_F1Zone, RatingCalculatorColumns.CartageZoneDistance.AciZone),
				new RateLineItemImportDataPair<RatingCalculatorColumns.CartageZoneDistance>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.CartageZoneDistance.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.CartageZoneDistance.RestrictedReason)
			);
		}
	}
}
