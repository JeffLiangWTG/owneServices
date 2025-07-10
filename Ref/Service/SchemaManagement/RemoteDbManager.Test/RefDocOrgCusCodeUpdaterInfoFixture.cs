using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefDocOrgCusCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefDocOrgCusCodeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDocOrgCusCode (DOC_PK, DOC_RN_NKRegulatingCountry, DOC_RN_NKCodeCountry, DOC_CodeType, DOC_DocumentType, DOC_Priority, DOC_Notes, DOC_ShortLabel, DOC_LongLabel, DOC_Description)
VALUES (NEWID(), 'A', 'A', 'A', 'HAW', 1, 'A', 'A', 'A', 'A'),
(NEWID(), 'B', 'B', 'B', 'HAW', 1, 'B', 'B', 'B', 'B')", trans);
					var info = new RefDocOrgCusCodeUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (DOC_PK, DOC_RN_NKRegulatingCountry, DOC_RN_NKCodeCountry, DOC_CodeType, DOC_DocumentType, DOC_Priority, DOC_Notes, DOC_ShortLabel, DOC_LongLabel, DOC_Description, Deleted)
VALUES (NEWID(), 'A', 'A', 'A', 'HAW', 1, 'A', 'A', 'A', 'X', 0),
(NEWID(), 'B', 'B', 'B', 'HAW', 1, 'B', 'B', 'B', 'B', 1),
(NEWID(), 'C', 'B', 'B', 'HAW', 1, 'B', 'B', 'B', 'B', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDocOrgCusCode WHERE DOC_RN_NKRegulatingCountry = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDocOrgCusCode WHERE DOC_RN_NKRegulatingCountry = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefDocOrgCusCode WHERE DOC_RN_NKRegulatingCountry = 'A' AND DOC_Description = 'X'", trans));
				}
			}
		}
	}
}
