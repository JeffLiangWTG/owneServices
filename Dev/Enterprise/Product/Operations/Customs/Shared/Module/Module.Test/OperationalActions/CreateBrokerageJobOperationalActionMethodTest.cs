using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(CreateBrokerageJobOperationalActionMethod))]
	sealed class CreateBrokerageJobOperationalActionMethodTest : OperationalActionMethodTest<CreateBrokerageJobOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new CreateBrokerageJobOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Create Brokerage Job operational action", method.Name);
				AssertEquals("TestMethodDescription", "Create Brokerage Job operational action for shipments", method.Description);
				AssertEquals("TestHasControl", true, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override CreateBrokerageJobOperationalActionMethod NewMethod()
		{
			return new CreateBrokerageJobOperationalActionMethod();
		}
	}
}
