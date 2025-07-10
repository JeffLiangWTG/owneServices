using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AgentBookingConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestBizoType()
		{
			AssertEquals(ObjectFactory.GetType<IDtbAgentBooking>(), ConsumerType.BizoType);
		}

		protected override ControllerID ExpectedControllerID => ControllerIDs.DtbBooking;

		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}

		#region Implementation

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceLocalTransport;

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.AgentBooking;

		#endregion
	}
}
