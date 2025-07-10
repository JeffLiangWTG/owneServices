using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public static class WorkflowCustomFieldsFilter
	{
		public const string WorkflowCustomFieldDescriptionDuplicateSuffix = "(WF)";

		static readonly Overridable<MRUCache<string, CCDStruct[]>> CCDCache = new Overridable<MRUCache<string, CCDStruct[]>>();

		public static void ClearCache()
		{
			CCDCache.ResetValue();
		}

		internal struct CCDStruct
		{
			[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
			public CCDStruct(GenCustomColumnDefinition gCCD)
			{
				this.XC_Name = gCCD.XC_Name;
				this.XC_Type = gCCD.XC_Type;
				this.XC_NameMultilingual = gCCD.XC_NameMultilingual;
			}

			public ZString XC_Name;
			public ZString XC_Type;
			public MultilingualString XC_NameMultilingual;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		public static void AddWorkflowCustomFieldsFilters(this ModuleFilterCollection moduleFilterCollection, BusinessObjectFactory factory, string workflowType, Type bizoType)
		{
			CCDStruct[] cols;

			lock (CCDCache)
			{
				var cacheInstance = CCDCache.Value;

				if (cacheInstance is null)
				{
					CCDCache.Value = cacheInstance = new MRUCache<string, CCDStruct[]>(20);
				}

				if (!cacheInstance.TryGetValue(workflowType, out cols))
				{
					var queryTemplates = new ZDBOnlySubQuery(typeof(ProcessTaskTemplate), ProcessTaskTemplateSchema.PK);
					queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, workflowType);
					queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);

					var queryDefinitions = new ZDBOnlyQuery(typeof(GenCustomColumnDefinition));
					queryDefinitions.AddSubQuery(GenCustomColumnDefinitionSchema.XC_ParentID, queryTemplates, JoinCondition.And);

					var bizOs = factory.Load<GenCustomColumnDefinition>(queryDefinitions);
					cols = bizOs.Select(x => new CCDStruct(x)).ToArray();
					cacheInstance.SetValue(workflowType, cols);
				}
			}

			var booleanColumns = new List<CCDStruct>();

			foreach (var group in cols.GroupBy(d => (string)d.XC_Name, StringComparer.OrdinalIgnoreCase)
#if NET
				.Select(g => CargoWise.Common.IEnumerableExtensions.DistinctBy(g, (d => d.XC_Type)).ToList()))
#else
				.Select(g => g.DistinctBy(d => d.XC_Type).ToList()))
#endif
			{
				var addTypeToName = group.Count > 1;
				ModuleFilter lastFilter = null;
				foreach (var columnDefinition in group)
				{
					try
					{
						switch (columnDefinition.XC_Type)
						{
							case AddOnColumnDataType.Codes.Integer:
								lastFilter = AddNumberFilter(moduleFilterCollection, columnDefinition, bizoType, addTypeToName);
								break;
							case AddOnColumnDataType.Codes.Decimal:
								lastFilter = AddNumberFilter(moduleFilterCollection, columnDefinition, bizoType, addTypeToName);
								break;
							case AddOnColumnDataType.Codes.Datetime:
								lastFilter = AddDatetimeFilter(moduleFilterCollection, columnDefinition, bizoType, addTypeToName);
								break;
							case AddOnColumnDataType.Codes.Boolean:
								booleanColumns.Add(columnDefinition);
								break;
							case AddOnColumnDataType.Codes.ComboBox:
								AddComboBoxFilter(moduleFilterCollection, columnDefinition, bizoType);
								break;
							default:
								lastFilter = AddTextFilter(moduleFilterCollection, columnDefinition, bizoType, addTypeToName);
								break;
						}
					}
					catch (ArgumentException ex)
					{
						ErrorReporter.ReportOnce($"Unable to create Custom Fields Filter due to invalid values.", ex);
					}
				}

				if (addTypeToName && lastFilter != null)
				{
					moduleFilterCollection.AliasFilters[group[0].XC_Name] = lastFilter;
				}
			}

			AddBooleanFilter(moduleFilterCollection, booleanColumns, bizoType);
		}

		public static FilterCategory Category => FilterCategories.GetOrCreateFilterCategory(FilterStripBusinessObject.CustomFieldCategoryDescription);

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static void AddComboBoxFilter(ModuleFilterCollection moduleFilterCollection, CCDStruct columnDefinition, Type bizoType)
		{
			AddTextFilter(moduleFilterCollection, columnDefinition, bizoType, false,
				" " + Res.GetString("396b9369-dd2f-44c7-9d53-02351c5af18a", "Code"), AddOnColumnDataType.PartIdentifier + "1", AddOnColumnDataType.Codes.String);
			AddTextFilter(moduleFilterCollection, columnDefinition, bizoType, false,
				" " + Res.GetString("f0e64f58-1e2c-473e-b18e-42b6160e8f14", "Description"), AddOnColumnDataType.PartIdentifier + "2", AddOnColumnDataType.Codes.String);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static ModuleFilter AddTextFilter(ModuleFilterCollection moduleFilterCollection, CCDStruct columnDefinition, Type bizoType, bool includeType, string extraName = "", string extraColumn = "", string typeOverride = "")
		{
			ModuleFilter filter = moduleFilterCollection.AddTextFilter(
				GetUniqueDescription(moduleFilterCollection, columnDefinition, includeType) + extraName,
				(comparisonOperator, value) =>
				{
					var queryValues = comparisonOperator.GetSubQueryForRelatedTextColumn(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, GenCustomAddOnValueSchema.XV_Data, value);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, columnDefinition.XC_Name + extraColumn);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, !string.IsNullOrEmpty(typeOverride) ? typeOverride : (string)columnDefinition.XC_Type);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));

					var queryResult = new ZDBOnlyQuery(bizoType);
					queryResult.AddSubQuery(queryValues, JoinCondition.And);

					return queryResult;
				});
			filter.Category = Category;
			filter.MaxLength = GenCustomAddOnValueSchema.XV_Data.MaxLength;
			return filter;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static ModuleFilter AddNumberFilter(ModuleFilterCollection moduleFilterCollection, CCDStruct columnDefinition, Type bizoType, bool includeType)
		{
			var isDecimal = columnDefinition.XC_Type == AddOnColumnDataType.Codes.Decimal;
			var sqlDataType = isDecimal
				? $"decimal({SchemaDecimalColumn.DefaultPrecision}, {DynamicBusinessObjectProperty.DefaultDecimalScale})"
				: (NoResString)"numeric";

			var filter = moduleFilterCollection.AddNumberRangeFilter(
				GetUniqueDescription(moduleFilterCollection, columnDefinition, includeType),
				(valueFrom, valueTo) =>
				{
					var queryResult = new ZDBOnlyQuery(bizoType);

					if (!valueTo.IsDefault || !valueFrom.IsDefault)
					{
						var queryValues = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID);
						queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, columnDefinition.XC_Name);
						queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, columnDefinition.XC_Type);
						queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));

						void AddFilterToQueryValues(string sql, params object[] args)
						{
							queryValues.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, sql, args), new ZSqlParameterCollection());
						}

						if (valueTo.Equals(valueFrom))
						{
							AddFilterToQueryValues("TRY_CAST({0} as {1}) = {2}", GenCustomAddOnValueSchema.XV_Data.Name, sqlDataType, valueFrom);
						}
						else
						{
							AddFilterToQueryValues("TRY_CAST({0} as {1}) >= {2}", GenCustomAddOnValueSchema.XV_Data.Name, sqlDataType, valueFrom);
							AddFilterToQueryValues("TRY_CAST({0} as {1}) <= {2}", GenCustomAddOnValueSchema.XV_Data.Name, sqlDataType, valueTo);
						}

						queryResult.AddSubQuery(queryValues, JoinCondition.And);
					}

					if ((ZDecimal)valueFrom <= 0 && (ZDecimal)valueTo >= 0)
					{
						// Custom fields are not saved for default values so we do a NOT IN filter to find jobs with the default value (0 for ints and decimals)
						var defaultQuery = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, notIn: true);
						defaultQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Name, columnDefinition.XC_Name);
						defaultQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Type, columnDefinition.XC_Type);
						defaultQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));
						queryResult.AddSubQuery(defaultQuery, JoinCondition.Or);
					}

					return queryResult;
				}, SchemaDecimalColumn.DefaultPrecision, (byte)(isDecimal ? DynamicBusinessObjectProperty.DefaultDecimalScale : 0));
			filter.Category = Category;
			return filter;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static ModuleFilter AddDatetimeFilter(ModuleFilterCollection moduleFilterCollection, CCDStruct columnDefinition, Type bizoType, bool includeType)
		{
			ModuleFilter filter = moduleFilterCollection.AddDateFilter(
				GetUniqueDescription(moduleFilterCollection, columnDefinition, includeType),
				(GetDateQuery)((comparisonOperator, dateFrom, dateTo) =>
				{
					ZDBOnlySubQuery queryValues;
					if (comparisonOperator == DateComparisonOperator.HasDateEntered || comparisonOperator == DateComparisonOperator.HasNoDateEntered)
					{
						queryValues = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, comparisonOperator == DateComparisonOperator.HasNoDateEntered);
						queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Data, SQLComparisonOperator.NotEqual, ZString.Empty);
					}
					else
					{
						queryValues = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID);
						if (dateFrom.IsValid)
						{
							queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Data, SQLComparisonOperator.GreaterThanOrEqualTo, dateFrom.SqlFormat);
						}

						if (dateTo.IsValid)
						{
							queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Data, SQLComparisonOperator.LessThanOrEqualTo, dateTo.SqlFormat);
						}
					}
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, columnDefinition.XC_Name);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, columnDefinition.XC_Type);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));

					ZDBOnlyQuery queryResult = new ZDBOnlyQuery(bizoType);
					queryResult.AddSubQuery(queryValues, JoinCondition.And);

					return queryResult;
				}));
			filter.Category = Category;
			return filter;
		}

		static void AddLegacyBooleanFilter(ModuleFilterCollection moduleFilterCollection, List<CCDStruct> booleanColumns, Type bizoType)
		{
			if (booleanColumns.Count > ModuleFlagsFilter.MaximumFlagsCount)
			{
				for (int from = 0, filterCounter = 1; from < booleanColumns.Count; from += ModuleFlagsFilter.MaximumFlagsCount, filterCounter++)
				{
					int to = from + ModuleFlagsFilter.MaximumFlagsCount - 1;
					if (to >= booleanColumns.Count)
					{
						to = booleanColumns.Count - 1;
					}
					if (from <= to)
					{
						AddLegacyBooleanFilter(moduleFilterCollection, booleanColumns, bizoType, from, to, filterCounter);
					}
				}
			}
			else if (booleanColumns.Count > 0)
			{
				AddLegacyBooleanFilter(moduleFilterCollection, booleanColumns, bizoType, 0, booleanColumns.Count - 1, 0);
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static void AddLegacyBooleanFilter(ModuleFilterCollection moduleFilterCollection, List<CCDStruct> booleanColumns, Type bizoType, int from, int to, int filterCounter)
		{
			string[] names = new string[to - from + 1];
			GetFlagsQuery[] delegates = new GetFlagsQuery[to - from + 1];

			for (int i = from; i <= to; i++)
			{
				names[i - from] = booleanColumns[i].XC_Name;
				delegates[i - from] = GetFlagsQueryMethod(booleanColumns[i], bizoType);
			}

			ModuleFilter filter = moduleFilterCollection.AddFlagsFilter("Workflow Flags" + (filterCounter > 0 ? " " + filterCounter.ToString(CultureInfo.CurrentCulture) : ""), names, delegates);
			filter.Category = Category;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static GetFlagsQuery GetFlagsQueryMethod(CCDStruct columnDefinition, Type bizoType)
		{
			return
				value =>
				{
					ZDBOnlySubQuery queryValues = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, !value);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, columnDefinition.XC_Name);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, columnDefinition.XC_Type);
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Data, SQLComparisonOperator.Equal, "Y");
					queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));

					ZDBOnlyQuery queryResult = new ZDBOnlyQuery(bizoType);
					queryResult.AddSubQuery(queryValues, JoinCondition.And);

					return queryResult;
				};
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static void AddBooleanFilter(ModuleFilterCollection moduleFilterCollection, List<CCDStruct> booleanColumns, Type bizoType)
		{
			if (!booleanColumns.Any())
			{
				return;
			}

			booleanColumns.Sort((a, b) => string.Compare(a.XC_Name, b.XC_Name, true));

			AddLegacyBooleanFilter(moduleFilterCollection, booleanColumns, bizoType);

			var descriptions = new CodeDescriptionPairList();
			var trueStr = ResString.GetMultilingualString("49cfe135-de8c-4d52-8495-b1fadf68389a", "True");
			descriptions.AddPair(trueStr, trueStr);
			var falseStr = ResString.GetMultilingualString("cbbf5925-bf9e-4a89-85ff-b4b010b64207", "False");
			descriptions.AddPair(falseStr, falseStr);

			foreach (var flag in booleanColumns)
			{
				ZString filterDescription = flag.XC_NameMultilingual;
				if (moduleFilterCollection[filterDescription] != null)
				{
					var match = new Regex("^" + filterDescription + "(| \\(\\d+\\))$");
					var count = moduleFilterCollection.Count(x => match.IsMatch(x.Description));
					if (count > 0)
					{
						filterDescription = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", filterDescription, count);
					}
					else
					{
						filterDescription = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", filterDescription, flag.XC_Type);
					}
				}
				var filter = moduleFilterCollection.AddTextFilter(filterDescription, GetFlagsWithTextQueryMethod(bizoType, flag.XC_Name), descriptions);
				filter.ShowDescription = false;
				filter.Category = Category;
			}
		}

		static GetTextQuery GetFlagsWithTextQueryMethod(Type bizoType, ZString column)
		{
			return value =>
			{
				var parsed = ZBool.ParseSafe(value, ZBool.True);
				ZDBOnlySubQuery queryValues = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, !parsed);
				queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Name, column);
				queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Type, AddOnColumnDataType.Codes.Boolean);
				queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_Data, SQLComparisonOperator.Equal, "Y");
				queryValues.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, BusinessObjectFactory.GetTableCodeFromType(bizoType));

				ZDBOnlyQuery queryResult = new ZDBOnlyQuery(bizoType);
				queryResult.AddSubQuery(queryValues, JoinCondition.And);

				return queryResult;
			};
		}

		static ZString GetUniqueDescription(ModuleFilterCollection moduleFilterCollection, CCDStruct column, bool includeType)
		{
			var result = includeType ? FormattableString.Invariant($"{column.XC_NameMultilingual} ({column.XC_Type})") : column.XC_NameMultilingual;
			if (moduleFilterCollection[result] != null)
			{
				result += " " + WorkflowCustomFieldDescriptionDuplicateSuffix;
			}
			return result;
		}
	}
}
