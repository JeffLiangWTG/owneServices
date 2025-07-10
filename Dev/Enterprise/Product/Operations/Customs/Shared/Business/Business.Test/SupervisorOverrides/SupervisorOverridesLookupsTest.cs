using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SupervisorOverridesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUsers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			SupervisorOverrides supervisorOverrides = new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
			AssertEquals(typeof(GlbStaffCollection), supervisorOverrides.Lookups.Users.GetType());
		}
	}
}
