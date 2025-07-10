using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using CargoWise.DataProtection.TestFramework;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

[assembly: UsesConstants(typeof(TestConstants))]
namespace Enterprise.MasterFiles.Business
{
	public class DbSecurityServerLevelTest : TransactionedTestCase
	{
		#region EFFECTIVE DB SECURITY MODE

		public void TestGetDisallowedServerRoleMembers()
		{
			var testSecurity = new DbSecurityForTest();

			testSecurity.CleanUpServerRoles_Exposed(TestAdminConnection);
			AssertEquals("Disallowed role member count", 0, testSecurity.GetDisallowedServerRoleMembers_Exposed(TestAdminConnection).Count);

			var sqlText = @"
				CREATE LOGIN [##sql_login_TestAreThereDisallowedServerRoleMembers##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				CREATE LOGIN [AnyDb_SomeLogin] WITH PASSWORD = '', CHECK_POLICY = OFF;
				ALTER SERVER ROLE sysadmin ADD MEMBER [##sql_login_TestAreThereDisallowedServerRoleMembers##];";
			TestAdminConnection.ExecuteNonQuery(sqlText);

			var disallowedRoleMembers = testSecurity.GetDisallowedServerRoleMembers_Exposed(TestAdminConnection);
			AssertEquals("Disallowed role member count", 1, disallowedRoleMembers.Count);
			AssertEquals("Disallowed role member - role", "sysadmin", disallowedRoleMembers[0].RoleName);
			AssertEquals("Disallowed role member - login", "##sql_login_TestAreThereDisallowedServerRoleMembers##", disallowedRoleMembers[0].LoginName);
			AssertEquals("Disallowed role member - login type", "SQL_LOGIN", disallowedRoleMembers[0].LoginType);

			sqlText = "ALTER SERVER ROLE dbcreator ADD MEMBER [##sql_login_TestAreThereDisallowedServerRoleMembers##];";
			TestAdminConnection.ExecuteNonQuery(sqlText);

			disallowedRoleMembers = testSecurity.GetDisallowedServerRoleMembers_Exposed(TestAdminConnection);
			AssertEquals("Disallowed role member count", 2, disallowedRoleMembers.Count);

			if (disallowedRoleMembers[0].RoleName == "sysadmin")
			{
				AssertEquals("Disallowed role member [1]", "dbcreator", disallowedRoleMembers[1].RoleName);
			}
			else
			{
				AssertEquals("Disallowed role [0]", "dbcreator", disallowedRoleMembers[0].RoleName);
				AssertEquals("Disallowed role [1]", "sysadmin", disallowedRoleMembers[1].RoleName);
			}

			disallowedRoleMembers = testSecurity.GetDisallowedServerRoleMembers_Exposed(TestAdminConnection, allowDbCreator: true);
			AssertEquals("Disallowed role member count (dbcreator allowed)", 1, disallowedRoleMembers.Count);
			AssertEquals("Disallowed role member - role", "sysadmin", disallowedRoleMembers[0].RoleName);
		}

		public void TestIsDatabaseSecurityOpen()
		{
			var testSecurity = new DbSecurityWithFakeSaPasswordForTesting();
			AssertEquals("IsDatabaseSecurityOpen?", true, testSecurity.IsDatabaseSecurityOpen());
		}

		class DbSecurityWithFakeSaPasswordForTesting : DbSecurityLockDown
		{
			protected override string GetSaPwd()
			{
				return "~SOME_INVALID_SA_PASSWORD~" + Guid.NewGuid().ToString();
			}
		}

		#endregion

		#region SERVER LEVEL DDL TRIGGERS

		/// <summary>
		/// Disables server level DDL triggers
		/// Allowed DDL triggers:
		///  - None at the moment. We don't ship any DDL triggers. Exclude them in code and add them here when we do.
		/// </summary>
		public void TestDisableServerDdlTriggers()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.DisableServerDdlTriggers_Exposed(TestAdminConnection);

			var sqlText = @"
				EXEC (N'CREATE TRIGGER [SneakyDropLoginTrigger] ON ALL SERVER FOR DROP_LOGIN AS ROLLBACK');
				EXEC (N'CREATE TRIGGER [SneakyDropTriggerTrigger01] ON ALL SERVER FOR DROP_TRIGGER AS ROLLBACK');
				EXEC (N'CREATE TRIGGER [SneakyDropTriggerTrigger02] ON ALL SERVER FOR DROP_TRIGGER AS ROLLBACK');
				";
			TestAdminConnection.ExecuteNonQuery(sqlText);
			AssertServerDdlTrigger("SneakyDropLoginTrigger", true);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger01", true);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger02", true);

			var outputLog = testSecurity.DisableServerDdlTriggers_Exposed(TestAdminConnection);

			DbSecurityTest.AssertOutputLog(outputLog,
				"DISABLED [SneakyDropLoginTrigger] DDL SQL_TRIGGER ON ALL SERVER\r\n",
				"DISABLED [SneakyDropTriggerTrigger01] DDL SQL_TRIGGER ON ALL SERVER\r\n",
				"DISABLED [SneakyDropTriggerTrigger02] DDL SQL_TRIGGER ON ALL SERVER\r\n");

			AssertServerDdlTrigger("SneakyDropLoginTrigger", false);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger01", false);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger02", false);
			AssertNoEnabledServerDdlTriggers();

			// Run again => no changes to be made
			outputLog = testSecurity.DisableServerDdlTriggers_Exposed(TestAdminConnection);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertServerDdlTrigger("SneakyDropLoginTrigger", false);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger01", false);
			AssertServerDdlTrigger("SneakyDropTriggerTrigger02", false);
			AssertNoEnabledServerDdlTriggers();
		}

		void AssertServerDdlTrigger(string ddlTriggerName, bool expectedEnabled)
		{
			var sqlText = string.Format(@"
				SELECT is_disabled
				FROM sys.server_triggers
				WHERE name = '{0}'",
				ddlTriggerName);
			var actualEnabled = !Convert.ToBoolean(TestAdminConnection.ExecuteScalar(sqlText));
			AssertEquals(string.Format("Is DDL trigger [{0}] enabled?", ddlTriggerName), expectedEnabled, actualEnabled);
		}

		void AssertNoEnabledServerDdlTriggers()
		{
			var sqlText = "SELECT count(*) FROM sys.server_triggers WHERE is_ms_shipped = 0 AND is_disabled = 0";
			var enabledTriggerCount = (int)TestAdminConnection.ExecuteScalar(sqlText);
			AssertEquals("Number of enabled server DDL triggers", 0, enabledTriggerCount);
		}

		#endregion

		#region SQL SERVER AGENT JOBS

		/// <summary>
		/// Disables SQL agent jobs
		/// Allowed Jobs:
		///  - None at the moment. We don't ship any SQL agent jobs. (Exclude them in code and add them here when we do.)
		/// </summary>
		public void TestDisableSqlAgentJobs()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.DisableSqlAgentJobs_Exposed(TestAdminConnection);

			var sqlText = @"
				EXEC (N'EXEC msdb.dbo.sp_add_job @job_name = N''~SneakySqlAgentJob~''');
				EXEC (N'EXEC msdb.dbo.sp_add_job @job_name = N''!DisabledSqlAgentJob!'', @enabled = 0');
				";
			TestAdminConnection.ExecuteNonQuery(sqlText);
			AssertSqlAgentJob("~SneakySqlAgentJob~", true);
			AssertSqlAgentJob("!DisabledSqlAgentJob!", false);

			var outputLog = testSecurity.DisableSqlAgentJobs_Exposed(TestAdminConnection);

			DbSecurityTest.AssertOutputLog(outputLog, "DISABLED SQL AGENT JOB [~SneakySqlAgentJob~]\r\n");

			AssertSqlAgentJob("~SneakySqlAgentJob~", false);
			AssertSqlAgentJob("!DisabledSqlAgentJob!", false);
			AssertNoEnabledSqlAgentJobs();

