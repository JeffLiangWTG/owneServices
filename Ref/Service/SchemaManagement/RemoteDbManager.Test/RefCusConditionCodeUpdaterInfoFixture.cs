using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConditionCodeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusConditionCodeUpdaterInfo(DbSchema dbSchema)
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

INSERT RefCusConditionCode (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping)
VALUES ('7E10794C-D89F-4290-B943-2E169EBF1953', 'AAA', 'Test1', 'ZA'),
('10C6E81C-33FC-4D9E-9E54-5AC6C635C37F', 'BBB', 'Test2', 'ZA'),
('C7C53D4D-ECA6-4A86-8605-BE24780C88EB', 'CCC', 'Test3', 'ZA')

INSERT RefCusConditionCodeLanguage (ZY8_PK, ZY8_ZY7_ConditionCode, ZY8_ZX6_NKLanguage, ZY8_Description)
VALUES(NEWID(),'7E10794C-D89F-4290-B943-2E169EBF1953','WW', 'language1'),
(NEWID(),'10C6E81C-33FC-4D9E-9E54-5AC6C635C37F','WW','language2'),
(NEWID(),'C7C53D4D-ECA6-4A86-8605-BE24780C88EB','WW','language3')", trans);
					var info = new RefCusConditionCodeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZY7_PK, ZY7_ConditionCode, ZY7_Description, ZY7_ZZZ_NKDataGrouping, Deleted)
VALUES ('7E10794C-D89F-4290-B943-2E169EBF1953', 'AAA', 'Test1111', 'ZA', 0),
('10C6E81C-33FC-4D9E-9E54-5AC6C635C37F', 'BBB', 'Test2', 'ZA', 1),
('6B9C4A1D-6E33-4EA7-9A33-7B8774F2EC52', 'CCC', 'Test3', 'ZA', 0),
(NEWID(), 'DDD', 'Test4', 'ZA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZY8_PK, ZY8_ZY7_ConditionCode, ZY8_ZX6_NKLanguage, ZY8_Description)
VALUES(NEWID(),'7E10794C-D89F-4290-B943-2E169EBF1953','WW', 'RRR'),
(NEWID(),'6B9C4A1D-6E33-4EA7-9A33-7B8774F2EC52','WW', 'CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCode WHERE ZY7_ConditionCode = 'AAA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCode WHERE ZY7_ConditionCode = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCode WHERE ZY7_ConditionCode = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'language1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'language2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'language3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'RRR'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(*) FROM RefCusConditionCodeLanguage WHERE ZY8_Description = 'CCC'", trans));
				}
			}
		}
	}
}
