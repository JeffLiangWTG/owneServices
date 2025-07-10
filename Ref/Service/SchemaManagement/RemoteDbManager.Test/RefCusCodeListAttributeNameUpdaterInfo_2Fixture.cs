using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusCodeListAttributeNameUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusCodeListAttributeNameUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'DGP', 'South Africa')

INSERT RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES (NEWID(), 'XXX', 'XXXX')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CodeT', 'Test1', 1, 1, 'DGP')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption)
VALUES 
('75241B39-A06D-432F-B6FA-0A29C88654BB', 'ZXE_Name1', 'ZXE_Description1', 'CodeT', 'DGP', 1, 1, 1, NULL, '', 1, 2, 0, 'ZXE_ColumnCaption1')
,(NEWID(), 'ZXE_Name2', 'ZXE_Description2', 'CodeT', 'DGP', 1, 1, 1, NULL, '', 1, 2, 0, 'ZXE_ColumnCaption2')
,(NEWID(), 'ZXE_Name3', 'ZXE_Description3', 'CodeT', 'DGP', 1, 1, 1, NULL, '', 1, 2, 0, 'ZXE_ColumnCaption3')
,(NEWID(), 'ZXE_Name4', 'ZXE_Description4', 'CodeT', 'DGP', 1, 1, 1, NULL, '', 1, 2, 0, 'ZXE_ColumnCaption4')
,(NEWID(), 'ZXE_Name5', 'ZXE_Description5', 'CodeT', 'DGP', 1, 1, 1, NULL, '', 1, 2, 0, 'ThisShouldKeepAsNoMatchFromServer')

INSERT RefCusCodeListAttributeNameLanguage (ZXH_PK,ZXH_ZX6_NKLanguage,ZXH_ZXE_CodeListAttributeName,ZXH_Description,ZXH_Name,ZXH_ColumnCaption)
VALUES(NEWID(),'XXX','75241B39-A06D-432F-B6FA-0A29C88654BB', '', '', '')
", trans);
					var info = new RefCusCodeListAttributeNameUpdaterInfo_2();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption, Deleted)
VALUES
('75241B39-A06D-432F-B6FA-0A29C88654BB',	'ZXE_Name1', 'ZXE_Description1FromServer', 'CodeT', 'DGP', 1, 1, 1, NULL, 'String', 1, 2, 0, 'ThisRecordKeepsZXE_Name1', 0)
,(NEWID(),	'ZXE_Name2', 'ZXE_Description2FromServer', 'CodeT', 'DGP', 1, 1, 1, NULL, 'String', 1, 2, 0, 'ThisRecordDeleteZXE_Name2', 1)
,(NEWID(),	'ZXE_Name3FromServer', 'ZXE_Description3FromServer', 'CodeT', 'DGP', 1, 1, 1, NULL, 'String', 1, 2, 0, 'ZXE_ColumnCaption3', 0)
,(NEWID(),	'ZXE_Name4FromServer', 'ZXE_Description4FromServer', 'CodeT', 'DGP', 1, 1, 1, NULL, 'String', 1, 2, 0, 'ZXE_ColumnCaption4', 1)

INSERT {info.PrepareTemporaryTablesScripts.LastOrDefault().Key} (ZXH_PK,ZXH_ZX6_NKLanguage,ZXH_ZXE_CodeListAttributeName,ZXH_Description,ZXH_Name,ZXH_ColumnCaption)
VALUES(NEWID(),'XXX','75241B39-A06D-432F-B6FA-0A29C88654BB', 'WW', 'A', 'A')
", trans);
					// Test result
					// #1 should be kept
					// #2 should be deleted because it is marked as Deleted=1 and match by 1st index
					// #3 should be kept but its name should be overwritten by server data
					// #4 should be deleted because it is marked as Deleted=1 and match by 2nd index
					// #5 should be kept because there is no match by either index
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name1'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name2'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name3FromServer'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name4'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name5'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_Name = 'ZXE_Name3FromServer' AND ZXE_Description='ZXE_Description3FromServer' AND ZXE_ColumnCaption='ZXE_ColumnCaption3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_ColumnCaption = 'ThisRecordKeepsZXE_Name1'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_ColumnCaption = 'ThisShouldKeepAsNoMatchFromServer'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeName WHERE ZXE_ColumnCaption = 'ThisRecordDeleteZXE_Name2'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'WW'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'BBB'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'CCC'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusCodeListAttributeNameLanguage WHERE ZXH_Description = 'DDD'", trans));
				}
			}
		}
	}
}
