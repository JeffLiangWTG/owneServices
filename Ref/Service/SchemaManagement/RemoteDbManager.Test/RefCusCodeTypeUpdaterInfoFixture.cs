using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusCodeTypeUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusCodeTypeUpdaterInfo(DbSchema dbSchema)
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
VALUES (NEWID(), 'XXX', 'XXXX')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES ('5C99FC26-73F3-4FFB-851A-03FC99BE1A90', 'AAA', 'Test1', 1, 1, 'ZA'),
('13694F04-2FD2-41CC-8AD9-2222487CE68D', 'BBB', 'Test2', 1, 1, 'ZA'),
('872B42AD-E8E0-45FA-B3D0-48392664F1FC', 'DDD', 'Test4', 1, 1, 'ZA')

INSERT RefCusCodeTypeLanguage (ZXI_PK,ZXI_ZX6_NKLanguage,ZXI_ZZK_CodeType,ZXI_Description)
VALUES(NEWID(),'XXX','5C99FC26-73F3-4FFB-851A-03FC99BE1A90','AAA'),
(NEWID(),'XXX','13694F04-2FD2-41CC-8AD9-2222487CE68D', 'BBB'),
(NEWID(),'XXX','872B42AD-E8E0-45FA-B3D0-48392664F1FC', 'DDD')", trans);
					var info = new RefCusCodeTypeUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping, Deleted)
VALUES ('D4D7301A-FE13-4A28-9500-EAC5C16C0AB8', 'AAA', 'Test1111', 1, 1, 'ZA', 0),
(NEWID(), 'BBB', 'Test2', 1, 1, 'ZA', 1),
('75241B39-A06D-432F-B6FA-0A29C88654BB', 'CCC', 'Test3', 1, 1, 'ZA', 0),
(NEWID(), 'DDD', 'Test4', 1, 1, 'ZA', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXI_PK,ZXI_ZX6_NKLanguage,ZXI_ZZK_CodeType,ZXI_Description)
VALUES(NEWID(),'XXX','D4D7301A-FE13-4A28-9500-EAC5C16C0AB8','WW'),
(NEWID(),'XXX','75241B39-A06D-432F-B6FA-0A29C88654BB','CCC')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeType WHERE ZZK_CodeType = 'AAA' AND ZZK_Description = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeType WHERE ZZK_CodeType = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeType WHERE ZZK_CodeType = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeType WHERE ZZK_CodeType = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeTypeLanguage WHERE ZXI_Description = 'WW'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeTypeLanguage WHERE ZXI_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeTypeLanguage WHERE ZXI_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeTypeLanguage WHERE ZXI_Description = 'DDD'", trans));
				}
			}
		}
	}
}
