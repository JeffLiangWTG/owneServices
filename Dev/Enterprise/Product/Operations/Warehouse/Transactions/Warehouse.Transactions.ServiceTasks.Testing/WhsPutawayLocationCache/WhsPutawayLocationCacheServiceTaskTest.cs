using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPutawayLocationCacheServiceTask))]
	public class WhsPutawayLocationCacheServiceTaskTest : ServiceTaskTestCase<WhsPutawayLocationCacheServiceTask>
	{
		#region TestInitialiseTask

		public void TestInitialiseTask()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		#endregion

		#region TestServiceTask

		public void TestServiceTask()
		{
			var logger = InitialiseAndRunServiceTask();

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information|WhsPutawayLocationCache Update Service Task started.
Information|WhsPutawayLocationCache Update Service Task completed.
".Trim(), logger.ToString());
		}

		#endregion

		#region TestHostedServiceAttribute

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertEquals("Warehouse Putaway Location Cache Maintenance Service Task", hostedServiceAttribute.Description);
			AssertEquals("WPC", hostedServiceAttribute.Code);
			AssertEquals("WHS", hostedServiceAttribute.Category);
			AssertEquals(typeof(WhsPutawayLocationCacheServiceTask), hostedServiceAttribute.Type);
			AssertEquals("15Minutes", hostedServiceAttribute.MinimumPeriod);
		}

		#endregion

		#region TestRunTask

		public void TestRunTask()
		{
			var mockedPutawayLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			using (ObjectFactory.Substitute(mockedPutawayLocationCacheUpdater.Object))
			{
				var serviceLog = InitialiseAndRunServiceTask();
				AssertEquals(2, serviceLog.Count);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task started.", serviceLog[0]);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task completed.", serviceLog[1]);
				mockedPutawayLocationCacheUpdater.Verify(mcm => mcm.UpdatePutawayLocationCache(It.IsAny<BusinessObjectFactory>(), It.IsAny<CancellationToken>()), Times.Once);
			}
		}

		public void TestRunTask_EndToEnd()
		{
			var warehousePK = ZGuid.NewZGuid();
			var location1PK = ZGuid.NewZGuid();
			var location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(new[] { new WhsPutawayLocationCacheInfo(location1PK, warehousePK), new WhsPutawayLocationCacheInfo(location2PK, warehousePK) });

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (ObjectFactory.Substitute(mockedStaleCacheFinder.Object))
			using (ObjectFactory.Substitute(mockedCacheManager.Object))
			{
				var serviceLog = InitialiseAndRunServiceTask();
				AssertEquals(2, serviceLog.Count);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task started.", serviceLog[0]);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task completed.", serviceLog[1]);
				mockedStaleCacheFinder.Verify(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>()), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { location1PK, location2PK }), Times.Once);
			}
		}

		public void TestRunTask_EndToEnd_MultipleWarehouses()
		{
			var warehouse1PK = ZGuid.NewZGuid();
			var warehouse2PK = ZGuid.NewZGuid();
			var location1PK = ZGuid.NewZGuid();
			var location2PK = ZGuid.NewZGuid();

			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(new[] { new WhsPutawayLocationCacheInfo(location1PK, warehouse1PK), new WhsPutawayLocationCacheInfo(location2PK, warehouse2PK) });

			var mockedCacheManager = new Mock<IWhsPutawayLocationCacheManager>();
			using (ObjectFactory.Substitute(mockedStaleCacheFinder.Object))
			using (ObjectFactory.Substitute(mockedCacheManager.Object))
			{
				var serviceLog = InitialiseAndRunServiceTask();
				AssertEquals(2, serviceLog.Count);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task started.", serviceLog[0]);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task completed.", serviceLog[1]);
				mockedStaleCacheFinder.Verify(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>()), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { location1PK }), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), new[] { location2PK }), Times.Once);
			}
		}

		public void TestRunTask_EndToEnd_NoStaleLocations()
		{
			var mockedStaleCacheFinder = new Mock<IStalePutawayLocationCacheFinder>();
			mockedStaleCacheFinder.Setup(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>())).Returns(Enumerable.Empty<WhsPutawayLocationCacheInfo>());

			var mockedPutawayLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			using (ObjectFactory.Substitute(mockedStaleCacheFinder.Object))
			{
				var serviceLog = InitialiseAndRunServiceTask();
				AssertEquals(2, serviceLog.Count);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task started.", serviceLog[0]);
				AssertEquals("Information|WhsPutawayLocationCache Update Service Task completed.", serviceLog[1]);
				mockedStaleCacheFinder.Verify(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>()), Times.Once);
				mockedPutawayLocationCacheUpdater.VerifyNoOtherCalls();
			}
		}

		#endregion

		#region TestRunTask_Batches_Cancelled

		public void TestRunTask_Batches_Cancelled()
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
			using (ObjectFactory.Substitute(mockedStaleCacheFinder.Object))
			using (ObjectFactory.Substitute(mockedCacheManager.Object))
			{
				mockedCacheManager.Setup(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(1 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK))))).Callback(cancellationTokenSource.Cancel);

				var task = new WhsPutawayLocationCacheServiceTask();

				AssertExceptionThrown<OperationCanceledException>(() => InitialiseAndRunTaskSchedule(task, cancellationTokenSource.Token));
				mockedStaleCacheFinder.Verify(scf => scf.FindCacheEntriesThatNeedUpdating(It.IsAny<BusinessObjectFactory>()), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(1 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Once);

				// Should have cancelled after second batch
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(2 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Never);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(3 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Never);
				mockedCacheManager.Verify(mcm => mcm.CreateCache(It.IsAny<BusinessObjectFactory>(), It.Is<IEnumerable<ZGuid>>(c => c.SequenceEqual(whsPutawayLocationCacheInfos.Skip(4 * BatchSizes).Take(BatchSizes).Select(info => info.LocationPK)))), Times.Never);
			}
		}

		#endregion

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(WhsPutawayLocationCacheServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == WhsPutawayLocationCacheServiceTask.Code);
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestServiceLogger InitialiseAndRunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new WhsPutawayLocationCacheServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("WPC", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
