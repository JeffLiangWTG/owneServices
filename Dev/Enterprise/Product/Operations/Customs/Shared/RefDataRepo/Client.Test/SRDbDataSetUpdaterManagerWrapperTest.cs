using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class SRDbDataSetUpdaterManagerWrapperTest : TransactionedTestCase
	{
		public void TestDoNotCallILoggerInDifferentThread()
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var loggerThreadId = int.MinValue;

			var logger = new Mock<Enterprise.Integration.ILogger>();
			logger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(() => loggerThreadId = Thread.CurrentThread.ManagedThreadId);

			var mapper = new Mock<IDataSetUpdaterMapper>();
			var updater = new Mock<IDataSetUpdater>();
			var dependencyProviderMock = new Mock<IUpdaterDependencyProvider<IDataSetUpdater>>();
			dependencyProviderMock.Setup(x => x.GetAllLeaves()).Returns(new[] { updater.Object });
			dependencyProviderMock.Setup(x => x.GetAllRoots()).Returns(new[] { updater.Object });
			dependencyProviderMock.Setup(x => x.GetParents(updater.Object)).Returns(Enumerable.Empty<IDataSetUpdater>);
			dependencyProviderMock.Setup(x => x.GetChildren(updater.Object)).Returns(Enumerable.Empty<IDataSetUpdater>);

			var registrationHelperMock = new Mock<IClientConfiguration>();
			registrationHelperMock.SetupGet(x => x.ClientId).Returns("XX");
			registrationHelperMock.SetupGet(x => x.SystemType).Returns("SYD");
			var errorReportingWrapper = new Mock<IErrorReportingClientWrapper>();

			var serviceLoggerWrapper = new ServiceLoggerWrapper(logger.Object);
			var manager = new DataSetUpdaterManager<IDataSetUpdater>(serviceLoggerWrapper, new Mock<IDbConnection>().Object, dependencyProviderMock.Object, registrationHelperMock.Object, errorReportingWrapper.Object);
			var dataSetVersion = new DataSetVersion("Updater1_DataSetVersion2", DateTime.Now);

			mapper.Setup(x => x.GetDataSetUpdater("Updater1")).Returns(updater.Object);
			mapper.Setup(x => x.GetDataSetUpdaterManager("Updater1")).Returns(manager);
			mapper.Setup(x => x.GetDataSetVersion("Updater1", "Updater1_DataSetVersion2")).Returns(dataSetVersion);

			var wrapper = new SRDbDataSetUpdaterManagerWrapperForTest(mapper.Object);
			wrapper.SetLogger(logger.Object);
			wrapper.Update("Updater1", "Updater1_DataSetVersion2");

			AssertEquals(loggerThreadId, threadId);
		}

		public void TestGetAllUpdaters()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();

			mapper.Setup(x => x.GetDataSets()).Returns(new[]
			{
				new SharedDataSetUpdater("Updater1", new[] { "Updater1_DataSetVersion1", "Updater1_DataSetVersion2" }),
				new SharedDataSetUpdater("Updater2", new[] { "Updater2_DataSetVersion1" })
			});

			var wrapper = new SRDbDataSetUpdaterManagerWrapperForTest(mapper.Object);
			var dataSetUpdaters = wrapper.GetAllDataSetUpdater();

			Assert("Updater count do not match", dataSetUpdaters.Count() == 2);
		}

		public void TestUpdate()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();
			var manager = new Mock<IDataSetUpdaterManager>();
			var updater = new Mock<IDataSetUpdater>();
			var dataSetVersion = new DataSetVersion("Updater1_DataSetVersion2", DateTime.Now);

			mapper.Setup(x => x.GetDataSetUpdater("Updater1")).Returns(updater.Object);
			mapper.Setup(x => x.GetDataSetUpdaterManager("Updater1")).Returns(manager.Object);
			mapper.Setup(x => x.GetDataSetVersion("Updater1", "Updater1_DataSetVersion2")).Returns(dataSetVersion);

			var wrapper = new RefDataSetUpdaterManagerWrapperForTest(mapper.Object);
			wrapper.Update("Updater1", "Updater1_DataSetVersion2");

			manager.Verify(x => x.Update(dataSetVersion, updater.Object), Times.Once);
			Assert(true);
		}

		public void TestUpdateAll()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();
			var wrapper = new SRDbDataSetUpdaterManagerWrapperForTest(mapper.Object);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from ServiceTaskDataSetUpdateManagerWrapper",
				() => wrapper.UpdateAll());
		}

		public void TestInitializeSRDbDataSetUpdaterMapperIfRequired()
		{
			var wrapper = new SRDbDataSetUpdaterManagerWrapperForTest(new Mock<IDataSetUpdaterMapper>().Object);
			Assert(wrapper.dbConnection.State == ConnectionState.Open);
			Assert(wrapper.dbConnection.CurrentDatabase == RefDbTableNameResolver.SingleRefDatabaseName);

			wrapper.dbConnection.CloseConnection();
			wrapper.GetAllDataSetUpdater();
			Assert(wrapper.dbConnection.State == ConnectionState.Open);
			Assert(wrapper.dbConnection.CurrentDatabase == RefDbTableNameResolver.SingleRefDatabaseName);
		}
	}

	public class SRDbDataSetUpdaterManagerWrapperForTest : SRDbDataSetUpdaterManagerWrapper
	{
		public SRDbDataSetUpdaterManagerWrapperForTest(IDataSetUpdaterMapper dataSetUpdaterMapper)
		{
			sRDbMapper = dataSetUpdaterMapper;
		}

		protected override string InitialDatabase => RefDbTableNameResolver.SingleRefDatabaseName;

		protected override bool preConditionCheck => true;
	}
}
