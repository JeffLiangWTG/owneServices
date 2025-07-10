using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	#region SuppressResourceStringsCheckRegion
	public class DbSecurityLockDown : DbSecurity
	{
		#region SERVER LEVEL

		#region Server Lockdown

		/// <summary>
		/// Entry point for server level security lockdown.
		/// Ensures all server security checks and fixes are performed and are performed in the correct order.
		/// </summary>
		public string LockdownServerLevelSecurity(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var logBuilder = new StringBuilder();

			DisableServerDdlTriggers(connection, logBuilder);
			DisableSqlAgentJobs(connection, logBuilder);
			SetLockModeSqlServerSysadminPwd(connection);
			CleanUpServerRoles(connection, logBuilder);
			CleanUpServerLevelPermissions(connection, logBuilder);

			return logBuilder.ToString();
		}

		/// <summary>
		/// SERVER LEVEL DDL TRIGGERS
		/// Disables all server DDL triggers
		/// (at the moment no server DDL triggers are allowed since our program does not ship any of them)
		/// </summary>
		protected void DisableServerDdlTriggers(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var sql = @"
SELECT
	name,
	type_desc
FROM
	sys.server_triggers
WHERE
	is_ms_shipped = 0
	AND is_disabled = 0
";

			var cmdBuilder = new StringBuilder();

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var triggerName = (string)reader["name"];
					var triggerType = (string)reader["type_desc"];

					if (triggerName != null && triggerType != null)
					{
						logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DISABLED [{0}] DDL {1} ON ALL SERVER", triggerName, triggerType));
						cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DISABLE TRIGGER {0} ON ALL SERVER;", triggerName.QuoteName()));
					}
				}
			}

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				ExecuteInSqlTryCatch(connection, stmt);
			}
		}

		/// <summary>
		/// SQL SERVER AGENT JOBS
		/// Disables unauthorised SQL Agent jobs
		/// (at the moment no jobs are allowed to run)
		/// </summary>
		protected void DisableSqlAgentJobs(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var sql = @"
SELECT
	job_id,
	name
FROM
	dbo.sysjobs
WHERE
	[enabled] = 1
";

			var jobs = new List<Tuple<Guid, string>>();

			using (((ICurrentDbControl)connection).UseDatabase("msdb"))
			{
				connection.ExecuteReader(sql, (reader) =>
				{
					var jobId = reader["job_id"];
					var jobName = (string)reader["name"];

					if (jobId != null && jobName != null)
					{
						jobs.Add(Tuple.Create((Guid)jobId, jobName));
					}
				});

				if (jobs.Count > 0)
				{
					var sqlUpdate = dbTryCatchWrapper.ExecuteInSqlTryCatch("EXEC dbo.sp_update_job @job_id = @jobId, @enabled = 0;");

					foreach (var job in jobs)
					{
						using (var cmd = connection.Command(sqlUpdate))
						{
							cmd.AddParameter("@jobId", SqlDbType.UniqueIdentifier, job.Item1);

							logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DISABLED SQL AGENT JOB [{0}]", job.Item2));
							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		protected void SetLockModeSqlServerSysadminPwd(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot

			var sql = string.Format(CultureInfo.InvariantCulture,
				"if (EXISTS (SELECT NULL FROM sys.sql_logins WHERE name = N'{0}' AND PWDCOMPARE('{1}', password_hash) = 0)) ALTER LOGIN [{0}] WITH PASSWORD = '{1}';"
				, Db.SysAdminUserLogin
				, Db.SaValue.QuoteEscapedName('\'')
				);

			connection.ExecuteNonQuery(sql);
		}

		/// <summary>
		/// SERVER LEVEL ROLES
		/// Revokes server role membership to logins
		/// Allowed roles:
		///  - serveradmin, processadmin, diskadmin to all
		///  - ANY role to sa and OdysseyAdmin
		/// Removes user created roles.
		/// </summary>
		protected void CleanUpServerRoles(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(logBuilder, nameof(logBuilder));

			RemoveServerRoleMembers(connection, logBuilder);
			RemoveNonFixedServerRoles(connection, logBuilder);
		}

		void RemoveServerRoleMembers(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var cmdBuilder = new StringBuilder();

			var disallowedRoleMembershipList = GetDisallowedServerRoleMembers(connection);
			foreach (var roleMembershipInfo in disallowedRoleMembershipList)
			{
				logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture,
					"REVOKED [{0}] FROM [{1}] {2}"
					, roleMembershipInfo.RoleName
					, roleMembershipInfo.LoginName
					, roleMembershipInfo.LoginType
					));
				cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture,
					"ALTER SERVER ROLE {0} DROP MEMBER {1};"
					, roleMembershipInfo.RoleName.QuoteName()
					, roleMembershipInfo.LoginName.QuoteName()
					));
			}

			var cmd = cmdBuilder.ToString();
			if (cmd.Length > 0)
			{
				ExecuteInSqlTryCatch(connection, cmd);
			}
		}

		void RemoveNonFixedServerRoles(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var sql = @"
SELECT
	role_name   = sr.name,
	member_name = ISNULL(sl.name, N'')
FROM
	sys.server_principals             AS sr
	LEFT JOIN sys.server_role_members AS srm ON srm.role_principal_id = sr.principal_id
	LEFT JOIN sys.server_principals   AS sl  ON sl.principal_id = srm.member_principal_id
WHERE
	sr.type = 'R'
	AND sr.name != 'public'
	AND sr.is_fixed_role = 0
";

			var roles = new List<Tuple<string, string>>();
			connection.ExecuteReader(sql, (reader) =>
			{
				roles.Add(Tuple.Create((string)reader["role_name"], (string)reader["member_name"]));
			});

			if (roles.Count > 0)
			{
				var cmdBuilder = new StringBuilder();
				foreach (var role in roles.GroupBy(r => r.Item1))
				{
					logBuilder.AppendFormat(CultureInfo.InvariantCulture, "REMOVED [{0}] SERVER_ROLE", role.Key).AppendLine();

					foreach (var member in role.Select(r => r.Item2).Where(m => !string.IsNullOrWhiteSpace(m)))
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "ALTER SERVER ROLE {0} DROP MEMBER {1};", role.Key.QuoteName(), member.QuoteName()).AppendLine();
					}

					cmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "DROP SERVER ROLE {0};", role.Key.QuoteName()).AppendLine();
				}

				ExecuteInSqlTryCatch(connection, cmdBuilder.ToString());
			}
		}

		/// <summary>
		/// SERVER LEVEL PERMISSIONS
		/// Revokes server permissions from logins (server principals which are not fixed roles or certificates)
		///
		/// Permission [VIEW ANY DATABASE] is implicit with the [public] role, so it does not need an explicit grant.
		/// Allowed permissions:
		///  - TO all:
		///    - CO   = [CONNECT],
		///    - COSQ = [CONNECT SQL],
		///    - VWAD = [VIEW ANY DEFINITION],
		///    - VWSS = [VIEW SERVER STATE],
		///    - ALTR = [ALTER TRACE]
		///    - AAES = [ALTER ANY EVENT SESSION]
		///    - VEL  = [VIEW ANY ERROR LOG]  --> for SQL Server 2022 and above
		///  - TO [NT AUTHORITY\SYSTEM]
		///    - ALAG = [ALTER ANY AVAILABILITY GROUP] (required for AlwaysOn operability)
		///  - TO logins of type K = ASYMMETRIC_KEY_MAPPED_LOGIN
		///    - XU   = [UNSAFE ASSEMBLY] (required for SSIS operability)
		/// </summary>
		protected void CleanUpServerLevelPermissions(AdminConnection connection, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNull(logBuilder, nameof(logBuilder));

			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				var cmdBuilder = new StringBuilder();
				var sqlCommand = GetSelectCurrentPermissionsToUpdateSqlCommand();

				using (var cmd = connection.Command(sqlCommand))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var serverPermission = reader["serverPermission"];
						var permissionState = reader["permissionState"];
						var loginName = reader["loginName"];
						var loginType = reader["loginType"];
						var targetName = reader["targetName"];
						var targetType = reader["targetType"];

						if (serverPermission != null && permissionState != null && loginName != null && loginType != null
							&& targetName != null && targetType != null)
						{
							logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture,
								"REVOKED [{0}] FROM [{1}] {2} ON {3}"
								, serverPermission
								, loginName
								, loginType
								, (targetName == DBNull.Value) ? "SERVER" : targetType + " [" + targetName + "]"
								));

							cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture,
								"REVOKE {0}{1} FROM {2}{3};"
								, serverPermission
								, (targetName == DBNull.Value) ? "" : " ON " + targetType.ToString() + "::" + targetName.ToString().QuoteName()
								, loginName.ToString().QuoteName()
								, (permissionState.ToString() == "W") ? " CASCADE" : ""
								));
						}
					}
				}

				var stmt = cmdBuilder.ToString();
				ExecuteCleanupPermissionsInSqlTryCatch(connection, stmt);
			}
		}

		internal virtual void ExecuteCleanupPermissionsInSqlTryCatch(AdminConnection connection, string stmt)
		{
			if (stmt.Length > 0)
			{
				ExecuteInSqlTryCatch(connection, stmt);
			}
		}

		internal virtual string GetSelectCurrentPermissionsToUpdateSqlCommand()
		{
			var sql = @"
SELECT
	serverPermission = sp.permission_name,
	permissionState  = sp.state,
	loginName        = sl.name,
	loginType        = sl.type_desc,
	targetName       = coalesce(tgtLogin.name, tgtEndpoint.name, tgtags.name),
	targetType       =
		CASE
			WHEN sp.class = 100 THEN NULL
			WHEN sp.class = 105 THEN 'ENDPOINT'
			WHEN sp.class = 108 THEN 'AVAILABILITY GROUP'
			WHEN tgtLogin.type = 'R' THEN 'SERVER ROLE'
			ELSE 'LOGIN'
		END
FROM
	sys.server_permissions          AS sp
	JOIN sys.server_principals      AS sl          ON sl.principal_id = sp.grantee_principal_id
	LEFT JOIN sys.server_principals AS tgtLogin    ON tgtLogin.principal_id = sp.major_id AND sp.class = 101
	LEFT JOIN sys.endpoints         AS tgtEndpoint ON tgtEndpoint.endpoint_id = sp.major_id AND sp.class = 105
	LEFT JOIN (Select grp.name, rep.replica_metadata_id
				From sys.availability_replicas rep INNER JOIN
					sys.availability_groups grp on rep.group_id = grp.group_id
				Where replica_metadata_id IS NOT NULL
			)  AS tgtAGs ON tgtAGs.replica_metadata_id = sp.major_id AND sp.class = 108
WHERE
	sl.type != 'C'
	AND sl.name != 'public'
	AND sl.is_fixed_role != 1
	AND sp.state != 'D'
	AND sp.type NOT in ('CO', 'COSQ', 'VWAD', 'VWSS', 'ALTR', 'AAES', 'VEL')
	AND (sl.name != 'NT AUTHORITY\SYSTEM' OR sp.type != 'ALAG')
	AND (sl.type != 'K' OR sp.type != 'XU')
";
			return sql;
		}

		#endregion // Server Lockdown

		#endregion // SERVER LEVEL

		#region DATABASE LEVEL

		#region Lockdown

		public string LockdownDatabaseLevelSecurity(AdminConnection connection)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot

			var logBuilder = new StringBuilder();
			var dbList = connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
			var mainDbName = ((ICurrentDbControl)connection).InitialDatabase;
			var staffLoginList = GetStaffLoginList(connection, mainDbName);
			var staffUserLoginPrefix = DbUserRepository.GetStaffDbLoginFullPrefix(mainDbName);
			var staffUserLoginLikePattern = DataUtils.ReplaceSqlLikeWildcard(staffUserLoginPrefix);

			foreach (var dbName in dbList)
			{
				if (RefDbTableNameResolver.IsSharedDatabase(dbName))
				{
					using (((ICurrentDbControl)connection).UseDatabase(dbName))
					{
						LockdownDatabaseLevelSecurityOnDatabase(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);
					}
				}
				else
				{
					LockdownDatabaseLevelSecurityOnDatabase(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);
				}
			}

			return logBuilder.ToString();
		}

		void LockdownDatabaseLevelSecurityOnDatabase(AdminConnection connection, string dbName, StringBuilder logBuilder, List<GlbStaff> staffLoginList, string staffUserLoginPrefix, string staffUserLoginLikePattern)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));
			Argument.NotNullOrEmpty(staffUserLoginPrefix, nameof(staffUserLoginPrefix));

			int referenceLogLength = logBuilder.Length;

			CleanUpDisallowedDbRightsSafe(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);

			if (logBuilder.Length > referenceLogLength)
			{
				referenceLogLength = logBuilder.Length;
				DisableDatabaseDdlTriggers(connection, dbName, logBuilder);

				if (logBuilder.Length > referenceLogLength)
				{
					CleanUpDisallowedDbRightsSafe(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);
				}
			}
		}

		void CleanUpDisallowedDbRightsSafe(AdminConnection connection, string dbName, StringBuilder logBuilder, List<GlbStaff> staffLoginList, string staffUserLoginPrefix, string staffUserLoginLikePattern)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));
			Argument.NotNullOrEmpty(staffUserLoginPrefix, nameof(staffUserLoginPrefix));

			try
			{
				CleanUpDatabaseLevelRoleMembership(connection, dbName, logBuilder);
				CleanUpDatabaseLevelPermissions(connection, dbName, logBuilder);
				CleanUpDatabaseUsers(connection, dbName, logBuilder, staffLoginList, staffUserLoginPrefix, staffUserLoginLikePattern);
			}
			catch (SqlException ex)
			{
				logBuilder.AppendFormat("Security lockdown failed for database [{0}] => {1}", dbName, ex.Message).AppendLine();
			}
		}

		/// <summary>
		/// DATABASE LEVEL DDL TRIGGERS
		/// Disables all database DDL triggers
		/// </summary>
		protected void DisableDatabaseDdlTriggers(AdminConnection connection, string dbName, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var allowedTriggers = new[] { "TG_AllowAlterCDCMetaObjects" };

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	name,
	type_desc
FROM
	{0}.sys.triggers
WHERE
	parent_class = 0
	AND is_ms_shipped = 0
	AND is_disabled = 0
	AND name NOT IN ('{1}')
",
					dbName.QuoteName(), // 0
					string.Join("', '", allowedTriggers) // 1
				);

			var cmdBuilder = new StringBuilder();

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var triggerName = (string)reader["name"];
					var triggerType = (string)reader["type_desc"];

					logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DISABLED [{0}] DDL {1} ON DATABASE [{2}]", triggerName, triggerType, dbName));
					cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DISABLE TRIGGER {0} ON DATABASE;", triggerName.QuoteName()));
				}
			}

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}';", dbName.QuoteName(), stmt.QuoteEscapedName('\''));

				RunDatabaseLevelSecurityActionCateringForReadonlyDb(connection, dbName, sql);
			}
		}

		/// <summary>
		/// DATABASE LEVEL ROLES
		/// Revokes database role membership to users (excluding dbo which is internal to SQL Server and cannot be modified)
		/// Allowed roles:
		///  - On all databses
		///    - [db_datareader], [db_backupoperator], [db_denydatareader], [db_denydatawriter], [cwReaderRole], [cwRestrictedReaderRole], [cwHRMStaffRole] to all,
		///    - [db_owner] to application login user.
		///    - [cwRestrictedReaderRole] to restricted reader login user.
		///    - [cwRestrictedWriterRole] to restricted writer login user and cargowise writer login user.
		///    - [cwUnrestrictedWriterRole] to unrestricted writer login user.
		///    - [cwReaderRole] to cargowise reader login user.
		///  - On user repository database
		///    - [db_datawriter] to all.
		/// </summary>
		protected void CleanUpDatabaseLevelRoleMembership(AdminConnection connection, string dbName, StringBuilder logBuilder)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName)); // Suggested By ReviewBot
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var reservedSchemas = string.Join("','", Db.SqlReservedSchemas);

			var databaseAccessGroupRoles = (dbName.EndsWith(DbUserRepository.RepositoryDbSuffix, StringComparison.OrdinalIgnoreCase))
				? DatabaseAccessGroupRoleTypes.AllDbRolesWithDescriptions.Keys
				: DatabaseAccessGroupRoleTypes.AllDbRolesWithDescriptions.Keys.Where(key => key != DbRoleTypes.DbDataWriterRole);

			if (IsHostedAtWiseTechGlobal(connection))
			{
				databaseAccessGroupRoles = databaseAccessGroupRoles.Except(DbRoleTypes.DbBackupOperatorRole);
			}

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	dbRole   = dr.name,
	userName = dl.name,
	userType = dl.type_desc
