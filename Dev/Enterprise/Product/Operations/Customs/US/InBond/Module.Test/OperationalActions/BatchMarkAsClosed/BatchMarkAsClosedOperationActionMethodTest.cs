using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions.Testing
{
	[TestedType(typeof(BatchMarkAsClosedOperationActionMethod))]
	sealed class BatchMarkAsClosedOperationActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<BatchMarkAsClosedOperationActionMethod>
	{
		public void TestProperties()
		{
			var method = new BatchMarkAsClosedOperationActionMethod();
			AssertEquals("Batch Mark As Closed Operational Action", method.Name);
			AssertEquals("Batch Mark As Closed", method.Description);
			AssertEquals(false, method.HasControl);
			AssertEquals(false, method.HasSettings);
		}

		protected override BatchMarkAsClosedOperationActionMethod NewMethod() => new();
	}
}
