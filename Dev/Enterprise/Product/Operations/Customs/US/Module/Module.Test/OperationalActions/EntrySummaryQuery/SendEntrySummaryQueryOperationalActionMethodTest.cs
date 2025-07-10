using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendEntrySummaryQueryOperationalActionMethod))]
	sealed class SendEntrySummaryQueryOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendEntrySummaryQueryOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendEntrySummaryQueryOperationalActionMethod();
			AssertEquals("Send Entry Summary Query operational action", method.Name);
			AssertEquals("Send Entry Summary Query(US)", method.Description);
			AssertEquals(false, method.HasControl);
			AssertEquals(false, method.HasSettings);
		}

		protected override SendEntrySummaryQueryOperationalActionMethod NewMethod() => new SendEntrySummaryQueryOperationalActionMethod();
	}
}
