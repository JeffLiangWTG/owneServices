using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	[TestFixture]
	class DBHelperFixture
	{
		[Test]
		[TransactionedTestCase]
		public void TurnOffAndOnHistoryTable()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
			conn.Open();
			using var trans = conn.BeginTransaction();
			DbHelper.ExecuteNonQuery(trans, @"
CREATE TABLE Department
(
    DeptID INT NOT NULL PRIMARY KEY CLUSTERED
  , SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL
  , SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL
  , PERIOD FOR SYSTEM_TIME (SysStartTime,SysEndTime)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.DepartmentHistory));");
			var sql = "SELECT temporal_type from sys.tables WHERE name = '{0}'";
			Assert.AreEqual(2, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "Department")));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "DepartmentHistory")));
			DbHelper.SetSystemVersioningOff(trans, "Department");
			Assert.AreEqual(0, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "Department")));
			Assert.AreEqual(0, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "DepartmentHistory")));
			DbHelper.SetSystemVersioningOn(trans, "Department");
			Assert.AreEqual(2, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "Department")));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(trans, string.Format(CultureInfo.InvariantCulture, sql, "DepartmentHistory")));
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestCheckDbIfExist()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreateor = new DbCreator(conn);
			dbCreateor.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			DBHelper.CreateDatabase(conn, TestDbName, path);
		
			var isExist = DBHelper.CheckIfDbExist(conn, TestDbName);
			Assert.That(isExist, Is.True);
		
			var isNotExist = DBHelper.CheckIfDbExist(conn, "NonExistingDb");
			Assert.That(isNotExist, Is.False);
		
			DBHelper.TearDown(conn, path, TestDbName);
		}

		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestSetAndGetExtendedProperties()
		{
			using var conn = new SqlConnection(TestConnectionString.GetAdmin(null));
			conn.Open();
			var dbCreateor = new DbCreator(conn);
			dbCreateor.DropDatabase(TestDbName);
			var path = Path.Combine(Path.GetTempPath(), TestDbName);
			DBHelper.CreateDatabase(conn, TestDbName, path);

			DBHelper.SetTestDbExtendedProperties(conn, "asdfas23", TestDbName);
			Assert.That(DBHelper.GetExtendedProperty(conn, TestDbName, TestDbExtendedProperties.DacPacFileModelHash), Is.EqualTo("asdfas23"));

			DBHelper.TearDown(conn, path, TestDbName);
		}

		const string TestDbName = "RefDbRepo0FB6525B9D92494FA6B0CE62932C3B1B";
	}
}
