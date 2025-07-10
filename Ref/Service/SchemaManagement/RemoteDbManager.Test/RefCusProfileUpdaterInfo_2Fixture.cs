using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusProfileUpdaterInfo_2Fixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusProfileUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					var refCusProfile = new RefCusProfile();
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa');
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', '1P1', '1P1','ZA');

INSERT INTO RefCusProfileType (XXX_PK,XXX_ProfileType,XXX_ZZI_TariffType,XXX_Description,XXX_ZZZ_NKDataGrouping)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','A Description','ZA'),
('D272AC2C-C3CB-4981-851E-764899C55D73','B','7EABDFE4-F6F0-4F33-BC3A-D20863ED5075','B Description','ZA');

INSERT INTO RefCusProfile (XX0_PK,XX0_XXX_ProfileType,XX0_AppliesToCode,XX0_QuestionCode,XX0_StartDate,XX0_EndDate,XX0_ZZZ_NKDataGrouping, XX0_AllowMultipleAnswers, XX0_IsAnswerMandatory)
VALUES ('9EC9F58A-0DE4-48AA-8081-18A45E5E216F','1799FE44-08BE-42CF-B130-E89921BC3E85','1001','QC1','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1), --keep
('6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','D272AC2C-C3CB-4981-851E-764899C55D73','1002','QC2','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1),           --update
('53404306-8565-4655-BFFC-1C96DAAB754E','D272AC2C-C3CB-4981-851E-764899C55D73','1003','QC3','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1);           --delete

INSERT INTO RefCusProfileAttribute(XXY_PK,XXY_XX0_Profile,XXY_Name,XXY_Value)
VALUES (NEWID(),'6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','Name1','Value1'),
(NEWID(),'6EDA47B3-F6E3-4FAE-819E-4CD5BC107E98','Name2','Value2'),
(NEWID(),'53404306-8565-4655-BFFC-1C96DAAB754E','Name3','Value3');
", trans);

					var info = new RefCusProfileUpdaterInfo_2();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}
					TestDBHelper.ExecuteNonQuery(conn, $@"
					INSERT INTO #TempRefCusProfile (XX0_PK,XX0_XXX_ProfileType,XX0_AppliesToCode,XX0_QuestionCode,XX0_StartDate,XX0_EndDate,XX0_ZZZ_NKDataGrouping,XX0_AllowMultipleAnswers,XX0_IsAnswerMandatory,Deleted)
					VALUES ('62319CF2-095E-4FEF-BB63-090D7303155C','1799FE44-08BE-42CF-B130-E89921BC3E85','2001','QC21','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1,0),                --insert
					('1B540903-4156-4AA7-AB4F-B376866F640E','D272AC2C-C3CB-4981-851E-764899C55D73','1002 Update','QC2 Update','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1,0),   --update
					('B487B019-E207-4ADA-8839-2B1691E53FA9','D272AC2C-C3CB-4981-851E-764899C55D73','1003','QC3','2024-01-01 00:00:00.000','2079-06-06 23:59:00.000','ZA',1,1,1);                           --delete

					INSERT INTO #TempRefCusProfileAttribute(XXY_PK,XXY_XX0_Profile,XXY_Name,XXY_Value)
					VALUES (NEWID(),'62319CF2-095E-4FEF-BB63-090D7303155C','Name11','Value11'),
					(NEWID(),'62319CF2-095E-4FEF-BB63-090D7303155C','Name12','Value12'),
					(NEWID(),'1B540903-4156-4AA7-AB4F-B376866F640E','Name21','Value21');
					", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_PK = '9EC9F58A-0DE4-48AA-8081-18A45E5E216F'", trans));
					Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileAttribute WHERE XXY_XX0_Profile = '62319CF2-095E-4FEF-BB63-090D7303155C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_AppliesToCode = '1002 Update' AND XX0_QuestionCode = 'QC2 Update'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfileAttribute JOIN RefCusProfile ON XXY_XX0_Profile = XX0_PK WHERE XX0_AppliesToCode = '1002 Update' AND XX0_QuestionCode = 'QC2 Update'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_AppliesToCode = '1003' AND XX0_QuestionCode = 'QC3'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, "SELECT COUNT(1) FROM RefCusProfile WHERE XX0_PK = '9EC9F58A-0DE4-48AA-8081-18A45E5E216F' AND XX0_AllowMultipleAnswers = 1 AND XX0_IsAnswerMandatory = 1", trans));
				}
			}
		}
	}
}
