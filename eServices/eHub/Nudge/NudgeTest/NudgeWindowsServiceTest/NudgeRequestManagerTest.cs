using System; 
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using System.Threading.Tasks;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class NudgeRequestManagerTest
	{
		[Test]
		public void TestRequestNudgeWhenAPreviousRequestIsStillInQueue()
		{
			NudgeSettings.Instance.NUDGE_CCDS_URL = "https://ccds.wtg.zone/ScheduleServiceTasksUri?ClientCode={endpoint}&amp;ServiceTasks=EHI";
			CacheManager.SystemInfoCache.PutInCache("sys", new EHubClientSystem("sys", "http://url"));
			var requestManager = new NudgeRequestManager("sys");
			CacheManager.NudgeRequestControlCache.PutInCache("sys", requestManager);
			NudgeHttpUtil httputil = MockRepository.GeneratePartialMock<NudgeHttpUtil>();
			httputil.Stub(h => h.MakeHttpRequest(string.Empty)).IgnoreArguments().Return(200);
			NudgeHttpUtil.Instance = httputil;
			requestManager.IsPendingRequest = true;
			NudgeRequestManager.RequestNudge("sys"); // this RequestNudge call will NOT be queued in ThreadPool
			Assert.AreEqual(requestManager.QueueTimeUTC, DateTime.MinValue);
			ClearAllCache();
		}

		[Test]
		public void TestRequestNudgeWithMockedUrl()
		{
			NudgeSettings.Instance.NEXT_INTERVALS = new TimeSpan[3] {
				TimeSpan.MinValue,
				TimeSpan.FromMilliseconds(10),
				TimeSpan.FromMilliseconds(20)
			};

			NudgeSettings.Instance.NUDGE_CCDS_URL = "https://ccds.wtg.zone/ScheduleServiceTasksUri?ClientCode={endpoint}&amp;ServiceTasks=EHI";

			CacheManager.SystemInfoCache.PutInCache("sys", new EHubClientSystem("sys", "http://url"));
			var requestManager = new NudgeRequestManagerForTest("sys");
			CacheManager.NudgeRequestControlCache.PutInCache("sys", requestManager);

			NudgeHttpUtil httputil = MockRepository.GeneratePartialMock<NudgeHttpUtil>();
			httputil.Stub(h => h.MakeHttpRequest(string.Empty)).IgnoreArguments().Return(200);
			NudgeHttpUtil.Instance = httputil;
			var timer = new TimeoutUtility();
			NudgeRequestManagerForTest.RequestNudge("sys");

			while (requestManager.IsPendingRequest)
			{
				timer.CheckTimeout();
			}

			Assert.IsFalse(requestManager.IsPendingRequest, "Nudge request should not be pending after execution.");
			Assert.AreEqual(0, requestManager.ServiceStatus, "ServiceStatus should be reset to 0 after a successful request.");

			ClearAllCache();
		}

		[Test]
		public void TestRequestNudgeWhenHttpFails()
		{
			NudgeSettings.Instance.NEXT_INTERVALS = new TimeSpan[3] {
				TimeSpan.MinValue,
				TimeSpan.FromMilliseconds(10),
				TimeSpan.FromMilliseconds(20)
			};

			NudgeSettings.Instance.NUDGE_CCDS_URL = "https://ccds.wtg.zone/ScheduleServiceTasksUri?ClientCode={endpoint}&amp;ServiceTasks=EHI";

			CacheManager.SystemInfoCache.PutInCache("sys", new EHubClientSystem("sys", "http://url"));
			NudgeHttpUtil httputil = MockRepository.GeneratePartialMock<NudgeHttpUtil>();
			httputil.Stub(h => h.MakeHttpRequest(string.Empty)).IgnoreArguments().Do((Func<string, int>)(url =>
			{
				Thread.Sleep(100);
				return 404;
			}));
			NudgeHttpUtil.Instance = httputil;
			var timer = new TimeoutUtility();
			NudgeRequestManagerForTest.RequestNudge("sys");

			var requestManager = CacheManager.NudgeRequestControlCache.GetItemFromCache("sys");

			while (requestManager.IsPendingRequest) // this while statement ensures that the previous Nudge has been completed
			{
				timer.CheckTimeout();
			}
			Assert.AreEqual(1, requestManager.ServiceStatus);
			DateTime lastCallTimeUTC = requestManager.LastCallTimeUTC;
			timer = new TimeoutUtility();
			do
			{
				NudgeRequestManagerForTest.RequestNudge("sys");
				timer.CheckTimeout();
			} while (!requestManager.IsPendingRequest); // this do-while statement ensures that a new Nudge is queued in the Threadpool
			Assert.IsTrue(DateTime.UtcNow >= lastCallTimeUTC + NudgeSettings.Instance.NEXT_INTERVALS[1]);
			timer = new TimeoutUtility();
			while (requestManager.IsPendingRequest) // this while statement ensures that the previous Nudge has been completed
			{
				timer.CheckTimeout();
			}
			Assert.AreEqual(2, requestManager.ServiceStatus);
			lastCallTimeUTC = requestManager.LastCallTimeUTC;
			timer = new TimeoutUtility();
			do
			{
				NudgeRequestManagerForTest.RequestNudge("sys");
				timer.CheckTimeout();
			} while (!requestManager.IsPendingRequest); // this do-while statement ensures that a new Nudge is queued in the Threadpool
			Assert.IsTrue(DateTime.UtcNow >= lastCallTimeUTC + NudgeSettings.Instance.NEXT_INTERVALS[2]);
			timer = new TimeoutUtility();
			while (requestManager.IsPendingRequest) // this while statement ensures that the previous Nudge has been completed
			{
				timer.CheckTimeout();
			}
			Assert.AreEqual(3, requestManager.ServiceStatus);
			lastCallTimeUTC = requestManager.LastCallTimeUTC;
			timer = new TimeoutUtility();
			do
			{
				NudgeRequestManagerForTest.RequestNudge("sys");
				timer.CheckTimeout();
			} while (!requestManager.IsPendingRequest); // this do-while statement ensures that a new Nudge is queued in the Threadpool
			Assert.IsTrue(DateTime.UtcNow >= lastCallTimeUTC + NudgeSettings.Instance.NEXT_INTERVALS[2]);
			timer = new TimeoutUtility();
			while (requestManager.IsPendingRequest) // this while statement ensures that the previous Nudge has been completed
			{
				timer.CheckTimeout();
			}
			Assert.AreEqual(3, requestManager.ServiceStatus);
			ClearAllCache();
		}

		public void TestRequestNudgeWhenNudgeURLIsNotValid()
		{
			CacheManager.SystemInfoCache.PutInCache("sys", new EHubClientSystem("sys", "   "));
			var requestManager = new NudgeRequestManager("sys");
			CacheManager.NudgeRequestControlCache.PutInCache("sys", requestManager);
			requestManager.IsPendingRequest = false;
			NudgeRequestManager.RequestNudge("sys"); // this RequestNudge call will NOT be queued in ThreadPool
			Assert.AreEqual(requestManager.QueueTimeUTC, DateTime.MinValue);
			ClearAllCache();
		}

		void ClearAllCache()
		{
			CacheManager.SystemInfoCache = new CacheManager<string, EHubClientSystem>();
			CacheManager.NudgeRequestControlCache = new CacheManager<string, NudgeRequestManager>();
		}
	}

	class TimeoutUtility
	{
		public void CheckTimeout()
		{
			if (DateTime.UtcNow > startTimeUTC + timeoutSpan)
			{
				throw new Exception("timeout");
			}
		}

		readonly DateTime startTimeUTC = DateTime.UtcNow;
		readonly TimeSpan timeoutSpan = TimeSpan.FromSeconds(60);
	}

	class NudgeRequestManagerForTest : NudgeRequestManager
	{
		public NudgeRequestManagerForTest(string systemCode) : base(systemCode) { }

		public new static void RequestNudge(string systemCode)
		{
			var nudgeRequestManager = CacheManager.NudgeRequestControlCache.GetItemFromCacheOrCreateWhenNotExist(systemCode, delegate { return new NudgeRequestManagerForTest(systemCode); });
			nudgeRequestManager.ProcessNudgeRequest(systemCode);
		}

		protected override Task<string> CallCCDSServiceAsync(string ccdsURL)
		{
			if (ccdsURL == "https://ccds.wtg.zone/ScheduleServiceTasksUri?ClientCode=sys&amp;ServiceTasks=EHI")
			{
				string content = "http://localhost:9753/mockhttpserver/nudge";
				return Task.FromResult(content);
			}

			else
			{
				throw new Exception("ccdsUrl was incorrect");
			}
		}
	}
}
