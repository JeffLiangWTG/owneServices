using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using Environment = System.Environment;

namespace CargoWise.RefDataRepo.Ent.Client.ServiceTask.Test
{
	public class RefServiceTaskDataSetUpdaterManagerWrapperTest : TransactionedTestCase
	{
		public void TestDoNotCallILoggerInDifferentThread()
		{
			var threadId = Environment.CurrentManagedThreadId;
			var loggerThreadId = int.MinValue;

			var logger = new Mock<Enterprise.Integration.ILogger>();
			logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => loggerThreadId = Environment.CurrentManagedThreadId);

			var updater = new Mock<IDataSetUpdater>();
			var dependencyProviderMock = new Mock<IUpdaterDependencyProvider<IDataSetUpdater>>();
			dependencyProviderMock.Setup(x => x.GetAllLeaves()).Returns(new[] { updater.Object });
			dependencyProviderMock.Setup(x => x.GetAllRoots()).Returns(new[] { updater.Object });
			dependencyProviderMock.Setup(x => x.GetParents(updater.Object)).Returns(Enumerable.Empty<IDataSetUpdater>);
			dependencyProviderMock.Setup(x => x.GetChildren(updater.Object)).Returns(Enumerable.Empty<IDataSetUpdater>);

			var registrationHelperMock = new Mock<IClientConfiguration>();
			registrationHelperMock.SetupGet(x => x.ClientId).Returns("XX");
			registrationHelperMock.SetupGet(x => x.SystemType).Returns("SYD");
			var errorReportingWrapper = new Mock<ErrorReportingClientWrapper>();

			var serviceLoggerWrapper = new ServiceLoggerWrapper(logger.Object);
			var manager = new DataSetUpdaterManager<IDataSetUpdater>(serviceLoggerWrapper, new Mock<IDbConnection>().Object, dependencyProviderMock.Object, registrationHelperMock.Object, errorReportingWrapper.Object);

			var wrapper = new RefServiceTaskDataSetUpdaterManagerWrapperForTest(logger.Object, new[] { manager });
			wrapper.UpdateAll();

			AssertEquals(loggerThreadId, threadId);
		}

		public void TestGetAllUpdaters()
		{
			var wrapper = new RefServiceTaskDataSetUpdaterManagerWrapperForTest(null, null);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from ServiceTaskDataSetUpdateManagerWrapper",
				() => wrapper.GetAllDataSetUpdater());
		}

		public void TestUpdate()
		{
			var wrapper = new RefServiceTaskDataSetUpdaterManagerWrapperForTest(null, null);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from ServiceTaskDataSetUpdateManagerWrapper",
				() => wrapper.Update("Updater1", "Updater1_DataSetVersion2"));
		}

		public void TestUpdateAll()
		{
			var manager1 = new Mock<IDataSetUpdaterManager>();
			var manager2 = new Mock<IDataSetUpdaterManager>();

			var wrapper = new RefServiceTaskDataSetUpdaterManagerWrapperForTest(null, new[] { manager1.Object, manager2.Object });
			wrapper.UpdateAll();

			manager1.Verify(x => x.UpdateAll(), Times.Once);
			Assert(true);
		}

		public void TestGetDataSetUpdatersWithLogger()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var dbHelper = new Mock<IDBHelper>();
			var updaterRegistration = new Mock<IUpdaterRegistration>();
			var proxy = new Mock<IServerProxy>();
			var versionControlManager = new Mock<IRefVersionControlManager>();
			updaterRegistration.Setup(x => x.Get(It.IsAny<IServerProxy>(), dbHelper.Object)).Returns(new IDataSetUpdater[]
			{
				new RefCurrencyUpdater(proxy.Object, dbHelper.Object, versionControlManager.Object),
			});

			var wrapper = new RefServiceTaskDataSetUpdaterManagerWrapperForTest(logger.Object, null);
			var updaters = wrapper.GetAllDataSetUpdatersWithLogger(updaterRegistration.Object, dbHelper.Object);
			AssertGreaterThan(updaters.Length, 0);
			Assert("All Dataset updaters should have Logger.", updaters.All(x => x.Logger != null));
		}
	}

	public class RefServiceTaskDataSetUpdaterManagerWrapperForTest : RefServiceTaskDataSetUpdaterManagerWrapper
	{
		readonly IEnumerable<IDataSetUpdaterManager> dataSetUpdaterManagers;

		public RefServiceTaskDataSetUpdaterManagerWrapperForTest(Enterprise.Integration.ILogger logger, IEnumerable<IDataSetUpdaterManager> dataSetUpdaterManagers) : base(logger, null)
		{
			this.dataSetUpdaterManagers = dataSetUpdaterManagers;
		}

		protected override IDataSetUpdaterManager SetupManager(IDbConnection dbConnection, IUpdaterRegistration updaterRegistration, IClientConfiguration config)
		{
			return dataSetUpdaterManagers.First();
		}

		public IDataSetUpdater[] GetAllDataSetUpdatersWithLogger(IUpdaterRegistration updaterRegistration, IDBHelper dbHelper)
		{
			return base.GetDataSetUpdatersWithLogger(updaterRegistration, dbHelper);
		}
	}
}
