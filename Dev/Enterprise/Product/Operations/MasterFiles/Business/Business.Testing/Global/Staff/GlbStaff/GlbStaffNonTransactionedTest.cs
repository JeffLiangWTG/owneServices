using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class GlbStaffNonTransactionedTest : TestCase
	{
		public void TestCreateLogsWhenPremissionFlagsAreChanged()
		{
			var factory = new BusinessObjectFactory();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_IsSalesRep = false;
				staff.GS_IsController = false;
				staff.GS_IsOperational = false;
				staff.IsDatabaseDeveloper = false;
				staff.IsReadOnlyDBUser = false;
				staff.IsBackupOperator = false;

				var staff1 = factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_IsController = true; //Otherwise it wouldnt allow you to modify GS_IsOperational of staff
				staff1.GS_IsOperational = false; //Otherwise it wouldnt allow you to modify GS_IsOperational of staff
				factory.Save();

				AssertEquals(0, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				var assertedLogsPK = new List<ZGuid>();
				StmALog lastCreatedLog;

				staff.GS_IsSalesRep = true;
				factory.Save();
				AssertEquals(1, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Sales Rep: Y", lastCreatedLog.SL_Reference);

				staff.GS_IsSalesRep = false;
				factory.Save();
				AssertEquals(2, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Sales Rep: N", lastCreatedLog.SL_Reference);

				staff.GS_IsController = true;
				factory.Save();
				AssertEquals(3, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Controller: Y", lastCreatedLog.SL_Reference);
				staff.GS_IsController = false;
				factory.Save();
				AssertEquals(4, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Controller: N", lastCreatedLog.SL_Reference);

				staff.GS_IsOperational = true;
				factory.Save();
				AssertEquals(5, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Operational: Y", lastCreatedLog.SL_Reference);
				staff.GS_IsOperational = false;
				factory.Save();
				AssertEquals(6, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Operational: N", lastCreatedLog.SL_Reference);

				staff.IsDatabaseDeveloper = true;
				factory.Save();
				AssertEquals(7, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Database Developer: Y", lastCreatedLog.SL_Reference);
				staff.IsDatabaseDeveloper = false;
				factory.Save();
				AssertEquals(8, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Database Developer: N", lastCreatedLog.SL_Reference);

				staff.IsReadOnlyDBUser = true;
				factory.Save();
				AssertEquals(9, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Database Reader: Y", lastCreatedLog.SL_Reference);
				staff.IsReadOnlyDBUser = false;
				factory.Save();
				AssertEquals(10, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Database Reader: N", lastCreatedLog.SL_Reference);

				staff.IsBackupOperator = true;
				factory.Save();
				AssertEquals(11, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Backup Operator: Y", lastCreatedLog.SL_Reference);
				staff.IsBackupOperator = false;
				factory.Save();
				AssertEquals(12, staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).Count());
				lastCreatedLog = LastCreatedLog(staff, assertedLogsPK);
				AssertEquals("EDT - Is Backup Operator: N", lastCreatedLog.SL_Reference);
			}
		}

		public void TestApplyStaffChangesToDbLogins()
		{
			// LLZ: This test tests that database roles are applied correctly (added or removed)
			// It also tests that if there is a non-empty schema that belongs to an sql user corresponding to staff member
			// then this staff member cannot be dropped. 
			// When UseModernSqlSecuritySystem is true, adding/removing roles testing is replaced with a number of other tests inside DbSecurityAdminTaskSecurityTest
			// Removing schemas is no longer supported even if it is empty so this test is replaced with a new validation test TestAttemptToDropStaffThatOwnsEmptySchemaResultsInValidationError
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_IsOperational = false;
			staff1.IsBackupOperator = true;
			staff1.IsReadOnlyDBUser = false;
			staff1.GS_LoginName = StaffLogin1;
			staff1.GS_Code = "BAS";
			var sgStaffWrapper = staff1.GetSGWrapper();
			var pwd1 = sgStaffWrapper.Tradenetv4Password;
			pwd1.GP_MailBoxID = "Mail1";
			pwd1.GP_UserID = "Bob1";
			var pwd2 = sgStaffWrapper.SGNationalTradePlatformPassword;
			pwd2.GP_MailBoxID = "Mail2";
			pwd2.GP_UserID = "Bob2";
			var nzStaffWrapper = staff1.GetNZWrapper();
			var pwd3 = nzStaffWrapper.NZBPassword;
			pwd3.GP_UserID = "Bob3";
			var pwd4 = sgStaffWrapper.AccessPassword;
			pwd4.GP_UserID = "Bob4";
			var pwd5 = staff1.GetAUWrapper().NUTPassword;
			pwd5.GP_UserID = "Bob5";

			factory.Save();

			Assert("Staff1 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
			Assert("Staff1 should have [backupoperator] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));

			var staff2 = factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = StaffLogin2;
			staff2.GS_IsOperational = false;
			staff2.IsReadOnlyDBUser = true;
			staff1.IsBackupOperator = false;
			staff1.IsReadOnlyDBUser = true;

			factory.Save();

			Assert("Staff1 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
			Assert("Staff1 should NOT have [backupoperator] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));

			Assert("Staff2 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin2, "db_backupoperator"));
			Assert("Staff2 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin2, DbRoleTypes.CwRestrictedReaderRole));
			Assert("Staff2 should NOT have [backupoperator] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin2, "db_backupoperator"));
			Assert("Staff2 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin2, DbRoleTypes.CwRestrictedReaderRole));

			using (var adminConnection = Db.NewAdminConnection())
			{
				// Disable Staff1 and assert login was removed.
				staff1.GS_IsActive = false;
				factory.Save();
				AssertEquals("Staff1 database login exists?", false, DbUserManagerForTesting.CheckLoginExists(StaffLogin1, adminConnection, readUncommitted: true));

				// Re-enable Staff1 and assert login will not be created
				staff1.GS_IsActive = true;
				factory.Save();
				AssertEquals("Staff1 database login exists?", false, DbUserManagerForTesting.CheckLoginExists(StaffLogin1, adminConnection, readUncommitted: true));

				// Recreate a staff db login
				staff1.IsReadOnlyDBUser = true;
				factory.Save();
				AssertEquals("Staff1 database login exists?", true, DbUserManagerForTesting.CheckLoginExists(StaffLogin1, adminConnection, readUncommitted: true));
				Assert("Staff1 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
				Assert("Staff1 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
				Assert("Staff1 should NOT have [backupoperator] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
				Assert("Staff1 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));

				// Delete Staff1 and assert login was removed.
				staff1.Delete();
				AssertEquals("Staff1 database login exists?", false, DbUserManagerForTesting.CheckLoginExists(StaffLogin1, adminConnection, readUncommitted: true));
				Assert("Staff1 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
				Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
				Assert("Staff1 should NOT have [backupoperator] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
				Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
				AssertEquals("Staff1 external passwords should be deleted", 0, factory.Load<GlbExternalPassword>(new ZQuery(GlbExternalPasswordSchema.GP_GS, staff1.PK)).Length);

				DbUserManagerForTesting.CreateUserSchemaOnMainDb(StaffLogin2, adminConnection);
				DbUserManagerForTesting.CreateDummyTableForSchemaOnMainDb(StaffLogin2, adminConnection);
			}

			Exception caught = null;
			try
			{
				staff2.Delete();
			}
			catch (Exception ex)
			{
				caught = ex;
			}

			ZCannotSaveException saveException = caught as ZCannotSaveException;
			AssertNotNull("expect ZCannotSaveException", saveException);
#if NET
			AssertStartsWith("Exception message", "Failed to synchronize logins, please contact your System Administrator and report the following error:\r\n\r\nMicrosoft.Data.SqlClient.SqlException (0x80131904): Cannot drop schema '(((login2)))' because it is being referenced by object 'T1'.",
				saveException.Message);
#else
			AssertStartsWith("Exception message", "Failed to synchronize logins, please contact your System Administrator and report the following error:\r\n\r\nSystem.Data.SqlClient.SqlException (0x80131904): Cannot drop schema '(((login2)))' because it is being referenced by object 'T1'.",
				saveException.Message);
#endif
			AssertEquals("Login Synchronization failed", saveException.Heading);

			// Cleanup logins
			factory.Save();
			DbUserManagerForTesting.DropDummyTableForSchemaOnMainDb(StaffLogin2, Db.Connection);
			using (var connection = Db.NewAdminConnection())
			{
				new DbUserManager().SynchroniseAllStaffAndDbLoginsForAllDatabases(connection);
			}
		}

		public void TestApplyStaffChangesToDbSpecificDbLogins()
		{
			// LLZ: This test tests that database roles are applied correctly (added or removed)
			// When UseModernSqlSecuritySystem is true, adding/removing role membership testing is replaced with a number of other tests inside DbSecurityAdminTaskSecurityTest
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			staff1.IsBackupOperator = true;
			staff1.IsReadOnlyDBUser = false;
			staff1.GS_LoginName = StaffLogin1;
			staff1.GS_Code = "BAS";

			factory.Save();

			Assert("Staff1 should have [backupoperator] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
			Assert("Staff1 should have [backupoperator] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should NOT have [cwRestrictedReaderRole] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));

			staff1.IsBackupOperator = false;
			staff1.IsReadOnlyDBUser = true;

			factory.Save();

			Assert("Staff1 should NOT have [backupoperator] rights on main DB", !CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should have [cwRestrictedReaderRole] rights on main DB", CheckUserHasRightsOnDb(Db.DatabaseName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));
			Assert("Staff1 should NOT have [backupoperator] rights on SD001 DB", !CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, "db_backupoperator"));
			Assert("Staff1 should have [cwRestrictedReaderRole] rights on SD001 DB", CheckUserHasRightsOnDb(StorageDbName, StaffLogin1, DbRoleTypes.CwRestrictedReaderRole));

			staff1.Delete();
		}

		public void TestChangeLoginNameCharacterCasingAlsoChangesDbLogin()
		{
			// LLZ: When UseModernSqlSecuritySystem is true, this test is replaced with TestChangingGlbStaffLoginNameChangesItsSqlLoginNameCase inside DbSecurityAdminTaskSecurityTest
			// Once case sensitivity is addressed in synchroniser
			// Once UseModernSqlSecuritySystem flag is removed, this test should be removed as well
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var factory = new BusinessObjectFactory();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var staff = factory.NewWithValidTestData<GlbStaff>();

				try
				{
					staff.IsReadOnlyDBUser = true;
					staff.GS_LoginName = "(-bob-)";
					staff.GS_Code = "~!@";

					AssertEquals("[PRE-CONDITION] Staff1 login exists?", false, DbUserManagerForTesting.CheckLoginExists(staff.GS_LoginName, adminConnection, readUncommitted: true));

					factory.Save();

					AssertEquals("Staff1 login created?", true, DbUserManagerForTesting.CheckLoginExists(staff.GS_LoginName, adminConnection, readUncommitted: true));
					AssertEquals("Staff1 has [cwRestrictedReaderRole] rights on main DB?", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, adminConnection));

					staff.GS_LoginName = "(-Bob-)";

					factory.Save();

					AssertEquals("Staff1 login exists?", true, DbUserManagerForTesting.CheckLoginExists(staff.GS_LoginName, adminConnection, readUncommitted: true));
					AssertEquals("Staff1 has [cwRestrictedReaderRole] rights on main DB?", true, DbUserManagerForTesting.CheckUserHasRightsOnDb(Db.DatabaseName, staff.GS_LoginName, DbRoleTypes.CwRestrictedReaderRole, adminConnection));
				}
				finally
				{
					staff.Delete();
				}
			}
		}

		public void TestDeactivationShouldDetachFixedDatabaseAccessGroups()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				// create staff and save it in one factory
				var factory = new BusinessObjectFactory();
				var staff = factory.NewWithValidTestData<GlbStaff>();
				staff.IsDatabaseDeveloper = true;
				staff.IsBackupOperator = true;
				staff.IsReadOnlyDBUser = true;
				staff.GS_LoginName = StaffLogin1;
				staff.GS_Code = "T01";
				factory.Save();

				// load the staff in another factory
				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				var staffInOtherFactory = otherFactory.Load<GlbStaff>(staff.PK);

				AssertEquals(4, staffInOtherFactory.Groups.Count);
				AssertEquals(3, staffInOtherFactory.DatabaseAccessGroupRoles.Count);
				AssertContainsExactElementsInAnyOrder(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole },
					staffInOtherFactory.DatabaseAccessGroupRoles.ToArray());

				staffInOtherFactory.GS_IsActive = false;
				otherFactory.Save();

				// load the staff in a third factory
				var otherFactory2 = new BusinessObjectFactory();
				otherFactory2.RefreshEnabled = false;
				var staffInOtherFactory2 = otherFactory2.Load<GlbStaff>(staff.PK);

				AssertEquals(0, staffInOtherFactory2.Groups.Count);
				AssertEquals(0, staffInOtherFactory2.DatabaseAccessGroupRoles.Count);
			}
		}

		public void TestDeactivationShouldDetachFlexibleDatabaseAccessGroups()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				// create staff and save it in one factory
				var factory = new BusinessObjectFactory();
				var staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = StaffLogin1;
				staff.GS_Code = "T01";

				var hrmStaffGroup = factory.NewWithValidTestData<GlbGroup>();
				var hrmStaffGroupRole = factory.NewWithValidTestData<GlbGroupRole>();
				hrmStaffGroupRole.GGR_GG_Group = hrmStaffGroup.PK;
				hrmStaffGroupRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;
				hrmStaffGroup.Staff.Add(staff);

				factory.Save();

				// load the staff in another factory
				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				var staffInOtherFactory = otherFactory.Load<GlbStaff>(staff.PK);

				AssertEquals(2, staffInOtherFactory.Groups.Count);
				AssertEquals(1, staffInOtherFactory.DatabaseAccessGroupRoles.Count);
				AssertEquals(DbRoleTypes.CwHRMStaffRole, staffInOtherFactory.DatabaseAccessGroupRoles.ElementAt(0));

				staffInOtherFactory.GS_IsActive = false;
				otherFactory.Save();

				// load the staff in a third factory
				var otherFactory2 = new BusinessObjectFactory();
				otherFactory2.RefreshEnabled = false;
				var staffInOtherFactory2 = otherFactory2.Load<GlbStaff>(staff.PK);

				AssertEquals(0, staffInOtherFactory2.Groups.Count);
				AssertEquals(0, staffInOtherFactory2.DatabaseAccessGroupRoles.Count);
			}
		}

		#region Implementation

		const string StaffLogin1 = "(((login)))";
		const string StaffLogin2 = "(((login2)))";
		readonly string StorageDbName = Db.DatabaseName + "_SD001";

		StmALog LastCreatedLog(GlbStaff staff, List<ZGuid> assertedLogsPK)
		{
			var log = staff.Logs.Find(l => l.SL_SE_NKEvent == Events.SecurityModifiedCode).First(g => !assertedLogsPK.Contains(g.PK));
			assertedLogsPK.Add(log.PK);
			return log;
		}

		bool CheckUserHasRightsOnDb(string dbName, string staffLoginName, string userRole)
		{
			return DbUserManagerForTesting.CheckUserHasRightsOnDb(dbName, staffLoginName, userRole, Db.Connection);
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, StorageDbName);
			}
		}

		#endregion // Implementation
	}
}
