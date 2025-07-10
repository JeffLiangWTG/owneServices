using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CanadaForwardingConsolExtensionTest : TestCaseWithFactory
	{
		public void TestIsDestinationToCanada()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "CATOR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			Assert(consol.IsDestinationToCanada());
			consol.JK_RL_NKLoadPort = "CABLO";
			Assert(!consol.IsDestinationToCanada());
			consol.JK_RL_NKDischargePort = "CNHSA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			Assert(!consol.IsDestinationToCanada());
		}
	}
}
