using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	[TestedType(typeof(CreateTransportJobsOperationalActionMethod))]
	public class CreateTransportJobsOperationalActionMethodTest : OperationalActionMethodTest<CreateTransportJobsOperationalActionMethod>
	{
		protected override CreateTransportJobsOperationalActionMethod NewMethod()
		{
			return new CreateTransportJobsOperationalActionMethod();
		}
	}
}
