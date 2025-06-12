namespace CargoWise.eServices.Billing.WcfService.Tests
{
	using NUnit.Framework;

	class BillingServiceHealthCheckHttpTaskAsyncHandlerTest
	{
		[Test]
		public void TestBillingServiceHealthCheckHttpTaskAsyncHandler()
		{
			var healthCheckHttpTaskAsyncHandler = new BillingServiceHealthCheckHttpTaskAsyncHandler();
			Assert.IsTrue(healthCheckHttpTaskAsyncHandler != null);
		}
	}
}
