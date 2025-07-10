using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Business.MessagingProcess
{
	public static class MessagingPOCHelper
	{
		public static bool IsPOCActive => (Env.CurrentUser.IsDeveloper || ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(zaMessagingPOCCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now));

#if DEBUG
		public static IDisposable TemporarilyEnablePOC(bool enabled) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(zaMessagingPOCCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, enabled);
#endif
		static string zaMessagingPOCCode => "ZAMESSAGINGPOC";
	}
}
