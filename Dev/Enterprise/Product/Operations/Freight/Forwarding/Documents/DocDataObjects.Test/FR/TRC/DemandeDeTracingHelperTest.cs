using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class DemandeDeTracingHelperTest : TestCaseWithFactory
	{
		public void TestIsTracingRequestApplicable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Assert(!DemandeDeTracingHelper.IsTracingRequestApplicable(consol, DemandeDeTracingDirection.Export));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "FRAXP";
			consol.JK_RL_NKDischargePort = "REBRP";

			Assert(DemandeDeTracingHelper.IsTracingRequestApplicable(consol, DemandeDeTracingDirection.Import));
			Assert(DemandeDeTracingHelper.IsTracingRequestApplicable(consol, DemandeDeTracingDirection.Export));

			consol.JK_RL_NKLoadPort = "CNSHG";
			consol.JK_RL_NKDischargePort = "";
			Assert(!DemandeDeTracingHelper.IsTracingRequestApplicable(consol, DemandeDeTracingDirection.Import));
			Assert(!DemandeDeTracingHelper.IsTracingRequestApplicable(consol, DemandeDeTracingDirection.Export));
		}
	}
}
