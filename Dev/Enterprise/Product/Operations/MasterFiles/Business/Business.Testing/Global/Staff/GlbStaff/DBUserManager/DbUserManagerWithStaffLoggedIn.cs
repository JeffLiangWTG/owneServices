using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	class DbUserManagerWithStaffLoggedIn : TestCase
	{
		public void TestSynchroniseDoesntOccurForInactiveRecordsWhileStaffIsLoggedIn()
		{
			// LLZ: When UseModernSqlSecuritySystem flag is on, this test is replaced with a test
			// TestNoExceptionIsThrownButReportOnceIssuedIfUserThatShouldBeDroppedIsLoggedInToSql inside SqlSecurityManagerTests
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			string loginName = "StaffName";
			string password = "boo";

			using (var testConnection = Db.NewAdminConnection())
			{
				//CREATE STAFF
				var factory = new BusinessObjectFactory(testConnection);
				GlbStaff staff = factory.New<GlbStaff>();
				staff.GS_LoginName = loginName;
				staff.StaffPlainTextPassword = password;
				staff.GS_IsActive = true;
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;

				var sysadminStaff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, "CWSupport"));
				sysadminStaff.GS_EmailAddress = "firstname1.lastname2@email.com";

				var postmasterGroup = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "PMG"));

				var newLink = factory.New<GlbGroupLink>();

				newLink.GK_GG = postmasterGroup.PK;
				newLink.GK_GS = sysadminStaff.PK;

				new DbUserManager().SetPasswordForStaff(staff, password);
				factory.Save();

				//LOGIN USING STAFF CREDENTIALS
				using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, "EnterpriseDbUser_" + Db.DatabaseName + "_" + loginName, password))
				{
					connection.EnsureIsOpen();

					//SET STAFF AS INACTIVE
					staff.GS_IsActive = false;
					AssertNoExceptionThrown(factory.Save);
				}

				//DROP CREATED LOGIN
				new DbUserManager().SynchroniseAllStaffAndDbLogins(testConnection);
				Assert(loginName + " should not exist.", !DbUserManagerForTesting.CheckLoginExists(loginName, testConnection));
			}
		}

		public void TestStaffCanStillLoginAfterEnsuringApplicationLoginsForBiDatabase()
		{
			// LLZ: This test is irrelevant when UseModernSqlSecuritySystem flag is on as the method being tested here will be removed
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			string loginName = "StaffName";
			string password = "boo";

			using (var testConnection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(testConnection, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			{
				//CREATE STAFF
				var factory = new BusinessObjectFactory(testConnection);
				GlbStaff staff = factory.New<GlbStaff>();
				staff.GS_LoginName = loginName;
				staff.StaffPlainTextPassword = password;
				staff.GS_IsActive = true;
				staff.IsDatabaseDeveloper = true;
				staff.IsReadOnlyDBUser = true;

				new DbUserManager().SetPasswordForStaff(staff, password);
				factory.Save();

				Assert(loginName + " should exist.", DbUserManagerForTesting.CheckLoginExists(loginName, testConnection));

				//LOG IN USING STAFF CREDENTIALS
				using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, "EnterpriseDbUser_" + Db.DatabaseName + "_" + loginName, password))
				{
					connection.EnsureIsOpen();
				}

				//SYNCHRONISE
				new DbUserManager().EnsureApplicationLoginForBiDatabase(Db.ServerName, Db.AuditDatabaseName);
				//Assert(loginName + " should exist.", DbUserManagerForTesting.CheckLoginExists(loginName, testConnection));

				//LOG IN AGAIN USING STAFF CREDENTIALS
				using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, "EnterpriseDbUser_" + Db.DatabaseName + "_" + loginName, password))
				{
					AssertNoExceptionThrown($"EnterpriseDbUser_{Db.DatabaseName}_{loginName} should still be able to log in.", () =>
					{
						connection.EnsureIsOpen();
					});
				}
			}
		}
	}
}
