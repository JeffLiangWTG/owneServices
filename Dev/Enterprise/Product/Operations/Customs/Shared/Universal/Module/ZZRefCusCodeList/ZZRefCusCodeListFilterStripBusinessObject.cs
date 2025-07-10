using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ModuleIDs = Enterprise.ZArchitecture.Modules.ModuleIDs;

namespace Enterprise.Customs.Universal.Module
{
	class ZZRefCusCodeListFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ZZRefCusCodeListFilterStripBusinessObject()
		{
			this.QueryObjectType = typeof(ZZRefCusCodeListCombined);
		}

		public ZZRefCusCodeListFilterStripBusinessObject(ZZRefCusCodeListCombinedCollection collection)
		{
			this.QueryObjectType = typeof(ZZRefCusCodeListCombined);
			this.collection = collection;
			if (collection != null && this is IFilterStripBusinessObjectInternals fsbObjectInternals)
			{
				var dataGroupingCodes = collection.DataGroupingCodes;
				var codeTypes = collection.CodeTypes;
				var isDataGroupValid = dataGroupingCodes?.Any(x => !x.IsEmpty) ?? false;
				var isCodeTypesValid = codeTypes?.Any() ?? false;
				if (isDataGroupValid || isCodeTypesValid)
				{
					fsbObjectInternals.LayoutContext = string.Join("_", string.Join("_", isDataGroupValid ? string.Join("_", dataGroupingCodes?.OrderBy(x => x)) : "", isCodeTypesValid ? string.Join("_", codeTypes?.OrderBy(x => x)) : ""));
				}
			}
		}

		readonly ZZRefCusCodeListCombinedCollection collection;

