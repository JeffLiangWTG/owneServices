using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	static class GlbExternalPasswordHelper
	{
		public static bool IsTesting(this MasterFiles.Business.GlbExternalPassword credential)
		{
			return (credential.GP_PasswordType == PasswordTypesList.Codes.TVA && credential.GP_UserID.EndsWith("TEST", StringComparison.OrdinalIgnoreCase))
				|| (credential.GP_PasswordType == PasswordTypesList.Codes.UVC && credential.GP_UserID.EndsWith("T", StringComparison.OrdinalIgnoreCase));
		}
	}
}
