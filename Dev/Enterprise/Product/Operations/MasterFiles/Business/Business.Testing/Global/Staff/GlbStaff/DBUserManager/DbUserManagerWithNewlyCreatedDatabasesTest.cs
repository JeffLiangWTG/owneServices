using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DbUserManagerWithNewlyCreatedDatabasesTest : DbUserManagerBaseTest
	{
		[UseSnapshotProtection]
		public void TestGrantStaffPermissionsToNewDatabase()
		{
			// LLZ: This test will become irrelevant for when UseModernSqlSecuritySystem flag is on
			// Once all of the self-healing is replaced with a nudge to DSA or removed
			// Hence, once UseModernSqlSecuritySystem is removed this test will have to be removed
			// This would mean that GrantStaffPermissionsToNewDatabase function is removed as well,
			// or replaced but either way the test will need to change or be replaced then
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var factory = new BusinessObjectFactory(TestConnection);
			var staff01 = factory.New<GlbStaff>();
			staff01.GS_LoginName = StaffLogin01;
			staff01.IsReadOnlyDBUser = ZBool.True;
			staff01.IsBackupOperator = ZBool.True;
			var staff02 = factory.New<GlbStaff>();
			staff02.GS_LoginName = StaffLogin02;
			staff02.IsReadOnlyDBUser = ZBool.True;
			factory.Save();

			var testDbUserManager = new DbUserManagerForTesting();
			AssertEquals("StaffLogin01 DB login exists?", true, CheckLoginExists(StaffLogin01));
			AssertEquals("StaffLogin02 DB login exists?", true, CheckLoginExists(StaffLogin02));

			// assert no rights to staff logins
			AssertEquals("(1) StaffLogin01 has [connect] rights on new DB?", false, CheckUserHasGrantedPermissionOnDb(TestNewDbName, StaffLogin01, "CO"));
			AssertEquals("(1) StaffLogin01 has [cwRestrictedReaderRole] rights on new DB?", false, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("(1) StaffLogin01 has [backupoperator] rights on new DB?", false, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin01, "db_backupoperator"));
			AssertEquals("(1) StaffLogin02 has [connect] rights on new DB?", false, CheckUserHasGrantedPermissionOnDb(TestNewDbName, StaffLogin02, "CO"));
			AssertEquals("(1) StaffLogin02 has [cwRestrictedReaderRole] rights on new DB?", false, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("(1) StaffLogin02 has [backupoperator] rights on new DB?", false, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin02, "db_backupoperator"));

			testDbUserManager.GrantStaffPermissionsToNewDatabase(TestConnection, TestNewDbName);

			// assert rights were granted to staff logins
			AssertEquals("(1) StaffLogin01 has [connect] rights on new DB?", true, CheckUserHasGrantedPermissionOnDb(TestNewDbName, StaffLogin01, "CO"));
			AssertEquals("(2) StaffLogin01 has [cwRestrictedReaderRole] rights on new DB?", true, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin01, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("(2) StaffLogin01 has [backupoperator] rights on new DB?", true, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin01, "db_backupoperator"));
			AssertEquals("(1) StaffLogin02 has [connect] rights on new DB?", true, CheckUserHasGrantedPermissionOnDb(TestNewDbName, StaffLogin02, "CO"));
			AssertEquals("(2) StaffLogin02 has [cwRestrictedReaderRole] rights on new DB?", true, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin02, DbRoleTypes.CwRestrictedReaderRole));
			AssertEquals("(2) StaffLogin02 has [backupoperator] rights on new DB?", false, CheckUserHasRightsOnDb(TestNewDbName, StaffLogin02, "db_backupoperator"));
		}

		static readonly string TestNewDbName = Db.DatabaseName + "_NewDb_E4090A6778134299ADFB3D922824F4FA";

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(auxConnection, TestNewDbName);
				AdoTestUtils.CreateDbIfNotExists(auxConnection, TestNewDbName);
			}
		}

		protected override void FinalTearDown()
		{
			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(auxConnection, TestNewDbName);
			}

			base.FinalTearDown();
		}
	}
}
