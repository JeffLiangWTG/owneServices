using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class LicenseNumberValidation
	{
		public static IEnumerable<Func<ZString, ZString, string>> GetLicenseNumberValidate(BusinessObjectFactory factory, ZDateTime exportDate)
		{
			yield return (licenseType, licenseNumber) => ValidateLicenseNumberIsRequired(licenseType, licenseNumber, factory, exportDate);
			yield return ValidateLicenseNumberFormat;
		}

		static string ValidateLicenseNumberIsRequired(ZString licenseType, ZString licenseNumber, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			return licenseNumber.IsEmpty && IsLicenseNumberRequired(factory, licenseType, exportDate) ? string.Format(LicenseNumberIsRequiredMessage, licenseType) : string.Empty;
		}
		internal const string LicenseNumberIsRequiredMessage = "License Number is required when License Type is '{0}'.";

		internal static bool IsLicenseNumberRequired(BusinessObjectFactory factory, ZString licenseType, ZDateTime exportDate)
		{
			if (exportDate.IsValid)
			{
				var licenseRequired = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseRequired).FirstOrDefault();
				if (licenseRequired != null && licenseRequired.ZZE_Value.EqualsIgnoringCase(YesNoDefaultList.Codes.Yes))
				{
					return true;
				}
			}
			return false;
		}

		static string ValidateLicenseNumberFormat(ZString licenseType, ZString licenseNumber)
		{
			if (licenseType == USAESLicenseCode.Codes.C30)
			{
				return !licenseNumber.StartsWith("D") ? C30LicenseInvalid : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.C31)
			{
				return !licenseNumber.StartsWith("S") && !licenseNumber.StartsWith("V") ? C31LicenseInvalid : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.C51)
			{
				return !licenseNumber.StartsWith("F") ? C51LicenseInvalid : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.C60)
			{
				return licenseNumber != LicenseExemptionTypeList.Codes.DY6 ? C60LicenseInvalid : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.C62)
			{
				return !licenseNumber.StartsWith("SCP") ? C62LicenseInvalid : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.S94)
			{
				return !Regex.IsMatch(licenseNumber, @"[A-Z0-9]{2}-[A-Z0-9]{1}-[A-Z0-9]{3}", RegexOptions.IgnoreCase) ? S94LicenseNoFormat : string.Empty;
			}
			else if (licenseType == USAESLicenseCode.Codes.T10)
			{
				return !Regex.IsMatch(licenseNumber, "^[a-zA-Z]{2}[0-9]{4}\\d+$") ? T10LicenseInvalid : string.Empty;
			}

			return string.Empty;
		}

		internal const string C30LicenseInvalid = "License number is not valid for License Type C30.\r\n(For C30, Bureau of Industry and Security (BIS) License Number must start with 'D').";
		internal const string C31LicenseInvalid = "License number is not valid for License Type C31.\r\n(For C31, Bureau of Industry and Security (BIS) License Number must start with 'S' or 'V').";
		internal const string C51LicenseInvalid = "License number is not valid for License Type C51.\r\nFor C51, only License Exception Agricultural Commodities (AGR) notice confirmation numbers issued by the Bureau of Industry and Security (BIS) beginning with 'F' will be accepted.\r\nThe license exception symbol “AGR” is no longer allowed in the Export License Number field for License Type C51.";
		internal const string C52LicenseInvalid = "License number is not valid for License Type C52.\r\nFor C52, License Number must start with 'USPL'.";
		internal const string C60LicenseInvalid = "License number is not valid for License Type C60.\r\nFor C60, License Number must be 'DY6'";
		internal const string C62LicenseInvalid = "License number is not valid for License Type C62.\r\nFor C62, License Number must start with 'SCP'.";
		internal const string S94LicenseNoFormat = "The acceptable format of a License Number for License Type 'S94' is 'AA-A-AAA', where 'A' represents an alphanumeric character and '-' represents a dash.";
		internal const string T10LicenseInvalid = "For T10, Format should be AAYYYY where AA = Country, YYYY = Year , followed by the Case No or Amendment No";
	}
}
