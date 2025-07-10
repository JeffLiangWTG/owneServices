using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business;

public static class UniversalValidationHelper
{
	public static bool IsInAESTransitionPeriod => FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);
}
