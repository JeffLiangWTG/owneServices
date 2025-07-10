using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class DbUserManagerWithStorageDocsAndReadonlyDbTest : DbUserManagerBaseTest
	{
		public void TestApplyStaffChangesToDbLogins()
		{
			// LLZ: This test tests that database roles are applied correctly (added or removed)
			// It also tests that if there is a non-empty schema that belongs to an sql user corresponding to staff member
			// then this staff member cannot be dropped.
			// Finally, it also tests that no changes are applied to readon storage docs. This functionality is tested sufficiently by tests for SqlSecurityManager
			// When UseModernSqlSecuritySystem is true, adding/removing roles testing is replaced with a number of other tests inside DbSecurityAdminTaskSecurityTest
			// Removing schemas is no longer supported even if it is empty so this test is replaced with a new validation test TestAttemptToDropStaffThatOwnsEmptySchemaResultsInValidationError
			// Once UseModernSqlSecuritySystem flag is removed, this test can be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			bool storageDocsDbExist = DoesDbExist(StorageDbName);

			if (!storageDocsDbExist && TestingState.IsRunningOnDAT)
			{
				Fail("The first Storage Docs database [" + StorageDbName + "] must exist");
			}

			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = GetNewStaff(factory, StaffLogin01);
			var staff02 = GetNewStaff(factory, StaffLogin02);
			staff01.IsReadOnlyDBUser = true;
			staff02.IsBackupOperator = true;

			factory.Save();

			CombineAssertions(() =>
			{
				Assert("(1) StaffLogin01 DB login should have been created", CheckLoginExists(StaffLogin01));
				Assert("(1) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(1) StaffLogin01 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
				Assert("(1) StaffLogin01 should have [cwRestrictedReaderRole] rights on Audit DB", CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(1) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

				Assert("(1) StaffLogin02 DB login should have been created", CheckLoginExists(StaffLogin02));
				Assert("(1) StaffLogin02 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_backupoperator"));
				Assert("(1) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(1) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on Audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(1) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));

				if (storageDocsDbExist)
				{
					Assert("(1) StaffLogin01 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					Assert("(1) StaffLogin02 should have [backupoperator] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin02, "db_backupoperator"));
				}

				// Read-only StorageDocs database should have no changes (no users cretaed)
				Assert(ReadonlyStorageDbName + " should be readonly", !TestConnection.IsDbWriteable(ReadonlyStorageDbName));
				Assert("(1) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on " + ReadonlyStorageDbName, !CheckUserHasRightsOnDb(ReadonlyStorageDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			});

			staff01.IsBackupOperator = true;
			staff02.GS_FullName = "AnyCodeToForceRowStateToChange";
			factory.Save();

			CombineAssertions(() =>
			{
				Assert("(2) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(2) StaffLogin01 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
				Assert("(2) StaffLogin01 should have [cwRestrictedReaderRole] rights on Audit DB", CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(2) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));

				Assert("(2) StaffLogin02 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, "db_backupoperator"));
				Assert("(2) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(2) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on Audit DB", !CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(2) StaffLogin02 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));

				if (storageDocsDbExist)
				{
					Assert("(2) StaffLogin01 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
					Assert("(2) StaffLogin01 should have [backupoperator] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin01, "db_backupoperator"));
				}

				// Read-only StorageDocs database should have no changes (no users cretaed)
				Assert("(2) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on " + ReadonlyStorageDbName, !CheckUserHasRightsOnDb(ReadonlyStorageDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			});

			// Create schema for users to assert they are deleted when the associated user is deleted
			DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin01, TestConnection);
			DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin02, TestConnection);

			CombineAssertions(() =>
			{
				Assert("StaffLogin01 should have a schema on main DB", CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin01));
				Assert("StaffLogin02 should have a schema on main DB", CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin02));
			});

			staff02.IsBackupOperator = false;
			factory.Save();

			CombineAssertions(() =>
			{
				Assert("(3) StaffLogin02 DB login should have been DROPPED", !CheckLoginExists(StaffLogin02));
				Assert("(3) StaffLogin01 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(3) StaffLogin01 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin01, "db_backupoperator"));
				Assert("(3) StaffLogin01 should have [cwRestrictedReaderRole] rights on Audit DB", CheckUserHasRightsOnDb(Db.AuditDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
				Assert("(3) StaffLogin01 should NOT have [cwRestrictedReaderRole] rights on EDW DB", !CheckUserHasRightsOnDb(Db.EdwDatabaseName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			});

			staff01.Delete();
			staff02.Delete();
			factory.Save();

			CombineAssertions(() =>
			{
				Assert("(4) StaffLogin01 DB login should have been DROPPED", !CheckLoginExists(StaffLogin01));
				Assert("(4) StaffLogin02 DB login should still NOT exist", !CheckLoginExists(StaffLogin02));

				Assert("StaffLogin01 schema should have been deleted", !CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin01));
				Assert("StaffLogin02 schema should have been deleted", !CheckUserHasSchemaOnDb(Db.DatabaseName, StaffLogin02));
			});
		}

		static readonly string StorageDbName = Db.DatabaseName + "_SD001";
		static readonly string ReadonlyStorageDbName = Db.DatabaseName + "_SD123";

		public void TestDbUserOwnsDatabaseObjects()
		{
			// LLZ: This test is irrelevant to modern security as objects dropping is not supported there
			// when UseModernSqlSecuritySystem flag is removed this test needs to be deleted
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory(TestConnection);
			var staff = GetNewStaff(factory, StaffLogin01);
			staff.IsDatabaseDeveloper = true;
			factory.Save();
			AssertEquals(false, DbUserManager.StaffLoginOwnsSchemaWithCurrentObjects(TestConnection, staff.GS_LoginName, staff.GetDownLevelLogonName));

			DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin01, TestConnection);
			DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);
			AssertEquals(true, DbUserManager.StaffLoginOwnsSchemaWithCurrentObjects(TestConnection, staff.GS_LoginName, staff.GetDownLevelLogonName));

			DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(StaffLogin01, TestConnection);
			AssertEquals(false, DbUserManager.StaffLoginOwnsSchemaWithCurrentObjects(TestConnection, staff.GS_LoginName, staff.GetDownLevelLogonName));
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, StorageDbName);
				AdoTestUtils.CreateDbIfNotExists(auxConnection, ReadonlyStorageDbName);
				auxConnection.ExecuteNonQuery(string.Format("ALTER DATABASE [{0}] SET READ_ONLY WITH ROLLBACK IMMEDIATE;", ReadonlyStorageDbName));
			}
		}

		protected override void FinalTearDown()
		{
			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(auxConnection, ReadonlyStorageDbName);
			}

			base.FinalTearDown();
		}
	}
}
