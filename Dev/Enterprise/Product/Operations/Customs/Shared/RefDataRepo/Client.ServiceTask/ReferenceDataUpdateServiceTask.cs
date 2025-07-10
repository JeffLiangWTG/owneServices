using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.RefDataRepo.Ent.Client.ServiceTask;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ReferenceDataUpdateServiceTask.Code,
	"Reference Data Updater",
	"SYS",
	typeof(ReferenceDataUpdateServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "10minutes",
	ActiveByDefault = true
	)]

namespace CargoWise.RefDataRepo.Ent.Client.ServiceTask
{
	public class ReferenceDataUpdateServiceTask : Enterprise.Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "REF";

		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var refDataSetUpdaterWrapper = GetUpdateManagerWrapper();

			if (refDataSetUpdaterWrapper.DbInitializationCheck())
			{
				RunTaskMain(refDataSetUpdaterWrapper);
			}
		}

		void RunTaskMain(SharedDataSetUpdaterWrapper refDataSetUpdaterWrapper)
		{
			var nextUpdate = RefDataRepoRegistry.Instance.NextUpdate.Value;
			if (nextUpdate < ZDateTime.UtcNow.ToDateTime())
			{
				var encryptedKey = RefDataRepoRegistry.Instance.ClientId;
				if (string.IsNullOrEmpty(encryptedKey))
				{
					refDataSetUpdaterWrapper.LogSystemNotRegisteredInfo();
				}
				else
				{
					RunUpdate(refDataSetUpdaterWrapper);
				}
			}
		}

#if DEBUG
		public virtual
#endif
		void RunUpdate(SharedDataSetUpdaterWrapper refDataSetUpdaterWrapper)
		{
			var result = refDataSetUpdaterWrapper.UpdateAll();
			var savedDataSetCount = refDataSetUpdaterWrapper.SavedDataSetCount;

			var failureCount = 0;
			if (result)
			{
				SetNextUpdate();
			}
			else if (savedDataSetCount == 0)
			{
				failureCount = Math.Max(RefDataRepoRegistry.Instance.FailureCount.Value, 0) + 1;
				ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(Code, ZDateTimeOffset.UtcNow.AddMinutes(1).ToDateTimeOffsetSafe());
			}

			if (failureCount >= 3)
			{
				failureCount = 0;
				SetNextUpdate();
			}
			RefDataRepoRegistry.Instance.FailureCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, failureCount);
		}

#if DEBUG
		protected virtual
#endif
		void SetNextUpdate()
		{
			var updateIntervalRegistry = RefDataRepoRegistry.Instance.UpdateInterval;
			var updateInterval = updateIntervalRegistry.Value > 0 ? updateIntervalRegistry.Value : updateIntervalRegistry.DefaultValue;
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddMinutes(updateInterval).ToDateTime());
		}

#if DEBUG
		protected virtual
#endif
		SharedDataSetUpdaterWrapper GetUpdateManagerWrapper()
		{
			return new RefServiceTaskDataSetUpdaterManagerWrapper(ServiceLogger, ResetVersionControl);
		}

		static void ResetVersionControl(IRefVersionControlManager versionControl)
		{
			if (!RefDataRepoRegistry.Instance.VersionControlReset.Value)
			{
				return;
			}

			versionControl.ResetAll();
			RefDataRepoRegistry.Instance.VersionControlReset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			RefDataRepoRegistry.Instance.NextUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}
	}
}
