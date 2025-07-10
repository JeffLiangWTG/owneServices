using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class DisbursementInterestCalculatorGlowImporter : RateLineItemGlowImporter<DisbursementInterestCalculator, RatingCalculatorColumns.DisbursementInterest>
	{
		protected override bool ImportCore(
			DisbursementInterestCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.DisbursementInterest, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.DisbursementInterest.Uplift, nameof(calculator.Uplift)),
				(RatingCalculatorColumns.DisbursementInterest.AdjustmentDays, nameof(calculator.AdjustmentDays)),
				(RatingCalculatorColumns.DisbursementInterest.ApplyToOutstandingDaysOnly, nameof(calculator.OutstandingDays)),
				(RatingCalculatorColumns.DisbursementInterest.IncludeGST, nameof(calculator.IncludeGST))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.DisbursementInterest.AgentUplift, nameof(calculator.Uplift)),
				(RatingCalculatorColumns.DisbursementInterest.AgentAdjustmentDays, nameof(calculator.AdjustmentDays))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataPair<RatingCalculatorColumns.DisbursementInterest>(RateLineItemsSchema.TM_Type, CalculatorConstants.Type.ApplyTo, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.DisbursementInterest.ApplyToType)
			);
		}
	}
}
