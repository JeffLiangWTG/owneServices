using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PutawayLocationCacheUpdaterTest : WhsGuiTestCaseWithFactory
	{
		#region TestUpdatePutawayLocationCache

		public void TestUpdatePutawayLocationCache()
		{
			var warehousePK = ZGuid.NewZGuid();
			var location1PK = ZGuid.NewZGuid();
			var location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(new[] { new WhsPutawayLocationCacheInfo(location1PK, warehousePK), new WhsPutawayLocationCacheInfo(location2PK, warehousePK) });

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { location1PK, location2PK }), Times.Once);
				mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory));
			}

			using (var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString()))
			{
				Assert(!updateCacheMutex.IsLocked);
			}
		}

		public void TestUpdatePutawayLocationCache_MultipleWarehouses()
		{
			var warehouse1PK = ZGuid.NewZGuid();
			var warehouse2PK = ZGuid.NewZGuid();
			var whs1Location1PK = ZGuid.NewZGuid();
			var whs1Location2PK = ZGuid.NewZGuid();
			var whs1Location3PK = ZGuid.NewZGuid();
			var whs2Location1PK = ZGuid.NewZGuid();
			var whs2Location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>()))
				.Returns(new[]
				{
					new WhsPutawayLocationCacheInfo(whs1Location1PK, warehouse1PK),
					new WhsPutawayLocationCacheInfo(whs2Location2PK, warehouse2PK),
					new WhsPutawayLocationCacheInfo(whs1Location2PK, warehouse1PK),
					new WhsPutawayLocationCacheInfo(whs2Location1PK, warehouse2PK),
					new WhsPutawayLocationCacheInfo(whs1Location3PK, warehouse1PK),
				});

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { whs1Location1PK, whs1Location2PK, whs1Location3PK }), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { whs2Location2PK, whs2Location1PK }), Times.Once);
				mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory));
				Assert(true);
			}
		}

		#endregion

		#region TestUpdatePutawayLocationCache_Batches

		public void TestUpdatePutawayLocationCache_Batches()
		{
			const int BatchSizes = 500;

			var whsPutawayLocationCacheInfos = new List<WhsPutawayLocationCacheInfo>();
			var warehousePK = ZGuid.NewZGuid();
			for (int i = 0; i < BatchSizes * 5; i++)
			{
				whsPutawayLocationCacheInfos.Add(new WhsPutawayLocationCacheInfo(ZGuid.NewZGuid(), warehousePK));
			}

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(whsPutawayLocationCacheInfos);
			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();

			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(1 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(2 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(3 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(4 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory));
				Assert(true);
			}
		}

		public void TestUpdatePutawayLocationCache_Cancelled()
		{
			const int BatchSizes = 500;

			var whsPutawayLocationCacheInfos = new List<WhsPutawayLocationCacheInfo>();
			var warehousePK = ZGuid.NewZGuid();
			for (int i = 0; i < BatchSizes * 5; i++)
			{
				whsPutawayLocationCacheInfos.Add(new WhsPutawayLocationCacheInfo(ZGuid.NewZGuid(), warehousePK));
			}

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(whsPutawayLocationCacheInfos);
			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();

			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var iteration = 1;
				mockedCacheManager.Setup(mcm => mcm.CreateCache(Factory, It.IsAny<IEnumerable<ZGuid>>()))
					.Callback(() =>
					{
						if (iteration == 2)
						{
							cancellationTokenSource.Cancel();
						}
						iteration++;
					});

				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				AssertExceptionThrown<OperationCanceledException>(() => putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token));
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<ZGuid>>()), Times.Exactly(2));
				mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory));
			}
		}

		#endregion

		#region TestUpdatePutawayLocationCache_MutexLocked

		public void TestUpdatePutawayLocationCache_MutexLocked()
		{
			var warehousePK = ZGuid.NewZGuid();
			var location1PK = ZGuid.NewZGuid();
			var location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(new[] { new WhsPutawayLocationCacheInfo(location1PK, warehousePK), new WhsPutawayLocationCacheInfo(location2PK, warehousePK) });

			var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString());
			updateCacheMutex.Lock();
			Assert(updateCacheMutex.HasLock);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (updateCacheMutex)
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token);
				mockedCacheManager.VerifyNoOtherCalls();
			}
		}

		public void TestUpdatePutawayLocationCache_MultipleWarehouses_MutexLocked()
		{
			var warehouse1PK = ZGuid.NewZGuid();
			var warehouse2PK = ZGuid.NewZGuid();
			var location1PK = ZGuid.NewZGuid();
			var location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(new[] { new WhsPutawayLocationCacheInfo(location1PK, warehouse1PK), new WhsPutawayLocationCacheInfo(location2PK, warehouse2PK) });

			var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehouse1PK.ToString());
			updateCacheMutex.Lock();
			Assert(updateCacheMutex.HasLock);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (updateCacheMutex)
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				putawayLocationCacheUpdater.UpdatePutawayLocationCache(Factory, cancellationTokenSource.Token);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { location2PK }), Times.Once);
				mockedCacheManager.VerifyNoOtherCalls();
			}
		}

		#endregion

		#region TestUpdateWarehousePutawayLocationCacheWithProgressForm

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm()
		{
			var locationPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(new[] { locationPK });

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			mockedCacheManager.Setup(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { locationPK }))
				.Callback(() =>
				{
					Application.DoEvents();

					RetryWithInterval("get ProgressForm", () => Application.OpenForms.OfType<ProgressForm>().SingleOrDefault() == null);
					var progressBarForm = Application.OpenForms.OfType<ProgressForm>().SingleOrDefault();

					RetryWithInterval("get the correct label text", () => progressBarForm.FindSingle<ZLabel>(c => c.Name == "ProgressLabel").Text != "Updating location cache...");
					var label = progressBarForm.FindSingle<ZLabel>(c => c.Name == "ProgressLabel");

					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertEquals("Progress bar should have correct caption.", "Updating location cache...", label.Text);
					AssertEquals("Should have no progress bar.", false, progressBarForm.ShowProgressBar);
					AssertEquals("Should have cancel button.", true, progressBarForm.ShowCancelButton);
				});

			var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
			var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, new NotificationBuffer());
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { locationPK }), Times.Once);
			mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory, warehousePK));
			Assert(result);

			RetryWithInterval("wait the form to close", () => Application.OpenForms.OfType<ProgressForm>().SingleOrDefault() != null);
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());

			using (var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString()))
			{
				Assert(!updateCacheMutex.IsLocked);
			}
		}

		void RetryWithInterval(string actionDescription, Func<bool> retryIfCondition)
		{
			var maxRetryAttempt = 20;
			var retryAttempt = 0;
			while (true)
			{
				if (retryAttempt < maxRetryAttempt)
				{
					if (retryIfCondition())
					{
						Thread.Sleep(200);
						retryAttempt++;
					}
					else
					{
						break;
					}
				}
				else
				{
					throw new InvalidOperationException($"Failed to {actionDescription} after {maxRetryAttempt} attempts.");
				}
			}
		}

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_NoLocations()
		{
			var warehousePK = ZGuid.NewZGuid();
			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(Enumerable.Empty<ZGuid>());

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
			var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, new NotificationBuffer());
			mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory, warehousePK));
			mockedCacheManager.VerifyNoOtherCalls();
			Assert(result);
		}

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_NotUserInteractive()
		{
			var warehousePK = ZGuid.NewZGuid();
			var locationPK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(new[] { locationPK });

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			mockedCacheManager.Setup(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { locationPK }))
				.Callback(() =>
				{
					Thread.Sleep(500);
					Application.DoEvents();
					AssertNull("Should *not* have shown progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, new NotificationBuffer());
				mockedStaleCacheFinder.Verify(mcf => mcf.FindCacheEntriesThatNeedUpdating(Factory, warehousePK));
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { locationPK }), Times.Once);
				Assert(result);
			}
		}

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_Cancelled()
		{
			var warehousePK = ZGuid.NewZGuid();
			const int BatchSizes = 500;

			var pks = new List<ZGuid>();
			for (int i = 0; i < BatchSizes * 5; i++)
			{
				pks.Add(ZGuid.NewZGuid());
			}

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(pks);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			mockedCacheManager.Setup(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<ZGuid>>()))
				.Callback(() =>
				{
					Application.DoEvents();

					RetryWithInterval("get ProgressForm", () => Application.OpenForms.OfType<ProgressForm>().SingleOrDefault() == null);
					var progressBarForm = Application.OpenForms.OfType<ProgressForm>().SingleOrDefault();

					RetryWithInterval("get the correct label text", () => progressBarForm.FindSingle<ZLabel>(c => c.Name == "ProgressLabel").Text != "Updating location cache...");
					var label = progressBarForm.FindSingle<ZLabel>(c => c.Name == "ProgressLabel");

					AssertNotNull("Should have shown progress form.", progressBarForm);
					AssertEquals("Progress bar should have correct caption.", "Updating location cache...", label.Text);
					AssertEquals("Should have no progress bar.", false, progressBarForm.ShowProgressBar);
					AssertEquals("Should have cancel button.", true, progressBarForm.ShowCancelButton);

					var button = (Button)progressBarForm.CancelButton;
					button.Invoke(new Action(button.PerformClick));
				});

			var notifications = new NotificationBuffer();
			var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
			var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, notifications);
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(pks.Take(BatchSizes)))), Times.Once);

			// Should have cancelled after first batch
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(pks.Skip(1 * BatchSizes).Take(BatchSizes)))), Times.Never);
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(pks.Skip(2 * BatchSizes).Take(BatchSizes)))), Times.Never);
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(pks.Skip(3 * BatchSizes).Take(BatchSizes)))), Times.Never);
			mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(pks.Skip(4 * BatchSizes).Take(BatchSizes)))), Times.Never);
			mockedCacheManager.VerifyNoOtherCalls();

			Assert(!result);
			Assert(!notifications.HasErrors);

			RetryWithInterval("wait the form to close", () => Application.OpenForms.OfType<ProgressForm>().SingleOrDefault() != null);
			AssertNull("Should have closed progress form.", Application.OpenForms.OfType<ProgressForm>().SingleOrDefault());
		}

		#endregion

		#region TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked()
		{
			var locationPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(new[] { locationPK });

			var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString());
			updateCacheMutex.Lock();
			Assert(updateCacheMutex.HasLock);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			var notifications = new NotificationBuffer();
			using (updateCacheMutex)
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
				var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, notifications, maxWaitInMinutes: 1);
				mockedCacheManager.VerifyNoOtherCalls();

				Assert(!result);
				Assert(notifications.HasErrors);
				AssertEquals("Failed to update the putaway location cache. Please try again.", notifications.AsString.Trim());
			}
		}

		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLockedWithoutValidUser()
		{
			var locationPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(new[] { locationPK });

			var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString());
			updateCacheMutex.Lock();
			Assert(updateCacheMutex.HasLock);

			var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
						SET SV_ParentId = '{Guid.Empty}',
							SV_SystemLastEditTimeUtc = GetUtcDate(),
							SV_SystemLastEditUser = 'USR'
						FROM dbo.StmServiceSemaphore
						INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
						WHERE SS_LockInfo LIKE '%{MutexIDs.WhsPutawayLocationCacheUpdate.Name}%';";
			TestConnection.ExecuteNonQuery(sql);

			var lockInfo = updateCacheMutex.GetLockInfo();
			AssertNotNull("Lock Info should be created.", lockInfo);
			AssertNull("User should not be loaded.", lockInfo.UserWithLock);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			var notifications = new NotificationBuffer();
			using (updateCacheMutex)
			{
				var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);

				AssertNoExceptionThrown("Null Reference Exception should not be thrown if no valid user exists for LockInfo", () =>
				{
					var result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, warehousePK, notifications, maxWaitInMinutes: 1);
					mockedCacheManager.VerifyNoOtherCalls();

					Assert(!result);
					Assert(notifications.HasErrors);
					AssertEquals("Failed to update the putaway location cache. Please try again.", notifications.AsString.Trim());
				});
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked_MutexCleared()
		{
			TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked_MutexCleared_Core();
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked_MutexCleared_FromRF()
		{
			TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked_MutexCleared_Core(isFromRF: true);
		}

		void TestUpdateWarehousePutawayLocationCacheWithProgressForm_MutexLocked_MutexCleared_Core(bool isFromRF = false)
		{
			var locationPK = ZGuid.NewZGuid();
			var warehousePK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>(), warehousePK)).Returns(new[] { locationPK });

			var updateCacheMutex = new ZGlobalMutex(MutexIDs.WhsPutawayLocationCacheUpdate, warehousePK.ToString());
			updateCacheMutex.Lock();
			Assert(updateCacheMutex.HasLock);

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			var notifications = new NotificationBuffer();
			var result = false;
			var task = new Task(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					if (isFromRF)
					{
						Globals.IsWeb = true;
						Globals.IsUserInteractive = false;
					}
					var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
					var putawayLocationCacheUpdater = new PutawayLocationCacheUpdater(mockedStaleCacheFinder.Object, mockedCacheManager.Object);
					result = putawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(factory2, warehousePK, notifications, maxWaitInMinutes: 1);
				}
			});

			task.Start();
			using (updateCacheMutex)
			{
				Task.Delay(45000).Wait();
				updateCacheMutex.Unlock();
				Assert(!updateCacheMutex.IsLocked);
			}

			task.Wait();
			mockedCacheManager.VerifyNoOtherCalls();

			if (!isFromRF)
			{
				Assert(result);
				Assert(!notifications.HasErrors);
			}
			else
			{
				Assert(!result);
				Assert(notifications.HasErrors);
				AssertEquals("Failed to update the putaway location cache. Please try again.", notifications.AsString.Trim());
			}
		}

		#endregion
	}
}
