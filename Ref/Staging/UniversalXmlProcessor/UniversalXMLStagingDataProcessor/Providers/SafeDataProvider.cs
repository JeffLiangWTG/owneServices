using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Microsoft.OData.Edm;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor;

public class SafeDataProvider : ISafeDataProvider
{
	public SafeDataProvider(ISafeRepository safeRepo, ICacheProvider cacheProvider, IOverlappingCalculator overlappingCalculator)
	{
		Argument.NotNull(safeRepo, nameof(safeRepo));
		Argument.NotNull(cacheProvider, nameof(cacheProvider));

		this.safeRepo = safeRepo;
		this.cacheProvider = cacheProvider;
		this.overlappingCalculator = overlappingCalculator;
	}

	readonly ISafeRepository safeRepo;
	readonly ICacheProvider cacheProvider;
	readonly IOverlappingCalculator overlappingCalculator;

	public IEnumerable<Tuple<Type, string, string[]>> GetRelatedTypeAndNKPropertyNames(string entityName, IMetadataProvider metadataProvider)
	{
		Argument.NotNullOrEmpty(entityName, nameof(entityName));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		var value = cacheProvider.RelatedTypeAndNKPropertyNamesCache.GetOrAdd(entityName, key => new Lazy<Tuple<Type, string, string[]>[]>(() => GetRelatedTypeAndNKPropertyNames(metadataProvider.GetProperties(entityName)).ToArray()));
		return value?.Value;
	}

	IEnumerable<Tuple<Type, string, string[]>> GetRelatedTypeAndNKPropertyNames(string[] propertyNames)
	{
		Argument.NotNull(propertyNames, nameof(propertyNames));

		var dictionary = new Dictionary<string, Tuple<Type, List<string>>>();
		foreach (var propertyName in propertyNames)
		{
			var splits = propertyName.Split('_');
			if (splits.Length > 2 && splits[splits.Length - 1].StartsWith("NK", StringComparison.OrdinalIgnoreCase))
			{
				var relatedType = GetTypeFromTblPrefix(splits[1]);
				if (relatedType != null)
				{
					var fkPropertyName = GetFKMapping(splits[0], splits[1], propertyName);
					if (!string.IsNullOrEmpty(fkPropertyName))
					{
						if (!dictionary.ContainsKey(fkPropertyName))
						{
							dictionary.Add(fkPropertyName, Tuple.Create(relatedType, new List<string>()));
						}
						dictionary[fkPropertyName].Item2.Add(propertyName);
					}

					if (SharedNKMappingStagingToSafe.ContainsKey(propertyName))
					{
						var sharedFKPropertyName = SharedNKMappingStagingToSafe[propertyName];
						if (!dictionary.ContainsKey(sharedFKPropertyName))
						{
							dictionary.Add(sharedFKPropertyName, Tuple.Create(relatedType, new List<string>()));
						}
						dictionary[sharedFKPropertyName].Item2.Add(propertyName);
					}
				}
			}
		}
		return dictionary.Select(x => Tuple.Create(x.Value.Item1, x.Key, x.Value.Item2.ToArray()));
	}

	KeyProperty[] GetSafeKeys<T>(IMetadataProvider metadataProvider)
	{
		var typeName = typeof(T).Name;
		var value = cacheProvider.SafeKeysCache.GetOrAdd(typeName, key => new Lazy<KeyProperty[]>(() => GetSafeProperties(metadataProvider.GetKeys(typeName))));
		return value?.Value;
	}

	static KeyProperty[] BuildProperties(IEnumerable<string> properties)
	{
		Argument.NotNull(properties, nameof(properties));

		var result = new List<KeyProperty>();
		foreach (var prop in properties)
		{
			result.Add(new KeyProperty { Name = prop });
		}
		return result.ToArray();
	}

	KeyProperty[] GetSafeProperties(KeyProperty[] properties)
	{
		Argument.NotNull(properties, nameof(properties));

		var onlyNames = properties.Select(o => o.Name).ToArray();
		var relatedEntitiyAndNKPropertyNames = GetRelatedTypeAndNKPropertyNames(onlyNames).ToArray();

		return properties.Where(x => !BuildProperties(relatedEntitiyAndNKPropertyNames.SelectMany(r => r.Item3)).Any(o => o.Name == x.Name))
			.Concat(BuildProperties(relatedEntitiyAndNKPropertyNames.Select(x => x.Item2))).ToArray();
	}

	string[] GetSafeProperties(string[] propertyNames)
	{
		Argument.NotNull(propertyNames, nameof(propertyNames));

		var relatedEntitiyAndNKPropertyNames = GetRelatedTypeAndNKPropertyNames(propertyNames).ToArray();
		return propertyNames.Except(relatedEntitiyAndNKPropertyNames.SelectMany(x => x.Item3))
			.Concat(relatedEntitiyAndNKPropertyNames.Select(x => x.Item2))
			.ToArray();
	}

	static bool IsSafeRelatedProperty(Type entityType, string propertyName)
	{
		Argument.NotNull(entityType, nameof(entityType));
		Argument.NotNull(propertyName, nameof(propertyName));
		var tblPrefix = entityType.GetTablePrefix();
		return !propertyName.StartsWith(tblPrefix, StringComparison.OrdinalIgnoreCase);
	}

	static string[] GetSafeRelatedType(Type entityType, string[] propertyNames)
	{
		Argument.NotNull(entityType, nameof(entityType));
		Argument.NotNull(propertyNames, nameof(propertyNames));
		return propertyNames.Where(x => IsSafeRelatedProperty(entityType, x)).Select(x => x.Split('.')[0]).ToArray();
	}

	static string BuildKey<T>((string PropertyName, object PropertyValue)[] nkPropertyNamesAndValues)
	{
		Argument.NotNull(nkPropertyNamesAndValues, nameof(nkPropertyNamesAndValues));
		var components = new List<string> { typeof(T).Name };
		var tblPrefix = typeof(T).GetTablePrefix();
		foreach (var nkPropertyNameAndValue in nkPropertyNamesAndValues)
		{
			var nkPropertyName = nkPropertyNameAndValue.PropertyName;
			var tblPrefixIdx = nkPropertyName.IndexOf(tblPrefix, StringComparison.Ordinal);
			var nkType = nkPropertyName.Substring(tblPrefixIdx);
			components.Add(nkType + ":" + (nkPropertyNameAndValue.PropertyValue == null ? "NULL" : nkPropertyNameAndValue.PropertyValue.ToString()));
		}
		return string.Join("|", components);
	}

