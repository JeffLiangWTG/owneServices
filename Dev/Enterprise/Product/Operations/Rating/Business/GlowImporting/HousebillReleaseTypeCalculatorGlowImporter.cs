using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class HousebillReleaseTypeCalculatorGlowImporter : RateLineItemGlowImporter<HousebillReleaseTypeCalculator, RatingCalculatorColumns.HousebillReleaseType>
	{
		protected override bool ImportCore(
			HousebillReleaseTypeCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.HousebillReleaseType, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HousebillReleaseType>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.HousebillReleaseType.Code),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HousebillReleaseType>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.HousebillReleaseType.Amount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.HousebillReleaseType>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.HousebillReleaseType.AgentAmount)
			);
		}
	}
}
