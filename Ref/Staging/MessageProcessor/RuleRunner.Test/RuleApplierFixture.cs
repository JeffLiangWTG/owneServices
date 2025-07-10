using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Rule.Test
{
	[TestFixture]
	public class RuleApplierFixture
	{
		[Test]
		public async Task ApplyRules()
		{
			await ApplyTariffRule();
			ruleMock.Verify(x => x.SynchroniseAsync(It.IsAny<Guid>(), It.IsAny<RefCusTariffUOM[]>()));
			ruleMock.Verify(x => x.SynchroniseAsync(It.IsAny<Guid>(), It.IsAny<RefCusTariffAttribute[]>()));
		}

		[Test]
		public async Task ApplyUOMRule()
		{
			var uom1 = new RefCusTariffUOM();
			var ruleResultMock1 = new Mock<IRuleResult<RefCusTariffUOM>>();
			ruleResultMock1.Setup(x => x.RuleType).Returns(RuleType.INSERT);
			ruleResultMock1.Setup(x => x.Data).Returns(uom1);

			var uom2 = new RefCusTariffUOM();
			var ruleResultMock2 = new Mock<IRuleResult<RefCusTariffUOM>>();
			ruleResultMock2.Setup(x => x.RuleType).Returns(RuleType.UPDATE);
			ruleResultMock2.Setup(x => x.Data).Returns(uom2);
			ruleMock.Setup(x => x.SynchroniseAsync(It.IsAny<Guid>(), It.IsAny<RefCusTariffUOM[]>()))
				.Returns(Task.FromResult(new[] { ruleResultMock1.Object, ruleResultMock2.Object }.AsEnumerable()));

			await ApplyTariffRule();
			safeMock.Verify(x => x.Add(uom1));
			safeMock.Verify(x => x.Add(uom2), Times.Never);
		}

		[Test]
		public async Task ApplyRateRule()
		{
			var rate = new RefCusRate();
			var rateRuleResult = new Mock<IRuleResult<RefCusRate>>();
			rateRuleResult.Setup(x => x.RuleType).Returns(RuleType.INSERT);
			rateRuleResult.Setup(x => x.Data).Returns(rate);
			ruleMock.Setup(x => x.SynchroniseAsync(It.IsAny<Guid>(), It.IsAny<RefCusRate[]>()))
				.Returns(Task.FromResult(new[] { rateRuleResult.Object }.AsEnumerable()));
			await ApplyTariffRule();

			rateRuleResult.Object.Data.ZZ2_RateFormula = "YYY";
			rateRuleResult.Setup(x => x.RuleType).Returns(RuleType.UPDATE);
			await ApplyTariffRule();
		}

		async Task ApplyTariffRule()
		{
			var applier = new RuleApplier(safeMock.Object);
			await applier.ApplyAsync(
				ruleMock.Object,
				new RefCusTariff() { ZZ1_ZZZ_NKDataGrouping = "ZA" },
				new RefCusTariffUOM[0],
				new RefCusTariffAttribute[0],
				new RefCusRate[0]);
		}

		Mock<ISafeRepository> safeMock;
		Mock<ITariffRuleWrapper> ruleMock;

		[SetUp]
		public void SetUp()
		{
			safeMock = new Mock<ISafeRepository>();
			ruleMock = new Mock<ITariffRuleWrapper>();
		}
	}
}
