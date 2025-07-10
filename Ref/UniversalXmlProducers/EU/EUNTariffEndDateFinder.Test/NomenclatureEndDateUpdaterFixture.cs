using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder.Test
{
	[TestFixture]
	class NomenclatureEndDateUpdaterFixture
	{
		[Test]
		public void Update()
		{
			var safeRepo = new Mock<ISafeRepository>();
			var updater = new NomenclatureEndDateUpdater(safeRepo.Object);
			updater.Update(new[] { "0100000000", "0200000000", "0300000000" }, new DateTime(2020, 02, 01));
			updater.Update(new[] { "0100000000", "0200000000", "0300000000" }, new DateTime(2020, 01, 01));
			updater.Update(new[] { "0100000000", "0200000000" }, new DateTime(2020, 03, 01));
			updater.Update(new[] { "0200000000" }, new DateTime(2020, 04, 01));
			var results = updater.GetCodesWithEndDates().ToArray();
			Assert.AreEqual(2, results.Length);
			Assert.Contains(Tuple.Create("0100000000", new DateTime(2020, 03, 31, 23, 59, 0)), results);
			Assert.Contains(Tuple.Create("0300000000", new DateTime(2020, 02, 29, 23, 59, 0)), results);
		}

		[Test]
		public async Task CheckCodesInSafeDb()
		{
			var safeRepo = new Mock<ISafeRepository>();
			var tariffType = new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_TariffType = "IMP"
			};
			var tariff1 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "0400000000",
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_StartDate = new DateTime(1900, 01, 01),
				ZZ1_EndDate = new DateTime(2021, 01, 01),
				RefCusTariffType = tariffType
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "0300000000",
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_StartDate = new DateTime(1900, 01, 01),
				ZZ1_EndDate = new DateTime(2021, 01, 01),
				RefCusTariffType = tariffType
			};
			safeRepo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2 }.AsQueryable());
			var updater = new NomenclatureEndDateUpdater(safeRepo.Object);
			updater.Update(new[] { "0100000000", "0200000000", "0300000000" }, new DateTime(2020, 01, 01));
			updater.Update(new[] { "0100000000", "0200000000", "0300000000" }, new DateTime(2020, 02, 01));
			updater.Update(new[] { "0100000000", "0200000000" }, new DateTime(2020, 03, 01));
			updater.Update(new[] { "0200000000" }, new DateTime(2020, 04, 01));

			var result = (await updater.CheckCodesInSafeDb(new DateTime(2020, 3, 15))).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.Contains(Tuple.Create("0300000000", new DateTime(2020, 02, 29, 23, 59, 0)), result);
			Assert.Contains(Tuple.Create("0400000000", new DateTime(2020, 03, 15, 0, 0, 0)), result);
		}
	}
}
