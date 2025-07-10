using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module.Testing
{
	public abstract class ValueAnalysisOrganisationModuleTest : ValueAnalysisModuleTest
	{
		public void TestContext()
		{
			using (var module = GetValueAnalysisModuleForTest())
			{
				AssertEquals(OrgHeaderSchema.Constants.PK, module.Context);
			}
		}
	}
}
