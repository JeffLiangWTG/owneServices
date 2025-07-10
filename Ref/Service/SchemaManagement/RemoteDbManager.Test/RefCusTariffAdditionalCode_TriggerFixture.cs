using System.Transactions;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test;

[TestFixture]
[TransactionedTestCase]
class RefCusTariffAdditionalCode_TriggerFixture
{
	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Insert_ThrowException_1(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Insert_ThrowException_2(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '02', 'CAT', 1, 'KR')
";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Insert_ThrowException_3(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);

		var sql = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', NULL, '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Update_ThrowException_1(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		var sql1 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);

		var sql =
			@"UPDATE RefCusTariffAdditionalCode SET ZY2_ParentAdditionalCode = '05' WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Update_ThrowException_2(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		var sql1 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);

		var sql =
			@"UPDATE RefCusTariffAdditionalCode SET ZY2_ZY3_NKParentCategory = 'SCA' WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Update_ThrowException_3(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		var sql1 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);

		var sql =
			"UPDATE RefCusTariffAdditionalCode SET ZY2_ParentAdditionalCode = '02' WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Update_ThrowException_4(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		var sql1 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		var sql2 =
			@"UPDATE RefCusTariffAdditionalCode SET ZY2_ParentAdditionalCode = '02', ZY2_ZZ1_Tariff = 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', ZY2_ZZW_NationalCode = NULL WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		var sql3 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES ('18D35FB8-30A0-48F8-8A9F-45404BF7927B', 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '03', 'Fruit', 'CAT', '', '', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);

		var sql =
			@"UPDATE RefCusTariffAdditionalCode SET ZY2_ZZ1_Tariff = 'FDCA07B0-8E98-4A44-A609-D441FCF0E585' WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		DBHelper.AssertThrowsSqlException(conn, sql, ExceptionMessage);
	}

	[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
	public void RefCusTariffAdditionalCode_Trigger_Update_DoesNot_ThrowException(DbSchema dbSchema)
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		TestDBHelper.ExecuteNonQuery(conn, PrepareDataScript);
		var sql1 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES (NEWID(), NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '3X', N'1m 이상', 'SCA', '01', 'CAT', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql1);
		var sql2 =
			@"UPDATE RefCusTariffAdditionalCode SET ZY2_ParentAdditionalCode = '02', ZY2_ZZ1_Tariff = 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', ZY2_ZZW_NationalCode = NULL WHERE ZY2_PK = 'ADB6824D-34AE-44FD-B84F-3B03F802D09D'";
		var sql3 = @"
INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES ('18D35FB8-30A0-48F8-8A9F-45404BF7927B', 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '03', 'Fruit', 'CAT', '', '', 1, 'KR')
";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql2);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql3);

		var sql4 = "UPDATE RefCusTariffAdditionalCode SET ZY2_ZZ1_Tariff = 'FDCA07B0-8E98-4A44-A609-D441FCF0E585' WHERE ZY2_PK = '18D35FB8-30A0-48F8-8A9F-45404BF7927B'";
		var sql5 = "UPDATE RefCusTariffAdditionalCode SET ZY2_ZZ1_Tariff = 'FDCA07B0-8E98-4A44-A609-D441FCF0E585' WHERE ZY2_PK = '7558A41B-F5AE-46A0-AB6D-510117207DFF'";
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql4);
		DBHelper.AssertDoesNotThrowsSqlException(conn, sql5);
	}

	const string ExceptionMessage =
		"A RefCusTariffAdditionalCode was inserted/updated with invalid values in ZY2_ParentAdditionalCode or ZY2_ZY3_NKParentCategory which are not found in RefCusTariffAdditionalCode.ZY2_ZZ1_Tariff or RefCusTariffAdditionalCode.ZY2_ZZW_NationalCode having matched ZY2_AdditionalCode and ZY2_ZY3_NKCategory.";

	const string PrepareDataScript = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'KR', 'South Korea')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'HSN', 'Korean Harmonized Tariff Codes', 'KR')

INSERT INTO RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'CAT', 'Main Category', 'KR')

INSERT INTO RefCusTariffAdditionalCodeCategory (ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'SCA', 'Sub Category', 'KR')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique, ZZ1_StartDate, ZZ1_EndDate, ZZ1_CompositeKeyOnZZ5, ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A609-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'KR', '0101219000', 'Other', '', 1, '1900-01-01', '2079-06-06', '', '1900-01-01')

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_IAMUnique, ZZ1_StartDate, ZZ1_EndDate, ZZ1_CompositeKeyOnZZ5, ZZ1_PublishedDate)
VALUES ('FDCA07B0-8E98-4A44-A60A-D441FCF0E585', '7EABDFE4-F6F0-4F33-BC3A-D20863ED5075', 'KR', '0101211000', 'Meat', '', 1, '1900-01-01', '2079-06-06', '', '1900-01-01')

INSERT INTO RefCusTariffNationalCode (ZZW_PK, ZZW_ZZ1_Tariff, ZZW_NationalCode, ZZW_Description, ZZW_ZZF_NKTaxOrFeeCode, ZZW_StartDate, ZZW_EndDate, ZZW_ZZZ_NKDataGrouping, ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B6-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'TEST', 'Test National Code', '', '1900-01-01', '2079-06-06', 'KR', '1900-01-01')

INSERT INTO RefCusTariffNationalCode (ZZW_PK, ZZW_ZZ1_Tariff, ZZW_NationalCode, ZZW_Description, ZZW_ZZF_NKTaxOrFeeCode, ZZW_StartDate, ZZW_EndDate, ZZW_ZZZ_NKDataGrouping, ZZW_PublishedDate)
VALUES ('8F6E0739-434F-4D9A-95B7-E93EA040ABC8', 'FDCA07B0-8E98-4A44-A609-D441FCF0E585', 'TES2', 'Test National Code 2', '', '1900-01-01', '2079-06-06', 'KR', '1900-01-01')

INSERT INTO RefCusTariffAdditionalCode (ZY2_PK, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_AdditionalCode, ZY2_Description, ZY2_ZY3_NKCategory, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory, ZY2_IsMandatory, ZY2_ZZZ_NKDataGrouping)
VALUES ('7558A41B-F5AE-46A0-AB6E-510117207DFF', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '01', 'Meat', 'CAT', '', '', 1, 'KR'),
('7558A41B-F5AE-46A0-AB6F-510117207DFF', 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '02', 'Other', 'CAT', '', '', 1, 'KR'),
('ADB6824D-34AE-44FD-B84F-3B03F802D09D', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '1N', N'자연산', 'SCA', '01', 'CAT', 1, 'KR'),
('ADB6824D-34AE-44FD-B84E-3B03F802D09D', NULL, '8F6E0739-434F-4D9A-95B6-E93EA040ABC8', '1S', N'양식', 'SCA', '01', 'CAT', 1, 'KR'),
('ADB6824D-34AE-44FD-B84D-3B03F802D09D', 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '2L', N'2kg 이상', 'SCA', '02', 'CAT', 1, 'KR'),
('ADB6824D-34AE-44FD-B84C-3B03F802D09D', 'FDCA07B0-8E98-4A44-A60A-D441FCF0E585', NULL, '2M', N'1~2kg', 'SCA', '02', 'CAT', 1, 'KR')
";
}
