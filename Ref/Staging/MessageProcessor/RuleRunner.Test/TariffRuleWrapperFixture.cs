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
	class TariffRuleWrapperFixture
	{
		[Test]
		public async Task SynchroniseRateAndRateAttr()
		{
			var tariffRule = new RefCusTariffRule { ZZ1_PK = Guid.NewGuid() };
			var rateRule = new RefCusRateRule
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariffRule.ZZ1_PK,
				ZZ2_SelectorFormula = "pp='EUQUOTA'",
				ZZ2_RateFormula = "MIN(2.5 * [KG], 0.475 * VFD)"
			};

			safeMock.Setup(x => x.Get<RefCusRateRule>()).Returns(new[] { rateRule }.AsQueryable());
			var rule = new TariffRuleWrapper(tariffRule.ZZ1_PK, safeMock.Object);
			var tariffPK = Guid.NewGuid();
			var rateResult = (await rule.SynchroniseAsync(tariffPK, new RefCusRate[0])).FirstOrDefault();
			Assert.That(rateResult.RuleType, Is.EqualTo(RuleType.INSERT));
			Assert.That(rateResult.Data.ZZ2_SelectorFormula, Is.EqualTo("pp='EUQUOTA'"));
			Assert.That(rateResult.Data.ZZ2_RateFormula, Is.EqualTo("MIN(2.5 * [KG], 0.475 * VFD)"));
			Assert.That(rateResult.Data.ZZ2_ZZ1_Tariff, Is.EqualTo(tariffPK));
			Assert.That(rateResult.Data.ZZ2_StartDate, Is.EqualTo(new DateTime(1900, 01, 01, 0, 0, 0).ToUTCDateTimeOffset()));
			Assert.That(rateResult.Data.ZZ2_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 0).ToUTCDateTimeOffset()));
		}

		[TestCase(null)]
		[TestCase("pp='EUQUOTA'")]
		public async Task SynchroniseRate(string selectorFormula)
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid()
			};
			var rateRule = new RefCusRateRule
			{
				ZZ2_ZZ1_Tariff = tariffRule.ZZ1_PK,
				ZZ2_SelectorFormula = selectorFormula,
				ZZ2_RateFormula = @"{""RebateAmount""}"
			};
			safeMock.Setup(x => x.Get<RefCusTariffRule>()).Returns(new[] { tariffRule }.AsQueryable());
			safeMock.Setup(x => x.Get<RefCusRateRule>()).Returns(new[] { rateRule }.AsQueryable());

			var rule = new TariffRuleWrapper(tariffRule.ZZ1_PK, safeMock.Object);
			var tariffPK = Guid.NewGuid();
			var result = (await rule.SynchroniseAsync(tariffPK, new RefCusRate[0])).FirstOrDefault();
			Assert.That(result.RuleType, Is.EqualTo(RuleType.INSERT));
			Assert.That(result.Data.ZZ2_SelectorFormula, Is.EqualTo(selectorFormula ?? string.Empty));
			Assert.That(result.Data.ZZ2_RateFormula, Is.EqualTo(@"{""RebateAmount""}"));
			Assert.That(result.Data.ZZ2_ZZ1_Tariff, Is.EqualTo(tariffPK));

			var rate = new RefCusRate
			{
				ZZ2_RateFormula = "ZZZZZ",
				ZZ2_ZZ1_Tariff = tariffPK,
				ZZ2_SelectorFormula = selectorFormula
			};
			result = (await rule.SynchroniseAsync(Guid.NewGuid(), new[] { rate })).FirstOrDefault();
			Assert.That(result.Data, Is.EqualTo(rate));
			Assert.That(result.RuleType, Is.EqualTo(RuleType.UPDATE));
			Assert.That(rate.ZZ2_RateFormula, Is.EqualTo(@"{""RebateAmount""}"));
			Assert.That(rate.ZZ2_ZZ1_Tariff, Is.EqualTo(tariffPK));
			Assert.That(result.Data.ZZ2_SelectorFormula, Is.EqualTo(selectorFormula));
		}

		[Test]
		public async Task SynchroniseTariffAttribute()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid()
			};
			var attrRule = new RefCusTariffAttributeRule
			{
				ZZ3_ZZ1_Tariff = tariffRule.ZZ1_PK,
				ZZ3_Name = "ImportPermit",
				ZZ3_Value = "MANDATORY"
			};
			safeMock.Setup(x => x.Get<RefCusTariffRule>()).Returns(new[] { tariffRule }.AsQueryable());
			safeMock.Setup(x => x.Get<RefCusTariffAttributeRule>()).Returns(new[] { attrRule }.AsQueryable());

			var rule = new TariffRuleWrapper(tariffRule.ZZ1_PK, safeMock.Object);
			var tariffPK = Guid.NewGuid();

			var result = (await rule.SynchroniseAsync(tariffPK, new RefCusTariffAttribute[0])).FirstOrDefault();
			Assert.That(result.RuleType, Is.EqualTo(RuleType.INSERT));
			Assert.That(result.Data.ZZ3_Name, Is.EqualTo("ImportPermit"));
			Assert.That(result.Data.ZZ3_Value, Is.EqualTo("MANDATORY"));
			Assert.That(result.Data.ZZ3_ZZ1_Tariff, Is.EqualTo(tariffPK));

			var attr = new RefCusTariffAttribute
			{
				ZZ3_Name = "ImportPermit",
				ZZ3_Value = "OPTIONAL",
				ZZ3_ZZ1_Tariff = tariffPK
			};
			result = (await rule.SynchroniseAsync(Guid.NewGuid(), new[] { attr })).FirstOrDefault();
			Assert.That(result.Data, Is.EqualTo(attr));
			Assert.That(result.RuleType, Is.EqualTo(RuleType.UPDATE));
			Assert.That(attr.ZZ3_Name, Is.EqualTo("ImportPermit"));
			Assert.That(attr.ZZ3_Value, Is.EqualTo("MANDATORY"));
			Assert.That(attr.ZZ3_ZZ1_Tariff, Is.EqualTo(tariffPK));
		}

		[Test]
		public async Task SynchroniseUOMs()
		{
			var tariffRule = new RefCusTariffRule
			{
				ZZ1_PK = Guid.NewGuid()
			};
			var uomRule = new RefCusTariffUOMRule
			{
				ZZ8_Type = "RU1",
				ZZ8_UOM = "KG",
				ZZ8_ZZ1_Tariff = tariffRule.ZZ1_PK,
				ZZ8_ZZZ_NKDataGrouping = "AU"
			};
			safeMock.Setup(x => x.Get<RefCusTariffRule>()).Returns(new[] { tariffRule }.AsQueryable());
			safeMock.Setup(x => x.Get<RefCusTariffUOMRule>()).Returns(new[] { uomRule }.AsQueryable());
			var rule = new TariffRuleWrapper(tariffRule.ZZ1_PK, safeMock.Object);
			var tariffPK = Guid.NewGuid();
			var result = (await rule.SynchroniseAsync(tariffPK, new RefCusTariffUOM[0])).FirstOrDefault();
			Assert.That(result.RuleType, Is.EqualTo(RuleType.INSERT));
			Assert.That(result.Data.ZZ8_Type, Is.EqualTo("RU1"));
			Assert.That(result.Data.ZZ8_UOM, Is.EqualTo("KG"));
			Assert.That(result.Data.ZZ8_ZZ1_Tariff, Is.EqualTo(tariffPK));
			Assert.That(result.Data.ZZ8_ZZZ_NKDataGrouping, Is.Not.Null);

			var uom = new RefCusTariffUOM
			{
				ZZ8_Type = "RU1",
				ZZ8_UOM = "G",
				ZZ8_ZZ1_Tariff = tariffPK,
				ZZ8_ZZZ_NKDataGrouping = "AU"
			};
			result = (await rule.SynchroniseAsync(Guid.NewGuid(), new[] { uom })).FirstOrDefault();
			Assert.That(result.RuleType, Is.EqualTo(RuleType.UPDATE));
			Assert.That(result.Data, Is.EqualTo(uom));
			Assert.That(uom.ZZ8_UOM, Is.EqualTo("KG"));
			Assert.That(uom.ZZ8_Type, Is.EqualTo("RU1"));
			Assert.That(uom.ZZ8_ZZ1_Tariff, Is.EqualTo(tariffPK));
			Assert.That(uom.ZZ8_ZZZ_NKDataGrouping, Is.Not.Null);
		}

		Mock<ISafeRepository> safeMock;

		[SetUp]
		public void SetUp()
		{
			safeMock = new Mock<ISafeRepository>();
		}
	}
}
