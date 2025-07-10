using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public class DispositionCodeListLoader : IDispositionCodeListLoader
	{
		public static CodeDescriptionPairList GetDispositionCodes(BusinessObjectFactory factory, ZString codeType)
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue("DispositionCodeList" + codeType + today.ToString("MMddyy"), () =>
			{
				var codeList = new CodeDescriptionPairList();
				var array = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, codeType, today);
				foreach (var code in array)
				{
					codeList.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
				}
				codeList.Sort();
				return codeList;
			});
		}

		IBusinessObjectCollection IDispositionCodeListLoader.GetAMSDispositionCollection(BusinessObjectFactory factory) => GetAMSDispositionCollection(factory);

		public static IBusinessObjectCollection GetAMSDispositionCollection(BusinessObjectFactory factory)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetDispositionCodes(ZString transportMode, BusinessObjectFactory factory)
		{
			return GetDispositionCodes(factory, GetCodeType(transportMode));
		}

		public static bool IsInBondClosed(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, codeType) || IsInBondClosedForType62And63(factory, codeString, codeType);
		}

		public static bool IsInBondClosed(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, GetCodeType(transportMode)) || IsInBondClosedForType62And63(factory, codeString, GetCodeType(transportMode));
		}

		public static bool IsInBondClosedForType62And63(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, codeType);
		}

		public static bool IsInBondClosedForType62And63(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, GetCodeType(transportMode));
		}

		public static bool IsInBondDispositionCode(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, codeType);
		}

		public static bool IsInBondDispositionCode(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, GetCodeType(transportMode));
		}

		public static bool IsExam(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, codeType);
		}

		public static bool IsExam(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, GetCodeType(transportMode));
		}

		public static bool IsHold(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, codeType);
		}

		public static bool IsHold(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, GetCodeType(transportMode));
		}

		public static bool IsHoldExamRemoved(BusinessObjectFactory factory, ZString codeString, ZString codeType)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, codeType);
		}

		public static bool IsHoldExamRemoved(ZString transportMode, BusinessObjectFactory factory, ZString codeString)
		{
			return HasAttribute(factory, codeString, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, GetCodeType(transportMode));
		}

		public static bool IsHoldRemovedOrExamCompleted(BusinessObjectFactory factory, ZString billStatus, ZString dispositionCode, ZString codeType)
		{
			return HasAttribute(factory, billStatus, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, codeType, dispositionCode);
		}

		public static bool IsHoldRemovedOrExamCompleted(ZString transportMode, BusinessObjectFactory factory, ZString billStatus, ZString dispositionCode)
		{
			return HasAttribute(factory, billStatus, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, GetCodeType(transportMode), dispositionCode);
		}

		static bool HasAttribute(BusinessObjectFactory factory, ZString codeString, ZString attribute, ZString codeType, string attributeValue = "Y")
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue(string.Format("AMSDS_{0}_{1}_{2}_{3}_{4}", codeString, attribute, codeType, attributeValue, today), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(factory, codeString, Core.Constants.CountryCodes.UnitedStates, codeType, today, attributeFilters: new[] { new RefCusCodeListAttributeFilter(attribute, JoinCondition.And, attributeValue) }) != null;
			});
		}

		static ZString GetCodeType(ZString transportMode)
		{
			return (transportMode == Core.Constants.TransportModes.Air || TransportModeCodes.IsAirTransport(transportMode)) ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode;
		}

		const string Code3U = "3U";
		const string Code3Z = "3Z";
		const string Code1W = "1W";
		const string Code83 = "83";

		public static ICodeDescriptionPairList GetCachedISFCodesForAMS(BusinessObjectFactory factory)
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue<ICodeDescriptionPairList>("ACEM1DispositionListISFCodesForAMS" + today.ToString(), () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ZZRefCusCodeListCombined.Loader.LoadForCodes(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, new ZString[] { Code3U, Code3Z }, today, null));
				return result;
			});
		}

		public static ICodeDescriptionPairList GetCachedPTTCodesForAMS(BusinessObjectFactory factory)
		{
			var today = ZDateTime.UtcToday;
			return factory.GetCachedValue<ICodeDescriptionPairList>("ACEM1DispositionListPTTCodesForAMS" + today.ToString(), () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ZZRefCusCodeListCombined.Loader.LoadForCodes(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, new ZString[] { Code1W, Code83 }, today, null));
				return result;
			});
		}
	}
}
