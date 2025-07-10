using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	[TestedType(typeof(CreatePortTransportJobsOperationalActionMethod))]
	public class CreatePortTransportJobsOperationalActionMethodTest : OperationalActionMethodTest<CreatePortTransportJobsOperationalActionMethod>
	{
		protected override CreatePortTransportJobsOperationalActionMethod NewMethod()
		{
			return new CreatePortTransportJobsOperationalActionMethod();
		}
	}
}
