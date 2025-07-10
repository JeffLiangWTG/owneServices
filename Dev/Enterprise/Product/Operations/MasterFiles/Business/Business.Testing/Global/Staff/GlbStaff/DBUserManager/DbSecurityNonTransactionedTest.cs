using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.SqlServer.Testing;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DbSecurityNonTransactionedTest : TestCase
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

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "hrm");
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
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "EXECUTE", false);

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

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "hrm");
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

						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "hrm");
						DbSecurityLockDownTest.CreateSchema(testAdminConnection, testMainDb, "cdc");

						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwRestrictedReaderRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwRestrictedWriterRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwHRMStaffRole");
						DbSecurityLockDownTest.CreateRole(testAdminConnection, "cwUnrestrictedWriterRole");

						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwRestrictedReaderRole", "dbo", "INSERT");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwRestrictedWriterRole", "hrm", "CONTROL");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwHRMStaffRole", "dbo", "ALTER");
						DbSecurityLockDownTest.GrantRoleSchemaPermissionOnDatabase(testAdminConnection, "cwUnrestrictedWriterRole", "hrm", "VIEW DEFINITION");

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

						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwRestrictedWriterRole", "CONTROL", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "dbo", "cwHRMStaffRole", "ALTER", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwUnrestrictedWriterRole", "VIEW DEFINITION", false);

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
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "SELECT");
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "INSERT", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "UPDATE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "DELETE", false);
						DbSecurityLockDownTest.AssertRoleSchemaPermission(testAdminConnection, testMainDb, "hrm", "cwHRMStaffRole", "EXECUTE", false);

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
}
