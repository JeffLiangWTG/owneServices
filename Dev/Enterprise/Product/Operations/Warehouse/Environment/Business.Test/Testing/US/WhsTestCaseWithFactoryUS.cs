using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	public abstract class WhsTestCaseWithFactoryUS : WhsTestCaseWithFactoryEnv
	{
		public WhsTestCaseWithFactoryUS()
		{
		}

		#region Properties

		protected new WhsTestHelperFunctionsEnvUS Helper
		{
			get { return (WhsTestHelperFunctionsEnvUS)base.Helper; }
		}

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnvUS(Factory);
		}

		#endregion
	}
}
