using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class CancelDocketActionMethodTest<S, T> : OperationalActionMethodTest<S>
			where T : WhsDocket
			where S : CancelDocketActionMethod<T>
	{
		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			var actionMethod = NewMethod();
			var expected = GetExpectedNameAndDescription();

			AssertNotNull(expected);
			AssertEquals(expected, actionMethod.Name);
			AssertEquals(expected, actionMethod.Description);
		}

		#endregion

		protected abstract string GetExpectedNameAndDescription();
	}
}
