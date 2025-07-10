using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.Data.Testing;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class DbSecurityAdminConnectionTest : TestCase
	{
		#region Refresh Database Reader Role

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

		public void TestRefreshSchemaDbRoles_CreateNewRoles()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, "hrm");
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
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "SELECT");
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "INSERT", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "UPDATE", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "DELETE", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "EXECUTE", false);

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

			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
		}

		public void TestRefreshSchemaDbRoles_RefreshExistingRoles()
		{
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwHRMStaffRole");
			DbSecurityLockDownTest.DropRoleOnDatabase(TestConnection, Db.DatabaseName, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, "hrm");
			DbSecurityLockDownTest.CreateSchema(TestConnection, Db.DatabaseName, "cdc");

			DbSecurityLockDownTest.CreateRole(TestConnection, "cwRestrictedReaderRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwRestrictedWriterRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwHRMStaffRole");
			DbSecurityLockDownTest.CreateRole(TestConnection, "cwUnrestrictedWriterRole");

			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedReaderRole", "dbo", "INSERT");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwRestrictedWriterRole", "hrm", "CONTROL");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwHRMStaffRole", "dbo", "ALTER");
			DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(TestConnection, "cwUnrestrictedWriterRole", "hrm", "VIEW DEFINITION");

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

			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwRestrictedWriterRole", "CONTROL", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "dbo", "cwHRMStaffRole", "ALTER", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwUnrestrictedWriterRole", "VIEW DEFINITION", false);

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
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "SELECT");
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "INSERT", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "UPDATE", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "DELETE", false);
			DbSecurityLockDownTest.AssertRoleSchemaPermission(TestConnection, Db.DatabaseName, "hrm", "cwHRMStaffRole", "EXECUTE", false);

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

			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "SELECT", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "INSERT", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "UPDATE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "DELETE", "cwUnrestrictedWriterRole", null, null, "GRANT");
			DbSecurityLockDownTest.AssertDatabasePermission(TestConnection, Db.DatabaseName, "EXECUTE", "cwUnrestrictedWriterRole", null, null, "GRANT");
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
}
