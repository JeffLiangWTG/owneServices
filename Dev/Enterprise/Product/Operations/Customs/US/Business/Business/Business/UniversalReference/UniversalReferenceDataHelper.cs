using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static ZString GetLicenseNo(BusinessObjectFactory factory, ZString code, ZDateTime exportDate)
		{
			return factory.GetCachedValue(ZString.Format("US_LicenseNo_{0}_{1}", code, exportDate.ToShortDateString()), () =>
			{
				var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate);
				return refCusCodeList?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AESLicenseCode) ?? ZString.Empty;
			});
		}

		public static ZBool GetLicenseNoReadOnly(BusinessObjectFactory factory, ZString code, ZDateTime exportDate)
		{
			return factory.GetCachedValue<ZBool>(ZString.Format("US_LicenseNoReadOnly_{0}_{1}", code, exportDate.ToShortDateString()), () =>
			{
				var refCusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate);
				return (refCusCodeList?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.LicenseRequired) ?? ZString.Empty) == YesNoDefaultList.Codes.Yes
				&& refCusCodeList.GetAttributesValues(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AESLicenseCode).Count() == 1;
			});
		}

		public static ZBool GetLicenseNoReadOnly_LicenseNoIsNotEmpty(BusinessObjectFactory factory, ZString code, ZDateTime exportDate, ZString licenseNo)
		{
			var result = GetLicenseNoReadOnly(factory, code, exportDate);
			if (result && !licenseNo.IsEmpty && licenseNo != GetLicenseNo(factory, code, exportDate))
			{
				result = false;
			}
			return result;
		}

		public static Universal.ITariff GetTariff(this BusinessObjectFactory factory, ZString tariffType, ZString tariffCode, ZDateTime effectiveDate)
		{
			Universal.ITariff result = null;
			if (factory != null)
			{
				if (tariffType == Universal.Constants.TariffTypes.HarmonizedSystem)
				{
					result = new USCTariff.Loader(factory).LoadBestMatch(tariffCode, effectiveDate); // TODO: this should be replace with TariffView once US Import is changed to use ZZ
				}
				else
				{
					result = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, tariffType, tariffCode, effectiveDate);
				}
			}
			return result;
		}

		public static TariffViewCollection GetTariffs(this BusinessObjectFactory factory, ZString tariffType, ZString tariffCode, ZDateTime effectiveDate)
		{
			var countryCode = Core.Constants.CountryCodes.UnitedStates;
			var result = Universal.TariffViewCollection.GetNewCollection(factory, countryCode, tariffType, effectiveDate, ZString.Empty, false);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffCode, "Property", tariffCode));
			return result;
		}

		public static ZString GetEffectiveSPIForDutyCalculation(this BusinessObjectFactory factory, ZString spiCode, ZDateTime dutyDate, USCCountry countryOfOrigin)
		{
			return factory.GetCachedValue("EffectiveSPIForDutyCalculation" + spiCode + dutyDate + countryOfOrigin?.UC_Code ?? ZString.Empty, () =>
			{
				var result = spiCode;

				bool isCountryOfOriginExcepted = false;
				bool hasSPIBeenExpired = false;

				var expiredSPIs = Universal.ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, dutyDate, ZDateTime.Today);
				foreach (var expiredSPI in expiredSPIs)
				{
					if (expiredSPI.ZZD_Code == spiCode)
					{
						hasSPIBeenExpired = true;

						var exceptionSPIList = expiredSPI.GetAttributesValues(Universal.RefCusCodeListAttributeTypes.Codes.USSPIException);
						foreach (var exceptionSPI in exceptionSPIList)
						{
							if (!exceptionSPI.IsEmpty && countryOfOrigin != null && countryOfOrigin.IsValidForSPI(exceptionSPI, dutyDate))
							{
								isCountryOfOriginExcepted = true;
								break;
							}
						}
					}
				}

				if (hasSPIBeenExpired && !isCountryOfOriginExcepted)
				{
					result = ZString.Empty;
				}

				return result;
			});
		}

		public static void ValidateNumberByRegexInZZ(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, ZString numberToValidate,
			ZString codeForValidate, ZString codeTypeForValidate, ZString maskAttributeName, ZString errorTextAttributeName, ZString messageErrorPrefix)
		{
			var (mask, errorText) = GetRefCusCodeListAttribute(factory, codeForValidate, codeTypeForValidate, maskAttributeName, errorTextAttributeName);

			if (!mask.IsEmpty && !errorText.IsEmpty && !Regex.IsMatch(numberToValidate, $@"^{mask}$"))
			{
				propertyInfo.AddMessageError(messageErrorPrefix + errorText);
			}
		}

		public static (ZString, ZString) GetRefCusCodeListAttribute(BusinessObjectFactory factory, ZString codeForValidate, ZString codeTypeForValidate, ZString maskAttributeName, ZString errorTextAttributeName)
		{
			return factory.GetCachedValue(ZString.Format("NumberRegexFor_{0}_{1}", codeTypeForValidate, codeForValidate), () =>
			{
				var refCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, codeForValidate, Core.Constants.CountryCodes.UnitedStates, codeTypeForValidate, ZDateTime.Today);
				return (refCodeList?.GetAttribute(maskAttributeName) ?? ZString.Empty, refCodeList?.GetAttribute(errorTextAttributeName) ?? ZString.Empty);
			});
		}

		public static CodeDescriptionPairList GetDispositionCodeDescriptionList(BusinessObjectFactory factory, ZString codeType)
		{
			var today = ZDateTime.Today;
			return factory.GetCachedValue("GetDispositionCodeDescriptionList" + codeType + today.ToString("MMddyy"), delegate
			{
				var result = GetRefCusCodeList(factory, codeType, today);

				if (codeType == Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode)
				{
					result.AddPair(SEBillProcessingResultList.BillStatusHoldOrExam, SEBillProcessingResultList.BillStatusHoldOrExamDesc);
				}

				result.Sort();
				return result;
			});
		}

		public static CodeDescriptionPairList GetRefCusCodeList(BusinessObjectFactory factory, ZString codeType)
		{
			var today = ZDateTime.Today;
			return factory.GetCachedValue("GetRefCusCodeList" + codeType + today.ToString("MMddyy"), delegate
			{
				return GetRefCusCodeList(factory, codeType, today);
			});
		}

		static CodeDescriptionPairList GetRefCusCodeList(BusinessObjectFactory factory, ZString codeType , ZDateTime date)
		{
			var result = new CodeDescriptionPairList();
			var codes = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, codeType, date);

			foreach (var code in codes)
			{
				result.AddPairIfNotExist(code.ZZD_Code, code.ZZD_Description);
			}

			result.Sort();
			return result;
		}

		public static ZZRefCusCodeListCombinedCollection GetCachedRefCusCodeListCombinedCollection(BusinessObjectFactory factory, ZString codeType, bool addAttributeFilterCondition = false, string attributeName = "", string attributeValue = "")
		{
			var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.UnitedStates, codeType, ZDateTime.Today);

			if (addAttributeFilterCondition)
			{
				var filter1 = new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeName, "Property", (ZString)attributeName);
				var filter2 = new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", (ZString)attributeValue);
				result.FilterBusinessObjectDefaults.Add(filter1);
				result.FilterBusinessObjectDefaults.Add(filter2);
			}
			return result;
		}

		public static ZZRefCusCodeListCombined GetCachedRefCusCodeListCombined(BusinessObjectFactory factory, ZString code, ZString codeType, ZQuery filter = null, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters = null, bool includeParentDataGrouping = true)
		{
			var today = ZDateTime.Today;
			return factory.GetCachedValue("GetCachedRefCusCodeListCombined" + codeType + code + today.ToString("MMddyy"), delegate
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(
					factory,
					code,
					Core.Constants.CountryCodes.UnitedStates,
					codeType,
					today,
					filter,
					attributeFilters,
					includeParentDataGrouping);
			});
		}
	}
}
