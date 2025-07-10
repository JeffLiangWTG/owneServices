using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusCodeListAttributeNameUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusCodeListAttributeNameUpdaterInfo(DbSchema dbSchema)
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
VALUES ('5C99FC26-73F3-4FFB-851A-03FC99BE1A90', 'QQQ', 'Test1', 1, 1, 'ZA')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption)
VALUES ('75241B39-A06D-432F-B6FA-0A29C88654BB', 'AAA', 'Test1', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A1'),
('928FDCC1-1128-494C-A8DC-33FFEDB42E19', 'BBB', 'Test2', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A2'),
('971CB8EC-E5CE-4E9A-B996-03997F73F769', 'DDD', 'Test4', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A4')

INSERT RefCusCodeListAttributeNameLanguage (ZXH_PK,ZXH_ZX6_NKLanguage,ZXH_ZXE_CodeListAttributeName,ZXH_Description,ZXH_Name,ZXH_ColumnCaption)
VALUES(NEWID(),'XXX','75241B39-A06D-432F-B6FA-0A29C88654BB', 'AAA', 'A', 'A'),
(NEWID(),'XXX','928FDCC1-1128-494C-A8DC-33FFEDB42E19', 'BBB', 'B', 'B'),
(NEWID(),'XXX','971CB8EC-E5CE-4E9A-B996-03997F73F769', 'DDD', 'D', 'D')", trans);
					var info = new RefCusCodeListAttributeNameUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption, Deleted)
VALUES ('75241B39-A06D-432F-B6FA-0A29C88654BB', 'AAA', 'Test1111', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A1', 0),
(NEWID(), 'BBB', 'Test2', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A', 1),
('684F762C-E375-4F49-9700-1541CCF45F7F', 'CCC', 'Test3', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A3', 0),
(NEWID(), 'DDD', 'Test4', 'QQQ', 'ZA', 1, 1, 1, NULL, 'String', 1, 2, 0, 'A', 0)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXH_PK,ZXH_ZX6_NKLanguage,ZXH_ZXE_CodeListAttributeName,ZXH_Description,ZXH_Name,ZXH_ColumnCaption)
VALUES(NEWID(),'XXX','75241B39-A06D-432F-B6FA-0A29C88654BB', 'WW', 'A', 'A'),
(NEWID(), 'XXX','684F762C-E375-4F49-9700-1541CCF45F7F', 'CCC', 'C', 'C')", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'AAA' AND ZXE_Description = 'Test1111'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'CCC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'DDD'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'WW'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'BBB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'DDD'", trans));
				}
			}
		}
	}
}
