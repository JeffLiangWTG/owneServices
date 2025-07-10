using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
public class RefDbVersionControlTriggerFixture
{
	[Test]
	public void TestTriggerThrowException_When_RVC_ParentCode_Is_ZXE_ThrowException()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript_ZXE);
		var updateSql = "update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = 'A3B4C7C0-DA61-4DD5-BEB5-84332B4BAC16'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeListAttributeName was not deleted due to existing RefCusCodeListAttribute.");
	}

	[Test]
	public void TestTriggerThrowException_When_RVC_ParentCode_Is_ZXE_DoesNot_ThrowException()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript_ZXE);
		var updateSql = @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = 'A3B4C7C0-DA61-4DD5-BEB5-84332B4BAC17'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, updateSql);
	}

	[Test]
	public void TestTriggerThrowException_When_RVC_ParentCode_Is_ZZK_DoesNot_ThrowException()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript_ZZK);
		var update1 = @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = '11DEB0BB-09FE-4618-BC29-BCA41CF06C14'";
		var update2 = @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = '11DEB0BB-09FE-4618-BC29-BCA41CF06C15'";

		DBHelper.AssertDoesNotThrowsSqlException(conn, update1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, update2);
	}

	[Test]
	public void TestTriggerThrowException_When_RVC_ParentCode_Is_ZZK_ThrowException_1()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript_ZZK);
		var updateSql = @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = '11DEB0BB-09FE-4618-BC29-BCA41CF06C12'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeType was not deleted due to existing RefCusCodeList.");
	}

	[Test]
	public void TestTriggerThrowException_When_RVC_ParentCode_Is_ZZK_ThrowException_2()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript_ZZK);
		var updateSql = @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = '11DEB0BB-09FE-4618-BC29-BCA41CF06C13'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeType was not deleted due to existing RefCusCodeListAttributeName.");
	}

	const string PrepareDataScript_ZXE = @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'DE', 'Germany')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CodeT', 'Test1', 1, 1, 'DE')

INSERT RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate,ZZD_EndDate,ZZD_ZZZ_NKDataGrouping)
VALUES ('61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'CodeT', 'Code1', 'Descr','1900-01-01 00:00:00','2079-06-06 23:59:00','DE')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption)
VALUES ('A3B4C7C0-DA61-4DD5-BEB5-84332B4BAC16', 'Attr1', 'Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption1'),
('A3B4C7C0-DA61-4DD5-BEB5-84332B4BAC17', 'Attr2', 'Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption2')

INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr1', 'Value')
";

	const string PrepareDataScript_ZZK = @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'DE', 'Germany'),
(NEWID(), 'AU', 'Australia')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES ('11DEB0BB-09FE-4618-BC29-BCA41CF06C12', 'Code1', 'Test1', 1, 1, 'DE'),
('11DEB0BB-09FE-4618-BC29-BCA41CF06C13', 'Code2', 'Test2', 1, 1, 'DE'),
('11DEB0BB-09FE-4618-BC29-BCA41CF06C14', 'Code3', 'Test2', 1, 1, 'AU'),
('11DEB0BB-09FE-4618-BC29-BCA41CF06C15', 'Code4', 'Test3', 1, 1, 'DE')

INSERT RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate,ZZD_EndDate,ZZD_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'Code1', 'Code1', 'Descr','1900-01-01 00:00:00','2079-06-06 23:59:00','DE')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption)
VALUES (NEWID(), 'Attr', 'Description', 'Code2', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption1')
";
}
