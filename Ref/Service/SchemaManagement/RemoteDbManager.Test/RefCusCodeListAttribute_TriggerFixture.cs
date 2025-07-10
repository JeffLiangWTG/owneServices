using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test;

[TestFixture]
[TransactionedTestCase]
class RefCusCodeListAttribute_TriggerFixture
{
	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Insert_ThrowException_1(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr1', 'Value1', '2024-01-01', '2025-01-01')";

		DBHelper.AssertThrowsSqlException(conn, sql, "RefCusCodeListAttribute was inserted with non-null values for ZZE_StartDate and/or ZZE_EndDate while ZXE_IsDateRangeUsed = 0");
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Insert_ThrowException_2(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql1 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2024-01-01', '2025-01-01')";
		var sql2 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2025-01-01 00:01', '2079-01-01')";
		var sql3 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value3', '2020-01-01', '2024-02-02')";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);

		var sql = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2020-01-01', '2024-02-02')";

		DBHelper.AssertThrowsSqlException(conn, sql, "RefCusCodeListAttribute was inserted with overlaping ZZE_StartDate/ZZE_EndDate values");
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Insert_ThrowException_3(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql1 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2024-01-01', '2025-01-01')";
		var sql2 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2025-01-01 00:01', '2079-01-01')";
		var sql3 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value3', '2020-01-01', '2024-02-02')";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);

		var sql = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', null, null)";

		DBHelper.AssertThrowsSqlException(conn, sql, "RefCusCodeListAttribute was inserted with overlaping ZZE_StartDate/ZZE_EndDate values");
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Update_ThrowException_1(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql1 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2024-01-01', '2025-01-01')";
		var sql2 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2025-01-01 00:01', '2079-01-01')";
		var sql3 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value3', '2020-01-01', '2024-02-02')";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);
		TestDBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF412', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");

		var updateSql= "update RefCusCodeListAttribute set ZZE_ZXE_NKName = 'Attr3' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF412'";
		DBHelper.AssertThrowsSqlException(conn, updateSql, "A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName");
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Update_ThrowException_2(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql1 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2024-01-01', '2025-01-01')";
		var sql2 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2025-01-01 00:01', '2079-01-01')";
		var sql3 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value3', '2020-01-01', '2024-02-02')";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);
		TestDBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF412', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZXE_NKName = 'Attr4' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF412'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, updateSql);
		TestDBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF413', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");

		var sql = "update RefCusCodeListAttribute set ZZE_ZZD_CodeList = '61DEB0BB-09FE-4618-BC29-BCA41CF06C94' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF413'";
		DBHelper.AssertThrowsSqlException(conn, sql, "A RefCusCodeListAttribute was not inserted/updated due to an invalid/non-existent RefCusCodeListAttributeName");
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusCodeListAttribute_Trigger_Update_DoesNot_ThrowException(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql1 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2024-01-01', '2025-01-01')";
		var sql2 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value2', '2025-01-01 00:01', '2079-01-01')";
		var sql3 = @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES (NEWID(), '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value3', '2020-01-01', '2024-02-02')";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);
		TestDBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF412', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");
		var updateSql = "update RefCusCodeListAttribute set ZZE_ZXE_NKName = 'Attr4' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF412'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, updateSql);
		TestDBHelper.ExecuteNonQuery(conn, @"INSERT RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES ('A2142364-8116-4ECA-A66A-004A550DF413', '61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'Attr2', 'Value5', '2024-01-01', '2025-01-01')");

		var sql = "update RefCusCodeListAttribute set ZZE_ZZD_CodeList = '61DEB0BB-09FE-4618-BC29-BCA41CF06C93' where ZZE_PK = 'A2142364-8116-4ECA-A66A-004A550DF413'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql);
	}

	const string PrepareDataScript = @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'DE', 'Germany')
,(NEWID(), 'AU', 'Australia')

INSERT RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CodeT', 'Test1', 1, 1, 'DE')
,(NEWID(), 'CodeT', 'Test2', 1, 1, 'AU')

INSERT RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping)
VALUES ('61DEB0BB-09FE-4618-BC29-BCA41CF06C92', 'CodeT', 'Code1', 'Descr', 'DE')
,('61DEB0BB-09FE-4618-BC29-BCA41CF06C93', 'CodeT', 'Code2', 'Descr 2', 'DE')
,('61DEB0BB-09FE-4618-BC29-BCA41CF06C94', 'CodeT', 'Code2', 'Descr 2', 'AU')

INSERT RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsMandatory, ZXE_AllowDuplicates, ZXE_IsValueMandatory, ZXE_ZZK_NKCodeTypeForValueList, ZXE_ValueDataType, ZXE_MinLengthOrValue, ZXE_MaxLengthOrValue, ZXE_DecimalPlaces, ZXE_ColumnCaption, ZXE_IsDateRangeUsed)
VALUES
(NEWID(), 'Attr1', 'Attr1 Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption1', 0)
,(NEWID(), 'Attr2', 'Attr2 Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption2', 1)
,(NEWID(), 'Attr3', 'Attr3 Description', 'CodeT', 'AU', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption3', 1)
,(NEWID(), 'Attr4', 'Attr4 Description', 'CodeT', 'DE', 1, 1, 1, NULL, '', 1, 2, 0, 'Caption4', 1)
";
}
