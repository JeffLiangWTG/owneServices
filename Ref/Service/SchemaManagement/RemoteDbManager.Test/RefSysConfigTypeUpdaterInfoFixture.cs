using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefSysConfigTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefSysConfigTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefSysConfigType(ZRT_PK,ZRT_ConfigCode,ZRT_Description,ZRT_LongDescription)
VALUES (NEWID(), 'AAA', 'T1', 'Test1'),
(NEWID(), 'BBB', 'T2', 'Test2'),
(NEWID(), 'DDD', 'T4', 'Test4')

INSERT RefSysConfig(ZRC_PK,ZRC_StartDate,ZRC_EndDate,ZRC_DecimalValue,ZRC_StringValue,ZRC_BitValue,ZRC_ZRT_NKConfigCode)
VALUES(NEWID(),'1900-01-01','2020-01-01',1,'',0,'AAA'),
(NEWID(),'1900-01-01','2020-01-01',2,'',0,'BBB'),
(NEWID(),'1900-01-01','2020-01-01',2,'',0,'DDD')", trans);
					var info = new RefSysConfigTypeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZRT_PK,ZRT_ConfigCode,ZRT_Description,ZRT_LongDescription, Deleted)
VALUES (NEWID(), 'AAA', 'T1', 'Test1111', 0),
(NEWID(), 'BBB', 'T2', 'Test2', 1),
(NEWID(), 'CCC', 'T3', 'Test3', 0),
(NEWID(), 'DDD', 'T4', 'Test4', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZRC_PK,ZRC_StartDate,ZRC_EndDate,ZRC_DecimalValue,ZRC_StringValue,ZRC_BitValue,ZRC_ZRT_NKConfigCode)
VALUES(NEWID(),'1900-01-01','2020-01-01',11,'',0,'AAA'),
(NEWID(),'1900-01-01','2020-01-01',3,'',0,'CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfigType WHERE ZRT_ConfigCode = 'AAA' AND ZRT_LongDescription = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfigType WHERE ZRT_ConfigCode = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfigType WHERE ZRT_ConfigCode = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfigType WHERE ZRT_ConfigCode = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfig WHERE ZRC_ZRT_NKConfigCode = 'AAA' AND ZRC_DecimalValue = 11", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfig WHERE ZRC_ZRT_NKConfigCode = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfig WHERE ZRC_ZRT_NKConfigCode = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefSysConfig WHERE ZRC_ZRT_NKConfigCode = 'DDD'", trans));
				}
			}
		}
	}
}
