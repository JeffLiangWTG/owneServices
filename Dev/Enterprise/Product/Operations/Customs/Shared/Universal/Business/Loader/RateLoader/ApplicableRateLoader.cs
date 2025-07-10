using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using GetMultiCriteriaSetRateProcedure = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.GetMultiCriteriaSetRates;
using RateCriteriaTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpRateSelectionCriteria_V2;
using SecondTradeGroupTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpSecondTradeGroup;

namespace Enterprise.Customs.Universal
{
	public class ApplicableRateLoader
	{
		public ApplicableRateLoader(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		public IEnumerable<RateView> LoadRatesForSingleCriteriaSet(RateLoadTariffCriteriaSet rateCriteriaSet)
		{
			return LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { rateCriteriaSet });
		}

		public IEnumerable<RateView> LoadRatesForMultipleCriteriaSets(IEnumerable<RateLoadTariffCriteriaSet> rateCriteriaSets)
		{
			var result = new List<RateView>();
			result.AddRange(LoadRatesFromCachedCriteria(rateCriteriaSets, out List<RateLoadTariffCriteriaSet> nonCachedCriteriaSets));
			result.AddRange(LoadRatesForMultipleNonCachedCriteria(nonCachedCriteriaSets));
			return GetDistinctRates(result);
		}

		public void CacheRatesForMultipleCriteriaSets(IEnumerable<RateLoadTariffCriteriaSet> rateCriteriaSets)
		{
			var nonCachedCriteriaSets = rateCriteriaSets.Where(rcs => !rcs.IsCached());
			LoadRatesForMultipleNonCachedCriteria(nonCachedCriteriaSets);
		}

		/// <summary>
		/// Loads rates from factory cache for each criteria set key.
		/// </summary>
		/// <param name="allRateCriteriaSets">All sets of criteria to load rates from (cached and non-cached)</param>
		/// <param name="nonCachedCriteriaSets">OUTPUT - non-cached sets of criteria</param>
		/// <returns>Rates from cached criteria sets</returns>
		List<RateView> LoadRatesFromCachedCriteria(IEnumerable<RateLoadTariffCriteriaSet> allRateCriteriaSets, out List<RateLoadTariffCriteriaSet> nonCachedCriteriaSets)
		{
			var result = new List<RateView>();
			var nonCachedSets = new List<RateLoadTariffCriteriaSet>();

			foreach (var rateCriteriaSet in allRateCriteriaSets)
			{
				IEnumerable<RateView> cachedRates;

				if (factory.TryGetValueFromCacheOnly(rateCriteriaSet.CacheKey, out cachedRates))
				{
					result.AddRange(cachedRates);
				}
				else
				{
					nonCachedSets.Add(rateCriteriaSet);
				}
			}

			nonCachedCriteriaSets = nonCachedSets;
			return result;
		}

