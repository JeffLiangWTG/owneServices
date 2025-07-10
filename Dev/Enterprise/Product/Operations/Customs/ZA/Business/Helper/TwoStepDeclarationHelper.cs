using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public static class TwoStepDeclarationHelper
	{
		public static ZBool IsTwoStepClearingValid => Universal.ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(
			Universal.Constants.FunctionalityTypes.ZATwoStepClearing
			, Core.Constants.CountryCodes.SouthAfrica
			, ZDateTime.Today);

#if DEBUG
		public static IDisposable TemporarilyEnableTwoStepClearing(bool value)
		{
			return ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Customs.Universal.Constants.FunctionalityTypes.ZATwoStepClearing
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today
				, value);
		}
#endif
	}
}
