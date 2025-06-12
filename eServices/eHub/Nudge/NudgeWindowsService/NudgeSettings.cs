using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	public class NudgeSettings : IObservable
	{
		public void LoadApplicationSettings()
		{
			SetFileWatcherOnSettingFile();
			LoadSettingsFromFile();
		}

		void SetFileWatcherOnSettingFile()
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(config.FilePath), Path.GetFileName(config.FilePath));
			fileWatcher.Changed += OnSettingsFileChanged;
			fileWatcher.EnableRaisingEvents = true;
			Logger.InfoFormat("Started monitoring settings file '{0}' in '{1}'", fileWatcher.Filter, fileWatcher.Path);
		}

		void LoadSettingsFromFile()
		{
			try
			{
				ConfigurationManager.RefreshSection("connectionStrings");
				ConfigurationManager.RefreshSection("nudge");
				CONNECTION_STRING = ConfigurationManager.ConnectionStrings["eHubTransactions"].ConnectionString;
				var dic = ConfigurationManager.GetSection("nudge") as IDictionary;
				RELOAD_SYSTEM_INTERVAL = Int32.Parse(dic["reloadSystemInterval"].ToString()) * 1000;
				CHECK_AND_NUDGE_INTERVAL = Int32.Parse(dic["checkAndNudgeInterval"].ToString()) * 1000;
				MAX_THREAD_NUM_IN_POOL = Int32.Parse(dic["maxThreadNumInPool"].ToString());
				AUTH_KEY = dic["authKey"].ToString();
				var nudgeRetryIntervals = dic["nudgeRetryIntervals"].ToString();
				string[] intervals = nudgeRetryIntervals.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				var ts = new TimeSpan[intervals.Length + 1];
				for (int i = 0; i < intervals.Length; i++)
				{
					ts[i + 1] = TimeSpan.FromSeconds(Int32.Parse(intervals[i]));
				}
				NEXT_INTERVALS = ts;
				NUDGE_CCDS_URL = dic["nudgeCCDSUrl"].ToString();
			}
			catch (NullReferenceException e)
			{
				Logger.Error("Failed to read setting file", e);
			}
		}

		void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
		{
			Logger.InfoFormat("Changes to settings file {0}\\{1} have been detected.", ((FileSystemWatcher)sender).Path, ((FileSystemWatcher)sender).Filter);
			LoadSettingsFromFile();
			NotifySubscribers();
			// intend to set the maximal number of workThreads only, however, portThreads is defined and retrieved only because the get/set API require it.
			int workThreads, portThreads;
			ThreadPool.GetMaxThreads(out workThreads, out portThreads);
			ThreadPool.SetMaxThreads(MAX_THREAD_NUM_IN_POOL, portThreads);
		}

		public void RegisterSubscriber(IObserver subscriber)
		{
			if (!subscribers.Contains(subscriber))
			{
				subscribers.Add(subscriber);
			}
		}

		void NotifySubscribers()
		{
			foreach (var subscriber in subscribers)
			{
				subscriber.OnNotify(this);
			}
		}

		public static NudgeSettings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NudgeSettings();
				}
				return instance;
			}
		}

		static NudgeSettings instance;

		public string CONNECTION_STRING;
		public int RELOAD_SYSTEM_INTERVAL;
		public int CHECK_AND_NUDGE_INTERVAL;
		public int MAX_THREAD_NUM_IN_POOL;
		public string AUTH_KEY;
		public TimeSpan[] NEXT_INTERVALS;
		public string NUDGE_CCDS_URL { get; set; }

		readonly List<IObserver> subscribers = new List<IObserver>();
		FileSystemWatcher fileWatcher;
		static readonly ILog Logger = LogManager.GetLogger(typeof(NudgeSettings));


	}
}