		/// <summary>
		/// Loads rates from stored procedure, passing batches with multiple criteria sets at once.
		/// </summary>
		/// <param name="nonCachedCriteriaSets">Non-cached sets of criteria</param>
		/// <returns>Loaded rates</returns>
		IEnumerable<RateView> LoadRatesForMultipleNonCachedCriteria(IEnumerable<RateLoadTariffCriteriaSet> nonCachedCriteriaSets)
		{
			var distinctCriteriaSets = GetDistinctCriteriaSets(nonCachedCriteriaSets);
			var result = new List<RateView>();

			var skipIndex = 0;
			var criteriaSetBatch = distinctCriteriaSets.Skip(skipIndex).Take(CriteriaSetBatchSize);

			while (criteriaSetBatch.Any())
			{
				result.AddRange(LoadRatesForBatchOfCriteriaSets(criteriaSetBatch));
				skipIndex += CriteriaSetBatchSize;
				criteriaSetBatch = distinctCriteriaSets.Skip(skipIndex).Take(CriteriaSetBatchSize);
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
		IEnumerable<RateView> LoadRatesForBatchOfCriteriaSets(IEnumerable<RateLoadTariffCriteriaSet> criteriaSetBatch)
		{
			var criteriaSetParameterTable = GetEmptyRateCriteriaTable();
			var secondTradeGroupsDataTable = GetEmptySecondTradeGroupsParameterTable();

			foreach (var rateCriteriaSet in criteriaSetBatch)
			{
				rateCriteriaSet.AddApplicableTableValueParameterRows(criteriaSetParameterTable, secondTradeGroupsDataTable);
			}

			var rateCriteriaIdAndPks = LoadRatePksFromDatabase(criteriaSetParameterTable, secondTradeGroupsDataTable);
			return LoadRatesAndCacheThemByCriteriaSetKey(criteriaSetBatch, rateCriteriaIdAndPks);
		}

		IEnumerable<(Guid CriteriaId, Guid RatePk)> LoadRatePksFromDatabase(DataTable criteriaDataTable, DataTable secondTradeGroupsDataTable)
		{
			var rateCriteriaIdAndPks = new List<(Guid CriteriaId, Guid RatePk)>();
			var sql = FormattableString.Invariant($"EXEC {GetMultiCriteriaSetRateProcedure.QualifiedName} {GetMultiCriteriaSetRateProcedure.Parameters.RateCriteriaTvp}, {GetMultiCriteriaSetRateProcedure.Parameters.SecondTradeGroupTvp}");

			using (var command = ((IDbConnected)factory).Connection.Command(sql))
			{
				command.AddTableValuedParameter(GetMultiCriteriaSetRateProcedure.Parameters.RateCriteriaTvp, RateCriteriaTvp.QualifiedName, criteriaDataTable);
				command.AddTableValuedParameter(GetMultiCriteriaSetRateProcedure.Parameters.SecondTradeGroupTvp, SecondTradeGroupTvp.QualifiedName, secondTradeGroupsDataTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var criteriaId = (Guid)reader[GetMultiCriteriaSetRateProcedure.Columns.CriteriaId];
						var ratePk = (Guid)reader[GetMultiCriteriaSetRateProcedure.Columns.RatePk];
						rateCriteriaIdAndPks.Add((criteriaId, ratePk));
					}
				}
			}

			return rateCriteriaIdAndPks;
		}

		RateView[] LoadRatesAndCacheThemByCriteriaSetKey(IEnumerable<RateLoadTariffCriteriaSet> rateLoadCriteriaSets, IEnumerable<(Guid CriteriaId, Guid RatePk)> rateCriteriaIdAndPks)
		{
			var query = new ZQuery(RateViewSchema.PK, rateCriteriaIdAndPks.Select(rcap => rcap.RatePk));
			var loadedRates = factory.Load<RateView>(query);

			foreach (var rateLoadCriteriaSet in rateLoadCriteriaSets)
			{
				// Cache relevant rates using the criteria set cache key
				factory.GetCachedValue(rateLoadCriteriaSet.CacheKey, () =>
				{
					return
					   from rate in loadedRates
					   join criteriaAndRatePk in rateCriteriaIdAndPks on rate.PK equals criteriaAndRatePk.RatePk
					   where criteriaAndRatePk.CriteriaId == rateLoadCriteriaSet.CriteriaId
					   select rate;
				});
			}

			return loadedRates;
		}

		RateLoadTariffCriteriaSet[] GetDistinctCriteriaSets(IEnumerable<RateLoadTariffCriteriaSet> criteriaSets)
		{
			return criteriaSets.Distinct(new RateLoadTariffCriteriaSetCacheKeyBasedEqualityComparer()).ToArray();
		}

		class RateLoadTariffCriteriaSetCacheKeyBasedEqualityComparer : IEqualityComparer<RateLoadTariffCriteriaSet>
		{
			public bool Equals(RateLoadTariffCriteriaSet x, RateLoadTariffCriteriaSet y)
			{
				return x.CacheKey == y.CacheKey;
			}

			public int GetHashCode(RateLoadTariffCriteriaSet obj)
			{
				return obj.CacheKey.GetHashCode();
			}
		}

		RateView[] GetDistinctRates(IEnumerable<RateView> rates)
		{
			return rates.Distinct(new RateViewPkBasedEqualityComparer()).ToArray();
		}

		class RateViewPkBasedEqualityComparer : IEqualityComparer<RateView>
		{
			public bool Equals(RateView x, RateView y)
			{
				return x.PK == y.PK;
			}

			public int GetHashCode(RateView obj)
			{
				return obj.PK.GetHashCode();
			}
		}

		public static DataTable GetEmptyRateCriteriaTable()
		{
			const string tableName = "RateCriteria";

			var dataTable = new DataTable(tableName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(RateCriteriaTvp.Columns.CriteriaId, typeof(Guid));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.TariffPK, typeof(Guid));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.TradeGroupCountry, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.DataGrouping, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.EffectiveDate, typeof(DateTime));

			dataTable.Columns.Add(RateCriteriaTvp.Columns.Preference, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.AdditionalCodesXml, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.OrderNumber, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.RateType, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.RateCode, typeof(string));
			dataTable.Columns.Add(RateCriteriaTvp.Columns.Direction, typeof(int));

			return dataTable;
		}

