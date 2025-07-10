using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Staging.Rule;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZACustomsProcessStagingTariffs.Test
{
	[TestFixture]
	class TariffRuleRepositoryFixture
	{
		[Test]
		public async Task GetRuleHeaderWrappersAsync()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "842720402",
				ZZ1_ZZZ_NKDataGrouping = "ZA"
			};

			safeMock.Setup(x => x.Get<RefCusTariffRule>()).Returns(new[] { tariffRule }.AsQueryable());
			var repo = new TariffRuleRepository(safeMock.Object);
			Assert.That((await repo.GetRuleHeaderWrappersAsync("842720402", "XX", null, null, "ZA")).ToArray().Length, Is.EqualTo(1));
			Assert.That((await repo.GetRuleHeaderWrappersAsync("842720401", "XX", null, null, "ZA")).ToArray().Length, Is.EqualTo(0));
		}

		[Test]
		public async Task GetUnAppliedRuleHeaderWrappersAsync()
		{
			var tariffRule1 = new RefCusTariffRule
			{
				ZZ1_Applied = true
			};
			var tariffRule2 = new RefCusTariffRule
			{
				ZZ1_Applied = false
			};
			safeMock.Setup(x => x.Get<RefCusTariffRule>()).Returns(new[] { tariffRule1, tariffRule2 }.AsQueryable());
			var repo = new TariffRuleRepository(safeMock.Object);
			Assert.That((await repo.GetUnAppliedRuleHeaderWrappersAsync()).ToArray().Length, Is.EqualTo(1));
		}

		Mock<ISafeRepository> safeMock;

		[SetUp]
		public void SetUp()
		{
			safeMock = new Mock<ISafeRepository>();
		}
	}
}
