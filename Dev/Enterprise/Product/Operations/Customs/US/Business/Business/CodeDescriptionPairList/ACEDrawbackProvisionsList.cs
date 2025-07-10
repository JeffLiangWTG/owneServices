using System.Collections.Generic;
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
	public static class ACEDrawbackProvisionsList
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
			public const string _15 = "15";
			public const string _16 = "16";
			public const string _17 = "17";
			public const string _18 = "18";
			public const string _19 = "19";
			public const string _20 = "20";
			public const string _21 = "21";
			public const string _22 = "22";
			public const string _51 = "51";
			public const string _52 = "52";
			public const string _53 = "53";
			public const string _54 = "54";
			public const string _55 = "55";
			public const string _56 = "56";
			public const string _57 = "57";
			public const string _58 = "58";
			public const string _59 = "59";
			public const string _60 = "60";
			public const string _61 = "61";
			public const string _62 = "62";
			public const string _63 = "63";
			public const string _64 = "64";
			public const string _65 = "65";
			public const string _66 = "66";
			public const string _67 = "67";
			public const string _68 = "68";
			public const string _69 = "69";
			public const string _70 = "70";
			public const string _71 = "71";
			public const string _72 = "72";
			public const string _73 = "73";
			public const string _74 = "74";
			public const string _75 = "75";
			public const string _76 = "76";
			public const string _77 = "77";
		}

		public static CodeDescriptionPairList GetDrawbackProvisionList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ACEDrawbackProvisionsList", () =>
			{
				var result = new CodeDescriptionPairList();
				var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes,
					ZDateTime.Now);

				result.AddRange(codes);
				result.Sort();
				return result;
			});
		}

		public static ZString GetSectionDescriptionFromCode(BusinessObjectFactory factory, ZString entryType)
		{
			var list = GetDrawbackProvisionList(factory);
			var result = list.GetDescriptionFromCode(entryType) ?? ZString.Empty;

			if (result.IndexOf('-') >= 0)
			{
				result = result.Split('-')[0].Trim();
			}

			return result;
		}

		public static ZBool IsRejectedMerchandise(ZString code)
		{
			return code == Codes._03 || code == Codes._04 || code == Codes._05 || code == Codes._06
				|| code == Codes._17 || code == Codes._18 || code == Codes._19 || code == Codes._20
				|| code == Codes._53 || code == Codes._54 || code == Codes._55 || code == Codes._56
				|| code == Codes._67 || code == Codes._68 || code == Codes._69 || code == Codes._70;
		}

		public static ZBool IsHMF_MPFClaimable(ZString code)
		{
			return code == Codes._08 || code == Codes._09 || code == Codes._15 || code == Codes._16
				|| code == Codes._58 || code == Codes._59 || code == Codes._65 || code == Codes._66
				|| code == Codes._73 || code == Codes._74 || code == Codes._76 || code == Codes._77;
		}

		public static ZBool IsDirectIdentificationManufacturing(ZString code)
		{
			return code == Codes._01 || code == Codes._21 || code == Codes._51 || code == Codes._71;
		}

		public static ZBool IsSubstitutionManufacturing(ZString code)
		{
			return code == Codes._02 || code == Codes._22 || code == Codes._52 || code == Codes._72 || code == Codes._75;
		}

		public static ZBool IsTFTEA(ZString code)
		{
			return code == Codes._51 || code == Codes._52 || code == Codes._53 || code == Codes._54 || code == Codes._55 || code == Codes._56
				|| code == Codes._57 || code == Codes._58 || code == Codes._59 || code == Codes._60 || code == Codes._61 || code == Codes._62
				|| code == Codes._63 || code == Codes._64 || code == Codes._65 || code == Codes._66 || code == Codes._67 || code == Codes._68
				|| code == Codes._69 || code == Codes._70 || code == Codes._71 || code == Codes._72 || code == Codes._73 || code == Codes._74 || code == Codes._75 || code == Codes._76 || code == Codes._77;
		}

		public static ZBool IsTFTEAExcept57(ZString code)
		{
			return code != Codes._57 && IsTFTEA(code);
		}

		public static ZBool IsSubstitutedValueRequired(ZString code)
		{
			return code == Codes._52 || code == Codes._59 || code == Codes._66 || code == Codes._72 || code == Codes._73 || code == Codes._75 || code == Codes._76 || code == Codes._77;
		}

		public static ZBool Is1313A(ZString code)
		{
			return code == Codes._01 || code == Codes._21 || code == Codes._51 || code == Codes._71;
		}

		public static ZBool Is1313D(ZString code)
		{
			return code == Codes._07 || code == Codes._57;
		}

		public static ZBool Is5062(ZString code)
		{
			return code == Codes._14 || code == Codes._64;
		}

		public static ZBool IsApplicableProvisionsForOneTimeWaiverInd(ZString code) => !ApplicableProvisionsExceptionList.Contains(code);

		public static List<ZString> ApplicableProvisionsExceptionList = new List<ZString>() { Codes._01, Codes._02, Codes._07, Codes._10, Codes._11, Codes._12, Codes._13, Codes._21, Codes._22, Codes._51, Codes._52, Codes._57,
					Codes._60, Codes._61, Codes._62, Codes._63, Codes._71, Codes._72, Codes._75, Codes._76 };
	}
}
