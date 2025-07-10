using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static System.FormattableString;
using static Enterprise.MasterFiles.Business.DbUserManager;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	class DbUserManagerTest : DbUserManagerBaseTest
	{
		public void TestCheckPolicyIsTurnedOf()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with test TestCheckPolicyIsTurnedOffUsingModernSecuritySystem inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.StaffPlainTextPassword = "strong_password";
			staff01.IsReadOnlyDBUser = true;
			AssertEquals("[PRE_CONDITION] StaffLogin01 exists?", false, CheckLoginExists(StaffLogin01));

			factory.Save();
			AssertEquals("StaffLogin01 exists?", true, CheckLoginExists(StaffLogin01));
			AssertEquals("StaffLogin01 has [cwRestrictedReaderRole] rights on DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

			string fullDbStaffLogin01 = DbUserManagerForTesting.GetFullUserLoginName(StaffLogin01, () => "");
			TestConnection.ExecuteNonQuery(string.Format("ALTER LOGIN [{0}] WITH CHECK_POLICY = ON", fullDbStaffLogin01));

			staff01.StaffPlainTextPassword = "";
			factory.Save();
			AssertEquals("StaffLogin01 exists?", true, CheckLoginExists(StaffLogin01));
		}

		public void TestEnableEmptyPasswordLogin()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests TestEnablesLogin and TestEnablesEmptyPasswordLogin inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.StaffPlainTextPassword = "";
			staff01.IsReadOnlyDBUser = true;
			AssertEquals("[PRE_CONDITION] StaffLogin01 exists?", false, CheckLoginExists(StaffLogin01));

			factory.Save();
			AssertEquals("StaffLogin01 exists?", true, CheckLoginExists(StaffLogin01));
			AssertEquals("StaffLogin01 has [cwRestrictedReaderRole] rights on DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("StaffLogin01 has [db_backupoperator] rights on DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));

			// Disable Login
			string fullDbStaffLogin01 = DbUserManagerForTesting.GetFullUserLoginName(StaffLogin01, () => "");
			TestConnection.ExecuteNonQuery(string.Format("ALTER LOGIN [{0}] DISABLE", fullDbStaffLogin01));
			AssertEquals("StaffLogin01 enabled?", false, DbUserManagerForTesting.CheckLoginIsEnabled(StaffLogin01, TestConnection));

			// Change staff record and apply to the database
			staff01.IsBackupOperator = true;
			factory.Save();
			AssertEquals("StaffLogin01 exists?", true, CheckLoginExists(StaffLogin01));
			AssertEquals("StaffLogin01 enabled?", true, DbUserManagerForTesting.CheckLoginIsEnabled(StaffLogin01, TestConnection));
			AssertEquals("StaffLogin01 has [cwRestrictedReaderRole] rights on DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("StaffLogin01 has [db_backupoperator] rights on DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
		}

		public void TestAssociatedSchemasAreAlsoRemoved()
		{
			// LLZ: It was decided not to remove any associated objects, including schemas when modern security is on, so this test is only relevant for old functionality
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.IsReadOnlyDBUser = true;

			factory.Save();
			Assert("StaffLogin01 should have been created", CheckLoginExists(StaffLogin01));
			Assert("StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

			DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin01, TestConnection);
			Assert("StaffLogin01 should have a schema on main DB", CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin01));

			staff01.Delete();

			factory.Save();
			Assert("StaffLogin01 schema should have been deleted", !CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin01));
			Assert("StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			Assert("StaffLogin01 DB login should have been DROPPED", !CheckLoginExists(StaffLogin01));
		}

		[UseSnapshotProtection]
		public void TestDropDbUser_InformativeErrorMessageIsLoggedWhenDroppingUser()
		{
			// Arrange
			const string staffLoginName = "DbUserManagerTest_Staff_SchemaOwnerLogin";
			var fullStaffLoginName = DbUserManagerForTesting.GetFullUserLoginName(staffLoginName, () => "");
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "cwRestrictedReaderRole" },
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => "",
				DbAuthenticationMode = DatabaseAuthenticationMode.Sql
			};
			var logger = new TestServiceLogger();
			var dbManager = new DbUserManager(logger);
			var dummyManagerToCreateLogins = new DbUserManagerForTesting();
			try
			{
				dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, staffLoginName, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
				dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, staffLoginName, new HashSet<string>() { "cwRestrictedReaderRole" }, () => "");
				DbUserManagerForTesting.CreateUserSchemaOnMainDb(staffLoginName, TestConnection);
				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(staffLoginName, TestConnection);

				// Act
				dbManager.DropDbUsers_Exposed(TestConnection, info);

				// Assert
				Assert(DbUserManagerForTesting.CheckUserHasSchemaOnDb(Db.DatabaseName, staffLoginName, TestConnection));
				AssertContains("Additional information is provided to user", "[Cannot drop schema 'DbUserManagerTest_Staff_SchemaOwnerLogin' because it is being referenced by object 'T1'.]. Please drop these objects manually or contact your database administrator if this cannot be done.", logger.ToString());
			}
			finally
			{
				dummyManagerToCreateLogins.DropDbLogin_Exposed(TestConnection, staffLoginName, () => "");
			}
		}

		public void TestSynchroniseAllStaffAndDbLogins_MainDatabase()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				// 1 - insert STAFF records for testing...
				//     staff with backup, read and both backup and read users
				InsertGlbStaffTestRowsForSyncTest();

				// 2 - create logins and users
				//     logins/users that match staff records
				//     logins/users that DO NOT match staff records
				//     leave some staff records without their required logins
				CreateLoginsAndUsersForSyncTest();

				// 3 - PRE-ASSERTION !!!
				Assert("(Before) StaffLogin01 should exist", CheckLoginExists(StaffLogin01));
				Assert("(Before) StaffLogin02 should NOT exist", !CheckLoginExists(StaffLogin02));
				Assert("(Before) StaffLogin03 should exist", CheckLoginExists(StaffLogin03));
				Assert("(Before) StaffLogin04 should exist", CheckLoginExists(StaffLogin04));
				Assert("(Before) StaffLogin06 should exist", CheckLoginExists(StaffLogin06));
				Assert("(Before) StaffLogin07 should NOT exist", !CheckLoginExists(StaffLogin07));
				Assert("(Before) StaffLogin08 should NOT exist", !CheckLoginExists(StaffLogin08));
				Assert("(Before) BadStaffLogin01 should NOT exist", !CheckLoginExists(BadStaffLogin01));
				Assert("(Before) BadStaffLogin02 should NOT exist", !CheckLoginExists(BadStaffLogin02));
				Assert("(Before) BadStaffLogin03 should exist", CheckLoginExists(BadStaffLogin03));
				Assert("(Before) StaffLogin01 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
				Assert("(Before) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin01 should NOT have [cwHRMStaffRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin03 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, "db_backupoperator"));
				Assert("(Before) StaffLogin03 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin03 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin04 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin04, "db_backupoperator"));
				Assert("(Before) StaffLogin04 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin04 should NOT have [cwHRMStaffRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin04, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin06 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, "db_backupoperator"));
				Assert("(Before) StaffLogin06 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin06 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin09 should exist", CheckLoginExists(StaffLogin09));
				Assert("(Before) StaffLogin09 should NOT have [cwHRMStaffRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) BadStaffLogin03 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, BadStaffLogin03, "db_backupoperator"));
				Assert("(Before) BadStaffLogin03 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, BadStaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) BadStaffLogin03 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, BadStaffLogin03, DbRoleTypes.CwHRMStaffRole));

				// 4 - CALL SYNCHRONISATION !!!
				ILogger logger = new TestServiceLogger();
				var testDbUserManager = new DbUserManager(logger);
				testDbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);

				// 5 - ASSERT !!!
				Assert("StaffLogin01 should have been dropped", !CheckLoginExists(StaffLogin01));
				Assert("StaffLogin02 should still NOT exist", !CheckLoginExists(StaffLogin02));
				Assert("StaffLogin03 should still exist", CheckLoginExists(StaffLogin03));
				Assert("StaffLogin04 should still exist", CheckLoginExists(StaffLogin04));
				Assert("StaffLogin05 should have been created", CheckLoginExists(StaffLogin05));
				Assert("StaffLogin06 should still exist", CheckLoginExists(StaffLogin06));
				Assert("StaffLogin07 should have been created", CheckLoginExists(StaffLogin07));
				Assert("StaffLogin08 should have been created", CheckLoginExists(StaffLogin08));
				Assert("StaffLogin03 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, "db_backupoperator"));
				Assert("StaffLogin03 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
				Assert("StaffLogin03 should NOT have [cwHRMStaffRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, DbRoleTypes.CwHRMStaffRole));
				Assert("StaffLogin04 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin04, "db_backupoperator"));
				Assert("StaffLogin04 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				// StaffLogin05
				AssertEquals("StaffLogin05 default language", "us_english", DbUserManagerForTesting.GetLoginDefaultLanguage(StaffLogin05, TestConnection));
				Assert("StaffLogin05 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin05, "db_backupoperator"));
				Assert("StaffLogin05 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin05, DbRoleTypes.CwRestrictedReaderRole));
				Assert("StaffLogin05 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin05, DbRoleTypes.CwHRMStaffRole));
				// StaffLogin06
				AssertEquals("StaffLogin06 default language", "us_english", DbUserManagerForTesting.GetLoginDefaultLanguage(StaffLogin06, TestConnection));
				Assert("StaffLogin06 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, "db_backupoperator"));
				Assert("StaffLogin06 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, DbRoleTypes.CwRestrictedReaderRole));
				Assert("StaffLogin06 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin06, DbRoleTypes.CwHRMStaffRole));
				// StaffLogin07
				AssertEquals("StaffLogin07 default language", "us_english", DbUserManagerForTesting.GetLoginDefaultLanguage(StaffLogin07, TestConnection));
				// StaffLogin08
				AssertEquals("StaffLogin08 default language", "us_english", DbUserManagerForTesting.GetLoginDefaultLanguage(StaffLogin08, TestConnection));
				// StaffLogin09
				Assert("StaffLogin09 should still exist", CheckLoginExists(StaffLogin09));
				Assert("StaffLogin09 should have [cwHRMStaffRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				// BadStaffLogins - should be dropped in synchronisation
				Assert("BadStaffLogin01 should still NOT exist", !CheckLoginExists(BadStaffLogin01));
				Assert("BadStaffLogin02 should still NOT exist", !CheckLoginExists(BadStaffLogin02));
				Assert("BadStaffLogin03 should still NOT exist", !CheckLoginExists(BadStaffLogin03));
			}
		}

		public void TestSynchroniseAllStaffAndDbLogins_BiDatabases()
		{
			// 1 - insert STAFF records for testing...
			//     staff with backup, read and both backup and read users
			InsertGlbStaffTestRowsForSyncTest();

			// 2 - create logins and users
			//     logins/users that match staff records
			//     logins/users that DO NOT match staff records
			//     leave some staff records without their required logins
			CreateLoginsAndUsersForSyncTest();
			DropUserRole(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole);
			DropUserRole(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole);

			// 3 - PRE-ASSERTION !!!
			CombineAssertions(() =>
			{
				Assert("(Before) StaffLogin04 should exist", CheckLoginExists(StaffLogin04));
				Assert("(Before) StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));

				Assert("(Before) StaffLogin09 should exist", CheckLoginExists(StaffLogin09));
				Assert("(Before) StaffLogin09 should NOT have [cwHRMStaffRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin09 should NOT have [cwHRMStaffRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
			});

			// 4 - CALL SYNCHRONISATION !!!
			ILogger logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);
			testDbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);

			// 5 - ASSERT !!!
			CombineAssertions(() =>
			{
				Assert("StaffLogin04 should still exist", CheckLoginExists(StaffLogin04));
				Assert("StaffLogin04 should have [cwRestrictedReaderRole] rights on audit DB", CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				Assert("StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));

				Assert("StaffLogin09 should still exist", CheckLoginExists(StaffLogin09));
				Assert("StaffLogin09 should have [cwHRMStaffRole] rights on audit DB", CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				Assert("StaffLogin09 should NOT have [cwHRMStaffRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
			});
		}

		public void TestSynchroniseAllStaffAndDbLogins_BiDatabases_HasNoAuditAccess()
		{
			// 1 - insert STAFF records for testing...
			//     staff with backup, read and both backup and read users
			InsertGlbStaffTestRowsForSyncTest();

			// 2 - create logins and users
			//     logins/users that match staff records
			//     logins/users that DO NOT match staff records
			//     leave some staff records without their required logins
			CreateLoginsAndUsersForSyncTest();
			DropUserRole(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole);
			DropUserRole(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole);

			// 3 - PRE-ASSERTION !!!
			CombineAssertions(() =>
			{
				Assert("(Before) StaffLogin04 should exist", CheckLoginExists(StaffLogin04));
				Assert("(Before) StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(Before) StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));

				Assert("(Before) StaffLogin09 should exist", CheckLoginExists(StaffLogin09));
				Assert("(Before) StaffLogin09 should NOT have [cwHRMStaffRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				Assert("(Before) StaffLogin09 should NOT have [cwHRMStaffRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
			});

			using (SystemDataRegistry.Instance.BiEnableAuditAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// 4 - CALL SYNCHRONISATION !!!
				ILogger logger = new TestServiceLogger();
				var testDbUserManager = new DbUserManager(logger);
				testDbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);
			}

			// 5 - ASSERT !!!
			CombineAssertions(() =>
			{
				Assert("StaffLogin04 should still exist", CheckLoginExists(StaffLogin04));
				Assert("StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));
				Assert("StaffLogin04 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin04, DbRoleTypes.CwRestrictedReaderRole));

				Assert("StaffLogin09 should still exist", CheckLoginExists(StaffLogin09));
				Assert("StaffLogin09 should NOT have [cwHRMStaffRole] rights on audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
				Assert("StaffLogin09 should NOT have [cwHRMStaffRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin09, DbRoleTypes.CwHRMStaffRole));
			});
		}

		public void TestSynchroniseAllStaffAndDbLoginsLogsWarningForUnableToDropUserOwnedSchema_MainDatabase()
		{
			try
			{
				using (BiServers.TemporarilySetAuditServerToNull())
				using (BiServers.TemporarilySetDataWarehouseServerToNull())
				{
					// Arrange
					InsertGlbStaffTestRowsForSyncTest();
					CreateLoginsAndUsersForSyncTest();

					DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin01, TestConnection);
					DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);

					var logger = new Mock<ILogger>();
					var testDbUserManager = new DbUserManager(logger.Object);

					// Act
					AssertNoExceptionThrown(() => testDbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection));

					// Assert
					logger.Verify(
						x => x.Log(
							LogType.Warning,
							$"Failed to drop database user \"{DbUserManagerForTesting.GetFullUserLoginName(StaffLogin01, () => "")}\". Server: [{TestConnection.ServerName}], Database: [{TestConnection.CurrentDatabase}], Sql Error: [Cannot drop schema 'DbUserManagerTest_Staff'Login01' because it is being referenced by object 'T1'.]. Please drop these objects manually or contact your database administrator if this cannot be done."),
						Times.Once);
				}
			}
			finally
			{
				DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);
				DbUserManagerForTesting.DropUserSchemaOnMainDb(StaffLogin01, TestConnection);
			}
		}

		public void TestSynchroniseAllStaffAndDbLoginsLogsWarningForUnableToDropUserOwnedSchema_BiDatabases()
		{
			try
			{
				// Arrange
				InsertGlbStaffTestRowsForSyncTest();
				CreateLoginsAndUsersForSyncTest();

				DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin01, TestConnection);
				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);

				var logger = new Mock<ILogger>();
				var testDbUserManager = new DbUserManager(logger.Object);

				// Act
				AssertNoExceptionThrown(() => testDbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection));

				// Assert
				logger.Verify(
					x => x.Log(
						LogType.Warning,
						$"Failed to drop database user \"{DbUserManagerForTesting.GetFullUserLoginName(StaffLogin01, () => "")}\". Server: [{TestConnection.ServerName}], Database: [{TestConnection.CurrentDatabase}], Sql Error: [Cannot drop schema 'DbUserManagerTest_Staff'Login01' because it is being referenced by object 'T1'.]. Please drop these objects manually or contact your database administrator if this cannot be done."),
					Times.Once);
			}
			finally
			{
				DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);
				DbUserManagerForTesting.DropUserSchemaOnMainDb(StaffLogin01, TestConnection);
			}
		}

		public void TestSynchroniseAllStaffAndDbLogins_WhenEmailSentFailed()
		{
			InsertGlbStaffTestRowsForSyncTest();
			CreateLoginsAndUsersForSyncTest();

			var logger = new TestServiceLogger();
			var testDbUserManager = new Mock<DbUserManager>(logger);

			AssertEquals("Log buffer should be empty", 0, logger.Count);

			testDbUserManager.Object.SynchroniseAllStaffAndDbLogins(TestConnection);

			Assert(!LogContains(logger, "Error|Couldn't send email: The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));

			logger.ClearLog();
			testDbUserManager = new Mock<DbUserManager>(logger);
			testDbUserManager.Protected()
				.Setup("PropagateLogin", ItExpr.IsAny<List<StaffLoginInfo>>(), ItExpr.IsAny<Action<AdminConnection, StaffLoginInfo>>())
				.Throws(new EmailHasNoFromAddressException("The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));

			AssertEquals("Log buffer should be empty", 0, logger.Count);

			testDbUserManager.Object.SynchroniseAllStaffAndDbLogins(TestConnection);

			Assert(LogContains(logger, "Error|Couldn't send email: The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));
		}

		public void TestSynchroniseAllStaffAndDbLoginsUsesPasswordHashStoredInStaffRecordToCreateSqlLogin()
		{
			// Arrange
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			var dbUserManager = new DbUserManager();
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();
			}

			var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

			AssertNotEquals("Precondition: sql password hash should be saved on staff record.",
				DBNull.Value,
				currentPasswordHash);

			TestConnection.ExecuteNonQuery($@"
IF EXISTS(SELECT null FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}')
	DROP LOGIN {staffSqlLoginName.QuoteName()}
");

			Assert(
				"Precondition: Sql login should not exist.",
				!TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));

			// Act
			dbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);

			// Assert
			Assert("Sql login should have been created.", TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));

			AssertEquals(
				"Sql password hash saved in the staff record should not have changed.",
				currentPasswordHash,
				TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));

			Assert(
				"Created Sql login password should have the same password hash as saved on the staff record.",
				TestConnection.ExecuteScalar<bool>(
					@"
SELECT CAST(IIF(staff.GS_SqlLoginPasswordHash = login.password_hash, 1, 0) AS bit)
FROM dbo.GlbStaff AS staff
	JOIN sys.sql_logins AS login ON login.name = @sqlLoginName
WHERE staff.GS_LoginName = @loginName
",
					cmd =>
					{
						cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName);
						cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, staffSqlLoginName);
					}));
		}

		public void TestSynchroniseAllStaffAndDbLoginsUsesPasswordHashStoredInStaffRecordToAlterSqlLogin()
		{
			// Arrange
			const string staffCode = "TS~";
			const string staffSqlLoginPassword = "pA$$w0rD!";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			var dbUserManager = new DbUserManager();
			dbUserManager.SetPasswordForStaff(staff, staffSqlLoginPassword);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();
			}

			var currentPasswordHash = TestConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

			TestConnection.ExecuteNonQuery($@"
IF NOT EXISTS(SELECT null FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}')
	CREATE LOGIN {staffSqlLoginName.QuoteName()} WITH PASSWORD = N'DIFFERENT PASSWORD123[]'
ELSE
	ALTER LOGIN {staffSqlLoginName.QuoteName()} WITH PASSWORD = N'DIFFERENT PASSWORD123[]'
");

			Assert(
				"Precondition: Sql login should be present.",
				TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));

			AssertEquals(
				"Precondition: Sql login password hash should be different from the one saved in the staff record.",
				currentPasswordHash,
				TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));

			// Act
			dbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);

			// Assert
			Assert(
				"Sql login should still be present.",
				TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));

			AssertEquals(
				"Sql password hash saved in the staff record should not have changed.",
				currentPasswordHash,
				TestConnection.ExecuteScalar<byte[]>(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));
			Assert(
				"Sql login should have been altered so its password hash matches the one saved on the staff record.",
				TestConnection.ExecuteScalar<bool>(
					@"
SELECT CAST(IIF(staff.GS_SqlLoginPasswordHash = login.password_hash, 1, 0) AS bit)
FROM dbo.GlbStaff AS staff
	JOIN sys.sql_logins AS login ON login.name = @sqlLoginName
WHERE staff.GS_LoginName = @loginName
",
					cmd =>
					{
						cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName);
						cmd.AddParameter("@sqlLoginName", SqlDbType.NVarChar, 128, staffSqlLoginName);
					}));
		}

		public void TestSynchroniseAllStaffAndDbLoginsLogsAWarningAndDropsALoginForStaffRecordsThatShouldHaveSqlLoginButAreMissingSqlPasswordHashInTheRecord()
		{
			// Arrange
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			const string staffCode = "TS~";
			var staffLoginName = $"Tester_{Guid.NewGuid():N}";
			var staffSqlLoginName = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{staffLoginName}";

			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = $"Tester_{Guid.NewGuid():N}";
			staff.GS_LoginName = staffLoginName;
			staff.GS_Code = staffCode;
			staff.IsReadOnlyDBUser = true; // db access role

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();
			}

			TestConnection.ExecuteNonQuery(
				"UPDATE dbo.GlbStaff SET GS_SqlLoginPasswordHash = NULL, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName));

			AssertEquals("Precondition: sql password hash should be NULL on staff record.",
				System.DBNull.Value,
				TestConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLoginName, GlbStaffSchema.GS_LoginName)));

			TestConnection.ExecuteNonQuery($@"
IF NOT EXISTS(SELECT null FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}')
	CREATE LOGIN {staffSqlLoginName.QuoteName()} WITH PASSWORD = N'some PASSWORD123[]'
");

			Assert(
				"Precondition: Sql login should exist.",
				TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));

			var loggerMock = new Mock<ILogger>();

			var dbUserManager = new DbUserManager(loggerMock.Object);

			// Act
			dbUserManager.SynchroniseAllStaffAndDbLoginsForAllDatabases(TestConnection);

			// Assert
			loggerMock.Verify(
				logger => logger.Log(
				LogType.Warning,
				It.Is<string>(message => message == $"Skipped synchronising SQL login for staff record {staff.GS_LoginName}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password")));
			Assert(
				"Sql login should have been dropped.",
				!TestConnection.Exists($"FROM sys.sql_logins WHERE name = N'{staffSqlLoginName}'"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Baseline")]
		public void TestGetUsersThatShouldHaveDbLogins_MissingADUser()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var factory = new BusinessObjectFactory(TestConnection);

			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "missing.ad";
			staff.IsBackupOperator = true;
			staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();
			}

			EnvProxy.SetHostedLocationForTest("SYD");
			var logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);

			var peopleWithResults = testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection);

			Console.WriteLine(logger);
			AssertEquals("Missing AD user should have been skipped", 0, peopleWithResults.Count);
			AssertContains("Logged error message - skip missing AD user", $@"Skipped synchronising staff and db logins for missing AD entity {staff.GS_LoginName}", logger.ToString());
		}

		public void TestSynchroniseAllStaffAndDbLogins_MissingADUser()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var factory = new BusinessObjectFactory(TestConnection);

			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "missing.ad";
			staff.IsBackupOperator = true;
			staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				factory.Save();
			}

			EnvProxy.SetHostedLocationForTest("SYD");
			var logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);

			testDbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);

			AssertEquals("No flow on error messages", 1, logger.Count);
			AssertContains("Logged error message - skip missing AD user", $@"Skipped synchronising staff and db logins for missing AD entity {staff.GS_LoginName}", logger.ToString());
		}

		public void TestSynchroniseAllStaffAndDbLogins_ADWithException()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);

			// AD not throwing exception
			AssertEquals("Log before run", 0, logger.Count);
			testDbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);
			AssertEquals("Log after run - expect no log", 0, logger.Count);

			// AD's GetDirectorySearcher() throws exception
			logger.ClearLog();
			var searcherProvider = new Mock<IDirectorySearcherProvider>();
			searcherProvider.Setup(s => s.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Throws(new COMException("You dont know this"));
			ObjectFactory.Substitute(searcherProvider.Object);

			AssertEquals("Log before run", 0, logger.Count);
			testDbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);
			Assert("Log after run - AD exception should log message", LogContains(logger, "User Login Synchronization cannot be run at this time as it failed to connect to the Domain or Active Directory server. This can be ignored if the system is currently performing an upgrade, the AlwaysOn Management Service will correct it. Alternatively, you can run 'Synchronize Staff Database Logins' manually after the upgrade."));
		}

		public void TestCreateStaffLogin_WhenEmailSentFailed()
		{
			var logger = new TestServiceLogger();
			var testDbUserManager = new Mock<DbUserManager>(logger);
			testDbUserManager.Protected()
				.Setup("PropagateLogin", ItExpr.IsAny<List<StaffLoginInfo>>(), ItExpr.IsAny<Action<AdminConnection, StaffLoginInfo>>())
				.Throws(new EmailHasNoFromAddressException("The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));

			AssertEquals("Log buffer should be empty", 0, logger.Count);

			var info = new StaffLoginInfo()
			{
				LoginName = StaffLogin01,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>(),
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => ""
			};

			testDbUserManager.Object.CreateStaffLogin_Exposed(info);

			Assert(LogContains(logger, "Error|Couldn't send email: The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));
		}

		public void TestAlterStaffLogin_WhenEmailSentFailed()
		{
			var logger = new TestServiceLogger();
			var testDbUserManager = new Mock<DbUserManager>(logger);
			testDbUserManager.Protected()
				.Setup("PropagateLogin", ItExpr.IsAny<List<StaffLoginInfo>>(), ItExpr.IsAny<Action<AdminConnection, StaffLoginInfo>>())
				.Throws(new EmailHasNoFromAddressException("The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));

			AssertEquals("Log buffer should be empty", 0, logger.Count);

			var info = new StaffLoginInfo()
			{
				LoginName = StaffLogin01,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>(),
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => ""
			};

			testDbUserManager.Object.AlterStaffLogin_Exposed(info);

			Assert(LogContains(logger, "Error|Couldn't send email: The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));
		}

		public void TestDropStaffLogin_WhenEmailSentFailed()
		{
			var logger = new TestServiceLogger();
			var testDbUserManager = new Mock<DbUserManager>(logger);
			testDbUserManager.Protected()
				.Setup("PropagateLogin", ItExpr.IsAny<List<StaffLoginInfo>>(), ItExpr.IsAny<Action<AdminConnection, StaffLoginInfo>>())
				.Throws(new EmailHasNoFromAddressException("The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));

			AssertEquals("Log buffer should be empty", 0, logger.Count);

			var info = new StaffLoginInfo()
			{
				LoginName = StaffLogin01,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>(),
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => ""
			};

			testDbUserManager.Object.DropStaffLogin_Exposed(info);

			Assert(LogContains(logger, "Error|Couldn't send email: The registry item Physical Server > Mail > Incoming > Mailbox Email Address must be set up in order to send emails from CargoWise One."));
		}

		public void TestStaffLogin_GetApplicableRoles()
		{
			var info = new StaffLoginInfo()
			{
				LoginName = StaffLogin01,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator" },
				DbPermissionChanged = true,
			};

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new string[] { "cwRestrictedReaderRole", "db_backupoperator" }, info.GetApplicableRoles(DbUserManager.DbType.eDocs));
				AssertContainsExactElementsInAnyOrder(new string[] { "cwRestrictedReaderRole", "db_backupoperator" }, info.GetApplicableRoles(DbUserManager.DbType.ReferenceDatabase));
				AssertContainsExactElementsInAnyOrder(new string[] { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator" }, info.GetApplicableRoles(DbUserManager.DbType.UserRepository));
				AssertContainsExactElementsInAnyOrder(new string[] { "cwRestrictedReaderRole", "db_backupoperator" }, info.GetApplicableRoles(DbUserManager.DbType.Other));
			});
		}

		public void TestSynchroniseDoesntOccurForInactiveRecords()
		{
			InsertGlbStaffTestRowsForSyncTest();
			CreateLoginsAndUsersForSyncTest();
			TestConnection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_IsActive = 0, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E';");
			var testDbUserManager = new DbUserManagerForTesting();
			testDbUserManager.SynchroniseAllStaffAndDbLogins(TestConnection);

			Assert("StaffLogin01 should not exist.", !CheckLoginExists(StaffLogin01));
			Assert("StaffLogin02 should not exist.", !CheckLoginExists(StaffLogin02));
			Assert("StaffLogin03 should not exist.", !CheckLoginExists(StaffLogin03));
			Assert("StaffLogin04 should not exist.", !CheckLoginExists(StaffLogin04));
			Assert("StaffLogin05 should not exist.", !CheckLoginExists(StaffLogin05));
			Assert("StaffLogin06 should not exist.", !CheckLoginExists(StaffLogin06));
			Assert("StaffLogin07 should not exist.", !CheckLoginExists(StaffLogin07));
		}

		public void TestApplyChangesDoesntOccurForInactiveRecords()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests TestDatabaseAccessNotAddedForInactiveRecords and TestDatabaseAccessRemovedForInactiveRecords inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);

			var staff1 = GetNewStaff(factory, StaffLogin01);
			staff1.GS_IsActive = true;
			staff1.IsReadOnlyDBUser = true;

			var staff2 = GetNewStaff(factory, StaffLogin02);
			staff2.GS_IsActive = false;
			staff2.IsReadOnlyDBUser = true;

			factory.Save();

			Assert("StaffLogin01 DB login should have been created", CheckLoginExists(StaffLogin01));
			Assert("StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			Assert("StaffLogin02 DB login should not have been created", !CheckLoginExists(StaffLogin02));
		}

		public void TestChangingLoginNameCreatesNewLoginAndDeletesOld()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests TestChangingLoginNameCreatesNewLoginAndDeletesOld inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "beaver";
			staff.IsBackupOperator = true;
			factory.Save();
			Assert("beaver login should have been created", CheckLoginExists("beaver"));

			staff.GS_LoginName = "evilBeaver";
			factory.Save();
			Assert("beaver login should have been deleted", !CheckLoginExists("beaver"));
			Assert("evilBeaver login should have been created", CheckLoginExists("evilBeaver"));

			staff.IsBackupOperator = false;
			factory.Save();
			Assert("evilBeaver login should have been deleted", !CheckLoginExists("evilBeaver"));
		}

		public void TestShouldUseWindowsOrSqlLogin()
		{
			var dbMan = new DbUserManagerForTesting();
			EnvProxy.SetHostedLocationForTest("NCW");
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("Not-hosted, AD Integrated: False, DomainName: empty.     Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => ""));
			AssertEquals("Not-hosted, AD Integrated: False, DomainName: non-empty. Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => "principalName"));
			AssertEquals("Not-hosted, AD Integrated: False, Should create SQL Logins?", true, dbMan.DbSupportsSqlAuthentication_Exposed());

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertEquals("Not-hosted, AD Integrated: True, DomainName: empty.     Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => ""));
			AssertEquals("Not-hosted, AD Integrated: True, DomainName: non-empty. Should create Windows Logins?", true, dbMan.DbSupportsWindowsAuthentication_Exposed(() => "principalName"));
			AssertEquals("Not-hosted, AD Integrated: False, Should create SQL Logins?", false, dbMan.DbSupportsSqlAuthentication_Exposed());

			EnvProxy.SetHostedLocationForTest("LON");
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("Hosted, AD Integrated: False, DomainName: empty.     Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => ""));
			AssertEquals("Hosted, AD Integrated: False, DomainName: non-empty. Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => "principalName"));
			AssertEquals("Hosted, AD Integrated: False, Should create SQL Logins?", true, dbMan.DbSupportsSqlAuthentication_Exposed());

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertEquals("Hosted, AD Integrated: True, DomainName: empty.     Should create Windows Logins?", false, dbMan.DbSupportsWindowsAuthentication_Exposed(() => ""));
			AssertEquals("Hosted, AD Integrated: True, DomainName: non-empty. Should create Windows Logins?", true, dbMan.DbSupportsWindowsAuthentication_Exposed(() => "principalName"));
			AssertEquals("Hosted, AD Integrated: True, Should create SQL Logins?", true, dbMan.DbSupportsSqlAuthentication_Exposed());
		}

		public void TestADIntegratedHostedSystemsCreateBothWindowsAndSqlLogins()
		{
			var currentUserName = TestConstants.ADTestAdminAccount.Name;
			var domainName = TestConstants.DomainPreWin2000;

			var sqlUsername = $"{DbUserRepository.StaffDbLoginPrefix}_{Db.DatabaseName}_{currentUserName}";
			var windowsUsername = TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000;

			try
			{
				// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests
				// TestADIntegratedHostedSystemsCreateBothWindowsAndSqlLogins
				// TestADIntegratedSelfHostedSystemsCreateOnlyWindowsLogins
				// inside DbSecurityAdminTaskSecurityTest
				// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
				EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				var factory = new BusinessObjectFactory(TestConnection);
				factory.Save();

				SetDomainCredentialCollection();

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = currentUserName;
				staff.DomainName = domainName;
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

				EnvProxy.SetHostedLocationForTest("NCW");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				factory.Save();
				ILogger logger = new TestServiceLogger();
				var testDbUserManager = new DbUserManager(logger);
				var logins = testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection);
				AssertEquals("Number of logins:", 1, logins.Count);
				AssertEquals("First login:", sqlUsername, logins[0].LoginName);
				AssertEquals($"Self-hosted, AD Integration disabled. SQL login [{sqlUsername}] exists?", true, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(sqlUsername, TestConnection));
				AssertEquals($"Self-hosted, AD Integration disabled. Windows login [{windowsUsername}] exists?", false, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(windowsUsername, TestConnection));
				DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				logins = testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection);
				AssertEquals("Number of logins:", 1, logins.Count);
				AssertEquals("First login:", true, logins[0].LoginName.Equals(windowsUsername, StringComparison.OrdinalIgnoreCase));
				AssertEquals($"Self-hosted, AD Integration enabled. SQL login [{sqlUsername}] exists?", false, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(sqlUsername, TestConnection));
				AssertEquals($"Self-hosted, AD Integration enabled. Windows login [{windowsUsername}] exists?", true, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(windowsUsername, TestConnection));
				DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				EnvProxy.SetHostedLocationForTest("SYD");
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				logins = testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection);
				AssertEquals("Number of logins:", 1, logins.Count);
				AssertEquals("First login:", sqlUsername, logins[0].LoginName);
				AssertEquals($"Hosted, AD Integration disabled. SQL login [{sqlUsername}] exists?", true, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(sqlUsername, TestConnection));
				AssertEquals($"Hosted, AD Integration disabled. Windows login [{windowsUsername}] exists?", false, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(windowsUsername, TestConnection));
				DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);

				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				staff.IsDatabaseDeveloper = !staff.IsDatabaseDeveloper;
				factory.Save();
				logins = testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection);
				AssertEquals("Number of logins:", 2, logins.Count);
				AssertEquals($"Login [{sqlUsername}] is in the list", true, logins.Any(x => x.LoginName.Equals(sqlUsername, StringComparison.OrdinalIgnoreCase)));
				AssertEquals($"Login [{windowsUsername}] is in the list", true, logins.Any(x => x.LoginName.Equals(windowsUsername, StringComparison.OrdinalIgnoreCase)));
				AssertEquals($"Hosted, AD Integration enabled. SQL login [{sqlUsername}] exists?", true, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(sqlUsername, TestConnection));
				AssertEquals($"Hosted, AD Integration enabled. Windows login [{windowsUsername}] exists?", true, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(windowsUsername, TestConnection));

				staff.Delete();
				factory.Save();
				AssertEquals($"Hosted, AD Integration enabled. SQL login [{sqlUsername}] exists after Staff record is deleted?", false, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(sqlUsername, TestConnection));
				AssertEquals($"Hosted, AD Integration enabled. Windows login [{windowsUsername}] exists after Staff record is deleted?", false, DbUserManagerForTesting.CheckLoginExistsWithActualDatabaseLoginName(windowsUsername, TestConnection));
			}
			finally
			{
				DropDatabasePrincipalAndServerPrincipal(currentUserName, windowsUsername);
			}
		}

		internal static void SetDomainCredentialCollection()
		{
			var domainCredentials = ObjectFactory.New<IDomainCredentials>();
			domainCredentials.DomainName = TestConstants.Domain;
			domainCredentials.DomainUserName = TestConstants.ADTestUserAccount.Name;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccount.Password;
			domainCredentials.IsDefaultDomain = true;
			domainCredentials.UserOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.GroupOrganisationalUnit = TestConstants.ValidOU;
			domainCredentials.DefaultPassword = "Changeme12345";

			ObjectFactory.Get<IADRegistry>().DefaultDomainCredentials = domainCredentials;
		}

		internal static void DropDatabasePrincipalAndServerPrincipal(string username, string downLevelLogonName)
		{
			var info = new StaffLoginInfo()
			{
				LoginName = username,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "cwRestrictedReaderRole" },
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => downLevelLogonName
			};

			var logger = new TestServiceLogger();
			var testDbUserManager = new Mock<DbUserManager>(logger);
			testDbUserManager.Object.DropStaffLogin_Exposed(info);
		}

		void DropADTestUserAccountInDB()
		{
			var dummyManagerToDropLogins = new DbUserManagerForTesting();
			dummyManagerToDropLogins.DropDbUsers_Exposed(TestConnection, TestConstants.ADTestUserAccount.Name, () => TestConstants.DomainPreWin2000);
			dummyManagerToDropLogins.DropDbLogin_Exposed(TestConnection, TestConstants.ADTestUserAccount.Name, () => TestConstants.DomainPreWin2000);
		}

		public void TestCreateLoginWithADEnabled_ShouldCreateWindowsAccountWithRightDomainName()
		{
			// LLZ: This is already being tested in test TestADIntegratedHostedSystemsCreateBothWindowsAndSqlLogins
			// When UseModernSqlSecuritySystem is true, the later is replaced with tests
			// TestADIntegratedHostedSystemsCreateBothWindowsAndSqlLogins
			// TestADIntegratedSelfHostedSystemsCreateOnlyWindowsLogins
			// inside DbSecurityAdminTaskSecurityTest
			// Hence, I do not see the need to replaced with test when UseModernSecuritySystem is true
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			DropADTestUserAccountInDB();
			try
			{
				SetDomainCredentialCollection();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

				var factory = new BusinessObjectFactory(TestConnection);

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = TestConstants.ADTestUserAccount.Name;
				staff.GS_ActiveDirectoryObjectGuid = new ZGuid(TestConstants.ADTestUserAccount.Guid);
				staff.IsBackupOperator = true;

				factory.Save();

				var message = string.Format(@"Login should have been created for user [{0}]", TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
				Assert(message, CheckLoginExistsWithRealLoginName(TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000));
			}
			finally
			{
				DropADTestUserAccountInDB();
			}
		}

		public void TestCreateLoginWithADEnabled_NonExistentUser_ShouldLogCorrectMessage()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var factory = new BusinessObjectFactory(TestConnection);

			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "evil.beaver";
			staff.IsBackupOperator = true;
			staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				AssertNoExceptionThrown(() => factory.Save());
			}

			var qualifiedUserName = $"{System.Environment.UserDomainName}\\{staff.GS_LoginName}";
			var message = $"Login should NOT have been created for user [{qualifiedUserName}] because a matching user account doesn't exist";
			Assert(message, !CheckLoginExistsWithRealLoginName(qualifiedUserName));

			ILogger logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);

			var glbGroupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
			glbGroupLinkSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, SQLComparisonOperator.Equal, new Guid[] { GlbGroup.DbDeveloperGroupPK, GlbGroup.DbReaderGroupPK, GlbGroup.BackupOperatorGroupPK });

			var query = new ZDBOnlyQuery(typeof(GlbStaff)) { ReLoadExistingRows = true };
			query.AddToFilter(GlbStaffSchema.GS_IsActive, ZBool.True);
			query.AddSubQuery(GlbStaffSchema.PK, GlbGroupLinkSchema.GK_GS, glbGroupLinkSubQuery, JoinCondition.And);

			var staffCollection = new GlbStaffCollection(factory, query);
			AssertEquals("Expected number of records", 1, staffCollection.Count);
			AssertNoExceptionThrown(() => testDbUserManager.GetFullUserLoginName_Exposed(staff.GS_LoginName, staff.GetDownLevelLogonName, DatabaseAuthenticationMode.Windows));
			AssertNoExceptionThrown(() => testDbUserManager.GetUsersThatShouldHaveDbLogins(TestConnection));

			AssertEquals("Number of logs:", 2, ((TestServiceLogger)logger).Count);
			Assert("First log:", ((TestServiceLogger)logger)[0].StartsWith("Error|Failed to get Full User Login Name.|CargoWise.ActiveDirectory.DirectoryServicesException: Could not locate or write to directory entry for entity evil.beaver", StringComparison.OrdinalIgnoreCase));
			Assert("Second log:", ((TestServiceLogger)logger)[1].StartsWith("Error|Skipped synchronising staff and db logins for missing AD entity evil.beaver", StringComparison.OrdinalIgnoreCase));
		}

		public void TestCreateLoginWithADEnabled_NonExistentUser_ShouldThrowWithRightDomainName()
		{
			// LLZ: for this test to work when UseModernSqlSecuritySystem is on, validation needs to be moved to OnFactorySaving
			// but there is already a validation on the form to make sure correct domain is specified
			// if AD synchronisation did not work prorly and there is no appropriate AD login in the system then
			// synchronisation will fail for the login anyway but usynchronously
			// so, it seems not necessary to keep this functionality as it is more relevant to synchronisation rather than saving staff members
			// Once UseModernSqlSecuritySystem is removed,  this test can be dropped.
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var currentUserName = "Maggot";
			var preWin2000Username = "MaggotsPreWinUsername";
			var userDomain = "JUNK";
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			var adUser = new Mock<IADUser>();
			adUser.Setup(m => m.DomainNetBiosName).Returns(userDomain);
			adUser.Setup(m => m.SAMAccountName).Returns(preWin2000Username);

			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(m => m.GetADUser(It.IsAny<Integration.IGlbStaff>())).Returns(adUser.Object);

			using (ObjectFactory.Substitute(adEntityProvider.Object))
			{
				var factory = new BusinessObjectFactory(TestConnection);

				var staff = factory.New<GlbStaff>();
				staff.GS_LoginName = currentUserName;
				staff.IsBackupOperator = true;
				staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();

#if NETFRAMEWORK
				var expectedExceptionMessage = $@"Failed to synchronize logins, please contact your System Administrator and report the following error:

System.Data.SqlClient.SqlException (0x80131904): Windows NT user or group '{userDomain}\{preWin2000Username}' not found. Check the name again.";
#else
				var expectedExceptionMessage = $@"Failed to synchronize logins, please contact your System Administrator and report the following error:

Microsoft.Data.SqlClient.SqlException (0x80131904): Windows NT user or group '{userDomain}\{preWin2000Username}' not found. Check the name again.";
#endif
				AssertExceptionThrown(typeof(ZCannotSaveException), expectedExceptionMessage, factory.Save, true);
			}

			adUser.VerifyAll();
		}

		public void TestSQL_EnableLoginHandlesExceptions()
		{
			ILogger logger = new TestServiceLogger();
			var testDbUserManager = new DbUserManager(logger);
			var info = new StaffLoginInfo()
			{
				LoginName = "staffLoginName",
			};
			var sqlEnableLoginCommand = testDbUserManager.SQL_EnableLogin_Exposed(info);

			AssertEquals(null, sqlEnableLoginCommand);
			AssertEquals("Number of logs:", 1, ((TestServiceLogger)logger).Count);
			Assert("Log message:", ((TestServiceLogger)logger)[0].StartsWith("Error|Failed to enable login.|System.NullReferenceException: Object reference not set to an instance of an object.", StringComparison.OrdinalIgnoreCase));
		}

		public void TestInvalidLoginNames()
		{
			// LLZ: for this test to work when UseModernSqlSecuritySystem is on, validation needs to be moved to OnFactorySaving
			// but there is already a validation on the form
			// there is also another WI to make sure SqlSecurityManager does not allow invalid login names
			// so, it seems not necessary to keep this functionality as it is more relevant to synchronisation rather than saving staff members
			// Once UseModernSqlSecuritySystem is removed,  this test can be dropped.
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			TryCreateLoginForInvalidLoginName(BadStaffLogin01);
			TryCreateLoginForInvalidLoginName(BadStaffLogin02);
			TryCreateLoginForInvalidLoginName(BadStaffLogin03);
		}

		void TryCreateLoginForInvalidLoginName(string loginName)
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, loginName);
			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;

			AssertExceptionThrown("User should not be added", typeof(InvalidOperationException)
				, string.Format("Could not add login to database. Login name: \"{0}\" contains invalid characters", loginName)
				, () => factory.Save()
				);
		}

		public void TestLoginRightsDependOnHostedLocation()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with tests
			// TestLoginRightsDependOnHostedLocation inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;
			staff01.IsBackupOperator = false;

			try
			{
				// Self-hosted client
				EnvProxy.SetHostedLocationForTest(Core.Constants.LicenceConstants.NotHostedWithCargoWise);
				factory.Save();
				AssertHostedLocationRelatedServerRights("Self-Hosted/IsDeveloper", StaffLogin01, true, false, true, true);

				// Hosted at WiseGrid
				EnvProxy.SetHostedLocationForTest("SYD");
				staff01.IsBackupOperator = true;
				factory.Save();
				AssertHostedLocationRelatedServerRights("Hosted/IsDeveloper", StaffLogin01, false, true, false, false);

				staff01.IsDatabaseDeveloper = false;

				// Self-hosted client
				EnvProxy.SetHostedLocationForTest(Core.Constants.LicenceConstants.NotHostedWithCargoWise);
				factory.Save();
				AssertHostedLocationRelatedServerRights("Self-Hosted/Non Developer", StaffLogin01, false, false, false, false);

				// Hosted at WiseGrid
				EnvProxy.SetHostedLocationForTest("SYD");
				staff01.IsBackupOperator = false;
				factory.Save();
				AssertHostedLocationRelatedServerRights("Hosted/Non Developer", StaffLogin01, false, true, false, false);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(null);
			}
		}

		public void TestHandlesRoleDoesNotExistError()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, there are a number of tests that replace this test
			// inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.IsReadOnlyDBUser = true;

			factory.Save();

			Assert("(1) StaffLogin01 DB login should have been created", CheckLoginExists(StaffLogin01));
			Assert("(1) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

			//Force drop CwRestrictedReaderRole to ensure the application recreates it if necessary
			DbSecurityTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedReaderRole);

			staff01.IsBackupOperator = true;
			factory.Save();

			Assert("(2) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			Assert("(2) StaffLogin01 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
		}

		[ExpectNoExceptions]
		public void TestCreateDbUserWithPermissions()
		{
			var testLoginName = StaffLogin01;
			var testDbName = "ATestDbForDbUserManager";
			AdoTestUtils.CreateDbDropExisting(testDbName);
			AdoTestUtils.DropDbLoginIfExists(TestConnection, testLoginName);

			var info = new StaffLoginInfo()
			{
				LoginName = testLoginName,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "db_datawriter", "cwRestrictedReaderRole", "db_backupoperator" },
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => "",
			};

			var dbManager = new DbUserManagerForTesting();
			try
			{
				dbManager.CreateDbLogin_Exposed(TestConnection, testLoginName, new HashSet<string>(), () => "", "000");
			}
			catch (SqlException ex) when (ex.Message.Contains("Incorrect syntax near '000'."))
			{
				//expected error, Invalid Password Hash
			}

			var hashedPwd = "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e";

			var fullLoginName = dbManager.CreateDbLogin_Exposed(TestConnection, testLoginName, new HashSet<string>(), () => "", hashedPwd);

			AdoTestUtils.DropDbLoginIfExists(TestConnection, fullLoginName);
			Assert(!DbUserManagerForTesting.CheckLoginExists(testLoginName, TestConnection));
			dbManager.CreateDbLogin_Exposed(TestConnection, testLoginName, new HashSet<string>(), () => "", hashedPwd);

			var hashedPwdFromSqlLogin = TestConnection.GetSqlLoginHashedPassword(fullLoginName);
			AssertEquals("SQL Principal is created with correct hashed password", hashedPwd, hashedPwdFromSqlLogin);

			dbManager.EnsureReaderRoleExist_Exposed(TestConnection, testDbName);
			dbManager.CreateDbUsersAndPermissions_Exposed(TestConnection, testDbName, info);
			Assert("Staff login is created and member of CwRestrictedReaderRole", CheckUserHasRightsOnDb(testDbName, testLoginName, DbRoleTypes.CwRestrictedReaderRole));

			AdoTestUtils.DropDbIfExists(TestConnection, testDbName);
			dbManager.CreateDbUsersAndPermissions_Exposed(TestConnection, testDbName, info);
		}

		public void TestUserCreationFailureIsLoggedAsWarning()
		{
			// Arrange
			var info = new StaffLoginInfo
			{
				LoginName = "BadDomain\\BadUser",
				StaffPK = Guid.NewGuid(),
				StaffDatabaseAccessGroupRoles = new HashSet<string>(),
				GetDownLevelLogonName = () => "",
			};

			var logger = new Mock<ILogger>();
			var dbManager = new DbUserManagerForTesting(logger.Object);

			// Act
			dbManager.CreateDbUsersAndPermissions_Exposed(TestConnection, info);

			// Assert
			AssertEquals("No error", 0, ErrorReporter.TotalErrorCount);
			logger.Verify(l => l.Log(LogType.Warning, It.Is<string>(s => s.Contains("Failed to create database user ")), It.IsAny<SqlException>()));
		}

		[UseSnapshotProtection]
		public void TestCreateDbUser_InformativeErrorMessageIsLoggedWhenUserCreationFails()
		{
			// Arrange
			const string sid1 = "0xE362AE3A4BA913449F14FB5E02FCDDB8";
			const string sid2 = "0xE362AE3A6A8E173F48C12B5E02FCD000";
			const string staffLoginName = "DbUserManagerTest_Staff_SchemaOwnerLogin";
			var fullStaffLoginName = DbUserManagerForTesting.GetFullUserLoginName(staffLoginName, () => "");
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				StaffDatabaseAccessGroupRoles = new HashSet<string>() { "cwRestrictedReaderRole" },
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => "",
				DbAuthenticationMode = DatabaseAuthenticationMode.Sql
			};
			var logger = new TestServiceLogger();
			var dbManager = new DbUserManager(logger);
			var dummyManagerToCreateLogins = new DbUserManagerForTesting();

			try
			{
				dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, staffLoginName, new HashSet<string>(), () => "", hashedPwd: "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e", sid: sid1);
				dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, staffLoginName, new HashSet<string>() { "cwRestrictedReaderRole" }, () => "");
				DbUserManagerForTesting.CreateUserSchemaOnMainDb(staffLoginName, TestConnection);
				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(staffLoginName, TestConnection);
				dummyManagerToCreateLogins.DropDbLogin_Exposed(TestConnection, staffLoginName, () => "");
				dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, staffLoginName, new HashSet<string>(), () => "", hashedPwd: "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e", sid: sid2);

				// Act
				dbManager.CreateDbUsersAndPermissions_Exposed(TestConnection, info);

				// Assert
				Assert(DbUserManagerForTesting.CheckUserHasSchemaOnDb(Db.DatabaseName, staffLoginName, TestConnection));
				AssertContains("User is provided extra details on the error", "This can be because of a pre-existing database user for the associated login", logger.ToString());
			}
			finally
			{
				dummyManagerToCreateLogins.DropDbLogin_Exposed(TestConnection, staffLoginName, () => "");
			}
		}

		[UseSnapshotProtection]
		public void TestDropUser_WithImpersonation()
		{
			var logger = new Mock<ILogger>().Object;
			var dbUserManager = new DbUserManager(logger);
			var staffLogins = new List<string>();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var allDatabases = adminConnection.GetDatabases(DatabaseType.AllWritable, writable: true);

				try
				{
					// Arrange
					var staffLogin1 = CreateStaffLogin(dbUserManager);
					var staffLogin2 = CreateStaffLogin(dbUserManager);

					staffLogins.Add(staffLogin1.LoginName);
					staffLogins.Add(staffLogin2.LoginName);
					staffLogins.ForEach(x =>
						Assert(DbUserExists(adminConnection, Db.DatabaseName, x)));

					var staffLoginName = staffLogins[0];
					var impersonatedLoginName = staffLogins[1];
					allDatabases.ForEach(db =>
						Impersonate(adminConnection, db, staffLoginName, impersonatedLoginName));

					// Act
					AssertNoExceptionThrown(() =>
						dbUserManager.DropDbUsersWithLoginInfo_Exposed(adminConnection, staffLogin1));

					// Assert
					CombineAssertions(
						$"Db user: {staffLoginName.QuoteName('\'')} with impersonation(s) should have been dropped.", () =>
						{
							allDatabases.ForEach(db =>
								Assert($"Not dropped and still exists on: {db.QuoteName()}", !DbUserExists(adminConnection, db, staffLoginName)));
						});
				}
				finally
				{
					staffLogins.ForEach(staffLoginName =>
						allDatabases.ForEach(db => DropDbUserIfExists(adminConnection, db, staffLoginName)));
				}
			}

			StaffLoginInfo CreateStaffLogin(DbUserManager dbUserManager)
			{
				var staffLoginInfo = new StaffLoginInfo()
				{
					LoginName = Invariant($"{Guid.NewGuid():N}"),
					StaffPK = Guid.NewGuid(),
					StaffDatabaseAccessGroupRoles = new HashSet<string>() { "cwRestrictedReaderRole" },
					DbPermissionChanged = true,
					GetDownLevelLogonName = () => "",
					HashedPassword = "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e",
					DbAuthenticationMode = DatabaseAuthenticationMode.Sql,
				};

				dbUserManager.CreateStaffLogin_Exposed(staffLoginInfo);
				return staffLoginInfo;
			}
		}

		bool DbUserExists(DbConnection connection, string db, string dbUserName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(db))
			{
				var sql = Invariant($"select count(*) from sys.database_principals where name = {dbUserName.QuoteName('\'')}");
				return connection.ExecuteScalar<int>(sql) > 0;
			}
		}

		void Impersonate(DbConnection connection, string db, string staffLoginName, string impersonatedLoginName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(db))
			{
				connection.ExecuteNonQuery(
					Invariant($"GRANT IMPERSONATE ON USER::{staffLoginName} TO {impersonatedLoginName.QuoteName()}"));
			}
		}

		void DropDbUserIfExists(DbConnection connection, string db, string dbUserName)
		{
			const string sql = @"-- BaseDropUser
DECLARE @execCmd nvarchar(4000);

if (EXISTS (SELECT NULL FROM sys.database_principals WHERE name = @userLogin))
BEGIN
	SET @execCmd = NULL;
	SELECT
		@execCmd = isnull(@execCmd + ';', '') +
		' REVOKE IMPERSONATE ON USER::' + QUOTENAME(dp_grantor.name) +
		' FROM ' + QUOTENAME(dp_grantee.name)
	FROM
		sys.database_permissions perm
		join sys.database_principals dp_grantee on dp_grantee.principal_id = perm.grantee_principal_id
		join sys.database_principals dp_grantor on dp_grantor.principal_id = perm.grantor_principal_id
	WHERE
		perm.class = 4
		AND dp_grantor.name = @userLogin;

	IF (@execCmd is NOT NULL) EXEC sys.sp_executesql @execCmd;

	SET @execCmd = N'DROP USER ' + QUOTENAME(@userLogin);
	EXEC sys.sp_executesql @execCmd;
END
";
			using (((ICurrentDbControl)connection).UseDatabase(db))
			{
				using (var cmd = connection.Command(sql))
				{
					cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, dbUserName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCreateDbUser_Impersonate()
		{
			var testLoginName = "User_TestCreateDbUser_Impersonate";

			var info = new StaffLoginInfo()
			{
				LoginName = testLoginName,
				StaffPK = Guid.NewGuid(),
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = new HashSet<string>(),
				DbPermissionChanged = true,
				GetDownLevelLogonName = () => "",
			};

			var dbManager = new DbUserManagerForTesting();

			var fullLoginName = dbManager.CreateDbLogin_Exposed(TestConnection, testLoginName, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");

			dbManager.CreateDbUsersAndPermissions_Exposed(TestConnection, info);

			using (var cmd = TestConnection.Command(ImpersonatePermissionSQL))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, fullLoginName);
				cmd.AddParameter("@impersonatedToUserLogin", SqlDbType.NVarChar, 128, UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
				var permissionName = cmd.ExecuteScalar();

				AssertEquals("Should have impersonate permission", "IMPERSONATE", permissionName);
			}
		}

		const string ImpersonatePermissionSQL = @"
SELECT
	pe.permission_name
FROM
	sys.database_permissions AS pe
JOIN
	sys.database_principals AS pr1
ON
	pe.grantor_principal_id = pr1.principal_id
JOIN
	sys.database_principals AS pr2
ON
	pe.grantee_principal_id = pr2.principal_id
WHERE
	pr1.name = @userLogin AND pr2.name = @impersonatedToUserLogin";

		void AssertHostedLocationRelatedServerRights(string message, string staffLoginName, bool grantedAlterTrace, bool deniedViewAnyDatabase, bool grantedViewAnyDefinition, bool grantedAlterAnyEventSession)
		{
			AssertEquals(message + " - [" + staffLoginName + "] was GRANTED [ALTER TRACE] rights on the server?", grantedAlterTrace, CheckUserHasGrantedServerLevelPermission(staffLoginName, "ALTR"));
			AssertEquals(message + " - [" + staffLoginName + "] was DENIED [VIEW ANY DATABASE] rights on the server?", deniedViewAnyDatabase, CheckUserHasDeniedServerLevelPermission(staffLoginName, "VWDB"));
			AssertEquals(message + " - [" + staffLoginName + "] was GRANTED [VIEW ANY DEFINITION] rights on the server?", grantedViewAnyDefinition, CheckUserHasGrantedServerLevelPermission(staffLoginName, "VWAD"));
			AssertEquals(message + " - [" + staffLoginName + "] was GRANTED [ALTER ANY EVENT SESSION] rights on the server?", grantedAlterAnyEventSession, CheckUserHasGrantedServerLevelPermission(staffLoginName, "AAES"));
		}

		protected void CreateLoginsAndUsersForSyncTest()
		{
			DbUserManagerForTesting dummyManagerToCreateLogins = new DbUserManagerForTesting();

			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, StaffLogin01, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, StaffLogin03, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, StaffLogin04, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, StaffLogin06, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, StaffLogin09, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");
			dummyManagerToCreateLogins.CreateDbLogin_Exposed(TestConnection, BadStaffLogin03, new HashSet<string>(), () => "", "0x02009b4291a6ae9f607f69ebb6a7559fa48044632166e4bb3d3463c6ad6fc62cf3fa414f37167674d16c6c8cf74f3c4c34d95177c3090406b489e2c3a6ea7ca5d1ab4528df0e");

			dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, StaffLogin01, new HashSet<string>() { "cwRestrictedReaderRole" }, () => "");
			dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, StaffLogin03, new HashSet<string>() { "cwRestrictedReaderRole", "cwHRMStaffRole" }, () => "");
			dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, StaffLogin04, new HashSet<string>() { "cwRestrictedReaderRole" }, () => "");
			dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, StaffLogin06, new HashSet<string>() { "cwRestrictedReaderRole", "db_backupoperator", "cwHRMStaffRole" }, () => "");
			dummyManagerToCreateLogins.CreateDbUsersAndManageRoles_Exposed(TestConnection, BadStaffLogin03, new HashSet<string>() { "cwRestrictedReaderRole", "db_backupoperator", "cwHRMStaffRole" }, () => "");
		}

		bool LogContains(TestServiceLogger logger, string message)
		{
			if (logger == null)
			{
				return false;
			}

			for (var i = 0; i < logger.Count; ++i)
			{
				var logMessage = logger[i];

				if (logMessage.Contains(message))
				{
					return true;
				}
			}

			return false;
		}

		public void TestEnsureApplicationDbLoginRightsForAuditDb()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var loginRepairConnection = (IDbLoginRepair)connection;

				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: true);

				loginRepairConnection.DropDbLoginUsersFromDatabase(Db.AuditDatabaseName, msg => { });
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: false);

				var dbUserManager = new DbUserManager();

				dbUserManager.EnsureApplicationLoginForBiDatabase(Db.ServerName, Db.AuditDatabaseName);

				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.AuditDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: true);
			}
		}

		public void TestEnsureApplicationDbLoginRightsForEdwDb()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var loginRepairConnection = (IDbLoginRepair)connection;

				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: true);

				loginRepairConnection.DropDbLoginUsersFromDatabase(Db.EdwDatabaseName, msg => { });
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: false);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: false);

				var dbUserManager = new DbUserManager();

				dbUserManager.EnsureApplicationLoginForBiDatabase(Db.ServerName, Db.EdwDatabaseName);

				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(connection, Db.EdwDatabaseName, loginRepairConnection.ReaderDbLoginName, expected: true);
			}
		}

		public void TestShowExceptionMessageToUsersWhenAllRecipientsOfEmailIsEmpty()
		{
			var testDbUserManager = new ReplicationResult();
			testDbUserManager.NewNotification(new Tuple<string, string, string>("test", "test", "test"));

			AssertNoExceptionThrown(() => testDbUserManager.SendLoginNotPropagatedNotification());
			AssertEquals($"Cannot send email without recipients. Following notification groups do not contain any users with email address: Database Health Check Notification Group, Company Notification Group (for Company {EnvProxy.Instance.CurrentCompany.Name}), Company Notification Group (for System level). Please ensure that there is at least one user with valid email address in above notification groups. You can find these notification groups in Registry -> Notification.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
