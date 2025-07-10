using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
class RefCusCodeListAttributeTrigger_AttributeNameFixture
{
	[Test]
	public void TestTrigger_When_RVC_ParentCode_Is_ZXE_ThrowException_1()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		DBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF412', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZXE_NKName = 'Attr3' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF412'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName.");
	}

	[Test]
	public void TestTrigger_When_RVC_ParentCode_Is_ZXE_DoesNot_ThrowException_1()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		DBHelper.ExecuteNonQuery(conn,
			@"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF412', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZXE_NKName = 'Attr4' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF412'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, updateSql);
	}

	[Test]
	public void TestTrigger_When_RVC_ParentCode_Is_ZXE_ThrowException_2()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		DBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF413', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZZD_CodeList = '61DEB0BB-09FE-4618-BC29-BCA41CF06C94' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF413'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName.");
	}

	[Test]
	public void TestTrigger_When_RVC_ParentCode_Is_ZXE_DoesNot_ThrowException_2()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		DBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF413', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZZD_CodeList = '61DEB0BB-09FE-4618-BC29-BCA41CF06C93' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF413'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, updateSql);
	}

	[Test]
	public void TestTrigger_When_RVC_ParentCode_Is_ZXE_ThrowException_3()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		DBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		DBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF413', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		DBHelper.ExecuteNonQuery(conn, @"update RefDbVersionControl set RVC_Deleted = 1 where RVC_ParentPK = 'A3B5C7C0-DA61-4DD5-BEB5-84332B4BAC12'");
		var updateSql = @"update RefCusCodeListAttribute set ZZE_ZZD_CodeList = '61DEB0BB-09FE-4618-BC29-BCA41CF06C95' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF413'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName.");
	}

	const string PrepareDataScript = @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'DE', 'Germany')
,(NEWID(), 'AU', 'Australia')
,(NEWID(), 'HK', 'Hong Kong')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CodeT', 'Test1', 1, 1, 'DE')
,(NEWID(), 'CodeT', 'Test2', 1, 1, 'AU')
,(NEWID(), 'CodeT', 'Test3', 1, 1, 'HK')

INSERT RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES ('61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'CodeT', 'Code1', 'Descr', 'DE', '1900-01-01 00:00:00', '2079-06-06 23:59:00')
,('61DEB0BB-09FE-4618-BC29-BCA41CF06C93', 'CodeT', 'Code2', 'Descr', 'DE', '1900-01-01 00:00:00', '2079-06-06 23:59:00')
,('61DEB0BB-09FE-4618-BC29-BCA41CF06C94', 'CodeT', 'Code3', 'Descr', 'AU', '1900-01-01 00:00:00', '2079-06-06 23:59:00')
,('61DEB0BB-09FE-4618-BC29-BCA41CF06C95', 'CodeT', 'Code3', 'Descr', 'HK', '1900-01-01 00:00:00', '2079-06-06 23:59:00')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption, ZXE_IsDateRangeUsed)
VALUES
(NEWID(), 'Attr2', 'Attr2 Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption2', 1)
,('A3B5C7C0-DA61-4DD5-BEB5-84332B4BAC12', 'Attr2', 'Attr2 Description', 'CodeT', 'HK', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption2', 1)
,(NEWID(), 'Attr3', 'Attr3 Description', 'CodeT', 'AU', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption3', 1)
,(NEWID(), 'Attr4', 'Attr4 Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption4', 1)
";
}
