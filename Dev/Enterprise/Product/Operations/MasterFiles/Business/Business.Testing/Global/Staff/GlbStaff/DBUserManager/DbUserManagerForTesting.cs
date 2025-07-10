using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DbUserManagerForTesting : DbUserManager
	{
		public DbUserManagerForTesting()
		{
		}

		public DbUserManagerForTesting(ILogger logger)
			: base(logger)
		{
		}

		public static string GetFullUserLoginName(string loginName, Func<string> getDownLevelLogonName)
		{
			return new DbUserManager().GetFullUserLoginName_Exposed(loginName, getDownLevelLogonName, DatabaseAuthenticationMode.Sql);
		}

		public void DropDbLogin_Exposed(AdminConnection securityConnection, string staffLoginName, Func<string> getDownLevelLogonName, DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				GetDownLevelLogonName = getDownLevelLogonName
			};

			DropLogin_Exposed(securityConnection, info, databaseAuthenticationMode);
		}

		public void DropDbUsers_Exposed(AdminConnection securityConnection, string staffLoginName, Func<string> getDownLevelLogonName, DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				GetDownLevelLogonName = getDownLevelLogonName
			};

			DropDbUsers_Exposed(securityConnection, info, databaseAuthenticationMode);
		}

		public string CreateDbLogin_Exposed(
			AdminConnection securityConnection,
			string staffLoginName,
			ISet<string> databaseAccessGroupRoles,
			Func<string> getDownLevelLogonName,
			string hashedPwd = "",
			string sid = "",
			DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				StaffPK = Guid.NewGuid(),
				HashedPassword = hashedPwd,
				PasswordChanged = true,
				StaffDatabaseAccessGroupRoles = databaseAccessGroupRoles,
				DbPermissionChanged = true,
				GetDownLevelLogonName = getDownLevelLogonName,
				DbAuthenticationMode = databaseAuthenticationMode,
				Sid = sid
			};

			CreateLoginWithPermissions_Exposed(securityConnection, info, databaseAuthenticationMode);
			return info.LoginName;
		}

		public void CreateDbUsersAndManageRoles_Exposed(AdminConnection securityConnection, string staffLoginName, ISet<string> databaseAccessGroupRoles, Func<string> getDownLevelLogonName)
		{
			var info = new StaffLoginInfo()
			{
				LoginName = staffLoginName,
				StaffDatabaseAccessGroupRoles = databaseAccessGroupRoles,
				DbPermissionChanged = true,
				GetDownLevelLogonName = getDownLevelLogonName,
				DbAuthenticationMode = DatabaseAuthenticationMode.Sql
			};

			CreateDbUsersAndPermissions_Exposed(securityConnection, info);
			CreateBiDbUsersAndPermissions_Exposed(securityConnection, Db.AuditDatabaseName, info);
			CreateBiDbUsersAndPermissions_Exposed(securityConnection, Db.EdwDatabaseName, info);
		}

		public static bool CheckLoginExists(string staffLoginName, AdminConnection conn, bool readUncommitted = false)
		{
			string actualDbLogin = GetFullUserLoginName(staffLoginName, () => "");
			return CheckLoginExistsWithActualDatabaseLoginName(actualDbLogin, conn, readUncommitted);
		}

		public static string GetLoginDefaultLanguage(string staffLoginName, AdminConnection conn)
		{
			string actualDbLogin = GetFullUserLoginName(staffLoginName, () => "");
			var cmd = conn.Command("SELECT default_language_name FROM sys.server_principals WHERE name = @loginName COLLATE SQL_Latin1_General_CP1_CI_AS");
			cmd.AddParameter("@loginName", SqlDbType.NVarChar, 128, actualDbLogin);
			object languageObj = cmd.ExecuteScalar();
			return (languageObj == null || languageObj == DBNull.Value) ? null : languageObj.ToString();
		}

		public static bool CheckLoginExistsWithActualDatabaseLoginName(string actualDbLoginName, AdminConnection conn, bool readUncommitted = false)
		{
			string sqlText = string.Format(
				"SELECT default_database_name FROM sys.server_principals {0} WHERE name = @userLogin COLLATE SQL_Latin1_General_CP1_CI_AS",
				readUncommitted ? "WITH (NOLOCK)" : ""
			);

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, actualDbLoginName);
				object userDb = cmd.ExecuteScalar();
				return (userDb != null && userDb.ToString() == Db.DatabaseName);
			}
		}

		public static bool CheckLoginIsEnabled(string staffLoginName, AdminConnection conn)
		{
			var loginName = GetFullUserLoginName(staffLoginName, () => "");
			var sqlText = "SELECT is_disabled FROM sys.server_principals WHERE name = @name";
			object isDisabled = conn.ExecuteScalar(sqlText, p => p.AddParameter("@name", SqlDbType.NVarChar, 128, loginName));
			return !(isDisabled == null || (bool)isDisabled);
		}

		public static bool CheckUserHasRightsOnDb(string dbName, string staffLoginName, string userRole, DbConnection connection)
		{
			string userLogin = GetFullUserLoginName(staffLoginName, () => "");

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT CONVERT(bit,
	CASE
		WHEN EXISTS
			(
				SELECT NULL
				FROM
					[{0}].sys.database_principals        AS r
					JOIN [{0}].sys.database_role_members AS m ON m.role_principal_id = r.principal_id
					JOIN [{0}].sys.database_principals   AS u ON u.principal_id = m.member_principal_id
				WHERE 1=1
					AND r.type = 'R' -- Database role
					AND r.name = @userRole
					AND u.name = @userLogin
			) THEN 1
		ELSE 0
	END)
"
				, dbName // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userRole", SqlDbType.NVarChar, 128, userRole);
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, userLogin);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public static void DropUserRole(string dbName, string staffLoginName, string userRole, DbConnection connection)
		{
			var userLogin = GetFullUserLoginName(staffLoginName, () => "");

			var sql = $"EXEC {dbName}.sys.sp_droprolemember @userRole, @userLogin;";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userRole", SqlDbType.NVarChar, 128, userRole);
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, userLogin);

				cmd.ExecuteNonQuery();
			}
		}

		public static bool CheckUserHasSchemaOnDb(string dbName, string staffLoginName, DbConnection conn)
		{
			string userLogin = GetFullUserLoginName(staffLoginName, () => "");

			string sqlText = string.Format(@"
				SELECT count(*) FROM [{0}].sys.database_principals u
				INNER JOIN [{0}].sys.schemas s ON s.principal_id = u.principal_id
				WHERE u.name = @userLogin", dbName);

			using (var cmd = conn.Command(sqlText)) // This class access systems tables and procedures not supported by ZArchitecture
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, userLogin);
				int count = Convert.ToInt32(cmd.ExecuteScalar());
				return (count > 0);
			}
		}

		public static void CreateUserSchemaOnMainDb(string staffLoginName, AdminConnection conn)
		{
			string userLogin = GetFullUserLoginName(staffLoginName, () => "");

			string sqlText = string.Format("CREATE SCHEMA [{0}] AUTHORIZATION [{1}]", staffLoginName, userLogin);
			conn.ExecuteNonQuery(sqlText);
		}

		public static void CreateUserSchemaOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");
				conn.ExecuteNonQuery($"CREATE SCHEMA {staffLoginName.QuoteName()} AUTHORIZATION {userLogin.QuoteName()}");
			}
		}

		public static void CreateUserTypeOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");

				conn.ExecuteNonQuery(@$"
CREATE TYPE {staffLoginName.QuoteName()} FROM int;
ALTER AUTHORIZATION ON TYPE::{staffLoginName.QuoteName()} TO {userLogin.QuoteName()};
");
			}
		}

		public static void CreateUserViewOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");

				conn.ExecuteNonQuery($@"
CREATE VIEW {staffLoginName.QuoteName()} AS SELECT name FROM sys.objects;
");
				conn.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON OBJECT::{staffLoginName.QuoteName()} TO {userLogin.QuoteName()};
");
			}
		}

		public static void CreateUserTableOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");
				conn.ExecuteNonQuery($@"
CREATE TABLE {staffLoginName.QuoteName()} (col1 int);
ALTER AUTHORIZATION ON OBJECT::{staffLoginName.QuoteName()} TO {userLogin.QuoteName()};
");
			}
		}

		public static void CreateUserFunctionOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");

				conn.ExecuteNonQuery($@"
CREATE FUNCTION {staffLoginName.QuoteName()} () RETURNS int
BEGIN
	RETURN 0;
END
");
				conn.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON OBJECT::{staffLoginName.QuoteName()} TO {userLogin.QuoteName()};
");
			}
		}

		public static void CreateUserProcedureOnUserRepositoryDb(string staffLoginName, AdminConnection conn)
		{
			using (((ICurrentDbControl)conn).UseDatabase(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix))
			{
				var userLogin = GetFullUserLoginName(staffLoginName, () => "");

				conn.ExecuteNonQuery($@"
CREATE PROCEDURE {staffLoginName.QuoteName()} AS print 'ok';
");
				conn.ExecuteNonQuery($@"
ALTER AUTHORIZATION ON OBJECT::{staffLoginName.QuoteName()} TO {userLogin.QuoteName()};
");
			}
		}

		public static void DropUserSchemaOnMainDb(string staffLoginName, DbConnection conn)
		{
			string userLogin = GetFullUserLoginName(staffLoginName, () => "");

			string sqlText = string.Format("DROP SCHEMA [{0}]", staffLoginName);
			conn.ExecuteNonQuery(sqlText);
		}

		public static void CreateDummyTableForSchemaOnMainDb(string staffLoginName, DbConnection conn)
		{
			string sqlText = string.Format("CREATE TABLE [{0}].T1 (col1 int)", staffLoginName);
			conn.ExecuteNonQuery(sqlText);
		}

		public static void DropDummyTableForSchemaOnMainDb(string staffLoginName, DbConnection conn)
		{
			string sqlText = string.Format("DROP TABLE IF EXISTS [{0}].T1", staffLoginName);
			conn.ExecuteNonQuery(sqlText);
		}

		public HashSet<string> GetOnlineDatabaseList_Exposed(DbConnection connection)
		{
			return GetEnterpriseDbs_Exposed(connection);
		}

		internal bool DbSupportsWindowsAuthentication_Exposed(Func<string> getDownLevelLogonName)
		{
			return base.DbSupportsWindowsAuthentication(getDownLevelLogonName);
		}

		internal bool DbSupportsSqlAuthentication_Exposed()
		{
			return base.DbSupportsSqlAuthentication();
		}
	}
}