		List<ModuleNkFilter> countryOrGroupingFilters;
		ModuleTextFilter listTypeFilter;
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.Code, ZZRefCusCodeListCombinedSchema.ZZD_Code);
			codeFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			codeFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			codeFilter.Visibility = FilterVisibility.AlwaysVisible;
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|Code", Constants.ZZRefCusCodeListFilters.Code);

			var descriptionFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.Description, ZZRefCusCodeListCombinedSchema.ZZD_Description);
			descriptionFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			descriptionFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|Description", Constants.ZZRefCusCodeListFilters.Description);

			AddCountryOrGroupingFilters(filters);

			listTypeFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.ListType, GetListTypeQuery, GetTypeList);
			listTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			listTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|ListType", Constants.ZZRefCusCodeListFilters.ListType);

			var listTypeDescriptionFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.ListTypeDescription, GetListTypeDescriptionQuery);
			listTypeDescriptionFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			listTypeDescriptionFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			listTypeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|ListTypeDescription", Constants.ZZRefCusCodeListFilters.ListTypeDescription);

			var effectiveDateFilter = filters.AddSingleDateFilter(Constants.ZZRefCusCodeListFilters.EffectiveDate, delegate(ZDateTime date)
			{
				var dateQuery = new ZQuery();
				if (date.IsValid)
				{
					dateQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
					dateQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				}
				return dateQuery;
			});
			effectiveDateFilter.Property1 = ZDateTime.Today;
			effectiveDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|EffectiveDate", Constants.ZZRefCusCodeListFilters.EffectiveDate);

			var attributeNameFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.AttributeName, GetAttributeNameQuery, GetAttributeNameList);
			attributeNameFilter.Category = FilterCategories.AttributeSearch;
			attributeNameFilter.SubGroup = AttributeSubGroup;
			attributeNameFilter.ComparisonOperator_List.Clear();
			attributeNameFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			attributeNameFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|AttributeName", Constants.ZZRefCusCodeListFilters.AttributeName);

			var attributeValueFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.AttributeValue, ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value);
			attributeValueFilter.Category = FilterCategories.AttributeSearch;
			attributeValueFilter.SubGroup = AttributeSubGroup;
			attributeValueFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|AttributeValue", Constants.ZZRefCusCodeListFilters.AttributeValue);

			var transportModeFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.TransportMode, GetTransportModeQuery, new RefTransportModeList());
			transportModeFilter.MaxLength = RefTransportModesHelper.MaxLength;
			transportModeFilter.ErrorOnCodeNotPresent = true;
			transportModeFilter.ComparisonOperator_List.Clear();
			transportModeFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			transportModeFilter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|TransportMode", Constants.ZZRefCusCodeListFilters.TransportMode);

			var attributeTransportModeFilter = filters.AddTextFilter(Constants.ZZRefCusCodeListFilters.AttributeTransportMode, GetAttributeTransportModeQuery, new RefTransportModeList());
			attributeTransportModeFilter.MaxLength = RefTransportModesHelper.MaxLength;
			attributeTransportModeFilter.SubGroup = AttributeSubGroup;
			attributeTransportModeFilter.ErrorOnCodeNotPresent = true;
			attributeTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|AttributeTransportMode", Constants.ZZRefCusCodeListFilters.AttributeTransportMode);

			AddDynamicFilters(filters);

			return filters;
		}

		void AddCountryOrGroupingFilters(ModuleFilterCollection filters)
		{
			countryOrGroupingFilters = new List<ModuleNkFilter>();
			var dataGroupingCodes = collection?.DataGroupingCodes;
			var shouldAddDefaultDataGroupFilter = collection?.ShouldAddDefaultDataGroupFilter ?? false;
			if (shouldAddDefaultDataGroupFilter || dataGroupingCodes.IsNullOrEmpty())
			{
				AddDataGroupFilter(filters);
			}
			else
			{
				var addOrCategory = dataGroupingCodes.Count > 1;
				foreach (var dataGroup in dataGroupingCodes)
				{
					AddDataGroupFilter(filters, dataGroup, addOrCategory);
				}
			}
		}

		void AddDataGroupFilter(ModuleFilterCollection filters, string dataGroup = "", bool addOrCategory = false)
		{
			var country = string.IsNullOrEmpty(dataGroup) ? Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) : dataGroup;
			var description = Constants.ZZRefCusCodeListFilters.CountryOrGrouping + (addOrCategory ? " " + country : "");
			var countryOrGroupingFilter = filters.AddNkFilter(description, GetCountryOrGroupingQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilters.Add(countryOrGroupingFilter);
			countryOrGroupingFilter.DefaultProperty = country;
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusCodeListFilter|CountryOrGrouping", Constants.ZZRefCusCodeListFilters.CountryOrGrouping);
			if (addOrCategory)
			{
				countryOrGroupingFilter.OrCategory = ZArchitecture.Business.FilterOrCategory.Green;
			}
		}

		void AddDynamicFilters(ModuleFilterCollection filters)
		{
			if (collection != null && collection.CodeTypes != null && collection.CodeTypes.Count == 1)
			{
				var existingFilters = filters.Select(x => x.Description).ToHashSet();

				foreach (var attribute in collection.MandatoryAttributeNames)
				{
					var columnCaption = attribute.ZXE_ColumnCaption;
					var filterName = columnCaption;

					if (existingFilters.Contains(filterName))
					{
						filterName = $"{columnCaption} - Attribute";
					}

					var valueDataType = attribute.ZXE_ValueDataType;
					try
					{
						switch (valueDataType.ToUpperInvariant())
						{
							case "":
							case Constants.RefCusCodeListAttributeName.ValueDataTypes.String:
								{
									ModuleTextFilter filter;
									if (attribute.ZXE_ZZK_NKCodeTypeForValueList.IsEmpty)
									{
										filter = filters.AddTextFilter(filterName,
											(comparisonOperator, value) => GetAttributeTextQueryWithOperator(attribute, comparisonOperator, value));
									}
									else
									{
										filter = filters.AddTextFilter(filterName,
											(comparisonOperator, value) => GetAttributeTextQueryWithOperator(attribute, comparisonOperator, value),
											() => GetAttributeList(attribute));
									}

									var maxLength = attribute.ZXE_MaxLengthOrValue;
									if (!maxLength.IsEmpty)
									{
										filter.MaxLength = maxLength.ToZInt();
									}
									break;
								}
							case Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean:
								filters.AddFlagsFilter(filterName,
									new string[] { columnCaption },
									new GetFlagsQuery[] { value => GetAttributeFlagsQuery(attribute, value) });
								break;
							case Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer:
							case Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal:
								{
									var filter = filters.AddNumberRangeFilter(filterName,
										(value1, value2) => GetAttributeNumberRangeQuery(attribute, value1, value2));
									filter.Decimals = valueDataType == Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer ? ZByte.Zero : attribute.ZXE_DecimalPlaces;
									var minValue = attribute.ZXE_MinLengthOrValue;
									if (!minValue.IsEmpty)
									{
										filter.MinValue = new ZDecimal(minValue);
									}

									var maxValue = attribute.ZXE_MaxLengthOrValue;
									if (!maxValue.IsEmpty)
									{
										filter.MaxValue = maxValue;
									}
									break;
								}
							default:
								throw new DeveloperNotificationException("Unrecognized ZXE_ValueDataType: " + valueDataType);
						}
					}
					catch (ArgumentException ex) when (ex.Message.StartsWith(ZArchitecture.Business.ModuleFilterCollection.AFilterAlreadyExistsWithDescriptionMessage))
					{
						var key = $"Duplicate ZXE_ColumnCaption='{columnCaption}'";
						ErrorReporter.ReportOnce(key, $"ZXE_ZZZ_NKDataGrouping='{attribute.ZXE_ZZZ_NKDataGrouping}', ZXE_ZZK_NKCodeType='{attribute.ZXE_ZZK_NKCodeType}'\r\nFilter: " + collection.CompleteFilter.LiteralTextSqlFormatted);
					}
				}
			}
		}

		ZQuery GetAttributeTextQueryWithOperator(RefCusCodeListAttributeName attribute, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attribute.ZXE_Name);
			attributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, comparisonOperator, value);
			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}

		IList GetAttributeList(RefCusCodeListAttributeName attribute)
		{
			return RefCusCodeListTypes.GetCachedList(Factory, attribute.ZXE_ZZZ_NKDataGrouping, attribute.ZXE_ZZK_NKCodeTypeForValueList, ZDateTime.Today);
		}

		ZQuery GetAttributeFlagsQuery(RefCusCodeListAttributeName attribute, ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attribute.ZXE_Name);
			attributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, value ? YesNoList.Codes.Yes : YesNoList.Codes.No);
			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetAttributeNumberRangeQuery(RefCusCodeListAttributeName attribute, INumericZType valueFrom, INumericZType valueTo)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, attribute.ZXE_Name);
			if (valueFrom.IsValid)
			{
				attributeSubQuery.AddFilterAndZSQLParameterCollection(
					string.Format(CultureInfo.InvariantCulture, "ISNUMERIC({0}) = 1 and (case when ISNUMERIC({0}) = 1 then CAST({0} as numeric({2}, {3})) else 0 end) >= {1}",
						ZZRefCusCodeListAttributeCombinedSchema.Constants.ZZE_Value, valueFrom, SqlDecimal.MaxPrecision, attribute.ZXE_DecimalPlaces),
					new ZSqlParameterCollection());
			}
			if (valueTo.IsValid)
			{
				attributeSubQuery.AddFilterAndZSQLParameterCollection(
					string.Format(CultureInfo.InvariantCulture, "ISNUMERIC({0}) = 1 and (case when ISNUMERIC({0}) = 1 then CAST({0} as numeric({2}, {3})) else 0 end) <= {1}",
						ZZRefCusCodeListAttributeCombinedSchema.Constants.ZZE_Value, valueTo, SqlDecimal.MaxPrecision, attribute.ZXE_DecimalPlaces),
					new ZSqlParameterCollection());
			}

			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCountryOrGroupingQuery(ZString nK)
		{
			ZQuery result;
			if (collection?.IncludeParentDataGroupings ?? false)
			{
				result = RefDataGrouping.GetQueryIncludeParentDataGrouping(Factory, ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, nK);
			}
			else
			{
				result = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, nK);
			}
			return result;
		}

		ZQuery GetListTypeQuery(ZString value)
		{
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, value);
		}

		ZQuery GetListTypeDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString description)
		{
			var predicate = comparisonOperator.GetPredicate(description);
			var types = GetTypeList()
				.Cast<CodeDescriptionPair>()
				.Where(pair => predicate(pair.Description))
				.Select(pair => pair.Code)
				.ToArray();
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, types);
		}

		IList GetAttributeNameList()
		{
			var countries = countryOrGroupingFilters != null
				? countryOrGroupingFilters.Where(countryOrGroupingFilter => countryOrGroupingFilter.IsActive).Select(countryOrGroupingFilter => countryOrGroupingFilter.Property).ToArray()
				: Array.Empty<ZString>();

			var codeType = ZString.Empty;
			if (listTypeFilter != null && listTypeFilter.IsActive)
			{
				codeType = listTypeFilter.Property;
			}

			return RefCusCodeListAttributeNameList.GetListForFilter(Factory, codeType, countries, true);
		}
		IList GetTypeList()
		{
			return RefCusCodeTypeList.GetListByCountry(Factory, countryOrGroupingFilters.Select(countryOrGroupingFilter => countryOrGroupingFilter.Property).ToArray(), false, collection?.IncludeParentDataGroupings ?? false);
		}

		ZQuery GetAttributeNameQuery(ZString value) => new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, value);

		ZQuery GetTransportModeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var predicate = comparisonOperator.GetPredicate(value);
			var result = predicate(value);
			var query = new ZQuery();
			query.AddToFilter(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(
				ZZRefCusCodeListCombined.GetTransportModePropertyName(value), ZZRefCusCodeListCombinedSchema.Constants.TableName), result);
			return query;
		}

		ZQuery GetAttributeTransportModeQuery(ZString value)
		{
			var attributeSubQuery = new ZDBOnlyQuery(typeof(ZZRefCusCodeListAttributeCombined));
			attributeSubQuery.AddToFilter(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(
				ZZRefCusCodeListAttributeCombined.GetTransportModePropertyName(value), ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName), true);
			return attributeSubQuery;
		}

		protected ModuleFilterSubGroup AttributeSubGroup
		{
			get { return attributeSubGroup ?? (attributeSubGroup = new AttributeFilterSubGroup()); }
		}
		ModuleFilterSubGroup attributeSubGroup;

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => "";

		#endregion
	}

	class AttributeFilterSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			var attributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeSubQuery.AddToFilter(filter);
			result.AddSubQuery(attributeSubQuery, JoinCondition.And);
			return result;
		}
	}
}
