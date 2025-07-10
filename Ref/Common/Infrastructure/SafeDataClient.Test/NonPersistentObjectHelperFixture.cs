using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	internal class NonPersistentObjectHelperFixture
	{
		[Test]
		public void BuildRefCusRateApplicabilities_RateApp()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate();
			var rateApps = NonPersistentObjectHelper.Build(new[] { (rate1, new[] { app1 }), (rate2, new[] { app2, app3 }) }, safeRepository).Cast<RefCusRateApplicability>();
			Assert.AreEqual(3, rateApps.Count());
			foreach (var app in new[] { app1, app2, app3 })
			{
				var rateApp = rateApps.FirstOrDefault(x => x.RefCusApplicability == app);
				Assert.IsNotNull(rateApp);
				Assert.AreEqual(app, rateApp.RefCusApplicability);
				Assert.IsTrue(app.RefCusRateApplicabilities.Contains(rateApp));
			}
		}

		[Test]
		public void BuildRefCusRateApplicabilities_RateAppUOM()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var uom1 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var uom2 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			var uom3 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate();
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate1.RefCusRateUOMs.Add(uom1);
			rate2.RefCusRateUOMs.Add(uom2);
			rate2.RefCusRateUOMs.Add(uom3);
			var rateAppUOMs = NonPersistentObjectHelper.Build(new[] { (rate1, new[] { app1 }), (rate2, new[] { app2 }), (rate3, new[] { app3 }) }, safeRepository).Cast<RefCusRateApplicability>().SelectMany(x => x.RefCusRateApplicabilityUOMs);
			Assert.AreEqual(3, rateAppUOMs.Count());
			foreach (var uom in new[] { uom1, uom2, uom2 })
			{
				var rateAppUOM = rateAppUOMs.FirstOrDefault(x => x.RefCusRateUOM == uom);
				Assert.IsNotNull(rateAppUOM);
				Assert.AreEqual(uom, rateAppUOM.RefCusRateUOM);
				Assert.IsTrue(uom.RefCusRateApplicabilityUOMs.Contains(rateAppUOM));
			}
		}

		[Test]
		public void BuildRefCusRateApplicabilities_ExcludedTradeGroupNew()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var ex1 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var ex2 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var ex3 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			app1.RefCusExcludedTradeGroups.Add(ex1);
			app2.RefCusExcludedTradeGroups.Add(ex2);
			app2.RefCusExcludedTradeGroups.Add(ex3);
			var excludedNews = NonPersistentObjectHelper.Build(new[] { (rate1, new[] { app1, app2, app3 }) }, safeRepository).Cast<RefCusRateApplicability>().SelectMany(x => x.RefCusExcludedTradeGroupNews);
			Assert.AreEqual(3, excludedNews.Count());
			foreach (var ex in new[] { ex1, ex2, ex3 })
			{
				var excludedNew = excludedNews.FirstOrDefault(x => x.RefCusExcludedTradeGroup == ex);
				Assert.IsNotNull(excludedNew);
				Assert.AreEqual(ex, excludedNew.RefCusExcludedTradeGroup);
				Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(excludedNew));
			}
		}

		[Test]
		public void BuildRefCusRateApplicabilities_Tariff()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);

			var tariff = new RefCusTariff();
			tariff.RefCusRates.Add(rate1);
			tariff.RefCusRates.Add(rate2);
			tariff.RefCusRates.Add(rate3);

			tariff.BuildNonPersistentObjects(safeRepository);
			foreach (var app in new[] { app1, app2, app3 })
			{
				var rateApp = tariff.RefCusRateApplicabilities.FirstOrDefault(x => x.RefCusApplicability == app);
				Assert.IsNotNull(rateApp);
				Assert.AreEqual(app, rateApp.RefCusApplicability);
			}
		}

		[Test]
		public void BuildRefCusRateApplicabilities_TariffNationalCode()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);

			var tariffNationalCode = new RefCusTariffNationalCode();
			tariffNationalCode.RefCusRates.Add(rate1);
			tariffNationalCode.RefCusRates.Add(rate2);
			tariffNationalCode.RefCusRates.Add(rate3);

			tariffNationalCode.BuildNonPersistentObjects(safeRepository);
			foreach (var app in new[] { app1, app2, app3 })
			{
				var rateApp = tariffNationalCode.RefCusRateApplicabilities.FirstOrDefault(x => x.RefCusApplicability == app);
				Assert.IsNotNull(rateApp);
				Assert.AreEqual(app, rateApp.RefCusApplicability);
			}
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2RateIdenticalRateApp_BothLinked1Rate()
		{
			var rate = new RefCusRate { ZZ2_RateFormula = "8%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app1);
			rate.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate, app1, safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(rate, app2, safeRepository) { S01_RateFormula = "9%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, rate)).Returns(false);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			Assert.AreEqual(1, results.UpdatedObjects.OfType<RefCusRate>().Count());
			var rateResult = results.UpdatedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2RateIdenticalRateApp_LinkedTo2DifferentRates()
		{
			var rate1 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var rate2 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_RateFormula = "9%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1, rate2 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count());
			Assert.AreEqual(1, results.UpdatedObjects.OfType<RefCusRate>().Count());
			var rateResult = results.UpdatedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2RateIdenticalRateApp_OneLinkedOneIsNot()
		{
			var rate1 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate1.RefCusApplicabilities.Add(app1);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "9%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count(), "Rate and App for rateApp2 are created");
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count(), "Rate for rateApp2 is unlinked");
			var rateResult = results.UpdatedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2RateIdenticalRateApp_BothAreUnlinked()
		{
			var rateApp1 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "9%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new RefCusRate[0], new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(4, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count(), "Rate is created then deleted");
			var rateResult = results.AddedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2NonRateIdenticalRateApp_BothLinked1Rate()
		{
			var rate = new RefCusRate { ZZ2_RateFormula = "7%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate.RefCusApplicabilities.Add(app1);
			rate.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate, app1, safeRepository) { S01_RateFormula = "8%" };
			var rateApp2 = new RefCusRateApplicability(rate, app2, safeRepository) { S01_RateFormula = "9%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "8%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count());
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(2, results.UnlinkedObjects.Count());
			var rateResult = results.UpdatedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("8%", rateResult.ZZ2_RateFormula);
			rateResult = results.AddedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2NonRateIdenticalRateApp_LinkedTo2DifferentRates()
		{
			var rate1 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var rate2 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_RateFormula = "10%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "10%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1, rate2 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(4, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var rateResults = results.UpdatedObjects.OfType<RefCusRate>();
			Assert.IsNotNull(rateResults.FirstOrDefault(x => x.ZZ2_RateFormula == "10%"));
			Assert.IsNotNull(rateResults.FirstOrDefault(x => x.ZZ2_RateFormula == "9%"));
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2NonRateIdenticalRateApp_OneLinkedOneIsNot()
		{
			var rate1 = new RefCusRate { ZZ2_RateFormula = "8%" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rate1.RefCusApplicabilities.Add(app1);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "10%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "10%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count());
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var rateResult = results.UpdatedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("9%", rateResult.ZZ2_RateFormula);
			rateResult = results.AddedObjects.OfType<RefCusRate>().FirstOrDefault();
			Assert.AreEqual("10%", rateResult.ZZ2_RateFormula);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_2NonRateIdenticalRateApp_BothAreUnlinked()
		{
			var rateApp1 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "9%" };
			var rateApp2 = new RefCusRateApplicability(safeRepository) { S01_RateFormula = "10%" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "9%"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "10%"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new RefCusRate[0], new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.AreEqual(4, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var rateResults = results.AddedObjects.OfType<RefCusRate>();
			Assert.IsNotNull(rateResults.FirstOrDefault(x => x.ZZ2_RateFormula == "9%"));
			Assert.IsNotNull(rateResults.FirstOrDefault(x => x.ZZ2_RateFormula == "10%"));
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_RateAppUOM_Linked()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability();
			var uom = new RefCusRateUOM { ZXG_UOM = "G" };
			rate1.RefCusApplicabilities.Add(app1);
			rate1.RefCusRateUOMs.Add(uom);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp1) { S02_UOM = "KG", S02_S01_RateApplicability = rateApp1.S01_PK, RefCusRateUOM = uom };
			rateApp1.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1 }, new[] { rateApp1 }, new[] { rateApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var uomResult = results.UpdatedObjects.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.AreEqual("KG", uomResult.ZXG_UOM);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_RelinkExisting()
		{
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_RateFormula = "10", ZZ2_RateFormulaDerivedFrom = "rate1" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_AdditionalCode = "app1" };
			var uom1 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_UOM = "uom1" };
			rate1.RefCusApplicabilities.Add(app1);
			rate1.RefCusRateUOMs.Add(uom1);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_PK = Guid.NewGuid(), S01_RateFormula = "15", S01_RateFormulaDerivedFrom = "rateApp1" };
			var rateAppUOM1 = new RefCusRateApplicabilityUOM(rateApp1) { S02_PK = Guid.NewGuid(), S02_UOM = "newUOM1" };
			rateApp1.RefCusRateApplicabilityUOMs.Add(rateAppUOM1);

			var rate2 = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_RateFormula = "5", ZZ2_RateFormulaDerivedFrom = "rate2" };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_AdditionalCode = "app2" };
			var uom2 = new RefCusRateUOM() { ZXG_PK = Guid.NewGuid(), ZXG_UOM = "uom2" };
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusRateUOMs.Add(uom2);
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository) { S01_PK = Guid.NewGuid(), S01_RateFormula = "15" };
			var rateAppUOM2 = new RefCusRateApplicabilityUOM(rateApp2) { S02_PK = Guid.NewGuid(), S02_UOM = "newUOM2" };
			rateApp2.RefCusRateApplicabilityUOMs.Add(rateAppUOM2);

			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "15"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "15"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusApplicability>(y => y.ZZT_AdditionalCode == "app1"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusApplicability>(y => y.ZZT_AdditionalCode == "app2"))).Returns(true);

			var results = NonPersistentObjectHelper.Merge(new[] { rate1, rate2 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.Contains(rateAppUOM1.RefCusRateUOM, results.AddedObjects.ToArray(), "newUOM1 is created");
			Assert.Contains(rateAppUOM2.RefCusRateUOM, results.AddedObjects.ToArray(), "newUOM2 is created and relinked later");
			Assert.Contains(rate2, results.UnlinkedObjects.ToList(), "rate2 is unlinked because it will be relinked later");
			Assert.Contains(app2, rate1.RefCusApplicabilities, "App2 is linked to rate1");
			Assert.Contains(uom2, rate1.RefCusRateUOMs, "uom2 is linked to rate1");
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_RelinkNew()
		{
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_RateFormula = "10", ZZ2_RateFormulaDerivedFrom = "rate1" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), ZZT_AdditionalCode = "app1" };
			var uom1 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), ZXG_UOM = "uom1" };
			rate1.RefCusApplicabilities.Add(app1);
			rate1.RefCusRateUOMs.Add(uom1);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository) { S01_PK = Guid.NewGuid(), S01_RateFormula = "15", S01_RateFormulaDerivedFrom = "rateApp1" };
			var rateAppUOM1 = new RefCusRateApplicabilityUOM(rateApp1) { S02_PK = Guid.NewGuid(), S02_UOM = "newUOM1" };
			rateApp1.RefCusRateApplicabilityUOMs.Add(rateAppUOM1);

			var rateApp2 = new RefCusRateApplicability(safeRepository) { S01_PK = Guid.NewGuid(), S01_RateFormula = "15" };
			var rateAppUOM2 = new RefCusRateApplicabilityUOM(rateApp2) { S02_PK = Guid.NewGuid(), S02_UOM = "newUOM2" };
			rateApp2.RefCusRateApplicabilityUOMs.Add(rateAppUOM2);

			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "15"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusRate>(y => y.ZZ2_RateFormula == "15"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp1, It.Is<RefCusApplicability>(y => y.ZZT_AdditionalCode == "app1"))).Returns(true);
			comp.Setup(x => x.IsIdentical(rateApp2, It.Is<RefCusApplicability>(y => y.ZZT_AdditionalCode == "app2"))).Returns(true);

			var results = NonPersistentObjectHelper.Merge(new[] { rate1 }, new[] { rateApp1, rateApp2 }, new[] { rateApp1, rateApp2 }, comp.Object);
			Assert.Contains(rateAppUOM1.RefCusRateUOM, results.AddedObjects.ToArray(), "newUOM1 is created");
			Assert.Contains(rateAppUOM2.RefCusRateUOM, results.AddedObjects.ToArray(), "newUOM2 is created and relinked later");
			Assert.AreEqual(rate1.ZZ2_PK, rateAppUOM2.RefCusRateUOM.ZXG_ZZ2_Rate, "newUOM2 is relinked to rate1");
			Assert.AreEqual(rate1.ZZ2_PK, rateApp2.RefCusApplicability.ZZT_ZZ2_Rate, "App2 is linked to rate1");
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_RateAppUOM_UnLinked()
		{
			var rateApp1 = new RefCusRateApplicability(safeRepository);
			var rateAppUOM = new RefCusRateApplicabilityUOM(rateApp1) { S02_UOM = "KG", S02_S01_RateApplicability = rateApp1.S01_PK };
			rateApp1.RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			var results = NonPersistentObjectHelper.Merge(new RefCusRate[0], new[] { rateApp1 }, new[] { rateApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(3, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var uomResult = results.AddedObjects.OfType<RefCusRateUOM>().FirstOrDefault();
			Assert.AreEqual("KG", uomResult.ZXG_UOM);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_ExNew_Linked()
		{
			var rate1 = new RefCusRate();
			var app1 = new RefCusApplicability();
			var ex = new RefCusExcludedTradeGroup { ZZC_ZZA_TradeGroup = Guid.NewGuid() };
			rate1.RefCusApplicabilities.Add(app1);
			app1.RefCusExcludedTradeGroups.Add(ex);
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(rateApp1) { S03_ZZA_TradeGroup = Guid.NewGuid(), S03_S01_RateApplicability = rateApp1.S01_PK, RefCusExcludedTradeGroup = ex };
			rateApp1.RefCusExcludedTradeGroupNews.Add(exNew);
			var results = NonPersistentObjectHelper.Merge(new[] { rate1 }, new[] { rateApp1 }, new[] { rateApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var exResult = results.UpdatedObjects.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, exResult.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void ConsolidateRefCusRateApplicabilities_ExNew_UnLinked()
		{
			var rateApp1 = new RefCusRateApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(rateApp1) { S03_ZZA_TradeGroup = Guid.NewGuid(), S03_S01_RateApplicability = rateApp1.S01_PK };
			rateApp1.RefCusExcludedTradeGroupNews.Add(exNew);
			var results = NonPersistentObjectHelper.Merge(new RefCusRate[0], new[] { rateApp1 }, new[] { rateApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(3, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var exResult = results.AddedObjects.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, exResult.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_CondApp()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition();
			var condApps = NonPersistentObjectHelper.Build(new[] { (cond1, new[] { app1 }), (cond2, new[] { app2, app3 }) }, safeRepository).Cast<RefCusConditionApplicability>();
			Assert.AreEqual(3, condApps.Count());
			foreach (var app in new[] { app1, app2, app3 })
			{
				var condApp = condApps.FirstOrDefault(x => x.RefCusApplicability == app);
				Assert.IsNotNull(condApp);
				Assert.AreEqual(app, condApp.RefCusApplicability);
				Assert.IsTrue(app.RefCusConditionApplicabilities.Contains(condApp));
			}
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_CondAppVal()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var val1 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var val2 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			var val3 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition();
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusConditionValues.Add(val1);
			cond2.RefCusConditionValues.Add(val2);
			cond2.RefCusConditionValues.Add(val3);
			var condAppValss = NonPersistentObjectHelper.Build(new[] { (cond1, new[] { app1 }), (cond2, new[] { app2 }), (cond3, new[] { app3 }) }, safeRepository).Cast<RefCusConditionApplicability>().SelectMany(x => x.RefCusConditionApplicabilityValues);
			Assert.AreEqual(3, condAppValss.Count());
			foreach (var val in new[] { val1, val2, val2 })
			{
				var condAppVal = condAppValss.FirstOrDefault(x => x.RefCusConditionValue == val);
				Assert.IsNotNull(condAppVal);
				Assert.AreEqual(val, condAppVal.RefCusConditionValue);
				Assert.IsTrue(val.RefCusConditionApplicabilityValues.Contains(condAppVal));
			}
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_CondAppLang()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var lang1 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var lang2 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			var lang3 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition();
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusConditionLanguages.Add(lang1);
			cond2.RefCusConditionLanguages.Add(lang2);
			cond2.RefCusConditionLanguages.Add(lang3);
			var condAppLangs = NonPersistentObjectHelper.Build(new[] { (cond1, new[] { app1 }), (cond2, new[] { app2 }), (cond3, new[] { app3 }) }, safeRepository).Cast<RefCusConditionApplicability>().SelectMany(x => x.RefCusConditionApplicabilityLanguages);
			Assert.AreEqual(3, condAppLangs.Count());
			foreach (var lang in new[] { lang1, lang2, lang2 })
			{
				var condAppLang = condAppLangs.FirstOrDefault(x => x.RefCusConditionLanguage == lang);
				Assert.IsNotNull(condAppLang);
				Assert.AreEqual(lang, condAppLang.RefCusConditionLanguage);
				Assert.IsTrue(lang.RefCusConditionApplicabilityLanguages.Contains(condAppLang));
			}
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_ExcludedTradeGroupNew()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var ex1 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var ex2 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var ex3 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			app1.RefCusExcludedTradeGroups.Add(ex1);
			app2.RefCusExcludedTradeGroups.Add(ex2);
			app2.RefCusExcludedTradeGroups.Add(ex3);
			var excludedNews = NonPersistentObjectHelper.Build(new[] { (cond1, new[] { app1, app2, app3 }) }, safeRepository).Cast<RefCusConditionApplicability>().SelectMany(x => x.RefCusExcludedTradeGroupNews);
			Assert.AreEqual(3, excludedNews.Count());
			foreach (var ex in new[] { ex1, ex2, ex3 })
			{
				var excludedNew = excludedNews.FirstOrDefault(x => x.RefCusExcludedTradeGroup == ex);
				Assert.IsNotNull(excludedNew);
				Assert.AreEqual(ex, excludedNew.RefCusExcludedTradeGroup);
				Assert.IsTrue(ex.RefCusExcludedTradeGroupNews.Contains(excludedNew));
			}
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_Tariff()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition();
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition();
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond2.RefCusApplicabilities.Add(app3);

			var tariff = new RefCusTariff();
			tariff.RefCusConditions.Add(cond1);
			tariff.RefCusConditions.Add(cond2);
			tariff.RefCusConditions.Add(cond3);

			tariff.BuildNonPersistentObjects(safeRepository);
			foreach (var app in new[] { app1, app2, app3 })
			{
				var condApp = tariff.RefCusConditionApplicabilities.FirstOrDefault(x => x.RefCusApplicability == app);
				Assert.IsNotNull(condApp);
				Assert.AreEqual(app, condApp.RefCusApplicability);
			}
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2CondIdenticalCondApp_BothLinked1Cond()
		{
			var cond = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app1);
			cond.RefCusApplicabilities.Add(app2);
			var condApp1 = new RefCusConditionApplicability(cond, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(cond, app2, safeRepository) { S07_Comment = "BBB" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, cond)).Returns(false);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			Assert.AreEqual(1, results.UpdatedObjects.OfType<RefCusCondition>().Count());
			var condResult = results.UpdatedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2CondIdenticalCondApp_LinkedTo2DifferentConds()
		{
			var cond1 = new RefCusCondition { ZX1_Comment = "AAA" };
			var cond2 = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(cond2, app2, safeRepository) { S07_Comment = "BBB" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1, cond2 }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count());
			Assert.AreEqual(1, results.UpdatedObjects.OfType<RefCusCondition>().Count());
			var condResult = results.UpdatedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2CondIdenticalCondApp_OneLinkedOneIsNot()
		{
			var cond1 = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusApplicabilities.Add(app1);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "BBB" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1 }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count());
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count() , "Cond is created then deleted");
			var condResult = results.UpdatedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2CondIdenticalCondApp_BothAreUnlinked()
		{
			var condApp1 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "BBB" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new RefCusCondition[0], new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(4, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(1, results.UnlinkedObjects.Count(), "Cond is created then deleted");
			var condResult = results.AddedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2NonCondIdenticalCondApp_BothLinked1Cond()
		{
			var cond = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond.RefCusApplicabilities.Add(app1);
			cond.RefCusApplicabilities.Add(app2);
			var condApp1 = new RefCusConditionApplicability(cond, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(cond, app2, safeRepository) { S07_Comment = "CCC" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "CCC"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count());
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(2, results.UnlinkedObjects.Count());
			var condResult = results.UpdatedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
			condResult = results.AddedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("CCC", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2NonCondIdenticalCondApp_LinkedTo2DifferentConds()
		{
			var cond1 = new RefCusCondition { ZX1_Comment = "AAA" };
			var cond2 = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(cond2, app2, safeRepository) { S07_Comment = "CCC" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "CCC"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1, cond2 }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(4, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var condResults = results.UpdatedObjects.OfType<RefCusCondition>();
			Assert.IsNotNull(condResults.FirstOrDefault(x => x.ZX1_Comment == "BBB"));
			Assert.IsNotNull(condResults.FirstOrDefault(x => x.ZX1_Comment == "CCC"));
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2NonCondIdenticalCondApp_OneLinkedOneIsNot()
		{
			var cond1 = new RefCusCondition { ZX1_Comment = "AAA" };
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			cond1.RefCusApplicabilities.Add(app1);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository) { S07_Comment = "BBB" };
			var condApp2 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "CCC" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "CCC"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1 }, new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(2, results.AddedObjects.Count());
			Assert.AreEqual(2, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var condResult = results.UpdatedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("BBB", condResult.ZX1_Comment);
			condResult = results.AddedObjects.OfType<RefCusCondition>().FirstOrDefault();
			Assert.AreEqual("CCC", condResult.ZX1_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_2NonCondIdenticalCondApp_BothAreUnlinked()
		{
			var condApp1 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "AAA" };
			var condApp2 = new RefCusConditionApplicability(safeRepository) { S07_Comment = "BBB" };
			var comp = new Mock<INonPersistentBusinessObjectComparison>();
			comp.Setup(x => x.IsIdentical(condApp1, It.Is<RefCusCondition>(y => y.ZX1_Comment == "AAA"))).Returns(true);
			comp.Setup(x => x.IsIdentical(condApp2, It.Is<RefCusCondition>(y => y.ZX1_Comment == "BBB"))).Returns(true);
			var results = NonPersistentObjectHelper.Merge(new RefCusCondition[0], new[] { condApp1, condApp2 }, new[] { condApp1, condApp2 }, comp.Object);
			Assert.AreEqual(4, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var condResults = results.AddedObjects.OfType<RefCusCondition>();
			Assert.IsNotNull(condResults.FirstOrDefault(x => x.ZX1_Comment == "AAA"));
			Assert.IsNotNull(condResults.FirstOrDefault(x => x.ZX1_Comment == "BBB"));
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_CondAppVal_Linked()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability();
			var val = new RefCusConditionValue { ZX3_Value = "C084" };
			cond1.RefCusApplicabilities.Add(app1);
			cond1.RefCusConditionValues.Add(val);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(val, condApp1) { S08_Value = "U045", S08_S07_ConditionApplicability = condApp1.S07_PK };
			condApp1.RefCusConditionApplicabilityValues.Add(condAppVal);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1 }, new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var valResult = results.UpdatedObjects.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.AreEqual("U045", valResult.ZX3_Value);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_CondAppLang_Linked()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability();
			var lang = new RefCusConditionLanguage { ZXJ_Comment = "AAA" };
			cond1.RefCusApplicabilities.Add(app1);
			cond1.RefCusConditionLanguages.Add(lang);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var condAppLanguage = new RefCusConditionApplicabilityLanguage(lang, condApp1) { S09_Comment = "BBB", S09_S07_ConditionApplicability = condApp1.S07_PK };
			condApp1.RefCusConditionApplicabilityLanguages.Add(condAppLanguage);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1 }, new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var langResult = results.UpdatedObjects.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.AreEqual("BBB", langResult.ZXJ_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_ExNew_Linked()
		{
			var cond1 = new RefCusCondition();
			var app1 = new RefCusApplicability();
			var ex = new RefCusExcludedTradeGroup { ZZC_ZZA_TradeGroup = Guid.NewGuid() };
			cond1.RefCusApplicabilities.Add(app1);
			app1.RefCusExcludedTradeGroups.Add(ex);
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(condApp1) { S03_ZZA_TradeGroup = Guid.NewGuid(), S03_S07_ConditionApplicability = condApp1.S07_PK, RefCusExcludedTradeGroup = ex };
			condApp1.RefCusExcludedTradeGroupNews.Add(exNew);
			var results = NonPersistentObjectHelper.Merge(new[] { cond1 }, new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(0, results.AddedObjects.Count());
			Assert.AreEqual(3, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var exResult = results.UpdatedObjects.OfType<RefCusExcludedTradeGroup>().FirstOrDefault();
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, exResult.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_CondAppVal_UnLinked()
		{
			var condApp1 = new RefCusConditionApplicability(safeRepository);
			var condAppVal = new RefCusConditionApplicabilityValue(condApp1) { S08_Value = "C084", S08_S07_ConditionApplicability = condApp1.S07_PK };
			condApp1.RefCusConditionApplicabilityValues.Add(condAppVal);
			var results = NonPersistentObjectHelper.Merge(new RefCusCondition[0], new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(3, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var valResult = results.AddedObjects.OfType<RefCusConditionValue>().FirstOrDefault();
			Assert.AreEqual("C084", valResult.ZX3_Value);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_CondAppLang_UnLinked()
		{
			var condApp1 = new RefCusConditionApplicability(safeRepository);
			var condAppLang = new RefCusConditionApplicabilityLanguage(condApp1) { S09_Comment = "AAA", S09_S07_ConditionApplicability = condApp1.S07_PK };
			condApp1.RefCusConditionApplicabilityLanguages.Add(condAppLang);
			var results = NonPersistentObjectHelper.Merge(new RefCusCondition[0], new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(3, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var langResult = results.AddedObjects.OfType<RefCusConditionLanguage>().FirstOrDefault();
			Assert.AreEqual("AAA", langResult.ZXJ_Comment);
		}

		[Test]
		public void ConsolidateRefCusConditionApplicabilities_ExNew_UnLinked()
		{
			var condApp1 = new RefCusConditionApplicability(safeRepository);
			var exNew = new RefCusExcludedTradeGroupNew(condApp1) { S03_ZZA_TradeGroup = Guid.NewGuid(), S03_S07_ConditionApplicability = condApp1.S07_PK };
			condApp1.RefCusExcludedTradeGroupNews.Add(exNew);
			var results = NonPersistentObjectHelper.Merge(new RefCusCondition[0], new[] { condApp1 }, new[] { condApp1 }, new Mock<INonPersistentBusinessObjectComparison>().Object);
			Assert.AreEqual(3, results.AddedObjects.Count());
			Assert.AreEqual(0, results.UpdatedObjects.Count());
			Assert.AreEqual(0, results.UnlinkedObjects.Count());
			var exResult = results.AddedObjects.OfType<RefCusExcludedTradeGroup>().First();
			Assert.AreEqual(exNew.S03_ZZA_TradeGroup, exResult.ZZC_ZZA_TradeGroup);
		}

		[Test]
		public void GetDeletedObjects_RateUOM()
		{
			var rateApp3 = new RefCusRateApplicability(safeRepository);
			var rateApp2 = new RefCusRateApplicability(safeRepository);
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			rate2.RefCusRateApplicabilities.Add(rateApp2);
			rate3.RefCusRateApplicabilities.Add(rateApp3);
			var uom1 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			var uom2 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid() };
			var uom3 = new RefCusRateUOM { ZXG_PK = Guid.NewGuid(), RefCusRateApplicabilityUOMs = new HashSet<RefCusRateApplicabilityUOM> { new RefCusRateApplicabilityUOM(rateApp3) } };
			rate1.RefCusRateUOMs.Add(uom1);
			rate2.RefCusRateUOMs.Add(uom2);
			rate3.RefCusRateUOMs.Add(uom3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { rate1, rate2, rate3, uom1, uom2, uom3 });
			Assert.That(result, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new List<Guid> { rate1.ZZ2_PK, uom1.ZXG_PK, uom2.ZXG_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(rate2.RefCusRateUOMs, Has.Count.EqualTo(0));
			Assert.That(rate3.RefCusRateUOMs, Has.Count.EqualTo(1));
		}

		[Test]
		public void GetDeletedObjects_ExcludedTradeGroup()
		{
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var ex1 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var ex2 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid() };
			var ex3 = new RefCusExcludedTradeGroup { ZZC_PK = Guid.NewGuid(), RefCusExcludedTradeGroupNews = new HashSet<RefCusExcludedTradeGroupNew> { new RefCusExcludedTradeGroupNew(new RefCusRateApplicability(safeRepository)) } };
			app1.RefCusExcludedTradeGroups.Add(ex1);
			app2.RefCusExcludedTradeGroups.Add(ex2);
			app3.RefCusExcludedTradeGroups.Add(ex3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { app1, app3, ex1, ex2, ex3 });
			Assert.That(result, Has.Count.EqualTo(4));
			CollectionAssert.AreEquivalent(new List<Guid> { ex1.ZZC_PK, ex2.ZZC_PK, app1.ZZT_PK, app3.ZZT_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(app1.RefCusExcludedTradeGroups, Has.Count.EqualTo(0));
			Assert.That(app2.RefCusExcludedTradeGroups, Has.Count.EqualTo(1));
			Assert.That(app3.RefCusExcludedTradeGroups, Has.Count.EqualTo(1));
		}

		[Test]
		public void GetDeletedObjects_Rate_Applicability()
		{
			var rateApp3 = new RefCusRateApplicability(safeRepository);
			var rateApp2 = new RefCusRateApplicability(safeRepository);
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid() };
			rate2.RefCusRateApplicabilities.Add(rateApp2);
			rate3.RefCusRateApplicabilities.Add(rateApp3);
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), RefCusRateApplicabilities = new HashSet<RefCusRateApplicability> { rateApp3 } };
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate3.RefCusApplicabilities.Add(app3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { rate1, rate2, rate3, app1, app2, app3 });
			Assert.That(result, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new List<Guid> { app1.ZZT_PK, app2.ZZT_PK, rate1.ZZ2_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(rate2.RefCusApplicabilities, Has.Count.EqualTo(0));
			Assert.That(rate3.RefCusApplicabilities, Has.Count.EqualTo(1));
		}

		[Test]
		public void GetDeletedObjects_Condition_Applicability()
		{
			var condApp3 = new RefCusConditionApplicability(safeRepository);
			var condApp2 = new RefCusConditionApplicability(safeRepository);
			var cond1 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			cond3.RefCusConditionApplicabilities.Add(condApp3);
			cond2.RefCusConditionApplicabilities.Add(condApp2);
			var app1 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app2 = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			var app3 = new RefCusApplicability { ZZT_PK = Guid.NewGuid(), RefCusConditionApplicabilities = new HashSet<RefCusConditionApplicability> { condApp3 } };
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond3.RefCusApplicabilities.Add(app3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { cond1, cond2, cond3, app1, app2, app3 });
			Assert.That(result, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new List<Guid> { cond1.ZX1_PK, app1.ZZT_PK, app2.ZZT_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(cond2.RefCusApplicabilities, Has.Count.EqualTo(0));
			Assert.That(cond3.RefCusApplicabilities, Has.Count.EqualTo(1));
		}

		[Test]
		public void GetDeletedObjects_ConditionValue()
		{
			var condApp3 = new RefCusConditionApplicability(safeRepository);
			var condApp2 = new RefCusConditionApplicability(safeRepository);
			var cond1 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			cond3.RefCusConditionApplicabilities.Add(condApp3);
			cond2.RefCusConditionApplicabilities.Add(condApp2);
			var val1 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			var val2 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
			var val3 = new RefCusConditionValue { ZX3_PK = Guid.NewGuid(), RefCusConditionApplicabilityValues = new HashSet<RefCusConditionApplicabilityValue> { new RefCusConditionApplicabilityValue(condApp3) } };
			cond1.RefCusConditionValues.Add(val1);
			cond2.RefCusConditionValues.Add(val2);
			cond3.RefCusConditionValues.Add(val3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { cond1, cond2, cond3, val1, val2, val3 });
			Assert.That(result, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new List<Guid> { cond1.ZX1_PK, val1.ZX3_PK, val2.ZX3_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(cond2.RefCusConditionValues, Has.Count.EqualTo(0));
			Assert.That(cond3.RefCusConditionValues, Has.Count.EqualTo(1));
		}

		[Test]
		public void GetDeletedObjects_ConditionLanguage()
		{
			var condApp3 = new RefCusConditionApplicability(safeRepository);
			var condApp2 = new RefCusConditionApplicability(safeRepository);
			var cond1 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond2 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			var cond3 = new RefCusCondition { ZX1_PK = Guid.NewGuid() };
			cond3.RefCusConditionApplicabilities.Add(condApp3);
			cond2.RefCusConditionApplicabilities.Add(condApp2);
			var lang1 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			var lang2 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
			var lang3 = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid(), RefCusConditionApplicabilityLanguages = new HashSet<RefCusConditionApplicabilityLanguage> { new RefCusConditionApplicabilityLanguage(condApp3) } };
			cond1.RefCusConditionLanguages.Add(lang1);
			cond2.RefCusConditionLanguages.Add(lang2);
			cond3.RefCusConditionLanguages.Add(lang3);

			var result = NonPersistentObjectHelper.GetDeleteObjects(new List<object> { cond1, cond3, lang1, lang2, lang3 });
			Assert.That(result, Has.Count.EqualTo(3));
			CollectionAssert.AreEquivalent(new List<Guid> { cond1.ZX1_PK, lang1.ZXJ_PK, lang2.ZXJ_PK }, result.Select(x => x.GetPKValue()));
			Assert.That(cond1.RefCusConditionLanguages, Has.Count.EqualTo(0));
			Assert.That(cond3.RefCusConditionLanguages, Has.Count.EqualTo(1));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
