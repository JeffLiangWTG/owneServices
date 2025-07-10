using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CreateLoadsFromOrdersActionMethod))]
	public class CreateLoadsFromOrdersActionMethodTest : OperationalActionMethodTest<CreateLoadsFromOrdersActionMethod>
	{
		protected override CreateLoadsFromOrdersActionMethod NewMethod() => new CreateLoadsFromOrdersActionMethod();

		public void TestActionName()
		{
			AssertEquals("Create Loads", Method.Name);
		}

		public void TestActionDescription()
		{
			AssertEquals("Create Loads", Method.Description);
		}
	}
}
