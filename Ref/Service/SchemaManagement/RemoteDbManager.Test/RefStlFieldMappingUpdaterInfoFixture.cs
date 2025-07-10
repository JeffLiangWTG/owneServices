using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefStlFieldMappingUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlFieldMappingUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT INTO RefStlFieldMapping (SFM_PK, SFM_FeatureCode, SFM_BillableCount, SFM_Reference1, SFM_Reference2, SFM_Reference3, SFM_Reference4)
VALUES(newid(), 'ADD', '1', 'REF1', 'REF2', 'REF3', 'REF4'),
(newid(), 'APP', '1', 'REF1', 'REF2', 'REF3', 'REF4'),
(newid(), 'PAK', '1', 'REF1', 'REF2', 'REF3', 'REF4')", trans);
					var info = new RefStlFieldMappingUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (SFM_PK, SFM_FeatureCode, SFM_BillableCount, SFM_Reference1, SFM_Reference2, SFM_Reference3, SFM_Reference4,Deleted)
VALUES(newid(), 'ADD', '1', 'REF1', 'REF2', 'REF3', 'REF4', 0),
(newid(), 'APP', '1', 'REF1', 'REF2', 'REF3', 'REF4', 1),
(newid(), 'PAK', '1', 'REFERENCE1', 'REF2', 'REF3', 'REF4', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlFieldMapping WHERE SFM_FeatureCode = 'ADD'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlFieldMapping WHERE SFM_FeatureCode = 'APP'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlFieldMapping WHERE SFM_FeatureCode = 'PAK'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlFieldMapping WHERE SFM_FeatureCode = 'PAK' AND SFM_Reference1 = 'REF1'", trans));
				}
			}
		}
	}
}
