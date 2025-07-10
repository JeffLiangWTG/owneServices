using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusAUNexdocECMCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusAUNexdocECMCodeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefCusAUNexdocECMCode (ZY5_PK,ZY5_CommodityCode,ZY5_PreservationCode,ZY5_ProductTypeCode,ZY5_PackTypeCode,ZY5_SupplementaryCode)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A')", trans);
					var info = new RefCusAUNexdocECMCodeUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZY5_PK,ZY5_CommodityCode,ZY5_PreservationCode,ZY5_ProductTypeCode,ZY5_PackTypeCode,ZY5_SupplementaryCode, Deleted)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 1),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusAUNexdocECMCode WHERE ZY5_CommodityCode = 'A'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusAUNexdocECMCode WHERE ZY5_CommodityCode = 'B'", trans));
				}
			}
		}
	}
}
