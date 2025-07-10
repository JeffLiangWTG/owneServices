using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RemoteDbCollationCsSchemaFixture
	{
		[Test]
		public void NoDiffFromRemoteDbSchema()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RemoteDbCollationCS);
			var schemaUpgrade = new SchemaUpgrade("RemoteDb.dacpac", Common.Infrastructure.Test.TestConnectionString.DataSource, null, null);
			var diff = schemaUpgrade.GetDiffSql(dbName);
			Assert.That(diff, Is.EqualTo(string.Empty), "RemoteDbCollationCS should have exactly the same schema as RemoteDb, except for collation. Please check that new objects introduced in RemoteDb project are also included in RemoteDbCollationCS project.");
		}

		[Test]
		public void CheckDatabaseCollation()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RemoteDbCollationCS);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				Assert.AreEqual("SQL_Latin1_General_CP1_CS_AS", TestDBHelper.GetCollation(conn, dbName),
					"Collation should be SQL_Latin1_General_CP1_CS_A");
			}
		}
	}
}
