using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class UNDGVersionUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void UNDGVersionUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName));
			conn.Open();
			using var trans = conn.BeginTransaction();
			TestDBHelper.ExecuteNonQuery(conn, @"
INSERT UNDGVersion (DV_PK, DV_Name, DV_Standard, DV_IsActive)
VALUES (NEWID(), 'version 1', 'ADN', 1),
(NEWID(), 'version 2', 'ADR', 1)", trans);
			var info = new UNDGVersionUpdaterInfo_1();
			TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);

			TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (DV_PK, DV_Name, DV_Standard, DV_IsActive, Deleted)
VALUES (NEWID(), 'version 3', 'CFR', 1, 0),
(NEWID(), 'version 2', 'ADR', 1, 1),
(NEWID(), 'version 5', 'JTT', 1, 0)", trans);
			TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
			Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGVersion WHERE DV_Name = 'version 1'", trans), Is.EqualTo(1));
			Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGVersion WHERE DV_Name = 'version 2'", trans), Is.EqualTo(0));
			Assert.That(TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM UNDGVersion WHERE DV_Name = 'version 5'", trans), Is.EqualTo(1));
		}
	}
}
