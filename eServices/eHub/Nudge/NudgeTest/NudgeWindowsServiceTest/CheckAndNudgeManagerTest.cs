using NUnit.Framework;
using CargoWise.eHub.Nudge;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class CheckAndNudgeManagerTest
	{
		[Test]
		public void TestCheckAndNudge_SystemCacheEmpty()
		{
			CheckAndNudgeManager manager = new CheckAndNudgeManager();
			manager.CheckAndNudge();
			Assert.AreEqual(CacheManager.SystemInfoCache.Count, 0);
		}

		[SetUp]
		public void Setup()
		{
			CacheManager.SystemInfoCache.Clear();
		}
	}
}
