using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class WhsTransitTestCaseWithFactory : WhsTestCaseWithFactoryEnv
	{
		#region Properties

		protected new WhsTransitTestHelper Helper
		{
			get { return (WhsTransitTestHelper)base.Helper; }
		}

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTransitTestHelper(Factory);
		}

		#endregion
	}
}
