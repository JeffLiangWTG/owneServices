using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCMPSemaphoreType))]
	class UCMPSemaphoreTypeTest : Semaphores.Common.Testing.SemaphoreTypeTestCase
	{
		public void TestProperties()
		{
			var semaphoreType = TestSemaphore;
			AssertEquals("LockInfo", "UCMP:UC1:_T1", semaphoreType.LockInfo);
			AssertEquals("Category", "UCM", semaphoreType.Category);
			AssertEquals("MaxConcurrentHandles", 4, semaphoreType.MaxConcurrentHandles);
		}

		protected override Semaphores.Common.ISemaphoreType TestSemaphore => new UCMPSemaphoreType("UC1", "_T1", 4);
	}
}
