using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolExcludeOCBEventTest : TestCaseWithFactory
	{
		public void TestForwardingConsolExcludeOCB()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				AssertCollectionContains(AutoEvents.OceanCarrierBookingByTEU, consol.Logs.EventsThatCannotBeAdded);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				AssertCollectionNotContains(AutoEvents.OceanCarrierBookingByTEU, consol.Logs.EventsThatCannotBeAdded);
			}
		}
	}
}