FROM
	{0}.sys.database_role_members    AS drm
	JOIN {0}.sys.database_principals AS dr  ON dr.principal_id = drm.role_principal_id
	JOIN {0}.sys.database_principals AS dl  ON dl.principal_id = drm.member_principal_id
WHERE
	dl.name NOT in ('{1}', '{2}')
	AND dr.name NOT in ('db_datareader', 'db_denydatareader', 'db_denydatawriter', '{3}', {4})
	{5}
"
				, dbName.QuoteName()  // 0
				, Db.SqlDbOwnerSchema // 1
				, reservedSchemas     // 2
				, CwReaderRole        // 3
				, string.Join(", ", databaseAccessGroupRoles.Select(role => $"'{role}'"))          // 4
				, string.Join("\n\t", connection.Logins.Select(l => $"AND NOT (dl.name {CreateLoginSuffixSubQuery(dbName, l)} AND {CreateRoleNameSubQuery(l)})")) // 5
				);

			var cmdBuilder = new StringBuilder();

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbRole = (string)reader["dbRole"];
					var userName = (string)reader["userName"];
					var userType = (string)reader["userType"];

					logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "REVOKED [{0}] FROM [{1}] {2} ON DATABASE [{3}]", dbRole, userName, userType, dbName));
					cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ALTER ROLE {0} DROP MEMBER {1};", dbRole.QuoteName(), userName.QuoteName()));
				}
			}

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}'", dbName.QuoteName(), stmt.QuoteEscapedName('\''));

				RunDatabaseLevelSecurityActionCateringForReadonlyDb(connection, dbName, sql);
			}
		}

		static string CreateLoginSuffixSubQuery(string dbName, DatabaseLogin login)
		{
			return (RefDbTableNameResolver.IsSharedDatabase(dbName))
				? "LIKE '%[_]" + login.LoginSuffix + "'"
				: "= '" + Db.DatabaseName + '_' + login.LoginSuffix + "'";
		}

		static string CreateRoleNameSubQuery(DatabaseLogin login)
		{
			var roleNames = login.DbLevelRoles.Select(r => $"'{r.Name}'");
			return $"dr.name in ({string.Join(", ", roleNames)})";
		}

		/// <summary>
		/// DATABASE LEVEL PERMISSIONS
		/// Revokes database permissions to users.
		/// Permissions to roles are allowed as these are controlled by the role membership cleanup.
		/// Allowed permissions:
		///  - On all databses
		///    - [CONNECT SQL], [VIEW DEFINITION] [SHOWPLAN] to all.
		///  - On user repository database
		///    - ReadOnlyDBUserPermissionList + DatabaseDeveloperPermissionList.
		///    - DatabaseDeveloperSchemaPermissionList ON SCHEMA
		/// </summary>
		protected void CleanUpDatabaseLevelPermissions(AdminConnection connection, string dbName, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	dbPermission    = dp.permission_name,
	permissionState = dp.state,
	userName        = dl.name,
	userType        = dl.type_desc,
	securableClass  = dp.class,
	schemaName      = ISNULL(CASE dp.class WHEN 3 THEN sch.name ELSE objsch.name END, N''),
	objectName      = ISNULL(obj.name, N''),
	columnName      = ISNULL(col.name, N'')
FROM
	{0}.sys.database_permissions     AS dp
	JOIN {0}.sys.database_principals AS dl ON dl.principal_id = dp.grantee_principal_id
	LEFT JOIN
	(
		{0}.sys.objects      AS obj
		JOIN {0}.sys.schemas AS objsch ON objsch.schema_id = obj.schema_id
	) ON obj.object_id = dp.major_id AND dp.class = 1

	LEFT JOIN {0}.sys.columns AS col ON col.object_id = obj.object_id AND col.column_id = dp.minor_id
	LEFT JOIN {0}.sys.schemas AS sch ON sch.schema_id = dp.major_id AND dp.class = 3
WHERE
	dl.type NOT in ('R')
	AND
	(
		dp.class = 0
		OR dp.class = 1 AND obj.object_id is NOT NULL
		OR dp.class = 3 AND sch.schema_id is NOT NULL
	)
	AND dp.state != 'D'
	AND dp.type NOT in ('CO', 'VW', 'SPLN')
	{1}
	AND dl.name NOT in ('{2}', '{3}'{4})
	{5}
"
				, dbName.QuoteName()
				, string.Join("\n\t", connection.Logins.Select(l => $"AND NOT (dp.type = 'EX' AND dp.class = 0 AND dl.name {CreateLoginNameSubQuery(dbName, l)})"))
				, Db.SqlDbOwnerSchema
				, string.Join("','", Db.SqlReservedSchemas)
				, dbName.Equals(RefDbTableNameResolver.SingleRefDatabaseName, StringComparison.OrdinalIgnoreCase) ? ", 'guest'" : ""
				, (dbName.EndsWith(DbUserRepository.RepositoryDbSuffix, StringComparison.OrdinalIgnoreCase))
					? FormattableString.Invariant($"AND NOT ( (dp.class = 0 AND dp.permission_name in ({GetSqlStringFromList(DatabaseDeveloperPermissionList.Concat(ReadOnlyDBUserPermissionList))})) OR (dp.class = 3 AND dp.permission_name in ({GetSqlStringFromList(DatabaseDeveloperSchemaPermissionList)})) )")
					: ""
				);

			var cmdBuilder = new StringBuilder();

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbPermission = reader["dbPermission"];
					var permissionState = reader["permissionState"];
					var userName = (string)reader["userName"];
					var userType = reader["userType"];
					var securableClass = Convert.ToInt32(reader["securableClass"], CultureInfo.InvariantCulture);
					var schemaName = (string)reader["schemaName"];
					var objectName = (string)reader["objectName"];
					var columnName = (string)reader["columnName"];

					if (dbPermission != null && permissionState != null && userName != null && userType != null
						&& schemaName != null && objectName != null && columnName != null)
					{
						logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture,
							"REVOKED [{0}] FROM [{1}] {2} ON {3}"
							, dbPermission
							, userName
							, userType
							, (string.IsNullOrWhiteSpace(objectName))
								? (string.IsNullOrWhiteSpace(schemaName))
									? "DATABASE [" + dbName + "]" // dbOnly
									: "SCHEMA [" + dbName + "].[" + schemaName + "]" // schemaAndDb
								: (string.IsNullOrWhiteSpace(columnName))
									? "OBJECT [" + dbName + "].[" + schemaName + "].[" + objectName + "]" // objectDbSchemaObject
									: "COLUMN [" + dbName + "].[" + schemaName + "].[" + objectName + "] (" + columnName + ")" // columnDbSchemaObjectColumn
							));

						var line = string.Format(CultureInfo.InvariantCulture,
							"REVOKE {0} {1} FROM {2} {3};"
							, dbPermission
							, GetDatabaseSecurableClause(securableClass, schemaName, objectName, columnName)
							, userName.QuoteName()
							, (permissionState.ToString() == "W") ? "CASCADE" : ""
							);

						cmdBuilder.AppendLine(line);
					}
				}
			}

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}'", dbName.QuoteName(), stmt.QuoteEscapedName('\''));
				RunDatabaseLevelSecurityActionCateringForReadonlyDb(connection, dbName, sql);
			}
		}

		static string CreateLoginNameSubQuery(string dbName, DatabaseLogin login)
		{
			return (RefDbTableNameResolver.IsSharedDatabase(dbName))
							? "LIKE '%[_]" + login.LoginSuffix + "'"
							: "= '" + login.LoginName + "'";
		}

		string GetSqlStringFromList(IEnumerable<string> stringList)
		{
			return string.Join(", ", stringList.Select(p => "'" + p + "'"));
		}

		public static ReadOnlyCollection<string> DatabaseDeveloperSchemaPermissionList => new ReadOnlyCollection<string>(new string[] { "ALTER" });

		/// <summary>
		/// DATABASE Users
		/// Drop users on unused Ref DB
		/// </summary>
		protected void CleanUpDbRightsFromUnusedRefDBSafe(AdminConnection connection, string mainDbName, StringBuilder logBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(mainDbName, nameof(mainDbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @DBName SYSNAME;
DECLARE @SQL AS NVARCHAR(MAX);
DECLARE @tbl_loop TABLE(
	id INT IDENTITY,
	DBName SYSNAME
);
DECLARE @tbl_result TABLE(
	DBName SYSNAME,
	UserName SYSNAME
);

INSERT INTO @tbl_loop
SELECT name as DBName
	FROM sys.databases
	WHERE (name like 'CW-RefDb%' OR name like 'CW-AG-RefDb%')
		and state = 0
		and name COLLATE database_default  not in (
			SELECT DISTINCT SUBSTRING(base_object_name,2, CHARINDEX('.',base_object_name) - 3) as DBName FROM [{0}].sys.synonyms
	)
	order by DBName

SELECT @DBName = MIN(DBName) FROM @tbl_loop

WHILE @DBName is not null
BEGIN
	BEGIN TRY
		SET @SQL = 'select '''+@DBName+''' as DBName, name as UserName from ['+@DBName+'].sys.database_principals where name like ''%{0}%'''
		INSERT INTO @tbl_result
			EXEC sp_executesql @SQL
	END TRY
	BEGIN CATCH
	END CATCH

SELECT @DBName = MIN(DBName) FROM @tbl_loop WHERE DBName > @DBName
END

SELECT DBName,UserName FROM @tbl_result",
				mainDbName);

			using (var command = connection.Command(sqlText))
			{
				var cmdBuilder = new StringBuilder();

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var dbName = reader.GetString(0);
						var userName = reader.GetString(1);

						if (dbName != null && userName != null)
						{
							logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DROP User [{0}] ON DATABASE [{1}]", userName, dbName));
							cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "EXEC [{0}]..sp_executesql N'DROP USER [{1}]';", dbName, userName));
						}
					}
				}

				if (cmdBuilder.Length > 0)
				{
					sqlText = cmdBuilder.ToString();
					ExecuteInSqlTryCatch(connection, sqlText);
				}
			}
		}

		/// <summary>
		/// DATABASE USERS and ROLES
		/// Removes users and roles not acknowledged by our application.
		/// </summary>
		protected void CleanUpDatabaseUsers(AdminConnection connection, string dbName, StringBuilder logBuilder, List<GlbStaff> staffLoginList, string staffUserLoginPrefix, string staffUserLoginLikePattern)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(logBuilder, nameof(logBuilder));
			Argument.NotNull(staffUserLoginPrefix, nameof(staffUserLoginPrefix));

			var staffLoginListCsv = string.Join(",", staffLoginList.Select(x => x.GS_LoginName.ToString().QuoteName('\'')).ToArray());
			var usersToDropSqlBuilder = new StringBuilder();

			usersToDropSqlBuilder.AppendFormat(CultureInfo.InvariantCulture, @"
SELECT
	principalName    = dp.name,
	principalKeyword =
		CASE dp.type
			WHEN 'A' THEN 'APPLICATION ROLE'
			WHEN 'R' THEN 'ROLE'
			ELSE 'USER'
		END,

	principalType       = dp.type_desc,
	owning_principal_id = dp.owning_principal_id
FROM
	{0}.sys.database_principals AS dp
WHERE
	(
		dp.type = 'A'
	)
	OR
	(
		dp.type = 'R'
		AND dp.is_fixed_role = 0
		AND dp.name NOT in ('public', '{1}', '{2}')
	)
	OR
	(
		dp.type NOT in ('A', 'R')
		AND dp.sid != 0x00
		AND dp.name NOT in ('{3}', '{4}')
"
				, dbName.QuoteName()
				, CwReaderRole
				, string.Join("','", DbRoleTypes.AllDbRoles.Select(r => r.Name))
				, Db.SqlDbOwnerSchema
				, string.Join("', '", Db.SqlReservedSchemas)
				);

			if (IsSharedDatabase(dbName))
			{
				usersToDropSqlBuilder.AppendFormat(CultureInfo.InvariantCulture, @"
		AND dp.type NOT in ('G', 'U')
		AND dp.name NOT LIKE '{0}[_]%'"
					, DbUserRepository.StaffDbLoginPrefix
					);

				connection.Logins.ForEach(l =>
				{
					usersToDropSqlBuilder.AppendFormat(CultureInfo.InvariantCulture, @"
		AND dp.name NOT LIKE '%[_]{0}'"
					, l.LoginSuffix);
				});
			}
			else
			{
				usersToDropSqlBuilder.AppendFormat(CultureInfo.InvariantCulture, @"
		AND dp.name NOT in ('{0}')
"
					, string.Join("', '", connection.Logins.Select(l => l.LoginName))
					);

				if (!string.IsNullOrWhiteSpace(staffLoginListCsv))
				{
					usersToDropSqlBuilder.AppendFormat(CultureInfo.InvariantCulture, @"
		AND NOT (dp.type = 'S' AND dp.name LIKE '{0}%' AND CHARINDEX(QUOTENAME(SUBSTRING(dp.name, @prefixLength, 128), ''''), @staffList) > 0)
		AND NOT
		(
			dp.type in ('G', 'U')
			AND CHARINDEX('\', REVERSE(dp.name)) > 0
			AND CHARINDEX(QUOTENAME(RIGHT(dp.name, CHARINDEX('\', REVERSE(dp.name)) - 1), ''''), @staffList) > 0
		)
"
						, staffUserLoginLikePattern
						);
				}
			}

			usersToDropSqlBuilder.Append(@"
	)
ORDER BY
	dp.owning_principal_id DESC
");

			var sql = usersToDropSqlBuilder.ToString();

			var users = new Dictionary<string, (string UserType, string TypeDescription)>();
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@prefixLength", SqlDbType.Int, staffUserLoginPrefix.Length + 1);
				cmd.AddParameter("@staffList", SqlDbType.NVarChar, -1, string.IsNullOrEmpty(staffLoginListCsv) ? DBNull.Value : staffLoginListCsv);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var principalName = (string)reader["principalName"];
						var userType = (string)reader["principalKeyword"];
						var typeDescription = (string)reader["principalType"];

						if (!string.IsNullOrEmpty(principalName))
						{
							users.Add(principalName, (userType, typeDescription));
						}
					}
				}
			}

			if (users.Count > 0)
			{
				var mainDbName = ((ICurrentDbControl)connection).InitialDatabase;
				var isUserRepository = IsUserRepository(mainDbName, dbName);

				var adLinkedStaffDbLogins = GetADLinkedStaffDbLogins(staffLoginList, logBuilder);
				foreach (var user in users.Where(x => IsNotADLinkedStaffDbLogin(x, adLinkedStaffDbLogins)))
				{
					logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "REMOVING {0} [{1}] FROM DATABASE [{2}]", user.Value.TypeDescription, user.Key, dbName));

					if (isUserRepository)
					{
						DropUserInUserRepository(connection, dbName, user, logBuilder);
					}
					else
					{
						var cmdBuilder = new StringBuilder();
						DropUserSchemas(connection, dbName, user.Key, cmdBuilder);
						DropRoleMembers(connection, dbName, user.Key, cmdBuilder);
						DropFullTextIndexes(connection, dbName, user.Key, cmdBuilder);
						DropFullTextCatalogs(connection, dbName, user.Key, cmdBuilder);
						RevokeUserPermissions(connection, dbName, user.Key, cmdBuilder, logBuilder);
						DropUser(user.Value.UserType, user.Key, cmdBuilder);

						var stmt = cmdBuilder.ToString();
						if (stmt.Length > 0)
						{
							sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}';", dbName.QuoteName(), stmt.QuoteEscapedName('\''));

							RunDatabaseLevelSecurityActionCateringForReadonlyDb(connection, dbName, sql);
						}
					}
				}
			}
		}

		protected virtual bool IsUserRepository(string mainDbName, string dbName)
		{
			return DbUserRepository.IsRepositoryDatabase(mainDbName, dbName);
		}

		void DropUserInUserRepository(AdminConnection connection, string dbName, KeyValuePair<string, (string UserType, string TypeDescription)> user, StringBuilder logBuilder)
		{
			var cmdBuilder = new StringBuilder();
			DropUser(user.Value.UserType, user.Key, cmdBuilder);

			var stmt = cmdBuilder.ToString();
			if (stmt.Length > 0)
			{
				var sql = string.Format(CultureInfo.InvariantCulture, "EXEC {0}.sys.sp_executesql N'{1}';", dbName.QuoteName(), stmt.QuoteEscapedName('\''));

				try
				{
					RunDatabaseLevelSecurityActionCateringForReadonlyDb(connection, dbName, sql);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					logBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Failed to remove {0} [{1}] from database [{2}] due to: {3}", user.Value.TypeDescription, user.Key, dbName, ex.InnerException.Message));
				}
			}
		}

		#region AD Linked Staff Db Logins

		bool IsNotADLinkedStaffDbLogin(KeyValuePair<string, (string UserType, string TypeDescription)> user, List<string> staffLoginList)
		{
			return !(
				string.Equals(user.Value.UserType, "USER", StringComparison.OrdinalIgnoreCase) &&
				string.Equals(user.Value.TypeDescription, "WINDOWS_USER", StringComparison.OrdinalIgnoreCase) &&
				staffLoginList.Any(login => string.Equals(login, user.Key, StringComparison.OrdinalIgnoreCase)));
		}

		List<string> GetADLinkedStaffDbLogins(List<GlbStaff> staffLoginList, StringBuilder logBuilder)
		{
			var result = new List<string>();
			foreach (var staff in staffLoginList)
			{
				try
				{
					var downlevelLoginName = staff.GetDownLevelLogonName();
					if (!string.IsNullOrEmpty(downlevelLoginName))
					{
						result.Add(downlevelLoginName);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// We don't care any AD exception here
					logBuilder.AppendLine(Invariant($"Error retrieving AD username for staff {staff.GS_LoginName}: ") + ex.Message);
				}
			}
			return result;
		}

		#endregion

		void DropUserSchemas(AdminConnection connection, string dbName, string principalName, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DROP RELATED SCHEMAS
SELECT
	sch.name
FROM
	{0}.sys.schemas                  AS sch
	JOIN {0}.sys.database_principals AS dp  ON dp.principal_id = sch.principal_id
WHERE
	dp.name = @userName;
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userName", SqlDbType.NVarChar, 128, principalName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "DROP SCHEMA {0};", ((string)reader["name"]).QuoteName())?.AppendLine();
					}
				}
			}
		}

		void DropRoleMembers(AdminConnection connection, string dbName, string principalName, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DROP MEMBERS (roles only)
SELECT
	dm.name
FROM
	{0}.sys.database_role_members    AS drm
	JOIN {0}.sys.database_principals AS dr  ON dr.principal_id = drm.role_principal_id
	JOIN {0}.sys.database_principals AS dm  ON dm.principal_id = drm.member_principal_id
WHERE
	dr.name = @userName;
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userName", SqlDbType.NVarChar, 128, principalName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "ALTER ROLE {0} DROP MEMBER {1};", principalName.QuoteName(), ((string)reader["name"]).QuoteName())?.AppendLine();
					}
				}
			}
		}

		void DropFullTextIndexes(AdminConnection connection, string dbName, string principalName, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DROP RELATED FULLTEXT INDEXES
SELECT
	sch_name = sch.name,
	obj_name = obj.name
FROM
	{0}.sys.fulltext_indexes         AS fti
	JOIN {0}.sys.fulltext_catalogs   AS ftc ON ftc.fulltext_catalog_id = fti.fulltext_catalog_id
	JOIN {0}.sys.objects             AS obj ON obj.object_id = fti.object_id
	JOIN {0}.sys.schemas             AS sch ON obj.schema_id = sch.schema_id
	JOIN {0}.sys.database_principals AS dp  ON dp.principal_id = ftc.principal_id
WHERE
	dp.name = @userName;
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userName", SqlDbType.NVarChar, 128, principalName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
							"DROP FULLTEXT INDEX ON {0}.{1};"
							, ((string)reader["sch_name"]).QuoteName()
							, ((string)reader["obj_name"]).QuoteName()
							)
							?.AppendLine();
					}
				}
			}
		}

		void DropFullTextCatalogs(AdminConnection connection, string dbName, string principalName, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DROP RELATED FULLTEXT CATALOGS
SELECT
	ftc.name
FROM
	{0}.sys.fulltext_catalogs        AS ftc
	JOIN {0}.sys.database_principals AS dp  ON dp.principal_id = ftc.principal_id
WHERE
	dp.name = @userName;
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userName", SqlDbType.NVarChar, 128, principalName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture, "DROP FULLTEXT CATALOG {0};", ((string)reader["name"]).QuoteName())?.AppendLine();
					}
				}
			}
		}

		void DropUser(string userType, string principalName, StringBuilder builder)
		{
			Argument.NotNull(builder, nameof(builder));

			builder.AppendFormat(CultureInfo.InvariantCulture, "DROP {0} {1};", userType, principalName.QuoteName()).AppendLine();
		}

		void RevokeUserPermissions(DbConnection connection, string dbName, string userName, StringBuilder cmdBuilder, StringBuilder logBuilder)
		{
			const string sql = @"
SELECT
	perm.permission_name permission, dp_grantor.name as grantor, dp_grantee.name as grantee
FROM
	sys.database_permissions perm
	join sys.database_principals dp_grantee on dp_grantee.principal_id = perm.grantee_principal_id
	join sys.database_principals dp_grantor on dp_grantor.principal_id = perm.grantor_principal_id
WHERE
	perm.class = 4
	AND dp_grantor.name = @userName
;
";

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteReader(sql,
					command =>
					{
						command.AddParameter("@userName", SqlDbType.NVarChar, 128, userName);
					},
					reader =>
					{
						var revoke = Invariant($"REVOKE {reader["permission"]} ON USER::[{reader["grantor"]}] FROM [{reader["grantee"]}];");
						cmdBuilder.AppendLine(revoke);
						logBuilder.AppendLine(revoke);
					});
			}
		}

		string GetDatabaseSecurableClause(int securableClass, string schemaName, string majorName, string minorName)
		{
			switch (securableClass)
			{
				case 0: // Database
					return "";
				case 1: // Object or Column
					return "ON " + schemaName.QuoteName() + "." + majorName.QuoteName() + (string.IsNullOrEmpty(minorName) ? "" : minorName.QuoteName().QuoteName('('));
				case 3:  // Schema
					return "ON SCHEMA::" + schemaName.QuoteName();
				case 4:  // "ON USER :: [" + majorName + "]";
				case 5:  // "ON ASSEMBLY :: [" + majorName + "]"
				case 6:  // "ON TYPE :: [" + schemaName + "].[" + majorName + "]"
				case 10: // "ON XML SCHEMA COLLECTION :: [" + schemaName + "].[" + majorName + "]"
				case 15: // "ON MESSAGE TYPE :: [" + majorName + "]"
				case 16: // "ON CONTRACT :: [" + majorName + "]"
				case 17: // "ON SERVICE :: [" + majorName + "]"
				case 18: // "ON REMOTE SERVICE BINDING :: [" + majorName + "]"
				case 19: // "ON ROUTE :: [" + majorName + "]"
				case 23: // "ON FULLTEXT CATALOG :: [" + majorName + "]" // and ?? FULLTEXT STOPLIST ??
				case 24: // "ON SYMMETRIC KEY :: [" + majorName + "]"
				case 25: // "ON CERTIFICATE :: [" + majorName + "]"
				case 26: // "ON ASYMMETRIC KEY :: [" + majorName + "]"
				default:
					throw new ArgumentOutOfRangeException(nameof(securableClass), string.Format(CultureInfo.InvariantCulture, "Securable class value of {0} is out of range.", securableClass.ToString(CultureInfo.InvariantCulture)));
			}
		}

		protected List<GlbStaff> GetStaffLoginList(AdminConnection connection, string connectionMainDbName)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(connectionMainDbName, nameof(connectionMainDbName));

			using (((ICurrentDbControl)connection).UseDatabase(connectionMainDbName))
			{
				return new DbUserManager().GetStaffThatShouldHaveDbLogins(connection).ToList();
			}
		}

		#endregion // Lockdown

		#endregion // DATABASE LEVEL

		#region Implementation

		/// <summary>
		/// Runs security cleanup methods catering for a read-only database
		/// </summary>
		void RunDatabaseLevelSecurityActionCateringForReadonlyDb(AdminConnection connection, string dbName, string commandScript)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNullOrEmpty(commandScript, nameof(commandScript));

			if (connection.IsDbWriteable(dbName))
			{
				ExecuteInSqlTryCatch(connection, commandScript);
			}
			else
			{
				try
				{
					connection.AlterDbWriteableState(dbName, true);
					ExecuteInSqlTryCatch(connection, commandScript);
				}
				finally
				{
					connection.AlterDbWriteableState(dbName, false);
				}
			}
		}

		void ExecuteInSqlTryCatch(DbConnection connection, string sqlCommand)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(sqlCommand, nameof(sqlCommand));

			var tryCacheWrappedCmd = dbTryCatchWrapper.ExecuteInSqlTryCatch(sqlCommand);

			try
			{
				connection.ExecuteNonQuery(tryCacheWrappedCmd);
			}
			catch (SqlException ex)
			{
				throw new Exception(tryCacheWrappedCmd, ex);
			}
		}

		/// <summary>
		/// WiseTech Global internal servers should not be completely locked down as some logins are required for internal admin tasks.
		/// </summary>
		public bool IsHostedAtWiseTechGlobal(DbConnection connection)
		{
#if DEBUG
			if (!Globals.IsTest)
			{
				return true;
			}
#endif
			return EnvProxy.IsHostedWithCargowise || DataUtils.IsWiseTechGlobalDatabaseServer(connection);
		}

		readonly DbTryCatchWrapper dbTryCatchWrapper = new DbTryCatchWrapper();

		#endregion // Implementation
	}

	#endregion // SuppressResourceStringsCheckRegion
}
