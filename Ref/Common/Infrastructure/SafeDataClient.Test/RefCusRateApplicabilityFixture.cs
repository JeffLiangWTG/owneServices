using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test.NonPersistentObject
{
	[TestFixture]
	internal class RefCusRateApplicabilityFixture
	{
		[Test]
		public void Construct_RateApp()
		{
			var rate = new RefCusRate
			{
				ZZ2_ZZ1_Tariff = Guid.NewGuid(),
				ZZ2_ZZW_TariffNationalCode = Guid.NewGuid(),
				ZZ2_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZ2_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZ2_RateFormula = "8% VFD",
				ZZ2_SelectorFormula = "SELECT 1",
				ZZ2_RateFormulaDerivedFrom = "TARIC",
				ZZ2_RX_NKCurrencyOverride = "AUD",
				ZZ2_ZY1_RateCode = Guid.NewGuid(),
				ZZ2_ZZS_Preference = Guid.NewGuid(),
				ZZ2_ZZZ_NKDataGrouping = "EUN"
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				RefCusRate = rate,
				ZZT_AdditionalCode = "C999",
				ZZT_OrderNumber = "S001",
				ZZT_StartDate = new DateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_EndDate = new DateTimeOffset(2009, 01, 01, 0, 0, 0, TimeSpan.Zero),
				ZZT_ZZA_TradeGroup = Guid.NewGuid(),
				ZZT_ZZA_SecondTradeGroup = Guid.NewGuid(),
			};
			rate.RefCusApplicabilities.Add(app);
			var result = new RefCusRateApplicability(rate, app, safeRepository);
			Assert.AreEqual(rate.ZZ2_ZZ1_Tariff, result.S01_ZZ1_Tariff);
			Assert.AreEqual(rate.ZZ2_ZZW_TariffNationalCode, result.S01_ZZW_TariffNationalCode);
			Assert.AreEqual(new DateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.Zero), result.S01_StartDate);
			Assert.AreEqual(new DateTimeOffset(2009, 01, 01, 0, 0, 0, TimeSpan.Zero), result.S01_EndDate);
			Assert.AreEqual("8% VFD", result.S01_RateFormula);
			Assert.AreEqual("SELECT 1", result.S01_SelectorFormula);
			Assert.AreEqual("TARIC", result.S01_RateFormulaDerivedFrom);
			Assert.AreEqual("AUD", result.S01_RX_NKCurrencyOverride);
			Assert.AreEqual(rate.ZZ2_ZY1_RateCode, result.S01_ZY1_RateCode);
			Assert.AreEqual(rate.ZZ2_ZZS_Preference, result.S01_ZZS_Preference);
			Assert.AreEqual(rate.ZZ2_ZZZ_NKDataGrouping, result.S01_ZZZ_NKDataGrouping);
			Assert.AreEqual("C999", result.S01_AdditionalCode);
			Assert.AreEqual("S001", result.S01_OrderNumber);
			Assert.AreEqual(app.ZZT_ZZA_TradeGroup, result.S01_ZZA_TradeGroup);
			Assert.AreEqual(app.ZZT_ZZA_SecondTradeGroup, result.S01_ZZA_SecondTradeGroup);
			Assert.AreEqual(app, result.RefCusApplicability);
			Assert.IsTrue(app.RefCusRateApplicabilities.Contains(result));
		}

		[Test]
		public void Construct_RateAppUOM()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability
			{
				RefCusRate = rate
			};
			rate.RefCusApplicabilities.Add(app);
			var uom = new RefCusRateUOM
			{
				ZXG_UOM = "KG",
				RefCusRate = rate
			};
			rate.RefCusRateUOMs.Add(uom);
			var result = new RefCusRateApplicability(rate, app, safeRepository).RefCusRateApplicabilityUOMs.FirstOrDefault();
			Assert.AreEqual(uom, result.RefCusRateUOM);
			Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(result));
			Assert.AreEqual("KG", result.S02_UOM);
		}

		[Test]
		public void Construct_ExNew()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability
			{
				RefCusRate = rate
			};
			rate.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup
			{
				RefCusApplicability = app,
				ZZC_ZZA_TradeGroup = Guid.NewGuid()
			};
			app.RefCusExcludedTradeGroups.Add(ex);
			var result = new RefCusRateApplicability(rate, app, safeRepository).RefCusExcludedTradeGroupNews.FirstOrDefault();
			Assert.AreEqual(ex, result.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(result));
			Assert.AreEqual(ex.ZZC_ZZA_TradeGroup, result.S03_ZZA_TradeGroup);
		}

		[Test]
		public void Link_MatchedApp()
		{
			var rate = new RefCusRate { ZZ2_ZZ1_Tariff = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = Guid.NewGuid() };
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app);
			var result = new RefCusRateApplicability(safeRepository);
			result.Create();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(result, app)).Returns(true);
			result.Link(rate, comp.Object, out var _);
			Assert.AreEqual(app, result.RefCusApplicability);
			Assert.IsTrue(app.RefCusRateApplicabilities.Contains(result));
		}

		[Test]
		public void Link_UnMatchedApp()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app);
			var result = new RefCusRateApplicability(safeRepository);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(result, app)).Returns(false);
			var newApp = result.Create().OfType<RefCusApplicability>().FirstOrDefault();
			result.Link(rate, comp.Object, out var unlinked);
			Assert.AreEqual(1, unlinked.Count());
			Assert.AreEqual(newApp, result.RefCusApplicability);
			Assert.AreNotEqual(newApp.ZZT_PK, app.ZZT_PK);
			Assert.AreEqual(newApp.ZZT_ZZ2_Rate, rate.ZZ2_PK);
			Assert.AreEqual(2, rate.RefCusApplicabilities.Count);
			Assert.IsTrue(rate.RefCusApplicabilities.Contains(newApp));
			Assert.IsNull(newApp.RefCusRate);
			Assert.That(rate.RefCusApplicabilities.All(x => x.ZZT_PK != Guid.Empty));
		}

		[Test]
		public void Link_MatchedUOM()
		{
			var rate = new RefCusRate();
			var uom = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			rate.RefCusRateUOMs.Add(uom);
			var rateApp = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp);
			rateApp.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			rateApp.Create();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateAppUOM, uom)).Returns(true);
			rateApp.Link(rate, comp.Object, out var result);
			Assert.AreEqual(1, result.OfType<RefCusRateUOM>().Count());
			Assert.AreEqual(uom, rateAppUOM.RefCusRateUOM);
			Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(rateAppUOM));
		}

		[Test]
		public void Link_MultMatchedUOM()
		{
			var rate1 = new RefCusRate();

		}

		[Test]
		public void Link_UnMatchedUOM()
		{
			var rate = new RefCusRate();
			var uom = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			rate.RefCusRateUOMs.Add(uom);
			var rateApp = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp);
			rateApp.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			var newUom = rateApp.Create().OfType<RefCusRateUOM>().FirstOrDefault();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateAppUOM, uom)).Returns(false);
			rateApp.Link(rate, comp.Object, out var result);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(newUom, rateAppUOM.RefCusRateUOM);
			Assert.AreNotEqual(newUom, uom);
			Assert.IsTrue(newUom.RefCusRateApplicabilityUOMs.Contains(rateAppUOM));
			Assert.That(rate.RefCusRateUOMs.All(x => x.ZXG_PK != Guid.Empty));
		}

		[Test]
		public void Link_MatchedEx()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);

			var rateApp = new RefCusRateApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(rateApp);
			rateApp.RefCusExcludedTradeGroupNews.Add(exNew);
			rateApp.Create();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(true);
			rateApp.Link(rate, comp.Object, out var result);
			Assert.AreEqual(3, result.Count());
			Assert.AreEqual(ex, exNew.RefCusExcludedTradeGroup);
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(exNew));
		}

		[Test]
		public void Link_UnMatchedEx()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);

			var rateApp = new RefCusRateApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(rateApp);
			rateApp.RefCusExcludedTradeGroupNews.Add(exNew);
			var newEx = rateApp.Create().OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(false);
			rateApp.Link(rate, comp.Object, out var result);
			Assert.AreEqual(2, result.Count());
			Assert.AreNotEqual(newEx, ex);
			Assert.AreEqual(newEx, exNew.RefCusExcludedTradeGroup);
			Assert.IsTrue(newEx.RefCusExcludedTradeGroupNews.Contains(exNew));
			Assert.That(app.RefCusExcludedTradeGroups.All(x => x.ZZC_PK != Guid.Empty));
		}

		[Test]
		public void Update_RateApp()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability { RefCusRate = rate };
			rate.RefCusApplicabilities.Add(app);
			var rateApp = new RefCusRateApplicability(safeRepository)
			{
				S01_ZZ1_Tariff = Guid.NewGuid(),
				S01_ZZW_TariffNationalCode = Guid.NewGuid(),
				S01_StartDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_EndDate = new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero),
				S01_RateFormula = "8% VFD",
				S01_SelectorFormula = "SELECT 1",
				S01_RateFormulaDerivedFrom = "TARIC",
				S01_RX_NKCurrencyOverride = "AUD",
				S01_ZY1_RateCode = Guid.NewGuid(),
				S01_ZZS_Preference = Guid.NewGuid(),
				S01_ZZZ_NKDataGrouping = "EUN",
				S01_PK = Guid.NewGuid(),
				S01_AdditionalCode = "C999",
				S01_OrderNumber = "S001",
				S01_ZZA_TradeGroup = Guid.NewGuid(),
				S01_ZZA_SecondTradeGroup = Guid.NewGuid(),
			};
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp, rate)).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp, app)).Returns(true);
			rateApp.Create();
			rateApp.Link(rate, comp.Object, out var _);
			rateApp.Update(true);
			Assert.AreEqual(new DateTimeOffset(1900, 01, 01, 0, 0, 0, TimeSpan.Zero), rate.ZZ2_StartDate);
			Assert.AreEqual(new DateTimeOffset(2079, 06, 06, 0, 0, 0, TimeSpan.Zero), rate.ZZ2_EndDate);
			Assert.AreEqual("8% VFD", rate.ZZ2_RateFormula);
			Assert.AreEqual("SELECT 1", rate.ZZ2_SelectorFormula);
			Assert.AreEqual("AUD", rate.ZZ2_RX_NKCurrencyOverride);
			Assert.AreEqual(rateApp.S01_ZY1_RateCode, rate.ZZ2_ZY1_RateCode);
			Assert.AreEqual(rateApp.S01_ZZS_Preference, rate.ZZ2_ZZS_Preference);
			Assert.AreEqual("EUN", rate.ZZ2_ZZZ_NKDataGrouping);
			Assert.AreEqual("C999", app.ZZT_AdditionalCode);
			Assert.AreEqual("S001", app.ZZT_OrderNumber);
			Assert.AreEqual(new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero), app.ZZT_StartDate);
			Assert.AreEqual(new DateTimeOffset(2010, 01, 01, 0, 0, 0, TimeSpan.Zero), app.ZZT_EndDate);
			Assert.AreEqual(rateApp.S01_ZZA_TradeGroup, app.ZZT_ZZA_TradeGroup);
			Assert.AreEqual(rateApp.S01_ZZA_SecondTradeGroup, app.ZZT_ZZA_SecondTradeGroup);
		}

		[Test]
		public void Update_RateAppUOM()
		{
			var rate = new RefCusRate();
			var uom = new RefCusRateUOM { RefCusRate = rate, ZXG_UOM = "KG" };
			rate.RefCusRateUOMs.Add(uom);
			var rateApp = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp) { S02_UOM = "G" };
			rateApp.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			rateApp.Create();
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp, rate)).Returns(true);
			comp.Setup(x => x.IsIdentical(rateAppUOM, uom)).Returns(true);
			rateApp.Link(rate, comp.Object, out var _);
			rateApp.Update(false);
			Assert.AreEqual("G", uom.ZXG_UOM);
		}

		[Test]
		public void Update_RateAppEx()
		{
			var rate = new RefCusRate();
			var app = new RefCusApplicability { RefCusRate = rate };
			rate.RefCusApplicabilities.Add(app);
			var ex = new RefCusExcludedTradeGroup { RefCusApplicability = app, ZZC_ZZA_TradeGroup = Guid.NewGuid() };
			app.RefCusExcludedTradeGroups.Add(ex);
			var rateApp = new RefCusRateApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(rateApp) { S03_ZZA_TradeGroup = Guid.NewGuid() };
			rateApp.RefCusExcludedTradeGroupNews.Add(exNew);
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp, rate)).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp, app)).Returns(true);
			comp.Setup(x => x.IsIdentical(exNew, ex)).Returns(true);
			rateApp.Create();
			rateApp.Link(rate, comp.Object, out var _);
			Assert.AreNotEqual(exNew.S03_ZZA_TradeGroup, ex.ZZC_ZZA_TradeGroup);
			rateApp.Update(false);
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, ex.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void Create_RateApp()
		{
			var rateApp = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp);
			rateApp.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			var newEx = new RefCusExcludedTradeGroupNew(rateApp);
			rateApp.RefCusExcludedTradeGroupNews.Add(newEx);
			var results = rateApp.Create();
			Assert.AreEqual(4, results.Count());
			var app = results.OfType<RefCusApplicability>().FirstOrDefault();
			var rate = results.OfType<RefCusRate>().FirstOrDefault();
			Assert.IsNull(app.RefCusRate);
			Assert.AreEqual(app.ZZT_ZZ2_Rate, rate.ZZ2_PK);
			Assert.IsTrue(rate.RefCusApplicabilities.Contains(app));
			var uom = results.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.AreEqual(uom.ZXG_ZZ2_Rate, rate.ZZ2_PK);
			Assert.IsTrue(rate.RefCusRateUOMs.Contains(uom));
			Assert.IsNull(uom.RefCusRate);
			var ex = results.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(ex.ZZC_ZZT_Applicability, app.ZZT_PK);
			Assert.IsTrue(app.RefCusExcludedTradeGroups.Contains(ex));
			Assert.IsNull(ex.RefCusApplicability);
		}

		[Test]
		public void Create_MultiRateApps_SingleRateUom()
		{
			var rate = new RefCusRate();
			var app1 = new RefCusApplicability { RefCusRate = rate };
			var app2 = new RefCusApplicability { RefCusRate = rate };
			var app3 = new RefCusApplicability { RefCusRate = rate };

			var rateApp1 = new RefCusRateApplicability(rate, app1, safeRepository);
			var rateAppUOM1 = new RefCusRateApplicabilityUOM(rateApp1) { S02_UOM = "DTN" };
			rateApp1.RefCusRateApplicabilityUOMs.Add(rateAppUOM1);
			var result1 = rateApp1.Create();
			Assert.AreEqual(1, result1.Count());
			var R_uom1 = result1.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.AreEqual(R_uom1.ZXG_ZZ2_Rate, rate.ZZ2_PK);
			Assert.IsNull(R_uom1.RefCusRate);

			var rateApp2 = new RefCusRateApplicability(rate, app2, safeRepository);
			var rateAppUOM2 = new RefCusRateApplicabilityUOM(rateApp2) { S02_UOM = "DTNE" };
			rateApp2.RefCusRateApplicabilityUOMs.Add(rateAppUOM2);
			var result2 = rateApp2.Create();
			Assert.AreEqual(1, result2.Count());
			var R_uom2 = result2.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.AreEqual(R_uom2.ZXG_ZZ2_Rate, rate.ZZ2_PK);
			Assert.IsNull(R_uom2.RefCusRate);

			var rateApp3 = new RefCusRateApplicability(rate, app3, safeRepository);
			var rateAppUOM3 = new RefCusRateApplicabilityUOM(rateApp3) { S02_UOM = "DTNE" };
			rateApp3.RefCusRateApplicabilityUOMs.Add(rateAppUOM3);
			var result3 = rateApp3.Create();
			Assert.AreEqual(0, result3.Count());
		}

		[Test]
		public void Create_RateApp_WithTariff()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rateApp = new RefCusRateApplicability(safeRepository);
			rateApp.S01_ZZ1_Tariff = tariff.ZZ1_PK;
			var results1 = rateApp.Create();
			var rate1 = results1.OfType<RefCusRate>().FirstOrDefault();
			Assert.IsNotNull(rate1);
			Assert.AreEqual(tariff.ZZ1_PK, rate1.ZZ2_ZZ1_Tariff);
		}

		[Test]
		public void Create_RateApp_WithTariffNationalCode()
		{
			var tariffNationalCode = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid() };
			var rateApp = new RefCusRateApplicability(safeRepository);
			rateApp.S01_ZZW_TariffNationalCode = tariffNationalCode.ZZW_PK;
			var results2 = rateApp.Create();
			var rate2 = results2.OfType<RefCusRate>().FirstOrDefault();
			Assert.IsNotNull(rate2);
			Assert.AreEqual(tariffNationalCode.ZZW_PK, rate2.ZZ2_ZZW_TariffNationalCode);
		}

		[Test]
		public void Unlink()
		{
			var rateApp = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp);
			rateApp.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			var newEx = new RefCusExcludedTradeGroupNew(rateApp);
			rateApp.RefCusExcludedTradeGroupNews.Add(newEx);
			var created = rateApp.Create();
			Assert.IsNotNull(rateApp.RefCusApplicability);
			var app = created.OfType<RefCusApplicability>().FirstOrDefault();
			Assert.IsTrue(app.RefCusRateApplicabilities.Contains(rateApp));
			Assert.IsNotNull(rateAppUOM.RefCusRateUOM);
			var uom = created.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(rateAppUOM));
			Assert.IsNotNull(newEx.RefCusExcludedTradeGroup);
			var ex = created.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(newEx));

			var unlinks = rateApp.Unlink();
			Assert.IsTrue(unlinks.Contains(app));
			Assert.IsTrue(unlinks.Contains(uom));
			Assert.IsTrue(unlinks.Contains(ex));
			Assert.IsNull(rateApp.RefCusApplicability);
			Assert.IsFalse(app.RefCusRateApplicabilities.Contains(rateApp));
			Assert.IsNull(rateAppUOM.RefCusRateUOM);
			Assert.IsFalse(uom.RefCusRateApplicabilityUOMs.Contains(rateAppUOM));
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
