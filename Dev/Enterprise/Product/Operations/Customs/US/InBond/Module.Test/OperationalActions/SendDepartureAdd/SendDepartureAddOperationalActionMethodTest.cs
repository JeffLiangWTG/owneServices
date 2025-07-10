using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendDepartureAddOperationalActionMethod))]
	sealed class SendDepartureAddOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendDepartureAddOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendDepartureAddOperationalActionMethod();
			AssertEquals("Send Departure Add Operational Action", method.Name);
			AssertEquals("Send Departure Add", method.Description);
			AssertEquals(true, method.HasControl);
			AssertEquals(false, method.HasSettings);
			using (var control = method.NewGuiControl())
			{
				AssertEquals(typeof(SendDepartureAddOperationActionControl), control.GetType());
			}
		}

		protected override SendDepartureAddOperationalActionMethod NewMethod() => new SendDepartureAddOperationalActionMethod();
	}
}
