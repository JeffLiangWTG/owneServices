using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class FinalizeDocketActionMethodTest<TActionMethod, TDocket> : OperationalActionMethodTest<TActionMethod>
		where TDocket : WhsDocket
		where TActionMethod : FinalizeDocketActionMethod<TDocket>, new()
	{
		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			var actionMethod = NewMethod();
			AssertEquals(ExpectedOperationalActionName, actionMethod.Name);
			AssertEquals(ExpectedOperationalActionName, actionMethod.Description);
		}

		protected abstract string ExpectedOperationalActionName { get; }

		#endregion

		#region Implementation

		protected sealed override TActionMethod NewMethod()
		{
			return new TActionMethod();
		}

		#endregion
	}
}
