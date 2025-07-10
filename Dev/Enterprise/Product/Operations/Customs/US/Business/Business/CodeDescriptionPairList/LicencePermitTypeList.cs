using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class LicencePermitTypeList
	{
		public static class Codes
		{
			public const string _01 = "01";
			public const string _02 = "02";
			public const string _03 = "03";
			public const string _04 = "04";
			public const string _05 = "05";
			public const string _06 = "06";
			public const string _07 = "07";
			public const string _08 = "08";
			public const string _09 = "09";
			public const string _10 = "10";
			public const string _11 = "11";
			public const string _12 = "12";
			public const string _13 = "13";
			public const string _14 = "14";
			public const string _16 = "16";
			public const string _17 = "17";
			public const string _18 = "18";
			public const string _19 = "19";
			public const string _20 = "20";
			public const string _21 = "21";
			public const string _22 = "22";
			public const string _23 = "23";
			public const string _25 = "25";
			public const string _26 = "26";
			public const string _27 = "27";
			public const string _28 = "28";
			public const string _29 = "29";
			public const string _30 = "30";
			public const string _31 = "31";
			public const string KR = "KR";
		}

		public static bool DeclareInCottonOrganicExemptionFieldInACS(ZString licenseType)
		{
			return licenseType == Codes._12 ||
				licenseType == Codes._22 ||
				licenseType == Codes._23;
		}

		public static CodeDescriptionPairList GetLicencePermitTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("LicencePermitTypeList" + ZDate.Today, () =>
			{
				var result = new CodeDescriptionPairList();
				var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType,
					ZDateTime.Today);

				result.AddRange(codes);
				result.Sort();
				return result;
			});
		}
	}
}
