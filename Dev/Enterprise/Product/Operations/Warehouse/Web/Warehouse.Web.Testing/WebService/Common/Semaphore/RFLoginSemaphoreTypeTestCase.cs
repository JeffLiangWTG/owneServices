using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.Testing
{
	[TestedType(typeof(RFLoginSemaphoreType))]
	public class RFLoginSemaphoreTypeTestCase : SemaphoreTypeTestCase
	{
		public void TestISemaphoreType()
		{
			ISemaphoreType semaphoreType = new RFLoginSemaphoreType();
			AssertEquals("LGN", semaphoreType.Category);
			AssertEquals("RFWebLogin", semaphoreType.LockInfo);
			AssertEquals(0, semaphoreType.MaxConcurrentHandles);
		}
	}
}
