using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public static class RefCusCodeListTypes
	{
		/// <summary>
		/// This method returns a filtered list of codes. It filters by country, by code type and optionally by the presence of a set of attribute names
		/// </summary>
		/// <param name="factory">The filtered list will be cached in this factory</param>
		/// <param name="country">The list will only return records that match the country (SELECT * FROM RefCusCodeList WHERE ZZD_ZZK_NKDataGrouping = 'ZA')</param>
		/// <param name="codeType">The list will only return records that match the code type (SELECT * FROM RefCusCodeList WHERE ZZD_ZZK_NKCodeType = codeType')</param>
		/// <param name="transportMode">The list will only return records that match the code type (SELECT * from RefCusCodeOrAttributeTransportMode where ZZU_TransportMode = transportMode')</param>
		/// <param name="attributeNames">The list will only return records that have attributes whose names match all of the attribute names in the list (SELECT * FROM RefCusCodeList INNER JOIN RefCusAttributesList on ZZE_ParentID = ZZD_PK and ZZE_ZXE_NKName = 'AttributeName1' and ZZE_ZXE_NKName = 'AttributeName2' etc)</param>
		/// <returns></returns>
		public enum IncludeParentDataGroupingOptions
		{
			Unknown, // just to make the code analyser shut up
			ChildOnly,
			Union,
			ChildFirstThenParent
		}

		public static CodeDescriptionPairList GetCachedList(
			BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date,
			ZString[] attributeNames = null, string transportMode = "", bool includeParentDataGrouping = true, string languageCode = "", bool onlyThisLanguage = false)
		{
			if (onlyThisLanguage && string.IsNullOrEmpty(languageCode))
			{
				throw new ArgumentException("Should specify languageCode when onlyThisLanguage is true.", nameof(languageCode));
			}

			return GetCachedList(
				factory,
				() => "ZZRefCusCodeList_" + GetKey(country, codeType, date.Date, transportMode, includeParentDataGrouping, attributeNames, languageCode: languageCode, onlyThisLanguage: onlyThisLanguage),
				() => CreateList(factory, country, codeType, date, attributeNames, transportMode, includeParentDataGrouping, languageCode, onlyThisLanguage));
		}

		public static CodeDescriptionPairList GetCachedListMatchAllAttributes(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, string transportMode = "", string languageCode = "", IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			switch (includeParentDataGrouping)
			{
				case IncludeParentDataGroupingOptions.ChildOnly:
					return GetCachedList(factory,
					() => "ZZRefCusCodeList_All_" + GetKey(country, codeType, date.Date, transportMode, false, attributeNameValuePairs: attributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, includeParentDataGrouping: false, languageCode: languageCode));

				case IncludeParentDataGroupingOptions.ChildFirstThenParent:
					var result = GetCachedList(factory,
					() => "ZZRefCusCodeList_All_" + GetKey(country, codeType, date.Date, transportMode, false, attributeNameValuePairs: attributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, includeParentDataGrouping: false, languageCode: languageCode));
					if (result.Count == 0)
					{
						result = GetCachedList(factory,
						() => "ZZRefCusCodeList_All_" + GetKey(country, codeType, date.Date, transportMode, true, attributeNameValuePairs: attributeNameValuePairs),
						() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, includeParentDataGrouping: true, languageCode: languageCode));
					}
					return result;

				case IncludeParentDataGroupingOptions.Union:
					return GetCachedList(factory,
					() => "ZZRefCusCodeList_All_" + GetKey(country, codeType, date.Date, transportMode, true, attributeNameValuePairs: attributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, includeParentDataGrouping: true, languageCode: languageCode));

				default:
					return new CodeDescriptionPairList();
			}
		}

		public static CodeDescriptionPairList GetCachedListMatchAllAttributes(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, KeyValuePair<ZString, ZString>[] notExistAttributeNameValuePairs, string transportMode = "", string languageCode = "", IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			switch (includeParentDataGrouping)
			{
				case IncludeParentDataGroupingOptions.ChildOnly:
					return GetCachedList(factory,
					() => "ZZRefCusCodeList_All_NoExists_" + GetKey(country, codeType, date.Date, transportMode, false, attributeNameValuePairs: attributeNameValuePairs, notExistAttributeNameValuePairs: notExistAttributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, notExistAttributeNameValuePairs, includeParentDataGrouping: false, languageCode: languageCode));

				case IncludeParentDataGroupingOptions.ChildFirstThenParent:
					var result = GetCachedList(factory,
					() => "ZZRefCusCodeList_All_NoExists_" + GetKey(country, codeType, date.Date, transportMode, false, attributeNameValuePairs: attributeNameValuePairs, notExistAttributeNameValuePairs: notExistAttributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, notExistAttributeNameValuePairs, includeParentDataGrouping: false, languageCode: languageCode));
					if (result.Count == 0)
					{
						result = GetCachedList(factory,
						() => "ZZRefCusCodeList_All_NoExists_" + GetKey(country, codeType, date.Date, transportMode, true, attributeNameValuePairs: attributeNameValuePairs, notExistAttributeNameValuePairs: notExistAttributeNameValuePairs),
						() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, notExistAttributeNameValuePairs, includeParentDataGrouping: true, languageCode: languageCode));
					}
					return result;

				case IncludeParentDataGroupingOptions.Union:
					return GetCachedList(factory,
					() => "ZZRefCusCodeList_All_NoExists_" + GetKey(country, codeType, date.Date, transportMode, true, attributeNameValuePairs: attributeNameValuePairs, notExistAttributeNameValuePairs: notExistAttributeNameValuePairs),
					() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, true, notExistAttributeNameValuePairs, includeParentDataGrouping: true, languageCode: languageCode));

				default:
					return new CodeDescriptionPairList();
			}
		}

		public static CodeDescriptionPairList GetCachedListMatchAnyAttributes(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, string transportMode = "", bool includeParentDataGrouping = true)
		{
			return GetCachedList(
				factory,
				() => "ZZRefCusCodeList_Any_" + GetKey(country, codeType, date.Date, transportMode, includeParentDataGrouping, attributeNameValuePairs: attributeNameValuePairs),
				() => CreateList(factory, country, codeType, date, attributeNameValuePairs, transportMode, false, includeParentDataGrouping));
		}

		public static CodeDescriptionPairList GetCachedListMatchSingleAttributeValues(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, bool matchIfAttributeNotExists, ZString attributeName, ZString[] attributeValues = null, string transportMode = "")
		{
			return GetCachedList(
				factory,
				() => "ZZRefCusCodeList_Single_" + GetKey(country, codeType, date.Date, transportMode, true, matchIfAttributeNotExists: matchIfAttributeNotExists, attributeName: attributeName, attributeValues: attributeValues),
				() => CreateList(factory, country, codeType, date, attributeName, attributeValues, transportMode, matchIfAttributeNotExists));
		}

		public static CodeDescriptionPairList GetCachedListValidBeforeDate(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime dateBefore, ZString[] attributeNames = null, string transportMode = "", bool includeParentDataGrouping = true)
		{
			return GetCachedList(
				factory,
				() => "ZZRefCusCodeList_Before_" + GetKey(country, codeType, dateBefore.Date, transportMode, includeParentDataGrouping, attributeNames),
				() => CreateListValideBefore(factory, country, codeType, dateBefore, null, includeParentDataGrouping));
		}

		static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, Func<ZString> getKey, Func<CodeDescriptionPairList> createList)
		{
			CodeDescriptionPairList result = null;
			if (factory == null)
			{
				result = new CodeDescriptionPairList();
			}
			else
			{
				result = factory.GetCachedValue(getKey(), () => createList());
			}
			return result;
		}

		public static ZString GetKey(
			ZString country, ZString codeType, ZDate date, ZString transportMode,
			bool includeParentDataGrouping = true,
			ZString[] attributes = null,
			KeyValuePair<ZString, ZString>[] attributeNameValuePairs = null,
			bool matchIfAttributeNotExists = false,
			string attributeName = null,
			ZString[] attributeValues = null,
			KeyValuePair<ZString, ZString>[] notExistAttributeNameValuePairs = null,
			string languageCode = "",
			bool onlyThisLanguage = false)
		{
			var result = new ZStringBuilder(country.PadRight(ZZRefCusCodeListCombined.Schema.ZZD_CountryOrGroupingMaxLength));
			result.Append(codeType.PadRight(ZZRefCusCodeListCombined.Schema.ZZD_CodeTypeMaxLength));
			result.Append(date.ToString("yyMMdd", CultureInfo.InvariantCulture));
			result.Append(transportMode);
			result.Append(includeParentDataGrouping.ToString());

			if (attributes != null)
			{
				attributes.OrderBy(x => x).ToList().ForEach(y => result.Append("a-" + y));
			}

			if (attributeNameValuePairs != null)
			{
				attributeNameValuePairs.Select(x => x.Key + ":" + x.Value).OrderBy(x => x).ToList().ForEach(y => result.Append("p-" + y));
			}

			if (notExistAttributeNameValuePairs != null)
			{
				notExistAttributeNameValuePairs.Select(x => x.Key + ":" + x.Value).OrderBy(x => x).ToList().ForEach(y => result.Append("x-" + y));
			}

			if (!string.IsNullOrEmpty(attributeName))
			{
				result.Append("n-" + attributeName);
				result.Append("m-" + matchIfAttributeNotExists.ToString());

				if (attributeValues != null && attributeValues.Any())
				{
					attributeValues.OrderBy(x => x).ToList().ForEach(y => result.Append("v-" + y));
				}
			}

			if (string.IsNullOrEmpty(languageCode))
			{
				result.Append(TranslationHelper.GetCurrentLanguageCode());
			}
			else
			{
				result.Append(languageCode);
			}

			result.Append("l-" + onlyThisLanguage.ToString());

			return result.ToStringWithDelimiterBetweenAppends("_");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Field Name")]
		internal const string CodeFieldName = "Code";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant Field Name")]
		internal const string DescriptionFieldName = "Description";

		static Func<IRefCusCodeListSqlQueryBuilder, IRefCusCodeListSqlQueryBuilder> CreateRefCusCodeListTransportModeQueryBuilderFactory(ZString transportMode)
		{
			if (transportMode.IsEmpty)
			{
				return null;
			}

			return (builder) => new RefCusCodeListTransportModeQueryBuilder(builder, transportMode);
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, ZQuery filter, bool includeParentDataGrouping, string languageCode = "", bool onlyThisLanguage = false, params Func<IRefCusCodeListSqlQueryBuilder, IRefCusCodeListSqlQueryBuilder>[] builderFactories)
		{
			var query = ZZRefCusCodeListCombined.Loader.GetFilter(factory, country, codeType, date, filter, includeParentDataGrouping);
			var result = LoadWithDirectQuery(
				factory,
				query,
				languageCode,
				onlyThisLanguage,
				builderFactories);
			SetAdditionalInformation(result, query);
			return result;
		}

		static void SetAdditionalInformation(IAdditionalInformationWithSetter setter, ZQuery query)
		{
#if !DEBUG
			if (Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.IsSupportUser)
#endif
			{
				setter.SetAdditionalInformation((NoResString)"Filter: " + query.LiteralTextSqlFormatted);
			}
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, ZString[] attributeNames, ZString transportMode, bool includeParentDataGrouping = true, string languageCode = "", bool onlyThisLanguage = false)
		{
			var filter = new ZQuery();
			var queryBuilderFactory = CreateRefCusCodeListTransportModeQueryBuilderFactory(transportMode);

			if (attributeNames != null && attributeNames.Any())
			{
				var attrFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				foreach (var attributeName in attributeNames)
				{
					var attrSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
					attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attributeName);
					attrFilter.AddSubQuery(attrSubFilter, JoinCondition.And);
				}
				filter.AddToFilter(attrFilter);
			}

			return CreateList(factory, country, codeType, date, filter, includeParentDataGrouping, languageCode, onlyThisLanguage, queryBuilderFactory);
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, ZString transportMode, ZBool matchall, bool includeParentDataGrouping = true, string languageCode = "")
		{
			var filter = new ZQuery();
			var queryBuilderFactory = CreateRefCusCodeListTransportModeQueryBuilderFactory(transportMode);

			if (attributeNameValuePairs != null && attributeNameValuePairs.Any())
			{
				var attrFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				foreach (var nameValePair in attributeNameValuePairs)
				{
					var attrSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
					attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, nameValePair.Key);
					if (!nameValePair.Value.IsEmpty)
					{
						attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, nameValePair.Value);
					}
					attrFilter.AddSubQuery(attrSubFilter, matchall ? JoinCondition.And : JoinCondition.Or);
				}
				filter.AddToFilter(attrFilter);
			}

			return CreateList(factory, country, codeType, date, filter, includeParentDataGrouping, languageCode, builderFactories: queryBuilderFactory);
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, ZString transportMode, ZBool matchall, KeyValuePair<ZString, ZString>[] notExistAttributeNameValuePairs, bool includeParentDataGrouping = true, string languageCode = "")
		{
			var filter = new ZQuery();
			var queryBuilderFactory = CreateRefCusCodeListTransportModeQueryBuilderFactory(transportMode);

			if (attributeNameValuePairs != null && attributeNameValuePairs.Any())
			{
				var attrFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				foreach (var nameValePair in attributeNameValuePairs)
				{
					var attrSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
					attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, nameValePair.Key);
					if (!nameValePair.Value.IsEmpty)
					{
						attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, nameValePair.Value);
					}
					attrFilter.AddSubQuery(attrSubFilter, matchall ? JoinCondition.And : JoinCondition.Or);
				}

				foreach (var nameValePair in notExistAttributeNameValuePairs)
				{
					var attrSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
					attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, nameValePair.Key);
					if (!nameValePair.Value.IsEmpty)
					{
						attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, nameValePair.Value);
					}
					attrFilter.AddSubQuery(attrSubFilter, matchall ? JoinCondition.And : JoinCondition.Or);
				}

				filter.AddToFilter(attrFilter);
			}

			return CreateList(factory, country, codeType, date, filter, includeParentDataGrouping, languageCode, builderFactories: queryBuilderFactory);
		}

		static CodeDescriptionPairList CreateList(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, ZString attributeName, ZString[] attributeValues, ZString transportMode, ZBool matchIfAttributeNotExists, bool includeParentDataGrouping = true, string languageCode = "")
		{
			var filter = new ZQuery();
			var queryBuilderFactory = CreateRefCusCodeListTransportModeQueryBuilderFactory(transportMode);

			if (!attributeName.IsEmpty)
			{
				var attrFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				var attrSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
				attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attributeName);
				if (attributeValues != null && attributeValues.Any())
				{
					attrSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, attributeValues);
				}
				attrFilter.AddSubQuery(attrSubFilter, JoinCondition.Or);

				if (matchIfAttributeNotExists)
				{
					var notExistsSubFilter = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
					notExistsSubFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attributeName);
					attrFilter.AddSubQuery(notExistsSubFilter, JoinCondition.Or);
				}
				filter.AddToFilter(attrFilter, JoinCondition.And);
			}

			return CreateList(factory, country, codeType, date, filter, includeParentDataGrouping, languageCode, builderFactories: queryBuilderFactory);
		}

		static CodeDescriptionPairList CreateListValideBefore(BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime dateBefore, ZQuery filter, bool includeParentDataGrouping)
		{
			var query = ZZRefCusCodeListCombined.Loader.GetFilterForStartDateBeforeAndAfterToday(factory, country, codeType, dateBefore, filter, includeParentDataGrouping: includeParentDataGrouping);
			var result = LoadWithDirectQuery(factory, query);
			SetAdditionalInformation(result, query);
			return result;
		}

		static CodeDescriptionPairList LoadWithDirectQuery(BusinessObjectFactory factory, ZQuery query, string languageCode = "", bool onlyThisLanguage = false, params Func<IRefCusCodeListSqlQueryBuilder, IRefCusCodeListSqlQueryBuilder>[] builderFactories)
		{
			if (query.IsNoResultQuery)
			{
				return new CodeDescriptionPairList();
			}

			var dynamicBOs = new DynamicBusinessObjectCollection(factory);
			if (string.IsNullOrEmpty(languageCode))
			{
				languageCode = TranslationHelper.GetCurrentLanguageCode();
			}

			IRefCusCodeListSqlQueryBuilder builder = new RefCusCodeListQueryBuilder(languageCode, onlyThisLanguage);
			foreach (var builderFactory in builderFactories.Where(f => f != null))
			{
				builder = builderFactory(builder);
			}

			var refCusCodeListSqlQuery = builder.Build(query);
			var (queryText, parameters) = refCusCodeListSqlQuery.GetSql();
			dynamicBOs.Load(queryText, parameters.ToArray());

			var list = new CodeDescriptionPairList();
			var loadedCodes = dynamicBOs.Select(x => new { Code = new ZString(x[CodeFieldName]), Description = new ZString(x[DescriptionFieldName]) });
			if (loadedCodes.Any())
			{
				var allNumbers = loadedCodes.All(x => x.Code.IsNumbersOnlyOrEmpty);
				var maxLength = loadedCodes.Max(c => c.Code.Length);
				foreach (var codeList in loadedCodes.OrderBy(x => allNumbers ? x.Code.PadLeft(maxLength, ' ') : x.Code))
				{
					list.AddPair(codeList.Code, codeList.Description);
				}
			}
			return list;
		}
	}

	public class RefCusCodeListTypesListProvider : Integration.Customs.Shared.Universal.IRefCusCodeListTypesListProvider
	{
		// This is so MasterFiles.Biz can use this functionality
		public ICodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime date, ZString[] attributeNames)
		{
			return RefCusCodeListTypes.GetCachedList(factory, dataGrouping, codeType, date, attributeNames);
		}
		public ICodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime dateBefore)
		{
			return RefCusCodeListTypes.GetCachedListValidBeforeDate(factory, dataGrouping, codeType, dateBefore, null);
		}
		public IBusinessObjectCollection GetCollection(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime dateBefore)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, dataGrouping, codeType, dateBefore);
		}

		public IBusinessObjectCollection GetCollection(BusinessObjectFactory factory, ZString[] dataGroupings, ZString codeType, ZDateTime date)
		{
			var codeTypesKeyPart = string.Join(",", codeType);
			var attributeFiltersList = new List<RefCusCodeListAttributeFilter>(0);
			var attrFilterKeyPart = string.Join(";", attributeFiltersList.Select(x => x.Key));
			var dataGroupingsFilterString = string.Join(",", dataGroupings);
			var key = string.Join("_", "ZZRefCusCodeListCombinedCollection", dataGroupings[0], codeTypesKeyPart, date.Date, attrFilterKeyPart, false, dataGroupingsFilterString);

			return factory.GetCachedValue(key,
				() =>
				{
					var collection = new ZZRefCusCodeListCombinedCollection(factory, dataGroupings, new[] { codeType }, date, null);
					if (!string.IsNullOrEmpty(codeType))
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", codeType));
					}

					if (date.IsValid)
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", date));
					}

					foreach (var dataGrouping in dataGroupings)
					{
						var filterName = $"{Constants.ZZRefCusCodeListFilters.CountryOrGrouping}{(dataGroupings.Length == 1 ? string.Empty : " " + dataGrouping)}";
						var category = dataGroupings.Length == 1 ? ZArchitecture.Business.FilterOrCategory.None : ZArchitecture.Business.FilterOrCategory.Green;
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", dataGrouping, category));
					}
					return collection;
				});
		}
	}
}
