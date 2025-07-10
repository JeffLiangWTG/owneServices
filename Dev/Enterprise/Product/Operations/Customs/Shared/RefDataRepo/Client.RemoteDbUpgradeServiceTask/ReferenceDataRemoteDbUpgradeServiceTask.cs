using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: HostedService(CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask.ReferenceDataRemoteDbUpgradeServiceTask.Code,
	"Reference Data Remote Database Upgrade",
	"SYS",
	typeof(CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask.ReferenceDataRemoteDbUpgradeServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "10minutes",
	ActiveByDefault = true
	)]

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask
{
	public class ReferenceDataRemoteDbUpgradeServiceTask : Enterprise.Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "RDU";

		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			var nextUpdate = RemoteDatabaseRegistry.Instance.NextUpgrade.Value;
			if (nextUpdate < ZDateTime.UtcNow.ToDateTime())
			{
				var refDataSetUpdaterWrapper = GetUpdateManagerWrapper();
				var encryptedKey = RemoteDatabaseRegistry.Instance.ClientId;
				if (string.IsNullOrEmpty(encryptedKey))
				{
					refDataSetUpdaterWrapper.LogSystemNotRegisteredInfo();
				}
				else
				{
					UpgradeSchemaAndDownloadData(refDataSetUpdaterWrapper);
				}
			}
		}

		void UpgradeSchemaAndDownloadData(SharedDataSetUpdaterWrapper refDataSetUpdaterWrapper)
		{
			var result = UpgradeDb();

			if (refDataSetUpdaterWrapper.DbInitializationCheck())
			{
				result = refDataSetUpdaterWrapper.UpdateAll() && result;
			}
			else
			{
				result = false;
			}
			var savedDataSetCount = refDataSetUpdaterWrapper.SavedDataSetCount;

			var failureCount = 0;
			if (result)
			{
				SetNextUpdate();
			}
			else if (savedDataSetCount == 0)
			{
				failureCount = Math.Max(RemoteDatabaseRegistry.Instance.FailureCount.Value, 0) + 1;
				ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(Code, ZDateTimeOffset.UtcNow.AddMinutes(1).ToDateTimeOffsetSafe());
			}

			if (failureCount >= 3)
			{
				failureCount = 0;
				SetNextUpdate();
			}
			RemoteDatabaseRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, failureCount);
		}

#if DEBUG
		protected virtual
#endif
		SharedDataSetUpdaterWrapper GetUpdateManagerWrapper()
		{
			return new RemoteDbServiceTaskDataSetUpdaterManagerWrapper(ServiceLogger, ResetVersionControl, serverList.Value);
		}

		static void ResetVersionControl(IRefVersionControlManager versionControl)
		{
			if (!RemoteDatabaseRegistry.Instance.VersionControlReset.Value)
			{
				return;
			}

			versionControl.ResetAll();
			RemoteDatabaseRegistry.Instance.VersionControlReset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		bool UpgradeDb()
		{
			var upgraderManager = GetUpgraderManager();
			return upgraderManager.Update();
		}

#if DEBUG
		protected virtual
#endif
		void SetNextUpdate()
		{
			var upgradeIntervalRegistry = RemoteDatabaseRegistry.Instance.UpgradeInterval;
			var upgradeInterval = upgradeIntervalRegistry.Value > 0 ? upgradeIntervalRegistry.Value : upgradeIntervalRegistry.DefaultValue;
			RemoteDatabaseRegistry.Instance.NextUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddMinutes(upgradeInterval).ToDateTime());
		}

#if DEBUG
		protected virtual
#endif
		ServiceTaskRemoteDbUpgraderManager GetUpgraderManager()
		{
			return new ServiceTaskRemoteDbUpgraderManager(ServiceLogger, new ErrorReportingClientWrapper(true), serverList.Value);
		}

		[ThreadSafe]
		static readonly Lazy<IEnumerable<string>> serverList = new Lazy<IEnumerable<string>>(GetServerList);

		static IEnumerable<string> GetServerList()
		{
			var result = new List<string>() { Db.ServerName };
			using (var adminConnection = Db.NewAdminConnection())
			{
				var availabilityGroup = AlwaysOnHelper.GetAvailabilityGroupInfo(adminConnection, Db.DatabaseName);
				var secondaryServers = availabilityGroup.GetSecondaryServers(adminConnection);
				if (secondaryServers != null)
				{
					result.AddRange(secondaryServers);
				}

				var edwServerName = DbRegistry.BiDataWarehouseServer.LoadValue(adminConnection);
				if (!string.IsNullOrEmpty(edwServerName) && !result.Contains(edwServerName))
				{
					result.Add(edwServerName);
				}
			}
			return result;
		}
	}
}
