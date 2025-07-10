using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class AgencyCalculatorGlowImporter : RateLineItemGlowImporter<AgencyCalculator, RatingCalculatorColumns.Agency>
	{
		protected override bool ImportCore(
			AgencyCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Agency, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			// Need to set any types first before any values. Otherwise some fields become readonly
			// and the imported value is ignored.
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Agency.LineType, nameof(calculator.AgencyLineType)),
				(RatingCalculatorColumns.Agency.FeeType, nameof(calculator.AgencyFeeType)),
				(RatingCalculatorColumns.Agency.MessageType, nameof(calculator.MessageType)),
				(RatingCalculatorColumns.Agency.MessageStyle, nameof(calculator.MessageSubType)),
				(RatingCalculatorColumns.Agency.HideFeeAndLineTypeOnQuotation, nameof(calculator.HideFeeLineTypeOnQuote)),
				(RatingCalculatorColumns.Agency.HideTypeAndStyleOnQuotation, nameof(calculator.HideMessageTypeOnQuote)),

				(RatingCalculatorColumns.Agency.FeeTypeRatePerAdditionalLine, nameof(calculator.AdditionalRate)),
				(RatingCalculatorColumns.Agency.FeeTypeIncludedLines, nameof(calculator.IncludedHeaders)),
				(RatingCalculatorColumns.Agency.BaseRate, nameof(calculator.AgencyRate)),
				(RatingCalculatorColumns.Agency.Maximum, nameof(calculator.Maximum)),
				(RatingCalculatorColumns.Agency.LineTypeRatePerAdditionalLine, nameof(calculator.PerAdditionalLine)),
				(RatingCalculatorColumns.Agency.LineTypeIncludedLines, nameof(calculator.IncludedLines)),
				(RatingCalculatorColumns.Agency.LineTypeMaximumLines, nameof(calculator.MaximumLines))
			);

			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.Agency.AgentBaseRate, nameof(calculator.AgencyRate)),
				(RatingCalculatorColumns.Agency.AgentMaximum, nameof(calculator.Maximum)),
				(RatingCalculatorColumns.Agency.AgentLineTypeRatePerAdditionalLine, nameof(calculator.PerAdditionalLine)),
				(RatingCalculatorColumns.Agency.AgentLineTypeIncludedLines, nameof(calculator.IncludedLines)),
				(RatingCalculatorColumns.Agency.AgentLineTypeMaximumLines, nameof(calculator.MaximumLines)),
				(RatingCalculatorColumns.Agency.AgentFeeTypeRatePerAdditionalLine, nameof(calculator.AdditionalRate)),
				(RatingCalculatorColumns.Agency.AgentFeeTypeIncludedLines, nameof(calculator.IncludedHeaders))
			);

			return true;
		}
	}
}
