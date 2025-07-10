using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	sealed class StrategicGoodsTest : TestCaseWithFactory
	{
		public void TestGetStrategicGoodsData_Parameters()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("factory is null", () => { testClass.GetStrategicGoodsData(null, tariffCodes, countryCode, ZDateTime.Empty); });
				AssertExceptionThrown<ArgumentNullException>("tariffCodes is null", () => { testClass.GetStrategicGoodsData(Factory, null, countryCode, ZDateTime.Empty); });
			});
		}

		public void TestGetStrategicGoodsData_NonTariffAndNomPresent()
		{
			var result = new StrategicGoods().GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Empty);

			AssertEquals("count comparison", 0, result.Count);
		}

		public void TestGetStrategicGoodsData_OnlyTariffPresent()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var dayOfLastWeek = ZDateTime.Today.AddDays(-7);
			var cusConditionType = CreateTestCusConditionType();
			CreateTestTariff(cusConditionType, yesterday, tomorrow, dayOfLastWeek);

			CombineAssertions(() =>
			{
				var result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Empty);
				AssertEquals("today - count comparison", 2, result.Count);
				AssertStrategicGoodRecord("today - Tariff - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2C3", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("today - Tariff - A3B4", result, "A3B4", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4", "345", "Test Risk condition class", yesterday, tomorrow);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Today.AddDays(-1));
				AssertEquals("yesterday - count comparison", 4, result.Count);
				AssertStrategicGoodRecord("yesterday - Tariff - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2C3", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("yesterday - Tariff - A1B2", result, "A1B2", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2", "345", "Test Risk condition class", yesterday, yesterday);
				AssertStrategicGoodRecord("yesterday - Tariff - A3B4C5", result, "A3B4C5", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4C5", "345", "Test Risk condition class", yesterday, yesterday);
				AssertStrategicGoodRecord("yesterday - Tariff - A3B4", result, "A3B4", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4", "345", "Test Risk condition class", yesterday, tomorrow);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Today.AddDays(-3));
				AssertEquals("3 days ago - count comparison", 0, result.Count);
			});
		}

		public void TestGetStrategicGoodsData_OnlyNomenclaturePresent()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var dayOfLastWeek = ZDateTime.Today.AddDays(-7);
			var cusConditionType = CreateTestCusConditionType();
			CreateTestNomenclature(cusConditionType, yesterday, tomorrow, dayOfLastWeek);

			CombineAssertions(() =>
			{
				var result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Empty);
				AssertEquals("today - count comparison", 0, result.Count);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Today.AddDays(-1));
				AssertEquals("yesterday - count comparison", 0, result.Count);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Today.AddDays(-3));
				AssertEquals("3 days ago - count comparison", 0, result.Count);
			});
		}

		public void TestGetStrategicGoodsData_BothTariffAndNomPresent()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var dayOfLastWeek = ZDateTime.Today.AddDays(-7);
			var cusConditionType = CreateTestCusConditionType();
			CreateTestTariff(cusConditionType, yesterday, tomorrow, dayOfLastWeek);
			CreateTestNomenclature(cusConditionType, yesterday, tomorrow, dayOfLastWeek);

			CombineAssertions(() =>
			{
				var result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode, ZDateTime.Empty);
				AssertEquals("today - count comparison", 4, result.Count);
				AssertStrategicGoodRecord("today - Tariff - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2C3", "345", "Test Risk condition class", yesterday,
					tomorrow);
				AssertStrategicGoodRecord("today - Nomenclature - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2C3", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("today - Nomenclature - A1B2", result, "A1B2", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("today - Tariff - A3B4", result, "A3B4", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4", "345", "Test Risk condition class", yesterday, tomorrow);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode,
					ZDateTime.Today.AddDays(-1));
				AssertEquals("yesterday - count comparison", 7, result.Count);
				AssertStrategicGoodRecord("yesterday - Tariff - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2C3", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("yesterday - Tariff - A1B2", result, "A1B2", "HSN DESC",
					"A1B2C3D4", "", "comment of tariff A1B2", "345", "Test Risk condition class", yesterday, yesterday);
				AssertStrategicGoodRecord("yesterday - Nomenclature - A1B2C3", result, "A1B2C3", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2C3", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("yesterday - Nomenclature - A1B2C", result, "A1B2C", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2C", "345", "Test Risk condition class", dayOfLastWeek, yesterday);
				AssertStrategicGoodRecord("yesterday - Nomenclature - A1B2", result, "A1B2", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2", "345", "Test Risk condition class", yesterday, tomorrow);
				AssertStrategicGoodRecord("yesterday - Tariff - A3B4C5", result, "A3B4C5", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4C5", "345", "Test Risk condition class", yesterday, yesterday);
				AssertStrategicGoodRecord("yesterday - Tariff - A3B4", result, "A3B4", "HSN DESC",
					"A3B4C5D6", "", "comment of tariff A3B4", "345", "Test Risk condition class", yesterday, tomorrow);

				result = testClass.GetStrategicGoodsData(Factory, tariffCodes, countryCode,
					ZDateTime.Today.AddDays(-3));
				AssertEquals("3 days ago - count comparison", 1, result.Count);
				AssertStrategicGoodRecord("3 days ago - Nomenclature - A1B2C", result, "A1B2C", "HSN DESC",
					"A1B2C3D4", "", "comment of nom A1B2C", "345", "Test Risk condition class", dayOfLastWeek, yesterday);
			});
		}

		static void AssertStrategicGoodRecord(string message, IEnumerable<IStrategicGoodsData> records, string tariff, string typeDescription, string requestedTariff, string source, string comment, string conditionType, string conditionTypeDesc, ZDateTime startDate, ZDateTime endDate)
		{
			AssertCollectionContains(message, records, r =>
				r.TariffTypeDescription == typeDescription && r.RequestedTariff == requestedTariff &&
				r.Tariff == tariff && r.Comment == comment && r.ConditionType == conditionType &&
				r.Source == source && r.ConditionTypeDescription == conditionTypeDesc
				&& r.StartDate == startDate && r.EndDate == endDate);
		}

		void CreateTestTariff(RefCusConditionType cusConditionType, ZDateTime yesterday, ZDateTime tomorrow, ZDateTime dayOfLastWeek)
		{
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("WCO", "HSN");
			Factory.Save();

			var tariff1 = helper.CreateRefCusTariff("WCO", cusTariffType.PK, "A1B2C3", yesterday, tomorrow, "tariff of A1B2C3");
			var tariff3 = helper.CreateRefCusTariff("WCO", cusTariffType.PK, "A1B2", dayOfLastWeek, yesterday, "tariff of A1B2");
			var tariff4 = helper.CreateRefCusTariff("WCO", cusTariffType.PK, "A3B4C5", yesterday, tomorrow, "tariff of A3B4C5");
			var tariff5 = helper.CreateRefCusTariff("WCO", cusTariffType.PK, "A3B4", yesterday, tomorrow, "tariff of A3B4");
			Factory.Save();

			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, tariff1.PK, $"comment of tariff {tariff1.ZZ1_TariffCode}", true, false, yesterday, tomorrow);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, tariff3.PK, $"comment of tariff {tariff3.ZZ1_TariffCode}", true, false, yesterday, tomorrow);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, tariff4.PK, $"comment of tariff {tariff4.ZZ1_TariffCode}", true, false, dayOfLastWeek, yesterday);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, tariff5.PK, $"comment of tariff {tariff5.ZZ1_TariffCode}", true, false, yesterday, tomorrow);
		}

		RefCusConditionType CreateTestCusConditionType()
		{
			return helper.CreateOrGetExistingRefCusConditionType("WCO", "RISK", "345", "Test Risk condition class");
		}

		void CreateTestNomenclature(RefCusConditionType cusConditionType, ZDateTime yesterday, ZDateTime tomorrow, ZDateTime dayOfLastWeek)
		{
			helper.CreateNewOrGetExistingTariffType("WCO", "HSN");
			Factory.Save();

			var nomenclature1 = helper.CreateNomenclatureGroup("WCO", "A1B2C3", yesterday, tomorrow, "nom of A1B2C3", "A1B2C3");
			var nomenclature2 = helper.CreateNomenclatureGroup("WCO", "A1B2C", dayOfLastWeek, yesterday, "nom of A1B2C", "A1B2C");
			var nomenclature3 = helper.CreateNomenclatureGroup("WCO", "A1B2", yesterday, tomorrow, "nom of A1B2", "A1B2");
			var nomenclature4 = helper.CreateNomenclatureGroup("WCO", "A1B", yesterday, tomorrow, "nom of A1B", "A1B");
			var nomenclature5 = helper.CreateNomenclatureGroup("WCO", "A1", yesterday, tomorrow, "nom of A1", "A1");
			Factory.Save();

			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, nomenclature1.PK, $"comment of nom {nomenclature1.ZZ5_Value}", true, false, yesterday, tomorrow, isTariff: false);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, nomenclature2.PK, $"comment of nom {nomenclature2.ZZ5_Value}", true, false, dayOfLastWeek, yesterday, isTariff: false);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, nomenclature3.PK, $"comment of nom {nomenclature3.ZZ5_Value}", true, false, yesterday, tomorrow, isTariff: false);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, nomenclature4.PK, $"comment of nom {nomenclature4.ZZ5_Value}", true, false, yesterday, tomorrow, isTariff: false);
			helper.CreateOrGetExistingRefCusCondition("WCO", cusConditionType.PK, nomenclature5.PK, $"comment of nom {nomenclature5.ZZ5_Value}", true, false, yesterday, tomorrow, isTariff: false);
		}

		protected override void SetUp()
		{
			helper = new UniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		readonly HashSet<ZString> tariffCodes = new HashSet<ZString>
		{
			new ZString("A1B2C3D4"),
			new ZString("A1B"),
			new ZString("ABCDEF"),
			new ZString("A3B4C5D6")
		};

		const string countryCode = Core.Constants.CountryCodes.Australia;
		readonly StrategicGoods testClass = new StrategicGoods();
		UniversalReferenceTestDataHelper helper;
	}
}
