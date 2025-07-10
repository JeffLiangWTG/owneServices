using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class RefCusTariffFixture
	{
		[Test]
		public void BuildRefCusRateApplicabilities_RefCusTariff()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusRates.Add(rate);
			var app = new RefCusApplicability();
			rate.RefCusApplicabilities.Add(app);
			tariff.BuildNonPersistentObjects();

			Assert.That(tariff.RefCusRateApplicabilities.Single().S01_ZZ1_Tariff == tariff.ZZ1_PK);
		}

		[Test]
		public void BuildRefCusRateApplicabilities_RefCusTariff_Multi()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate1 = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff.ZZ1_PK };
			var rate2 = new RefCusRate() { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusRates.Add(rate1);
			tariff.RefCusRates.Add(rate2);

			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate1.RefCusApplicabilities.Add(app);
			}
			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate2.RefCusApplicabilities.Add(app);
			}

			tariff.BuildNonPersistentObjects();

			Assert.That(tariff.RefCusRateApplicabilities.Count == 6);
			Assert.That(tariff.RefCusRateApplicabilities.All(x => x.S01_ZZ1_Tariff == tariff.ZZ1_PK));
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_RefCusTariff()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var cond = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusConditions.Add(cond);
			var app = new RefCusApplicability();
			cond.RefCusApplicabilities.Add(app);
			tariff.BuildNonPersistentObjects();

			Assert.That(tariff.RefCusConditionApplicabilities.Single().S07_ZZ1_Tariff == tariff.ZZ1_PK);
		}

		[Test]
		public void BuildRefCusConditionApplicabilities_RefCusTariff_Multi()
		{
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate1 = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = tariff.ZZ1_PK };
			var rate2 = new RefCusCondition() { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = tariff.ZZ1_PK };
			tariff.RefCusConditions.Add(rate1);
			tariff.RefCusConditions.Add(rate2);

			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate1.RefCusApplicabilities.Add(app);
			}
			for (int i = 0; i < 3; i++)
			{
				var app = new RefCusApplicability { ZZT_PK = Guid.NewGuid() };
				rate2.RefCusApplicabilities.Add(app);
			}

			tariff.BuildNonPersistentObjects();

			Assert.That(tariff.RefCusConditionApplicabilities.Count == 6);
			Assert.That(tariff.RefCusConditionApplicabilities.All(x => x.S07_ZZ1_Tariff == tariff.ZZ1_PK));
		}
	}
}
