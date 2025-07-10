using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.OData.Client;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class SafeRepositoryFixture
	{
		[Test]
		public void SavePersistentObjects_Tariff_RateApp()
		{
			var container = new Mock<IContainer>();
			var repo = new SafeRepository(container.Object);
			repo.NonPersistentObjecComparison = new Mock<INonPersistentBusinessObjectComparison>().Object;

			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusRates.Add(rate);
			var app = new RefCusApplicability();
			rate.RefCusApplicabilities.Add(app);
			tariff.BuildNonPersistentObjects(repo);

			container.Setup(x => x.EntityStates).Returns(new[] { Tuple.Create((object)tariff, EntityStates.Added) });

			repo.Update(tariff.RefCusRateApplicabilities.FirstOrDefault());

			var rateApp = new RefCusRateApplicability(repo) { S01_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusRateApplicabilities.Add(rateApp);
			repo.Add(rateApp);

			var rateDelete = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff.ZZ1_PK };
			var appDelete = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rateDelete.RefCusApplicabilities.Add(appDelete);
			var rateAppDelete = new RefCusRateApplicability(rateDelete, appDelete, repo) { S01_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusRateApplicabilities.Add(rateAppDelete);
			repo.Delete(rateAppDelete);

			repo.SavePersistentObjects();
			var results = repo.GetAllPersistentObjects();

			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusRateUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusApplicabilityUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.UpdateObject(It.IsAny<object>()), Times.Exactly(2));
			container.Verify(x => x.DeleteObject(It.IsAny<object>()), Times.Exactly(2));

			Assert.That(results.Count(), Is.EqualTo(4));
			Assert.That(results.OfType<RefCusRate>().Count(), Is.EqualTo(2));
			Assert.That(results.OfType<RefCusApplicability>().Count(), Is.EqualTo(2));
		}

		[Test]
		public void SavePersistentObjects_Tariff_ConditionApp()
		{
			var container = new Mock<IContainer>();
			var repo = new SafeRepository(container.Object);
			repo.NonPersistentObjecComparison = new Mock<INonPersistentBusinessObjectComparison>().Object;

			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var cond = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusConditions.Add(cond);
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			tariff.BuildNonPersistentObjects(repo);
			app.RefCusCondition = cond;
			container.Setup(x => x.EntityStates).Returns(new[] { Tuple.Create((object)tariff, EntityStates.Added) });

			repo.Update(tariff.RefCusConditionApplicabilities.FirstOrDefault());

			var condApp = new RefCusConditionApplicability(repo) { S07_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusConditionApplicabilities.Add(condApp);
			repo.Add(condApp);

			var condDelete = new RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = tariff.ZZ1_PK };
			var appDelete = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			condDelete.RefCusApplicabilities.Add(appDelete);
			var condAppDelete = new RefCusConditionApplicability(condDelete, appDelete, repo) { S07_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusConditionApplicabilities.Add(condAppDelete);
			repo.Delete(condAppDelete);

			repo.SavePersistentObjects();
			var results = repo.GetAllPersistentObjects();
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusConditionUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusApplicabilityUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.UpdateObject(It.IsAny<object>()), Times.Exactly(2));
			container.Verify(x => x.DeleteObject(It.IsAny<object>()), Times.Exactly(2));

			Assert.That(results.Count(), Is.EqualTo(4));
			Assert.That(results.OfType<RefCusCondition>().Count(), Is.EqualTo(2));
			Assert.That(results.OfType<RefCusApplicability>().Count(), Is.EqualTo(2));
		}

		[Test]
		public void SavePersistentObjects_TariffNationalCode()
		{
			var container = new Mock<IContainer>();
			var repo = new SafeRepository(container.Object);
			repo.NonPersistentObjecComparison = new Mock<INonPersistentBusinessObjectComparison>().Object;

			var national = new RefCusTariffNationalCode { ZZW_PK = Guid.NewGuid() };
			var rate = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = national.ZZW_PK };
			national.RefCusRates.Add(rate);
			var app = new RefCusApplicability();
			rate.RefCusApplicabilities.Add(app);
			app.RefCusRate = rate;
			national.BuildNonPersistentObjects(repo);

			container.Setup(x => x.EntityStates).Returns(new[] { Tuple.Create((object)national, EntityStates.Added) });
			repo.Update(national.RefCusRateApplicabilities.FirstOrDefault());

			var rateApp = new RefCusRateApplicability(repo) { S01_ZZW_TariffNationalCode = national.ZZW_PK };
			national.RefCusRateApplicabilities.Add(rateApp);
			repo.Add(rateApp);

			var rateDelete = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZW_TariffNationalCode = national.ZZW_PK };
			var appDelete = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			rateDelete.RefCusApplicabilities.Add(appDelete);
			var rateAppDelete = new RefCusRateApplicability(rateDelete, appDelete, repo) { S01_ZZW_TariffNationalCode = national.ZZW_PK };
			national.RefCusRateApplicabilities.Add(rateAppDelete);
			repo.Delete(rateAppDelete);

			repo.SavePersistentObjects();
			var results = repo.GetAllPersistentObjects();

			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusRateUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusApplicabilityUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.UpdateObject(It.IsAny<object>()), Times.Exactly(2));
			container.Verify(x => x.DeleteObject(It.IsAny<object>()), Times.Exactly(2));

			Assert.That(results.Count(), Is.EqualTo(4));
			Assert.That(results.OfType<RefCusRate>().Count(), Is.EqualTo(2));
			Assert.That(results.OfType<RefCusApplicability>().Count(), Is.EqualTo(2));
		}

		[Test]
		public void SavePersistentObjects_NomenclatureGroup()
		{
			var container = new Mock<IContainer>();
			var repo = new SafeRepository(container.Object);
			repo.NonPersistentObjecComparison = new Mock<INonPersistentBusinessObjectComparison>().Object;

			var ng = new RefCusNomenclatureGroup { ZZ5_PK = Guid.NewGuid() };
			var cond = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ5_Nomenclature = ng.ZZ5_PK };
			ng.RefCusConditions.Add(cond);
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			ng.BuildNonPersistentObjects(repo);

			container.Setup(x => x.EntityStates).Returns(new[] { Tuple.Create((object)ng, EntityStates.Added) });

			repo.Update(ng.RefCusConditionApplicabilities.FirstOrDefault());

			var condApp = new RefCusConditionApplicability(repo) { S07_ZZ5_Nomenclature = ng.ZZ5_PK };
			ng.RefCusConditionApplicabilities.Add(condApp);
			repo.Add(condApp);

			var condDelete = new RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_ZZ5_Nomenclature = ng.ZZ5_PK };
			var appDelete = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
			condDelete.RefCusApplicabilities.Add(appDelete);
			var condAppDelete = new RefCusConditionApplicability(condDelete, appDelete, repo) { S07_ZZ5_Nomenclature = ng.ZZ5_PK };
			ng.RefCusConditionApplicabilities.Add(condAppDelete);
			repo.Delete(condAppDelete);

			repo.SavePersistentObjects();
			var results = repo.GetAllPersistentObjects();
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusConditionUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.AddObject(It.Is<string>(y => y == "RefCusApplicabilityUpdate"), It.IsAny<object>()), Times.Once);
			container.Verify(x => x.UpdateObject(It.IsAny<object>()), Times.Exactly(2));
			container.Verify(x => x.DeleteObject(It.IsAny<object>()), Times.Exactly(2));

			Assert.That(results.Count(), Is.EqualTo(4));
			Assert.That(results.OfType<RefCusCondition>().Count(), Is.EqualTo(2));
			Assert.That(results.OfType<RefCusApplicability>().Count(), Is.EqualTo(2));
		}

		[Test]
		public async Task SaveActualChangesOnly()
		{
			var container = new Mock<IContainer>();
			var tracker1 = new Mock<IHasChangesTracker>();
			tracker1.Setup(x => x.GetHasChanges()).Returns(true);
			var tracker2 = new Mock<IHasChangesTracker>();
			container.SetupGet(x => x.EntityStates).Returns(new[] {
				Tuple.Create((object)tracker1.Object, EntityStates.Modified),
				Tuple.Create((object)tracker2.Object, EntityStates.Modified) }
			.AsQueryable());

			var safe = new SafeRepository(container.Object);
			await safe.SaveChangesAysnc();

			Assert.AreEqual(1, safe.AffectedRecords);
			container.Verify(x => x.ChangeState(tracker2.Object, EntityStates.Unchanged));
			container.Verify(x => x.ChangeState(tracker1.Object, EntityStates.Unchanged), Times.Never);
		}

		[Test]
		public void TrackingHasChanges()
		{
			var tariff = new RefCusTariff();
			tariff.ZZ1_ZZF_NKTaxOrFeeCode = "VAT";
			Assert.IsFalse(tariff.GetHasChanges());
			tariff.EnableTrackingHasChanges();
			tariff.ZZ1_ZZF_NKTaxOrFeeCode = "VAT";
			Assert.IsFalse(tariff.GetHasChanges());
			tariff.ZZ1_ZZF_NKTaxOrFeeCode = "FEE";
			Assert.IsTrue(tariff.GetHasChanges());
		}

		[Test]
		public void TrackChangesFromNullToValue()
		{
			var complianceList = new RefComplianceList() { RCL_IntegrationDate = null };
			complianceList.EnableTrackingHasChanges();
			var date = DateTime.Now;
			Assert.DoesNotThrow(() => complianceList.RCL_IntegrationDate = date);
			Assert.That(complianceList.RCL_IntegrationDate, Is.Not.Null);
			Assert.IsTrue(complianceList.GetHasChanges());
		}

		[Test]
		public void Add_ObjectsWithoutApp()
		{
			var container = new Mock<IContainer>();
			var safeRepo = new SafeRepository(container.Object);
			var rateWithoutApp = new RefCusRateWithoutApplicability { ZZ2_RateFormula = "0" };
			var rate = rateWithoutApp.ConvertToRefCusRate();
			safeRepo.Add(rateWithoutApp);
			container.Verify(x => x.AddObject("RefCusRateUpdate", rate), Times.Once);

			var conditionWithoutApp = new RefCusConditionWithoutApplicability { ZX1_Comment = "A" };
			var condition = conditionWithoutApp.ConvertToRefCusCondition();
			safeRepo.Add(conditionWithoutApp);
			container.Verify(x => x.AddObject("RefCusConditionUpdate", condition), Times.Once);
		}

		[Test]
		public void Update_ObjectsWithoutApp()
		{
			var container = new Mock<IContainer>();
			var safeRepo = new SafeRepository(container.Object);
			var rate = new RefCusRate { ZZ2_RateFormula = "0" };
			var rateWithoutApp = new RefCusRateWithoutApplicability(rate, safeRepo);
			rateWithoutApp.ZZ2_RateFormula = "1";
			safeRepo.Update(rateWithoutApp);
			container.Verify(x => x.UpdateObject(It.Is<RefCusRate>(r => r.ZZ2_RateFormula == "1")), Times.Once);

			var condition = new RefCusCondition { ZX1_Comment = "A" };
			var conditionWithoutApp = new RefCusConditionWithoutApplicability(condition, safeRepo);
			conditionWithoutApp.ZX1_Comment = "B";
			safeRepo.Update(conditionWithoutApp);
			container.Verify(x => x.UpdateObject(It.Is<RefCusCondition>(c => c.ZX1_Comment == "B")), Times.Once);
		}

		[Test]
		public void Delete_ObjectsWithoutApp()
		{
			var container = new Mock<IContainer>();
			var safeRepo = new SafeRepository(container.Object);
			var rate = new RefCusRate { ZZ2_RateFormula = "0" };
			var rateWithoutApp = new RefCusRateWithoutApplicability(rate, safeRepo);
			var convertedRate = rateWithoutApp.ConvertToRefCusRate();
			safeRepo.Delete(rateWithoutApp);
			container.Verify(x => x.DeleteObject(convertedRate), Times.Once);

			var condition = new RefCusCondition { ZX1_Comment = "A" };
			var conditionWithoutApp = new RefCusConditionWithoutApplicability(condition, safeRepo);
			var convertedCondition = conditionWithoutApp.ConvertToRefCusCondition();
			safeRepo.Delete(conditionWithoutApp);
			container.Verify(x => x.DeleteObject(convertedCondition), Times.Once);
		}
	}
}
