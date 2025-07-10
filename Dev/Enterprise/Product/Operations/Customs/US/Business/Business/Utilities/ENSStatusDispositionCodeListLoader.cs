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
	public static class ENSStatusDispositionCodeListLoader
	{
		public static CodeDescriptionPairList GetENSStatusDispositionCodeList(BusinessObjectFactory factory)
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue("ENSStatusDispositionCodeList" + today, () =>
			{
				var codeList = new CodeDescriptionPairList();
				var array = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, today);
				foreach (var code in array)
				{
					codeList.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
				}
				codeList.Sort();
				return codeList;
			});
		}

		public static bool IsFurtherActionRequiredDespiteAbsenceOfActionID(BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IActionRequiredDespiteActionID);
		}

		public static bool IsFurtherActionRequired(BusinessObjectFactory factory, ZString codeString)
		{
			return !HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INoFurtherActionRequired);
		}

		static bool HasAttribute(BusinessObjectFactory factory, ZString codeString, ZString attribute, string attributeValue = "Y")
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue(string.Format("UCDSP_{0}_{1}_{2}_{3}", codeString, attribute, attributeValue, today), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(factory, codeString, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(attribute, JoinCondition.And, attributeValue) }) != null;
			});
		}
	}

	public static class ENSStatusDispositionCodeList
	{
		public const string _1 = "1";
		public const string _2 = "2";
		public const string _3 = "3";
		public const string _4 = "4";
		public const string _5 = "5";
		public const string _6 = "6";
		public const string _7 = "7";
		public const string _8 = "8";
		public const string _E = "E";
		public const string _P = "P";
		public const string _Q = "Q";
		public const string _R = "R";
		public const string _C = "C";
		public const string _D = "D";
	}
}
