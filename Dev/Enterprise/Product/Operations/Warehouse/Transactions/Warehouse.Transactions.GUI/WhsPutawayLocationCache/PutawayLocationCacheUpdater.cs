using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class PutawayLocationCacheUpdater : IPutawayLocationCacheUpdater
	{
		public PutawayLocationCacheUpdater(
			IStalePutawayLocationCacheFinder stalePutawayLocationCacheFinder,
			IWhsPutawayLocationCacheManager whsPutawayLocationCacheManager)
		{
			StaleLocationCacheFinder = Argument.NotNull(stalePutawayLocationCacheFinder, nameof(stalePutawayLocationCacheFinder));
			PutawayLocationCacheManager = Argument.NotNull(whsPutawayLocationCacheManager, nameof(whsPutawayLocationCacheManager));
		}

		IStalePutawayLocationCacheFinder StaleLocationCacheFinder { get; }
		IWhsPutawayLocationCacheManager PutawayLocationCacheManager { get; }

		const int BatchSize = 500;

		public void UpdatePutawayLocationCache(BusinessObjectFactory factory, CancellationToken token)
		{
			var locationsNeedUpdating = StaleLocationCacheFinder.FindCacheEntriesThatNeedUpdating(factory).ToArray();
			if (locationsNeedUpdating.Length > 0)
			{
				var warehouseToLocationGrouping = new Dictionary<ZGuid, HashSet<ZGuid>>();
				foreach (var locationInfo in locationsNeedUpdating)
				{
					token.ThrowIfCancellationRequested();

					if (!warehouseToLocationGrouping.TryGetValue(locationInfo.WarehousePK, out var locations))
					{
						locations = new HashSet<ZGuid>();
						warehouseToLocationGrouping.Add(locationInfo.WarehousePK, locations);
					}

					locations.Add(locationInfo.LocationPK);
				}

				foreach (var warehouseToLocations in warehouseToLocationGrouping)
				{
					UpdatePutawayLocationCacheIfAllowed(factory, warehouseToLocations.Key, warehouseToLocations.Value, 0, token);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public bool UpdateWarehousePutawayLocationCacheWithProgressForm(BusinessObjectFactory factory, ZGuid warehousePK, INotifications notifications, int maxWaitInMinutes = 30)
		{
			var locationsNeedUpdating = StaleLocationCacheFinder.FindCacheEntriesThatNeedUpdating(factory, warehousePK).ToArray();
			var locationCacheUpdated = !(locationsNeedUpdating.Length > 0);
			if (!locationCacheUpdated)
			{
				if (Globals.IsUserInteractive)
				{
					using (var progressForm = new ProgressFormManager { IsCancelButtonVisible = true, IsProgressBarVisible = false })
					using (var tokenSource = new CancellationTokenSource())
					{
						progressForm.Cancelled += (s, e) => tokenSource.Cancel();
						progressForm.UpdateStatus(Res.GetString("8082dc92-89f6-4b43-b13d-d06563b32e3d", "Updating location cache..."), 0);
						progressForm.Start();

						var cancellationToken = tokenSource.Token;
						try
						{
							locationCacheUpdated = UpdatePutawayLocationCacheIfAllowed(factory, warehousePK, locationsNeedUpdating, maxWaitInMinutes, cancellationToken, notifications);
						}
						catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
						{
							locationCacheUpdated = false;
						}
					}
				}
				else
				{
					locationCacheUpdated = UpdatePutawayLocationCacheIfAllowed(factory, warehousePK, locationsNeedUpdating, maxWaitInMinutes, notifications: notifications);
				}
			}

			return locationCacheUpdated;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		bool UpdatePutawayLocationCacheIfAllowed(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> locationsNeedUpdating, int maxWaitInMinutes, CancellationToken token = default, INotifications notifications = null)
		{
			var locationCacheUpdated = false;
			using (var mutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString()))
			{
				locationCacheUpdated = UpdatePutawayLocationCache(factory, locationsNeedUpdating, maxWaitInMinutes, mutex, token, notifications);
			}

			return locationCacheUpdated;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		bool UpdatePutawayLocationCache(BusinessObjectFactory factory, IEnumerable<ZGuid> locationsNeedUpdating, int maxWaitInMinutes, ZGlobalMutex mutex, CancellationToken token, INotifications notifications)
		{
			mutex.Lock();
			return mutex.HasLock
				? UpdatePutawayLocationCacheCore(factory, locationsNeedUpdating, mutex, token)
				: maxWaitInMinutes > 0 && IsLocationCacheUpdateUnlockedAfterWait(mutex, maxWaitInMinutes, token, notifications);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		bool IsLocationCacheUpdateUnlockedAfterWait(ZGlobalMutex mutex, int maxWaitInMinutes, CancellationToken token, INotifications notifications)
		{
			var waitCount = 0;
			var waitRetryCount = WhsEnvironment.IsRF ? 30 : maxWaitInMinutes * 60;
			var originalLockInfo = mutex.GetLockInfo();
			while (isOriginalMutexLocked(mutex.GetLockInfo(), originalLockInfo) && waitCount < waitRetryCount)
			{
				token.ThrowIfCancellationRequested();
				Thread.Sleep(1000);
				waitCount++;
			}

			var isMutexLocked = isOriginalMutexLocked(mutex.GetLockInfo(), originalLockInfo);
			if (isMutexLocked && notifications != null)
			{
				notifications.AddError(Res.GetString("8264e843-a3cf-4dad-ad34-9a77287519b0", "Failed to update the putaway location cache. Please try again."));
			}

			return !isMutexLocked;
		}

		bool isOriginalMutexLocked(LockInfo currentLockInfo, LockInfo originalLockInfo)
		{
			return currentLockInfo != null
				&& currentLockInfo.LockStartTime == originalLockInfo.LockStartTime
				&& currentLockInfo.MutexID == originalLockInfo.MutexID
				&& currentLockInfo.RecordID == originalLockInfo.RecordID
				&& currentLockInfo.UserWithLock?.GS_FullName == originalLockInfo.UserWithLock?.GS_FullName
				&& currentLockInfo.HostName == originalLockInfo.HostName
				&& currentLockInfo.ProcessId == originalLockInfo.ProcessId;
		}

		bool UpdatePutawayLocationCacheCore(BusinessObjectFactory factory, IEnumerable<ZGuid> locationsNeedUpdating, ZGlobalMutex mutex, CancellationToken token)
		{
			var putawayLocationCacheUpdated = false;

			try
			{
				var batches = locationsNeedUpdating.Batch(BatchSize);
				foreach (var batch in batches)
				{
					token.ThrowIfCancellationRequested();
					PutawayLocationCacheManager.CreateCache(factory, batch);
				}

				putawayLocationCacheUpdated = true;
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}

			return putawayLocationCacheUpdated;
		}
	}
}
