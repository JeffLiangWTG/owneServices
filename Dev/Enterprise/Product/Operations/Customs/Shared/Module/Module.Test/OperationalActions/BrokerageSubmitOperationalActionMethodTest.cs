using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(BrokerageSubmitOperationalActionMethod))]
	sealed class BrokerageSubmitOperationalActionMethodTest : OperationalActionMethodTest<BrokerageSubmitOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new BrokerageSubmitOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Submit operational action", method.Name);
				AssertEquals("TestMethodDescription", "Submit operational action for brokerage jobs", method.Description);
				AssertEquals("TestHasControl", false, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override BrokerageSubmitOperationalActionMethod NewMethod() => new BrokerageSubmitOperationalActionMethod();
	}
}