		public static DataTable GetEmptySecondTradeGroupsParameterTable()
		{
			const string tableName = "SecondTradeGroups";

			var dataTable = new DataTable(tableName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(SecondTradeGroupTvp.Columns.Id, typeof(Guid));
			dataTable.Columns.Add(SecondTradeGroupTvp.Columns.CriteriaId, typeof(Guid));
			dataTable.Columns.Add(SecondTradeGroupTvp.Columns.SecondTradeGroup, typeof(string));

			return dataTable;
		}

		public static void AddNewCriteriaSetRow(DataTable criteriaParameterTable, DataTable secondTradeGroupsDataTable, Guid criteriaId, ZGuid tariffPk, IZZRateSelectionCriteria selectionCriteria)
		{
			var criteriaRow = criteriaParameterTable.NewRow();
			criteriaRow[RateCriteriaTvp.Columns.CriteriaId] = criteriaId;
			criteriaRow[RateCriteriaTvp.Columns.TariffPK] = tariffPk.ToGuid();
			criteriaRow[RateCriteriaTvp.Columns.TradeGroupCountry] = selectionCriteria.TradeGroupCountry;
			criteriaRow[RateCriteriaTvp.Columns.DataGrouping] = selectionCriteria.DataGrouping;
			criteriaRow[RateCriteriaTvp.Columns.Preference] = selectionCriteria.PrimaryPreference;
			criteriaRow[RateCriteriaTvp.Columns.AdditionalCodesXml] = selectionCriteria.XmlAdditionalCodes();
			criteriaRow[RateCriteriaTvp.Columns.OrderNumber] = selectionCriteria.ConcessionOrder;
			criteriaRow[RateCriteriaTvp.Columns.EffectiveDate] = selectionCriteria.ValidEffectiveDate().ToDateTime();
			criteriaRow[RateCriteriaTvp.Columns.RateType] = selectionCriteria.RateType;
			criteriaRow[RateCriteriaTvp.Columns.RateCode] = selectionCriteria.RateCode;
			criteriaRow[RateCriteriaTvp.Columns.Direction] = selectionCriteria.Direction;
			criteriaParameterTable.Rows.Add(criteriaRow);

			var secondTradeGroups = selectionCriteria.SecondTradeGroups;
			if (secondTradeGroups != null)
			{
				foreach (var secondTradeGroup in secondTradeGroups.Where(x => !x.IsEmpty))
				{
					var existDataRows = secondTradeGroupsDataTable.Select(
						$"{SecondTradeGroupTvp.Columns.CriteriaId} = '{criteriaId}' AND {SecondTradeGroupTvp.Columns.SecondTradeGroup} = '{secondTradeGroup}'");
					if (existDataRows.Length == 0)
					{
						var secondTradeGroupsRow = secondTradeGroupsDataTable.NewRow();
						secondTradeGroupsRow[SecondTradeGroupTvp.Columns.Id] = Guid.NewGuid();
						secondTradeGroupsRow[SecondTradeGroupTvp.Columns.CriteriaId] = criteriaId;
						secondTradeGroupsRow[SecondTradeGroupTvp.Columns.SecondTradeGroup] = secondTradeGroup;
						secondTradeGroupsDataTable.Rows.Add(secondTradeGroupsRow);
					}
				}
			}
		}
	}
}
