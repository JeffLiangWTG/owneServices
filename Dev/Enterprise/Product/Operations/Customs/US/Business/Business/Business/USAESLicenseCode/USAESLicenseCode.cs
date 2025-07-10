using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class USAESLicenseCode
	{
		public static class Codes
		{
			public const string C30 = "C30";
			public const string C31 = "C31";
			public const string C32 = "C32";
			public const string C33 = "C33";
			public const string C35 = "C35";
			public const string C36 = "C36";
			public const string C37 = "C37";
			public const string C38 = "C38";
			public const string C40 = "C40";
			public const string C41 = "C41";
			public const string C42 = "C42";
			public const string C43 = "C43";
			public const string C44 = "C44";
			public const string C45 = "C45";
			public const string C46 = "C46";
			public const string C50 = "C50";
			public const string C51 = "C51";
			public const string C53 = "C53";
			public const string C54 = "C54";
			public const string C57 = "C57";
			public const string C58 = "C58";
			public const string C59 = "C59";
			public const string C60 = "C60";
			public const string C62 = "C62";
			public const string C63 = "C63";
			public const string C64 = "C64";
			public const string E01 = "E01";
			public const string N01 = "N01";
			public const string N02 = "N02";
			public const string OPA = "OPA";
			public const string S00 = "S00";
			public const string S05 = "S05";
			public const string S61 = "S61";
			public const string S73 = "S73";
			public const string S94 = "S94";
			public const string S85 = "S85";
			public const string SAG = "SAG";
			public const string SAU = "SAU";
			public const string SCA = "SCA";
			public const string SGB = "SGB";
			public const string T10 = "T10";
			public const string T11 = "T11";
			public const string T12 = "T12";
			public const string VDC = "VDC";
			public const string VDO = "VDO";
			public const string VDS = "VDS";
		}

		public static bool IsLicenseValueRequired(ZString licenseType, BusinessObjectFactory factory, ZDateTime exportDate)
		{
			if (exportDate.IsValid)
			{
				var licenseValueRequired = new RefCusCodeListAttribute.Loader(factory).Load(Core.Constants.CountryCodes.UnitedStates, exportDate,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, licenseType, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseValueRequired).FirstOrDefault();
				if (licenseValueRequired != null && licenseValueRequired.ZZE_Value.EqualsIgnoringCase(YesNoDefaultList.Codes.Yes))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsDDTCDataRequired(ZString licenseType)
		{
			return licenseType == Codes.SAG
				|| licenseType == Codes.SAU
				|| licenseType == Codes.SCA
				|| licenseType == Codes.SGB
				|| licenseType == Codes.S00
				|| licenseType == Codes.S05
				|| licenseType == Codes.S61
				|| licenseType == Codes.S73
				|| licenseType == Codes.S85
				|| licenseType == Codes.S94
				|| licenseType == Codes.VDS;
		}

		public static bool IsDDTCITARExemptionRequired(ZString licenseType)
		{
			return licenseType == Codes.SAG
				|| licenseType == Codes.SAU
				|| licenseType == Codes.SCA
				|| licenseType == Codes.SGB
				|| licenseType == Codes.S00;
		}

		public static bool IsDDTCPartyCertIndicatorRequired(ZString licenseType)
		{
			return licenseType == Codes.SAU
				|| licenseType == Codes.SCA
				|| licenseType == Codes.SGB
				|| licenseType == Codes.S00;
		}

		public static bool IsDDTCRegistrationNumberRequired(ZString licenseType)
		{
			return licenseType == Codes.SAG
				|| licenseType == Codes.SAU
				|| licenseType == Codes.SCA
				|| licenseType == Codes.SGB
				|| licenseType == Codes.S05
				|| licenseType == Codes.S61
				|| licenseType == Codes.S73
				|| licenseType == Codes.S85
				|| licenseType == Codes.S94
				|| licenseType == Codes.VDS;
		}

		public static bool IsDDTCRegistrationNumberAllowed(ZString licenseType)
		{
			return licenseType == Codes.S00;
		}

		public static bool Is600SeriesDotYECCN(ZString licenseType)
		{
			return licenseType == Codes.C32 || licenseType == Codes.C33;
		}
	}
}
