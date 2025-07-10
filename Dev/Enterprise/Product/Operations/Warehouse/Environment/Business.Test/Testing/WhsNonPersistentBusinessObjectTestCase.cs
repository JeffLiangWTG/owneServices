using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsNonPersistentBusinessObjectTestCase : NonPersistentBusinessObjectTestCase
	{
		public WhsNonPersistentBusinessObjectTestCase()
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
					helper = new WhsTestHelperFunctionsEnv(Factory);
				}
				return helper;
			}
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
