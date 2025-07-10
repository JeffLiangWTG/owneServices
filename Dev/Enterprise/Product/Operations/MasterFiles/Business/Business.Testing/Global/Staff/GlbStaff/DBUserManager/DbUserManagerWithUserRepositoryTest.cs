using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.MasterFiles.Business.DbUserManager;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DbUserManagerWithUserRepositoryTest : DbUserManagerBaseTest
	{
		public void TestDbNameShouldBeCaseInsensitive()
		{
			var testDbUserManager = new DbUserManagerForTesting();
			var dbName = Db.DatabaseName.ToLower();
			AssertEquals("[PRE-CONDITION] Db.DatabaseName has upper case characters", false, dbName == Db.DatabaseName);
			Assert("Database name should be case insensitive.", testDbUserManager.GetOnlineDatabaseList_Exposed(TestConnection).Contains(dbName));
		}

		public void TestStaffPasswordChangeDoesNotChangeSQLLoginPasswordAndViceVersa()
		{
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			serviceTaskNudgerMock.Setup(nudger => nudger.NudgeServiceTask("DSA", null))
				.Callback(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: true);
						sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);
					}
				});

			var staffPassword = "something";
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.StaffPlainTextPassword = staffPassword;
			staff01.IsReadOnlyDBUser = false;
			staff01.IsDatabaseDeveloper = false;
			factory.Save();

			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;

			var oldSQLPassword = "oldPasswrod!@34";
			var newSQLPassword = "newPassword#$%12";

			try
			{
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				{
					AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
					AssertEquals("[PRE-CONDITION] User Repository Database exists?", false, DoesDbExist(DbUserManager.UserRepositoryDb));

					new DbUserManager().SetPasswordForStaff(staff01, oldSQLPassword);
					factory.Save();

					AssertEquals("(1) StaffLogin01 DB login created?", true, CheckLoginExists(StaffLogin01));
					AssertEquals("Password on GlbStaff should not change", true, staff01.VerifyPassword(staffPassword));

					var sqlServerPrincipalName = new DbUserManager().GetFullUserLoginName_Exposed(StaffLogin01, () => "ADomainSomehere.58", DatabaseAuthenticationMode.Sql);

					new DbUserManager().SetPasswordForStaff(staff01, oldSQLPassword);
					factory.Save();

					AssertStaffCanLogin(username: sqlServerPrincipalName, password: oldSQLPassword);
					AssertEquals("Password on GlbStaff should not change", true, staff01.VerifyPassword("something"));

					new DbUserManager().SetPasswordForStaff(staff01, newSQLPassword);
					factory.Save();

					AssertEquals("Password on GlbStaff should not change", true, staff01.VerifyPassword("something"));
					AssertStaffCanLogin(username: sqlServerPrincipalName, password: newSQLPassword);

					// staff password cleared
					staff01.LocalPasswordMustBeReset = true;
					factory.Save();
					AssertStaffCanLogin(username: sqlServerPrincipalName, password: newSQLPassword);

					// staff password changed
					staff01.ChangeLocalPassword(null, "newStaffPassword");
					factory.Save();
					AssertStaffCanLogin(username: sqlServerPrincipalName, password: newSQLPassword);
				}
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
			}
		}

		void AssertStaffCanLogin(string username, string password)
		{
			using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, username, password))
			{
				connection.EnsureIsOpen();
				//Should not throw exceptions
			}
		}

		public void TestStaffRightsGrantedOnUserRepositoryArePreservedWhenCleaningUpUnauthorisedDatabaseLevelPermissions()
		{
			// LLZ: this test is irrelevant for modern security. Moreover, the clean up code will have to be removed
			// once modern security is in place. Hence, once UseModernSqlSecuritySystem flag is removed, this test should be removed.
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;

			try
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
				AssertEquals("[PRE-CONDITION] User Repository Database exists?", false, DoesDbExist(DbUserManager.UserRepositoryDb));

				//
				// Initial GlbStaff changes
				factory.Save();
				AssertDatabaseDeveloperRights(StaffLogin01, expectedValue: true);
				AssertDatabaseReaderRights(StaffLogin01, expectedValue: true);

				new DbSecurityForTest().CleanUpDatabaseLevelPermissions_Exposed(TestConnection, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix);
				AssertDatabaseDeveloperRights(StaffLogin01, expectedValue: true);
				AssertDatabaseReaderRights(StaffLogin01, expectedValue: true);
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
			}
		}

		class DbSecurityForTest : DbSecurityLockDown
		{
			public void CleanUpDatabaseLevelPermissions_Exposed(AdminConnection connection, string dbName)
			{
				var logBuilder = new StringBuilder();
				CleanUpDatabaseLevelPermissions(connection, dbName, logBuilder);
			}
		}

		public void TestApplyStaffChangesToDbLogins()
		{
			// LLZ: This test tests that database roles and permissions are applied correctly (added or removed)
			// It also tests that UserRepository database is created. The creation of user repository will be covered by a separate WI
			// Finally, it tests logins and users deletion when staff member is deleted
			// When UseModernSqlSecuritySystem is true, adding/removing roles testing is replaced with a number of other tests inside DbSecurityAdminTaskSecurityTest
			// Adding correct permissions is tested by tests starting from TestCorrectUserRightsAreAddedTo  inside DbSecurityAdminTaskSecurityTest
			// Deleting logins and users is covered by tests starting from TestLoginsAndUsersAreDroppedWhenStaffMemberIsDeleted and TestLoginsAndUsersAreDroppedWhenDatabaseAccessIsRevokedFromStaffMember inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			var staff02 = GetNewStaff(factory, StaffLogin02);
			var staff03 = GetNewStaff(factory, StaffLogin03);
			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;
			staff02.IsBackupOperator = true;
			staff03.IsReadOnlyDBUser = true;

			try
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
				AssertEquals("[PRE-CONDITION] User Repository Database exists?", false, DoesDbExist(DbUserManager.UserRepositoryDb));

				//
				// Initial GlbStaff changes
				factory.Save();

				CombineAssertions(() =>
				{
					// DB Logins
					AssertEquals("(1) StaffLogin01 DB login created?", true, CheckLoginExists(StaffLogin01));
					AssertEquals("(1) StaffLogin02 DB login created?", true, CheckLoginExists(StaffLogin02));
					AssertEquals("(1) StaffLogin03 DB login created?", true, CheckLoginExists(StaffLogin03));

					// Main Database
					AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin01 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_datawriter"));
					AssertEquals("(1) StaffLogin01 has [backupoperator] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
					AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on Audit DB?", true, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on EDW DB", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

					AssertEquals("(1) StaffLogin02 has [cwRestrictedReaderRole] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin02 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_datawriter"));
					AssertEquals("(1) StaffLogin02 has [backupoperator] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_backupoperator"));
					AssertEquals("(1) StaffLogin02 has [cwRestrictedReaderRole] rights on Audit DB?", false, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on EDW DB?", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));

					AssertEquals("(1) StaffLogin03 has [cwRestrictedReaderRole] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin03 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, "db_datawriter"));
					AssertEquals("(1) StaffLogin03 has [backupoperator] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin03, "db_backupoperator"));
					AssertEquals("(1) StaffLogin03 has [cwRestrictedReaderRole] rights on Audit DB?", true, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin03 should NOT have [cwRestrictedReaderRole] rights on EDW DB?", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));

					// User Repository Database
					// Assert repository database and synonyms were created
					AssertEquals("User Repository Database exists?", true, DoesDbExist(DbUserManager.UserRepositoryDb));
					string sqlText = string.Format("SELECT count(*) FROM [{0}].sys.synonyms", DbUserManager.UserRepositoryDb);
					AssertEquals("Synonyms created?", true, (int)TestConnection.ExecuteScalar(sqlText) > 0);

					AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin01 has [backupoperator] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "db_backupoperator"));
					AssertEquals("(1) StaffLogin01 has [execute] rights on user repository DB?", true, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "EX"));

					AssertEquals("(1) StaffLogin02 has [cwRestrictedReaderRole] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin02 has [backupoperator] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "db_backupoperator"));
					AssertEquals("(1) StaffLogin02 has [execute] rights on user repository DB?", false, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "EX"));

					AssertEquals("(1) StaffLogin03 has [cwRestrictedReaderRole] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin03, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(1) StaffLogin03 has [backupoperator] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin03, "db_backupoperator"));
					AssertEquals("(1) StaffLogin03 has [execute] rights on user repository DB?", true, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin03, "EX"));

					// Database Developer Rights
					AssertDatabaseDeveloperRights(StaffLogin01, expectedValue: true);
					AssertDatabaseDeveloperRights(StaffLogin02, expectedValue: false);
					AssertDatabaseDeveloperRights(StaffLogin03, expectedValue: false);

					// Database Reader Rights
					AssertDatabaseReaderRights(StaffLogin01, expectedValue: true);
					AssertDatabaseReaderRights(StaffLogin03, expectedValue: true);
				});

				//
				// Make changes to staff records
				staff01.IsBackupOperator = true;
				staff01.IsDatabaseDeveloper = false;
				staff02.GS_FullName = "AnyCodeToForceRowStateToChange";
				staff03.IsReadOnlyDBUser = false;
				factory.Save();

				// DB Logins
				AssertEquals("(2) StaffLogin03 DB login exists?", false, CheckLoginExists(StaffLogin03));

				CombineAssertions(() =>
				{
					// Main Database
					AssertEquals("(2) StaffLogin01 has [cwRestrictedReaderRole] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin01 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_datawriter"));
					AssertEquals("(2) StaffLogin01 has [backupoperator] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
					AssertEquals("(2) StaffLogin01 has [cwRestrictedReaderRole] rights on Audit DB?", true, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on EDW DB", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

					AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin02 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_datawriter"));
					AssertEquals("(2) StaffLogin02 has [backupoperator] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_backupoperator"));
					AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on Audit DB?", false, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin02 should NOT [cwRestrictedReaderRole] rights on EDW DB?", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));

					// User Repository Database
					AssertEquals("(2) StaffLogin01 has [cwRestrictedReaderRole] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin01 has [backupoperator] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "db_backupoperator"));
					AssertEquals("(2) StaffLogin01 has [execute] rights on user repository DB?", true, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "EX"));

					AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin02 has [backupoperator] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "db_backupoperator"));
					AssertEquals("(2) StaffLogin02 has [execute] rights on user repository DB?", false, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "EX"));

					// Database Developer Rights
					AssertDatabaseDeveloperRights(StaffLogin01, expectedValue: false);
					AssertDatabaseDeveloperRights(StaffLogin02, expectedValue: false);

					// Database Reader Rights
					AssertDatabaseReaderRights(StaffLogin01, expectedValue: true);
					AssertDatabaseReaderRights(StaffLogin03, expectedValue: false);
				});

				//
				// Remove all test staff 1 database permissions => user/login should be removed
				staff01.IsReadOnlyDBUser = false;
				staff01.IsBackupOperator = false;
				staff01.IsDatabaseDeveloper = false;
				factory.Save();

				AssertEquals("(3) StaffLogin01 DB login exists?", false, CheckLoginExists(StaffLogin01));

				CombineAssertions(() =>
				{
					AssertEquals("(3) StaffLogin01 has [cwRestrictedReaderRole] rights on Audit DB?", false, CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(3) StaffLogin01 has [cwRestrictedReaderRole] rights on EDW DB?", false, CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(3) StaffLogin02 has [backupoperator] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "db_backupoperator"));
				});

				// Delete all test staff => users/logins should be removed
				staff01.Delete();
				staff02.Delete();
				staff03.Delete();
				factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("(4) StaffLogin01 DB login exists?", false, CheckLoginExists(StaffLogin01));
					AssertEquals("(4) StaffLogin02 DB login exists?", false, CheckLoginExists(StaffLogin02));
					AssertEquals("(4) StaffLogin03 DB login exists?", false, CheckLoginExists(StaffLogin03));
				});
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateNameToRemoveQuoteAfterHavingBeenSavedThenReaddQuote()
		{
			// LLZ: This test is irrelevant for when UseModernSqlSecuritySystem flag is on as synchronising staff with sql logins
			// is done usynchronously in this situation
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			try
			{
				DropExtraLogin();
				var factory = new BusinessObjectFactory(TestConnection);
				var staff01 = GetNewStaff(factory, StaffLogin03 + "'");
				staff01.IsDatabaseDeveloper = true;
				factory.Save();

				staff01.GS_LoginName = StaffLogin03;
				factory.Save();

				staff01.GS_LoginName = StaffLogin03 + "'";
				factory.Save();
			}
			finally
			{
				DropExtraLogin();
			}
		}

		void DropExtraLogin()
		{
			try
			{
				Db.Connection.ExecuteNonQuery($"DROP LOGIN [{StaffLogin03 + "'"}]");
			}
			catch (SqlException)
			{
			}
		}

		public void TestApplyStaffChangesWhenUserRepositoryIsOffline()
		{
			//LLZ: a new workitem is created to deal with offline database when moderns security is being used
			// When UseModernSqlSecuritySystem flag is removed this test should be dropped as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			staff01.IsReadOnlyDBUser = true;
			staff01.IsDatabaseDeveloper = true;

			try
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
				AssertEquals("[PRE-CONDITION] User Repository Database exists?", false, DoesDbExist(DbUserManager.UserRepositoryDb));

				//
				// Initial GlbStaff changes
				factory.Save();

				// DB Logins
				AssertEquals("(1) StaffLogin01 DB login created?", true, CheckLoginExists(StaffLogin01));

				// Main Database
				AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				AssertEquals("(1) StaffLogin01 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_datawriter"));
				AssertEquals("(1) StaffLogin01 has [backupoperator] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));

				// User Repository Database
				// Assert repository database and synonyms were created
				AssertEquals("User Repository Database exists?", true, DoesDbExist(DbUserManager.UserRepositoryDb));
				string sqlText = string.Format("SELECT count(*) FROM [{0}].sys.synonyms", DbUserManager.UserRepositoryDb);
				AssertEquals("Synonyms created?", true, (int)TestConnection.ExecuteScalar(sqlText) > 0);

				AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				AssertEquals("(1) StaffLogin01 has [backupoperator] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "db_backupoperator"));
				AssertEquals("(1) StaffLogin01 has [execute] rights on user repository DB?", true, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin01, "EX"));

				// Database Developer Rights
				AssertDatabaseDeveloperRights(StaffLogin01, expectedValue: true);

				var staff02 = GetNewStaff(factory, StaffLogin02);
				staff02.IsReadOnlyDBUser = true;
				staff02.IsDatabaseDeveloper = true;

				TestConnection.ExecuteNonQuery(string.Format(@"ALTER DATABASE [{0}] SET OFFLINE", DbUserManager.UserRepositoryDb));
				try
				{
					factory.Save();

					// DB Logins
					AssertEquals("(2) StaffLogin02 DB login created?", true, CheckLoginExists(StaffLogin02));

					// Main Database
					AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on main DB?", true, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					AssertEquals("(2) StaffLogin02 has [datawriter] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_datawriter"));
					AssertEquals("(2) StaffLogin02 has [backupoperator] rights on main DB?", false, CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_backupoperator"));

					// User Repository Database, StaffLogin02 should not exist, as the database was offline.
					bool exceptionThrown = false;
					try
					{
						AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
					}
					catch (SqlException)
					{
						exceptionThrown = true;
					}
					Assert("UserRepository should be offline", exceptionThrown);
				}
				finally
				{
					TestConnection.ExecuteNonQuery(string.Format(@"ALTER DATABASE [{0}] SET ONLINE", DbUserManager.UserRepositoryDb));
				}

				new DbUserManager().SynchroniseAllStaffAndDbLogins(TestConnection);

				//User Repository Database, StaffLogin02 should now exist as the database is back online.
				AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on user repository DB?", true, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
				AssertEquals("(2) StaffLogin02 has [backupoperator] rights on user repository DB?", false, CheckUserHasRightsOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "db_backupoperator"));
				AssertEquals("(2) StaffLogin02 has [execute] rights on user repository DB?", true, CheckUserHasGrantedPermissionOnDb(DbUserManager.UserRepositoryDb, StaffLogin02, "EX"));

				// Database Developer Rights
				AssertDatabaseDeveloperRights(StaffLogin02, expectedValue: true);

				//
				// Delete all test staff => users/logins should be removed
				staff01.Delete();
				staff02.Delete();
				factory.Save();

				AssertEquals("(4) StaffLogin01 DB login exists?", false, CheckLoginExists(StaffLogin01));
				AssertEquals("(4) StaffLogin02 DB login exists?", false, CheckLoginExists(StaffLogin02));
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(TestConnection, DbUserManager.UserRepositoryDb);
			}
		}
	}
}
