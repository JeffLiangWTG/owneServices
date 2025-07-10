using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsNonPersistentBusinessObjectCollectionTestCase<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : INonPersistentBusinessObjectCollection
	{
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
