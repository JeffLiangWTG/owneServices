using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbGroupTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new GlbGroupTypeDecider();
			AssertEquals(typeof(GlbGroup), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var salesTeam = Factory.NewWithValidTestData<SalesTeam>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(typeof(GlbGroup), factory2.Load<GlbGroup>(group.PK).GetType());
			AssertEquals(typeof(SalesTeam), factory2.Load<GlbGroup>(salesTeam.PK).GetType());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new GlbGroupTypeDecider();
			AssertEquals(typeof(GlbGroup), typeDecider.GetTypeForNew());
		}
	}
}
