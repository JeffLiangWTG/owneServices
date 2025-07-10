using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.SqlSecurity;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UserRepositoryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetMainDbObjects()
		{
			var mainDbObjects = (List<UserRepository.IDatabaseObject>)userRepository.GetMainDbObjects();
			AssertEquals("Main database should have views and functions.", true, mainDbObjects.Count > 0);

			var someView = mainDbObjects.Find((o) => o.ObjectName == "vw_FkReferences" && o.ObjectType == "VIEW");
			AssertNotNull("vw_FkReferences should exist.", someView);

			var someScalarFunction = mainDbObjects.Find((o) => o.ObjectName == "csfn_GetAddInfoValueFromCodeInline" && o.ObjectType == "TABLE_FUNCTION");
			AssertNotNull("csfn_GetAddInfoValueFromCodeInline should exist.", someScalarFunction);

			var someTableFunction = mainDbObjects.Find((o) => o.ObjectName == "OrgTablesToMerge" && o.ObjectType == "TABLE_FUNCTION");
			AssertNotNull("OrgTablesToMerge should exist.", someTableFunction);

			var someProcedure = mainDbObjects.Find((o) => o.ObjectName == "ep_BackupDb" && o.ObjectType == "SQL_STORED_PROCEDURE");
			AssertNull("ep_BackupDb should not be found.", someProcedure);

			var someNonExistingObject = mainDbObjects.Find((o) => o.ObjectName == "~some~non~existing~object~");
			AssertNull("~some~non~existing~object~ should not exist.", someNonExistingObject);
		}

		BusinessObjectFactory Factory => userRepository.Factory;

		void EnsureTestUserAndUserRepository(Action action)
		{
			EnsureTestUserAndUserRepository(action, true);
		}

		void EnsureTestUserAndUserRepository(Action action, bool isDeveloper)
		{
			GlbStaff testUser = CreateTestUser(isDeveloper);

			try
			{
				using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					action();
				}
			}
			finally
			{
				RemoveTestUser(testUser);
			}
		}

		GlbStaff CreateTestUser(bool isDeveloper)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TestUser";
			staff.GS_Code = "TSU";
			staff.IsDatabaseDeveloper = isDeveloper;
			staff.IsReadOnlyDBUser = true;
			staff.StaffPlainTextPassword = "1234";
			if (isDeveloper)
			{
				new DbUserManager().SetPasswordForStaff(staff, sqlPassword);
			}

			Factory.Save();

			return staff;
		}

		void RemoveTestUser(GlbStaff staff)
		{
			staff.Delete();
			Factory.Save();
		}

		[UseSnapshotProtection]
		public void TestCreateObjectDoesNotExecuteAndShowsErrorMessageIfUserNonDatabaseDeveloper()
		{
			AssertNotEquals("'TestUser' is not defined as a Database Developer. Please ensure you are set as a Database Developer in your staff profile. Contact your administrator if you don't have access to change it.", UnitTestUserNotification.Instance.LastMessage.Text);
			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute("CREATE VIEW vw_NonDbDev AS SELECT 1 AS Col");
			}, false);

			AssertEquals("'TestUser' is not defined as a Database Developer. Please ensure you are set as a Database Developer in your staff profile. Contact your administrator if you don't have access to change it.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertUserRepositoryObjectExists("vw_NonDbDev", "VIEW", false);
		}

		[UseSnapshotProtection]
		public void TestCreateObjectDoesNotExecuteAndShowsErrorMessageIfNoUserRepository()
		{
			AssertNotEquals("The User Repository Database does not exist. Please untick and re-tick your Database Reader (and Developer) access in your staff profile. Contact your administrator if you don't have access to change it.", UnitTestUserNotification.Instance.LastMessage.Text);
			EnsureTestUserAndUserRepository(() =>
			{
				DbUserRepositoryTest.DropUserRepositoryDatabase();
				userRepository.Execute("CREATE VIEW vw_NoUserRepo AS SELECT 1 AS Col");
			});

			AssertEquals("The User Repository Database does not exist. Please untick and re-tick your Database Reader (and Developer) access in your staff profile. Contact your administrator if you don't have access to change it.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertUserRepositoryObjectExists("vw_NoUserRepo", "VIEW", false);
		}

		[UseSnapshotProtection]
		public void TestGetUserRepositoryDbObjects()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				DbUserRepositoryTest.DropUserRepositoryDatabase();
				AssertDatabaseExists(DbUserManager.UserRepositoryDb, false);
			});

			EnsureTestUserAndUserRepository(() =>
			{
				var userRepositoryObjects = GetUserRepositoryDbObjects();
				AssertEquals("User Repository object count", 0, userRepositoryObjects.Count);

				userRepository.Execute("CREATE VIEW TestView AS SELECT 1 AS Col");

				userRepositoryObjects = GetUserRepositoryDbObjects();
				AssertEquals("User Repository object count", 1, userRepositoryObjects.Count);
				AssertEquals("ObjectName[0]", "TestView", userRepositoryObjects[0].ObjectName);
				AssertEquals("ObjectType[0]", "VIEW", userRepositoryObjects[0].ObjectType);

				userRepository.Execute("CREATE FUNCTION TestFunction() RETURNS INT AS BEGIN RETURN 2 END");
				userRepository.Execute("CREATE PROC TestProcedure AS BEGIN RETURN 0 END");
				userRepository.Execute("CREATE TABLE TestTable ( Col INT )");

				userRepositoryObjects = GetUserRepositoryDbObjects();
				AssertEquals("User Repository object count", 4, userRepositoryObjects.Count);
				AssertEquals("ObjectName[0]", "TestFunction", userRepositoryObjects[0].ObjectName);
				AssertEquals("ObjectType[0]", "SCALAR_FUNCTION", userRepositoryObjects[0].ObjectType);
				AssertEquals("ObjectName[1]", "TestProcedure", userRepositoryObjects[1].ObjectName);
				AssertEquals("ObjectType[1]", "PROCEDURE", userRepositoryObjects[1].ObjectType);
				AssertEquals("ObjectName[2]", "TestTable", userRepositoryObjects[2].ObjectName);
				AssertEquals("ObjectType[2]", "USER_TABLE", userRepositoryObjects[2].ObjectType);
				AssertEquals("ObjectName[3]", "TestView", userRepositoryObjects[3].ObjectName);
				AssertEquals("ObjectType[3]", "VIEW", userRepositoryObjects[3].ObjectType);
			});
		}

		[UseSnapshotProtection]
		public void TestCreateObjectReferencingClrFunction()
		{
			const string testObjName = "TestCreateOrReplaceUserRepositoryDatabaseObjectReferencingClrFunction";
			string createSql = String.Format("CREATE PROCEDURE {0} AS SELECT dbo.CLRCssvAgg('x')", testObjName);

			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute(createSql);
				var testObj = AssertUserRepositoryObjectExists(testObjName, "PROCEDURE", true);
				AssertEquals("Test object definition", createSql, testObj.GetCreateScript());
			});
		}

		[UseSnapshotProtection]
		public void TestCreateObjectShowsFriendlyMessageIfNameConflictsWithExistingSynonym()
		{
			try
			{
				EnsureTestUserAndUserRepository(() =>
				{
					userRepository.Execute("CREATE VIEW vw_FkReferences AS SELECT 1 AS Col");
					Fail("Should have thrown an exception");
				});
			}
			catch (Exception ex)
			{
				AssertEquals("Caught exception message",
					"There is already an object named 'vw_FkReferences' in the database. Note that names used in the main database are reserved, and cannot be used in the user repository.",
					ex.Message);
			}
		}

		[UseSnapshotProtection]
		public void TestCreateObjectEnsuresRepositoryDatabaseExists()
		{
			const string testObjName = "TestCreateObjectEnsuresRepositoryDatabaseExists";

			EnsureTestUserAndUserRepository(() =>
			{
				DbUserRepositoryTest.DropUserRepositoryDatabase();
				AssertDatabaseExists(DbUserManager.UserRepositoryDb, false);
			});

			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute(String.Format("CREATE VIEW {0} AS SELECT 1 AS Col", testObjName));

				AssertDatabaseExists(DbUserManager.UserRepositoryDb, true);
			});
			string sqlText = String.Format("SELECT count(*) FROM [{0}].sys.synonyms", DbUserManager.UserRepositoryDb);
			AssertEquals("Synonyms created?", true, (int)Db.Connection.ExecuteScalar(sqlText) > 0);

			var userRepositoryObjects = GetUserRepositoryDbObjects();
			AssertEquals("User Repository object count", 1, userRepositoryObjects.Count);
			AssertEquals("ObjectName[0]", testObjName, userRepositoryObjects[0].ObjectName);
			AssertEquals("ObjectType[0]", "VIEW", userRepositoryObjects[0].ObjectType);
		}

		[UseSnapshotProtection]
		public void TestDropObject()
		{
			const string testObjName = "TestDropObject";
			string createSql = string.Empty;
			UserRepository.IUserRepositoryDatabaseObject testObj;

			EnsureTestUserAndUserRepository(() =>
			{
				createSql = String.Format("CREATE VIEW {0} AS SELECT 1 AS Col", testObjName);
				userRepository.Execute(createSql);
				testObj = AssertUserRepositoryObjectExists(testObjName, "VIEW", true);
				testObj.Drop();
				AssertUserRepositoryObjectExists(testObjName, "VIEW", false);
			});
			EnsureTestUserAndUserRepository(() =>
			{
				createSql = String.Format("CREATE FUNCTION {0}() RETURNS TABLE AS RETURN SELECT 1 AS Col", testObjName);
				userRepository.Execute(createSql);
				testObj = AssertUserRepositoryObjectExists(testObjName, "TABLE_FUNCTION", true);
				testObj.Drop();
				AssertUserRepositoryObjectExists(testObjName, "TABLE_FUNCTION", false);
			});

			EnsureTestUserAndUserRepository(() =>
			{
				createSql = String.Format("CREATE PROCEDURE {0}(@Param int) AS RETURN -1", testObjName);
				userRepository.Execute(createSql);
				testObj = AssertUserRepositoryObjectExists(testObjName, "PROCEDURE", true);
				testObj.Drop();
				AssertUserRepositoryObjectExists(testObjName, "PROCEDURE", false);
			});

			EnsureTestUserAndUserRepository(() =>
			{
				createSql = String.Format("CREATE TABLE {0} ( Col INT )", testObjName);
				userRepository.Execute(createSql);
				testObj = AssertUserRepositoryObjectExists(testObjName, "USER_TABLE", true);
				testObj.Drop();
				AssertUserRepositoryObjectExists(testObjName, "USER_TABLE", false);
			});
		}

		[UseSnapshotProtection]
		public void TestPermissionsPreventUserModifyingOtherDatabases()
		{
			var exceptionThrowingSqlScripts = new string[]
				{
					"USE {0}; CREATE TABLE InjecterWasHere ( Col INT );",
					"CREATE TABLE {0}.dbo.InjecterWasHere ( Col INT );",
					"EXEC [{0}]..sp_executesql N'CREATE TABLE {0}.dbo.InjecterWasHere ( Col INT )'"
				};

			foreach (var sql in exceptionThrowingSqlScripts)
			{
				try
				{
					EnsureTestUserAndUserRepository(() =>
					{
						userRepository.Execute(String.Format(sql, Db.DatabaseName));
						Fail(String.Format("The following SQL shown have thrown an exception:\r\n{0}", sql));
					});
				}
				catch (Exception ex)
				{
					Assert(ex.Message.EndsWith("Note that you only have permissions on the user repository database."));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSelectInsertUpdateDeletePermissions()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute("CREATE TABLE TestTable ( Col INT );");
				userRepository.Execute("INSERT INTO TestTable VALUES (1);");
				userRepository.Execute("INSERT INTO TestTable VALUES (2);");
				userRepository.Execute("INSERT INTO TestTable VALUES (3);");
				userRepository.Execute("UPDATE TestTable SET Col = 4 WHERE Col = 3;");
				userRepository.Execute("DELETE FROM TestTable WHERE Col = 2;");
				userRepository.Execute("SELECT * FROM TestTable;");

				var results = new List<int>();
				using (var command = Db.Connection.Command(String.Format("SELECT * FROM {0}.dbo.TestTable;", DbUserManager.UserRepositoryDb)))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add((int)reader["Col"]);
					}
				}
				AssertEquals("Should have selected 2 rows.", 2, results.Count);
				AssertEquals(1, results[0]);
				AssertEquals(4, results[1]);
			});
		}

		[UseSnapshotProtection]
		public void TestReferencesPermission()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute(@"
CREATE TABLE TestTable01 ( PKCol INT NOT NULL, CONSTRAINT CONSTRAINT_PK PRIMARY KEY ( PKCol ) );
CREATE TABLE TestTable02 ( FKCol INT NULL );
ALTER TABLE dbo.TestTable02 ADD CONSTRAINT FK_2_TestTable01 FOREIGN KEY (FKCol) REFERENCES dbo.TestTable01 (PKCol);
				");
				var objects = GetUserRepositoryDbObjects();

				AssertEquals(
	@"CREATE TABLE [dbo].[TestTable01]
(
	  [PKCol] INT NOT NULL
	, CONSTRAINT [CONSTRAINT_PK] PRIMARY KEY ([PKCol] ASC)
)
",
					objects[0].GetCreateScript());
				AssertEquals(
	@"CREATE TABLE [dbo].[TestTable02]
(
	  [FKCol] INT NULL
)

ALTER TABLE [dbo].[TestTable02] WITH CHECK ADD CONSTRAINT [FK_2_TestTable01] FOREIGN KEY([FKCol]) REFERENCES [dbo].[TestTable01] ([PKCol])
ALTER TABLE [dbo].[TestTable02] CHECK CONSTRAINT [FK_2_TestTable01]
",
					objects[1].GetCreateScript());
			});
		}

		[UseSnapshotProtection]
		public void TestGetDatabaseObjectCreateScript()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute("CREATE PROCEDURE TestProcedure(@Param int) AS RETURN -1");
				var userRepositoryObject = AssertUserRepositoryObjectExists("TestProcedure", "PROCEDURE", true);
				AssertEquals("CREATE PROCEDURE TestProcedure(@Param int) AS RETURN -1", userRepositoryObject.GetCreateScript());

				userRepository.Execute("CREATE TABLE TestTable ( Col INT );");
				userRepositoryObject = AssertUserRepositoryObjectExists("TestTable", "USER_TABLE", true);
				AssertEquals("CREATE TABLE [dbo].[TestTable]\r\n(\r\n\t  [Col] INT NULL\r\n)\r\n", userRepositoryObject.GetCreateScript());

				userRepository.Execute("CREATE PROCEDURE TestProcedureWithEncryption(@Param int) WITH ENCRYPTION AS RETURN -1;");
				userRepositoryObject = AssertUserRepositoryObjectExists("TestProcedureWithEncryption", "PROCEDURE", true);
				AssertEquals("[TestProcedureWithEncryption] may not be retrievable due to insufficient access rights; the text is encrypted.", userRepositoryObject.GetCreateScript());

				userRepository.Execute("CREATE TYPE TestType AS TABLE (Col1 INT);");
				AssertUserRepositoryObjectExists("TestType", "USER_TYPE", true);
			});
		}

		void AssertDatabaseExists(string dbName, bool expectedValue)
		{
			AssertEquals("Database [" + dbName + "] exists?", expectedValue, Db.Connection.DatabaseExists(dbName));
		}

		[UseSnapshotProtection]
		public void TestSchemaObjectsCanBeCreated()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				DbUserRepositoryTest.DropUserRepositoryDatabase();
				AssertDatabaseExists(DbUserManager.UserRepositoryDb, false);
			});

			var userRepositoryObjects = GetUserRepositoryDbObjects();

			AssertEquals("User Repository object count", 0, userRepositoryObjects.Count);

			EnsureTestUserAndUserRepository(() =>
			{
				userRepository.Execute("Create Schema TestSchema");
				userRepository.Execute("CREATE VIEW TestSchema.TestView AS SELECT 1 AS Col");

				userRepositoryObjects = GetUserRepositoryDbObjects();
				AssertEquals("User Repository object count", 1, userRepositoryObjects.Count);
				AssertEquals("ObjectName[0]", "TestView", userRepositoryObjects[0].ObjectName);
				AssertEquals("ObjectType[0]", "VIEW", userRepositoryObjects[0].ObjectType);
				AssertEquals("ObjectSchema[0]", "TestSchema", userRepositoryObjects[0].ObjectSchema);

				userRepository.Execute("CREATE FUNCTION TestSchema.TestFunction() RETURNS INT AS BEGIN RETURN 2 END");
				userRepository.Execute("CREATE PROC TestSchema.TestProcedure AS BEGIN RETURN 0 END");
				userRepository.Execute("CREATE TABLE TestSchema.TestTable ( Col INT )");
				userRepository.Execute("CREATE TYPE TestSchema.TestType AS TABLE (Col1 INT)");

				userRepositoryObjects = GetUserRepositoryDbObjects();
				AssertEquals("User Repository object count", 5, userRepositoryObjects.Count);
				AssertEquals("ObjectName[0]", "TestSchema", userRepositoryObjects[0].ObjectSchema);
				AssertEquals("ObjectName[0]", "TestFunction", userRepositoryObjects[0].ObjectName);
				AssertEquals("ObjectType[0]", "SCALAR_FUNCTION", userRepositoryObjects[0].ObjectType);
				AssertEquals("ObjectName[1]", "TestProcedure", userRepositoryObjects[1].ObjectName);
				AssertEquals("ObjectType[1]", "PROCEDURE", userRepositoryObjects[1].ObjectType);
				AssertEquals("ObjectName[2]", "TestTable", userRepositoryObjects[2].ObjectName);
				AssertEquals("ObjectType[2]", "USER_TABLE", userRepositoryObjects[2].ObjectType);
				AssertEquals("ObjectName[3]", "TestType", userRepositoryObjects[3].ObjectName);
				AssertEquals("ObjectType[3]", "USER_TYPE", userRepositoryObjects[3].ObjectType);
				AssertEquals("ObjectName[4]", "TestView", userRepositoryObjects[4].ObjectName);
				AssertEquals("ObjectType[4]", "VIEW", userRepositoryObjects[4].ObjectType);

				string dropTestObjectsSql = @"
					DROP VIEW TestSchema.TestView;
					DROP FUNCTION TestSchema.TestFunction;
					DROP TABLE TestSchema.TestTable;
					DROP TYPE TestSchema.TestType;
					DROP PROCEDURE TestSchema.TestProcedure;
					DROP SCHEMA TestSchema;
				";
				userRepository.Execute(dropTestObjectsSql);
			});
		}

		[UseSnapshotProtection]
		public void TestViewAnyDatabaseDeniedToExecuteUser()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				using (var connection = userRepository.GetDbConnection())
				{
					userRepository.Execute("SELECT name INTO dbo.TestViewAnyDatabaseDeniedToExecuteUserTable FROM sys.databases");

					var dbList = DataUtils.GetListOfValuesFromQuery(
						connection,
						String.Format(
							CultureInfo.InvariantCulture,
							"SELECT * FROM [{0}].dbo.TestViewAnyDatabaseDeniedToExecuteUserTable;",
							DbUserManager.UserRepositoryDb)
					);

					var currentUser = connection.ExecuteScalar("select SYSTEM_USER").ToString();
					AssertEndsWith("LoginName matches", "TestUser", currentUser);
					AssertEquals("master database in the list", true, dbList.Contains(Db.SqlMasterDb));
					AssertEquals("tempdb database in the list", true, dbList.Contains("tempdb"));
					AssertEquals(DbUserManager.UserRepositoryDb + " database in the list", true, dbList.Contains(DbUserManager.UserRepositoryDb));
				}
			});
		}

		[UseSnapshotProtection]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestUseIntegratedSecurityDbConnection()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				var integrationMode = ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;

				try
				{
					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
					string loginName = GetLoginNameFromConnection();
					AssertStartsWith("Integrate mode should use windows account", @"CORP\", loginName);
				}
				finally
				{
					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = integrationMode;
				}
			});
		}

		[UseSnapshotProtection]
		public void TestUseNonIntegratedSecuirtyDbConnection()
		{
			EnsureTestUserAndUserRepository(() =>
			{
				var integrationMode = ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;
				try
				{
					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
					string loginName = GetLoginNameFromConnection();
					AssertStartsWith("Non-integrate mode should use sql account", DbUserRepository.StaffDbLoginPrefix, loginName);
				}
				finally
				{
					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = integrationMode;
				}
			});
		}

		[UseSnapshotProtection]
		public void TestValidateLoginWithSqlPasswordLengthLongerThan128WillReturnFalseAndNoUnhandledExceptionInNet48ButWillHaveExceptionInNetCore()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "DBdevTestLongPassword";
			staff.IsDatabaseDeveloper = true;
			Factory.Save();

			using (new TemporaryUserContext() { StaffLoginName = "DBdevTestLongPassword" }.Set())
			{
				var longSqlPassword = new string('1', 140);
				var userRepository = new UserRepository(new BusinessObjectFactory(), longSqlPassword);

#if NETFRAMEWORK
				var validateResult = true;
				AssertNoExceptionThrown(() => { validateResult = userRepository.ValidateLogin(out _); });
				AssertEquals("validate userRepository through password longer than 128 characters will return false", false, validateResult);
#elif NET
				AssertExceptionThrown<ArgumentException>(() => { userRepository.ValidateLogin(out _); });
#endif
			}
		}

		string GetLoginNameFromConnection()
		{
			var login = string.Empty;
			using (var connection = userRepository.GetDbConnection())
			{
				login = connection.ExecuteScalar($"SELECT login_name FROM sys.dm_exec_sessions WHERE session_id = {connection.SPID};").ToString();
			}

			return login;
		}

		UserRepository.IUserRepositoryDatabaseObject AssertUserRepositoryObjectExists(string objName, string objType, bool expectedValue)
		{
			var userRepositoryObjects = GetUserRepositoryDbObjects();
			var foundObject = userRepositoryObjects.Find((o) => o.ObjectName == objName && o.ObjectType == objType);
			AssertEquals(objName + " " + objType + " exists?", expectedValue, foundObject != null);
			return foundObject;
		}

		List<UserRepository.IUserRepositoryDatabaseObject> GetUserRepositoryDbObjects()
		{
			return userRepository.GetUserRepositoryDbObjects().Cast<UserRepository.IUserRepositoryDatabaseObject>().ToList();
		}

		IDisposable serviceTaskNudgerDisposable;
		Mock<IServiceTaskNudger> serviceTaskNudgerMock;

		protected override void SetUp()
		{
			base.SetUp();

			serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			_ = serviceTaskNudgerMock.Setup(nudger => nudger.NudgeServiceTask("DSA", null))
				.Callback(() =>
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						var manager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName, allowTransaction: true);
						manager.BuildSecurity(adminConnection, CancellationToken.None);
					}
				});

			serviceTaskNudgerDisposable = ObjectFactory.Substitute(serviceTaskNudgerMock.Object);
		}

		protected override void FinalTearDown()
		{
			serviceTaskNudgerDisposable?.Dispose();
			base.FinalTearDown();
		}

		protected override void TearDown()
		{
			CleanupTestRepositoryObjects();
			base.TearDown();
		}

		void CleanupTestRepositoryObjects()
		{
			var testObjects = GetUserRepositoryDbObjects().ToArray();

			for (int i = testObjects.Length - 1; i >= 0; i--)
			{
				testObjects[i].Drop();
			}
		}

		const string sqlPassword = "Sql1234";

		readonly UserRepository userRepository = new UserRepository(new BusinessObjectFactory(), sqlPassword);
	}
}
