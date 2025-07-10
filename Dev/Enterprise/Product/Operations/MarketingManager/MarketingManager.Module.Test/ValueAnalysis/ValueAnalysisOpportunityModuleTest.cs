using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module.Testing
{
	public abstract class ValueAnalysisOpportunityModuleTest : ValueAnalysisModuleTest
	{
		public void TestContext()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				AssertEquals(OrgOpportunitySchema.Constants.PK, module.Context);
			}
		}
	}
}