	public T GetRelatedEntity<T>((string PropertyName, object PropertyValue)[] nkPropertyNamesAndValues) where T : class
	{
		Argument.NotNull(nkPropertyNamesAndValues, nameof(nkPropertyNamesAndValues));
		var key = BuildKey<T>(nkPropertyNamesAndValues);
		var value = cacheProvider.RelatedEntityCache.GetOrAdd(key, k => new Lazy<object>(() => GetRelatedEntityCore<T>(nkPropertyNamesAndValues)));
		var result = (T)value?.Value;
		return result;
	}

	IQueryable<T> GetAllRelatedEntities<T>(Dictionary<Type, List<Type>> expandTypes) where T : class
	{
		Argument.NotNull(expandTypes, nameof(expandTypes));

		var key = new StringBuilder(typeof(T).Name);
		if (expandTypes.Count > 0)
		{
			foreach (var expandType in expandTypes.Keys.OrderBy(x => x.Name))
			{
				key.Append("|" + expandType.Name);
			}
		}
		var value = cacheProvider.RelatedEntities.GetOrAdd(key.ToString(), k => new Lazy<object[]>(() =>
		{
			var query = safeRepo.Get<T>();
			foreach (var expandType in expandTypes.Keys)
			{
				var expandPath = expandType.Name;
				var relatedTypes = expandTypes[expandType];
				if (relatedTypes.Any())
				{
					var expandRelatedTypes = string.Join(",", relatedTypes.Select(x => x.Name).Distinct());
					expandPath = $"{expandPath}($expand={expandRelatedTypes})";
				}
				query = query.Expand(expandPath);
			}
			return query.ExecuteAsync().Result.ToArray();
		}));
		var result = value?.Value?.Cast<T>().AsQueryable();
		return result;
	}

	T GetRelatedEntityCore<T>((string PropertyName, object PropertyValue)[] nkPropertyNamesAndValues) where T : class
	{
		Argument.NotNull(nkPropertyNamesAndValues, nameof(nkPropertyNamesAndValues));
		var filters = new List<Expression<Func<T, bool>>>();
		var expandTypes = new Dictionary<Type, List<Type>>();
		var allValuesAreEmpty = true;
		foreach (var nkPropertyNameAndValue in nkPropertyNamesAndValues)
		{
			var propertyName = nkPropertyNameAndValue.PropertyName;
			var splits = propertyName.Split('_');
			var value = nkPropertyNameAndValue.PropertyValue;
			if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
			{
				allValuesAreEmpty = false;
				if (splits.Length > 2)
				{
					var relatedProperty = GetRelatedProperty(typeof(T), splits[1], propertyName);
					if (relatedProperty != null)
					{
						var safeValue = value.ConvertToSafeValue(relatedProperty.PropertyType);
						filters.Add(ExpressionHelper.GetPropertyFiltersExpression<T>(relatedProperty, new[] { safeValue }, Operations.Equals));
					}
					else if (splits.Length > 3)
					{
						if (splits.Length > 5 && !splits[splits.Length - 1].EndsWith("NKDataGrouping", StringComparison.OrdinalIgnoreCase))
						{
							throw new RefDataProcessingException($"{propertyName} has more than 4 levels and it's not supported.", ErrorCodes.NotSupportedNKColumn, [propertyName]);
						}

						Type relatedRelatedType = null;
						var relatedType = GetTypeFromTblPrefix(splits[2]);
						var relatedRelatedEntityTypeAndFilterExpression = GetRelatedRelatedEntityTypeAndPropertyFilterExpression<T>(splits[2], propertyName, value);
						if (relatedRelatedEntityTypeAndFilterExpression.Item1 == null && splits.Length > 4)
						{
							relatedRelatedType = GetTypeFromTblPrefix(splits[3]);
							relatedRelatedEntityTypeAndFilterExpression = GetRelatedRelatedEntityTypeAndPropertyFilterExpression<T>(splits[3], propertyName, value, relatedType);
						}
						if (relatedRelatedEntityTypeAndFilterExpression.Item1 != null)
						{
							if (!expandTypes.ContainsKey(relatedType))
							{
								expandTypes[relatedType] = new List<Type>();
							}
							if (relatedRelatedType != null)
							{
								expandTypes[relatedType].Add(relatedRelatedType);
							}
							filters.Add(relatedRelatedEntityTypeAndFilterExpression.Item2);
						}
					}
				}
			}
		}
		if (allValuesAreEmpty)
		{
			return default;
		}
		var query = GetAllRelatedEntities<T>(expandTypes);
		foreach (var filter in filters)
		{
			query = query.Where(filter);
		}
		query = query.Take(2);
		var results = query.ToArray();
		if (results.Length == 1)
		{
			return results[0];
		}

		throw new RefDataProcessingException($@"{nameof(GetRelatedEntity)} returns {(results.Any() ? "multiple matches" : "0 match")} for {typeof(T).Name}.
NkPropertyNamesAndValues: {string.Join("; ", nkPropertyNamesAndValues.Select(x => $"{x.PropertyName} = {x.PropertyValue}"))}."
, ErrorCodes.MultipleOrNoneRelatedEntities, [typeof(T)], nkPropertyNamesAndValues.Select(x => x.PropertyName));
	}

	(Type, Expression<Func<T, bool>>) GetRelatedRelatedEntityTypeAndPropertyFilterExpression<T>(string tablePrefix, string propertyName, object value, Type parentEntityType = null)
	{
		var relatedRelatedEntityType = GetTypeFromTblPrefix(tablePrefix);
		var relatedEntityProperty = GetRelatedProperty(relatedRelatedEntityType, tablePrefix, propertyName);
		if (relatedEntityProperty != null)
		{
			var safeValue = value.ConvertToSafeValue(relatedEntityProperty.PropertyType);
			var filterExpression = parentEntityType == null ? ExpressionHelper.GetRelatedEntityPropertyFilterExpression<T>(relatedRelatedEntityType, relatedEntityProperty, safeValue)
				: ExpressionHelper.GetRelatedRelatedEntityPropertyFilterExpression<T>(parentEntityType, relatedRelatedEntityType, relatedEntityProperty, safeValue);
			return (relatedRelatedEntityType, filterExpression);
		}
		return (null, null);
	}

