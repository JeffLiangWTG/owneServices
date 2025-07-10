using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsTestCaseWithFactoryEnv : TestCaseWithFactory
	{
		public WhsTestCaseWithFactoryEnv()
		{
		}

		#region Properties

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = GetNewTestHelperFunctions();
				}
				return helper;
			}
		}

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		TestNotificationBuffer notify;
		protected TestNotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new TestNotificationBuffer();
				}
				return notify;
			}
		}

		#endregion
	}
}
