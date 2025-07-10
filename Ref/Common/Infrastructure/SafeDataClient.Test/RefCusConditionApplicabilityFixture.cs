using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	internal class RefCusConditionApplicabilityFixture
	{
		[Test]
		public void Construct_CondApp()
		{
			var cond = new RefCusCondition
			{
				ZX1_ZZ1_Tariff = Guid.NewGuid(),
				ZX1_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZX1_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZX1_ZX2_ConditionType = Guid.NewGuid(),
				ZX1_ZZ5_Nomenclature = Guid.NewGuid(),
				ZX1_Source = "AAA",
				ZX1_Comment = "BBB",
				ZX1_IsImport = true,
				ZX1_IsExport = false,
				ZX1_ConditionValueTrueMeansStop = true,
				ZX1_ZZS_Preference = Guid.NewGuid(),
				ZX1_ZZZ_NKDataGrouping = "CCC",
				ZX1_LogicalANDWithinGroup = 1,
				ZX1_ZY7_NKConditionCode = "DDD",
				ZX1_AdditionalComment = "EEE",
			};
			var app = new RefCusApplicability
			{
				ZZT_AdditionalCode = "C999",
				ZZT_OrderNumber = "S001",
				ZZT_StartDate = new DateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_EndDate = new DateTimeOffset(2009, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_ZZA_SecondTradeGroup = Guid.NewGuid(),
			};
			cond.RefCusApplicabilities.Add(app);
			var result = new RefCusConditionApplicability(cond, app, safeRepository);

			Assert.AreEqual(cond.ZX1_ZZ1_Tariff, result.S07_ZZ1_Tariff);
			Assert.AreEqual(cond.ZX1_ZX2_ConditionType, result.S07_ZX2_ConditionType);
			Assert.AreEqual(cond.ZX1_ZZ5_Nomenclature, result.S07_ZZ5_Nomenclature);
			Assert.AreEqual(cond.ZX1_ZZS_Preference, result.S07_ZZS_Preference);
			Assert.AreEqual(new DateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.Zero), result.S07_StartDate);
			Assert.AreEqual(new DateTimeOffset(2009, 01, 01, 0, 0, 0, TimeSpan.Zero), result.S07_EndDate);
			Assert.AreEqual("AAA", result.S07_Source);
			Assert.AreEqual("BBB", result.S07_Comment);
			Assert.AreEqual("CCC", result.S07_ZZZ_NKDataGrouping);
			Assert.AreEqual("DDD", result.S07_ZY7_NKConditionCode);
			Assert.AreEqual("EEE", result.S07_AdditionalComment);
			Assert.AreEqual("C999", result.S07_AdditionalCode);
			Assert.AreEqual("S001", result.S07_OrderNumber);
			Assert.AreEqual(app.ZZT_ZZA_TradeGroup, result.S07_ZZA_TradeGroup);
			Assert.AreEqual(app.ZZT_ZZA_SecondTradeGroup, result.S07_ZZA_SecondTradeGroup);
			Assert.IsTrue(app.RefCusConditionApplicabilities.Contains(result));
		}

		[Test]
		public void Construct_CondAppVal()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			var val = new RefCusConditionValue { ZX3_Value = "AAA" };
			cond.RefCusConditionValues.Add(val);
			var result = new RefCusConditionApplicability(cond, app, safeRepository).RefCusConditionApplicabilityValues.FirstOrDefault();
			Assert.AreEqual(val, result.RefCusConditionValue);
			Assert.IsTrue(val.RefCusConditionApplicabilityValues.Contains(result));
			Assert.AreEqual("AAA", result.S08_Value);
		}

		[Test]
		public void Construct_CondAppLang()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			var lang = new RefCusConditionLanguage { ZXJ_Comment = "AAA" };
			cond.RefCusConditionLanguages.Add(lang);
			var result = new RefCusConditionApplicability(cond, app, safeRepository).RefCusConditionApplicabilityLanguages.FirstOrDefault();
			Assert.AreEqual(lang, result.RefCusConditionLanguage);
			Assert.IsTrue(lang.RefCusConditionApplicabilityLanguages.Contains(result));
			Assert.AreEqual("AAA", result.S09_Comment);
		}

		[Test]
		public void Construct_ExNew()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_ZZA_TradeGroup = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);
			var result = new RefCusConditionApplicability(cond, app, safeRepository).RefCusExcludedTradeGroupNews.FirstOrDefault();
			Assert.AreEqual(ex, result.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(result));
			Assert.AreEqual(ex.ZZC_ZZA_TradeGroup, result.S03_ZZA_TradeGroup);
		}

		[Test]
		public void Link_MatchedApp()
		{
			var cond = new RefCusCondition() { ZX1_ZZ1_Tariff = Guid.NewGuid() };
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app);
			var result = new RefCusConditionApplicability(safeRepository);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(result, app)).Returns(true);
			result.Create();
			result.Link(cond, comp.Object, out var _);
			Assert.AreEqual(app, result.RefCusApplicability);
			Assert.IsTrue(app.RefCusConditionApplicabilities.Contains(result));
		}

		[Test]
		public void Link_UnMatchedApp()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app);
			var result = new RefCusConditionApplicability(safeRepository);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(result, app)).Returns(false);
			var newApp = result.Create().OfType<RefCusApplicability>().FirstOrDefault();
			result.Link(cond, comp.Object, out var _);
			Assert.IsNotNull(newApp);
			Assert.AreEqual(newApp, result.RefCusApplicability);
			Assert.AreNotEqual(newApp.ZZT_PK, app.ZZT_PK);
			Assert.AreEqual(newApp.ZZT_ZX1_Conditions, cond.ZX1_PK);
			Assert.AreEqual(2, cond.RefCusApplicabilities.Count);
			Assert.IsTrue(cond.RefCusApplicabilities.Contains(newApp));
			Assert.IsNull(newApp.RefCusCondition);
		}

		[Test]
		public void Link_MatchedVal()
		{
			var cond = new RefCusCondition();
			var val = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			cond.RefCusConditionValues.Add(val);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp);
			condApp.RefCusConditionApplicabilityValues.Add(condAppVal);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condAppVal, val)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var result);
			Assert.AreEqual(1, result.OfType<RefCusConditionValue>().Count());
			Assert.AreEqual(val, condAppVal.RefCusConditionValue);
			Assert.IsTrue(val.RefCusConditionApplicabilityValues.Contains(condAppVal));
		}

		[Test]
		public void Link_UnMatchedVal()
		{
			var cond = new RefCusCondition();
			var val = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			cond.RefCusConditionValues.Add(val);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp);
			condApp.RefCusConditionApplicabilityValues.Add(condAppVal);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condAppVal, val)).Returns(false);
			var newVal = condApp.Create().OfType<RefCusConditionValue>().FirstOrDefault();
			condApp.Link(cond, comp.Object, out var _);
			Assert.IsNotNull(newVal);
			Assert.AreEqual(newVal, condAppVal.RefCusConditionValue);
			Assert.AreNotEqual(newVal, val);
			Assert.IsTrue(newVal.RefCusConditionApplicabilityValues.Contains(condAppVal));
		}

		[Test]
		public void Link_MatchedLang()
		{
			var cond = new RefCusCondition();
			var lang = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			cond.RefCusConditionLanguages.Add(lang);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp);
			condApp.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condAppLang, lang)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var result);
			Assert.AreEqual(1, result.OfType<RefCusConditionLanguage>().Count());
			Assert.AreEqual(lang, condAppLang.RefCusConditionLanguage);
			Assert.IsTrue(lang.RefCusConditionApplicabilityLanguages.Contains(condAppLang));
		}

		[Test]
		public void Link_UnMatchedLang()
		{
			var cond = new RefCusCondition();
			var lang = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			cond.RefCusConditionLanguages.Add(lang);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp);
			condApp.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condAppLang, lang)).Returns(false);
			var newLang = condApp.Create().OfType<RefCusConditionLanguage>().FirstOrDefault();
			condApp.Link(cond, comp.Object, out var _);
			Assert.IsNotNull(newLang);
			Assert.AreEqual(newLang, condAppLang.RefCusConditionLanguage);
			Assert.AreNotEqual(newLang, lang);
			Assert.IsTrue(newLang.RefCusConditionApplicabilityLanguages.Contains(condAppLang));
		}

		[Test]
		public void Link_MatchedEx()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);

			var condApp = new RefCusConditionApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(condApp);
			condApp.RefCusExcludedTradeGroupNews.Add(exNew);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var result);
			Assert.AreEqual(3, result.Count());
			Assert.AreEqual(ex, exNew.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(exNew));
		}

		[Test]
		public void Link_UnMatchedEx()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);

			var condApp = new RefCusConditionApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(condApp);
			condApp.RefCusExcludedTradeGroupNews.Add(exNew);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(false);
			var newEx = condApp.Create().OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			condApp.Link(cond, comp.Object, out var _);
			Assert.IsNotNull(newEx);
			Assert.AreNotEqual(newEx, ex);
			Assert.AreEqual(newEx, exNew.RefCusExcludedTradeGroup);
			Assert.IsTrue(newEx.RefCusExcludedTradeGroupNews.Contains(exNew));
		}

		[Test]
		public void Update_CondApp()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			var condApp = new RefCusConditionApplicability(safeRepository)
			{
				S07_ZZ1_Tariff = Guid.NewGuid(),
				S07_ZX2_ConditionType = Guid.NewGuid(),
				S07_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S07_ZZ5_Nomenclature = Guid.NewGuid(),
				S07_Source = "AAA",
				S07_Comment = "BBB",
				S07_IsImport = true,
				S07_IsExport = true,
				S07_ConditionValueTrueMeansStop = true,
				S07_ZZS_Preference = Guid.NewGuid(),
				S07_ZZZ_NKDataGrouping = "CCC",
				S07_LogicalANDWithinGroup = 1,
				S07_ZY7_NKConditionCode = "DDD",
				S07_AdditionalComment = "EEE",
				S07_ZZA_TradeGroup = Guid.NewGuid(),
				S07_AdditionalCode = "FFF",
				S07_OrderNumber = "GGG",
				S07_ZZA_SecondTradeGroup = Guid.NewGuid(),
			};
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, cond)).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp, app)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var _);
			condApp.Update(true);
			Assert.AreEqual(new DateTimeOffset(1900, 01, 01, 0, 0, 0, TimeSpan.Zero), cond.ZX1_StartDate);
			Assert.AreEqual(new DateTimeOffset(2079, 06, 06, 0, 0, 0, TimeSpan.Zero), cond.ZX1_EndDate);
			Assert.AreEqual("AAA", cond.ZX1_Source);
			Assert.AreEqual("BBB", cond.ZX1_Comment);
			Assert.AreEqual(true, cond.ZX1_IsImport);
			Assert.AreEqual(true, cond.ZX1_IsExport);
			Assert.AreEqual(true, cond.ZX1_ConditionValueTrueMeansStop);
			Assert.AreEqual(condApp.S07_ZZS_Preference, cond.ZX1_ZZS_Preference);
			Assert.AreEqual("CCC", cond.ZX1_ZZZ_NKDataGrouping);
			Assert.AreEqual(1, cond.ZX1_LogicalANDWithinGroup);
			Assert.AreEqual("DDD", cond.ZX1_ZY7_NKConditionCode);
			Assert.AreEqual("EEE", cond.ZX1_AdditionalComment);
			Assert.AreEqual("FFF", app.ZZT_AdditionalCode);
			Assert.AreEqual("GGG", app.ZZT_OrderNumber);
			Assert.AreEqual(new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero), app.ZZT_StartDate);
			Assert.AreEqual(new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero), app.ZZT_EndDate);
			Assert.AreEqual(condApp.S07_ZZA_TradeGroup, app.ZZT_ZZA_TradeGroup);
			Assert.AreEqual(condApp.S07_ZZA_SecondTradeGroup, app.ZZT_ZZA_SecondTradeGroup);
		}

		[Test]
		public void Update_CondAppVal()
		{
			var cond = new RefCusCondition();
			var val = new RefCusConditionValue { ZX3_Value = "AAA" };
			cond.RefCusConditionValues.Add(val);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp) { S08_Value = "BBB" };
			condApp.RefCusConditionApplicabilityValues.Add(condAppVal);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, cond)).Returns(true);
			comp.Setup(x => x.IsIdentical(condAppVal, val)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var _);
			condApp.Update(false);
			Assert.AreEqual("BBB", val.ZX3_Value);
		}

		[Test]
		public void Update_CondAppLang()
		{
			var cond = new RefCusCondition();
			var lang = new RefCusConditionLanguage { ZXJ_Comment = "AAA" };
			cond.RefCusConditionLanguages.Add(lang);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp) { S09_Comment = "BBB" };
			condApp.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, cond)).Returns(true);
			comp.Setup(x => x.IsIdentical(condAppLang, lang)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var _);
			condApp.Update(false);
			Assert.AreEqual("BBB", lang.ZXJ_Comment);
		}

		[Test]
		public void Update_CondAppEx()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_ZZA_TradeGroup = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);
			var condApp = new RefCusConditionApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(condApp) { S03_ZZA_TradeGroup = Guid.NewGuid() };
			condApp.RefCusExcludedTradeGroupNews.Add(exNew);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp, cond)).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(true);
			condApp.Create();
			condApp.Link(cond, comp.Object, out var _);
			Assert.AreNotEqual(exNew.S03_ZZA_TradeGroup, ex.ZZC_ZZA_TradeGroup);
			condApp.Update(false);
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, ex.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void Create_CondApp()
		{
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp);
			condApp.RefCusConditionApplicabilityValues.Add(condAppVal);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp);
			condApp.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var newEx = new RefCusExcludedTradeGroupNew(condApp);
			condApp.RefCusExcludedTradeGroupNews.Add(newEx);
			var results = condApp.Create();
			Assert.AreEqual(5, results.Count());
			var app = results.OfType<RefCusApplicability>().FirstOrDefault();
			var cond = results.OfType<RefCusCondition>().FirstOrDefault();
			Assert.IsNull(app.RefCusCondition);
			Assert.AreEqual(app.ZZT_ZX1_Conditions, cond.ZX1_PK);
			Assert.IsTrue(cond.RefCusApplicabilities.Contains(app));
			var val = results.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.AreEqual(val.ZX3_ZX1_Condition, cond.ZX1_PK);
			Assert.IsTrue(cond.RefCusConditionValues.Contains(val));
			Assert.IsNull(val.RefCusCondition);
			var lang = results.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.AreEqual(lang.ZXJ_ZX1_Condition, cond.ZX1_PK);
			Assert.IsTrue(cond.RefCusConditionLanguages.Contains(lang));
			Assert.IsNull(lang.RefCusCondition);
			var ex = results.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(ex.ZZC_ZZT_Applicability, app.ZZT_PK);
			Assert.IsTrue(app.RefCusExcludedTradeGroups.Contains(ex));
			Assert.IsNull(ex.RefCusApplicability);
		}

		[Test]
		public void Create_CondApp_MultiExcludedTradeGroups()
		{
			var cond = new RefCusCondition();
			var app = new RefCusApplicability();
			var tradeGroupPK = Guid.NewGuid();

			var condApp = new RefCusConditionApplicability(cond, app, safeRepository);
			var exTrade1 = new RefCusExcludedTradeGroupNew(condApp) {S03_ZZA_TradeGroup= tradeGroupPK};
			condApp.RefCusExcludedTradeGroupNews.Add(exTrade1);
			var result = condApp.Create();
			Assert.AreEqual(1, result.Count());
			var R_ex1 = result.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(R_ex1.ZZC_ZZT_Applicability, app.ZZT_PK);
			Assert.IsNull(R_ex1.RefCusApplicability);

			var exTrade2 = new RefCusExcludedTradeGroupNew(condApp) { S03_ZZA_TradeGroup = tradeGroupPK };
			condApp.RefCusExcludedTradeGroupNews.Add(exTrade2);
			result = condApp.Create();
			Assert.AreEqual(0, result.Count());
		}

		[Test]
		public void Create_MultiCondApps_SingleCondVal()
		{
			var cond = new RefCusCondition();
			var valueTypePK = Guid.NewGuid();
			var app1 = new RefCusApplicability();
			var app2 = new RefCusApplicability();
			var app3 = new RefCusApplicability();

			var condApp1 = new RefCusConditionApplicability(cond, app1, safeRepository);
			var condAppVal1 = new RefCusConditionApplicabilityValue(condApp1) { S08_Value = "D08", S08_ZX4_ValueType = valueTypePK };
			condApp1.RefCusConditionApplicabilityValues.Add(condAppVal1);
			var result1 = condApp1.Create();
			Assert.AreEqual(1, result1.Count());
			var R_val1 = result1.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.AreEqual(R_val1.ZX3_ZX1_Condition, cond.ZX1_PK);
			Assert.IsNull(R_val1.RefCusCondition);

			var condApp2 = new RefCusConditionApplicability(cond, app2, safeRepository);
			var condAppVal2 = new RefCusConditionApplicabilityValue(condApp2) { S08_Value = "D09", S08_ZX4_ValueType = valueTypePK };
			condApp2.RefCusConditionApplicabilityValues.Add(condAppVal2);
			var result2 = condApp2.Create();
			Assert.AreEqual(1, result2.Count());
			var R_val2 = result2.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.AreEqual(R_val2.ZX3_ZX1_Condition, cond.ZX1_PK);
			Assert.IsNull(R_val2.RefCusCondition);

			var condApp3 = new RefCusConditionApplicability(cond, app3, safeRepository);
			var condAppVal3 = new RefCusConditionApplicabilityValue(condApp3) { S08_Value = "D09", S08_ZX4_ValueType = valueTypePK };
			condApp3.RefCusConditionApplicabilityValues.Add(condAppVal3);
			var result3 = condApp3.Create();
			Assert.AreEqual(0, result3.Count());
		}

		[Test]
		public void Create_MultiCondApps_SingleCondLan()
		{
			var cond = new RefCusCondition();
			var app1 = new RefCusApplicability();
			var app2 = new RefCusApplicability();
			var app3 = new RefCusApplicability();

			var condApp1 = new RefCusConditionApplicability(cond, app1, safeRepository);
			var condAppLan1 = new RefCusConditionApplicabilityLanguage(condApp1) { S09_ZX6_NKLanguage = "EN" };
			condApp1.RefCusConditionApplicabilityLanguages.Add(condAppLan1);
			var result1 = condApp1.Create();
			Assert.AreEqual(1, result1.Count());
			var R_lan1 = result1.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.AreEqual(R_lan1.ZXJ_ZX1_Condition, cond.ZX1_PK);
			Assert.IsNull(R_lan1.RefCusCondition);

			var condApp2 = new RefCusConditionApplicability(cond, app2, safeRepository);
			var condAppLan2 = new RefCusConditionApplicabilityLanguage(condApp2) { S09_ZX6_NKLanguage = "FRN" };
			condApp2.RefCusConditionApplicabilityLanguages.Add(condAppLan2);
			var result2 = condApp2.Create();
			Assert.AreEqual(1, result2.Count());
			var R_lan2 = result2.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.AreEqual(R_lan2.ZXJ_ZX1_Condition, cond.ZX1_PK);
			Assert.IsNull(R_lan2.RefCusCondition);

			var condApp3 = new RefCusConditionApplicability(cond, app3, safeRepository);
			var condAppLan3 = new RefCusConditionApplicabilityLanguage(condApp3) { S09_ZX6_NKLanguage = "FRN" };
			condApp3.RefCusConditionApplicabilityLanguages.Add(condAppLan3);
			var result3 = condApp3.Create();
			Assert.AreEqual(0, result3.Count());
		}

		[Test]
		public void Create_CondApp_WithTariff()
		{
			var tariff = new RefCusTariff() { ZZ1_PK = Guid.NewGuid() };
			var condApp = new RefCusConditionApplicability(safeRepository);
			condApp.S07_ZZ1_Tariff = tariff.ZZ1_PK;
			var results1 = condApp.Create();
			var cond1 = results1.OfType<RefCusCondition>().FirstOrDefault();
			Assert.IsNotNull(cond1);
			Assert.AreEqual(tariff.ZZ1_PK, cond1.ZX1_ZZ1_Tariff);
		}

		[Test]
		public void Unlink()
		{
			var condApp = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp);
			condApp.RefCusConditionApplicabilityValues.Add(condAppVal);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp);
			condApp.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var newEx = new RefCusExcludedTradeGroupNew(condApp);
			condApp.RefCusExcludedTradeGroupNews.Add(newEx);
			var created = condApp.Create();
			Assert.IsNotNull(condApp.RefCusApplicability);
			var app = created.OfType<RefCusApplicability>().FirstOrDefault();
			Assert.IsTrue(app.RefCusConditionApplicabilities.Contains(condApp));
			Assert.IsNotNull(condAppVal.RefCusConditionValue);

			var val = created.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.IsTrue(val.RefCusConditionApplicabilityValues.Contains(condAppVal));
			var lang = created.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.IsTrue(lang.RefCusConditionApplicabilityLanguages.Contains(condAppLang));
			Assert.IsNotNull(newEx.RefCusExcludedTradeGroup);
			var ex = created.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(newEx));

			var unlinks = condApp.Unlink();
			Assert.IsTrue(unlinks.Contains(app));
			Assert.IsTrue(unlinks.Contains(val));
			Assert.IsTrue(unlinks.Contains(lang));
			Assert.IsTrue(unlinks.Contains(ex));
			Assert.IsNull(condApp.RefCusApplicability);
			Assert.IsFalse(app.RefCusConditionApplicabilities.Contains(condApp));
			Assert.IsNull(condAppVal.RefCusConditionValue);
			Assert.IsFalse(val.RefCusConditionApplicabilityValues.Contains(condAppVal));
			Assert.IsNull(condAppLang.RefCusConditionLanguage);
			Assert.IsFalse(lang.RefCusConditionApplicabilityLanguages.Contains(condAppLang));
			Assert.IsNull(newEx.RefCusExcludedTradeGroup);
			Assert.IsFalse(ex.RefCusExcludedTradeGroupNews.Contains(newEx));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
