using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	public static class TranslationHelper
	{
		[ThreadSafe]
		readonly static Dictionary<ZString, ZString> dicCountryLanguageCode = new Dictionary<ZString, ZString>()
		{
			{ Core.Constants.CountryCodes.Spain, Enterprise.Core.SharedConstants.Languages.Spanish },
			{ Core.Constants.CountryCodes.Germany, Enterprise.Core.SharedConstants.Languages.German },
			{ Core.Constants.CountryCodes.France, Enterprise.Core.SharedConstants.Languages.French },
			{ Core.Constants.CountryCodes.China, Enterprise.Core.SharedConstants.Languages.ChineseSimplified },
			{ Core.Constants.CountryCodes.Taiwan, Enterprise.Core.SharedConstants.Languages.ChineseTraditional },
			{ Core.Constants.CountryCodes.Italy, Enterprise.Core.SharedConstants.Languages.Italian },
			{ Core.Constants.CountryCodes.Turkey, Enterprise.Core.SharedConstants.Languages.Turkish },
			{ Core.Constants.CountryCodes.Netherlands, Enterprise.Core.SharedConstants.Languages.Dutch },
			{ Core.Constants.CountryCodes.Belgium, Enterprise.Core.SharedConstants.Languages.Dutch },
			{ Core.Constants.CountryCodes.Denmark, Enterprise.Core.SharedConstants.Languages.Danish },
			{ Core.Constants.CountryCodes.Estonia, Enterprise.Core.SharedConstants.Languages.Estonian },
			{ Core.Constants.CountryCodes.Finland, Enterprise.Core.SharedConstants.Languages.Finnish },
			{ Core.Constants.CountryCodes.CzechRepublic, Enterprise.Core.SharedConstants.Languages.Czech },
			{ Core.Constants.CountryCodes.Sweden, Enterprise.Core.SharedConstants.Languages.Swedish },
			{ Core.Constants.CountryCodes.Hungary, Enterprise.Core.SharedConstants.Languages.Hungarian },
			{ Core.Constants.CountryCodes.Latvia, Enterprise.Core.SharedConstants.Languages.Latvian },
			{ Core.Constants.CountryCodes.Greece, Enterprise.Core.SharedConstants.Languages.Greek },
			{ Core.Constants.CountryCodes.Slovenia, Enterprise.Core.SharedConstants.Languages.Slovenian },
			{ Core.Constants.CountryCodes.Lithuania, Enterprise.Core.SharedConstants.Languages.Lithuanian },
			{ Core.Constants.CountryCodes.Romania, Enterprise.Core.SharedConstants.Languages.Romanian },
			{ Core.Constants.CountryCodes.Bulgaria, Enterprise.Core.SharedConstants.Languages.Bulgarian },
			{ Core.Constants.CountryCodes.Croatia, Enterprise.Core.SharedConstants.Languages.Croation },
			{ Core.Constants.CountryCodes.Poland, Enterprise.Core.SharedConstants.Languages.Polish },
			{ Core.Constants.CountryCodes.Slovakia, Enterprise.Core.SharedConstants.Languages.Slovak },
			{ Core.Constants.CountryCodes.Portugal, Enterprise.Core.SharedConstants.Languages.Portuguese },
			{ Core.Constants.CountryCodes.Brazil, Enterprise.Core.SharedConstants.Languages.Portuguese },
			{ Core.Constants.CountryCodes.KoreaSouth, Enterprise.Core.SharedConstants.Languages.Korean },
			{ Core.Constants.CountryCodes.Japan, Enterprise.Core.SharedConstants.Languages.Japanese },
			{ Core.Constants.CountryCodes.Israel, Enterprise.Core.SharedConstants.Languages.Hebrew }
		};

		internal static ZString GetTranslatedValue(ITranslatableZZBusinessObject bizO, ZString defaultValue, SchemaColumn translationSchemaColumn)
		{
			return GetTranslatedValue(bizO, defaultValue, translationSchemaColumn, TranslationHelper.GetCurrentLanguageCode());
		}

		public static ZString GetTranslatedValue(ITranslatableZZBusinessObject bizO, ZString defaultValue, SchemaColumn translationSchemaColumn, ZString languageCode)
		{
			var result = ZString.Empty;
			if (!languageCode.IsEmpty)
			{
				var key = ZString.Format("{0}_{1}_{2}_{3}", bizO.GetType().Name, bizO.Identifier, translationSchemaColumn.Name, languageCode);
				result = bizO.Factory.GetCachedValue(key, () =>
				{
					var workingLanguage = GetWorkingLanguage(bizO, translationSchemaColumn.TableSchema, languageCode);
					if (workingLanguage != null)
					{
						return (ZString)workingLanguage[translationSchemaColumn.Name];
					}

					return ZString.Empty;
				});
			}

			return result.IsEmpty ? defaultValue : result;
		}

		internal static ZString GetAlternateLanguageDescription(ITranslatableZZBusinessObject bizO, SchemaColumn translationSchemaColumn)
		{
			return GetAlternateLanguageDescription(bizO, translationSchemaColumn, TranslationHelper.GetCurrentLanguageCode(), true);
		}

		internal static ZString GetAlternateLanguageDescription(ITranslatableZZBusinessObject bizO, SchemaColumn translationSchemaColumn, ZString languageCode, bool useCountryLanguageAsFallback = false)
		{
			var key = ZString.Format("{0}_{1}_{2}_{3}_{4}_Alternate", bizO.GetType().Name, bizO.Identifier, translationSchemaColumn.Name, languageCode, useCountryLanguageAsFallback);
			var result = bizO.Factory.GetCachedValue(key, () =>
			{
				var alternateLanguage = GetAlternateLanguage(bizO, translationSchemaColumn, languageCode, useCountryLanguageAsFallback);
				if (alternateLanguage != null)
				{
					return (ZString)alternateLanguage[translationSchemaColumn.Name];
				}

				return ZString.Empty;
			});

			if (!result.IsEmpty)
			{
				return result;
			}
			else
			{
				return Constants.RefCusTariffFilters.NotAvailable;
			}
		}

		static BusinessObject GetWorkingLanguage(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema, ZString languageCode)
		{
			var key = ZString.Format("{0}Language_{1}_{2}", bizO.GetType().Name, bizO.Identifier, languageCode);
			return bizO.Factory.GetCachedValue(key, () =>
			{
				return bizO.Factory.LoadTop1(bizO.LanguageTableType, GetWorkingLanguageQuery(bizO, languageTableSchema, languageCode));
			});
		}

		static BusinessObject GetAlternateLanguage(ITranslatableZZBusinessObject bizO, SchemaColumn translationSchemaColumn, ZString languageCode, bool useCountryLanguageAsFallback)
		{
			var countryLanguageCode = TranslationHelper.GetCurrentCountryLanguageCode();

			var key = ZString.Format("{0}AlternateLanguage_{1}_{2}_{3}", bizO.GetType().Name, bizO.Identifier, languageCode, useCountryLanguageAsFallback);
			var languageTableName = translationSchemaColumn.TableSchema.TableName;
			var languageColumnName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(languageTableName), RefLanguageTypeSchema.Constants.Prefix, "NKLanguage");
			return bizO.Factory.GetCachedValue(key, () =>
			{
				BusinessObject result = null;
				ZQuery languageQuery = useCountryLanguageAsFallback ? GetAlternateLanguageQuery(bizO, translationSchemaColumn.TableSchema, languageCode, countryLanguageCode) : GetAlternateLanguageQuery(bizO, translationSchemaColumn.TableSchema, languageCode);
				var languageResults = bizO.Factory.Load(bizO.LanguageTableType, languageQuery);

				if (languageResults != null)
				{
					result = languageResults.FirstOrDefault(c => ((ZString)c[languageColumnName]).StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
					if (result == null && useCountryLanguageAsFallback)
					{
						return languageResults.FirstOrDefault(c => ((ZString)c[languageColumnName]).StartsWith(countryLanguageCode, StringComparison.OrdinalIgnoreCase));
					}
				}
				return result;
			});
		}

		internal static ZQuery GetWorkingLanguageQuery(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema)
		{
			return GetWorkingLanguageQuery(bizO, languageTableSchema, GetCurrentLanguageCode());
		}

		internal static ZQuery GetWorkingLanguageQuery(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema, ZString languageCode)
		{
			return GetLanguagesQuery(bizO, languageTableSchema, new ZString[] { languageCode });
		}

		internal static ZQuery GetAlternateLanguageQuery(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema, ZString languageCode)
		{
			return GetLanguagesQuery(bizO, languageTableSchema, new ZString[] { languageCode });
		}

		internal static ZQuery GetAlternateLanguageQuery(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema, ZString languageCode, ZString countryLanguageCode)
		{
			return GetLanguagesQuery(bizO, languageTableSchema, new ZString[] { languageCode, countryLanguageCode });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		internal static ZQuery GetLanguagesQuery(ITranslatableZZBusinessObject bizO, ITableSchema languageTableSchema, ZString[] languageCodes)
		{
			var tableName = bizO.TableName;
			var languageTableName = languageTableSchema.TableName;

			var dependentColumnKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(languageTableName), ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName));
			var dependentColumn = languageTableSchema.All.FirstOrDefault(schemaColumn => schemaColumn.Name.StartsWith(dependentColumnKey, StringComparison.OrdinalIgnoreCase));
			var languageColumnName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(languageTableName), RefLanguageTypeSchema.Constants.Prefix, "NKLanguage");
			var languageColumn = languageTableSchema.GetSchemaColumn(languageColumnName);

			var query = new ZQuery(dependentColumn, bizO.Identifier);
			var languageCodeQuery = new ZQuery();
			foreach (ZString languageCode in languageCodes)
			{
				if (languageCode.IsEmpty)
				{
					languageCodeQuery.AddToFilter(JoinCondition.Or, languageColumn, languageCode);
				}
				else
				{
					languageCodeQuery.AddToFilter(JoinCondition.Or, languageColumn, SQLComparisonOperator.Like, languageCode + "%");
				}
			}
			query.AddToFilter(languageCodeQuery);
			return query;
		}

		public static ZString GetCurrentLanguageCode()
		{
			return GetLanguageCode(Env.CurrentUser?.Language ?? GetCurrentCountryLanguageCode());
		}

		public static ZString GetLanguageCode(ZString language)
		{
			var result = ZString.Empty;
			if (language.Length > 1)
			{
				switch (language)
				{
					case Core.SharedConstants.Languages.ChineseSimplified:
						result = Core.Constants.Customs.Universal.RefLanguageType.Codes.ChineseSimplified;
						break;
					case Core.SharedConstants.Languages.ChineseTraditional:
						result = Core.Constants.Customs.Universal.RefLanguageType.Codes.ChineseTraditional;
						break;
					case Core.SharedConstants.Languages.Japanese:
						result = Core.Constants.Customs.Universal.RefLanguageType.Codes.Japanese;
						break;
					default:
						result = language.Left(2);
						break;
				}
			}
			return result;
		}

		public static ZString GetCurrentCountryLanguageCode()
		{
			return GetLanguageCodeForCountry(Env.CurrentCompany.Country.Code);
		}

		static ZString GetCountryLanguageCode(ZString countryCode)
		{
			return dicCountryLanguageCode.TryGetValue(countryCode, out var processResult)
				? processResult
				: ZString.Empty;
		}

		public static ZString GetLanguageCodeForCountry(ZString countryCode)
		{
			var language = GetCountryLanguageCode(countryCode);
			return GetLanguageCode(language);
		}
	}
}