			// Run again => no changes to be made
			outputLog = testSecurity.DisableSqlAgentJobs_Exposed(TestAdminConnection);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertSqlAgentJob("~SneakySqlAgentJob~", false);
			AssertSqlAgentJob("!DisabledSqlAgentJob!", false);
			AssertNoEnabledSqlAgentJobs();
		}

		void AssertSqlAgentJob(string jobName, bool expectedEnabled)
		{
			var sqlText = string.Format(@"
				SELECT [enabled]
				FROM msdb.dbo.sysjobs
				WHERE name = '{0}'",
				jobName);
			var actualEnabled = Convert.ToBoolean(TestAdminConnection.ExecuteScalar(sqlText));
			AssertEquals(string.Format("Is SQL Agent job [{0}] enabled?", jobName), expectedEnabled, actualEnabled);
		}

		void AssertNoEnabledSqlAgentJobs()
		{
			var sqlText = "SELECT count(*) FROM msdb.dbo.sysjobs WHERE [enabled] = 1";
			var enabledCount = (int)TestAdminConnection.ExecuteScalar(sqlText);
			AssertEquals("Number of enabled SQL Agent jobs", 0, enabledCount);
		}

		#endregion

		#region SERVER LEVEL ROLES

		/// <summary>
		/// Revokes server role membership to logins
		/// Allowed role membership:
		///  - serveradmin, processadmin, diskadmin to all
		///  - ANY ROLE to sa and OdysseyAdmin
		/// Removes user created server roles.
		/// </summary>
		public void TestCleanUpServerRoles()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpServerRoles_Exposed(TestAdminConnection);

			var sqlAgentServiceAccount = testSecurity.GetSqlAgentServiceAccountName_Exposed(TestAdminConnection);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				CREATE SERVER ROLE [##server_role_CleanUpServerRoles##];
				CREATE LOGIN [##sql_login_CleanUpServerRoles##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				CREATE LOGIN [{1}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'CORP\DATBackground') CREATE LOGIN [CORP\DATBackground] FROM WINDOWS;
				ALTER SERVER ROLE dbcreator ADD MEMBER [##server_role_CleanUpServerRoles##];
				ALTER SERVER ROLE sysadmin ADD MEMBER [##sql_login_CleanUpServerRoles##];
				ALTER SERVER ROLE sysadmin ADD MEMBER [{0}];
				ALTER SERVER ROLE securityadmin ADD MEMBER [CORP\DATBackground];
				ALTER SERVER ROLE serveradmin ADD MEMBER [##server_role_CleanUpServerRoles##];
				ALTER SERVER ROLE processadmin ADD MEMBER [##sql_login_CleanUpServerRoles##];
				ALTER SERVER ROLE diskadmin ADD MEMBER [CORP\DATBackground];
				ALTER SERVER ROLE [##server_role_CleanUpServerRoles##] ADD MEMBER [##sql_login_CleanUpServerRoles##];
				ALTER SERVER ROLE [##server_role_CleanUpServerRoles##] ADD MEMBER [CORP\DATBackground];
				ALTER SERVER ROLE [##server_role_CleanUpServerRoles##] ADD MEMBER [{0}];
				ALTER SERVER ROLE [bulkadmin] ADD MEMBER [##sql_login_CleanUpServerRoles##];
				ALTER SERVER ROLE [bulkadmin] ADD MEMBER [{1}];
				ALTER SERVER ROLE [bulkadmin] ADD MEMBER [{2}];",
				sqlAgentServiceAccount,
				CargoWise.DataProtection.CargoWiseWriterLoginCredentials.UserNameFor("AnotherDb"),
				CargoWise.DataProtection.RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
			TestAdminConnection.ExecuteNonQuery(sqlText);

			var expectedAdminLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);

			AssertServerRole("sysadmin", true);
			AssertServerRoleMembership("sysadmin", "sa", true);
			AssertServerRoleMembership("sysadmin", expectedAdminLogin.UserName, true);
			AssertServerRoleMembership("sysadmin", sqlAgentServiceAccount, true);
			AssertServerRoleMembership("sysadmin", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRole("dbcreator", true);
			AssertServerRoleMembership("dbcreator", "##server_role_CleanUpServerRoles##", true);
			AssertServerRole("securityadmin", true);
			AssertServerRoleMembership("securityadmin", "CORP\\DATBackground", true);
			AssertServerRole("serveradmin", true);
			AssertServerRoleMembership("serveradmin", "##server_role_CleanUpServerRoles##", true);
			AssertServerRole("processadmin", true);
			AssertServerRoleMembership("processadmin", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRole("diskadmin", true);
			AssertServerRoleMembership("diskadmin", "CORP\\DATBackground", true);
			AssertServerRole("bulkadmin", true);
			AssertServerRoleMembership("bulkadmin", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.CargoWiseWriterLoginCredentials.UserNameFor("AnotherDb"), true);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertServerRole("##server_role_CleanUpServerRoles##", true);
			AssertServerRoleMembership("##server_role_CleanUpServerRoles##", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRoleMembership("##server_role_CleanUpServerRoles##", "CORP\\DATBackground", true);
			AssertServerRoleMembership("##server_role_CleanUpServerRoles##", sqlAgentServiceAccount, true);

			var outputLog = testSecurity.CleanUpServerRoles_Exposed(TestAdminConnection);

			DbSecurityTest.AssertOutputLog(outputLog,
				"REVOKED [sysadmin] FROM [##sql_login_CleanUpServerRoles##] SQL_LOGIN",
				"REVOKED [securityadmin] FROM [CORP\\DATBackground] WINDOWS_LOGIN",
				"REVOKED [dbcreator] FROM [##server_role_CleanUpServerRoles##] SERVER_ROLE",
				"REVOKED [bulkadmin] FROM [##sql_login_CleanUpServerRoles##] SQL_LOGIN",
				string.Format(CultureInfo.InvariantCulture, "REVOKED [bulkadmin] FROM [{0}] SQL_LOGIN", CargoWise.DataProtection.RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)),
				string.Format(CultureInfo.InvariantCulture, "REVOKED [bulkadmin] FROM [{0}] SQL_LOGIN", CargoWise.DataProtection.CargoWiseWriterLoginCredentials.UserNameFor("AnotherDb")),
				"REVOKED [##server_role_CleanUpServerRoles##] FROM [##sql_login_CleanUpServerRoles##] SQL_LOGIN",
				"REVOKED [##server_role_CleanUpServerRoles##] FROM [CORP\\DATBackground] WINDOWS_LOGIN",
				"REMOVED [##server_role_CleanUpServerRoles##] SERVER_ROLE");

			AssertServerRole("sysadmin", true);
			AssertServerRoleMembership("sysadmin", "sa", true);
			AssertServerRoleMembership("sysadmin", expectedAdminLogin.UserName, true);
			AssertServerRoleMembership("sysadmin", sqlAgentServiceAccount, true);
			AssertServerRoleMembership("sysadmin", "##sql_login_CleanUpServerRoles##", false);
			AssertServerRole("dbcreator", true);
			AssertServerRoleMembership("dbcreator", "##server_role_CleanUpServerRoles##", false);
			AssertServerRole("securityadmin", true);
			AssertServerRoleMembership("securityadmin", "CORP\\DATBackground", false);
			AssertServerRole("serveradmin", true);
			AssertServerRoleMembership("serveradmin", "##server_role_CleanUpServerRoles##", false);
			AssertServerRole("processadmin", true);
			AssertServerRoleMembership("processadmin", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRole("diskadmin", true);
			AssertServerRoleMembership("diskadmin", "CORP\\DATBackground", true);
			AssertServerRole("bulkadmin", true);
			AssertServerRoleMembership("bulkadmin", "##sql_login_CleanUpServerRoles##", false);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.CargoWiseWriterLoginCredentials.UserNameFor("AnotherDb"), false);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), false);
			AssertServerRole("##server_role_CleanUpServerRoles##", false);

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpServerRoles_Exposed(TestAdminConnection);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertServerRole("sysadmin", true);
			AssertServerRoleMembership("sysadmin", "sa", true);
			AssertServerRoleMembership("sysadmin", expectedAdminLogin.UserName, true);
			AssertServerRoleMembership("sysadmin", sqlAgentServiceAccount, true);
			AssertServerRoleMembership("sysadmin", "##sql_login_CleanUpServerRoles##", false);
			AssertServerRole("dbcreator", true);
			AssertServerRoleMembership("dbcreator", "##server_role_CleanUpServerRoles##", false);
			AssertServerRole("securityadmin", true);
			AssertServerRoleMembership("securityadmin", "CORP\\DATBackground", false);
			AssertServerRole("serveradmin", true);
			AssertServerRoleMembership("serveradmin", "##server_role_CleanUpServerRoles##", false);
			AssertServerRole("processadmin", true);
			AssertServerRoleMembership("processadmin", "##sql_login_CleanUpServerRoles##", true);
			AssertServerRole("diskadmin", true);
			AssertServerRoleMembership("diskadmin", "CORP\\DATBackground", true);
			AssertServerRole("bulkadmin", true);
			AssertServerRoleMembership("bulkadmin", "##sql_login_CleanUpServerRoles##", false);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.CargoWiseWriterLoginCredentials.UserNameFor("AnotherDb"), false);
			AssertServerRoleMembership("bulkadmin", CargoWise.DataProtection.RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), false);
			AssertServerRole("##server_role_CleanUpServerRoles##", false);
		}

		void AssertServerRoleMembership(string roleName, string loginName, bool expectedIsMember)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM sys.server_role_members srm
				INNER JOIN sys.server_principals sr ON sr.principal_id = srm.role_principal_id
				INNER JOIN sys.server_principals sl ON sl.principal_id = srm.member_principal_id
				WHERE sr.name = '{0}'
				AND sl.name = '{1}'",
				roleName, loginName);
			var actualIsMember = ((int)TestAdminConnection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("Is login [{0}] member of role [{1}]?", loginName, roleName), expectedIsMember, actualIsMember);
		}

		void AssertServerRole(string roleName, bool expectedExists)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (SELECT 1 FROM sys.server_principals sr WHERE sr.name = '{0}' {1}) SELECT 1 ELSE SELECT 0",
				roleName,
				(expectedExists ? "AND sr.type = 'R'" : ""));
			var actualExists = Convert.ToBoolean(TestAdminConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "Role [{0}] exists?", roleName), expectedExists, actualExists);
		}

		#endregion

		#region SERVER LEVEL PERMISSIONS

		public void TestCleanUpAlwaysOnGroupPermissions()
		{
			var testSecurity = new DbSecurityForTestingAlwaysOnAgroupPermissions();
			var outputLog = testSecurity.CleanUpServerLevelPermissions_Exposed(TestAdminConnection);

			DbSecurityTest.AssertOutputLog(outputLog,
				"REVOKED [ALTER] FROM [EnterpriseDbUser_DbName_LoginName] SQL_LOGIN ON AVAILABILITY GROUP [GrpName2]",
				"REVOKED [ALTER] FROM [Domain\\User] WINDOWS_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [ALTER] FROM [Domain\\AnotherUserUser] WINDOWS_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [CONTROL] FROM [EnterpriseDbUser_DbName_LoginName] SQL_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [CONTROL] FROM [Domain\\User] WINDOWS_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [VIEW DEFINITION] FROM [Domain\\AnotherUserUser] WINDOWS_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [TAKE OWNERSHIP] FROM [Domain\\User] WINDOWS_LOGIN ON AVAILABILITY GROUP [GrpName1]",
				"REVOKED [VIEW ANY DATABASE] FROM [ASqlLoginName] SQL_LOGIN ON SERVER",
				"REVOKED [VIEW ANY DATABASE] FROM [Domain\\AnotherUserUser] WINDOWS_LOGIN ON SERVER",
				"REVOKED [ALTER] FROM [ASqlLoginName] SQL_LOGIN ON ENDPOINT [Hadr_endpoint]",
				"REVOKED [ALTER] FROM [Domain\\AnotherUserUser] WINDOWS_LOGIN ON ENDPOINT [Hadr_endpoint]"
				);

			DbSecurityTest.AssertOutputLog(testSecurity.ExecutedCommands.ToString(),
				"REVOKE ALTER ON AVAILABILITY GROUP::[GrpName2] FROM [EnterpriseDbUser_DbName_LoginName];",
				"REVOKE ALTER ON AVAILABILITY GROUP::[GrpName1] FROM [Domain\\User];",
				"REVOKE ALTER ON AVAILABILITY GROUP::[GrpName1] FROM [Domain\\AnotherUserUser];",
				"REVOKE CONTROL ON AVAILABILITY GROUP::[GrpName1] FROM [EnterpriseDbUser_DbName_LoginName];",
				"REVOKE CONTROL ON AVAILABILITY GROUP::[GrpName1] FROM [Domain\\User];",
				"REVOKE VIEW DEFINITION ON AVAILABILITY GROUP::[GrpName1] FROM [Domain\\AnotherUserUser];",
				"REVOKE TAKE OWNERSHIP ON AVAILABILITY GROUP::[GrpName1] FROM [Domain\\User];",
				"REVOKE VIEW ANY DATABASE FROM [ASqlLoginName];",
				"REVOKE VIEW ANY DATABASE FROM [Domain\\AnotherUserUser];",
				"REVOKE ALTER ON ENDPOINT::[Hadr_endpoint] FROM [ASqlLoginName];",
				"REVOKE ALTER ON ENDPOINT::[Hadr_endpoint] FROM [Domain\\AnotherUserUser];"
				);
		}

		public void TestCleanUpServerLevelPermissions()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpServerLevelPermissions_Exposed(TestAdminConnection);

			var sqlText = @"
				CREATE SERVER ROLE [##server_role_CleanUpServerLevelPermissions##];
				CREATE LOGIN [##sql_login_CleanUpServerLevelPermissions##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'CORP\DATBackground') CREATE LOGIN [CORP\DATBackground] FROM WINDOWS;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'NT AUTHORITY\SYSTEM') CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;
				CREATE ASYMMETRIC KEY [**asymmetric_key__CleanUpServerLevelPermissions**] WITH ALGORITHM = RSA_2048 ENCRYPTION BY PASSWORD = 'Qw3rT!u1(0)Qw3rT!u1(0)';
				CREATE LOGIN [**login_for__asymmetric_key__CleanUpServerLevelPermissions**] FROM ASYMMETRIC KEY [**asymmetric_key__CleanUpServerLevelPermissions**];
				GRANT CONTROL SERVER TO [##server_role_CleanUpServerLevelPermissions##];
				GRANT ALTER ANY AVAILABILITY GROUP TO [##sql_login_CleanUpServerLevelPermissions##];
				GRANT ALTER ANY AVAILABILITY GROUP TO [NT AUTHORITY\SYSTEM];
				GRANT IMPERSONATE ON LOGIN::OdysseyAdmin TO [##sql_login_CleanUpServerLevelPermissions##];
				GRANT ALTER TRACE TO [##sql_login_CleanUpServerLevelPermissions##];
				GRANT ALTER ANY EVENT SESSION TO [##sql_login_CleanUpServerLevelPermissions##];
				GRANT UNSAFE ASSEMBLY TO [##sql_login_CleanUpServerLevelPermissions##];
				DENY CONTROL SERVER TO [##sql_login_CleanUpServerLevelPermissions##];
				DENY VIEW ANY DATABASE TO [##sql_login_CleanUpServerLevelPermissions##];
				GRANT CONNECT SQL TO [CORP\DATBackground];
				GRANT CONTROL SERVER TO [CORP\DATBackground] WITH GRANT OPTION;
				GRANT UNSAFE ASSEMBLY TO [**login_for__asymmetric_key__CleanUpServerLevelPermissions**];
				GRANT CONTROL SERVER TO [**login_for__asymmetric_key__CleanUpServerLevelPermissions**];
				";

			using (((ICurrentDbControl)TestAdminConnection).UseDatabase(Db.SqlMasterDb))
			{
				TestAdminConnection.ExecuteNonQuery(sqlText);
			}

			AssertServerPermission("CONTROL SERVER", "##server_role_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "NT AUTHORITY\\SYSTEM", "GRANT");
			AssertServerPermission("IMPERSONATE", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER TRACE", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER ANY EVENT SESSION", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("UNSAFE ASSEMBLY", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("CONTROL SERVER", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("VIEW ANY DATABASE", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("CONNECT SQL", "CORP\\DATBackground", "GRANT");
			AssertServerPermission("CONTROL SERVER", "CORP\\DATBackground", "GRANT_WITH_GRANT_OPTION");
			AssertServerPermission("UNSAFE ASSEMBLY", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "GRANT");
			AssertServerPermission("CONTROL SERVER", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "GRANT");

			var outputLog = testSecurity.CleanUpServerLevelPermissions_Exposed(TestAdminConnection);

			DbSecurityTest.AssertOutputLog(outputLog,
				"REVOKED [CONTROL SERVER] FROM [##server_role_CleanUpServerLevelPermissions##] SERVER_ROLE ON SERVER\r\n",
				"REVOKED [CONTROL SERVER] FROM [CORP\\DATBackground] WINDOWS_LOGIN ON SERVER\r\n",
				"REVOKED [ALTER ANY AVAILABILITY GROUP] FROM [##sql_login_CleanUpServerLevelPermissions##] SQL_LOGIN ON SERVER\r\n",
				"REVOKED [UNSAFE ASSEMBLY] FROM [##sql_login_CleanUpServerLevelPermissions##] SQL_LOGIN ON SERVER\r\n",
				"REVOKED [IMPERSONATE] FROM [##sql_login_CleanUpServerLevelPermissions##] SQL_LOGIN ON LOGIN [OdysseyAdmin]\r\n",
				"REVOKED [CONTROL SERVER] FROM [**login_for__asymmetric_key__CleanUpServerLevelPermissions**] ASYMMETRIC_KEY_MAPPED_LOGIN ON SERVER\r\n");

			AssertServerPermission("CONTROL SERVER", "##server_role_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "NT AUTHORITY\\SYSTEM", "GRANT");
			AssertServerPermission("IMPERSONATE", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER TRACE", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER ANY EVENT SESSION", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("UNSAFE ASSEMBLY", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("CONTROL SERVER", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("VIEW ANY DATABASE", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("CONNECT SQL", "CORP\\DATBackground", "GRANT");
			AssertServerPermission("CONTROL SERVER", "CORP\\DATBackground", "REVOKE");
			AssertServerPermission("UNSAFE ASSEMBLY", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "GRANT");
			AssertServerPermission("CONTROL SERVER", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "REVOKE");

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpServerLevelPermissions_Exposed(TestAdminConnection);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertServerPermission("CONTROL SERVER", "##server_role_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER ANY AVAILABILITY GROUP", "NT AUTHORITY\\SYSTEM", "GRANT");
			AssertServerPermission("IMPERSONATE", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("ALTER TRACE", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("ALTER ANY EVENT SESSION", "##sql_login_CleanUpServerLevelPermissions##", "GRANT");
			AssertServerPermission("UNSAFE ASSEMBLY", "##sql_login_CleanUpServerLevelPermissions##", "REVOKE");
			AssertServerPermission("CONTROL SERVER", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("VIEW ANY DATABASE", "##sql_login_CleanUpServerLevelPermissions##", "DENY");
			AssertServerPermission("CONNECT SQL", "CORP\\DATBackground", "GRANT");
			AssertServerPermission("CONTROL SERVER", "CORP\\DATBackground", "REVOKE");
			AssertServerPermission("UNSAFE ASSEMBLY", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "GRANT");
			AssertServerPermission("CONTROL SERVER", "**login_for__asymmetric_key__CleanUpServerLevelPermissions**", "REVOKE");
		}

		void AssertServerPermission(string permissionName, string loginName, string expectedPermissionState)
		{
			var sqlText = string.Format(@"
				SELECT sp.state_desc
				FROM sys.server_permissions sp
				INNER JOIN sys.server_principals sl ON sl.principal_id = sp.grantee_principal_id
				WHERE sp.permission_name = '{0}'
				AND sl.name = '{1}'",
				permissionName, loginName);
			var actualPermissionStateObj = TestAdminConnection.ExecuteScalar(sqlText);
			var actualPermissionState = (actualPermissionStateObj == null) ? "REVOKE" : actualPermissionStateObj.ToString();
			AssertEquals(string.Format("Login [{0}] permission [{1}] state:", loginName, permissionName), expectedPermissionState, actualPermissionState);
		}

		#endregion

		#region Implementation

		AdminConnection TestAdminConnection
		{
			get { return (AdminConnection)TestConnection; }
		}

		protected override DbConnection TestConnection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}
				return adminConnection;
			}
		}
		AdminConnection adminConnection;

		#endregion
	}

	public class DbSecurityLockDownTest : DbSecurityTest
	{
		#region DATABASE LEVEL DDL TRIGGERS

		/// <summary>
		/// Disables database level DDL triggers
		/// Allowed DDL triggers:
		///  - TG_AllowAlterCDCMetaObjects
		/// </summary>
		public void TestDisableDatabaseDdlTriggers()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.DisableDatabaseDdlTriggers_Exposed(testAdminConnection, TestDbList);

			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'CREATE TRIGGER [SneakyDropUserTrigger] ON DATABASE FOR DROP_USER AS ROLLBACK';
				EXEC [{0}]..sp_executesql N'CREATE TRIGGER [SneakyDropTriggerTrigger01] ON DATABASE FOR DROP_TRIGGER AS ROLLBACK';
				EXEC [{0}]..sp_executesql N'CREATE TRIGGER [SneakyDropTriggerTrigger02] ON DATABASE FOR DROP_TRIGGER AS ROLLBACK';
				EXEC [{1}]..sp_executesql N'CREATE TRIGGER [SneakyCreateTableTrigger] ON DATABASE FOR CREATE_TABLE AS ROLLBACK';
				",
				Db.DatabaseName, testRefDb);
			testAdminConnection.ExecuteNonQuery(sqlText);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropUserTrigger", true);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger01", true);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger02", true);
			AssertDatabaseDdlTrigger(testRefDb, "SneakyCreateTableTrigger", true);

			var outputLog = testSecurity.DisableDatabaseDdlTriggers_Exposed(testAdminConnection, TestDbList);

			AssertOutputLog(outputLog,
				string.Format("DISABLED [SneakyDropUserTrigger] DDL SQL_TRIGGER ON DATABASE [{0}]\r\n", Db.DatabaseName),
				string.Format("DISABLED [SneakyDropTriggerTrigger01] DDL SQL_TRIGGER ON DATABASE [{0}]\r\n", Db.DatabaseName),
				string.Format("DISABLED [SneakyDropTriggerTrigger02] DDL SQL_TRIGGER ON DATABASE [{0}]\r\n", Db.DatabaseName),
				string.Format("DISABLED [SneakyCreateTableTrigger] DDL SQL_TRIGGER ON DATABASE [{0}]\r\n", testRefDb));

			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropUserTrigger", false);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger01", false);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger02", false);
			AssertDatabaseDdlTrigger(testRefDb, "SneakyCreateTableTrigger", false);
			AssertNoEnabledDatabaseDdlTriggers(Db.DatabaseName);
			AssertNoEnabledDatabaseDdlTriggers(testRefDb);

			// Run again => no changes to be made
			outputLog = testSecurity.DisableDatabaseDdlTriggers_Exposed(testAdminConnection, TestDbList);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropUserTrigger", false);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger01", false);
			AssertDatabaseDdlTrigger(Db.DatabaseName, "SneakyDropTriggerTrigger02", false);
			AssertDatabaseDdlTrigger(testRefDb, "SneakyCreateTableTrigger", false);
			AssertNoEnabledDatabaseDdlTriggers(Db.DatabaseName);
			AssertNoEnabledDatabaseDdlTriggers(testRefDb);
		}

		void AssertDatabaseDdlTrigger(string dbName, string ddlTriggerName, bool expectedEnabled)
		{
			var sqlText = string.Format(@"
				SELECT is_disabled
				FROM [{0}].sys.triggers
				WHERE name = '{1}'",
				dbName, ddlTriggerName);
			var actualEnabled = !Convert.ToBoolean(testAdminConnection.ExecuteScalar(sqlText));
			AssertEquals(string.Format("Is DDL trigger [{0}] on DB [{1}] enabled?", ddlTriggerName, dbName), expectedEnabled, actualEnabled);
		}

		void AssertNoEnabledDatabaseDdlTriggers(string dbName)
		{
			var excludedTriggers = new[] { "TG_AllowAlterCDCMetaObjects" };

			var sqlText = string.Format(
				"SELECT count(*) FROM [{0}].sys.triggers WHERE parent_class = 0 AND is_ms_shipped = 0 AND is_disabled = 0 and name NOT IN ('{1}')",
				dbName, string.Join("', '", excludedTriggers));
			var enabledTriggerCount = (int)testAdminConnection.ExecuteScalar(sqlText);
			AssertEquals("Number of enabled DDL triggers", 0, enabledTriggerCount);
		}

		[UseSnapshotProtection]
		public void TestDisableDatabaseDdlTriggersExclusionList()
		{
			AssertDatabaseDdlTrigger(Db.DatabaseName, "TG_AllowAlterCDCMetaObjects", expectedEnabled: true);

			var testSecurity = new DbSecurityForTest();
			testSecurity.DisableDatabaseDdlTriggers_Exposed(testAdminConnection, TestDbList);

			AssertDatabaseDdlTrigger(Db.DatabaseName, "TG_AllowAlterCDCMetaObjects", expectedEnabled: true);
		}

		#endregion

		#region DATABASE USERS

		public void TestCleanUpDatabaseUsers_OnSharedDatabase()
		{
			var testSecurity = new DbSecurityForTest { IsSharedDatabase_Override = true };
			testSecurity.CleanUpDatabaseUsers_Exposed(testAdminConnection, TestDbList);

			var staffDbLogin = string.Format("{0}_xxx", DbUserRepository.StaffDbLoginPrefix);
			var randomUserLogin = "RandomUser";

			var otherDBReaderLogin = RestrictedReaderLoginCredentials.UserNameFor("OtherDatabase");
			var otherDBRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor("OtherDatabase");
			var otherDBRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor("OtherDatabase");
			var otherDBUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor("OtherDatabase");

			var sqlText = string.Format(@"
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '##sql_login_CleanUpDatabaseUsers##') CREATE LOGIN [##sql_login_CleanUpDatabaseUsers##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'CORP\DATBackground') CREATE LOGIN [CORP\DATBackground] FROM WINDOWS;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{1}') CREATE LOGIN [{1}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{2}') CREATE LOGIN [{2}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{3}') CREATE LOGIN [{3}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{4}') CREATE LOGIN [{4}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{5}') CREATE LOGIN [{5}] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '{6}') CREATE LOGIN [{6}] WITH PASSWORD = '', CHECK_POLICY = OFF;

				EXEC [{0}]..sp_executesql N'
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''##sql_login_CleanUpDatabaseUsers##'') CREATE USER [##sql_login_CleanUpDatabaseUsers##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					CREATE ROLE [##CleanUpDatabaseUsersRole##] AUTHORIZATION [##sql_login_CleanUpDatabaseUsers##];
					CREATE APPLICATION ROLE [##CleanUpDatabaseUsersAppRole##] WITH PASSWORD = ''This-Has*Got&To^Be%A$Long#Enough@Password!'';
				';

				EXEC [{0}]..sp_executesql N'
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{1}'') CREATE USER [{1}];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{2}'') CREATE USER [{2}];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{3}'') CREATE USER [{3}];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{4}'') CREATE USER [{4}];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{5}'') CREATE USER [{5}];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''{6}'') CREATE USER [{6}];
				';
				",
				Db.DatabaseName,
				otherDBReaderLogin,
				staffDbLogin,
				randomUserLogin,
				otherDBRestrictedReaderLogin,
				otherDBRestrictedWriterLogin,
				otherDBUnrestrictedWriterLogin
			);

			CombineAssertions("Initial conditions", () =>
			{
				testAdminConnection.ExecuteNonQuery(sqlText);

				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "CORP\\DATBackground", true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBReaderLogin, true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, staffDbLogin, true);

				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, randomUserLogin, true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##sql_login_CleanUpDatabaseUsers##", true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRole##", true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRole##", true);

				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBRestrictedReaderLogin, true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBRestrictedWriterLogin, true);
				AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBUnrestrictedWriterLogin, true);
			});

			CombineAssertions(@"GIVEN shared-database, WHEN execute CleanUpDatabaseUsers once or twice, THEN only the following database principals should not be deleted:
Windows User/Widows Group
%[_]DbReaderLoginSuffix (CargoWiseReaderLogin)
%[_]RestrictedReaderLoginSuffix (RestrictedReaderLogin)
%[_]RestrictedWriterLoginSuffix (RestrictedWriterLogin)
%[_]UnrestrictedWriterLoginSuffix (UnrestrictedWriterLogin)
StaffDbLoginPrefix[_]% (EnterpriseDbUser)", () =>
			{
				for (int i = 0; i < 2; i++)
				{
					testSecurity.CleanUpDatabaseUsers_Exposed(testAdminConnection, TestDbList);

					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "CORP\\DATBackground", true);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBReaderLogin, true);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, staffDbLogin, true);

					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, randomUserLogin, false);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##sql_login_CleanUpDatabaseUsers##", false);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRole##", false);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRole##", false);

					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBRestrictedReaderLogin, true);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBRestrictedWriterLogin, true);
					AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, otherDBUnrestrictedWriterLogin, true);
				}
			});
		}

		[UseSnapshotProtection(true)]
		public void TestCleanUpDatabaseUsers()
		{
			var adRegistryMock = new Mock<IADRegistry>();
			adRegistryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
			ObjectFactory.Substitute(adRegistryMock.Object);
			AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

			var adUser = new Mock<IADUser>();
			var adEntityProvider = new Mock<IADEntityProvider>();
			adEntityProvider.Setup(a => a.GetADUser(It.IsAny<GlbStaff>())).Returns(adUser.Object);
			adUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
			adUser.SetupGet(a => a.SAMAccountName).Throws(new CargoWise.ActiveDirectory.DirectoryServicesException("Could not locate or write to directory entry for entity imMissing"));
			ObjectFactory.Substitute(adEntityProvider.Object);

			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpDatabaseUsers_Exposed(testAdminConnection, TestDbList);

			var sqlText = string.Format(@"
				IF not exists (SELECT null FROM sys.server_principals WHERE name = '##sql_login_CleanUpDatabaseUsers##') CREATE LOGIN [##sql_login_CleanUpDatabaseUsers##] WITH PASSWORD = '', CHECK_POLICY = OFF;

				EXEC [{0}]..sp_executesql N'
					declare @person uniqueidentifier
					declare @staff1 uniqueidentifier
					declare @staff2 uniqueidentifier
					declare @staff3 uniqueidentifier
					declare @staff4 uniqueidentifier
					set @person = newid()
					set @staff1 = newid()
					set @staff2 = newid()
					set @staff3 = newid()
					set @staff4 = newid()
					insert dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values(@person, ''name'', GetUtcDate(), ''~BP'', GetUtcDate(), ''~BP'')
					INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_PER, GS_ActiveDirectoryObjectGuid, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
						(@staff1, ''~1#'', ''##CleanUpDatabaseUsersStaff1##'', @person, newid(), GetUtcDate(), ''~BP'', GetUtcDate(), ''~BP''),
						(@staff2, ''~2#'', ''##CleanUpDatabaseUsersStaff2##'', @person, null, GetUtcDate(), ''~BP'', GetUtcDate(), ''~BP''),
						(@staff3, ''~3#'', ''##CleanUpDatabaseUsersStaff3##'', @person, null, GetUtcDate(), ''~BP'', GetUtcDate(), ''~BP''),
						(@staff4, ''~4#'', ''##CleanUpDatabaseUsersStaff4##'', @person, null, GetUtcDate(), ''~BP'', GetUtcDate(), ''~BP'');
					INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES
						(newid(), ''6f0eb310-fc5c-4696-9594-f8ce156542c6'', @staff1),
						(newid(), ''6f0eb310-fc5c-4696-9594-f8ce156542c6'', @staff3),
						(newid(), ''208068b6-3383-44bf-8e0d-dbd827f9d675'', @staff3),
						(newid(), ''208068b6-3383-44bf-8e0d-dbd827f9d675'', @staff4);
					CREATE USER [##sql_login_CleanUpDatabaseUsers##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersNoStaff##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff1##] WITHOUT LOGIN;
					CREATE ROLE [##CleanUpDatabaseUsersRole##] AUTHORIZATION [##sql_login_CleanUpDatabaseUsers##];
					CREATE APPLICATION ROLE [##CleanUpDatabaseUsersAppRole##] WITH PASSWORD = ''This-Has*Got&To^Be%A$Long#Enough@Password!'';
				';
				EXEC [{0}]..sp_executesql N'CREATE SCHEMA [##Staff1Schema##] AUTHORIZATION [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff1##]';
				EXEC [{0}]..sp_executesql N'CREATE SCHEMA [##CleanUpDatabaseUsersRoleSchema##] AUTHORIZATION [##CleanUpDatabaseUsersRole##]';
				EXEC [{0}]..sp_executesql N'CREATE SCHEMA [##CleanUpDatabaseUsersAppRoleSchema##] AUTHORIZATION [##CleanUpDatabaseUsersAppRole##]';

				EXEC [{1}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseUsers##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff3##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff4##] WITHOUT LOGIN;
					CREATE ROLE [##CleanUpDatabaseUsersRole##] AUTHORIZATION [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##];
					ALTER ROLE [##CleanUpDatabaseUsersRole##] ADD MEMBER [{2}];
					CREATE APPLICATION ROLE [##CleanUpDatabaseUsersAppRole##] WITH PASSWORD = ''This-Has*Got&To^Be%A$Long#Enough@Password!'';
				';
				EXEC [{1}]..sp_executesql N'CREATE SCHEMA [##Staff2Schema##] AUTHORIZATION [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##]';
				EXEC [{1}]..sp_executesql N'CREATE SCHEMA [##Staff3Schema##] AUTHORIZATION [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff3##]';
				EXEC [{1}]..sp_executesql N'CREATE SCHEMA [##CleanUpDatabaseUsersRoleSchema##] AUTHORIZATION [##CleanUpDatabaseUsersRole##]';
				",
				Db.DatabaseName,
				testRefDb,
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)
			);

			testAdminConnection.ExecuteNonQuery(sqlText);
			testAdminConnection.CommitTransaction();
			var staffList = testSecurity.GetStaffLoginList_Exposed(testAdminConnection, Db.DatabaseName);
			foreach (var staffLoginName in new[] { "##CleanUpDatabaseUsersStaff1##", "##CleanUpDatabaseUsersStaff3##", "##CleanUpDatabaseUsersStaff4##" })
			{
				Assert($"Staff login {staffLoginName} with db access has been created and loaded", staffList.Any(x => x.GS_LoginName == staffLoginName));
			}

			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwUnrestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwHRMStaffRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##sql_login_CleanUpDatabaseUsers##", true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "CORP\\DATBackground", true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersNoStaff##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff1##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRole##", true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRole##", true);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##Staff1Schema##", true);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRoleSchema##", true);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRoleSchema##", true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##sql_login_CleanUpDatabaseUsers##", true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "CORP\\DATBackground", true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff3##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff4##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRole##", true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersAppRole##", true);
			AssertSchema(testAdminConnection, testRefDb, "##Staff2Schema##", true);
			AssertSchema(testAdminConnection, testRefDb, "##Staff3Schema##", true);
			AssertSchema(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRoleSchema##", true);

			var outputLog = testSecurity.CleanUpDatabaseUsers_Exposed(testAdminConnection, TestDbList);

			AssertOutputLog(outputLog,
				string.Format("REMOVING WINDOWS_USER [CORP\\DATBackground] FROM DATABASE [{0}]", Db.DatabaseName),
				string.Format("REMOVING SQL_USER [##sql_login_CleanUpDatabaseUsers##] FROM DATABASE [{0}]", Db.DatabaseName),
				string.Format("REMOVING SQL_USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersNoStaff##] FROM DATABASE [{0}]", Db.DatabaseName),
				"Error retrieving AD username for staff ##CleanUpDatabaseUsersStaff1##: Could not locate or write to directory entry for entity imMissing",
				string.Format("REMOVING DATABASE_ROLE [##CleanUpDatabaseUsersRole##] FROM DATABASE [{0}]", Db.DatabaseName),
				string.Format("REMOVING APPLICATION_ROLE [##CleanUpDatabaseUsersAppRole##] FROM DATABASE [{0}]", Db.DatabaseName),
				string.Format("REMOVING WINDOWS_USER [CORP\\DATBackground] FROM DATABASE [{0}]", testRefDb),
				string.Format("REMOVING SQL_USER [##sql_login_CleanUpDatabaseUsers##] FROM DATABASE [{0}]", testRefDb),
				string.Format("REMOVING SQL_USER [EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##] FROM DATABASE [{1}]", Db.DatabaseName, testRefDb),
				string.Format("REMOVING DATABASE_ROLE [##CleanUpDatabaseUsersRole##] FROM DATABASE [{0}]", testRefDb),
				"Error retrieving AD username for staff ##CleanUpDatabaseUsersStaff1##: Could not locate or write to directory entry for entity imMissing",
				string.Format("REMOVING APPLICATION_ROLE [##CleanUpDatabaseUsersAppRole##] FROM DATABASE [{0}]", testRefDb)
			);

			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwUnrestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwHRMStaffRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##sql_login_CleanUpDatabaseUsers##", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "CORP\\DATBackground", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersNoStaff##", Db.DatabaseName), false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff1##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRole##", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRole##", false);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##Staff1Schema##", true);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRoleSchema##", false);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRoleSchema##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##sql_login_CleanUpDatabaseUsers##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "CORP\\DATBackground", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##", Db.DatabaseName), false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff3##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff4##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRole##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersAppRole##", false);
			AssertSchema(testAdminConnection, testRefDb, "##Staff2Schema##", false);
			AssertSchema(testAdminConnection, testRefDb, "##Staff3Schema##", true);
			AssertSchema(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRoleSchema##", false);

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpDatabaseUsers_Exposed(testAdminConnection, TestDbList);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwRestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwUnrestrictedWriterRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, DbRoleTypes.CwHRMStaffRole, true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##sql_login_CleanUpDatabaseUsers##", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "CORP\\DATBackground", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersNoStaff##", Db.DatabaseName), false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff1##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRole##", false);
			AssertDatabasePrincipal(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRole##", false);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##Staff1Schema##", true);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersRoleSchema##", false);
			AssertSchema(testAdminConnection, Db.DatabaseName, "##CleanUpDatabaseUsersAppRoleSchema##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##sql_login_CleanUpDatabaseUsers##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "CORP\\DATBackground", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff2##", Db.DatabaseName), false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff3##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, string.Format("EnterpriseDbUser_{0}_##CleanUpDatabaseUsersStaff4##", Db.DatabaseName), true);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRole##", false);
			AssertDatabasePrincipal(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersAppRole##", false);
			AssertSchema(testAdminConnection, testRefDb, "##Staff2Schema##", false);
			AssertSchema(testAdminConnection, testRefDb, "##Staff3Schema##", true);
			AssertSchema(testAdminConnection, testRefDb, "##CleanUpDatabaseUsersRoleSchema##", false);

			testAdminConnection.ExecuteNonQuery(@"
				IF exists (SELECT null FROM sys.server_principals WHERE name = '##sql_login_CleanUpDatabaseUsers##') DROP LOGIN [##sql_login_CleanUpDatabaseUsers##];");
		}

		#endregion

		#region DATABASE LEVEL ROLE MEMBERSHIP

		public void TestCleanUpDatabaseLevelRoleMembership()
		{
			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);

			var sqlText = string.Format(@"
				CREATE LOGIN [##sql_login_CleanUpDatabaseLevelRoles##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'CORP\DATBackground') CREATE LOGIN [CORP\DATBackground] FROM WINDOWS;

				EXEC [{0}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelRoles##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					ALTER ROLE [db_backupoperator] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_datawriter] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_denydatawriter] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [db_owner] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [cwReaderRole] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [cwRestrictedReaderRole] ADD MEMBER [{3}];
					ALTER ROLE [cwRestrictedWriterRole] ADD MEMBER [{4}];
					ALTER ROLE [cwUnrestrictedWriterRole] ADD MEMBER [{5}];
				';
				EXEC [{1}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelRoles##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					ALTER ROLE [db_datareader] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_datawriter] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [cwReaderRole] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_denydatareader] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [db_securityadmin] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [cwRestrictedReaderRole] ADD MEMBER [{3}];
					ALTER ROLE [cwRestrictedWriterRole] ADD MEMBER [{4}];
					ALTER ROLE [cwUnrestrictedWriterRole] ADD MEMBER [{5}];
				';
				EXEC [{2}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelRoles##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					ALTER ROLE [db_datareader] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_datawriter] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [##sql_login_CleanUpDatabaseLevelRoles##];
					ALTER ROLE [db_denydatareader] ADD MEMBER [CORP\DATBackground];
					ALTER ROLE [db_securityadmin] ADD MEMBER [CORP\DATBackground];
				';",
				Db.DatabaseName,
				testRefDb,
				userRepositoryDb,
				RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName),
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName),
				UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)
			);

			testAdminConnection.ExecuteNonQuery(sqlText);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_owner", Db.SysAdminUserLogin, false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_denydatawriter", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_owner", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_owner", Db.SysAdminUserLogin, false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_owner", Db.SysAdminUserLogin, false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_securityadmin", "CORP\\DATBackground", true);

			var outputLog = testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);

			AssertOutputLog(outputLog,
				string.Format("REVOKED [db_owner] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", Db.DatabaseName),
				string.Format("REVOKED [db_datawriter] FROM [##sql_login_CleanUpDatabaseLevelRoles##] SQL_USER ON DATABASE [{0}]", Db.DatabaseName),
				string.Format("REVOKED [db_securityadmin] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [db_ddladmin] FROM [##sql_login_CleanUpDatabaseLevelRoles##] SQL_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [db_datawriter] FROM [##sql_login_CleanUpDatabaseLevelRoles##] SQL_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [db_securityadmin] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", userRepositoryDb),
				string.Format("REVOKED [db_ddladmin] FROM [##sql_login_CleanUpDatabaseLevelRoles##] SQL_USER ON DATABASE [{0}]", userRepositoryDb)
			);

			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_denydatawriter", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_owner", "CORP\\DATBackground", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", "CORP\\DATBackground", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_securityadmin", "CORP\\DATBackground", false);

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_denydatawriter", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_owner", "CORP\\DATBackground", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, DbSecurity.CwReaderRole, "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwUnrestrictedWriterRole", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, DbSecurity.CwReaderRole, "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", "CORP\\DATBackground", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "cwRestrictedWriterRole", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datareader", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datawriter", "##sql_login_CleanUpDatabaseLevelRoles##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_ddladmin", "##sql_login_CleanUpDatabaseLevelRoles##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_denydatareader", "CORP\\DATBackground", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_securityadmin", "CORP\\DATBackground", false);
		}

		public void TestCleanUpDatabaseLevelRoleMembership_EnterpriseDbUser()
		{
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);

			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##] WITHOUT LOGIN;
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_datawriter] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [cwRestrictedReaderRole] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
					ALTER ROLE [db_backupoperator] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
				';

				EXEC [{1}]..sp_executesql N'
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##] WITHOUT LOGIN;
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##] WITHOUT LOGIN;
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_datawriter] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [cwRestrictedReaderRole] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff2##];
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
					ALTER ROLE [db_backupoperator] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff3##];
				';

				EXEC [{2}]..sp_executesql N'
					CREATE USER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##] WITHOUT LOGIN;
					ALTER ROLE [db_ddladmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_securityadmin] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
					ALTER ROLE [db_datawriter] ADD MEMBER [EnterpriseDbUser_{0}_##CleanUpDatabaseLevelRoleMembershipStaff1##];
				';",

				Db.DatabaseName,
				testRefDb,
				userRepositoryDb
			);

			testAdminConnection.ExecuteNonQuery(sqlText);

			var outputLog = testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);

			AssertOutputLog(outputLog,
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_datawriter] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{Db.DatabaseName}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_datawriter] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{testRefDb}]",
				$"REVOKED [db_securityadmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{userRepositoryDb}]",
				$"REVOKED [db_ddladmin] FROM [EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##] SQL_USER ON DATABASE [{userRepositoryDb}]"
			);

			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", true);

			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", true);

			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpDatabaseLevelRoleMembership_Exposed(testAdminConnection, TestDbList);
			AssertEquals("Log count", 0, outputLog.Length);

			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", true);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, Db.DatabaseName, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", true);

			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", true);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff2##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", false);
			AssertDatabaseRoleMembership(testAdminConnection, testRefDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff3##", true);

			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_ddladmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_securityadmin", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_datawriter", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", true);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "cwRestrictedReaderRole", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
			AssertDatabaseRoleMembership(testAdminConnection, userRepositoryDb, "db_backupoperator", $"EnterpriseDbUser_{Db.DatabaseName}_##CleanUpDatabaseLevelRoleMembershipStaff1##", false);
		}

		#endregion

		#region DATABASE LEVEL PERMISSIONS

		public void TestCleanUpDatabaseLevelPermissions()
		{
			RefDbTableNameResolver.ResetSingleRefDatabaseName(testSingleRefDb);

			// Initial cleanup to build a test base from scratch
			var testSecurity = new DbSecurityForTest();
			testSecurity.CleanUpDatabaseLevelPermissions_Exposed(testAdminConnection, TestDbList);

			var sqlText = string.Format(@"
				CREATE LOGIN [##sql_login_CleanUpDatabaseLevelPermissions##] WITH PASSWORD = '', CHECK_POLICY = OFF;
				IF not exists (SELECT null FROM sys.server_principals WHERE name = 'CORP\DATBackground') CREATE LOGIN [CORP\DATBackground] FROM WINDOWS;
				EXEC [{0}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelPermissions##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					GRANT EXECUTE TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT EXECUTE ON [dbo].[ep_SpaceUsed] TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT INSERT ON [dbo].[GlbStaff] TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					DENY CONTROL TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT REFERENCES ON [dbo].[StmData]([SD_PK]) TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT ALTER ON SCHEMA::[dbo] TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CONTROL TO [CORP\DATBackground] WITH GRANT OPTION;
					GRANT CONNECT TO [CORP\DATBackground];
					GRANT SELECT ON [dbo].[OrgHeader]([OH_PK]) TO [CORP\DATBackground];
				';
				EXEC [{1}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelPermissions##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					GRANT VIEW DEFINITION TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE FUNCTION TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT INSERT TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE TABLE TO [CORP\DATBackground];
					GRANT EXECUTE TO [CORP\DATBackground];
					GRANT DELETE TO [CORP\DATBackground];
					DENY ALTER TO [CORP\DATBackground];
				';
				EXEC [{2}]..sp_executesql N'
					CREATE USER [##sql_login_CleanUpDatabaseLevelPermissions##];
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''CORP\DATBackground'') CREATE USER [CORP\DATBackground];
					GRANT UPDATE TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT VIEW DEFINITION TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE FUNCTION TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE PROCEDURE TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE SCHEMA TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE TYPE TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT INSERT TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT ALTER ON SCHEMA::[dbo] TO [##sql_login_CleanUpDatabaseLevelPermissions##];
					GRANT CREATE VIEW TO [CORP\DATBackground];
					GRANT CREATE TABLE TO [CORP\DATBackground];
					GRANT SELECT TO [CORP\DATBackground];
					GRANT EXECUTE TO [CORP\DATBackground];
					GRANT DELETE TO [CORP\DATBackground];
					GRANT REFERENCES TO [CORP\DATBackground];
					DENY ALTER TO [CORP\DATBackground];
				';
				EXEC [{3}]..sp_executesql N'
					IF not exists (SELECT null FROM sys.database_principals WHERE name = ''guest'') CREATE USER [guest];
					GRANT CONNECT TO [guest];
					GRANT EXECUTE TO [guest];
					GRANT SELECT TO [guest];
				';",
				Db.DatabaseName, testRefDb, userRepositoryDb, testSingleRefDb);
			testAdminConnection.ExecuteNonQuery(sqlText);
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", "ep_SpaceUsed", null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", "GlbStaff", null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "REFERENCES", "##sql_login_CleanUpDatabaseLevelPermissions##", "StmData", "SD_PK", "GRANT");
			AssertPermissionOnSchema(testAdminConnection, Db.DatabaseName, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "CORP\\DATBackground", null, null, "GRANT_WITH_GRANT_OPTION");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONNECT", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "CORP\\DATBackground", "OrgHeader", "OH_PK", "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "DELETE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "UPDATE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE PROCEDURE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertPermissionOnSchema(testAdminConnection, userRepositoryDb, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE VIEW", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "EXECUTE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "DELETE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "REFERENCES", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "CONNECT", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "EXECUTE", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "SELECT", "guest", null, null, "GRANT");

			var outputLog = testSecurity.CleanUpDatabaseLevelPermissions_Exposed(testAdminConnection, TestDbList);

			AssertOutputLog(outputLog,
				string.Format("REVOKED [EXECUTE] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON OBJECT [{0}].[dbo].[ep_SpaceUsed]", Db.DatabaseName),
				string.Format("REVOKED [INSERT] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON OBJECT [{0}].[dbo].[GlbStaff]", Db.DatabaseName),
				string.Format("REVOKED [REFERENCES] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON COLUMN [{0}].[dbo].[StmData] (SD_PK)", Db.DatabaseName),
				string.Format("REVOKED [ALTER] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON SCHEMA [{0}].[dbo]", Db.DatabaseName),
				string.Format("REVOKED [SELECT] FROM [CORP\\DATBackground] WINDOWS_USER ON COLUMN [{0}].[dbo].[OrgHeader] (OH_PK)", Db.DatabaseName),
				string.Format("REVOKED [EXECUTE] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON DATABASE [{0}]", Db.DatabaseName),
				string.Format("REVOKED [CONTROL] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", Db.DatabaseName),
				string.Format("REVOKED [CREATE FUNCTION] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [INSERT] FROM [##sql_login_CleanUpDatabaseLevelPermissions##] SQL_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [CREATE TABLE] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [DELETE] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", testRefDb),
				string.Format("REVOKED [EXECUTE] FROM [CORP\\DATBackground] WINDOWS_USER ON DATABASE [{0}]", testRefDb)
			);

			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", "ep_SpaceUsed", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", "GlbStaff", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "REFERENCES", "##sql_login_CleanUpDatabaseLevelPermissions##", "StmData", "SD_PK", "REVOKE");
			AssertPermissionOnSchema(testAdminConnection, Db.DatabaseName, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONNECT", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "CORP\\DATBackground", "OrgHeader", "OH_PK", "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "REFERENCES", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "DELETE", "CORP\\DATBackground", "StmData", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "UPDATE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE PROCEDURE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertPermissionOnSchema(testAdminConnection, userRepositoryDb, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE VIEW", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "EXECUTE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "DELETE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "REFERENCES", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "CONNECT", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "EXECUTE", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "SELECT", "guest", null, null, "GRANT");

			// Run again => no changes to be made
			outputLog = testSecurity.CleanUpDatabaseLevelPermissions_Exposed(testAdminConnection, TestDbList);
			AssertEquals("Log count", 0, outputLog.Length);
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "##sql_login_CleanUpDatabaseLevelPermissions##", "ep_SpaceUsed", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", "GlbStaff", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "REFERENCES", "##sql_login_CleanUpDatabaseLevelPermissions##", "StmData", "SD_PK", "REVOKE");
			AssertPermissionOnSchema(testAdminConnection, Db.DatabaseName, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONTROL", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "CONNECT", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "CORP\\DATBackground", "OrgHeader", "OH_PK", "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_CargoWiseReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedReaderLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_RestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", Db.DatabaseName + "_UnrestrictedWriterLogin", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "REFERENCES", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "EXECUTE", "CORP\\DATBackground", null, null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "DELETE", "CORP\\DATBackground", "StmData", null, "REVOKE");
			AssertDatabasePermission(testAdminConnection, testRefDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "UPDATE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "VIEW DEFINITION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE FUNCTION", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE PROCEDURE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE SCHEMA", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE TYPE", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "INSERT", "##sql_login_CleanUpDatabaseLevelPermissions##", null, null, "GRANT");
			AssertPermissionOnSchema(testAdminConnection, userRepositoryDb, "ALTER", "##sql_login_CleanUpDatabaseLevelPermissions##", "dbo", "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE VIEW", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "CREATE TABLE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "SELECT", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "EXECUTE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "DELETE", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "REFERENCES", "CORP\\DATBackground", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, userRepositoryDb, "ALTER", "CORP\\DATBackground", null, null, "DENY");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "CONNECT", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "EXECUTE", "guest", null, null, "GRANT");
			AssertDatabasePermission(testAdminConnection, testSingleRefDb, "SELECT", "guest", null, null, "GRANT");

			RefDbTableNameResolver.ResetSingleRefDatabaseName();
		}
		#endregion

		protected override void SetUp()
		{
			// ProcessFieldChangeRules need to be pre-loaded in the UberCache or terrible things happen
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
			testAdminConnection = Db.NewAdminConnection();
			AdoTestUtils.CreateDbIfNotExists(testAdminConnection, userRepositoryDb, Db.DatabaseName);
			AdoTestUtils.CreateDbIfNotExists(testAdminConnection, testSingleRefDb, Db.DatabaseName);
			testAdminConnection.BeginTransaction();
		}

		protected override void TearDown()
		{
			testAdminConnection.RollbackTransaction();
			AdoTestUtils.DropDbIfExists(testAdminConnection, testSingleRefDb, Db.DatabaseName);
			if (testAdminConnection != null)
			{
				testAdminConnection.Dispose();
			}

			base.TearDown();
		}

		string[] TestDbList
		{
			get
			{
				return testDbList ?? (testDbList = new string[] { Db.DatabaseName, testRefDb, userRepositoryDb, testSingleRefDb });
			}
		}
		string[] testDbList;

		AdminConnection testAdminConnection;
		readonly string testRefDb = Db.DatabaseName + "_RefDb_Ent_CA";
		readonly string testSingleRefDb = RefDbTableNameResolver.DefaultSingleRefDbName + "_ForTest";
		readonly string userRepositoryDb = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;

		internal static void CreateRole(AdminConnection connection, string roleName)
		{
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, $"CREATE ROLE {roleName.QuoteName()}"));
		}

		internal static void CreateRoleOnDatabase(AdminConnection connection, string dbName, string roleName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "\r\nDECLARE\r\n\t@SqlCmd nvarchar(max) = 'CREATE ROLE {1}'\r\n\r\nEXEC [{0}].sys.sp_executesql @SqlCmd;\r\n", dbName, roleName);
			connection.ExecuteNonQuery(sqlText);
		}

		internal static void GrantRoleSchemaPermissionOnDatabase(AdminConnection connection, string roleName, string schemaName, string permissionName)
		{
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, $"GRANT {permissionName} ON SCHEMA :: {schemaName.QuoteName()} TO {roleName.QuoteName()};"));
		}
	}

	public class DbSecurityAdminConnectionTest : TestCase
	{
		#region Refresh Database Reader Role

		[UseSnapshotProtection]
		public void TestRefreshDbReaderRolePermissions_fixesCargoWiseReaderLogin()
		{
			var testSecurity = new DbSecurityForTest();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			var cwReaderRoleUid = TestConnection.ExecuteScalar("select Principal_Id from sys.Database_Principals where name = 'cwReaderRole'");
			var cargoWiseReaderLoginUid = TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select Principal_Id from sys.database_Principals where name = '{0}_CargoWiseReaderLogin'", TestConnection.CurrentDatabase));
			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
				"ALTER ROLE [{0}] DROP MEMBER [{1}]", "cwReaderRole", TestConnection.CurrentDatabase + "_CargoWiseReaderLogin"));
			AssertEquals(0, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));

			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });

			AssertEquals(1, TestConnection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture,
				"select count(*) from sys.database_role_members where role_principal_id = {0} and member_principal_id = {1}", cwReaderRoleUid, cargoWiseReaderLoginUid)));
		}

		[UseSnapshotProtection]
		public void TestRefreshDbReaderRolePermissions()
		{
			var testRefDb = ((IPhysicalRefDbLocation)TestConnection).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "CA");

			// Pre-condition: role should exist in all application databases
			// Main DB
			AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, Db.DatabaseName, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SHOWPLAN", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.EpClientDatabasesProc, null, "REVOKE");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.ClrFunctionUncompressAsBytes, null, "GRANT");
			DbSecurityLockDownTest.AssertTypePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.TvpUniqueidentifier, "GRANT");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, CwReaderRole.EpClientDatabasesProc, DbSecurity.CwReaderRole);

			// Test Reference DB
			AssertDatabaseRoleExists(TestConnection, testRefDb, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, testRefDb, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");

			// DROP ROLE from main DB and test RefDb
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole);
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, testRefDb, DbSecurity.CwReaderRole);
			// Assert ROLE does not exist now
			AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole, false);
			AssertDatabaseRoleExists(TestConnection, testRefDb, DbSecurity.CwReaderRole, false);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.EpClientDatabasesProc, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, CwReaderRole.EpClientDatabasesProc, null);

			// RefreshDbReaderRolePermissions: Should recreate role
			var testSecurity = new DbSecurityForTest();
			var builder1 = new StringBuilder();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => builder1.AppendLine(msg));

			DbSecurityLockDownTest.AssertLogEntries(builder1.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", DbSecurity.CwReaderRole, Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", DbSecurity.CwReaderRole, testRefDb)
			);

			//Check if the RoleOwner is dbo
			DbSecurityLockDownTest.AssertRoleOwnerIsCorrect(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole, Db.SqlDbOwnerSchema);

			// Assert role and its permissions were recreated
			// Main DB
			AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, Db.DatabaseName, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SHOWPLAN", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.EpClientDatabasesProc, null, "REVOKE");
			DbSecurityLockDownTest.AssertTypePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.TvpUniqueidentifier, "GRANT");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, CwReaderRole.EpClientDatabasesProc, DbSecurity.CwReaderRole);

			// Test Reference DB
			AssertDatabaseRoleExists(TestConnection, testRefDb, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, testRefDb, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SHOWPLAN", DbSecurity.CwReaderRole, null, null, "GRANT");

			// Create a scalar funtion on each test database
			const string testFuctionName = "TestFunction_78314006-EDEF-4AC1-945B-874454D66FE0";
			var sqlText = string.Format(@"
				EXEC [{0}]..sp_executesql N'IF EXISTS (SELECT null FROM sys.objects WHERE name = ''{2}'') DROP FUNCTION [{2}]';
				EXEC [{0}]..sp_executesql N'CREATE FUNCTION [{2}]() RETURNS int AS BEGIN RETURN 0 END';
				EXEC [{1}]..sp_executesql N'IF EXISTS (SELECT null FROM sys.objects WHERE name = ''{2}'') DROP FUNCTION [{2}]';
				EXEC [{1}]..sp_executesql N'CREATE FUNCTION [{2}]() RETURNS int AS BEGIN RETURN 0 END';",
				Db.DatabaseName, testRefDb, testFuctionName);
			TestConnection.ExecuteNonQuery(sqlText);

			// Role should not yet have rights on newly created function
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "REVOKE");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "REVOKE");

			// Create a Stored Procedure on each test database
			var testTableName = "TestTable_059801AF-9524-4D1A-A616-18A3B3054473";
			var testProcedureName = "TestProcedure_059801AF-9524-4D1A-A616-18A3B3054473";
			var sql = string.Format(CultureInfo.InvariantCulture, @"
				EXEC [{0}].sys.sp_executesql N'if (OBJECT_ID(N''[{2}]'', N''U'') is NOT NULL) DROP TABLE [{2}]';
				EXEC [{0}].sys.sp_executesql N'CREATE TABLE [{2}] (id int, value int)';
				EXEC [{0}].sys.sp_executesql N'if (OBJECT_ID(N''[{3}]'', N''P'') is NOT NULL) DROP PROCEDURE [{3}]';
				EXEC [{0}].sys.sp_executesql N'CREATE PROCEDURE [{3}] @id int AS UPDATE [{2}] SET value = value + 1 WHERE id = @id';

				EXEC [{1}].sys.sp_executesql N'if (OBJECT_ID(N''[{2}]'', N''U'') is NOT NULL) DROP TABLE [{2}]';
				EXEC [{1}].sys.sp_executesql N'CREATE TABLE [{2}] (id int, value int)';
				EXEC [{1}].sys.sp_executesql N'if (OBJECT_ID(N''[{3}]'', N''P'') is NOT NULL) DROP PROCEDURE [{3}]';
				EXEC [{1}].sys.sp_executesql N'CREATE PROCEDURE [{3}] @id int AS UPDATE [{2}] SET value = value + 1 WHERE id = @id';
				"
				, Db.DatabaseName   // 0
				, testRefDb         // 1
				, testTableName     // 2
				, testProcedureName // 3
				);
			TestConnection.ExecuteNonQuery(sql);

			// Role should not yet have rights on newly created procedure
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, testProcedureName, null);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, testRefDb, Db.SqlDbOwnerSchema, testProcedureName, null);

			// RefreshDbReaderRolePermissions: Should grant rights to new funtion and procedure in Main DB only
			var builder2 = new StringBuilder();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => builder2.AppendLine(msg));

			DbSecurityTest.AssertOutputLog(builder2.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", DbSecurity.CwReaderRole, Db.DatabaseName)
			);

			// Assert role has execute rights on new function in the main database, but not in the test reference database
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "REVOKE");

			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, testProcedureName, DbSecurity.CwReaderRole);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, testRefDb, Db.SqlDbOwnerSchema, testProcedureName, null);

			// Run again => no changes to be made
			var builder3 = new StringBuilder();
			testSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => builder3.AppendLine(msg));
			AssertEquals("Log count", 0, builder3.Length);

			// Assert role and its permissions are still there
			// Main DB
			AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, Db.DatabaseName, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.EpClientDatabasesProc, null, "REVOKE");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, CwReaderRole.ClrFunctionUncompressAsBytes, null, "GRANT");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, CwReaderRole.EpClientDatabasesProc, DbSecurity.CwReaderRole);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, testProcedureName, DbSecurity.CwReaderRole);
			// Test Reference DB
			AssertDatabaseRoleExists(TestConnection, testRefDb, DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabaseRoleMembership(TestConnection, testRefDb, "db_datareader", DbSecurity.CwReaderRole, true);
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "VIEW DEFINITION", DbSecurity.CwReaderRole, null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "REVOKE");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, testRefDb, "EXECUTE", DbSecurity.CwReaderRole, testProcedureName, null, "REVOKE");
			DbSecurityLockDownTest.AssertObjectPrincipal(TestConnection, testRefDb, Db.SqlDbOwnerSchema, testProcedureName, null);
		}

		[UseSnapshotProtection]
		public void TestRefreshSchemaDbRoles_CreateNewRoles()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema);
			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, "cdc");

			var builder = new StringBuilder();
			new DbSecurityForTest().RefreshDbReaderRolePermissions(TestConnection, msg => builder.AppendLine(msg));
			new DbSecurityForTest().RefreshSchemaDbRoles(TestConnection, msg => builder.AppendLine(msg));

			DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", Db.DatabaseName)
			);

			var allCwRoles = DbRoleTypes.AllDbRoles;
			foreach (var cwRole in allCwRoles)
			{
				AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, cwRole.Name, true);
			}

			CombineAssertions(() =>
			{
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedReaderRole", "SELECT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "INSERT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "UPDATE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "DELETE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "EXECUTE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "ALTER");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "EXECUTE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			});
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRefreshSchemaDbRoles_CreateNewRoles_EdwDb()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema);
			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.EdwDatabaseName, "cdc");

			var builder = new StringBuilder();
			new DbSecurityForTest().RefreshDbReaderRolePermissions(TestConnection, msg => builder.AppendLine(msg));
			new DbSecurityForTest().RefreshSchemaDbRoles(TestConnection, msg => builder.AppendLine(msg));

			DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", Db.EdwDatabaseName)
			);

			var allCwRoles = DbRoleTypes.AllDbRoles;
			foreach (var cwRole in allCwRoles)
			{
				AssertDatabaseRoleExists(TestConnection, Db.EdwDatabaseName, cwRole.Name, true);
			}

			var sql = $"SELECT [name] FROM {Db.EdwDatabaseName}.sys.schemas WHERE name not in ('sys', 'guest', 'INFORMATION_SCHEMA') AND name not like 'db[_]%'";
			var edwSchemas = new List<string>();
			using (var command = TestConnection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						edwSchemas.Add(reader["name"].ToString());
					}
				}
			}
			var message = "EDW contains the following schemas:\r\n" + string.Join(", ", edwSchemas);
			CombineAssertions(message, () =>
			{
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedReaderRole", "SELECT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "INSERT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "UPDATE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "DELETE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "EXECUTE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "ALTER");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "EXECUTE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "cdc", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "cdc", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "cdc", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "Staging", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "Staging", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "Staging", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			});
		}

		[UseSnapshotProtection]
		public void TestRefreshSchemaDbRoles_RefreshExistingRoles()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema);
			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, "cdc");

			DbSecurityLockDownTest.CreateRole(TestConnection, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwHRMStaffRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedReaderRole", "dbo", "INSERT");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedWriterRole", DbSecurity.SqlHrmSchema, "CONTROL");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwHRMStaffRole", "dbo", "ALTER");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwUnrestrictedWriterRole", DbSecurity.SqlHrmSchema, "VIEW DEFINITION");

			var builder = new StringBuilder();
			new DbSecurityForTest().RefreshSchemaDbRoles(TestConnection, msg => builder.AppendLine(msg));

			DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", Db.DatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", Db.DatabaseName)
			);

			var allCwRoles = DbRoleTypes.AllDbRoles;
			foreach (var cwRole in allCwRoles)
			{
				AssertDatabaseRoleExists(TestConnection, Db.DatabaseName, cwRole.Name, true);
			}

			CombineAssertions(() =>
			{
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "CONTROL", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "ALTER", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwUnrestrictedWriterRole", "VIEW DEFINITION", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedReaderRole", "SELECT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "INSERT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "UPDATE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "DELETE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "EXECUTE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwRestrictedWriterRole", "ALTER");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "EXECUTE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			});
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRefreshSchemaDbRoles_RefreshExistingRoles_EdwDb()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema);
			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.EdwDatabaseName, "cdc");

			DbSecurityLockDownTest.CreateRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.CreateRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.CreateRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.CreateRoleOnDatabase(TestConnection, Db.EdwDatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedReaderRole", "dbo", "INSERT");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedWriterRole", DbSecurity.SqlHrmSchema, "CONTROL");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwHRMStaffRole", "dbo", "ALTER");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwUnrestrictedWriterRole", DbSecurity.SqlHrmSchema, "VIEW DEFINITION");

			var builder = new StringBuilder();
			new DbSecurityForTest().RefreshSchemaDbRoles(TestConnection, msg => builder.AppendLine(msg));

			DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", Db.EdwDatabaseName),
				string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", Db.EdwDatabaseName)
			);

			var allCwRoles = DbRoleTypes.AllDbRoles;
			foreach (var cwRole in allCwRoles)
			{
				AssertDatabaseRoleExists(TestConnection, Db.EdwDatabaseName, cwRole.Name, true);
			}

			CombineAssertions(() =>
			{
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "CONTROL", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "ALTER", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwUnrestrictedWriterRole", "VIEW DEFINITION", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedReaderRole", "SELECT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "INSERT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "UPDATE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "DELETE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "EXECUTE");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwRestrictedWriterRole", "ALTER");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, "dbo", "cwHRMStaffRole", "EXECUTE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlCdcSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlCdcSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlCdcSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlCdcSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlCdcSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlStagingSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlStagingSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlStagingSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlStagingSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlStagingSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
				DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.EdwDatabaseName, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
				DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.EdwDatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			});
		}

		public static void AssertDatabaseRoleExists(AdminConnection connection, string dbName, string roleName, bool expected)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dr
				WHERE dr.type = 'R'
				AND dr.name = '{1}'",
				dbName, roleName);
			var actual = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("Does role [{0}] exist on database [{1}]?", roleName, dbName), expected, actual);
		}

		#endregion

		#region Stored Procedure Authorization

		[UseSnapshotProtection]
		public void TestStoredProcedureAuthorization()
		{
			var schemaName = Db.SqlDbOwnerSchema;
			var tableName = "_Test_Table";
			var procedureName = "_Test_Procedure";
			var readonlyUser = "EnterpriseDbUser_##ReadonlyUser##";

			// Prepare data
			var dbSecurity = new DbSecurity();
			dbSecurity.RefreshDbReaderRolePermissions(TestConnection, msg => { });
			CreateTestObjects(TestConnection, schemaName, tableName, procedureName, readonlyUser);

			var expected = new string[]
				{
					"id = 1, value = 0",
					"id = 2, value = 0",
					"id = 3, value = 0",
				};
			AssertContainsExactElementsInAnyOrder("PRECONDITION", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in current user context (Application)
			var sql = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].[{1}] @id = 1;", schemaName, procedureName);
			TestConnection.ExecuteNonQuery(sql);
			expected = new string[]
				{
					"id = 1, value = 1",
					"id = 2, value = 0",
					"id = 3, value = 0",
				};
			AssertContainsExactElementsInAnyOrder("Application user", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in read-only user context
			sql = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}] @id = 2;", schemaName, procedureName);
			try
			{
				TestConnection.ExecuteNonQuery($"EXECUTE AS USER = '{readonlyUser}'");
				TestConnection.ImpersonatedLogin = readonlyUser;

				TestConnection.ExecuteNonQuery(sql);
				Fail("Exception expected");
			}
			catch (SqlException ex)
			{
				AssertEquals(string.Format("The UPDATE permission was denied on the object '{0}', database '{1}', schema '{2}'.", tableName, TestConnection.CurrentDatabase, schemaName), ex.Message);
			}
			finally
			{
				TestConnection.ExecuteNonQuery("REVERT");
				TestConnection.ImpersonatedLogin = null;
			}

			// no changes done
			AssertContainsExactElementsInAnyOrder("Read-only user", expected, GetTestValues(TestConnection, schemaName, tableName));

			// running procedure in current user context (Application)
			sql = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}] @id = 3;", schemaName, procedureName);
			TestConnection.ExecuteNonQuery(sql);
			expected = new string[]
				{
					"id = 1, value = 1",
					"id = 2, value = 0",
					"id = 3, value = 1",
				};
			AssertContainsExactElementsInAnyOrder("Admin user", expected, GetTestValues(TestConnection, schemaName, tableName));
		}

		void CreateTestObjects(AdminConnection connection, string schemaName, string tableName, string procedureName, string readonlyUser)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID(N'[{0}].[{2}]', N'P') is NOT NULL) DROP PROCEDURE [{0}].[{2}];
