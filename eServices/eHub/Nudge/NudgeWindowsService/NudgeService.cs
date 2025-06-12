using System;
using System.ServiceProcess;
using System.Threading;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	public partial class NudgeService : ServiceBase
	{
		public NudgeService()
		{
			NudgeSettings.Instance.LoadApplicationSettings();
			reloadSystemInfoTask = new RepeatableTask(NudgeSettings.Instance.RELOAD_SYSTEM_INTERVAL, delegate { ReloadSystemInfoManager.Instance.ReloadSystemInfo(); });
			reloadSystemInfoTask.GetRunIntervalFromCurrentSettingFunc = delegate(NudgeSettings nu) { return nu.RELOAD_SYSTEM_INTERVAL; };
			reloadSystemInfoTask.SubscribeTo(NudgeSettings.Instance);
			checkAndNudgeTask = new RepeatableTask(NudgeSettings.Instance.CHECK_AND_NUDGE_INTERVAL, delegate { CheckAndNudgeManager.Instance.CheckAndNudge(); });
			checkAndNudgeTask.GetRunIntervalFromCurrentSettingFunc = delegate(NudgeSettings nu) { return nu.CHECK_AND_NUDGE_INTERVAL; };
			checkAndNudgeTask.SubscribeTo(NudgeSettings.Instance);
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			Logger.Info("Starting eHub Nudge Service...");
			// intend to set the maximal number of workThreads only, however, portThreads is defined and retrieved only because the get/set API require it.
			int workThreads, portThreads;
			ThreadPool.GetMaxThreads(out workThreads, out portThreads);
			ThreadPool.SetMaxThreads(NudgeSettings.Instance.MAX_THREAD_NUM_IN_POOL, portThreads);
			reloadSystemInfoTask.Start();
			checkAndNudgeTask.Start(TimeSpan.FromSeconds(3));
		}

		protected override void OnStop()
		{
			Logger.Info("eHub Nudge Service is stopping...");
			reloadSystemInfoTask.Dispose();
			checkAndNudgeTask.Dispose();
			WaitUntilAllRequestsInThreadPoolComplete();
			Logger.Info("eHub Nudge Service is now stopped.");
		}

		void WaitUntilAllRequestsInThreadPoolComplete()
		{
			while (!CacheManager.NudgeRequestControlCache.IsEmpty)
			{
				foreach (NudgeRequestManager item in CacheManager.NudgeRequestControlCache.GetAllItems())
				{
					if (!item.IsPendingRequest)
					{
						CacheManager.NudgeRequestControlCache.RemoveItemFromCacheByKey(item.SystemCode);
					}
				}
				Thread.Sleep(100);
			}
		}

		RepeatableTask reloadSystemInfoTask, checkAndNudgeTask;
		static readonly ILog Logger = LogManager.GetLogger(typeof(NudgeService));
	}
}
