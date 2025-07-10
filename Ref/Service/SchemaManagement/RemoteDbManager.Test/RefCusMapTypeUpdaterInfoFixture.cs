using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusMapTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusMapTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefCusMapType (ZZP_PK, ZZP_MapType, ZZP_Direction, ZZP_Description, ZZP_IsReadonly)
VALUES (NEWID(), 'A', 'BTH', 'AAA', 1),
(NEWID(), 'B', 'BTH', 'BBB', 1)", trans);
					var info = new RefCusMapTypeUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZP_PK, ZZP_MapType, ZZP_Direction, ZZP_Description, ZZP_IsReadonly, Deleted)
VALUES (NEWID(), 'A', 'BTH', 'XXX', 1, 0),
(NEWID(), 'B', 'BTH', 'BBB', 1, 1),
(NEWID(), 'C', 'BTH', 'CCC', 1, 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMapType WHERE ZZP_MapType = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMapType WHERE ZZP_MapType = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusMapType WHERE ZZP_MapType = 'A' AND ZZP_Description = 'XXX'", trans));
				}
			}
		}
	}
}
