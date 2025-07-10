using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConditionValueTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusConditionValueTypeUpdaterInfo(DbSchema dbSchema)
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


INSERT RefCusConditionValueType (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping)
VALUES ('DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E', 'AAA', 'Test1', 1, 'ZA'),
('F94C22D5-57BC-43DE-B680-9CF1DF50CB59', 'BBB', 'Test2', 1, 'ZA'),
('A3554EC4-DBDF-4FD3-9113-8A5C17C37F5C', 'DDD', 'Test4', 1, 'ZA')

INSERT RefCusConditionValueTypeLanguage (ZXX_PK, ZXX_ZX4_ValueType, ZXX_ZX6_NKLanguage, ZXX_Description)
VALUES(NEWID(),'DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E','WW', 'AAA'),
(NEWID(),'F94C22D5-57BC-43DE-B680-9CF1DF50CB59','WW', 'BBB'),
(NEWID(),'A3554EC4-DBDF-4FD3-9113-8A5C17C37F5C','WW', 'DDD')", trans);
					var info = new RefCusConditionValueTypeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping, Deleted)
VALUES ('DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E', 'AAA', 'Test1111', 1, 'ZA', 0),
('F94C22D5-57BC-43DE-B680-9CF1DF50CB59', 'BBB', 'Test2', 1, 'ZA', 1),
('B966B0DB-583F-4A7A-8B6F-AC2B1E75249B', 'CCC', 'Test3', 1, 'ZA', 0),
('A3554EC4-DBDF-4FD3-9113-8A5C17C37F5C', 'DDD', 'Test4', 1, 'ZA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXX_PK, ZXX_ZX4_ValueType, ZXX_ZX6_NKLanguage, ZXX_Description)
VALUES(NEWID(),'DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E','WW', 'QQQ'),
(NEWID(),'B966B0DB-583F-4A7A-8B6F-AC2B1E75249B','WW', 'CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueType WHERE ZX4_ValueType = 'AAA' AND ZX4_Description = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueType WHERE ZX4_ValueType = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueType WHERE ZX4_ValueType = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueType WHERE ZX4_ValueType = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description = 'QQQ'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description = 'DDD'", trans));
				}
			}
		}
	}
}
