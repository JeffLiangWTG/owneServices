using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class EqualizationCalculatorGlowImporter : RateLineItemGlowImporter<EqualizationCalculator, RatingCalculatorColumns.Equalization>
	{
		protected override bool ImportCore(
			EqualizationCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Equalization, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Equalization.UseInclusiveBreaks, nameof(calculator.UseInclusiveBreaks))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				replaceExisting: true,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.Equalization.PivotBreakMinusOrPlus),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.Equalization.PivotBreak),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.Equalization.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.Equalization.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.Equalization.FlatAmount),
				new RateLineItemImportDataPair<RatingCalculatorColumns.Equalization>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.Equalization.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Equalization.RestrictedReason)
			);
		}
	}
}