if (OBJECT_ID(N'[{0}].[{1}]', N'U') is NOT NULL) DROP TABLE [{0}].[{1}];

CREATE TABLE [{0}].[{1}]
(
	id    int,
	value int,
);

INSERT [{0}].[{1}] (id, value) VALUES
-- (id, value)
   (1 , 0    ),
   (2 , 0    ),
   (3 , 0    );
"
				, schemaName    // 0
				, tableName     // 1
				, procedureName // 2
				);
			connection.ExecuteNonQuery(sql);

			sql = string.Format(CultureInfo.InvariantCulture, @"
CREATE PROCEDURE [{0}].[{2}]
	@id int
AS

UPDATE [{0}].[{1}] SET
	value = value + 1
WHERE
	id = @id
"
				, schemaName    // 0
				, tableName     // 1
				, procedureName // 2
				);
			connection.ExecuteNonQuery(sql);

			sql = string.Format(CultureInfo.InvariantCulture, @" -- CreateTestUser
CREATE USER [{0}] WITHOUT LOGIN;
ALTER ROLE [{1}] ADD MEMBER [{0}];
ALTER AUTHORIZATION ON [{2}].[{3}] TO [{1}];
"
				, readonlyUser            // 0
				, DbSecurity.CwReaderRole // 1
				, schemaName              // 2
				, procedureName           // 3
				);
			connection.ExecuteNonQuery(sql);
		}

		IEnumerable<string> GetTestValues(AdminConnection connection, string schemaName, string tableName)
		{
			var values = new List<string>(3);

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT id, value FROM [{0}].[{1}] ORDER BY id", schemaName, tableName);
			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add(string.Format(CultureInfo.InvariantCulture, "id = {0}, value = {1}", reader["id"], reader["value"]));
				}
			}

			return values;
		}

		#endregion // Stored Procedure Authorization

		#region Drop Users with Grant Permission

		public void TestCleanUpDatabaseUsersWithGrantsToRole()
		{
			var dbName = Db.DatabaseName;
			var logins = new List<string>();
			var grantPermissions = new List<string> { "IMPERSONATE", "CONTROL", "ALTER", "VIEW DEFINITION" };

			try
			{
				CreateRole(TestConnection, DbRoleTypes.CwUnrestrictedWriterRole);

				foreach (var permission in grantPermissions)
				{
					var login = Invariant($"{dbName}_TestLoginWithGrant{permission.Replace(" ", "_")}");
					logins.Add(login);
					CreateLogin(TestConnection, login);
					GrantPermission(TestConnection, login, permission, DbRoleTypes.CwUnrestrictedWriterRole);
				}

				logins.ForEach(loginName => AssertDatabasePrincipal(TestConnection, loginName, true));

				var testSecurity = new DbSecurityForTest();
				testSecurity.CleanUpDatabaseUsers_Exposed(TestConnection, dbName);

				logins.ForEach(loginName => AssertDatabasePrincipal(TestConnection, loginName, false));
			}
			finally
			{
				logins.ForEach(DropLogin);
			}
		}

		public void TestCleanUpDatabaseUsersWithGrantsToUser()
		{
			var dbName = Db.DatabaseName;
			var logins = new List<string>();
			var grantPermissions = new List<string> { "IMPERSONATE", "CONTROL", "ALTER", "VIEW DEFINITION" };

			try
			{
				CreateLogin(TestConnection, "DummyTestCleanupUser_001");

				foreach (var permission in grantPermissions)
				{
					var login = Invariant($"{dbName}_TestLoginWithGrant{permission.Replace(" ", "_")}");
					logins.Add(login);

					CreateLogin(TestConnection, login);
					GrantPermission(TestConnection, login, permission, "DummyTestCleanupUser_001");
				}

				logins.ForEach(loginName => AssertDatabasePrincipal(TestConnection, loginName, true));

				var testSecurity = new DbSecurityForTest();
				testSecurity.CleanUpDatabaseUsers_Exposed(TestConnection, dbName);

				logins.ForEach(loginName => AssertDatabasePrincipal(TestConnection, loginName, false));
			}
			finally
			{
				DropLogin("DummyTestCleanupUser_001");
				logins.ForEach(DropLogin);
			}
		}

		#endregion

		#region Implementation

		AdminConnection TestConnection;

		void CreateLogin(DbConnection connection, string loginName)
		{
			var sqlText = $@"
CREATE LOGIN {loginName.QuoteName()} WITH PASSWORD = '', CHECK_POLICY = OFF;
CREATE USER {loginName.QuoteName()} FOR LOGIN {loginName.QuoteName()};
";
			connection.ExecuteNonQuery(sqlText);
		}

		void CreateRole(DbConnection connection, string roleName)
		{
			var sqlText = $@"
IF not exists (SELECT null FROM sys.database_principals WHERE type = 'R' AND name = '{roleName}') CREATE ROLE {roleName.QuoteName()};
";
			connection.ExecuteNonQuery(sqlText);
		}

		void GrantPermission(DbConnection connection, string loginName, string permission, string granteePrincipleName)
		{
			var sqlText = $@"
GRANT {permission} ON USER::{loginName.QuoteName()} TO {granteePrincipleName.QuoteName()}
";
			connection.ExecuteNonQuery(sqlText);
		}

		void DropLogin(string loginName)
		{
			TestConnection.ExecuteNonQuery($@"
IF EXISTS(SELECT NULL FROM sys.server_principals WHERE name = N'{loginName.QuoteEscapedName('\'')}') DROP LOGIN {loginName.QuoteName()};
");
		}

		void AssertDatabasePrincipal(DbConnection connection, string principalName, bool expectedExists)
		{
			var actualExists = connection.Exists("FROM sys.database_principals WHERE name = @name",
				cmd =>
				{
					cmd.AddParameter("@name", SqlDbType.NVarChar, 128, principalName);
				});

			AssertEquals($"Principal [{principalName}] exists?", expectedExists, actualExists);
		}

		#endregion

		protected override void SetUp()
		{
			TestConnection = Db.NewAdminConnection();

			base.SetUp();
		}

		protected override void TearDown()
		{
			TestConnection.Dispose();
			TestConnection = null;

			base.TearDown();
		}
	}

	public class DbSecurityNonTransactionedTest : TestCase
	{
		public void TestCleanUpDatabaseUsersFromUnusedRefDB()
		{
			var testMainDb = "TestCleanUpDatabaseUsersFromUnusedRefDB";
			var testrefDb1 = "CW-RefDb-TTT-AU-0000001";
			var testrefDb2 = "CW-RefDb-TTT-AU-0000002";
			var testTable = "tbl_Test";
			var testSecurity = new DbSecurityForTest { IsSharedDatabase_Override = true };
			var otherDBReaderLogin = string.Format(CultureInfo.InvariantCulture, "{0}_CargoWiseReaderLogin", testMainDb);
			var otherDBWriterLogin = string.Format(CultureInfo.InvariantCulture, "{0}_CargoWiseWriterLogin", testMainDb);
			var staffDbLogin = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_xxx", DbUserRepository.StaffDbLoginPrefix, testMainDb);
			var synonymName = "RefDbTTTAU_Test";
			var randomUserLogin = "RandomUser";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb1, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb2, testMainDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testrefDb1, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testrefDb2, testMainDb);

					var sqlText = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS(SELECT * FROM [{1}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{1}].[dbo].[{0}]
						END

						IF EXISTS(SELECT * FROM [{2}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{2}].[dbo].[{0}]
						END

						IF EXISTS(SELECT * FROM [{3}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{3}].[dbo].[{0}]
						END

						CREATE TABLE [{1}].[dbo].[{0}](
							[id] [int] NULL);

						CREATE TABLE [{2}].[dbo].[{0}](
							[id] [int] NULL);

						CREATE TABLE [{3}].[dbo].[{0}](
							[id] [int] NULL);", testTable, testMainDb, testrefDb1, testrefDb2);

					using (((ICurrentDbControl)createDbConnection).UseDatabase(testMainDb))
					{
						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS(SELECT * FROM sys.synonyms WHERE NAME = '{0}')
							BEGIN
								DROP SYNONYM [dbo].{0}
							END
							CREATE SYNONYM [dbo].[{0}] FOR [{1}].[dbo].[test]", synonymName, testrefDb2);

						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{0}') CREATE LOGIN [{0}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{1}') CREATE LOGIN [{1}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{2}') CREATE LOGIN [{2}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{3}') CREATE LOGIN [{3}] WITH PASSWORD = '', CHECK_POLICY = OFF;

						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
						",
							otherDBReaderLogin,
							otherDBWriterLogin,
							staffDbLogin,
							randomUserLogin
						);

						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"
							use [{4}]
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
							",
						   otherDBReaderLogin,
						   otherDBWriterLogin,
						   staffDbLogin,
						   randomUserLogin,
						   testrefDb1
					   );
						createDbConnection.ExecuteNonQuery(sqlText);
						sqlText = string.Format(CultureInfo.InvariantCulture, @"
							use [{4}]
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
							",
							otherDBReaderLogin,
							otherDBWriterLogin,
							staffDbLogin,
							randomUserLogin,
							testrefDb2
						);
						createDbConnection.ExecuteNonQuery(sqlText);
					}

					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, randomUserLogin, true);

					testSecurity.CleanUpDbRightsFromUnusedRefDBSafe_Exposed(createDbConnection, testMainDb);

					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBWriterLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBReaderLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, staffDbLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, randomUserLogin, true);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb1, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb2, testMainDb);

					AdoTestUtils.DropDbLoginIfExists(createDbConnection, otherDBReaderLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, otherDBWriterLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, staffDbLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, randomUserLogin);
				}
			}
		}

		public void TestCleanUpDatabaseUsersFromUnusedAvailabilityGroupRefDB()
		{
			var testMainDb = "TestCleanUpDatabaseUsersFromUnusedRefDB";
			var testrefDb1 = "CW-AG-RefDb-ORDWP4-CP1AS1-TTT-AU-0000001";
			var testrefDb2 = "CW-AG-RefDb-ORDWP4-CP1AS1-TTT-AU-0000002";
			var testTable = "tbl_Test";
			var testSecurity = new DbSecurityForTest { IsSharedDatabase_Override = true };
			var otherDBReaderLogin = string.Format(CultureInfo.InvariantCulture, "{0}_CargoWiseReaderLogin", testMainDb);
			var otherDBWriterLogin = string.Format(CultureInfo.InvariantCulture, "{0}_CargoWiseWriterLogin", testMainDb);
			var staffDbLogin = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_xxx", DbUserRepository.StaffDbLoginPrefix, testMainDb);
			var synonymName = "RefDbTTTAU_Test";
			var randomUserLogin = "RandomUser";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb1, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb2, testMainDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testrefDb1, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testrefDb2, testMainDb);

					var sqlText = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS(SELECT * FROM [{1}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{1}].[dbo].[{0}]
						END

						IF EXISTS(SELECT * FROM [{2}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{2}].[dbo].[{0}]
						END

						IF EXISTS(SELECT * FROM [{3}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
						BEGIN
						DROP TABLE [{3}].[dbo].[{0}]
						END

						CREATE TABLE [{1}].[dbo].[{0}](
							[id] [int] NULL);

						CREATE TABLE [{2}].[dbo].[{0}](
							[id] [int] NULL);

						CREATE TABLE [{3}].[dbo].[{0}](
							[id] [int] NULL);", testTable, testMainDb, testrefDb1, testrefDb2);

					using (((ICurrentDbControl)createDbConnection).UseDatabase(testMainDb))
					{
						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS(SELECT * FROM sys.synonyms WHERE NAME = '{0}')
							BEGIN
								DROP SYNONYM [dbo].{0}
							END
							CREATE SYNONYM [dbo].[{0}] FOR [{1}].[dbo].[test]", synonymName, testrefDb2);

						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{0}') CREATE LOGIN [{0}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{1}') CREATE LOGIN [{1}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{2}') CREATE LOGIN [{2}] WITH PASSWORD = '', CHECK_POLICY = OFF;
						IF not exists (SELECT null FROM sys.server_principals WHERE name = '{3}') CREATE LOGIN [{3}] WITH PASSWORD = '', CHECK_POLICY = OFF;

						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
						IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
						",
							otherDBReaderLogin,
							otherDBWriterLogin,
							staffDbLogin,
							randomUserLogin
						);

						createDbConnection.ExecuteNonQuery(sqlText);

						sqlText = string.Format(CultureInfo.InvariantCulture, @"
							use [{4}]
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
							",
							otherDBReaderLogin,
							otherDBWriterLogin,
							staffDbLogin,
							randomUserLogin,
							testrefDb1
						);
						createDbConnection.ExecuteNonQuery(sqlText);
						sqlText = string.Format(CultureInfo.InvariantCulture, @"
							use [{4}]
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{0}') CREATE USER [{0}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{1}') CREATE USER [{1}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{2}') CREATE USER [{2}];
							IF not exists (SELECT null FROM sys.database_principals WHERE name = '{3}') CREATE USER [{3}];
							",
							otherDBReaderLogin,
							otherDBWriterLogin,
							staffDbLogin,
							randomUserLogin,
							testrefDb2
						);
						createDbConnection.ExecuteNonQuery(sqlText);
					}

					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, randomUserLogin, true);

					testSecurity.CleanUpDbRightsFromUnusedRefDBSafe_Exposed(createDbConnection, testMainDb);

					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testMainDb, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBWriterLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, otherDBReaderLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, staffDbLogin, false);
					AssertDatabasePrincipal(createDbConnection, testrefDb1, randomUserLogin, true);

					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBWriterLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, otherDBReaderLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, staffDbLogin, true);
					AssertDatabasePrincipal(createDbConnection, testrefDb2, randomUserLogin, true);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb1, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testrefDb2, testMainDb);

					AdoTestUtils.DropDbLoginIfExists(createDbConnection, otherDBReaderLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, otherDBWriterLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, staffDbLogin);
					AdoTestUtils.DropDbLoginIfExists(createDbConnection, randomUserLogin);
				}
			}
		}

		public void AssertDatabasePrincipal(AdminConnection connection, string dbName, string principalName, bool expectedExists, string message = "")
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dl
				WHERE dl.name = '{1}'",
				dbName, principalName);
			var actualExists = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("{0}: Principal [{1}] exists on database [{2}]?", message, principalName, dbName), expectedExists, actualExists);
		}
		/// <summary>
		/// CREATE FULLTEXT CATALOG statement cannot be used inside a user transaction.
		/// DROP FULLTEXT CATALOG statement cannot be used inside a user transaction.
		/// </summary>
		public void TestCleanUpDatabaseUsersRemoveRelatedFulltextObjects()
		{
			const string testDb = "DbSecurityNonTransactionedTest00DbMain00";
			var testSecurity = new DbSecurityForTest();

			using (var adminConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConn, testDb);

					using (var testDbConn = Db.NewAdminConnection(testDb))
					{
						// Initial cleanup to build a test base from scratch
						testSecurity.CleanUpDatabaseUsers_Exposed(testDbConn, testDb);

						var sqlText = @"
							CREATE USER [TestUser] WITHOUT LOGIN;
							CREATE FULLTEXT CATALOG [TestFulltextCatalog] AS DEFAULT AUTHORIZATION [TestUser];
							CREATE TABLE [TestTable] (Col1 CHAR(1) NOT NULL CONSTRAINT [TestPk] PRIMARY KEY);
							IF (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1)
							BEGIN
								CREATE FULLTEXT INDEX ON [TestTable](Col1) KEY INDEX [TestPk] ON [TestFulltextCatalog];
							END";
						testDbConn.ExecuteNonQuery(sqlText);

						AssertUserExists(testDbConn, "TestUser", true);
						AssertUserDependentFulltextCatalogExist(testDbConn, "TestUser", "TestFulltextCatalog");

						var log = testSecurity.CleanUpDatabaseUsers_Exposed(testDbConn, testDb);

						DbSecurityTest.AssertOutputLog(log,
							string.Format("REMOVING SQL_USER [TestUser] FROM DATABASE [{0}]", testDb)
						);

						AssertUserExists(testDbConn, "TestUser", false);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConn, testDb);
				}
			}
		}

		void AssertUserExists(AdminConnection conn, string userName, bool expected)
		{
			var sqlText = string.Format("SELECT count(*) FROM sys.database_principals WHERE name = '{0}'", userName);
			var count = Convert.ToInt32(conn.ExecuteScalar(sqlText));
			AssertEquals("User [" + userName + "] exists?", expected, count == 1);
		}

		void AssertUserDependentFulltextCatalogExist(AdminConnection conn, string userName, string expectedCatalogName)
		{
			var sqlText = string.Format(@"
				SELECT ftc.name
				FROM sys.fulltext_catalogs ftc
				INNER JOIN sys.database_principals dp ON dp.principal_id = ftc.principal_id
				LEFT JOIN sys.fulltext_indexes fti ON fti.fulltext_catalog_id = ftc.fulltext_catalog_id
				WHERE dp.name = '{0}'
				AND (fti.object_id is not null OR FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 0)",
				userName);
			var actualCatalogName = conn.ExecuteScalar(sqlText).ToString();
			AssertEquals("User [" + userName + "] fulltext catalog", expectedCatalogName, actualCatalogName);
		}

		#region Refresh Database Reader Role

		public void TestRefreshDbReaderRolePermissionsSkipsReadonlyDatabases()
		{
			var testMainDb = "TestRefreshDbReaderRolePermissionsSkipsReadonlyDatabases";
			var testEdoc1Db = testMainDb + "_SD001";
			var testEdoc2Db = testMainDb + "_SD002";

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(adminConnection, testEdoc1Db);
				AdoTestUtils.DropDbIfExists(adminConnection, testEdoc2Db);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testEdoc1Db);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, testEdoc2Db);

					adminConnection.AlterDbWriteableState(testEdoc2Db, false);

					using (var connection = Db.NewAdminConnection(Db.ServerName, testMainDb))
					{
						new DbSecurityForTest().RefreshDbReaderRolePermissions(connection, msg => { });

						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testMainDb, DbSecurity.CwReaderRole, true);
						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testEdoc1Db, DbSecurity.CwReaderRole, true);
						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testEdoc2Db, DbSecurity.CwReaderRole, false);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(adminConnection, testEdoc1Db);
					AdoTestUtils.DropDbIfExists(adminConnection, testEdoc2Db);
				}
			}
		}

		public void TestRefreshDbReaderRolePermissionsWorksWithMainDbNameCaseMismatch()
		{
			var testMainDb = "TestRefreshDbReaderRolePermissionsWithMainDbNameCaseMismatch";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);

					// Create a scalar funtion
					const string testFuctionName = "TestScalarFunction";
					createDbConnection.ExecuteNonQuery($"EXEC [{testMainDb}]..sp_executesql N'CREATE FUNCTION [{testFuctionName}]() RETURNS bit AS BEGIN RETURN 0 END'");

					using (var connection = Db.NewAdminConnection(Db.ServerName, testMainDb.ToUpperInvariant()))
					{
						//We now need to explicitly delete the role because it gets automatically generated.

						connection.ExecuteNonQuery("DROP ROLE IF EXISTS [cwReaderRole]");

						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testMainDb, DbSecurity.CwReaderRole, false);

						new DbSecurityForTest().RefreshDbReaderRolePermissions(connection, msg => { });

						DbSecurityAdminConnectionTest.AssertDatabaseRoleExists(connection, testMainDb, DbSecurity.CwReaderRole, true);
						DbSecurityLockDownTest.AssertDatabasePermission(connection, testMainDb, "EXECUTE", DbSecurity.CwReaderRole, testFuctionName, null, "GRANT");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				}
			}
		}

		#endregion

		#region RefreshSchemaDbRoles

		[UseSnapshotProtection]
		public void TestRefreshSchemaDbRoles_CreateNewRoles_MainDb()
		{
			var testMainDb = "DbTestRefreshSchemaDbRolesForNewRoles";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);

					using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, testMainDb))
					{
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwRestrictedReaderRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwRestrictedWriterRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwHRMStaffRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwUnrestrictedWriterRole");

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema);
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "cdc");

						((IDbLoginRepair)testAdminConnection).EnsureReaderDbLogin();

						var builder = new StringBuilder();
						new DbSecurityForTest().RefreshDbReaderRolePermissions(testAdminConnection, msg => builder.AppendLine(msg));
						new DbSecurityForTest().RefreshSchemaDbRoles(testAdminConnection, msg => builder.AppendLine(msg));

						DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", testMainDb)
						);

						var allCwRoles = DbRoleTypes.AllDbRoles;
						foreach (var cwRole in allCwRoles)
						{
							AssertDatabaseRoleExists(testAdminConnection, testMainDb, cwRole.Name, true);
						}

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedReaderRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedReaderRole", "EXECUTE");

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "INSERT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "UPDATE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "DELETE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "EXECUTE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "ALTER");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "EXECUTE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRefreshSchemaDbRoles_CreateNewRoles_ReferenceDb()
		{
			var testMainDb = "DbTestRefreshSchemaDbRolesForNewRoles";
			var testRefDb = testMainDb + "_SD001";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testRefDb);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testRefDb);

					using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, testMainDb))
					{
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testRefDb, "cwRestrictedReaderRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testRefDb, "cwRestrictedWriterRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testRefDb, "cwHRMStaffRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testRefDb, "cwUnrestrictedWriterRole");

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema);
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "cdc");
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testRefDb, "rschema1");
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testRefDb, "rschema2");

						((IDbLoginRepair)testAdminConnection).EnsureReaderDbLogin();

						var builder = new StringBuilder();
						new DbSecurityForTest().RefreshDbReaderRolePermissions(testAdminConnection, msg => builder.AppendLine(msg));
						new DbSecurityForTest().RefreshSchemaDbRoles(testAdminConnection, msg => builder.AppendLine(msg));

						DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", testRefDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", testRefDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", testRefDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", testRefDb)
						);

						var allCwRoles = DbRoleTypes.AllDbRoles;
						foreach (var cwRole in allCwRoles)
						{
							AssertDatabaseRoleExists(testAdminConnection, testRefDb, cwRole.Name, true);
						}

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwRestrictedReaderRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwRestrictedReaderRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwRestrictedReaderRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwRestrictedReaderRole", "EXECUTE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwRestrictedReaderRole", "EXECUTE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwRestrictedReaderRole", "EXECUTE");

						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwHRMStaffRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "dbo", "cwHRMStaffRole", "EXECUTE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwHRMStaffRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema1", "cwHRMStaffRole", "EXECUTE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwHRMStaffRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "rSchema2", "cwHRMStaffRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "cdc", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "cdc", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "cdc", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "Staging", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "Staging", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "Staging", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testRefDb, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testRefDb);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRefreshSchemaDbRoles_RefreshExistingRoles()
		{
			var testMainDb = "MainDbTestRefreshSchemaDbRolesForExistingRoles";
			var testRef1Db = testMainDb + "_SD001";
			var testRef2Db = testMainDb + "_SD002";

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
				AdoTestUtils.DropDbIfExists(createDbConnection, testRef1Db);
				AdoTestUtils.DropDbIfExists(createDbConnection, testRef2Db);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testMainDb);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testRef1Db);
					AdoTestUtils.CreateDbIfNotExists(createDbConnection, testRef2Db);

					using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, testMainDb))
					{
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwRestrictedReaderRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwRestrictedWriterRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwHRMStaffRole");
						DbSecurityLockDownTest.DropRoleOnDatabase(testAdminConnection, testMainDb, "cwUnrestrictedWriterRole");

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema);
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "cdc");

						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwRestrictedReaderRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwRestrictedWriterRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwHRMStaffRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwUnrestrictedWriterRole");

						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwRestrictedReaderRole", "dbo", "INSERT");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwRestrictedWriterRole", DbSecurity.SqlHrmSchema, "CONTROL");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwHRMStaffRole", "dbo", "ALTER");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwUnrestrictedWriterRole", DbSecurity.SqlHrmSchema, "VIEW DEFINITION");

						((IDbLoginRepair)testAdminConnection).EnsureReaderDbLogin();

						var builder = new StringBuilder();
						new DbSecurityForTest().RefreshDbReaderRolePermissions(testAdminConnection, msg => builder.AppendLine(msg));
						new DbSecurityForTest().RefreshSchemaDbRoles(testAdminConnection, msg => builder.AppendLine(msg));

						DbSecurityLockDownTest.AssertLogEntries(builder.ToString(),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedReaderRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwRestrictedWriterRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwHRMStaffRole", testMainDb),
							string.Format("Role [{0}] permissions updated on database [{1}].", "cwUnrestrictedWriterRole", testMainDb)
						);

						var allCwRoles = DbRoleTypes.AllDbRoles;
						foreach (var cwRole in allCwRoles)
						{
							AssertDatabaseRoleExists(testAdminConnection, testMainDb, cwRole.Name, true);
						}

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwRestrictedWriterRole", "CONTROL", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "ALTER", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwUnrestrictedWriterRole", "VIEW DEFINITION", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedReaderRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedReaderRole", "EXECUTE");

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "INSERT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "UPDATE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "DELETE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "EXECUTE");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwRestrictedWriterRole", "ALTER");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "ALTER", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "VIEW DATABASE STATE", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "VIEW DEFINITION", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "SHOWPLAN", "cwRestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, testMainDb, "REFERENCES", "cwRestrictedWriterRole", null, null, "GRANT");

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "EXECUTE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, DbSecurity.SqlHrmSchema, "cwHRMStaffRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "cdc", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "SELECT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "Staging", "cwRestrictedWriterRole", "EXECUTE", false);

						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
						DbSecurityLockDownTest.AssertDatabasePermission(testAdminConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(createDbConnection, testMainDb);
					AdoTestUtils.DropDbIfExists(createDbConnection, testRef1Db);
					AdoTestUtils.DropDbIfExists(createDbConnection, testRef2Db);
				}
			}
		}

		public static void AssertDatabaseRoleExists(DbConnection connection, string dbName, string roleName, bool expected)
		{
			var sqlText = string.Format(@"
				SELECT count(*)
				FROM [{0}].sys.database_principals dr
				WHERE dr.type = 'R'
				AND dr.name = '{1}'",
				dbName, roleName);
			var actual = ((int)connection.ExecuteScalar(sqlText) == 1);
			AssertEquals(string.Format("Does role [{0}] exist on database [{1}]?", roleName, dbName), expected, actual);
		}

		#endregion

		public void TestCleanUpDatabaseUsersInUserRepository()
		{
			// Arrange
			var mainDb = "DsaMainDb";
			var userRepositoryDbName = mainDb + DbUserRepository.RepositoryDbSuffix;
			var testSecurity = new DbSecurityForTest() { IsUserRepository_ForTest = true };
			var cwRole = DbRoleTypes.CwUnrestrictedWriterRole;
			var fakeUser = "FakeUser";
			var fakeSchema = "FakeSchema";
			var fakeTable = "FakeTable";
			var fakeFullTextCatalog = "FakeFullTextCatalog";
			var fakeRole = "FakeRole";

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, userRepositoryDbName, Db.DatabaseName))
			using (((ICurrentDbControl)connection).UseDatabase(userRepositoryDbName))
			{
				connection.ExecuteNonQuery($"CREATE USER [{fakeUser}] WITHOUT LOGIN;");
				connection.ExecuteNonQuery($"CREATE SCHEMA [{fakeSchema}] AUTHORIZATION [{fakeUser}];");
				connection.ExecuteNonQuery($"CREATE TABLE [{fakeSchema}].[{fakeTable}] (Col1 CHAR(1) NOT NULL CONSTRAINT [TestPk] PRIMARY KEY);");
				connection.ExecuteNonQuery($"CREATE FULLTEXT CATALOG [{fakeFullTextCatalog}] AS DEFAULT AUTHORIZATION [{fakeUser}];");
				connection.ExecuteNonQuery($@"
					if (FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1)
					begin
						CREATE FULLTEXT INDEX ON [{fakeSchema}].[{fakeTable}] (Col1) KEY INDEX [TestPk] ON [{fakeFullTextCatalog}];
					end");
				connection.ExecuteNonQuery($"CREATE ROLE [{cwRole}];");
				connection.ExecuteNonQuery($"CREATE ROLE [{fakeRole}];");
				connection.ExecuteNonQuery($"ALTER ROLE [{fakeRole}] ADD MEMBER [{cwRole}];");
				connection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::[{fakeUser}] TO [{cwRole}];");
				connection.ExecuteNonQuery($"GRANT CREATE TABLE TO [{fakeUser}];");

				// Act
				// Assert
				CombineAssertions(() =>
				{
					var log = "";
					AssertNoExceptionThrown(() => log = testSecurity.CleanUpDatabaseUsers_Exposed(connection, userRepositoryDbName));

					DbSecurityTest.AssertOutputLog(log, new[]
					{
						$"REMOVING DATABASE_ROLE [{fakeRole}] FROM DATABASE [{userRepositoryDbName}]",
						$"Failed to remove DATABASE_ROLE [{fakeRole}] from database [{userRepositoryDbName}] due to: The role has members. It must be empty before it can be dropped.",
						$"REMOVING SQL_USER [{fakeUser}] FROM DATABASE [{userRepositoryDbName}]",
						$"Failed to remove SQL_USER [{fakeUser}] from database [{userRepositoryDbName}] due to: The database principal owns a schema in the database, and cannot be dropped.",
					});

					SqlSecurityUtils.AssertDbUserExists(connection, userRepositoryDbName, fakeUser, expected: true);
					AssertSchemaExists(connection, fakeSchema, expected: true);
					AssertSchemaOwner(connection, schemaName: fakeSchema, expected: fakeUser);
					AssertTableExists(connection, schemaName: fakeSchema, tableName: fakeTable, expected: true);
					AssertFullTextCatalogExists(connection, fakeFullTextCatalog, expected: true);
					AssertFullTextCatalogOwner(connection, catalogName: fakeFullTextCatalog, expected: fakeUser);
					SqlSecurityUtils.AssertDbRoleExists(connection, cwRole, expected: true);
					SqlSecurityUtils.AssertDbRoleExists(connection, fakeRole, expected: true);
					AssertRoleContains(connection, roleName: fakeRole, memberName: cwRole, expected: true);
					AssertGrantorPermissionExists(connection, grantor: fakeUser, stateName: "GRANT", permission: "IMPERSONATE", grantee: cwRole, expected: true);
					AssertGranteePermissionExists(connection, stateName: "GRANT", className: "DATABASE", permission: "CREATE TABLE", grantee: fakeUser, expected: true);
				});
			}
		}

		public void TestCleanUpDatabaseUsersInUserRepository_Succeeded()
		{
			// Arrange
			var mainDb = "DsaMainDb";
			var userRepositoryDbName = mainDb + DbUserRepository.RepositoryDbSuffix;
			var cwRole = DbRoleTypes.CwUnrestrictedWriterRole;
			var fakeUser = "FakeUser";
			var fakeRole = "FakeRole";
			var testSecurity = new DbSecurityForTest() { IsUserRepository_ForTest = true };

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, userRepositoryDbName, Db.DatabaseName))
			using (((ICurrentDbControl)connection).UseDatabase(userRepositoryDbName))
			{
				connection.ExecuteNonQuery($"CREATE USER [{fakeUser}] WITHOUT LOGIN;");
				connection.ExecuteNonQuery($"CREATE ROLE [{cwRole}];");
				connection.ExecuteNonQuery($"CREATE ROLE [{fakeRole}];");
				connection.ExecuteNonQuery($"GRANT CREATE TABLE TO [{fakeUser}];");

				// Act
				// Assert
				CombineAssertions(() =>
				{
					var log = "";
					AssertNoExceptionThrown(() => log = testSecurity.CleanUpDatabaseUsers_Exposed(connection, userRepositoryDbName));

					DbSecurityTest.AssertOutputLog(log, new[]
					{
						$"REMOVING DATABASE_ROLE [{fakeRole}] FROM DATABASE [{userRepositoryDbName}]",
						$"REMOVING SQL_USER [{fakeUser}] FROM DATABASE [{userRepositoryDbName}]",
					});

					SqlSecurityUtils.AssertDbUserExists(connection, userRepositoryDbName, fakeUser, expected: false);
					SqlSecurityUtils.AssertDbRoleExists(connection, cwRole, expected: true);
					SqlSecurityUtils.AssertDbRoleExists(connection, fakeRole, expected: false);
				});
			}
		}

		void AssertSchemaExists(AdminConnection connection, string schemaName, bool expected)
		{
			var actual = connection.Exists(@"
FROM
	sys.schemas
WHERE 1=1
	AND name = @schemaName

"
				, cmd =>
				{
					cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				});

			AssertEquals($"Does schema [{schemaName}] exist?", expected, actual);
		}

		void AssertSchemaOwner(AdminConnection connection, string schemaName, string expected)
		{
			var actual = connection.ExecuteScalar<string>(@"
SELECT
	owner = ISNULL(MAX(USER_NAME(principal_id)), '')
FROM
	sys.schemas
WHERE 1=1
	AND name = @schemaName

"
				, cmd =>
				{
					cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				});

			AssertEquals("Schema owner", expected, actual);
		}

		void AssertTableExists(AdminConnection connection, string schemaName, string tableName, bool expected)
		{
			var actual = connection.Exists(@"
FROM
	sys.objects
WHERE 1=1
	AND type = 'U'
	AND schema_id = SCHEMA_ID(@schemaName)
	AND name = @tableName

"
				, cmd =>
				{
					cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
					cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				});

			AssertEquals($"Does table [{schemaName}].[{tableName}] exist?", expected, actual);
		}

		void AssertFullTextCatalogExists(AdminConnection connection, string catalogName, bool expected)
		{
			var actual = connection.Exists(@"
FROM
	sys.fulltext_catalogs
WHERE 1=1
	AND name = @catalogName

"
				, cmd =>
				{
					cmd.AddParameter("@catalogName", SqlDbType.NVarChar, 128, catalogName);
				});

			AssertEquals($"Does Full text catalog [{catalogName}] exist?", expected, actual);
		}

		void AssertFullTextCatalogOwner(AdminConnection connection, string catalogName, string expected)
		{
			var actual = connection.ExecuteScalar<string>(@"
SELECT
	owner = ISNULL(MAX(USER_NAME(principal_id)), '')
FROM
	sys.fulltext_catalogs
WHERE 1=1
	AND name = @catalogName

"
				, cmd =>
				{
					cmd.AddParameter("@catalogName", SqlDbType.NVarChar, 128, catalogName);
				});

			AssertEquals("Full text catalog owner", expected, actual);
		}

		void AssertRoleContains(AdminConnection connection, string roleName, string memberName, bool expected)
		{
			var actual = SqlSecurityUtils.DbRole.Contains(connection, roleName, memberName);

			AssertEquals($"Does role [{roleName}] contain member [{memberName}]?", expected, actual);
		}

		void AssertGrantorPermissionExists(AdminConnection connection, string grantor, string stateName, string permission, string grantee, bool expected)
		{
			var actual = connection.Exists(@"
FROM
	sys.database_permissions
WHERE 1=1
	AND state_desc = @stateName
	AND permission_name = @permission
	AND grantee_principal_id = DATABASE_PRINCIPAL_ID(@grantee)
	AND grantor_principal_id = DATABASE_PRINCIPAL_ID(@grantor)

"
				, cmd =>
				{
					cmd.AddParameter("@stateName", SqlDbType.NVarChar, 60, stateName);
					cmd.AddParameter("@permission", SqlDbType.NVarChar, 128, permission);
					cmd.AddParameter("@grantee", SqlDbType.NVarChar, 128, grantee);
					cmd.AddParameter("@grantor", SqlDbType.NVarChar, 128, grantor);
				});

			AssertEquals($"Does permission [{permission}] exist?", expected, actual);
		}

		void AssertGranteePermissionExists(AdminConnection connection, string stateName, string className, string permission, string grantee, bool expected)
		{
			var actual = connection.Exists(@"
FROM
	sys.database_permissions
WHERE 1=1
	AND state_desc = @stateName
	AND class_desc = @className
	AND permission_name = @permission
	AND grantee_principal_id = DATABASE_PRINCIPAL_ID(@grantee)

"
				, cmd =>
				{
					cmd.AddParameter("@stateName", SqlDbType.NVarChar, 60, stateName);
					cmd.AddParameter("@className", SqlDbType.NVarChar, 60, className);
					cmd.AddParameter("@permission", SqlDbType.NVarChar, 128, permission);
					cmd.AddParameter("@grantee", SqlDbType.NVarChar, 128, grantee);
				});

			AssertEquals($"Does permission [{permission}] exist?", expected, actual);
		}
	}

	public class DbSecurityForTestingAlwaysOnAgroupPermissions : DbSecurityLockDown
	{
		internal StringBuilder ExecutedCommands = new StringBuilder();

		public string CleanUpServerLevelPermissions_Exposed(AdminConnection connection)
		{
			var logBuilder = new StringBuilder();
			base.CleanUpServerLevelPermissions(connection, logBuilder);
			return logBuilder.ToString();
		}

		internal override void ExecuteCleanupPermissionsInSqlTryCatch(AdminConnection connection, string stmt)
		{
			ExecutedCommands.AppendLine(stmt);
		}

		internal override string GetSelectCurrentPermissionsToUpdateSqlCommand()
		{
			return @"
Select 'ALTER' serverPermission, 'G' permissionState, 'EnterpriseDbUser_DbName_LoginName' loginName, 'SQL_LOGIN' loginType, 'GrpName2' targetName, 'AVAILABILITY GROUP' targetType UNION ALL
Select 'ALTER', 'G', 'Domain\User', 'WINDOWS_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'ALTER', 'G', 'Domain\AnotherUserUser', 'WINDOWS_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'CONTROL', 'G', 'EnterpriseDbUser_DbName_LoginName', 'SQL_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'CONTROL', 'G', 'Domain\User', 'WINDOWS_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'VIEW DEFINITION', 'G', 'Domain\AnotherUserUser', 'WINDOWS_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'TAKE OWNERSHIP', 'G', 'Domain\User', 'WINDOWS_LOGIN', 'GrpName1', 'AVAILABILITY GROUP' UNION ALL
Select 'VIEW ANY DATABASE', 'G', 'ASqlLoginName', 'SQL_LOGIN', null, null UNION ALL
Select 'VIEW ANY DATABASE', 'G', 'Domain\AnotherUserUser', 'WINDOWS_LOGIN', null, null UNION ALL
Select 'ALTER', 'G', 'ASqlLoginName', 'SQL_LOGIN', 'Hadr_endpoint', 'ENDPOINT' UNION ALL
Select 'ALTER', 'G', 'Domain\AnotherUserUser', 'WINDOWS_LOGIN', 'Hadr_endpoint', 'ENDPOINT'
";
		}
	}

	public class DbSecurityForTest : DbSecurityLockDown
	{
		public IList<ServerRoleMembership> GetDisallowedServerRoleMembers_Exposed(AdminConnection connection, bool allowDbCreator = false)
		{
			return base.GetDisallowedServerRoleMembers(connection, allowDbCreator);
		}

		public string DisableServerDdlTriggers_Exposed(AdminConnection connection)
		{
			var logBuilder = new StringBuilder();
			base.DisableServerDdlTriggers(connection, logBuilder);
			return logBuilder.ToString();
		}

		public string DisableSqlAgentJobs_Exposed(AdminConnection connection)
		{
			var logBuilder = new StringBuilder();
			base.DisableSqlAgentJobs(connection, logBuilder);
			return logBuilder.ToString();
		}

		public string CleanUpServerRoles_Exposed(AdminConnection connection)
		{
			var logBuilder = new StringBuilder();
			base.CleanUpServerRoles(connection, logBuilder);
			return logBuilder.ToString();
		}

		public string CleanUpServerLevelPermissions_Exposed(AdminConnection connection)
		{
			var logBuilder = new StringBuilder();
			base.CleanUpServerLevelPermissions(connection, logBuilder);
			return logBuilder.ToString();
		}

		public string DisableDatabaseDdlTriggers_Exposed(AdminConnection connection, params string[] dbList)
		{
			var logBuilder = new StringBuilder();

			foreach (var dbName in dbList)
			{
				DisableDatabaseDdlTriggers(connection, dbName, logBuilder);
			}

			return logBuilder.ToString();
		}

		public string CleanUpDatabaseLevelRoleMembership_Exposed(AdminConnection connection, params string[] dbList)
		{
			var logBuilder = new StringBuilder();

			foreach (var dbName in dbList)
			{
				CleanUpDatabaseLevelRoleMembership(connection, dbName, logBuilder);
			}

			return logBuilder.ToString();
		}

		public string CleanUpDatabaseLevelPermissions_Exposed(AdminConnection connection, params string[] dbList)
		{
			var logBuilder = new StringBuilder();

			foreach (var dbName in dbList)
			{
				CleanUpDatabaseLevelPermissions(connection, dbName, logBuilder);
			}

			return logBuilder.ToString();
		}

		public string CleanUpDbRightsFromUnusedRefDBSafe_Exposed(AdminConnection connection, string mainDbName)
		{
			var logBuilder = new StringBuilder();
			CleanUpDbRightsFromUnusedRefDBSafe(connection, mainDbName, logBuilder);

			return logBuilder.ToString();
		}

		public string CleanUpDatabaseUsers_Exposed(AdminConnection connection, params string[] dbList)
		{
			var logBuilder = new StringBuilder();
			var staffLoginList = GetStaffLoginList(connection, Db.DatabaseName);
			var staffUserLoginPrefix = DbUserRepository.GetStaffDbLoginFullPrefix(dbList[0]);
			var staffUserLoginLikePattern = DataUtils.ReplaceSqlLikeWildcard(staffUserLoginPrefix);

			foreach (var dbName in dbList)
			{
				CleanUpDatabaseUsers(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);
			}

			return logBuilder.ToString();
		}

		public List<GlbStaff> GetStaffLoginList_Exposed(AdminConnection connection, string mainDbName)
		{
			return GetStaffLoginList(connection, mainDbName);
		}

		public string GetSqlAgentServiceAccountName_Exposed(DbConnection connection)
		{
			return base.GetSqlAgentServiceAccountName(connection);
		}

		protected override bool IsSharedDatabase(string dbName) => IsSharedDatabase_Override ?? base.IsSharedDatabase(dbName);

		public bool? IsSharedDatabase_Override { get; set; }

		protected override bool IsUserRepository(string mainDbName, string dbName) => IsUserRepository_ForTest ?? base.IsUserRepository(mainDbName, dbName);

		public bool? IsUserRepository_ForTest { get; set; }
	}

	public class DbSecurityLockDownWithAdIntegrationsTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped()
		{
			#region Test Setup

			const string domainCredentials = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfDomainCredentials xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<DomainCredentials>
		<DomainName>sand.wtg.zone</DomainName>
		<DomainUserName>sand\ADTest_Admin</DomainUserName>
		<DomainUserPassword>Cqe+Qog2s8WjLEzx6xX/vfC+zMVVF+PxJB0lFuy0eKz/+67g3784MZPxBM5IXYin</DomainUserPassword>
		<IsDefaultDomain>Y</IsDefaultDomain>
		<UserOrganisationalUnit>root/Accounts/ADUnitTesting</UserOrganisationalUnit>
		<GroupOrganisationalUnit />
		<DefaultPassword>Changeme1234</DefaultPassword>
	</DomainCredentials>
</ArrayOfDomainCredentials>";

			var insertStmDataDomainCredentials = Invariant($@"
UPDATE dbo.StmData SET
	SD_BinaryValue = @domainCredentials
WHERE 1=1
	AND SD_Name = 'DomainCredentialsCollection'
	AND SD_Owner is NULL
	AND SD_DepartmentGuid is NULL

IF (@@rowcount = 0)
BEGIN
	INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_BinaryValue)
		VALUES (NEWID(), 'DomainCredentialsCollection', NULL, NULL, @domainCredentials)
END
");

			var adUsers = new List<(string name, string samAccountName, string adObjectGuid)>
			{
				(ADTestUserLongNameB.Name, ADTestUserLongNameB.NameWithDomainPreWindows2000, ADTestUserLongNameB.Guid),
				(ADTestUserLongNameC.Name, ADTestUserLongNameC.NameWithDomainPreWindows2000, ADTestUserLongNameC.Guid),
			};

			var createStaffLoginAndDbUserScriptBuilder = new StringBuilder();
			var dropStaffAndDbUsersScriptBuilder = new StringBuilder();

			var n = 0;
			var staffDbUsers = new SortedSet<string>();
			foreach (var (name, sqlLoginCreatedFromWindows, adObjectGuid) in adUsers)
			{
				var staffUserCode = Invariant($"#{++n:D2}");
				var staffDbUserName = sqlLoginCreatedFromWindows;

				staffDbUsers.Add(staffDbUserName);

				dropStaffAndDbUsersScriptBuilder.AppendLine(Invariant($@"
DELETE FROM dbo.GlbStaff WHERE GS_Code = '{staffUserCode}';
DELETE FROM dbo.GlbStaff WHERE GS_ActiveDirectoryObjectGuid = '{adObjectGuid}';
DROP USER IF EXISTS {staffDbUserName.QuoteName()};
IF EXISTS(SELECT NULL FROM sys.server_principals WHERE name = N'{sqlLoginCreatedFromWindows.QuoteEscapedName('\'')}') DROP LOGIN {sqlLoginCreatedFromWindows.QuoteName()};
"));

				var staffPK = Guid.NewGuid();

				createStaffLoginAndDbUserScriptBuilder.AppendLine(Invariant($@"
INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_DomainName, GS_ActiveDirectoryObjectGuid, GS_IsActive, GS_SystemCreateTimeUtc, Gs_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{staffPK}', '{staffUserCode}', '{name}', '{TestDomainName}', '{adObjectGuid}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES (NEWID(), '{GlbGroup.DbDeveloperGroupPK}', '{staffPK}');
CREATE LOGIN {sqlLoginCreatedFromWindows.QuoteName()} FROM WINDOWS;
CREATE USER {staffDbUserName.QuoteName()} FOR LOGIN {sqlLoginCreatedFromWindows.QuoteName()};
"));
			}

			try
			{
				using (var connection = Db.NewAdminConnection(Db.DatabaseName))
				{
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{dropStaffAndDbUsersScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{createStaffLoginAndDbUserScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture

					connection.ExecuteNonQuery(insertStmDataDomainCredentials, command =>
					{
						command.AddParameterBasedOnDbColumn("@domainCredentials", Encoding.Unicode.GetBytes(domainCredentials), StmDataSchema.SD_BinaryValue);
					});
				}
			}
			catch (Exception exception)
			{
				Fail(exception.Message);
			}

			var getStaffDbUsersScript =
				Invariant($"select name from [{Db.DatabaseName}].sys.database_principals where name in ({string.Join(",", staffDbUsers.Select(x => x.QuoteName('\'')))})");

			#endregion

			var staffDbUsersBeforeLockDown = string.Empty;
			var staffDbUsersAfterLockDown = string.Empty;

			var adRegistryMock = new Mock<IADRegistry>();
			adRegistryMock.Setup(x => x.IsIntegrationEnabled).Returns(true);

			using (ObjectFactory.Substitute(adRegistryMock.Object))
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					staffDbUsersBeforeLockDown = string.Join(",", LoadGlbStaffDbUsers(getStaffDbUsersScript, connection));

					new DbSecurityLockDown().LockdownDatabaseLevelSecurity(connection);

					staffDbUsersAfterLockDown = string.Join(",", LoadGlbStaffDbUsers(getStaffDbUsersScript, connection));
				}
				catch (Exception exception)
				{
					Fail($"{exception}");
				}
				finally
				{
					connection.ExecuteNonQuery(Invariant($"EXEC [{Db.DatabaseName}]..sp_executesql N'{dropStaffAndDbUsersScriptBuilder.ToString().QuoteEscapedName('\'')}'")); // This class access systems tables and procedures not supported by ZArchitecture
				}
			}

			AssertEquals(
				Invariant($@"Staff db users should not have been cleaned up by DSA service task!
Before LockDown: {staffDbUsersBeforeLockDown},
After LockDown: {staffDbUsersAfterLockDown}"),
				staffDbUsersBeforeLockDown,
				staffDbUsersAfterLockDown);
		}

		static SortedSet<string> LoadGlbStaffDbUsers(string script, DbConnection connection)
		{
			var dbUsers = new SortedSet<string>();
			using (var cmd = connection.Command(script))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					dbUsers.Add((string)reader[0]);
				}
			}

			return dbUsers;
		}

		const string TestDomainName = "sand.wtg.zone";

		static class ADTestUserLongNameB
		{
			public const string Guid = "cf37d78c-4d08-44a4-a0df-1bc92be23d99";
			public const string Name = "12345678901234567890B";
			public const string NameWithDomain = "12345678901234567890B@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567802";
			public const string Password = "Changeme1234";
		}

		static class ADTestUserLongNameC
		{
			public const string Guid = "60003917-fe84-4961-b196-65bd8764276b";
			public const string Name = "12345678901234567890C";
			public const string NameWithDomain = "12345678901234567890C@sand.wtg.zone";
			public const string NameWithDomainPreWindows2000 = "sand\\12345678901234567803";
			public const string Password = "Changeme1234";
		}
	}
}
