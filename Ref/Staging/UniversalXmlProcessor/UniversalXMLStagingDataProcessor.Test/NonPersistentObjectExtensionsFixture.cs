using NUnit.Framework;
using System;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using Moq;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class NonPersistentObjectExtensionsFixture
	{
		[Test]
		public void BuildRefCusRateApplicabilities_Safe_RefCusTariff()
		{
			var rate1 = new Safe.RefCusRate();
			var app1 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate2 = new Safe.RefCusRate();
			var app2 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate3 = new Safe.RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);
			var tariff = new Safe.RefCusTariff();
			tariff.RefCusRates.Add(rate1);
			tariff.RefCusRates.Add(rate2);
			tariff.RefCusRates.Add(rate3);

			Assert.That(tariff.RefCusRateApplicabilities, Is.Empty);
			var obj = (object)tariff;
			obj.BuildNonPersistentObjects(safeRepository);
			Assert.That(tariff.RefCusRateApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusRateApplicabilities_Staging_RefCusTariff()
		{
			var rate1 = new Stage.RefCusRate();
			var app1 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate2 = new Stage.RefCusRate();
			var app2 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate3 = new Stage.RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);
			var tariff = new Stage.RefCusTariff();
			tariff.RefCusRates.Add(rate1);
			tariff.RefCusRates.Add(rate2);
			tariff.RefCusRates.Add(rate3);

			Assert.That(tariff.RefCusRateApplicabilities, Is.Empty);
			var obj = (object)tariff;
			obj.BuildNonPersistentObjects();
			Assert.That(tariff.RefCusRateApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusRateApplicabilities_Safe_RefCusTariffNationalCode()
		{
			var rate1 = new Safe.RefCusRate();
			var app1 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate2 = new Safe.RefCusRate();
			var app2 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate3 = new Safe.RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);
			var tariffNationalCode = new Safe.RefCusTariffNationalCode();
			tariffNationalCode.RefCusRates.Add(rate1);
			tariffNationalCode.RefCusRates.Add(rate2);
			tariffNationalCode.RefCusRates.Add(rate3);

			Assert.That(tariffNationalCode.RefCusRateApplicabilities, Is.Empty);
			var obj = (object)tariffNationalCode;
			obj.BuildNonPersistentObjects(safeRepository);
			Assert.That(tariffNationalCode.RefCusRateApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_Safe_RefCusTariff()
		{
			var cond1 = new Safe.RefCusCondition();
			var app1 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond2 = new Safe.RefCusCondition();
			var app2 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond3 = new Safe.RefCusCondition();
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond2.RefCusApplicabilities.Add(app3);
			var tariff = new Safe.RefCusTariff();
			tariff.RefCusConditions.Add(cond1);
			tariff.RefCusConditions.Add(cond2);
			tariff.RefCusConditions.Add(cond3);

			Assert.That(tariff.RefCusConditionApplicabilities, Is.Empty);
			var obj = (object)tariff;
			obj.BuildNonPersistentObjects(safeRepository);
			Assert.That(tariff.RefCusConditionApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_Safe_RefCusNomenclatureGroup()
		{
			var cond1 = new Safe.RefCusCondition();
			var app1 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond2 = new Safe.RefCusCondition();
			var app2 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Safe.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond3 = new Safe.RefCusCondition();
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond2.RefCusApplicabilities.Add(app3);
			var nomenclatureGroup = new Safe.RefCusNomenclatureGroup();
			nomenclatureGroup.RefCusConditions.Add(cond1);
			nomenclatureGroup.RefCusConditions.Add(cond2);
			nomenclatureGroup.RefCusConditions.Add(cond3);

			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities, Is.Empty);
			var obj = (object)nomenclatureGroup;
			obj.BuildNonPersistentObjects(safeRepository);
			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusRateApplicabilities_Staging_RefCusTariffNationalCode()
		{
			var rate1 = new Stage.RefCusRate();
			var app1 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate2 = new Stage.RefCusRate();
			var app2 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var rate3 = new Stage.RefCusRate();
			rate1.RefCusApplicabilities.Add(app1);
			rate2.RefCusApplicabilities.Add(app2);
			rate2.RefCusApplicabilities.Add(app3);
			var tariffNationalCode = new Stage.RefCusTariffNationalCode();
			tariffNationalCode.RefCusRates.Add(rate1);
			tariffNationalCode.RefCusRates.Add(rate2);
			tariffNationalCode.RefCusRates.Add(rate3);

			Assert.That(tariffNationalCode.RefCusRateApplicabilities, Is.Empty);
			var obj = (object)tariffNationalCode;
			obj.BuildNonPersistentObjects();
			Assert.That(tariffNationalCode.RefCusRateApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_Staging_RefCusTariff()
		{
			var cond1 = new Stage.RefCusCondition();
			var app1 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond2 = new Stage.RefCusCondition();
			var app2 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond3 = new Stage.RefCusCondition();
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond2.RefCusApplicabilities.Add(app3);
			var tariff = new Stage.RefCusTariff();
			tariff.RefCusConditions.Add(cond1);
			tariff.RefCusConditions.Add(cond2);
			tariff.RefCusConditions.Add(cond3);

			Assert.That(tariff.RefCusConditionApplicabilities, Is.Empty);
			var obj = (object)tariff;
			obj.BuildNonPersistentObjects();
			Assert.That(tariff.RefCusConditionApplicabilities, Has.Count.EqualTo(3));
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_Staging_RefCusNomenclatureGroup()
		{
			var cond1 = new Stage.RefCusCondition();
			var app1 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond2 = new Stage.RefCusCondition();
			var app2 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var app3 = new Stage.RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
			var cond3 = new Stage.RefCusCondition();
			cond1.RefCusApplicabilities.Add(app1);
			cond2.RefCusApplicabilities.Add(app2);
			cond2.RefCusApplicabilities.Add(app3);
			var nomenclatureGroup = new Stage.RefCusNomenclatureGroup();
			nomenclatureGroup.RefCusConditions.Add(cond1);
			nomenclatureGroup.RefCusConditions.Add(cond2);
			nomenclatureGroup.RefCusConditions.Add(cond3);

			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities, Is.Empty);
			var obj = (object)nomenclatureGroup;
			obj.BuildNonPersistentObjects();
			Assert.That(nomenclatureGroup.RefCusConditionApplicabilities, Has.Count.EqualTo(3));
		}

		ISafeRepository safeRepository;

		[SetUp]
		public void Setup()
		{
			safeRepository = new Mock<ISafeRepository>().Object;
		}
	}
}
