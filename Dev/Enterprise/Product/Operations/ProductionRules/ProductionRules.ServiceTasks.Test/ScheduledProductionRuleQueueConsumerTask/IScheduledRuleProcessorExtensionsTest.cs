using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Moq;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	class IScheduledRuleProcessorExtensionsTest : TestCaseWithFactory
	{
		public void TestGetTemporaryUserContextWithBranchOffRuleSet()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var rulesProcessor = new Mock<IScheduledRuleProcessor>();
			rulesProcessor.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.IsAny<ProductionRuleSet>())).Returns(branch.PK);

			Assert("Precondition", branch.PK != Env.CurrentBranchPK);

			var ruleSet = Factory.New<ProductionRuleSet>();
			using (rulesProcessor.Object.GetTemporaryUserContextWithBranchOffRuleSet(Factory.GetCachedReadOnlyFactory(), ruleSet))
			{
				AssertEquals("Env.CurrentBranchPK correct", branch.PK, Env.CurrentBranchPK);
			}

			Assert("Postcondition", branch.PK != Env.CurrentBranchPK);
		}
	}
}
