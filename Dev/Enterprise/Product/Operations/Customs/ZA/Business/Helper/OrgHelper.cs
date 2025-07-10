using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public static class OrgHelper
	{
		public static bool UsePassport => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAUSEPASSPORT, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);

		public static ZString GetPassportNumber(OrgHeader orgHeader)
		{
			var passport = GetPassportCusCode(orgHeader);

			return passport?.OK_CustomsRegNo ?? string.Empty;
		}

		public static OrgCusCode GetPassportCusCode(OrgHeader orgHeader)
		{
			var passports = orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.PassportID);

			var zaPassport = passports.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.SouthAfrica && !x.OK_CustomsRegNo.IsEmpty);

			return zaPassport ?? passports.Where(x => !x.OK_CustomsRegNo.IsEmpty).OrderBy(x => x.OK_RN_NKCodeCountry).FirstOrDefault();
		}
	}
}
