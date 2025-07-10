using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class TariffAdditionalCodeLoadTariffCriteriaSetTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Attempt to instantiate TariffAdditionalCodeLoadTariffCriteriaSet with a null selectionCriteria", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: selectionCriteria", () => new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, selectionCriteria: null));
				AssertExceptionThrown("Attempt to instantiate TariffAdditionalCodeLoadTariffCriteriaSet with a null tariff", typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: tariff", () => new TariffAdditionalCodeLoadTariffCriteriaSet(tariff: null, testCriteria));
				AssertNoExceptionThrown("Instantiating TariffAdditionalCodeLoadTariffCriteriaSet with valid tariff and selectionCriteria objects", () => new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria));
			});
		}

		public void TestCriteriaId()
		{
			var tariffCriteriaSet1 = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertNotNull(nameof(tariffCriteriaSet1.CriteriaId), tariffCriteriaSet1.CriteriaId);
			var tariffCriteriaSet2 = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertNotNull(nameof(tariffCriteriaSet2.CriteriaId), tariffCriteriaSet2.CriteriaId);
			AssertNotEquals("Different instances of TariffAdditionalCodeLoadTariffCriteriaSet must have different CriteriaId", tariffCriteriaSet1.CriteriaId, tariffCriteriaSet2.CriteriaId);
		}

		public void TestCacheKey()
		{
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertEquals("CacheKey", $"TariffAdditionalCodes_{testTariff.PK}_SEP_2024-06-30T00:00:00_AU_FR", tariffCriteriaSet.CacheKey);
			var cacheKeyFromGetCriteriaSetCacheKey = TariffAdditionalCodeLoadTariffCriteriaSet.GetCriteriaSetCacheKey(testTariff.PK, testCriteria);
			AssertEquals("CacheKey from GetCriteriaSetCacheKey", tariffCriteriaSet.CacheKey, cacheKeyFromGetCriteriaSetCacheKey);
		}

		[TestDate(2020, 8, 17, 7, 53, 15)]
		public void TestCacheKeyWhenCriteriaSetHasNullValues()
		{
			var emptyTestCriteria = new TariffAdditionalCodeSelectionCriteria(null, ZDate.Empty, null, null);
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, emptyTestCriteria);
			AssertEquals("CacheKey", $"TariffAdditionalCodes_{testTariff.PK}__2020-08-17T00:00:00__", tariffCriteriaSet.CacheKey);
		}

		[TestDate(2020, 8, 17, 7, 53, 15)]
		public void TestGetCriteriaSetCacheKey()
		{
			var cacheKey1 = TariffAdditionalCodeLoadTariffCriteriaSet.GetCriteriaSetCacheKey(testTariff.PK, testCriteria);
			AssertEquals("CacheKey", $"TariffAdditionalCodes_{testTariff.PK}_SEP_2024-06-30T00:00:00_AU_FR", cacheKey1);

			var emptyTariffPk = ZGuid.Empty;
			var emptyTestCriteria = new TariffAdditionalCodeSelectionCriteria(null, ZDate.Empty, null, null);
			var cacheKey2 = TariffAdditionalCodeLoadTariffCriteriaSet.GetCriteriaSetCacheKey(emptyTariffPk, emptyTestCriteria);
			AssertEquals("Cache Key with empty criteria values", $"TariffAdditionalCodes_00000000-0000-0000-0000-000000000000__2020-08-17T00:00:00__", cacheKey2);
		}

		public void TestIsCached()
		{
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			AssertEquals("[Before Caching] IsCached", false, tariffCriteriaSet.IsCached());
			Factory.GetCachedValue(tariffCriteriaSet.CacheKey, () => Enumerable.Empty<TariffAdditionalCodeView>());
			AssertEquals("[After Caching] IsCached", true, tariffCriteriaSet.IsCached());
		}

		public void TestAddApplicableTableValueParameterRows()
		{
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			var tvpCriteriaTable = ApplicableTariffAdditionalCodeLoader.GetEmptyCriteriaTable();
			AssertEquals("[Empty Table Created] TVP Row Count", 0, tvpCriteriaTable.Rows.Count);

			tariffCriteriaSet.AddApplicableTableValueParameterRows(tvpCriteriaTable);
			CombineAssertions("Added rows when Tariff is not National Code", () =>
			{
				AssertEquals("[Tariff is not a National Code - should add 1 row] TVP Row Count", 1, tvpCriteriaTable.Rows.Count);
				var row = tvpCriteriaTable.Rows[0];
				AssertEquals("Row 0 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("Row 0 - TariffPK", testTariff.PK, (Guid)row[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK]);
			});
		}

		public void TestAddApplicableTableValueParameterRows_IsTariffNationalCode()
		{
			var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(testTariff, testCriteria);
			var tvpCriteriaTable = ApplicableTariffAdditionalCodeLoader.GetEmptyCriteriaTable();

			testTariff.ZZ1_TableType = RefCusTariffNationalCodeSchema.Constants.Prefix;
			testTariff.ZZ1_ZZ1_Tariff = ZGuid.NewZGuid();

			tariffCriteriaSet.AddApplicableTableValueParameterRows(tvpCriteriaTable);
			CombineAssertions("Added rows when Tariff is National Code", () =>
			{
				AssertEquals("[Tariff is a National Code - should add 2 rows] TVP Row Count", 2, tvpCriteriaTable.Rows.Count);
				var row1 = tvpCriteriaTable.Rows[0];
				AssertEquals("Row 1 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row1[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("Row 1 - TariffPK", testTariff.PK, (Guid)row1[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK]);
				var row2 = tvpCriteriaTable.Rows[1];
				AssertEquals("Row 2 - CriteriaId", tariffCriteriaSet.CriteriaId, (Guid)row2[TvpTariffAdditionalCodeSelectionCriteria.Columns.CriteriaId]);
				AssertEquals("Row 2 - TariffPK", testTariff.ZZ1_ZZ1_Tariff, (Guid)row2[TvpTariffAdditionalCodeSelectionCriteria.Columns.TariffPK]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			testTariff = Factory.New<TariffView>();
			testCriteria = new TariffAdditionalCodeSelectionCriteria("SEP", new ZDateTime(2024, 6, 30), "AU", "FR");
		}

		TariffView testTariff;
		TariffAdditionalCodeSelectionCriteria testCriteria;
	}
}
