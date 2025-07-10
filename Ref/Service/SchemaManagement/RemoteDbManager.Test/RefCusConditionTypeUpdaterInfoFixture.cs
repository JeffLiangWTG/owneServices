using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConditionTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusConditionTypeUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'WW', 'WWW')

INSERT RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES ('93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CLASS', 'AAA', 'Test1', 'ZA'),
('CB682E16-FE25-4A2F-9997-A5AF0CA6A37E', 'CLASS', 'BBB', 'Test2', 'ZA'),
('13CB1F8E-7793-43C8-B401-B5DBE6FA9782', 'CLASS', 'DDD', 'Test4', 'ZA')

INSERT RefCusConditionTypeLanguage (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'93E985C5-E990-4ABC-9E81-376FB9795AA8','WW', 'AAA'),
(NEWID(),'CB682E16-FE25-4A2F-9997-A5AF0CA6A37E','WW','BBB'),
(NEWID(),'13CB1F8E-7793-43C8-B401-B5DBE6FA9782','WW','DDD')", trans);
					var info = new RefCusConditionTypeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping, Deleted)
VALUES ('93E985C5-E990-4ABC-9E81-376FB9795AA8', 'CLASS', 'AAA', 'Test1111', 'ZA', 0),
('CB682E16-FE25-4A2F-9997-A5AF0CA6A37E', 'CLASS', 'BBB', 'Test2', 'ZA', 1),
('99DD2453-AA65-44FC-AA93-90077980F5DB', 'CLASS', 'CCC', 'Test3', 'ZA', 0),
(NEWID(), 'CLASS', 'DDD', 'Test4', 'ZA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXW_PK, ZXW_ZX2_ConditionType, ZXW_ZX6_NKLanguage, ZXW_Description)
VALUES(NEWID(),'93E985C5-E990-4ABC-9E81-376FB9795AA8','WW', 'QQQ'),
(NEWID(),'99DD2453-AA65-44FC-AA93-90077980F5DB','WW', 'CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionClass != 'CLASS'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'AAA' AND ZX2_Description = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionType WHERE ZX2_ConditionType = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionTypeLanguage WHERE ZXW_Description = 'DDD'", trans));
				}
			}
		}
	}
}
