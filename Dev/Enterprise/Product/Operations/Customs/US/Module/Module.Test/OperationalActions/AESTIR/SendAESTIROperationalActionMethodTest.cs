using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendAESTIROperationalActionMethod))]
	sealed class SendAESTIROperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendAESTIROperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendAESTIROperationalActionMethod();
			AssertEquals("Send AESTIR operational action", method.Name);
			AssertEquals("Send AES (US)", method.Description);
			AssertEquals(true, method.HasControl);
			AssertEquals(false, method.HasSettings);
			using (var control = method.NewGuiControl())
			{
				AssertEquals(typeof(SendAESTIROperationActionControl), control.GetType());
			}
		}

		protected override SendAESTIROperationalActionMethod NewMethod() => new SendAESTIROperationalActionMethod();
	}
}
