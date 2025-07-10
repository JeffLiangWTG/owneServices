using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class PercentageBreaksCalculatorGlowImporter : RateLineItemGlowImporter<PercentageBreaksCalculator, RatingCalculatorColumns.PercentageBreaks>
	{
		protected override bool ImportCore(
			PercentageBreaksCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.PercentageBreaks, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.PercentageBreaks.IncludeGST, nameof(calculator.IncludeGST)),
				(RatingCalculatorColumns.PercentageBreaks.UseInclusiveBreaks, nameof(calculator.UseInclusiveBreaks)),
				(RatingCalculatorColumns.PercentageBreaks.BreaksAreBasedOnValuesRatherThanMeasures, nameof(calculator.UseBreaksBasedOnValues))
			);

			// The PEB calculator has both ApplyTo and Operator values.
			// The ApplyTo can refer to a charge code but the Operator values don't want a charge code.
			// So we must import it into groups. One with ApplyTo (if any) and charge code relationship;
			// and a separate one for the remaining Operator/breaks.
			var hasApplyToData = HasContent(directValues, RatingCalculatorColumns.PercentageBreaks.ApplyToType);
			var success = false;
			if (hasApplyToData)
			{
				success |= ImportRateLineItem(
					calculator.RateLineBizO,
					context,
					directValues,
					relationshipValues,
					new RateLineItemImportDataPair<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_Type, CalculatorConstants.Type.ApplyTo, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.PercentageBreaks.ApplyToType)
				);
			}

			success |= ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				null,
				// Relationships are removed from the breaks to avoid saving a break with the TM_AC set. (because the TM_AC would only be used in the ApplyTo above)
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.PercentageBreaks.Operator), // In the PEB calculator, the relationship data is only used for the ApplyTo
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.PercentageBreaks.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_FlatAmount, RatingCalculatorColumns.PercentageBreaks.FlatAmount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.PercentageBreaks.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.PercentageBreaks.AgentRate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_BreakMinimum, RatingCalculatorColumns.PercentageBreaks.Percentage),
				new RateLineItemImportDataPair<RatingCalculatorColumns.PercentageBreaks>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.PercentageBreaks.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.PercentageBreaks.RestrictedReason)
			);

			return success;
		}
	}
}
