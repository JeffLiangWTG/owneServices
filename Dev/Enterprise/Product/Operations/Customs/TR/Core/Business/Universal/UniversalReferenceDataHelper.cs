using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TR.Business.CusEntryMessageConstants;

namespace Enterprise.Customs.TR.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static CodeDescriptionPairList GetTaxTypeList(this BusinessObjectFactory factory)
		{
			return RefCusTaxOrFee.Loader.GetList(factory, Core.Constants.CountryCodes.Turkey, ZDateTime.Now, TaxOrFeeCodeListType);
		}

		public static void GatherRefCusCodeList(BusinessObjectFactory factory, ZString questionCode, ZString questionDesc, ZString questionType)
		{
			var questionTypeCode = questionType == QuestionTypeList.Codes.Q ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsQuestion : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyCustomsWarning;
			var zzRefCusCodeListCombined = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, questionCode, Core.Constants.CountryCodes.Turkey, questionTypeCode, ZDate.Today);
			if (zzRefCusCodeListCombined == null)
			{
				zzRefCusCodeListCombined = factory.New<ZZRefCusCodeListCombined>();
				zzRefCusCodeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Turkey;
				zzRefCusCodeListCombined.ZZD_CodeType = questionTypeCode;
				zzRefCusCodeListCombined.ZZD_Code = questionCode;
				zzRefCusCodeListCombined.ZZD_Description = questionDesc;
				zzRefCusCodeListCombined.ZZD_StartDate = ZDateTime.MinSmallDateTimeValue;
				zzRefCusCodeListCombined.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
				zzRefCusCodeListCombined.ZZD_IsSystem = ZBool.True;
			}
		}

		public static ZString MapCW1CountryCodeToCustomsCode(BusinessObjectFactory factory, ZString cwCode, ZDateTime effectiveDate)
		{
			return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Turkey, CountryConstants.CountryMapType, cwCode, effectiveDate);
		}

		public static ZString GetErrorDescByCode(this BusinessObjectFactory factory, ZString errorCode)
		{
			var errorDescription = ZString.Empty;
			if (!errorCode.IsEmpty)
			{
				var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, errorCode, Core.Constants.CountryCodes.Turkey, CusEntryMessageConstants.CountryConstants.TRNCTS4ErrorCodes, ZDate.Today);
				if (cusCode != null)
				{
					errorDescription = cusCode.ZZD_Description;
				}
			}
			return errorDescription;
		}

		const string TaxOrFeeCodeListType = "OTH";
	}
}
