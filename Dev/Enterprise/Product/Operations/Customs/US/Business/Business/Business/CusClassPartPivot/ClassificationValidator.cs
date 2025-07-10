using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class ClassificationValidator
	{
		public static void CheckPercentageActiveIngredient(ZPropertyInfo info)
		{
			var value = (ZDecimal)info.Value;
			if (value > 9.999999m || value < ZDecimal.Zero)
			{
				info.AddError(InvalidPercentageActiveIngredientValue);
			}
		}
		internal const string InvalidPercentageActiveIngredientValue = "Percentage of Active Ingredient must be between 0.000001 and 9.999999 if applicable or zero if not";

		internal static void CheckLicNo(ZPropertyInfo licNoInfo, bool isDDTCDataRequired, bool isDDTCDataAllowed, ZString requiredMessage, ZString donotEnterMessage)
		{
			if (isDDTCDataRequired)
			{
				if (licNoInfo.Value.IsEmpty)
				{
					licNoInfo.AddMessageError(requiredMessage);
				}
			}
			else
			{
				if (!licNoInfo.Value.IsEmpty && !isDDTCDataAllowed)
				{
					licNoInfo.AddMessageError(donotEnterMessage);
				}
			}
		}

		internal const string DDTCITARExemptionNumberIsRequiredMessage = "DDTC ITAR Exemption Number is required when License Type is 'SAG', 'SAU, 'SCA', 'SGB' or 'S00'.";
		internal const string DDTCITARExemptionNumberShouldNotBeEntered = "DDTC ITAR Exemption Number should only be entered when License Type is 'SAG', 'SAU, 'SCA', 'SGB' or 'S00'.";
		internal const string DDTCRegistrationNumberIsRequiredMessage = "DDTC Registration Number is required when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCRegistrationNumberShouldNotBeEntered = "DDTC Registration should only be entered when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCMilitaryEquipmentIndicatorIsRequiredMessage = "DDTC Significant Military Equipment Indicator is required when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCMilitaryEquipmentIndicatorShouldNotBeEntered = "DDTC Significant Miliary Equipment Indicator should only be entered when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCPartyCertificationIndicatorIsRequiredMessage = "DDTC Eligible Party Certification Indicator is required when License Type is 'SAU' 'SCA', 'SGB' or 'S00'.";
		internal const string DDTCPartyCertificationIndicatorShouldNotBeEntered = "DDTC Eligible Party Certification Indicator should only be entered when License Type is 'SAU' 'SCA', 'SGB' or 'S00'.";
		internal const string DDTCUSMLCategoryCodeIsRequiredMessage = "DDTC USML Category Code is required when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCUSMLCategoryCodeShouldNotBeEntered = "DDTC USML Category Code should only be entered when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
	}
}
