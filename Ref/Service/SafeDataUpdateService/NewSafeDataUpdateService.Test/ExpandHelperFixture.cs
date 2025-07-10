using System;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class ExpandHelperFixture
	{
		[Test]
		public void Expand()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var tariff1 = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var tariff2 = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var rate1 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff1.ZZ1_PK };
			var rate2 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff1.ZZ1_PK };
			var rate3 = new RefCusRate { ZZ2_PK = Guid.NewGuid(), ZZ2_ZZ1_Tariff = tariff2.ZZ1_PK };
			repo.Setup(x => x.Get<RefCusRate>()).Returns(new[] { rate1, rate2, rate3 }.AsQueryable());

			var expandClause = new Mock<IExpandClauseWrapper>();
			var expandItem = new Mock<IExpandedItemWrapper>();
			expandItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusRate));
			expandClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { expandItem.Object });

			repo.Object.Expand(new[] { tariff1, tariff2 }, expandClause.Object);

			Assert.That(tariff1.RefCusRates, Contains.Item(rate1));
			Assert.That(tariff1.RefCusRates, Contains.Item(rate2));
			Assert.That(tariff2.RefCusRates, Contains.Item(rate3));
		}

		[Test]
		public void ExpandManyToOne()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var type1 = new RefCusTariffType { ZZI_PK = Guid.NewGuid() };
			var type2 = new RefCusTariffType { ZZI_PK = Guid.NewGuid() };
			var type3 = new RefCusTariffType { ZZI_PK = Guid.NewGuid() };
			var tariff1 = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_TariffType = type1.ZZI_PK };
			var tariff2 = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_TariffType = type1.ZZI_PK };
			var tariff3 = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_TariffType = type3.ZZI_PK };
			repo.Setup(x => x.Get<RefCusTariffType>()).Returns(new[] { type1, type2, type3 }.AsQueryable());

			var expandClause = new Mock<IExpandClauseWrapper>();
			var expandItem = new Mock<IExpandedItemWrapper>();
			expandItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffType));
			expandClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { expandItem.Object });

			repo.Object.Expand(new[] { tariff1, tariff2, tariff3 }, expandClause.Object);

			Assert.That(tariff1.RefCusTariffType, Is.EqualTo(type1));
			Assert.That(tariff2.RefCusTariffType, Is.EqualTo(type1));
			Assert.That(tariff3.RefCusTariffType, Is.EqualTo(type3));
		}

		[Test]
		public void ExpandManyToOneDoesNotThrowException()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var tariffType1 = new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = "AAA" };
			var tariffType2 = new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = "BBB" };
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_TariffType = tariffType1.ZZI_PK, ZZ1_TariffCode = "1001" };
			var tariffRule = new RefCusTariffRule { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_TariffType = tariffType2.ZZI_PK, ZZ1_TariffCode = "1001" };
			repo.Setup(x => x.Get<RefCusTariffType>()).Returns(new[] { tariffType1, tariffType2 }.AsQueryable());

			var expandClause = new Mock<IExpandClauseWrapper>();
			var expandItem = new Mock<IExpandedItemWrapper>();
			expandItem.Setup(x => x.GetExpandType()).Returns(typeof(RefCusTariffType));
			expandClause.Setup(x => x.GetExpandClauseWrapper()).Returns(new[] { expandItem.Object });

			repo.Object.Expand(new[] { tariff }, expandClause.Object);
			repo.Object.Expand(new[] { tariffRule }, expandClause.Object);
			Assert.That(tariff.RefCusTariffType, Is.EqualTo(tariffType1));
			Assert.That(tariffRule.RefCusTariffType, Is.EqualTo(tariffType2));
		}
	}
}