	static PropertyInfo GetRelatedProperty(Type entityType, string tblPrefix, string nkPropertyName)
	{
		Argument.NotNull(entityType, nameof(entityType));
		Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));
		Argument.NotNullOrEmpty(nkPropertyName, nameof(nkPropertyName));

		if (ExplicitNKMappingStagingToSafe.ContainsKey(nkPropertyName))
		{
			return entityType.GetProperty(ExplicitNKMappingStagingToSafe[nkPropertyName].Item2);
		}
		else
		{
			var idx = nkPropertyName.IndexOf("_" + tblPrefix + "_", StringComparison.Ordinal) + tblPrefix.Length + 2;
			var nkPropertyPart = nkPropertyName.Substring(idx);
			var result = entityType.GetProperty(tblPrefix + "_" + nkPropertyPart);
			if (result == null && nkPropertyPart.StartsWith("NK", StringComparison.OrdinalIgnoreCase))
			{
				nkPropertyPart = nkPropertyPart.Substring(2);
				result = entityType.GetProperty(tblPrefix + "_" + nkPropertyPart);
			}
			return result;
		}
	}

	string GetFKMapping(string tblPrefix, string relatedTblPrefix, string propertyName)
	{
		Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));
		Argument.NotNullOrEmpty(relatedTblPrefix, nameof(relatedTblPrefix));

		if (ExplicitNKMappingStagingToSafe.ContainsKey(propertyName))
		{
			return ExplicitNKMappingStagingToSafe[propertyName].Item1;
		}
		var entityType = GetTypeFromTblPrefix(tblPrefix);
		var fkProperties = entityType?.GetProperties().Where(x => (x.PropertyType == typeof(Guid)
			|| x.PropertyType == typeof(Guid?)) && x.Name.StartsWith(tblPrefix + "_" + relatedTblPrefix, StringComparison.OrdinalIgnoreCase));
		if (fkProperties != null && fkProperties.Count() > 1)
		{
			throw new RefDataProcessingException($"There are multiple FKs between table prefix {tblPrefix} and table prefix {relatedTblPrefix}. Please use explicit mapping in this case."
				, ErrorCodes.MultipleFKsBetweenTablePrefixes, tblPrefix, relatedTblPrefix);
		}
		return fkProperties?.FirstOrDefault()?.Name;
	}

	static readonly Dictionary<string, (string, string)> ExplicitNKMappingStagingToSafe = new() {
			{ "ZZT_ZZA_NKTradeGroup", ("ZZT_ZZA_TradeGroup", "ZZA_TradeGroup") },
			{ "ZZT_ZZA_ZZZ_NKDataGrouping", ("ZZT_ZZA_TradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "ZZT_ZZA_NKSecondTradeGroup", ("ZZT_ZZA_SecondTradeGroup", "ZZA_TradeGroup") },
			{ "ZZT_ZZA_ZZZ_NKSecondDataGrouping", ("ZZT_ZZA_SecondTradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "S01_ZZA_NKTradeGroup", ("S01_ZZA_TradeGroup", "ZZA_TradeGroup") },
			{ "S01_ZZA_ZZZ_NKDataGrouping", ("S01_ZZA_TradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "S01_ZZA_NKSecondTradeGroup", ("S01_ZZA_SecondTradeGroup", "ZZA_TradeGroup") },
			{ "S01_ZZA_ZZZ_NKSecondDataGrouping", ("S01_ZZA_SecondTradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "S07_ZZA_NKTradeGroup", ("S07_ZZA_TradeGroup", "ZZA_TradeGroup") },
			{ "S07_ZZA_ZZZ_NKDataGrouping", ("S07_ZZA_TradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "S07_ZZA_NKSecondTradeGroup", ("S07_ZZA_SecondTradeGroup", "ZZA_TradeGroup") },
			{ "S07_ZZA_ZZZ_NKSecondDataGrouping", ("S07_ZZA_SecondTradeGroup", "ZZA_ZZZ_NKDataGrouping") },

			{ "XQP_XQ2_NKQuestionParent", ("XQP_XQ2_QuestionParent", "XQ2_QuestionCode") },
			{ "XQP_XQ2_NKQuestionStartDateParent", ("XQP_XQ2_QuestionParent", "XQ2_StartDate") },
			{ "XQP_XQ2_ZZZ_NKDataGroupingParent", ("XQP_XQ2_QuestionParent", "XQ2_ZZZ_NKDataGrouping") },
			{ "XQP_XQ2_NKQuestionChild", ("XQP_XQ2_QuestionChild", "XQ2_QuestionCode") },
			{ "XQP_XQ2_NKQuestionStartDateChild", ("XQP_XQ2_QuestionChild", "XQ2_StartDate") },
			{ "XQP_XQ2_ZZZ_NKDataGroupingChild", ("XQP_XQ2_QuestionChild", "XQ2_ZZZ_NKDataGrouping") },
			{ "XQP_XQ2_XXX_NKProfileType", ("XQP_XQ2_QuestionParent", "XXX_ProfileType") },
			{ "XQP_XQ2_XXX_ZZZ_NKDataGrouping", ("XQP_XQ2_QuestionParent", "XXX_ZZZ_NKDataGrouping") },
			{ "XQP_XQ2_XXX_ZZI_NKTariffType", ("XQP_XQ2_QuestionParent", "ZZI_TariffType") },
			{ "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping", ("XQP_XQ2_QuestionParent", "ZZI_ZZZ_NKDataGrouping") },
			{ "ZZ8_ZZA_NKTradeGroup", ("ZZ8_ZZA_TradeGroup", "ZZA_TradeGroup") },
			{ "ZZ8_ZZA_ZZZ_NKDataGrouping", ("ZZ8_ZZA_TradeGroup", "ZZA_ZZZ_NKDataGrouping") },
			{ "ZZ8_ZZA_NKSecondTradeGroup", ("ZZ8_ZZA_SecondTradeGroup", "ZZA_TradeGroup") },
			{ "ZZ8_ZZA_ZZZ_NKSecondDataGrouping", ("ZZ8_ZZA_SecondTradeGroup", "ZZA_ZZZ_NKDataGrouping") },
		};

	static readonly Dictionary<string, string> SharedNKMappingStagingToSafe = new() {
			{ "XQP_XQ2_XXX_NKProfileType", "XQP_XQ2_QuestionChild" },
			{ "XQP_XQ2_XXX_ZZZ_NKDataGrouping", "XQP_XQ2_QuestionChild" },
			{ "XQP_XQ2_XXX_ZZI_NKTariffType", "XQP_XQ2_QuestionChild" },
			{ "XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping", "XQP_XQ2_QuestionChild" }
		};

	public Type GetTypeFromTblPrefix(string tablePrefix)
	{
		Argument.NotNullOrEmpty(tablePrefix, nameof(tablePrefix));

		var type = cacheProvider.TableCodeAndTypeCache.GetOrAdd(tablePrefix, (k => new Lazy<Type>(() => typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x => x.GetProperty(tablePrefix + "_PK") != null))));
		return type.Value;
	}

	public IEnumerable<T> GetData<T>(IStagingDataWrapper[] wrappers, IMetadataProvider metadataProvider) where T : class
	{
		Argument.NotNull(wrappers, nameof(wrappers));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		var result = safeRepo.GetWithOptimizedExpand<T>();
		var path = OdataExpandHelper.Expand(typeof(T), metadataProvider, null);
		if (!string.IsNullOrEmpty(path))
		{
			result = result.Expand(path);
		}
		return GetDataCore<T>(wrappers, metadataProvider, result);
	}

	IEnumerable<T> GetDataCore<T>(IStagingDataWrapper[] wrappers, IMetadataProvider metadataProvider, IQueryable<object> query) where T : class
	{
		Argument.NotNull(wrappers, nameof(wrappers));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(query, nameof(query));
		var result = query.Cast<T>();
		var paramExp = Expression.Parameter(typeof(T));
		Expression allFilterExpression = null;
		var propertyGroups = GetSafeKeys<T>(metadataProvider).GroupBy(x => x.Order).OrderBy(x => x.Key);
		foreach (var propertyGroup in propertyGroups)
		{
			var properties = propertyGroup.ToArray();
			var propertyNames = properties.Select(o => o.Name).ToArray();
			var values = wrappers.Select(w => propertyNames.Select(k => w.GetWrapperValue(k)).ToArray()).ToArray();
			if (properties.Length == 1)
			{
				values = values.Where(a => a[0] != null && !string.IsNullOrEmpty(a[0].ToString())).ToArray();
				if (values.Length == 0)
				{
					continue;
				}
			}

			Expression filterExpression = null;
			for (var i = 0; i < propertyNames.Length; i++)
			{
				var propertyName = propertyNames[i];
				var distinctValues = values.Select(x => x[i]).Distinct().ToList();
				var propertyInfo = typeof(T).GetProperty(propertyName);
				if (propertyInfo == null)
				{
					continue;
				}
				var property = properties.FirstOrDefault(o => o.Name == propertyName);
				var operation = property.Operation;
				var propFilterExp = ExpressionHelper.GetPropertyFiltersExpression<T>(propertyInfo, distinctValues.Select(x => x.ConvertToSafeValue(propertyInfo.PropertyType)).ToArray(), operation, paramExpression: paramExp);
				filterExpression = filterExpression == null ? propFilterExp.Body : Expression.And(filterExpression, propFilterExp.Body);
			}
			if (filterExpression != null)
			{
				allFilterExpression = allFilterExpression == null ? filterExpression : Expression.Or(allFilterExpression, filterExpression);
			}
		}
		if (allFilterExpression == null)
		{
			return [];
		}
		result = result.Where(Expression.Lambda<Func<T, bool>>(allFilterExpression, paramExp));
		return Task.Run(() => result.ExecuteAsync())?.Result;
	}

	public IEnumerable<object> GetRelatedData<T>(T obj, string relatedTypeName)
	{
		Argument.NotNull(obj, nameof(obj));
		Argument.NotNullOrEmpty(relatedTypeName, nameof(relatedTypeName));
		IEnumerable<object> result = null;
		var relatedEntityType = typeof(RefCusTariff).GetTypeFromBaseType(relatedTypeName);
		if (relatedEntityType != null)
		{
			var relatedEntityProperty = GetRelatedDataProperty(obj, relatedEntityType);
			if (relatedEntityProperty != null)
			{
				result = ((IEnumerable)relatedEntityProperty.GetValue(obj))?.Cast<object>();
			}
		}
		return result ?? [];
	}

	static void AddRelatedData<T, TRelated>(T parent, TRelated obj)
	{
		var relatedEntityProperty = GetRelatedDataProperty(parent, typeof(TRelated));
		((ICollection<TRelated>)relatedEntityProperty.GetValue(parent))?.Add(obj);
	}

	static void RemoveRelatedData<T, TRelated>(T parent, TRelated obj)
	{
		var relatedEntityProperty = GetRelatedDataProperty(parent, typeof(TRelated));
		((ICollection<TRelated>)relatedEntityProperty.GetValue(parent))?.Remove(obj);
	}

	static PropertyInfo GetRelatedDataProperty<T>(T _, Type relatedType)
	{
		var relatedEntityPropertyType = typeof(ICollection<>).MakeGenericType(relatedType);
		return typeof(T).GetProperties().FirstOrDefault(x => relatedEntityPropertyType.IsAssignableFrom(x.PropertyType));
	}

	public T GetSpecifiedDateTimeRangeObjectFromList<T>(object[] safeObjs, DateTimeRange dateTimeRange, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider) where T : class
	{
		Argument.NotNull(safeObjs, nameof(safeObjs));
		Argument.NotNull(dateTimeRange, nameof(dateTimeRange));
		var query = safeObjs.Cast<T>().AsQueryable();
		foreach (var safeObj in query)
		{
			var dateRange = GetDateTimeRange(safeObj);
			if (dateRange.Equals(dateTimeRange))
			{
				return safeObj;
			}
		}

		return null;
	}

	public Dictionary<int, T[]> GetNewestObjectFromList<T>(object[] safeObjs, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, bool returnAllObjects = false) where T : class
	{
		return GetNewestObjectFromListCore<T>(safeObjs, wrapper, metadataProvider, GetSafeKeys<T>(metadataProvider), returnAllObjects);
	}

	Dictionary<int, T[]> GetNewestObjectFromListCore<T>(object[] safeObjs, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, KeyProperty[] keyProperties, bool returnAllObjects) where T : class
	{
		var matchResults = new Dictionary<int, T[]>();
		var newestMatches = GetNewestByDirectProperty<T>(wrapper, safeObjs, metadataProvider, keyProperties);
		foreach (var order in newestMatches.Keys)
		{
			var matches = GetByRelatedProperty(wrapper, newestMatches[order], metadataProvider, keyProperties, returnAllObjects);
			if (matches?.Length > 0)
			{
				matchResults[order] = matches.Select(x =>
				{
					x.BuildNonPersistentObjects(safeRepo);
					return x;
				}).ToArray();
			}
		}

		return matchResults;
	}

	Dictionary<int, T[]> GetNewestByDirectProperty<T>(IStagingDataWrapper wrapper, object[] safeObjs, IMetadataProvider metadataProvider) where T : class
	{
		return GetNewestByDirectProperty<T>(wrapper, safeObjs, metadataProvider, GetSafeKeys<T>(metadataProvider));
	}

	static Dictionary<int, T[]> GetNewestByDirectProperty<T>(IStagingDataWrapper wrapper, object[] safeObjs, IMetadataProvider metadataProvider, KeyProperty[] keyProperties) where T : class
	{
		Argument.NotNull(wrapper, nameof(wrapper));
		Argument.NotNull(safeObjs, nameof(safeObjs));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(keyProperties, nameof(keyProperties));

		var query = safeObjs.Cast<T>().AsQueryable();
		var pkExp = ExpressionHelper.GetPKExpression<T>();

		var isExpirableType = DataProviderHelper.IsExpirableType(typeof(T));
		var propertyGroups = keyProperties.GroupBy(x => x.Order).OrderBy(x => x.Key);
		return propertyGroups.Select(g => Tuple.Create(g.Key, query.Where(item => HasMatchByProperty(item, wrapper, metadataProvider, g)).AsQueryable()))
			.Where(t => t.Item2.Any())
			.ToDictionary(t => t.Item1, t => isExpirableType
				? t.Item2.OrderByDescending(x => GetDateValue(x, Constants.StartDatePropertySuffix)).ThenBy(pkExp).ToArray()
				: t.Item2.OrderBy(pkExp).ToArray());
	}

	static bool HasMatchByProperty<T>(T item, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, IGrouping<int, KeyProperty> propertyGroup) where T : class
	{
		var keyPropertyArray = propertyGroup.ToArray();
		var relatedProperties = keyPropertyArray.Where(x => IsSafeRelatedProperty(typeof(T), x.Name));
		var directProperties = keyPropertyArray.Except(relatedProperties);
		if (!metadataProvider.EnableNullOrEmptyKeyMatching(typeof(T).Name) && directProperties.All(x => string.IsNullOrEmpty(wrapper.GetWrapperValue(x.Name)?.ToString())))
		{
			return false;
		}

		var match = true;
		foreach (var directProperty in directProperties)
		{
			var propertyInfo = typeof(T).GetProperty(directProperty.Name);
			if (!CompareHelper.MatchPropertyValues(propertyInfo.PropertyType, propertyInfo.GetValue(item),
				wrapper.GetWrapperValue(directProperty.Name).ConvertToSafeValue(propertyInfo.PropertyType), directProperty.Operation, StringComparison.OrdinalIgnoreCase))
			{
				match = false;
				break;
			}
		}

		return match;
	}

	T[] GetByRelatedProperty<T>(IStagingDataWrapper wrapper, T[] safeObjs, IMetadataProvider metadataProvider)
	{
		return GetByRelatedProperty(wrapper, safeObjs, metadataProvider, GetSafeKeys<T>(metadataProvider), false);
	}

	T[] GetByRelatedProperty<T>(IStagingDataWrapper wrapper, T[] safeObjs, IMetadataProvider metadataProvider, KeyProperty[] keyProperties, bool returnAllObjects)
	{
		Argument.NotNull(wrapper, nameof(wrapper));
		Argument.NotNull(safeObjs, nameof(safeObjs));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(keyProperties, nameof(keyProperties));

		var results = new List<T>();

		var relatedProperties = keyProperties.Where(x => IsSafeRelatedProperty(typeof(T), x.Name));
		var relatedTypeAndProperties = relatedProperties.Select(x =>
		{
			var parts = x.Name.Split('.');
			return Tuple.Create(parts[0], new KeyProperty { Name = string.Join(".", parts.Skip(1)), Operation = x.Operation, ConstantValue = x.ConstantValue });
		}).GroupBy(x => x.Item1, x => x.Item2);

		foreach (var result in safeObjs)
		{
			var matched = true;
			foreach (var relatedTypeAndProperty in relatedTypeAndProperties)
			{
				var relatedType = typeof(RefCusTariff).GetTypeFromBaseType(relatedTypeAndProperty.Key);
				var relatedsafeObjs = GetRelatedData(result, relatedTypeAndProperty.Key).ToArray();
				var isRelatedKeyUnSpecified = relatedTypeAndProperty.Any(x => string.IsNullOrEmpty(x.Name));
				var relatedEntities = wrapper.GetRelatedEntities(relatedTypeAndProperty.Key);
				var keyPropertyFilter = new ConstantValueKeyPropertyFilter(relatedType, relatedTypeAndProperty.ToArray());
				var filteredRelatedEntities = keyPropertyFilter.GetFilteredEntities(relatedEntities);
				var filteredRelatedSafeObjects = keyPropertyFilter.GetFilteredSafeObjects(relatedsafeObjs).ToArray();
				if (!DataProviderHelper.IsExpirableType(relatedType))
				{
					matched = IsCountMatching(filteredRelatedSafeObjects, filteredRelatedEntities);
					if (!matched)
					{
						break;
					}
				}
				foreach (var relatedWrapper in filteredRelatedEntities)
				{
					var newestObjects = isRelatedKeyUnSpecified ? (IDictionary)this.InvokeGenericMethod(nameof(GetNewestObjectFromList), relatedType, filteredRelatedSafeObjects, relatedWrapper, metadataProvider, false)
						: (IDictionary)this.InvokeGenericMethod(nameof(GetNewestObjectFromListCore), relatedType, filteredRelatedSafeObjects, relatedWrapper, metadataProvider, GetSafeProperties(relatedTypeAndProperty.ToArray()), false);
					if (newestObjects == null || newestObjects.Keys.Count == 0)
					{
						matched = false;
						break;
					}
				}
				if (!matched)
				{
					break;
				}
			}
			if (matched)
			{
				results.Add(result);

				if (keyProperties.All(o => o.Operation == Operations.Equals) && !returnAllObjects)
				{
					return results.ToArray();
				}
			}
		}
		if (returnAllObjects)
		{
			if (keyProperties.All(o => o.Operation == Operations.Equals))
			{
				return results.ToArray();
			}
			else
			{
				var keyPropertiesOtherThanEqual = keyProperties.Where(x => x.Operation != Operations.Equals);
				throw new RefDataProcessingException($@"Do not support 'return all' objects for key properties where Operation is other than 'Equals'.
Key properties other than equals: {string.Join("; ", keyPropertiesOtherThanEqual.Select(x => $"{x.Name} {x.Operation} {x.ConstantValue}"))}."
, ErrorCodes.ReturnAllObjectsForOperationOtherThanEqual, keyPropertiesOtherThanEqual.Select(x => x.Name));
			}
		}

		return GetNewestOfEachObjectInTheList(results, metadataProvider);
	}

	static bool IsCountMatching(IEnumerable<object> safeObjects, IEnumerable<IStagingDataWrapper> stagingDataWrappers)
	{
		if (stagingDataWrappers == null)
		{
			if (safeObjects.Any())
			{
				return false;
			}
		}
		else if (safeObjects.Count() != stagingDataWrappers.Count())
		{
			return false;
		}
		return true;
	}

	T[] GetNewestOfEachObjectInTheList<T>(IEnumerable<T> dataList, IMetadataProvider metadataProvider)
	{
		Argument.NotNull(dataList, nameof(dataList));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));

		var safeKeys = GetSafeKeys<T>(metadataProvider);
		var safeKeysNames = safeKeys.Select(x => x.Name).ToArray();
		var equalityComparer = new CompareColumnsEquality<IDictionary<string, object>>();
		var groupedList = dataList.GroupBy(x =>
		{
			var groupByColumns = new System.Dynamic.ExpandoObject();
			((IDictionary<string, object>)groupByColumns).Clear();
			foreach (string column in safeKeysNames)
			{
				((IDictionary<string, object>)groupByColumns).Add(column, GetPropertyValue(x, column));
			}
			return groupByColumns;
		}, equalityComparer);

		return groupedList.Select(x => x.Select(o => o).First()).ToArray();
	}

	static object GetPropertyValue(object obj, string propertyName)
	{
		Argument.NotNull(obj, nameof(obj));
		Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
		return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
	}

	public void RemoveMatchedRelatedObj<T, TRelated>(T safeObj, IStagingDataWrapper relatedWrapper, IMetadataProvider metadataProvider)
	{
		var relatedSafeObjs = GetRelatedData(safeObj, typeof(TRelated).Name);
		var matchedRelatedSafeObjsDic = (IDictionary<int, TRelated[]>)this.InvokeGenericMethod(nameof(GetNewestObjectFromList), typeof(TRelated), relatedSafeObjs.ToArray(), relatedWrapper, metadataProvider, false);
		foreach (var matchedRelatedObjs in matchedRelatedSafeObjsDic)
		{
			foreach (var matchedRelatedObj in matchedRelatedObjs.Value)
			{
				RemoveRelatedData(safeObj, matchedRelatedObj);
			}
		}
	}

	public T GetIdenticalObjectFromList<T>(object[] safeObjs, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider) where T : class
	{
		Argument.NotNull(safeObjs, nameof(safeObjs));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(wrapper, nameof(wrapper));
		var query = safeObjs.Cast<T>().AsQueryable();
		var matches = safeObjs.Cast<T>().Where(x => GetIdenticalLevelCore(x, wrapper, metadataProvider) == (int)IdenticalLevel.Identical);
		var entityTypeName = typeof(T).Name;
		foreach (var match in matches)
		{
			var matched = true;
			foreach (var relatedProperty in GetSafeRelatedType(typeof(T), metadataProvider.GetProperties(entityTypeName)))
			{
				var relatedType = typeof(RefCusTariff).GetTypeFromBaseType(relatedProperty);
				if (!DataProviderHelper.IsExpirableType(relatedType))
				{
					var relatedsafeObjs = GetRelatedData(match, relatedProperty).ToArray();
					var relatedWrappers = wrapper.GetRelatedEntities(relatedProperty);
					matched = IsCountMatching(relatedsafeObjs, relatedWrappers);
					if (!matched)
					{
						break;
					}
					foreach (var relatedWrapper in relatedWrappers)
					{
						if (this.InvokeGenericMethod(nameof(GetIdenticalObjectFromList), relatedType,
							relatedsafeObjs, relatedWrapper, metadataProvider) == null)
						{
							matched = false;
							break;
						}
					}
					if (!matched)
					{
						break;
					}
				}
			}
			if (matched)
			{
				return match;
			}
		}
		return default(T);
	}

	public T Create<T>(IStagingDataWrapper wrapper, object safeParentObj, List<object> safeObjsListToSearch, IMetadataProvider metadataProvider) where T : class
	{
		Argument.NotNull(wrapper, nameof(wrapper));
		var result = typeof(INonPersistentBusinessObject).IsAssignableFrom(typeof(T))
						? typeof(INonPersistentBusinessObjectFlatten).IsAssignableFrom(typeof(T)) ? (T)Activator.CreateInstance(typeof(T), safeRepo) : (T)Activator.CreateInstance(typeof(T), safeParentObj)
						: (T)Activator.CreateInstance(typeof(T));

		var pkColumn = typeof(T).GetPKPropertyInfo();
		pkColumn.SetValue(result, Guid.NewGuid());
		foreach (var property in typeof(T).GetProperties().Where(x => x.Name.StartsWith(typeof(T).GetTablePrefix(), StringComparison.Ordinal) && x != pkColumn))
		{
			if (property.Name.EndsWith(Constants.PublishedDate, StringComparison.OrdinalIgnoreCase))
			{
				property.SetValue(result, PublishDateProvider.PublishedDate.ConvertToSafeValue(property.PropertyType));
			}
			else if (property.Name.EndsWith(Constants.IAmUniqueSuffix, StringComparison.OrdinalIgnoreCase) && metadataProvider.ShouldCalculateIAmUnique(typeof(T).Name, typeof(T).GetTablePrefix()))
			{
				property.SetValue(result, GetIAmUniqueForTariff<T>(wrapper, safeObjsListToSearch, metadataProvider).ConvertToSafeValue(property.PropertyType));
			}
			else
			{
				var value = wrapper.GetWrapperValue(property.Name).ConvertToSafeValue(property.PropertyType);
				property.SetValue(result, value);
			}
		}
		safeRepo.Add(result);
		if (safeParentObj != null)
		{
			GetType().InvokeStaticGenericMethod(nameof(AddRelatedData), new[] { safeParentObj.GetType(), typeof(T) }, safeParentObj, result);
		}
		return result;
	}

	#region IAmUnique for ResCusTariff

	short GetIAmUniqueForTariff<T>(IStagingDataWrapper wrapper, List<object> safeObjsListToSearch, IMetadataProvider metadataProvider) where T : class
	{
		short iAmUnique = 0;
		var matchedTariffs = GetNewestByDirectProperty<T>(wrapper, safeObjsListToSearch.ToArray(), metadataProvider);
		if (matchedTariffs.Keys.Count > 0)
		{
			var tariffs = matchedTariffs.Values.SelectMany(x => x).ToArray();
			var uniqueTariff = GetByRelatedProperty(wrapper, tariffs, metadataProvider);
			iAmUnique = uniqueTariff.Any() ? (uniqueTariff.First() as RefCusTariff).ZZ1_IAMUnique : (short)(tariffs.Cast<RefCusTariff>().Max(x => x.ZZ1_IAMUnique) + 1);
		}
		return iAmUnique;
	}

	#endregion

	public bool ShouldOverWriteAndNotExpire<T>()
	{
		if (NonExpirableTypes.Contains(typeof(T)))
		{
			return true;
		}
		return false;
	}

	public bool IsExpirable<T>(DateTimeRange safeDateTimeRange, DateTimeRange wrapperDateTimeRange)
	{
		if (!DataProviderHelper.IsExpirableType(typeof(T)) || NonExpirableTypes.Contains(typeof(T)))
		{
			return false;
		}
		return wrapperDateTimeRange.StartDate > safeDateTimeRange.StartDate;
	}

	static DateTimeOffset GetDateValue<T>(T safeObj, string suffix)
	{
		Argument.NotNull(safeObj, nameof(safeObj));

		var prefix = typeof(T).GetTablePrefix();
		var propertyName = $"{prefix}_{suffix}";
		var result = safeObj.GetValue(propertyName);
		if (result is DateTimeOffset timeValue)
		{
			return timeValue;
		}
		if (result is Date dateValue)
		{
			return ((DateTime)dateValue).ToUTCDateTimeOffset();
		}
		throw new RefDataProcessingException($"Property {propertyName} in {typeof(T).Name} has to be type DateTimeOffset or Edm.Date and not null.", ErrorCodes.InvalidDatePropertyType, [typeof(T)], [propertyName]);
	}

	public void Expire<T>(T safeObj, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(wrapper, nameof(wrapper));
		var wrapperDateRange = wrapper.GetDateTimeRange();
		var endDateProperty = typeof(T).GetProperties().FirstOrDefault(x => x.Name.EndsWith(Constants.EndDatePropertySuffix, StringComparison.OrdinalIgnoreCase));
		var endDate = overlappingCalculator.GetEndDate(wrapperDateRange.StartDate);
		if (endDateProperty.PropertyType == typeof(DateTimeOffset) || Nullable.GetUnderlyingType(endDateProperty.PropertyType) == typeof(DateTimeOffset))
		{
			endDateProperty.SetValue(safeObj, endDate);
		}
		else if (endDateProperty.PropertyType == typeof(Date) || Nullable.GetUnderlyingType(endDateProperty.PropertyType) == typeof(Date))
		{
			endDateProperty.SetValue(safeObj, (Date)endDate.DateTime);
		}
		else
		{
			throw new RefDataProcessingException($"Property {endDateProperty.Name} in {typeof(T).Name} has to be type DateTimeOffset or Edm.Date", ErrorCodes.InvalidDatePropertyType, [typeof(T)], [endDateProperty.Name]);
		}
		safeRepo.Update(safeObj);
	}

	public async Task<bool> BatchExpire<T>(IEnumerable<Guid> safeObjPKs, DateTime expiredDate) where T : class
	{
		return await safeRepo.BatchExpire<T>(safeObjPKs, expiredDate.ToUTCDateTimeOffset());
	}

	public async Task<bool> BatchInActive<T>(IEnumerable<Guid> safeObjPKs) where T : class
	{
		return await safeRepo.BatchInActive<T>(safeObjPKs);
	}

	public async Task<bool> BatchDelete<T>(IEnumerable<Guid> safeObjPKs) where T : class
	{
		return await safeRepo.BatchDelete<T>(safeObjPKs);
	}

	public int GetIdenticalLevel<T>(T safeObj, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(wrapper, nameof(wrapper));
		return GetIdenticalLevelCore(safeObj, wrapper, metadataProvider);
	}

	int GetIdenticalLevelCore<T>(T safeObj, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		Argument.NotNull(wrapper, nameof(wrapper));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));

		var result = IdenticalLevel.Identical;
		var propertyNames = GetSafeProperties(metadataProvider.GetProperties(typeof(T).Name)).Where(
			x => x.StartsWith(typeof(T).GetTablePrefix(), StringComparison.OrdinalIgnoreCase)).ToArray();

		var keyPropertyNames = GetSafeKeys<T>(metadataProvider).Select(x => x.Name);
		foreach (var propertyName in propertyNames)
		{
			var propertyInfo = typeof(T).GetProperty(propertyName);
			if (propertyInfo != null)
			{
				var stringComparisonType = keyPropertyNames.Contains(propertyName) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
				if (!CompareHelper.MatchPropertyValues(propertyInfo.PropertyType, wrapper.GetWrapperValue(propertyName).ConvertToSafeValue(propertyInfo.PropertyType), propertyInfo.GetValue(safeObj), Operations.Equals, stringComparisonType))
				{
					if ((!DescriptionAsMainContentList.Contains(typeof(T).Name) && propertyName.EndsWith("Description", StringComparison.OrdinalIgnoreCase))
						|| propertyName.EndsWith(Constants.StartDatePropertySuffix, StringComparison.OrdinalIgnoreCase)
						|| propertyName.EndsWith(Constants.EndDatePropertySuffix, StringComparison.OrdinalIgnoreCase))
					{
						result = IdenticalLevel.MainContentIdentical;
						continue;
					}
					return (int)IdenticalLevel.HasChange;
				}
			}
		}
		return (int)result;
	}

	public void Update<T>(T safeObject, IStagingDataWrapper wrapper, IMetadataProvider metadataProvider, IdenticalLevel identicalLevel, int keyOrder = 0)
	{
		Argument.NotNull(safeObject, nameof(safeObject));
		Argument.NotNull(metadataProvider, nameof(metadataProvider));
		Argument.NotNull(wrapper, nameof(wrapper));
		var updatingPropertyNames = GetSafeProperties(metadataProvider.GetUpdatableProperties(typeof(T).Name, keyOrder))
				.Where(x => x.StartsWith(typeof(T).GetTablePrefix(), StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (var propertyName in updatingPropertyNames)
		{
			if (ShouldOverWriteAndNotExpire<T>() || !propertyName.EndsWith(Constants.StartDatePropertySuffix, StringComparison.OrdinalIgnoreCase))
			{
				var propertyInfo = typeof(T).GetProperty(propertyName);
				if (propertyInfo != null)
				{
					var value = wrapper.GetWrapperValue(propertyName);
					propertyInfo.SetValue(safeObject, value.ConvertToSafeValue(propertyInfo.PropertyType));
				}
			}
		}
		if (identicalLevel == IdenticalLevel.HasChange && typeof(T).GetProperties().Where(x => x.Name.EndsWith("PublishedDate", StringComparison.OrdinalIgnoreCase)).Any())
		{
			var propertyInfo = typeof(T).GetProperty(typeof(T).GetTablePrefix() + "_PublishedDate");
			propertyInfo.SetValue(safeObject, PublishDateProvider.PublishedDate.ConvertToSafeValue(propertyInfo.PropertyType));
		}
		safeRepo.Update(safeObject);
	}

	public Task<bool> SaveChangesAsync()
	{
		return safeRepo.SaveChangesAysnc();
	}

	public void Add<T>(T safeObj)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		safeRepo.Add(safeObj);
	}

	public void Delete<T>(T safeObj)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		safeRepo.Delete(safeObj);
	}

	public IEnumerable<Guid> DeleteConflictedRecordsByOrder<T>(int keyOrder, Dictionary<int, IEnumerable<object>> safeObjsDictionary) where T : class
	{
		if (safeObjsDictionary.Keys.Count <= 1)
		{
			return Enumerable.Empty<Guid>();
		}

		var matchedObjectPKs = safeObjsDictionary[keyOrder].Select(x => x.GetPKValue());
		var deletedPKs = safeObjsDictionary.Values.SelectMany(x => x).Select(x => x.GetPKValue()).Except(matchedObjectPKs);
		if (deletedPKs.Any())
		{
			Console.WriteLine("Deleting conflicted records: {0}", string.Join(", ", deletedPKs));
			safeRepo.ForceDelete<T>(deletedPKs);
		}
		return deletedPKs;
	}

	static Type[] NonExpirableTypes
	{
		get
		{
			return new[]
			{
					typeof(RefCusCodeList),
					typeof(RefCusProcedure),
				};
		}
	}

	public bool IsUserOverride<T>(T safeObj)
	{
		Argument.NotNull(safeObj, nameof(safeObj));
		var tblPrefix = typeof(T).GetTablePrefix();
		var userOverrideProperty = typeof(T).GetProperty($"{tblPrefix}_{Constants.UserOverrideSuffix}");
		if (userOverrideProperty != null)
		{
			var result = userOverrideProperty.GetValue(safeObj);
			return (bool)result;
		}
		return false;
	}

	public DateTimeRange GetDateTimeRange<T>(T safeObj)
	{
		return new DateTimeRange(GetDateValue(safeObj, Constants.StartDatePropertySuffix), GetDateValue(safeObj, Constants.EndDatePropertySuffix));
	}

	public async Task<IEnumerable<CloneProcessResult>> CloneExistingRecordChildrenIntoNewRecord<T>(IEnumerable<CloneProcessObject> cloneProcessObjects) where T : class
	{
		Argument.NotNull(cloneProcessObjects, nameof(cloneProcessObjects));
		var cloneProcessResults = await safeRepo.CloneExistingRecordChildrenIntoNewRecord<T>(cloneProcessObjects);
		return cloneProcessResults;
	}

	public void SavePersistentObjects()
	{
		safeRepo.SavePersistentObjects();
	}

	public IEnumerable<object> GetAllPersistentObjects()
	{
		return safeRepo.GetAllPersistentObjects();
	}
}
