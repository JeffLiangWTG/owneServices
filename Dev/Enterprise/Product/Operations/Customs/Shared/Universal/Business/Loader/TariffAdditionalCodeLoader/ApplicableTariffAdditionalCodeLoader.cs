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

namespace Enterprise.Customs.Universal
{
	public class ApplicableTariffAdditionalCodeLoader
	{
		public ApplicableTariffAdditionalCodeLoader(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		public IEnumerable<TariffAdditionalCodeView> LoadTariffAdditionalCodesForSingleCriteriaSet(TariffAdditionalCodeLoadTariffCriteriaSet tariffAdditionalCodeCriteriaSet)
		{
			return LoadTariffAdditionalCodesForMultipleCriteriaSets(new TariffAdditionalCodeLoadTariffCriteriaSet[] { tariffAdditionalCodeCriteriaSet });
		}

		public IEnumerable<TariffAdditionalCodeView> LoadTariffAdditionalCodesForMultipleCriteriaSets(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> tariffAdditionalCodeCriteriaSets)
		{
			var result = new List<TariffAdditionalCodeView>();
			result.AddRange(LoadTariffAdditionalCodesFromCachedCriteria(tariffAdditionalCodeCriteriaSets, out List<TariffAdditionalCodeLoadTariffCriteriaSet> nonCachedCriteriaSets));
			result.AddRange(LoadTariffAdditionalCodesForMultipleNonCachedCriteria(nonCachedCriteriaSets));
			return GetDistinctTariffAdditionalCodes(result);
		}

		public void CacheTariffAdditionalCodesForMultipleCriteriaSets(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> tariffAdditionalCodeCriteriaSets)
		{
			var nonCachedCriteriaSets = tariffAdditionalCodeCriteriaSets.Where(rcs => !rcs.IsCached());
			LoadTariffAdditionalCodesForMultipleNonCachedCriteria(nonCachedCriteriaSets);
		}

		List<TariffAdditionalCodeView> LoadTariffAdditionalCodesFromCachedCriteria(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> allTariffAdditionalCodeCriteriaSets, out List<TariffAdditionalCodeLoadTariffCriteriaSet> nonCachedCriteriaSets)
		{
			var result = new List<TariffAdditionalCodeView>();
			var nonCachedSets = new List<TariffAdditionalCodeLoadTariffCriteriaSet>();

			foreach (var tariffAdditionalCodeCriteriaSet in allTariffAdditionalCodeCriteriaSets)
			{
				IEnumerable<TariffAdditionalCodeView> cachedTariffAdditionalCodes;

				if (factory.TryGetValueFromCacheOnly(tariffAdditionalCodeCriteriaSet.CacheKey, out cachedTariffAdditionalCodes))
				{
					result.AddRange(cachedTariffAdditionalCodes);
				}
				else
				{
					nonCachedSets.Add(tariffAdditionalCodeCriteriaSet);
				}
			}

			nonCachedCriteriaSets = nonCachedSets;
			return result;
		}

		IEnumerable<TariffAdditionalCodeView> LoadTariffAdditionalCodesForMultipleNonCachedCriteria(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> nonCachedCriteriaSets)
		{
			var distinctCriteriaSets = GetDistinctCriteriaSets(nonCachedCriteriaSets);
			var result = new List<TariffAdditionalCodeView>();

			var skipIndex = 0;
			var criteriaSetBatch = distinctCriteriaSets.Skip(skipIndex).Take(CriteriaSetBatchSize);

			while (criteriaSetBatch.Any())
			{
				result.AddRange(LoadTariffAdditionalCodesForBatchOfCriteriaSets(criteriaSetBatch));
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

		IEnumerable<TariffAdditionalCodeView> LoadTariffAdditionalCodesForBatchOfCriteriaSets(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> criteriaSetBatch)
		{
			var criteriaSetParameterTable = GetEmptyCriteriaTable();

			foreach (var criteriaSet in criteriaSetBatch)
			{
				criteriaSet.AddApplicableTableValueParameterRows(criteriaSetParameterTable);
			}

			var tariffAdditionalCodeCriteriaIdAndPks = LoadDataPksFromDatabase(criteriaSetParameterTable);
			return LoadTariffAdditionalCodesAndCacheThemByCriteriaSetKey(criteriaSetBatch, tariffAdditionalCodeCriteriaIdAndPks);
		}

		IEnumerable<(Guid CriteriaId, Guid dataPk)> LoadDataPksFromDatabase(DataTable criteriaDataTable)
		{
			var criteriaIdAndTariffAdditionalCodePks = new List<(Guid CriteriaId, Guid tariffAdditionalCodePk)>();
			var sql = FormattableString.Invariant($"EXEC {GetMultiCriteriaSetTariffAdditionalCodesProcedure.QualifiedName} {GetMultiCriteriaSetTariffAdditionalCodesProcedure.Parameters.CriteriaTvp}");

			using (var command = ((IDbConnected)factory).Connection.Command(sql))
			{
				command.AddTableValuedParameter(GetMultiCriteriaSetTariffAdditionalCodesProcedure.Parameters.CriteriaTvp, TvpTariffAdditionalCodeSelectionCriteria.QualifiedName, criteriaDataTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var criteriaId = (Guid)reader[GetMultiCriteriaSetTariffAdditionalCodesProcedure.Columns.CriteriaId];
						var tariffAdditionalCodePk = (Guid)reader[GetMultiCriteriaSetTariffAdditionalCodesProcedure.Columns.DataPk];
						criteriaIdAndTariffAdditionalCodePks.Add((criteriaId, tariffAdditionalCodePk));
					}
				}
			}

			return criteriaIdAndTariffAdditionalCodePks;
		}

		TariffAdditionalCodeView[] LoadTariffAdditionalCodesAndCacheThemByCriteriaSetKey(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> tariffAdditionalCodeLoadCriteriaSets, IEnumerable<(Guid CriteriaId, Guid TariffAdditionalCodePk)> tariffAdditionalCodeCriteriaIdAndPks)
		{
			var query = new ZQuery(TariffAdditionalCodeViewSchema.PK, tariffAdditionalCodeCriteriaIdAndPks.Select(rcap => rcap.TariffAdditionalCodePk));
			var loadedTariffAdditionalCodes = factory.Load<TariffAdditionalCodeView>(query);

			foreach (var tariffAdditionalCodeLoadCriteriaSet in tariffAdditionalCodeLoadCriteriaSets)
			{
				factory.GetCachedValue(tariffAdditionalCodeLoadCriteriaSet.CacheKey, () =>
				{
					return
					   from tariffAdditionalCode in loadedTariffAdditionalCodes
					   join criteriaIdAndTariffAdditionalCodePks in tariffAdditionalCodeCriteriaIdAndPks on tariffAdditionalCode.PK equals criteriaIdAndTariffAdditionalCodePks.TariffAdditionalCodePk
					   where criteriaIdAndTariffAdditionalCodePks.CriteriaId == tariffAdditionalCodeLoadCriteriaSet.CriteriaId
					   select tariffAdditionalCode;
				});
			}

			return loadedTariffAdditionalCodes;
		}

		TariffAdditionalCodeLoadTariffCriteriaSet[] GetDistinctCriteriaSets(IEnumerable<TariffAdditionalCodeLoadTariffCriteriaSet> criteriaSets)
		{
			return criteriaSets.Distinct(new TariffAdditionalCodeLoadTariffCriteriaSetCacheKeyBasedEqualityComparer()).ToArray();
		}

		class TariffAdditionalCodeLoadTariffCriteriaSetCacheKeyBasedEqualityComparer : IEqualityComparer<TariffAdditionalCodeLoadTariffCriteriaSet>
		{
			public bool Equals(TariffAdditionalCodeLoadTariffCriteriaSet x, TariffAdditionalCodeLoadTariffCriteriaSet y)
			{
				return x.CacheKey == y.CacheKey;
			}

			public int GetHashCode(TariffAdditionalCodeLoadTariffCriteriaSet obj)
			{
				return obj.CacheKey.GetHashCode();
			}
		}

		TariffAdditionalCodeView[] GetDistinctTariffAdditionalCodes(IEnumerable<TariffAdditionalCodeView> tariffAdditionalCodes)
		{
			return tariffAdditionalCodes.Distinct(new TariffAdditionalCodeViewPkBasedEqualityComparer()).ToArray();
		}

		class TariffAdditionalCodeViewPkBasedEqualityComparer : IEqualityComparer<TariffAdditionalCodeView>
		{
			public bool Equals(TariffAdditionalCodeView x, TariffAdditionalCodeView y)
			{
				return x.PK == y.PK;
			}

			public int GetHashCode(TariffAdditionalCodeView obj)
			{
				return obj.PK.GetHashCode();
			}
		}

		public static DataTable GetEmptyCriteriaTable()
		{
			const string tableName = "TariffAdditionalCodeCriteriaSets";
			var criteriaTable = new DataTable(tableName);

			criteriaTable.Locale = CultureInfo.InvariantCulture;
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId, typeof(Guid));
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK, typeof(Guid));
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.Category, typeof(string));
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.EffectiveDate, typeof(DateTime));
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry, typeof(string));
			criteriaTable.Columns.Add(TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping, typeof(string));

			return criteriaTable;
		}

		public static void AddNewCriteriaSetRow(DataTable criteriaParameterTable, Guid criteriaId, ZGuid tariffPk, ITariffAdditionalCodeSelectionCriteria selectionCriteria)
		{
			var criteriaRow = criteriaParameterTable.NewRow();
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId] = criteriaId;
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK] = tariffPk.ToGuid();
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.Category] = selectionCriteria.Category;
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.EffectiveDate] = selectionCriteria.ValidEffectiveDate().ToDateTime();
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.TradeGroupCountry] = selectionCriteria.TradeGroupCountry;
			criteriaRow[TvpTariffAdditionalCodeSelectionCriteria.Columns.DataGrouping] = selectionCriteria.DataGrouping;
			criteriaParameterTable.Rows.Add(criteriaRow);
		}
	}
}
