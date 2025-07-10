using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TransportBookingWithAgentConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.DtbBooking;
		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceLocalTransport;
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.TransportBookingWithAgent;
		public override void TestSupportsWiseRates()
		{
			AssertEquals("TransportBookingWithAgentConsumerType should support wise rates", true, ConsumerType.SupportsWiseRates);
		}
	}
}
