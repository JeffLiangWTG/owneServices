using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
public class RemoveDuplicateRefCusCodeListTransformationFixture
{
	[Test]
	[TransactionedTestCase]
	public void Run()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, @"
ALTER INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZN_NKCodeType_ZZD_Code ON RefCusCodeList  
DISABLE;
				");

		PrepareData(dbCreator, dbName);

		using (var trans = conn.BeginTransaction())
		{
			var task = new RemoveDuplicateRefCusCodeListTransformation(1);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = "SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_PK IN ('9C608FBE-BB06-431C-A455-43550F39808E')";
			Assert.AreEqual(1, (int)cmd.ExecuteScalar());
			cmd.CommandText = "SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_PK IN ('05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9')";
			Assert.AreEqual(0, (int)cmd.ExecuteScalar());
		}

		Assert.DoesNotThrow(() => dbCreator.ExcuteDbScript(dbName, @"
ALTER INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZN_NKCodeType_ZZD_Code ON RefCusCodeList  
REBUILD;
				"));
	}

	void PrepareData(DbCreator dbCreator, string dbName)
	{
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'AU', 'X')");
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
VALUES(newid(), 'USFPC', 'X' , 1, 'AU')");
		dbCreator.ExcuteDbScript(dbName, @"insert into RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
values (newid(), 'Name1', 'Name1', 'USFPC', 'AU'),
(newid(), 'Name2', 'Name2', 'USFPC', 'AU'),
(newid(), 'Name3', 'Name3', 'USFPC', 'AU')");
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES('05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'USFPC', 'TST', 'Desc', '2019-03-05', '2079-06-06 23:59:00', 'AU'),
('CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'USFPC', 'TST', 'Desc', '2019-03-06', '2079-06-06 23:59:00', 'AU'),
('9C608FBE-BB06-431C-A455-43550F39808E', 'USFPC', 'TST', 'Desc', '2019-03-07', '2079-06-06 23:59:00', 'AU')

INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES ('D788FA5A-C9D1-409E-A320-2478B0FE984A', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'Name1', 'Value1'),
('7BCE0E31-494C-4B34-9309-CB58BE1C90F6', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'Name2', 'Value2'),
('BD10D399-6D5F-4788-B16D-844FC0AEBEA2', '9C608FBE-BB06-431C-A455-43550F39808E', 'Name3', 'Value3')

INSERT INTO RefCusCodeOrAttributeTransportMode(ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute, ZZU_DataSetPK, ZZU_DataSetCode)
VALUES(newid(), 'AIR', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', NULL, '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'ZZD'),
(newid(), 'AIR', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', NULL, 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'ZZD'),
(newid(), 'AIR', '9C608FBE-BB06-431C-A455-43550F39808E', NULL, '9C608FBE-BB06-431C-A455-43550F39808E', 'ZZD'),
(newid(), 'SEA', NULL, 'D788FA5A-C9D1-409E-A320-2478B0FE984A', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'ZZD'),
(newid(), 'SEA', NULL, '7BCE0E31-494C-4B34-9309-CB58BE1C90F6', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'ZZD'),
(newid(), 'SEA', NULL, 'BD10D399-6D5F-4788-B16D-844FC0AEBEA2', '9C608FBE-BB06-431C-A455-43550F39808E', 'ZZD')

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES(newid(), 'EN', 'English')

INSERT INTO RefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES (newid(), 'EN', '05E4CE24-B6AD-4F7B-B7ED-3C78FBC8B6C3', 'Desc1'),
(newid(), 'EN', 'CD079D46-EB75-4AB3-B73C-497A3C86C0E9', 'Desc2'),
(newid(), 'EN', '9C608FBE-BB06-431C-A455-43550F39808E', 'Desc3')");
	}
}
