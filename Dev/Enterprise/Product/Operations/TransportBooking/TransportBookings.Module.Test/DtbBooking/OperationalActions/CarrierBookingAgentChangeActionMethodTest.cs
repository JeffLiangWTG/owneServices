using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CarrierBookingAgentChangeActionMethod))]
	class CarrierBookingAgentChangeActionMethodTest : OperationalActionMethodTest<CarrierBookingAgentChangeActionMethod>
	{
		public void TestNewControl()
		{
			var actionMethod = new CarrierBookingAgentChangeActionMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<CarrierBookingAgentChangeControl>("Expecting correct new control type", newControl);
			}
		}

		public void TestApplicatorType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(CarrierBookingAgentChangeActionMethodApplicator), applicator);
		}

		public void TestNameAndDescription()
		{
			AssertEquals("Assign Carrier Booking Agent", Method.Name);
			AssertEquals("Mass Assign Carrier Booking Agent", Method.Description);
		}

		protected override CarrierBookingAgentChangeActionMethod NewMethod()
		{
			return new CarrierBookingAgentChangeActionMethod();
		}
	}
}
