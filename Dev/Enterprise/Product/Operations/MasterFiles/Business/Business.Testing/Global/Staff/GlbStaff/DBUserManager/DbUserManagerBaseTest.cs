using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.DbUserManager;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	abstract class DbUserManagerBaseTest : TestCase
	{
		protected static readonly string StaffLogin01 = "DbUserManagerTest_Staff'Login01";
		protected static readonly string StaffLogin02 = "DbUserManagerTest_Staff''Login02";
		protected static readonly string StaffLogin03 = "DbUserManagerTest_StaffLogin03";
		protected static readonly string StaffLogin04 = "DbUserManagerTest_StaffLogin04";
		protected static readonly string StaffLogin05 = "DbUserManagerTest_StaffLogin05";
		protected static readonly string StaffLogin06 = "DbUserManagerTest_StaffLogin06";
		protected static readonly string StaffLogin07 = "DbUserManagerTest'StaffLogin07";
		protected static readonly string StaffLogin08 = "DbUserManagerTest_StaffLogin08";
		protected static readonly string StaffLogin09 = "DbUserManagerTest_StaffLogin09";

		protected static readonly string BadStaffLogin01 = "DbUserManagerTest_BadStaffLogin[01]";
		protected static readonly string BadStaffLogin02 = "DbUserManagerTest_BadStaffLogin\"02\"";
		protected static readonly string BadStaffLogin03 = "DbUserManagerTest_BadStaffLogin<03>";

		protected override void SetUp()
		{
			base.SetUp();
			SetUpTestAdminConnection();
			CreateBiSnapshot();
			DropTestLogins();
		}

		protected override void TearDown()
		{
			DropTestLogins();
			RestoreBiSnapshot();
			CloseTestAdminConnection();
			IgnoreCommitTrackerAsSpBindSessionIsUsedToEnsureDbTransaction();
			base.TearDown();
		}

		void CreateBiSnapshot()
		{
			auditSnapshot = SnapshotCreator.CreateSnapshot(TestConnection, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS");
			edwSnapshot = SnapshotCreator.CreateSnapshot(TestConnection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS");
		}
		IDisposable auditSnapshot;
		IDisposable edwSnapshot;

		void RestoreBiSnapshot()
		{
			auditSnapshot?.Dispose();
			edwSnapshot?.Dispose();
		}

		void SetUpTestAdminConnection()
		{
			TestConnection = Db.NewAdminConnection();
		}

		void CloseTestAdminConnection()
		{
			if (TestConnection != null)
			{
				TestConnection.Dispose();
			}
			TestConnection = null;
		}

		/// <summary>
		/// 1/ These tests need an AdminConnection.
		/// 2/ Cannot simply override TestConnection to return an AdminConnection instance
		///    because our stone-age Registry always operates on Db.Connection (arrrrrgh!!).
		/// 3/ So we use sp_bindsession to join an AdminConnection with the TestConnection.
		/// 4/ DbCommitTracker does not consider sp_bindsession and thinks the AdminConnection is not in transaction.
		/// </summary>
		void IgnoreCommitTrackerAsSpBindSessionIsUsedToEnsureDbTransaction()
		{
			DbCommitTracker.Ignore(DbUserManager.UserRepositoryDb);
		}

		protected AdminConnection TestConnection;

		protected void DropStaffDbUserAndLoginsIfExist(IEnumerable<GlbStaff> staffs)
		{
			var dbUserManager = new DbUserManagerForTesting();

			foreach (var staff in staffs)
			{
				dbUserManager.DropDbUsers_Exposed(
					TestConnection,
					staff.GS_LoginName,
					() => staff.GetDownLevelLogonName(),
					staff.IsADLinked ? DatabaseAuthenticationMode.Windows : DatabaseAuthenticationMode.Sql);

				dbUserManager.DropDbLogin_Exposed(
					TestConnection,
					staff.GS_LoginName,
					() => staff.GetDownLevelLogonName(),
					staff.IsADLinked ? DatabaseAuthenticationMode.Windows : DatabaseAuthenticationMode.Sql);
			}
		}

		protected bool CheckStaffLoginExists(GlbStaff staff)
		{
			return DbUserManagerForTesting.CheckLoginExists(staff.GS_LoginName, TestConnection);
		}

		protected void DropTestLogins()
		{
			var dummyManagerToDropLogins = new DbUserManagerForTesting();

			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin01, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin02, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin03, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin04, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin05, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin06, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin07, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, StaffLogin08, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, BadStaffLogin01, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, BadStaffLogin02, () => "");
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, BadStaffLogin03, () => "");

			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin01, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin02, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin03, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin04, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin05, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin06, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin07, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, StaffLogin08, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, BadStaffLogin01, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, BadStaffLogin02, () => "");
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, BadStaffLogin03, () => "");
		}

		protected GlbStaff GetNewStaff(BusinessObjectFactory factory, string userLogin)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_LoginName = userLogin;
			staff.StaffPlainTextPassword = "";
			staff.GS_IsActive = true;
			staff.IsReadOnlyDBUser = false;
			staff.IsDatabaseDeveloper = false;
			staff.IsBackupOperator = false;

			return staff;
		}

		protected void InsertGlbStaffTestRowsForSyncTest()
		{
			var guid_staff_01 = System.Guid.NewGuid().ToString();
			var guid_staff_02 = System.Guid.NewGuid().ToString();
			var guid_staff_03 = System.Guid.NewGuid().ToString();
			var guid_staff_04 = System.Guid.NewGuid().ToString();
			var guid_staff_05 = System.Guid.NewGuid().ToString();
			var guid_staff_06 = System.Guid.NewGuid().ToString();
			var guid_staff_07 = System.Guid.NewGuid().ToString();
			var guid_staff_08 = System.Guid.NewGuid().ToString();
			var guid_staff_09 = System.Guid.NewGuid().ToString();
			var guid_badstaff_01 = System.Guid.NewGuid().ToString();
			var guid_badstaff_02 = System.Guid.NewGuid().ToString();
			var guid_badstaff_03 = System.Guid.NewGuid().ToString();

			var guid_group_01 = System.Guid.NewGuid().ToString();
			var guid_group_02 = System.Guid.NewGuid().ToString();

			string sqlText = $@"
				------ GlbStaff
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_01}', '!T1', 'DbUserManagerTest_StaffLogin01', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_02}', '!T2', 'DbUserManagerTest_StaffLogin02', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_03}', '!T3', 'DbUserManagerTest_StaffLogin03', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_04}', '!T4', 'DbUserManagerTest_StaffLogin04', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_05}', '!T5', 'DbUserManagerTest_StaffLogin05', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_06}', '!T6', 'DbUserManagerTest_StaffLogin06', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_07}', '!T7', 'DbUserManagerTest''StaffLogin07', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_08}', '!T8', 'DbUserManagerTest_StaffLogin08', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_staff_09}', '!T9', 'DbUserManagerTest_StaffLogin09', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_badstaff_01}', '!B1', 'DbUserManagerTest_BadStaffLogin[01]', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_badstaff_02}', '!B2', 'DbUserManagerTest_BadStaffLogin""02""', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SqlLoginPasswordHash, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{guid_badstaff_03}', '!B3', 'DbUserManagerTest_BadStaffLogin<03>', 0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				------ Fixed Database Access Groups
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_staff_03}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_staff_04}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_staff_05}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_staff_05}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_staff_06}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_staff_06}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_staff_07}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_staff_07}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbDeveloperGroupPK}', '{guid_staff_08}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_badstaff_01}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_badstaff_01}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_badstaff_02}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_badstaff_02}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.BackupOperatorGroupPK}', '{guid_badstaff_03}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbReaderGroupPK}', '{guid_badstaff_03}')

				------ Flexible Database Access Groups
				INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{guid_group_01}', 'TG_x1x', 'Test Group 1', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES ('{guid_group_02}', 'TG_x2x', 'Test Group 2', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'cwHRMStaffRole', '{guid_group_01}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.GlbGroupRole (GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser) VALUES (NEWID(), 'cwHRMStaffRole', '{guid_group_02}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group_01}', '{guid_staff_05}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group_02}', '{guid_staff_06}')
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{guid_group_01}', '{guid_staff_09}')
				";

			TestConnection.ExecuteNonQuery(sqlText); // This class access systems tables and procedures not supported by ZArchitecture
		}

		protected bool CheckLoginExists(string staffLoginName)
		{
			return DbUserManagerForTesting.CheckLoginExists(staffLoginName, TestConnection);
		}

		protected bool CheckLoginExistsWithRealLoginName(string realLoginName)
		{
			return DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(realLoginName, TestConnection);
		}

		protected bool CheckUserHasRightsOnDb(string dbName, string staffLoginName, string userRole)
		{
			return DbUserManagerForTesting.CheckUserHasRightsOnDb(dbName, staffLoginName, userRole, TestConnection);
		}

		protected void DropUserRole(string dbName, string staffLoginName, string userRole)
		{
			DbUserManagerForTesting.DropUserRole(dbName, staffLoginName, userRole, TestConnection);
		}

		protected bool CheckUserHasSchemaOnDb(string dbName, string staffLoginName)
		{
			return DbUserManagerForTesting.CheckUserHasSchemaOnDb(dbName, staffLoginName, TestConnection);
		}

		protected bool DoesDbExist(string dbName)
		{
			string sqlText = string.Format(@"
				IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
					SELECT 1;
				ELSE
					SELECT 0;",
				dbName);

			var cmd = TestConnection.Command(sqlText);
			return (Convert.ToInt32(cmd.ExecuteScalar()) == 1);
		}

		protected bool CheckUserHasGrantedPermissionOnDb(string dbName, string staffLoginName, string permissionType, string targetClass = "DATABASE")
		{
			string userLogin = DbUserManagerForTesting.GetFullUserLoginName(staffLoginName, () => "");

			string sqlText = string.Format(@"
				SELECT count(*) FROM [{0}].sys.database_permissions p
				INNER JOIN [{0}].sys.database_principals u ON u.principal_id = p.grantee_principal_id
				WHERE u.name = @userLogin AND p.[type] = @permissionType AND p.class_desc = '{1}' AND p.[state] = 'G';", dbName, targetClass);

			var cmd = TestConnection.Command(sqlText);
			cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, userLogin);
			cmd.AddParameter("@permissionType", SqlDbType.Char, 4, permissionType);
			int count = Convert.ToInt32(cmd.ExecuteScalar());

			return (count == 1);
		}

		protected bool CheckUserHasGrantedServerLevelPermission(string staffLoginName, string permissionType)
		{
			return CheckUserHasGrantedOrDeniedServerLevelPermission(staffLoginName, permissionType, deniedState: false);
		}

		protected bool CheckUserHasDeniedServerLevelPermission(string staffLoginName, string permissionType)
		{
			return CheckUserHasGrantedOrDeniedServerLevelPermission(staffLoginName, permissionType, deniedState: true);
		}

		bool CheckUserHasGrantedOrDeniedServerLevelPermission(string staffLoginName, string permissionType, bool deniedState)
		{
			string userLogin = DbUserManagerForTesting.GetFullUserLoginName(staffLoginName, () => "");

			string sqlText = @"
				SELECT count(*)
				FROM sys.server_permissions sp
				INNER JOIN sys.server_principals sl ON sl.principal_id = sp.grantee_principal_id
				WHERE sl.name = @userLogin
				AND sp.[type] = @permissionType
				AND sp.[state] = @permissionState";

			var cmd = TestConnection.Command(sqlText);
			cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, userLogin);
			cmd.AddParameter("@permissionType", SqlDbType.Char, 4, permissionType);
			cmd.AddParameter("@permissionState", SqlDbType.Char, 1, (deniedState) ? "D" : "G");
			int count = Convert.ToInt32(cmd.ExecuteScalar());

			return (count == 1);
		}

		protected void AssertDatabaseDeveloperRights(string staffLoginName, bool expectedValue)
		{
			// Server Level
			AssertEquals(staffLoginName + " has [VIEW SERVER STATE] rights on the server?", expectedValue, CheckUserHasGrantedServerLevelPermission(staffLoginName, "VWSS"));
			AssertEquals(staffLoginName + " has [VIEW ANY DEFINITION] rights on the server?", expectedValue, CheckUserHasGrantedServerLevelPermission(staffLoginName, "VWAD"));
			AssertEquals(staffLoginName + " has [ALTER TRACE] rights on the server?", expectedValue, CheckUserHasGrantedServerLevelPermission(staffLoginName, "ALTR"));
			AssertEquals(staffLoginName + " has [ALTER ANY EVENT SESSION] rights on the server?", expectedValue, CheckUserHasGrantedServerLevelPermission(staffLoginName, "AAES"));
			if (TestConnection.ServerVersionNumber.IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2022))
			{
				AssertEquals(staffLoginName + " has [VIEW ANY ERROR LOG] rights on the server?", expectedValue, CheckUserHasGrantedServerLevelPermission(staffLoginName, "VEL"));
			}
			// User Repository Database
			AssertEquals(staffLoginName + " has [datawriter] rights on user repository DB?", expectedValue, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "db_datawriter"));
			AssertEquals(staffLoginName + " has [CREATE TYPE] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRTY"));
			AssertEquals(staffLoginName + " has [CREATE SCHEMA] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRSM"));
			AssertEquals(staffLoginName + " has [CREATE TABLE] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRTB"));
			AssertEquals(staffLoginName + " has [CREATE VIEW] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRVW"));
			AssertEquals(staffLoginName + " has [CREATE FUNCTION] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRFN"));
			AssertEquals(staffLoginName + " has [CREATE PROCEDURE] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "CRPR"));
			AssertEquals(staffLoginName + " has [INSERT] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "IN"));
			AssertEquals(staffLoginName + " has [UPDATE] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "UP"));
			AssertEquals(staffLoginName + " has [DELETE] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "DL"));
			AssertEquals(staffLoginName + " has [REFERENCES] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "RF"));
			AssertEquals(staffLoginName + " has [ALTER SCHEMA] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "AL", "SCHEMA"));
		}

		protected void AssertDatabaseReaderRights(string staffLoginName, bool expectedValue)
		{
			AssertEquals(staffLoginName + " has [SELECT] rights on user repository DB?", expectedValue, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, staffLoginName, "SL"));
		}
	}
}
