using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefStlScriptUpdaterInfoFixture
	{
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlScriptUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefStlScript (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A'),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B')", trans);
					var info = new RefStlScriptUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, Deleted)
VALUES (NEWID(), 'A', 'X', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 0),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 1),
(NEWID(), 'C', 'C', 'C', 'C', 'C', 'TRN', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 1, 1, 'ALL', 'C', 'C', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'X'", trans));
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlScriptUpdaterInfo_2(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
DELETE FROM RefStlScript;
INSERT RefStlScript (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE'),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT')", trans);
					var info = new RefStlScriptUpdaterInfo_2();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, Deleted)
VALUES (NEWID(), 'A', 'X', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE', 0),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT', 1),
(NEWID(), 'C', 'C', 'C', 'C', 'C', 'TRN', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 1, 1, 'ALL', 'C', 'C', 'DTO', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'X'", trans));
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlScriptUpdaterInfo_3And4(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
DELETE FROM RefStlScript;
INSERT RefStlScript (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE'),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT')", trans);
					var info = new RefStlScriptUpdaterInfo_3();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, Deleted)
VALUES (NEWID(), 'A', 'X', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE', 0),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT', 1),
(NEWID(), 'C', 'C', 'C', 'C', 'C', 'TRN', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 1, 1, 'ALL', 'C', 'C', 'DTO', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'X'", trans));
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlScriptUpdaterInfo_5(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
DELETE FROM RefStlScript;
INSERT RefStlScript (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE'),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT')", trans);
					var info = new RefStlScriptUpdaterInfo_5();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, Deleted)
VALUES (NEWID(), 'A', 'X', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE', 0),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT', 1),
(NEWID(), 'C', 'C', 'C', 'C', 'C', 'TRN', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 1, 1, 'ALL', 'C', 'C', 'DTO', 0),
(NEWID(), 'A', 'Y', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A1', 'A1', 'DTE', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'Y'", trans));
				}
			}
		}

		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefStlScriptUpdaterInfo_6(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
DELETE FROM RefStlScript;
INSERT RefStlScript (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType)
VALUES (NEWID(), 'A', 'A', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE'),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT')", trans);
					var info = new RefStlScriptUpdaterInfo_6();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (STL_PK, STL_FeatureCode, STL_RoleName, STL_ModuleName, STL_FunctionName, STL_FeatureName, STL_DataGranularity, STL_CompanyCode, STL_BranchCode, STL_TransactionDateUtc, STL_CreatingUserCode, STL_GuidReference, STL_BillingReference1, STL_BillingReference2, STL_BillingReference3, STL_BillingReference4, STL_AdditionalRefs, STL_TransactionCount, STL_PreparationScript, STL_FromClause, STL_WhereClause, STL_WithOptionRecompile, STL_UsedInBilling, STL_ActiveOn, STL_MinCW1Version, STL_MaxCW1Version, STL_DateType, STL_CollectionStartDateUtc, Deleted)
VALUES (NEWID(), 'A', 'X', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A', 'A', 'DTE', '2023-09-06 09:37:12', 0),
(NEWID(), 'B', 'B', 'B', 'B', 'B', 'TRN', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 'B', 1, 1, 'ALL', 'B', 'B', 'SDT', '2023-09-06 09:37:22', 1),
(NEWID(), 'C', 'C', 'C', 'C', 'C', 'TRN', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 'C', 1, 1, 'ALL', 'C', 'C', 'DTO', NULL, 0),
(NEWID(), 'A', 'Y', 'A', 'A', 'A', 'TRN', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 1, 1, 'ALL', 'A1', 'A1', 'DTE', '2023-09-06 09:37:32', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'C'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'X'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_FeatureCode = 'A' AND STL_RoleName = 'Y'", trans));
					Assert.AreEqual(2, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefStlScript WHERE STL_CollectionStartDateUtc is not NULL", trans));
				}
			}
		}
	}
}
