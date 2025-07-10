using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.OData.Client;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Rule.Test
{
	class TariffRuleHeaderWrapperFixture
	{
		[Test]
		public void MatchingTariffCode()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "842720402",
				ZZ1_ZZZ_NKDataGrouping = "ZA"
			};

			var ruleHeader = new TariffRuleHeaderWrapper(tariffRule, safeMock.Object);
			Assert.True(ruleHeader.IsMatch("842720402", "XX", null, null, "ZA"));
			Assert.False(ruleHeader.IsMatch("842720401", "XX", null, null, "ZA"));
		}

		[Test]
		public void MatchingTariffCodeAndSchedule()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "40907",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				RefCusTariffType = new RefCusTariffType
				{
					ZZI_PK = Guid.NewGuid(),
					ZZI_TariffType = "4P1"
				}
			};
			var ruleHeader = new TariffRuleHeaderWrapper(tariffRule, safeMock.Object);
			Assert.True(ruleHeader.IsMatch("40907221", "4P1", null, null, "ZA"));
			Assert.False(ruleHeader.IsMatch("40907221", "4P2", null, null, "ZA"));
			Assert.False(ruleHeader.IsMatch("40908221", "4P1", null, null, "ZA"));
		}

		[Test]
		public void MatchingTariffCodeAndChkDigit()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "040640",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
			};
			var attrRule = new RefCusTariffAttributeRule
			{
				ZZ3_Name = "CheckDigit",
				ZZ3_Value = "8",
				ZZ3_ZZ1_Tariff = tariffRule.ZZ1_PK
			};
			tariffRule.RefCusTariffAttributeRules = new DataServiceCollection<RefCusTariffAttributeRule>(new[] { attrRule }, TrackingMode.None);
			var ruleHeader = new TariffRuleHeaderWrapper(tariffRule, safeMock.Object);
			Assert.True(ruleHeader.IsMatch("040640", "SP1", "8", null, "ZA"));
			Assert.False(ruleHeader.IsMatch("040640", "SP1", "9", null, "ZA"));
			Assert.False(ruleHeader.IsMatch("040641", "SP1", "8", null, "ZA"));
		}

		[Test]
		public async Task GetMatchingTariffsWithRelatedObjects()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_TariffCode = "040640",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
			};
			var tariffType = new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_TariffType = "IMP"
			};
			var tariff1 = new RefCusTariff
			{
				ZZ1_TariffCode = "04064010",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				RefCusTariffType = tariffType
			};
			var tariff2 = new RefCusTariff
			{
				ZZ1_TariffCode = "04064020",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				RefCusTariffType = tariffType
			};
			var tariff3 = new RefCusTariff
			{
				ZZ1_TariffCode = "04064110",
				ZZ1_ZZZ_NKDataGrouping = "ZA",
				RefCusTariffType = tariffType
			};
			safeMock.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3 }.AsQueryable());
			var ruleHeader = new TariffRuleHeaderWrapper(tariffRule, safeMock.Object);
			Assert.That(await ruleHeader.GetMatchingTariffsWithRelatedObjects(), Is.EqualTo(new[] { tariff1, tariff2 }));
		}

		Mock<ISafeRepository> safeMock;

		[SetUp]
		public void SetUp()
		{
			safeMock = new Mock<ISafeRepository>();
		}
	}
}
