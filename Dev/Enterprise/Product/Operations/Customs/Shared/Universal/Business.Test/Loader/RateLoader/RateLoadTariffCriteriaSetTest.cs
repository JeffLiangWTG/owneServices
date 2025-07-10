using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RateCriteriaTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpRateSelectionCriteria_V2;
using SecondTradeGroupTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpSecondTradeGroup;

namespace Enterprise.Customs.Universal.Testing
{
	class RateLoadTariffCriteriaSetTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Attempt to instantiate RateLoadTariffCriteriaSet with a null selectionCriteria", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: selectionCriteria", () => new RateLoadTariffCriteriaSet(testTariff, selectionCriteria: null));
				AssertExceptionThrown("Attempt to instantiate RateLoadTariffCriteriaSet with a null tariff", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: tariff", () => new RateLoadTariffCriteriaSet(tariff: null, testCriteria));
				AssertNoExceptionThrown("Instantiating RateLoadTariffCriteriaSet with valid tariff and selectionCriteria objects", () => new RateLoadTariffCriteriaSet(testTariff, testCriteria));
			});
		}

		public void TestCriteriaId()
		{
			var tariffCriteriaSet1 = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertNotNull(nameof(tariffCriteriaSet1.CriteriaId), tariffCriteriaSet1.CriteriaId);
			var tariffCriteriaSet2 = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertNotNull(nameof(tariffCriteriaSet2.CriteriaId), tariffCriteriaSet2.CriteriaId);
			AssertNotEquals("Different instances of RateLoadTariffCriteriaSet must have different CriteriaId", tariffCriteriaSet1.CriteriaId, tariffCriteriaSet2.CriteriaId);
		}

		public void TestCacheKey()
		{
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertEquals("CacheKey", $"ApplicableRates_{testTariff.PK}_XX_SecondTR_YYY_2020-08-12T23:40:21_PP_AC1.AC2_CO_RT_RC", tariffCriteriaSet.CacheKey);
			var cacheKeyFromGetCriteriaSetCacheKey = RateLoadTariffCriteriaSet.GetCriteriaSetCacheKey(testTariff.PK, testCriteria);
			AssertEquals("CacheKey from GetCriteriaSetCacheKey", tariffCriteriaSet.CacheKey, cacheKeyFromGetCriteriaSetCacheKey);
		}

		[TestDate(2020, 8, 17, 7, 53, 15)]
		public void TestCacheKeyWhenCriteriaSetHasNullValues()
		{
			var testCriteriaSetWithNullValues = new SpecificRateSelectionCriteria(null, null, null, null, null, ZDate.Empty, null, null);
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(testTariff, testCriteriaSetWithNullValues);
			AssertEquals("CacheKey", $"ApplicableRates_{testTariff.PK}____2020-08-17T00:00:00_____", tariffCriteriaSet.CacheKey);
		}

		[TestDate(2020, 7, 14, 11, 55, 30)]
		public void TestGetCriteriaSetCacheKey()
		{
			var cacheKey1 = RateLoadTariffCriteriaSet.GetCriteriaSetCacheKey(testTariff.PK, testCriteria);
			AssertEquals("Cache Key", $"ApplicableRates_{testTariff.PK}_XX_SecondTR_YYY_2020-08-12T23:40:21_PP_AC1.AC2_CO_RT_RC", cacheKey1);
			var emptyTariffPk = ZGuid.Empty;
			var emptyTestCriteria = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString>(), ZDate.Empty, "", "");
			var cacheKey2 = RateLoadTariffCriteriaSet.GetCriteriaSetCacheKey(emptyTariffPk, emptyTestCriteria);
			AssertEquals("Cache Key with empty criteria values", $"ApplicableRates_00000000-0000-0000-0000-000000000000____2020-07-14T00:00:00_____", cacheKey2);
		}

		public void TestIsCached()
		{
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertEquals("[Before Caching] IsCached", false, tariffCriteriaSet.IsCached());
			Factory.GetCachedValue(tariffCriteriaSet.CacheKey, () => Enumerable.Empty<RateView>());
			AssertEquals("[After Caching] IsCached", true, tariffCriteriaSet.IsCached());
		}

		public void TestAddApplicableTableValueParameterRows()
		{
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			var tvpRateCriteriaTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();
			var tvpSecondTradeGroupsParameterTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();
			AssertEquals("[Empty Table Created] TVP Row Count", 0, tvpRateCriteriaTable.Rows.Count);
			AssertEquals("[Empty Table Created] TVP Row Count", 0, tvpSecondTradeGroupsParameterTable.Rows.Count);

			tariffCriteriaSet.AddApplicableTableValueParameterRows(tvpRateCriteriaTable, tvpSecondTradeGroupsParameterTable);

			CombineAssertions("Added rows when Tariff is not National Code", () =>
			{
				AssertEquals("[Tariff is not a National Code - should add 1 row] TVP Row Count", 1, tvpRateCriteriaTable.Rows.Count);
				var row = tvpRateCriteriaTable.Rows[0];
				AssertEquals("Row 0 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row[RateCriteriaTvp.Columns.CriteriaId]);
				AssertEquals("Row 0 - TariffPK", testTariff.PK, (Guid)row[RateCriteriaTvp.Columns.TariffPK]);

				AssertEquals("[Empty Table Created] TVP Row Count", 1, tvpSecondTradeGroupsParameterTable.Rows.Count);
				var secondTradeGroupRow1 = tvpSecondTradeGroupsParameterTable.Rows[0];
				AssertEquals("CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)secondTradeGroupRow1[SecondTradeGroupTvp.Columns.CriteriaId]);
			});
		}
		public void TestAddApplicableTableValueParameterRows_IsTariffNationalCode()
		{
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(testTariff, testCriteria);
			var tvpRateCriteriaTable = ApplicableRateLoader.GetEmptyRateCriteriaTable();
			var tvpSecondTradeGroupsParameterTable = ApplicableRateLoader.GetEmptySecondTradeGroupsParameterTable();

			testTariff.ZZ1_TableType = RefCusTariffNationalCodeSchema.Constants.Prefix;
			testTariff.ZZ1_ZZ1_Tariff = ZGuid.NewZGuid();

			tariffCriteriaSet.AddApplicableTableValueParameterRows(tvpRateCriteriaTable, tvpSecondTradeGroupsParameterTable);

			CombineAssertions("Added rows when Tariff is National Code", () =>
			{
				AssertEquals("[Tariff is a National Code - should add 2 rows] TVP Row Count", 2, tvpRateCriteriaTable.Rows.Count);
				var row1 = tvpRateCriteriaTable.Rows[0];
				AssertEquals("Row 1 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row1[RateCriteriaTvp.Columns.CriteriaId]);
				AssertEquals("Row 1 - TariffPK", testTariff.PK, (Guid)row1[RateCriteriaTvp.Columns.TariffPK]);
				var row2 = tvpRateCriteriaTable.Rows[1];
				AssertEquals("Row 2 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row2[RateCriteriaTvp.Columns.CriteriaId]);
				AssertEquals("Row 2 - TariffPK", testTariff.ZZ1_ZZ1_Tariff, (Guid)row2[RateCriteriaTvp.Columns.TariffPK]);

				AssertEquals("[Empty Table Created] TVP Row Count", 1, tvpSecondTradeGroupsParameterTable.Rows.Count);
				var secondTradeGroupRow1 = tvpSecondTradeGroupsParameterTable.Rows[0];
				AssertEquals("CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)secondTradeGroupRow1[SecondTradeGroupTvp.Columns.CriteriaId]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			testTariff = Factory.New<TariffView>();
			testCriteria = new SpecificRateSelectionCriteria("XX", "YYY", "PP", "CO",
				new HashSet<ZString> { "AC1", "AC2" }, new ZDateTime(2020, 8, 12, 23, 40, 21), "RT", "RC", new HashSet<ZString> { "SecondTR" });
		}

		TariffView testTariff;
		SpecificRateSelectionCriteria testCriteria;
	}
}
