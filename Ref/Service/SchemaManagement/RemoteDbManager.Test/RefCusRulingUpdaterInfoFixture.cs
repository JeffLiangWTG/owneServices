using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusRulingUpdaterInfoFixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusRulingUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT INTO RefCusRuling (ZZX_PK,ZZX_RN_NKCountryCode,ZZX_RulingNumber,ZZX_Description,ZZX_RulingType,ZZX_StartDate,ZZX_EndDate)
VALUES ('03F192EA-D899-4EB0-867C-DE8AC4CF66C3','NZ','Ruling Number','D','GST','1900-01-01','2079-06-06 23:59:00'),
('7A04BADA-BE27-4AC9-841E-17551E034664','BB','RulingB','D','GST','1900-01-01','2079-06-06 23:59:00')

INSERT INTO RefCusRulingConfig (ZZY_PK,ZZY_Category,ZZY_Type,ZZY_Rate,ZZY_Value,ZZY_ZZX_CusRuling)
VALUES (NEWID(),'GST', 'T', 1, 'V', '03F192EA-D899-4EB0-867C-DE8AC4CF66C3'),
(NEWID(),'EXC', 'T', 1, 'V', '7A04BADA-BE27-4AC9-841E-17551E034664')
", trans);
					var info = new RefCusRulingUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefCusRuling (ZZX_PK,ZZX_RN_NKCountryCode,ZZX_RulingNumber,ZZX_Description,ZZX_RulingType,ZZX_StartDate,ZZX_EndDate, Deleted)
VALUES (NEWID(),'NZ','Ruling Number','D','GST','1900-01-01','2079-06-06 23:59:00', 1),
('E9AFA04D-BC81-4A85-8803-E9E2AA720004','CC','CCCC','D','GST','1900-01-01','2079-06-06 23:59:00', 0),
('18F98C73-2196-41E4-8CC6-2EBF6C759557','BB','BBBB','D','GST','1900-01-01','2079-06-06 23:59:00', 0)

INSERT INTO #TempRefCusRulingConfig (ZZY_PK,ZZY_Category,ZZY_Type,ZZY_Rate,ZZY_Value,ZZY_ZZX_CusRuling)
VALUES (NEWID(),'SIM', 'T', 1, 'XC', 'E9AFA04D-BC81-4A85-8803-E9E2AA720004'),
(NEWID(),'EXC', 'T', 1, 'XB', '18F98C73-2196-41E4-8CC6-2EBF6C759557')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRuling WHERE ZZX_RN_NKCountryCode = 'NZ' AND ZZX_RulingNumber = 'Ruling Number'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRulingConfig WHERE ZZY_Category = 'GST'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRuling WHERE ZZX_RN_NKCountryCode = 'BB' AND ZZX_RulingNumber = 'BBBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRulingConfig WHERE ZZY_Category = 'EXC' AND ZZY_Value = 'XB'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRuling WHERE ZZX_RN_NKCountryCode = 'CC' AND ZZX_RulingNumber = 'CCCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusRulingConfig WHERE ZZY_Category = 'SIM' AND ZZY_Value = 'XC'", trans));
				}
			}
		}
	}
}
