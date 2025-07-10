using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.LicenseManager
{
	public static class ExportCodeValidation
	{
		public static IEnumerable<Func<ZString, ZString, string>> GetExportCodeValidate()
		{
			yield return ValidateExportCodeIsInAlowedList;
			yield return ValidateExportCodeIsInNotAllowedList;
			yield return ValidateExportCodeIsRequired;
		}

		static string ValidateExportCodeIsInAlowedList(ZString licenseType, ZString exportCode)
		{
			var allowedList = GetAllowedExportCodes(licenseType);
			if (allowedList.Length > 0 && !allowedList.Contains(exportCode.ToString()))
			{
				return string.Format(ExportCodeIsInvalidMessage, (allowedList.Length > 1 ? "are" : "is"), string.Join(",", allowedList));
			}
			return string.Empty;
		}
		internal const string ExportCodeIsInvalidMessage = "Export Code entered is invalid; valid Export Code {0} {1}.";

		static string[] GetAllowedExportCodes(ZString licenseType)
		{
			switch (licenseType)
			{
				case USAESLicenseCode.Codes.C30:
					return new string[] { ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.IW, ExportInformationCodeList.Codes.CH };
				case USAESLicenseCode.Codes.C31:
				case USAESLicenseCode.Codes.C32:
					return new string[] { ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C35:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.CH, ExportInformationCodeList.Codes.CI, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.DD, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C36:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.DD, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C37:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.DD, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C38:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C40:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.DD, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C41:
					return new string[] { ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C42:
					return new string[] { ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.CH, ExportInformationCodeList.Codes.CI, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.DD, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C43:
					return new string[] { ExportInformationCodeList.Codes.UG, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C44:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C45:
					return new string[] { ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C46:
					return new string[] { ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C50:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C51:
					return new string[] { ExportInformationCodeList.Codes.CH, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C53:
					return new string[] { ExportInformationCodeList.Codes.CR, ExportInformationCodeList.Codes.GP, ExportInformationCodeList.Codes.IS, ExportInformationCodeList.Codes.TE, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.IP, ExportInformationCodeList.Codes.IR, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C57:
					return new string[] { ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.TL, ExportInformationCodeList.Codes.IW };
				case USAESLicenseCode.Codes.C58:
				case USAESLicenseCode.Codes.C59:
				case USAESLicenseCode.Codes.C60:
					return new string[] { ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.CH, ExportInformationCodeList.Codes.CI };
				case USAESLicenseCode.Codes.E01:
					return new string[] { ExportInformationCodeList.Codes.OS };
				case USAESLicenseCode.Codes.N01:
				case USAESLicenseCode.Codes.N02:
				case USAESLicenseCode.Codes.T10:
				case USAESLicenseCode.Codes.T11:
				case USAESLicenseCode.Codes.SAG:
				case USAESLicenseCode.Codes.SCA:
				case USAESLicenseCode.Codes.S00:
				case USAESLicenseCode.Codes.S05:
				case USAESLicenseCode.Codes.S61:
				case USAESLicenseCode.Codes.S85:
				case USAESLicenseCode.Codes.SAU:
				case USAESLicenseCode.Codes.SGB:
					return new string[] { ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS };
				case USAESLicenseCode.Codes.T12:
					return new string[] { ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS };
				case USAESLicenseCode.Codes.S73:
					return new string[] { ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.TP, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS };
				case USAESLicenseCode.Codes.S94:
					return new string[] { ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.FS, ExportInformationCodeList.Codes.FI };
				case USAESLicenseCode.Codes.VDS:
					return new string[] { ExportInformationCodeList.Codes.MS, ExportInformationCodeList.Codes.GS, ExportInformationCodeList.Codes.OI, ExportInformationCodeList.Codes.OS, ExportInformationCodeList.Codes.FS, ExportInformationCodeList.Codes.FI, ExportInformationCodeList.Codes.TP };
				default:
					return Array.Empty<string>();
			}
		}

		static string ValidateExportCodeIsInNotAllowedList(ZString licenseType, ZString exportCode)
		{
			var notAllowedList = GetNotAllowedExportCodes(licenseType);
			if (notAllowedList.Length > 0 && notAllowedList.Contains(exportCode.ToString()))
			{
				return string.Format(ExportCodeIsNotAllowedMessage, string.Join(",", notAllowedList));
			}
			return string.Empty;
		}
		internal const string ExportCodeIsNotAllowedMessage = "Export Code entered is invalid; valid Export Code are all except {0}.";

		static string[] GetNotAllowedExportCodes(ZString licenseType)
		{
			switch (licenseType)
			{
				case USAESLicenseCode.Codes.C33:
				case USAESLicenseCode.Codes.C62:
				case USAESLicenseCode.Codes.C63:
					return new string[] { ExportInformationCodeList.Codes.UG, ExportInformationCodeList.Codes.FS, ExportInformationCodeList.Codes.FI };
				case USAESLicenseCode.Codes.OPA:
				case USAESLicenseCode.Codes.VDO:
					return new string[] { ExportInformationCodeList.Codes.UG, ExportInformationCodeList.Codes.FS, ExportInformationCodeList.Codes.FI, ExportInformationCodeList.Codes.IW };
				default:
					return Array.Empty<string>();
			}
		}

		static string ValidateExportCodeIsRequired(ZString licenseType, ZString exportCode)
		{
			return exportCode.IsEmpty ? string.Format(ExportCodeIsRequired, licenseType) : string.Empty;
		}
		internal const string ExportCodeIsRequired = "Export Code is required when License Type is '{0}'.";
	}
}
