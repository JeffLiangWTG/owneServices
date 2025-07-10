using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsBusinessObjectCollectionTestCase : BusinessObjectCollectionTestCase
	{
		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		public TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}

		WhsTestHelperFunctionsEnv helper;
		TestNotificationBuffer notify;
	}
}
