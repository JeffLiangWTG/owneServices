using System;
using System.Data;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	[TestFixture]
	class UpgradeManagerFixture
	{
		[Test]
		[CreateDatabase("256DC0D8A87048CF909BDD1FE36BF2E8", DbSchema.None)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void Run()
		{
			var testDbName = CreateDatabaseAttribute.DbNamePrefix + "256DC0D8A87048CF909BDD1FE36BF2E8";
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(null)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.ExcuteDbScript(testDbName, @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
Column2 char(3)
)", null);
				var versionMgr = new Mock<ISchemaVersionManager>();
				versionMgr.Setup(x => x.GetVersion(It.IsAny<IDbTransaction>())).Returns(0);
				var schemaUpgrade = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null);
				var manager = new UpgradeManager(conn, testDbName, new DummyTransformationManager(new IDataTransformationTask[0]),
					new DummyTransformationManager(new IDataTransformationTask[0]), 1, schemaUpgrade, versionMgr.Object, dbCreator, new Mock<IDbLockout>().Object, false);
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $"SELECT count(*) from {testDbName}.INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'TestXXX' and COLUMN_NAME = 'Column3'";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
					manager.Run();
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
			}
		}

		[Test]
		[CreateDatabase("256DC0D8A87048CF909BDD1FE36BF2E8", DbSchema.None)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void RunWithPreUpgrade()
		{
			var testDbName = CreateDatabaseAttribute.DbNamePrefix + "256DC0D8A87048CF909BDD1FE36BF2E8";
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(null)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.ExcuteDbScript(testDbName, @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
)", null);
				var versionMgr = new Mock<ISchemaVersionManager>();
				versionMgr.Setup(x => x.GetVersion(It.IsAny<IDbTransaction>())).Returns(0);
				var schemaUpgrade = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null);
				var preUpgradeTask = new Mock<IDataTransformationTask>();
				var preUpgradeTaskRun = false;
				preUpgradeTask.Setup(x => x.Version).Returns(1);
				preUpgradeTask.Setup(x => x.Run(It.IsAny<IDbTransaction>())).Callback<IDbTransaction>(trans =>
				{
					using (var cmd = trans.Connection.CreateCommand())
					{
						cmd.CommandText = @"ALTER TABLE TestXXX ADD column4 char(3)";
						cmd.Transaction = trans;
					}
					preUpgradeTaskRun = true;
				});
				var manager = new UpgradeManager(conn, testDbName, new DummyTransformationManager(new[] { preUpgradeTask.Object }),
					new DummyTransformationManager(new IDataTransformationTask[0]), 1, schemaUpgrade, versionMgr.Object, dbCreator, new Mock<IDbLockout>().Object, false);
				manager.Run();
				Assert.True(preUpgradeTaskRun);
			}
		}

		[Test]
		[CreateDatabase("256DC0D8A87048CF909BDD1FE36BF2E8", DbSchema.None)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void RunWithGreaterOrSameVersionWhenForceUpgrade()
		{
			var testDbName = CreateDatabaseAttribute.DbNamePrefix + "256DC0D8A87048CF909BDD1FE36BF2E8";
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(null)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.ExcuteDbScript(testDbName, @"CREATE TABLE TestXXX (
Column1 uniqueidentifier,
Column2 char(3)
)", null);
				var versionMgr = new Mock<ISchemaVersionManager>();
				versionMgr.Setup(x => x.GetVersion(It.IsAny<IDbTransaction>())).Returns(1);
				var schemaUpgrade = new SchemaUpgrade("DbTest.dacpac", TestConnectionString.DataSource, null, null);
				var manager = new UpgradeManager(conn, testDbName, new DummyTransformationManager(new IDataTransformationTask[0]),
					new DummyTransformationManager(new IDataTransformationTask[0]), 1, schemaUpgrade, versionMgr.Object, dbCreator, new Mock<IDbLockout>().Object, true);
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $"SELECT count(*) from {testDbName}.INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'TestXXX' and COLUMN_NAME = 'Column3'";
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
					manager.Run();
					Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				}
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void ErrorReportingTest()
		{
			var testDbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
			var errorReporter = new Mock<ErrorReportingClientWrapper>(null);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(null)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				var versionMgr = new Mock<ISchemaVersionManager>();
				versionMgr.Setup(x => x.GetVersion(It.IsAny<IDbTransaction>())).Returns(1);
				var schemaUpgrade = new SchemaUpgrade("NonExistent.dacpac", TestConnectionString.DataSource, null, null);
				var manager = new UpgradeManager(conn, testDbName, new DummyTransformationManager([]),
					new DummyTransformationManager([]), 1, schemaUpgrade, versionMgr.Object, dbCreator, new Mock<IDbLockout>().Object, true, errorReporter.Object);
				using (var cmd = conn.CreateCommand())
				{
					try
					{
						manager.Run();
					}
					catch (Exception)
					{ }
				}
			}
			errorReporter.Verify(x => x.PostCrashReport(It.IsAny<Exception>(), null, null), Times.Once);
		}
	}
}
