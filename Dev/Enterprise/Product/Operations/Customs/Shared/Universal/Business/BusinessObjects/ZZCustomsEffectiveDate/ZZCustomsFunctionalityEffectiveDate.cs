using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZCustomsFunctionalityEffectiveDate : IZZCustomsFunctionalityEffectiveDate
	{
		bool IZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate)
		{
			return IsFunctionalityValid(code, dataGroupingCode, effectiveDate);
		}

		bool IZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString companyKey)
		{
			return IsFunctionalityValid(code, dataGroupingCode, effectiveDate, companyKey);
		}

		public static bool IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, bool priorityToPilotFunctionality = false)
		{
			return IsFunctionalityValid(code, dataGroupingCode, effectiveDate, GlbCompany.CurrentCompany.LicenceKeyIdentifier, priorityToPilotFunctionality);
		}

		public static bool IsFunctionalityValid(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString companyKey, bool priorityToPilotFunctionality = false)
		{
			var key = GetKeyForFunctionality(code, dataGroupingCode, companyKey);
			if (!FunctionalityKeys.TryGetValue(key, out var result))
			{
				try
				{
					result = LoadAndAddToCache(key, code, dataGroupingCode, companyKey, priorityToPilotFunctionality);
				}
				catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.SynonymRefersToAnInvalidObject)
				{
					return false;
				}
			}

			var dateRange = result as (ZDate StartDate, ZDate EndDate)?;
			return dateRange != null && dateRange?.StartDate <= effectiveDate.Date && effectiveDate.Date <= dateRange?.EndDate;
		}

		public static ZString GetEffectiveCusCodeAttribute(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString attributeName)
		{
			return GetEffectiveCusCodeAttribute(code, dataGroupingCode, effectiveDate, attributeName, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
		}

		public static ZString GetEffectiveCusCodeAttribute(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString attributeName, ZString companyKey)
		{
			var key = string.Join("|", code, dataGroupingCode, effectiveDate.ToString("yyMMdd", CultureInfo.InvariantCulture), companyKey, attributeName);

			if (!FunctionalityKeys.TryGetValue(key, out var result))
			{
				try
				{
					var factory = new BusinessObjectFactory() { NameForDebugging = nameof(ZZCustomsFunctionalityEffectiveDate) };
					var cusCode = GetNewCustomsFunctionalityCusCodeList(factory, code, dataGroupingCode, effectiveDate);
					if (cusCode == null)
					{
						var pilotCusCode = GetPilotFunctionalityCusCodeList(factory, code, dataGroupingCode, effectiveDate);
						if (IsPilotFunctionalityValid(pilotCusCode, companyKey))
						{
							cusCode = pilotCusCode;
						}
					}
					result = cusCode?.GetAttribute(attributeName) ?? ZString.Empty;
					FunctionalityKeys.Add(key, result);
				}
				catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.SynonymRefersToAnInvalidObject)
				{
					return ZString.Empty;
				}
			}
			return (ZString)result;
		}

		public static bool IsFunctionalityDefined(ZString code, bool priorityToPilotFunctionality = false)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var companyKey = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

			if (code.IsEmpty || countryCode.IsEmpty)
			{
				return false;
			}

			var key = GetKeyForFunctionality(code, countryCode, companyKey);
			if (!FunctionalityKeys.TryGetValue(key, out var result))
			{
				try
				{
					result = LoadAndAddToCache(key, code, countryCode, companyKey, priorityToPilotFunctionality);
				}
				catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.SynonymRefersToAnInvalidObject)
				{
					return false;
				}
			}

			return result as (ZDate, ZDate)? != null;
		}

		static (ZDate StartDate, ZDate EndDate)? LoadAndAddToCache(string key, ZString code, ZString countryCode, ZString companyKey, bool priorityToPilotFunctionality)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(ZZCustomsFunctionalityEffectiveDate) };
			ZZRefCusCodeListCombined functionalityItem = null;

			if (!priorityToPilotFunctionality)
			{
				functionalityItem = GetProductionFunctionality() ?? GetPilotFunctionality();
			}
			else
			{
				functionalityItem = GetPilotFunctionality() ?? GetProductionFunctionality();
			}

			var result = functionalityItem != null ? (functionalityItem.ZZD_StartDate.Date, functionalityItem.ZZD_EndDate.Date) : ((ZDate, ZDate)?)null;
			FunctionalityKeys.Add(key, result);
			return result;

			ZZRefCusCodeListCombined GetProductionFunctionality() => LoadFirstOrDefault(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, code, countryCode);

			ZZRefCusCodeListCombined GetPilotFunctionality() => GetPilotFunctionalityWithAttribute(LoadFirstOrDefault(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, code, countryCode), companyKey);
		}

		static string GetKeyForFunctionality(ZString code, ZString countryCode, ZString companyKey) => string.Join("|", code, countryCode, companyKey);

		static ZZRefCusCodeListCombined LoadFirstOrDefault(BusinessObjectFactory factory, ZString codeType, ZString code, ZString countryCode)
		{
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, codeType);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, countryCode);
			return factory.Load<ZZRefCusCodeListCombined>(query)
				.OrderBy((ZZRefCusCodeListCombined x) => x.ZZD_StartDate)
				.FirstOrDefault();
		}

		static ZZRefCusCodeListCombined GetNewCustomsFunctionalityCusCodeList(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime effectiveDate)
			=> ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, effectiveDate);

		static ZZRefCusCodeListCombined GetPilotFunctionalityCusCodeList(BusinessObjectFactory factory, ZString code, ZString dataGroupingCode, ZDateTime effectiveDate)
			=> ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, effectiveDate);

		static bool IsPilotFunctionalityValid(ZZRefCusCodeListCombined pilotFunctionalityCusCodeList, ZString companyKey)
		{
			return GetPilotFunctionalityWithAttribute(pilotFunctionalityCusCodeList, companyKey) != null;
		}

		public static ZZRefCusCodeListCombined GetPilotFunctionalityWithAttribute(ZZRefCusCodeListCombined pilotFunctionalityCusCodeList, ZString companyKey)
		{
			ZZRefCusCodeListCombined result = null;
			if (pilotFunctionalityCusCodeList != null &&
				(pilotFunctionalityCusCodeList.HasAttribute(RefCusCodeListAttributeTypes.Codes.Company, companyKey) ||
				pilotFunctionalityCusCodeList.HasAttribute(RefCusCodeListAttributeTypes.Codes.System, string.Concat(companyKey.Left(3), companyKey.Right(3)))))
			{
				result = pilotFunctionalityCusCodeList;
			}
			return result;
		}

		static Dictionary<string, object> FunctionalityKeys => functionalityKeys ?? (functionalityKeys = new Dictionary<string, object>());

		[ThreadStatic]
		static Dictionary<string, object> functionalityKeys;

#if DEBUG
		IDisposable IZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, bool value)
		{
			return TemporarilySetFunctionality(code, dataGroupingCode, effectiveDate, value);
		}

		public static IDisposable TemporarilySetFunctionality(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, bool value)
		{
			var key = GetKeyForFunctionality(code, dataGroupingCode, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			FunctionalityKeys[key] = value ? (effectiveDate.Date, effectiveDate.Date) : ((ZDate, ZDate)?)null;
			return new DisposableAction(() => FunctionalityKeys.Remove(key));
		}

		public static IDisposable TemporarilySetFunctionalityAttribute(ZString code, ZString dataGroupingCode, ZDateTime effectiveDate, ZString attributeName, ZString value)
		{
			var key = string.Join("|", code, dataGroupingCode, effectiveDate.ToString("yyMMdd", CultureInfo.InvariantCulture), GlbCompany.CurrentCompany.LicenceKeyIdentifier, attributeName);
			FunctionalityKeys[key] = value;
			return new DisposableAction(() => FunctionalityKeys.Remove(key));
		}

		public static void ClearDictionary()
		{
			functionalityKeys.Clear();
		}
#endif
	}
}
