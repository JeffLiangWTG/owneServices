using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendEntrySummaryOperationalActionMethod))]
	sealed class SendEntrySummaryOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendEntrySummaryOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendEntrySummaryOperationalActionMethod();
			AssertEquals("Send Entry Summary operational action", method.Name);
			AssertEquals("Send Entry Summary (US)", method.Description);
			AssertEquals(true, method.HasControl);
			AssertEquals(false, method.HasSettings);
			using (var control = method.NewGuiControl())
			{
				AssertEquals(typeof(SendEntrySummaryOperationActionControl), control.GetType());
			}
		}

		protected override SendEntrySummaryOperationalActionMethod NewMethod() => new SendEntrySummaryOperationalActionMethod();
	}
}
