using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	[TestFixture]
	class DbCreatorFixture
	{
		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void CreateAndDropDatabase()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreator = new DbCreator(conn);
			dbCreator.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			Directory.CreateDirectory(path);
			dbCreator.CreateDatabase(TestDbName, path);
			using (var cmd = conn.CreateCommand())
			{
				cmd.Parameters.Add("@dbName", System.Data.SqlDbType.NVarChar).Value = TestDbName;
				cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				dbCreator.DropDatabase(TestDbName);
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
			}
			Directory.Delete(path);
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void CreateAndDropDatabaseWithCollation()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreator = new DbCreator(conn);
			dbCreator.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			Directory.CreateDirectory(path);
			dbCreator.CreateDatabase(TestDbName, path, "SQL_Latin1_General_CP1_CS_AS");
			using (var cmd = conn.CreateCommand())
			{
				cmd.Parameters.Add("@dbName", System.Data.SqlDbType.NVarChar).Value = TestDbName;
				cmd.CommandText = "SELECT collation_name FROM sys.databases WHERE name = @dbName";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo("SQL_Latin1_General_CP1_CS_AS"));
				dbCreator.DropDatabase(TestDbName);
				cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));
			}
			Directory.Delete(path);
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void ExecuteSqlScript()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreateor = new DbCreator(conn);
			dbCreateor.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			Directory.CreateDirectory(path);
			dbCreateor.CreateDatabase(TestDbName, path);
			dbCreateor.ExcuteDbScript(TestDbName, "CREATE TABLE TestXXX ( ID uniqueidentifier )", null);
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $"SELECT COUNT(*) FROM {TestDbName}.sys.tables WHERE name = 'TestXXX'";
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
			}
			dbCreateor.DropDatabase(TestDbName);
			Directory.Delete(path);
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestExecuteSqlScriptWithTimeoutException()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreateor = new DbCreator(conn);
			dbCreateor.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			Directory.CreateDirectory(path);
			dbCreateor.CreateDatabase(TestDbName, path);
			try
			{
				dbCreateor.ExcuteDbScript(TestDbName, LongRunningDbScipt, null);
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no exception, but got error: " + ex.Message);
			}
			dbCreateor.DropDatabase(TestDbName);
			Directory.Delete(path);
		}

		const string LongRunningDbScipt = "WAITFOR DELAY '00:00:40' CREATE TABLE TestXXX(ID uniqueidentifier)";
		const string TestDbName = "RefDbRepo9FCBD10E4AE5460BA503F5384ED3F22C";
	}
}
