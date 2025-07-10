using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusTariffAttributeNameUpdaterInfoFixture
	{
		[TestCase(DbSchema.RemoteDbCollationCS)]
		[TestCase(DbSchema.RemoteDb)]
		[TransactionedTestCase]
		public void RefCusTariffAttributeNameUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			var ZZZ_PK = "339164FE-E316-40F6-9568-3831E9DA0D1A";
			var ZZI_PK = "EA9C8E0F-F99C-4E8A-A77A-15B030EC67FD";
			var ZY6_PK_1 = "C2551555-938E-4989-8F1C-9FE3947A8E31";
			var ZY6_PK_2 = "4C33B44C-87F0-4F9D-A2CC-631E3756B984";
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
VALUES ('{ZZZ_PK}', 'GB', 'For Unit Test', '{ZZZ_PK}');

INSERT RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZ9_NKNomenclatureGroupType, ZZI_ZZZ_NKDataGrouping)
VALUES ('{ZZI_PK}', 'EXP', 'For Unit Test', 'GB', 'GB');

INSERT RefCusTariffAttributeName (ZY6_PK, ZY6_Name, ZY6_Description, ZY6_ZZI_NKTariffType, ZY6_ZZZ_NKDataGrouping, ZY6_ColumnCaption)
VALUES ('{ZY6_PK_1}', 'TestName', 'For Unit Test', 'EXP', 'GB', 'TST'),
('{ZY6_PK_2}', 'TestName2', 'For Unit Test2', 'EXP', 'GB', 'TST2'),
(newID(), 'TestName3', 'For Unit Test3', 'EXP', 'GB', 'TST3')
", trans);
					var info = new RefCusTariffAttributeNameUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT #TempRefCusTariffAttributeName (ZY6_PK, ZY6_Name, ZY6_Description, ZY6_ZZI_NKTariffType, ZY6_ZZZ_NKDataGrouping, ZY6_ColumnCaption, Deleted)
VALUES ('{ZY6_PK_1}', 'TestName', 'For Unit Test', 'EXP', 'GB', 'TST', 1),
('{ZY6_PK_2}', 'TestName2', 'For Unit Test2', 'EXP', 'GB', 'TST2', 0),
(newID(), 'TestName3', 'For Unit Test3', 'EXP', 'GB', 'TST3-1', 0)
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"select count(*) from RefCusTariffAttributeName where ZY6_Name = 'TestName'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"select count(*) from RefCusTariffAttributeName where ZY6_Name = 'TestName2'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"select count(*) from RefCusTariffAttributeName where ZY6_Name = 'TestName3'", trans));
				}
			}
		}
	}
}
