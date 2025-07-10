using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Universal
{
	public abstract class BaseLoader<T>
		where T : EnterpriseBusinessObject, ITariffEffectiveDatesRelatedBusinessObject
	{
		public BaseLoader(BusinessObjectFactory factory)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		protected internal readonly BusinessObjectFactory factory;

		public IEnumerable<T> LoadDataForSingleCriteriaSet(TariffCriteriaSet<T> criteriaSet)
		{
			return LoadDataForMultipleCriteriaSets(new[] { criteriaSet });
		}

		public IEnumerable<T> LoadDataForMultipleCriteriaSets(IEnumerable<TariffCriteriaSet<T>> criteriaSets)
		{
			IEnumerable<T> result;
			if (criteriaSets.Any())
			{
				var list = new List<T>();
				var dictionary = GetCachedDictionary();
				list.AddRange(LoadDataFromCachedCriteria(dictionary, criteriaSets, out List<TariffCriteriaSet<T>> nonCachedCriteriaSets));
				list.AddRange(LoadDataForMultipleNonCachedCriteria(dictionary, nonCachedCriteriaSets));
				result = GetDistinctData(list);
			}
			else
			{
				result = Enumerable.Empty<T>();
			}
			return result;
		}

		public void CacheDataForMultipleCriteriaSets(IEnumerable<TariffCriteriaSet<T>> criteriaSets)
		{
			if (criteriaSets.Any())
			{
				var dictionary = GetCachedDictionary();
				var nonCachedCriteriaSets = criteriaSets.Where(rcs => !dictionary.ContainsKey(rcs.CacheKey));
				LoadDataForMultipleNonCachedCriteria(dictionary, nonCachedCriteriaSets);
			}
		}

		/// <summary>
		/// Loads Data from factory cache for each criteria set key.
		/// </summary>
		/// <param name="allCriteriaSets">All sets of criteria to load Data from (cached and non-cached)</param>
		/// <param name="nonCachedCriteriaSets">OUTPUT - non-cached sets of criteria</param>
		/// <returns>Data from cached criteria sets</returns>
		List<T> LoadDataFromCachedCriteria(Dictionary<string, T[]> dictionary, IEnumerable<TariffCriteriaSet<T>> allCriteriaSets, out List<TariffCriteriaSet<T>> nonCachedCriteriaSets)
		{
			var result = new List<T>();
			var nonCachedSets = new List<TariffCriteriaSet<T>>();
			foreach (var criteriaSet in allCriteriaSets)
			{
				if (dictionary.TryGetValue(criteriaSet.CacheKey, out var cachedData))
				{
					result.AddRange(cachedData);
				}
				else
				{
					nonCachedSets.Add(criteriaSet);
				}
			}

			nonCachedCriteriaSets = nonCachedSets;
			return result;
		}

		protected Dictionary<string, T[]> GetCachedDictionary()
		{
			return factory.GetCachedValue("UniversalCachedData", () => new Dictionary<string, T[]>());
		}

		/// <summary>
		/// Loads Data from stored procedure, passing batches with multiple criteria sets at once.
		/// </summary>
		/// <param name="nonCachedCriteriaSets">Non-cached sets of criteria</param>
		/// <returns>Loaded Data</returns>
		IEnumerable<T> LoadDataForMultipleNonCachedCriteria(Dictionary<string, T[]> dictionary, IEnumerable<TariffCriteriaSet<T>> nonCachedCriteriaSets)
		{
			var result = new List<T>();

			foreach (var criteriaSetBatch in GetDistinctCriteriaSets(nonCachedCriteriaSets))
			{
				result.AddRange(LoadDataForBatchOfCriteriaSets(dictionary, criteriaSetBatch));
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
		int CriteriaSetBatchSize => 100;

#if DEBUG
		protected virtual
#endif
		IEnumerable<T> LoadDataForBatchOfCriteriaSets(Dictionary<string, T[]> dictionary, IEnumerable<TariffCriteriaSet<T>> criteriaSetBatch)
		{
			var criteriaSetParameterTable = GetEmptyCriteriaTable();
			var additionalCodesParameterTable = GetEmptyAdditionalCodesParameterTable();
			var secondTradeGroupsDataTable = GetEmptySecondTradeGroupsParameterTable();

			foreach (var criteriaSet in criteriaSetBatch)
			{
				criteriaSet.AddApplicableTableValueParameterRows(criteriaSetParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable);
			}

			var conditionCriteriaIdAndPks = LoadDataPksFromDatabase(criteriaSetParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable);
			return LoadDataAndCacheThemByCriteriaSetKey(dictionary, criteriaSetBatch, conditionCriteriaIdAndPks);
		}

		protected abstract IEnumerable<(Guid CriteriaId, Guid dataPk)> LoadDataPksFromDatabase(DataTable criteriaDataTable, DataTable additionalCodesDataTable, DataTable secondTradeGroupsDataTable);

		T[] LoadDataAndCacheThemByCriteriaSetKey(Dictionary<string, T[]> dictionary, IEnumerable<TariffCriteriaSet<T>> criteriaSets, IEnumerable<(Guid CriteriaId, Guid dataPk)> criteriaIdAndDataPks)
		{
			var query = new ZQuery(DataPkSchemaColumn, criteriaIdAndDataPks.Select(rcap => rcap.dataPk));
			var loadedData = factory.Load<T>(query);

			foreach (var criteriaSet in criteriaSets)
			{
				// Cache relevant Data using the criteria set cache key
				var key = criteriaSet.CacheKey;
				if (!dictionary.ContainsKey(key))
				{
					var criteriaAndDatas = from data in loadedData
										   join criteriaAndDataPk in criteriaIdAndDataPks on data.PK equals criteriaAndDataPk.dataPk
										   where criteriaAndDataPk.CriteriaId == criteriaSet.CriteriaId
										   select data;
					dictionary.Add(key, criteriaAndDatas.ToArray());
				}
			}

			return loadedData;
		}

		protected abstract SchemaColumn DataPkSchemaColumn { get; }

		TariffCriteriaSet<T>[][] GetDistinctCriteriaSets(IEnumerable<TariffCriteriaSet<T>> criteriaSets)
		{
			return criteriaSets.Distinct(new TariffCriteriaSetCacheKeyBasedEqualityComparer()).Chunk(CriteriaSetBatchSize).Select(x => x.ToArray()).ToArray();
		}

		class TariffCriteriaSetCacheKeyBasedEqualityComparer : IEqualityComparer<TariffCriteriaSet<T>>
		{
			public bool Equals(TariffCriteriaSet<T> x, TariffCriteriaSet<T> y)
			{
				return x.CacheKey == y.CacheKey;
			}

			public int GetHashCode(TariffCriteriaSet<T> obj)
			{
				return obj.CacheKey.GetHashCode();
			}
		}

		T[] GetDistinctData(IEnumerable<T> data)
		{
			return data.Distinct(new DataPkBasedEqualityComparer()).ToArray();
		}

		class DataPkBasedEqualityComparer : IEqualityComparer<T>
		{
			public bool Equals(T x, T y)
			{
				return x.PK == y.PK;
			}

			public int GetHashCode(T obj)
			{
				return obj.PK.GetHashCode();
			}
		}

		public virtual DataTable GetEmptyCriteriaTable()
		{
			const string tableName = "CriteriaSets";

			var criteriaTable = new DataTable(tableName);
			criteriaTable.Locale = CultureInfo.InvariantCulture;

			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.CriteriaId, typeof(Guid));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.TariffPK, typeof(Guid));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.TradeGroupCountry, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.DataGrouping, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.EffectiveDate, typeof(DateTime));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.Preference, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.OrderNumber, typeof(string));

			return criteriaTable;
		}

		public static DataTable GetEmptyAdditionalCodesParameterTable()
		{
			const string tableName = "AdditionalCodes";

			var dataTable = new DataTable(tableName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(TvpAdditionalCodes.Columns.Id, typeof(Guid));
			dataTable.Columns.Add(TvpAdditionalCodes.Columns.CriteriaId, typeof(Guid));
			dataTable.Columns.Add(TvpAdditionalCodes.Columns.AdditionalCode, typeof(string));

			return dataTable;
		}

		public static DataTable GetEmptySecondTradeGroupsParameterTable()
		{
			const string tableName = "SecondTradeGroups";

			var dataTable = new DataTable(tableName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(TvpSecondTradeGroup.Columns.Id, typeof(Guid));
			dataTable.Columns.Add(TvpSecondTradeGroup.Columns.CriteriaId, typeof(Guid));
			dataTable.Columns.Add(TvpSecondTradeGroup.Columns.SecondTradeGroup, typeof(string));

			return dataTable;
		}
	}
}
