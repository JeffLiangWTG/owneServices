using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class SplitMonthBillingCalculatorGlowImporter : RateLineItemGlowImporter<SplitMonthBillingCalculator, RatingCalculatorColumns.SplitMonthBilling>
	{
		protected override bool ImportCore(
			SplitMonthBillingCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.SplitMonthBilling, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.SplitMonthBilling>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.SplitMonthBilling.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.SplitMonthBilling>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.SplitMonthBilling.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.SplitMonthBilling>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.SplitMonthBilling.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.SplitMonthBilling>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.SplitMonthBilling.AgentRate),
				new RateLineItemImportDataPair<RatingCalculatorColumns.SplitMonthBilling>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.SplitMonthBilling.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.SplitMonthBilling.RestrictedReason)
			);
		}
	}
}
