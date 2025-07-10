using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.DataProtection;
using CargoWise.DataProtection.TestFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DbSecurityServerLevelTest : TransactionedTestCase
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
				RestrictedWriterLoginCredentials.UserNameFor("AnotherDb"),
				RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
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
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor("AnotherDb"), true);
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), true);
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
				string.Format(CultureInfo.InvariantCulture, "REVOKED [bulkadmin] FROM [{0}] SQL_LOGIN", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)),
				string.Format(CultureInfo.InvariantCulture, "REVOKED [bulkadmin] FROM [{0}] SQL_LOGIN", RestrictedWriterLoginCredentials.UserNameFor("AnotherDb")),
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
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor("AnotherDb"), false);
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), false);
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
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor("AnotherDb"), false);
			AssertServerRoleMembership("bulkadmin", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), false);
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
}
