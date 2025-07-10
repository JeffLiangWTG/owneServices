using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class NonPersistentBusinessObjectComparisonFixture
	{
		[Test]
		public void IsIdentical_Rate()
		{
			var rate = new RefCusRate
			{
				ZZ2_ZZ1_Tariff = Guid.NewGuid(),
				ZZ2_ZY1_RateCode = Guid.NewGuid(),
				ZZ2_RateFormula = "8%",
				ZZ2_SelectorFormula = "XXX",
				ZZ2_ZZZ_NKDataGrouping = "EUN",
				ZZ2_StartDate = NonPersistentBusinessObjectComparison.MinStartDate,
				ZZ2_EndDate = NonPersistentBusinessObjectComparison.MaxEndDate,
				ZZ2_RateFormulaDerivedFrom = null,
				ZZ2_RX_NKCurrencyOverride = "AUD"
			};
			var rateApp = new RefCusRateApplicability(safeRepository)
			{
				S01_ZZ1_Tariff = rate.ZZ2_ZZ1_Tariff,
				S01_ZY1_RateCode = rate.ZZ2_ZY1_RateCode,
				S01_RateFormula = "8%",
				S01_SelectorFormula = "xxx",
				S01_ZZZ_NKDataGrouping = "eun",
				S01_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_RateFormulaDerivedFrom = null,
				S01_RX_NKCurrencyOverride = "aud"
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(rateApp, rate));
			rateApp.S01_RateFormula = "9%";
			Assert.IsFalse(comp.IsIdentical(rateApp, rate));
		}

		[Test]
		public void IsIdentical_RateApp_App()
		{
			var app = new RefCusApplicability()
			{
				ZZT_AdditionalCode = "C99",
				ZZT_OrderNumber = "SS1",
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_ZZA_SecondTradeGroup = Guid.NewGuid(),
				ZZT_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_ZY2_AdditionalCode = Guid.NewGuid()
			};
			var rateApp = new RefCusRateApplicability(safeRepository)
			{
				S01_AdditionalCode = "c99",
				S01_OrderNumber = "ss1",
				S01_ZZA_TradeGroup = app.ZZT_ZZA_TradeGroup,
				S01_ZZA_SecondTradeGroup = app.ZZT_ZZA_SecondTradeGroup,
				S01_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_ZY2_AdditionalCode = app.ZZT_ZY2_AdditionalCode
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(rateApp, app));
			rateApp.S01_AdditionalCode = "c98";
			Assert.IsFalse(comp.IsIdentical(rateApp, app));
		}

		[Test]
		public void IsIdentical_UOM()
		{
			var uom = new RefCusRateUOM
			{
				ZXG_UOM = "KG"
			};
			var rateAppUOM = new RefCusRateApplicabilityUOM(new RefCusRateApplicability(safeRepository))
			{
				S02_UOM = "KG"
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(rateAppUOM, uom));
			rateAppUOM.S02_UOM = "G";
			Assert.IsFalse(comp.IsIdentical(rateAppUOM, uom));
		}

		[Test]
		public void IsIdentical_Ex()
		{
			var ex = new RefCusExcludedTradeGroup
			{
				ZZC_ZZA_TradeGroup = Guid.NewGuid()
			};
			var exNew = new RefCusExcludedTradeGroupNew(new RefCusRateApplicability(safeRepository))
			{
				S03_ZZA_TradeGroup = ex.ZZC_ZZA_TradeGroup
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(exNew, ex));
			exNew.S03_ZZA_TradeGroup = Guid.NewGuid();
			Assert.IsFalse(comp.IsIdentical(exNew, ex));
		}

		[Test]
		public void IsIdentical_Cond_WithoutCondVal()
		{
			var cond = new RefCusCondition
			{
				ZX1_Comment = "AAA",
				ZX1_StartDate = NonPersistentBusinessObjectComparison.MinStartDate,
				ZX1_EndDate = NonPersistentBusinessObjectComparison.MaxEndDate,
				ZX1_AdditionalComment = "CCC",
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_IsExport = true,
				ZX1_IsImport = false,
				ZX1_LogicalANDWithinGroup = new byte(),
				ZX1_Source = "DDD",
				ZX1_ZX2_ConditionType = Guid.NewGuid(),
				ZX1_ZY7_NKConditionCode = "EEE",
				ZX1_ZZZ_NKDataGrouping = "FFF",
				ZX1_ZZS_Preference = Guid.NewGuid()
			};
			var condApp = new RefCusConditionApplicability(safeRepository)
			{
				S07_Comment = "AAA",
				S07_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_AdditionalComment = "CCC",
				S07_ConditionValueTrueMeansStop = true,
				S07_IsExport = true,
				S07_IsImport = false,
				S07_LogicalANDWithinGroup = cond.ZX1_LogicalANDWithinGroup,
				S07_Source = "DDD",
				S07_ZX2_ConditionType = cond.ZX1_ZX2_ConditionType,
				S07_ZY7_NKConditionCode = "EEE",
				S07_ZZZ_NKDataGrouping = "FFF",
				S07_ZZS_Preference = cond.ZX1_ZZS_Preference
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(condApp, cond));
			condApp.S07_Comment = "BBB";
			Assert.IsFalse(comp.IsIdentical(condApp, cond));
		}

		[Test]
		public void IsIdentical_Cond_WithCondVal()
		{
			var cond = new RefCusCondition
			{
				ZX1_Comment = "AAA",
				ZX1_StartDate = NonPersistentBusinessObjectComparison.MinStartDate,
				ZX1_EndDate = NonPersistentBusinessObjectComparison.MaxEndDate,
			};
			var condVal1 = new RefCusConditionValue { ZX3_Value = "BBB" };
			var condVal2 = new RefCusConditionValue { ZX3_Value = "CCC" };
			cond.RefCusConditionValues.Add(condVal1);
			cond.RefCusConditionValues.Add(condVal2);

			var condApp = new RefCusConditionApplicability(safeRepository) { S07_Comment = "AAA" };
			var condAppValue1 = new RefCusConditionApplicabilityValue(condApp) { S08_Value = "BBB" };
			var condAppValue2 = new RefCusConditionApplicabilityValue(condApp) { S08_Value = "CCC" };
			condApp.RefCusConditionApplicabilityValues.Add(condAppValue1);
			condApp.RefCusConditionApplicabilityValues.Add(condAppValue2);

			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(condApp, cond));
			condApp.RefCusConditionApplicabilityValues.First().S08_Value = "EEE";
			Assert.IsFalse(comp.IsIdentical(condApp, cond));
		}

		[Test]
		public void IsIdentical_CondApp_App()
		{
			var app = new RefCusApplicability
			{
				ZZT_AdditionalCode = "C99",
				ZZT_OrderNumber = "SS1",
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_ZZA_SecondTradeGroup = Guid.NewGuid(),
				ZZT_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_ZY2_AdditionalCode = Guid.NewGuid()
			};
			var condApp = new RefCusConditionApplicability(safeRepository)
			{
				S07_AdditionalCode = "c99",
				S07_OrderNumber = "ss1",
				S07_ZZA_TradeGroup = app.ZZT_ZZA_TradeGroup,
				S07_ZZA_SecondTradeGroup = app.ZZT_ZZA_SecondTradeGroup,
				S07_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_ZY2_AdditionalCode = app.ZZT_ZY2_AdditionalCode
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(condApp, app));
			condApp.S07_OrderNumber = "BBB";
			Assert.IsFalse(comp.IsIdentical(condApp, app));
		}

		[Test]
		public void IsIdentical_CondVal()
		{
			var val = new RefCusConditionValue
			{
				ZX3_Value = "AAA"
			};
			var condAppVal = new RefCusConditionApplicabilityValue(new RefCusConditionApplicability(safeRepository))
			{
				S08_Value = "AAA"
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(condAppVal, val));
			condAppVal.S08_Value = "BBB";
			Assert.IsFalse(comp.IsIdentical(condAppVal, val));
		}

		[Test]
		public void IsIdentical_CondLang()
		{
			var lang = new RefCusConditionLanguage
			{
				ZXJ_ZX6_NKLanguage = "AAA",
				ZXJ_Comment = "abc"
			};
			var condAppLang = new RefCusConditionApplicabilityLanguage(new RefCusConditionApplicability(safeRepository))
			{
				S09_ZX6_NKLanguage = "AAA",
				S09_Comment = "def"
			};
			var comp = new NonPersistentBusinessObjectComparison();
			Assert.IsTrue(comp.IsIdentical(condAppLang, lang));
			condAppLang.S09_ZX6_NKLanguage = "BBB";
			Assert.IsFalse(comp.IsIdentical(condAppLang, lang));
		}

		[Test]
		public void IsIdentical_NotSupport()
		{
			var comp = new NonPersistentBusinessObjectComparison();
			var rateApp = new RefCusRateApplicability(safeRepository);
			var tariff = new RefCusTariff();
			var ex = Assert.Throws<ArgumentException>(() => { comp.IsIdentical(rateApp, tariff); });
			Assert.IsNotNull(ex);
			Assert.That(ex.Message == "Not support type group: nonPersistentObject is RefCusRateApplicability, persistentObject is RefCusTariff");
		}

		[Test]
		public void IsIdentical_NullObject()
		{
			var comp = new NonPersistentBusinessObjectComparison();
			var ex = Assert.Throws<ArgumentNullException>(() => { comp.IsIdentical(new RefCusRateApplicability(safeRepository), null); });
			Assert.IsNotNull(ex);
			Assert.That(ex.Message == "Value cannot be null. (Parameter 'persistentObject')");

			ex = Assert.Throws<ArgumentNullException>(() => { comp.IsIdentical(null, new RefCusRate()); });
			Assert.IsNotNull(ex);
			Assert.That(ex.Message == "Value cannot be null. (Parameter 'nonPersistentObject')");
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
