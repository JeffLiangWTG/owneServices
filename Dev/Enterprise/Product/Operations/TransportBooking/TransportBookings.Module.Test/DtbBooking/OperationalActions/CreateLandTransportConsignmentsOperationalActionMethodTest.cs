using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	[TestedType(typeof(CreateLandTransportConsignmentsOperationalActionMethod))]
	public class CreateLandTransportConsignmentsOperationalActionMethodTest : OperationalActionMethodTest<CreateLandTransportConsignmentsOperationalActionMethod>
	{
		protected override CreateLandTransportConsignmentsOperationalActionMethod NewMethod()
		{
			return new CreateLandTransportConsignmentsOperationalActionMethod();
		}
	}
}
