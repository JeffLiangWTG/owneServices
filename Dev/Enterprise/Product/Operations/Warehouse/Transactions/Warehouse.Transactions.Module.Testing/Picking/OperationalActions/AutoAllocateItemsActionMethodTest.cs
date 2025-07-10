using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AutoAllocateItemsActionMethod))]
	public class AutoAllocateItemsActionMethodTest : OperationalActionMethodTest<AutoAllocateItemsActionMethod>
	{
		protected override AutoAllocateItemsActionMethod NewMethod() => new AutoAllocateItemsActionMethod();

		public void TestNameValue()
		{
			AssertEquals("Auto Allocate Remaining Items", Method.Name);
		}

		public void TestDescriptionValue()
		{
			AssertEquals("Auto Allocate Remaining Items", Method.Description);
		}
	}
}
