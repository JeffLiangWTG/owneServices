using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbExternalPasswordTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var huiPwd = Factory.New<GlbCompanyExternalPasswordHUI>();
			var insPwd = Factory.New<GlbBranchExternalPasswordINS>();
			var intPwd = Factory.New<GlbBranchExternalPasswordINT>();

			var pwd = Factory.New<GlbExternalPassword>();
			CombineAssertions(() =>
			{
				var decider = new GlbExternalPasswordTypeDecider();
				AssertEquals("Type", typeof(GlbCompanyExternalPasswordHUI), decider.GetTypeForLoad(((INeedRow)huiPwd).Row, Factory));
				AssertEquals("Type", typeof(GlbBranchExternalPasswordINS), decider.GetTypeForLoad(((INeedRow)insPwd).Row, Factory));
				AssertEquals("Type", typeof(GlbBranchExternalPasswordINT), decider.GetTypeForLoad(((INeedRow)intPwd).Row, Factory));

				AssertEquals("Type", typeof(GlbExternalPassword), decider.GetTypeForLoad(((INeedRow)pwd).Row, Factory));
			});
		}

		public void TestGetTypeForBinding()
		{
			var decider = new GlbExternalPasswordTypeDecider();
			AssertEquals("Type", typeof(GlbExternalPassword), decider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var decider = new GlbExternalPasswordTypeDecider();
			AssertEquals("Type", typeof(GlbExternalPassword), decider.GetTypeForNew());
		}
	}
}
