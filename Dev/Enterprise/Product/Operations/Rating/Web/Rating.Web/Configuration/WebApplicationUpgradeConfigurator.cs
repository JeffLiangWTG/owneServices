using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;

namespace Enterprise.Rating.Web.Configuration
{
	/// <summary>
	/// WebUpgradeManagerConfig
	/// </summary>
	public class WebApplicationUpgradeConfigurator
	{
		const string WebUpgradeManagerInstance = "RatesAPIsWebUpgradeManagerInstance";
		const string UpgradeManagerInitializerTimerInstance = "RatesAPIsUpgradeManagerInitializerTimerInstance";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="retryIntervalsInMillisecond"></param>
		public WebApplicationUpgradeConfigurator(int retryIntervalsInMillisecond)
		{
			if (retryIntervalsInMillisecond > 0)
			{
				this.retryIntervalsInMillisecond = retryIntervalsInMillisecond;
			}
			var virtualDomainAppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
			lazySiteInfo = new Lazy<SiteInformation>(() => SiteInformation.Create(databaseName: Db.DatabaseName, serverName: Db.ServerName));
		}

		readonly Lazy<SiteInformation> lazySiteInfo;

		readonly int retryIntervalsInMillisecond = 5000;

		static readonly object lockObject = new object();

		/// <summary>
		/// ConfigureWebUpgradeManager
		/// </summary>
		/// <param name="config"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		public void ConfigureWebUpgradeManager(HttpConfiguration config)
		{
			try
			{
				CreateAndStartWebUpgradeManager(config);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				WebUpgradeManager.Log($"Web upgrade exception during Rates APIs Startup." + ex.ToString(), EventLogEntryType.Error, lazySiteInfo?.Value);

				var initializerTimer = new Timer(InitializeUpgradeManager, config, TimeSpan.FromMilliseconds(retryIntervalsInMillisecond), TimeSpan.FromMilliseconds(retryIntervalsInMillisecond));

				config
				.Properties
				.AddOrUpdate(
					UpgradeManagerInitializerTimerInstance,
					initializerTimer,
					(key, oldValue) =>
					{
						if (oldValue == initializerTimer)
						{
							return initializerTimer;
						}
						else if (oldValue is IDisposable disposableOldValue)
						{
							disposableOldValue.Dispose();
						}

						return initializerTimer;
					}
				);
			}
		}

		/// <summary>
		/// CreateAndStartWebUpgradeManager
		/// </summary>
		/// <param name="config"></param>
		protected virtual void CreateAndStartWebUpgradeManager(HttpConfiguration config)
		{
			var upgradeManager = CreateWebUpgradeManager();

			config.Properties
			.AddOrUpdate(
				WebUpgradeManagerInstance,
				upgradeManager,

				(key, oldValue) =>
				{
					if (oldValue == upgradeManager)
					{
						return oldValue;
					}
					else if (oldValue is IDisposable oldValueAsDisposable)
					{
						oldValueAsDisposable.Dispose();
					}

					return upgradeManager;
				}
			);
		}

		/// <summary>
		/// InitializeUpgradeManager will be called periodically if WebUpgradeManager can not be created and started.
		/// </summary>
		/// <param name="state"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		protected virtual void InitializeUpgradeManager(object state)
		{
			if (state is HttpConfiguration config)
			{
				lock (lockObject)
				{
					if (config.Properties.TryGetValue(UpgradeManagerInitializerTimerInstance, out var timer))
					{
						try
						{
							CreateAndStartWebUpgradeManager(config);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							WebUpgradeManager.Log($"Exception during Rates APIs InitializeUpgradeManager.: " + ex.ToString(), EventLogEntryType.Error, lazySiteInfo?.Value);
							return;
						}

						if (timer is IDisposable disposableTimer)
						{
							disposableTimer.Dispose();
							config.Properties.TryRemove(UpgradeManagerInitializerTimerInstance, out _);
						}
					}
				}
			}
		}

		WebUpgradeManager CreateWebUpgradeManager()
		{
			var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, GetSqlConnection);
			return new WebUpgradeManager(sqlContext);
		}

		System.Data.Common.DbConnection GetSqlConnection()
		{
			using (Db.DisableSchemaVersionCheck())
			{
				return ((IDbConnectionInternals)Db.NewExtraConnectionToMainDb()).ADOConnection;
			}
		}
	}
}
