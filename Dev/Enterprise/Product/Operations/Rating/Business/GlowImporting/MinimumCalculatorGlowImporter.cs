using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Rating.Business
{
	class MinimumCalculatorGlowImporter : RateLineItemGlowImporter<MinimumCalculator, RatingCalculatorColumns.Minimum>
	{
		protected override bool ImportCore(
			MinimumCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Minimum, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Minimum.Value, nameof(calculator.MinimumValue))
			);
			SetCalculatorPropertyForAgencyRates(directValues, calculator,
				(RatingCalculatorColumns.Minimum.AgentValue, nameof(calculator.MinimumValue))
			);

			if (!TryGetMinimumType(directValues, context, out var minimumType))
			{
				return false;
			}

			if (minimumType == CalculatorConstants.Text.MIN_Job)
			{
				calculator.IsJobMinimum = true;
			}
			else
			{
				calculator.IsChargeCodeMinimum = true;
			}
			return true;
		}

		bool TryGetMinimumType(Dictionary<RatingCalculatorColumns.Minimum, string> directValues, ValueObjectImportContext context, out string result)
		{
			var hasJobMinimum = TryGet(RatingCalculatorColumns.Minimum.IsPerJobMinimum, directValues, out ZBool isJobMinimum);
			var hasChargeMinimum = TryGet(RatingCalculatorColumns.Minimum.IsPerChargeMinimum, directValues, out ZBool isChargeMinimum);

			isJobMinimum = isJobMinimum && hasJobMinimum;
			isChargeMinimum = isChargeMinimum && hasChargeMinimum;

			if (isJobMinimum == isChargeMinimum)
			{
				context.Notifications.AddError(ResString.GetMultilingualString("4fa26333-a25f-4884-b1db-de8d3c175a70", "Calculator of type MIN contains invalid values for 'Is Job Minimum' and for 'Is Charge Minimum' as they are both set to the same value"));
				result = default;
				return false;
			}

			result = isJobMinimum ? CalculatorConstants.Text.MIN_Job : CalculatorConstants.Text.MIN_ChargeCode;
			return true;
		}
	}
}
