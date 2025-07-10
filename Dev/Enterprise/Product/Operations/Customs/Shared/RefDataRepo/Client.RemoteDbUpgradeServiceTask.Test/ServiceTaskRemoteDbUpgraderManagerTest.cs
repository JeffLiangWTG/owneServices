using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask.Test
{
	public class ServiceTaskRemoteDbUpgraderManagerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDoUpgrade()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			upgrader.Setup(x => x.DoUpgrade(It.IsAny<IDbTransaction>())).Returns(true);
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				DataUtils.DropDbExtendedProperty(conn, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName);
				Assert(!conn.DatabaseExists(testDbName));
				var manager = new ServiceTaskRemoteDbUpgraderManagerForTest(logger.Object, upgrader.Object, errorReporter.Object);
				manager.Update();

				upgrader.Verify(x => x.DoUpgrade(It.IsAny<IDbTransaction>()), Times.Once);
				Assert(conn.DatabaseExists(testDbName));
				Assert(true);
			}
		}

		[UseSnapshotProtection]
		public void TestDoUpgrade_WhenCanNotUpgradeSRDB()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			var error = new Exception("Test Exception");
			upgrader.Setup(x => x.DoUpgrade(It.IsAny<IDbTransaction>())).Throws(error);
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				DataUtils.DropDbExtendedProperty(conn, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName);
				Assert(!conn.DatabaseExists(testDbName));
				var manager = new ServiceTaskRemoteDbUpgraderManagerForTest(logger.Object, upgrader.Object, errorReporter.Object);
				manager.Update();
				logger.Verify(x => x.Log(LogType.Error, string.Format("Please use Help -> Database Administraion -> Reference Data option and try again")), Times.Once);
				logger.Verify(x => x.Log(LogType.Error, "Can not upgrade SRDb because " + error.ToString()), Times.Once);
				upgrader.Verify(x => x.GetServerResponse());
			}
		}

		[UseSnapshotProtection]
		public void TestNotUpgrade_WhenSingleRefDatabaseConsumeDATSnapshot()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				DataUtils.SaveDbExtendedProperty(conn, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, "Y", RefDbTableNameResolver.DefaultSingleRefDbName);

				Assert(!conn.DatabaseExists(testDbName));
				var manager = new ServiceTaskRemoteDbUpgraderManagerForTest(logger.Object, upgrader.Object, errorReporter.Object);
				manager.Update();
				logger.Verify(x => x.Log(LogType.Error, string.Format("SRDb is set to use DAT Snapshot, RDU won't run, please go to Testing -> Reset Single Reference Database -> Use Reference Service menu to switch")), Times.Once);
				DataUtils.DropDbExtendedProperty(conn, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName);
			}
		}

		[UseSnapshotProtection]
		public void TestDoUpgradeResult()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				DataUtils.DropDbExtendedProperty(conn, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName);
				upgrader.Setup(x => x.DoUpgrade(It.IsAny<IDbTransaction>())).Returns(true);
				var manager = new ServiceTaskRemoteDbUpgraderManagerForTest(logger.Object, upgrader.Object, errorReporter.Object);
				var result = manager.Update();
				AssertEquals(true, result);
				logger.Verify(x => x.Log(LogType.Information, $"Start upgrading {manager.refDatabaseName} schemas on server {Db.ServerName}."), Times.Once);
				logger.Verify(x => x.Log(LogType.Information, $"Upgraded {manager.refDatabaseName} on server {Db.ServerName} successfully."), Times.Once);

				upgrader.Setup(x => x.DoUpgrade(It.IsAny<IDbTransaction>())).Returns(false);
				result = manager.Update();
				AssertEquals(false, result);
				logger.Verify(x => x.Log(LogType.Information, $"Upgraded {manager.refDatabaseName} on server {Db.ServerName} failed."), Times.Once);
			}
		}

		public void TestCallErrorReporterOnFailure()
		{
			var logger = new Mock<Enterprise.Integration.ILogger>();
			var exception = new Exception("There is an error");
			logger.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Throws(exception);
			var upgrader = new Mock<IRefDataBaseUpgrader>();
			var errorReporter = new Mock<IErrorReportingClientWrapper>();
			var upgradeManagerForTest = new ServiceTaskRemoteDbUpgraderManagerForTest(logger.Object, upgrader.Object, errorReporter.Object);
			upgradeManagerForTest.Update();
			errorReporter.Verify(x => x.PostCrashReport(It.IsAny<Exception>()), Times.Once);
			logger.Verify(x => x.Log(LogType.Error, exception.ToString()));
			Assert(true);
		}

		protected override void TearDown()
		{
			base.TearDown();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, testDbName, Db.DatabaseName);
			}
		}

		static readonly string testDbName = RefDbTableNameResolver.SingleRefDatabaseName + "RDUServiceTaskTestRandom";
	}

	public class ServiceTaskRemoteDbUpgraderManagerForTest : ServiceTaskRemoteDbUpgraderManager
	{
		readonly IRefDataBaseUpgrader upgrader;
		public override string refDatabaseName => RefDbTableNameResolver.SingleRefDatabaseName + "RDUServiceTaskTestRandom";

		public ServiceTaskRemoteDbUpgraderManagerForTest(Enterprise.Integration.ILogger logger, IRefDataBaseUpgrader upgrader, IErrorReportingClientWrapper errorReportingWrapper) : base(logger, errorReportingWrapper, new[] { Db.ServerName })
		{
			this.upgrader = upgrader;
		}

		protected override IRefDataBaseUpgrader SetupUpgrader(IDbConnection connection)
		{
			return upgrader;
		}
	}
}
