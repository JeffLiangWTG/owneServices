using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(CreateAndSubmitBrokerageOperationalActionMethod))]
	sealed class CreateAndSubmitBrokerageOperationalActionMethodTest : OperationalActionMethodTest<CreateAndSubmitBrokerageOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new CreateAndSubmitBrokerageOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Create and Submit Brokerage Job operational action", method.Name);
				AssertEquals("TestMethodDescription", "Create and Submit Brokerage Job operational action for shipments", method.Description);
				AssertEquals("TestHasControl", true, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override CreateAndSubmitBrokerageOperationalActionMethod NewMethod()
		{
			return new CreateAndSubmitBrokerageOperationalActionMethod();
		}
	}
}
