using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusNomenclatureGroupTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusNomenclatureGroupTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefCusNomenclatureGroupType (ZZ9_PK, ZZ9_GroupType, ZZ9_Description)
VALUES (NEWID(), 'A', 'AAA'),
(NEWID(), 'B', 'BBB')", trans);
					var info = new RefCusNomenclatureGroupTypeUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZ9_PK, ZZ9_GroupType, ZZ9_Description, Deleted)
VALUES (NEWID(), 'A', 'XXX', 0),
(NEWID(), 'B', 'BBB', 1),
(NEWID(), 'C', 'CCC', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupType WHERE ZZ9_GroupType = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupType WHERE ZZ9_GroupType = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusNomenclatureGroupType WHERE ZZ9_GroupType = 'A' AND ZZ9_Description = 'XXX'", trans));
				}
			}
		}
	}
}
