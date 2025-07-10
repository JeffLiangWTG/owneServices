using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsActiveBusinessObjectCollectionTestCase<T> : ActiveBusinessObjectCollectionTestCase<T> where T : IActiveBusinessObjectCollection
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
