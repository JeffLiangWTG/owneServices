using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolidatedTransportBookingJobDataTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Transport Booking (Legacy)", new ConsolidatedTransportBookingJobData().HumanReadableName);
		}
	}
}
