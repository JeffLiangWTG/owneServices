using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	public partial class DbUserManager
	{
		public DbUserManager()
			: this(null)
		{
		}

		public DbUserManager(ILogger logger)
		{
			Logger = logger;
		}

		#region BI Connections

		AdminConnection GetAuditConnection(AdminConnection mainDbConnection)
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);
			return GetBiConnection(auditServer);
		}

		AdminConnection GetDataWarehouseConnection(AdminConnection mainDbConnection)
		{
			var dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(mainDbConnection);
			return GetBiConnection(dataWarehouseServer);
		}

		AdminConnection GetBiConnection(string biServer)
		{
			if (!string.IsNullOrEmpty(biServer))
			{
				return Db.NewAdminConnection(biServer, Db.SqlMasterDb);
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Apply GlbStaff changes to DB Server

		#region Create Login

		void CreateStaffLogin(StaffLoginInfo info)
		{
			using (var securityConnection = Db.NewAdminConnection())
			using (((ICurrentDbControl)securityConnection).UseDatabase(Db.DatabaseName))
			using (var auditConnection = GetAuditConnection(securityConnection))
			using (var dwConnection = GetDataWarehouseConnection(securityConnection))
			{
				CreateAndPropagateStaffLogin(securityConnection, auditConnection, dwConnection, info);
			}
		}

		readonly bool biEnableAuditAccess = SystemDataRegistry.Instance.BiEnableAuditAccess.Value;
		void CreateAndPropagateStaffLogin(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			info.LoginName = GetFullUserLoginName(securityConnection.CurrentDatabase, info.LoginName, info.GetDownLevelLogonName, info.DbAuthenticationMode);

			if (CreateLoginWithPermissions(securityConnection, info, Db.DatabaseName))
			{
				CreateDbUserWithPermissions(securityConnection, info);
			}

			CreateAndPropagateStaffLoginOnAuditServer(securityConnection, auditConnection, info);

			try
			{
				PropagateLogin(new List<StaffLoginInfo>() { info }, (newConnection, newInfo) => CreateLoginWithPermissions(newConnection, newInfo, Db.DatabaseName));
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void CreateAndPropagateStaffLoginOnAuditServer(AdminConnection securityConnection, AdminConnection auditConnection, StaffLoginInfo info)
		{
			if (auditConnection != null && biEnableAuditAccess)
			{
				var shouldCreateBiDbUser = true;
				if (!string.Equals(auditConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					shouldCreateBiDbUser = CreateLoginWithPermissions(auditConnection, info, auditConnection.CurrentDatabase);
				}

				if (shouldCreateBiDbUser)
				{
					CreateDbUserWithPermissionsOnBiDatabase(auditConnection, Db.AuditDatabaseName, info);
				}
			}
		}

		#endregion

		#region Alter Login

		void AlterStaffLogin(StaffLoginInfo info)
		{
			using (var securityConnection = Db.NewAdminConnection())
			using (((ICurrentDbControl)securityConnection).UseDatabase(Db.DatabaseName))
			using (var auditConnection = GetAuditConnection(securityConnection))
			using (var dwConnection = GetDataWarehouseConnection(securityConnection))
			{
				AlterAndPropagateStaffLogin(securityConnection, auditConnection, dwConnection, info);
			}
		}

		void AlterAndPropagateStaffLogin(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			try
			{
				info.LoginName = GetFullUserLoginName(securityConnection.CurrentDatabase, info.LoginName, info.GetDownLevelLogonName, info.DbAuthenticationMode);
				info.OldLoginName = GetFullUserLoginName(securityConnection.CurrentDatabase, info.OldLoginName, info.GetDownLevelLogonName, info.DbAuthenticationMode);

				if (info.LoginNameChanged)
				{
					RenameLogin(securityConnection, info, Db.DatabaseName);
					PropagateLogin(new List<StaffLoginInfo>() { info }, (newConnection, newInfo) => RenameLogin(newConnection, newInfo, Db.DatabaseName));
					RenameDbUsers(securityConnection, info);

					RenameDbUserOnAuditDatabase(securityConnection, auditConnection, info);
					RenameDbUserOnEdwDatabase(securityConnection, auditConnection, dwConnection, info);
				}

				if (info.PasswordChanged || info.DbPermissionChanged)
				{
					CreateOrAlterSqlLogin(securityConnection, info, Db.DatabaseName);
					PropagateLogin(new List<StaffLoginInfo>() { info }, (newConnection, newInfo) => CreateOrAlterSqlLogin(newConnection, newInfo, Db.DatabaseName));

					if (info.DbPermissionChanged)
					{
						ManageServerLevelPermissions(securityConnection, info);
						PropagateLogin(new List<StaffLoginInfo>() { info }, (newConnection, newInfo) => ManageServerLevelPermissions(newConnection, newInfo));

						CreateDbUserWithPermissions(securityConnection, info);
					}

					CreateDbUserWithPermissionsOnAuditDatabase(securityConnection, auditConnection, info);
					CreateDbUserWithPermissionsOnEdwDatabase(securityConnection, auditConnection, dwConnection, info);
				}
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void RenameDbUserOnAuditDatabase(AdminConnection securityConnection, AdminConnection auditConnection, StaffLoginInfo info)
		{
			if (auditConnection != null && biEnableAuditAccess)
			{
				if (!string.Equals(auditConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					RenameLogin(auditConnection, info, auditConnection.CurrentDatabase);
				}
				RenameDbUserOnDatabase(auditConnection, Db.AuditDatabaseName, info);
			}
		}

		void RenameDbUserOnEdwDatabase(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			if (dwConnection != null)
			{
				if (!string.Equals(dwConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(dwConnection.ServerNameReportedByDatabase, auditConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					RenameLogin(dwConnection, info, dwConnection.CurrentDatabase);
				}
				RenameDbUserOnDatabase(dwConnection, Db.EdwDatabaseName, info);
			}
		}

		void RenameDbUsers(AdminConnection securityConnection, StaffLoginInfo info)
		{
			CreateUserRepositoryDatabaseIfNeeded(securityConnection, info);

			foreach (var dbName in GetEnterpriseDbs(securityConnection))
			{
				RenameDbUserOnDatabase(securityConnection, dbName, info);
			}
		}

		void RenameDbUserOnDatabase(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			if (connection.IsDbWriteableAndOnline(dbName))
			{
				RenameDbUser(connection, dbName, info);
			}
		}

		void RenameDbUser(AdminConnection securityConnection, string dbName, StaffLoginInfo info)
		{
			using (((ICurrentDbControl)securityConnection).UseDatabase(dbName))
			{
				if (securityConnection.Exists((NoResString)"FROM sys.database_principals WHERE name = @oldUserName", cmd => cmd.AddParameter("@oldUserName", SqlDbType.NVarChar, 128, info.OldLoginName)))
				{
					securityConnection.ExecuteNonQuery(FormattableString.Invariant($"ALTER USER {info.OldLoginName.QuoteName()} WITH NAME = {info.LoginName.QuoteName()};"));
				}
			}
		}

		void RenameLogin(AdminConnection securityConnection, StaffLoginInfo info, string defaultDatabase)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- RenameLogin
DECLARE
	@isDisabled int = (SELECT is_disabled FROM sys.server_principals WHERE name = @userLogin);

if (@isDisabled is NULL)
begin
	{0}
end
else
begin
	{1}

	if (@isDisabled = 1)
	begin
		{2}
	end
end
"
				, SQL_CreateLogin(info, defaultDatabase) // 0
				, SQL_RenameLogin(info) // 1
				, SQL_EnableLogin(info) // 2
				);

			using (var cmd = securityConnection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.OldLoginName);
				cmd.ExecuteNonQuery();
			}
		}

		void CreateDbUserWithPermissionsOnAuditDatabase(AdminConnection securityConnection, AdminConnection auditConnection, StaffLoginInfo info)
		{
			if (auditConnection != null && biEnableAuditAccess)
			{
				if (!string.Equals(auditConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					CreateOrAlterSqlLogin(auditConnection, info, auditConnection.CurrentDatabase);

					if (info.DbPermissionChanged)
					{
						ManageServerLevelPermissions(auditConnection, info);
						CreateDbUserWithPermissionsOnDatabase(auditConnection, Db.AuditDatabaseName, info);
					}
				}
			}
		}

		void CreateDbUserWithPermissionsOnEdwDatabase(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			if (dwConnection != null)
			{
				if (!string.Equals(dwConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(dwConnection.ServerNameReportedByDatabase, auditConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					CreateOrAlterSqlLogin(dwConnection, info, dwConnection.CurrentDatabase);

					if (info.DbPermissionChanged)
					{
						ManageServerLevelPermissions(dwConnection, info);
						CreateDbUserWithPermissionsOnDatabase(dwConnection, Db.EdwDatabaseName, info);
					}
				}
			}
		}

		#endregion

		#region Drop Login

		void DropStaffLogin(StaffLoginInfo info)
		{
			using (var securityConnection = Db.NewAdminConnection())
			using (var auditConnection = GetAuditConnection(securityConnection))
			using (var dwConnection = GetDataWarehouseConnection(securityConnection))
			{
				if (DbSupportsWindowsAuthentication(info.GetDownLevelLogonName))
				{
					var sqlInfo = StaffCopy(info);
					sqlInfo.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Windows);
					DropStaffLoginAndPropagate(securityConnection, auditConnection, dwConnection, sqlInfo);
				}

				if (DbSupportsSqlAuthentication())
				{
					var winInfo = StaffCopy(info);
					winInfo.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Sql);
					DropStaffLoginAndPropagate(securityConnection, auditConnection, dwConnection, winInfo);
				}
			}
		}

		void DropStaffLoginAndPropagate(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			DropLogin(securityConnection, info);
			DropDbUser(securityConnection, info);

			DropDbUserOnAuditDatabase(securityConnection, auditConnection, info);
			DropDbUserOnEdwDatabase(securityConnection, auditConnection, dwConnection, info);

			try
			{
				PropagateLogin(new List<StaffLoginInfo>() { info }, DropLogin);
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void DropDbUserOnAuditDatabase(AdminConnection securityConnection, AdminConnection auditConnection, StaffLoginInfo info)
		{
			if (auditConnection != null)
			{
				if (!string.Equals(auditConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					DropLogin(auditConnection, info);
				}
				DropDbUserOnDatabase(auditConnection, Db.AuditDatabaseName, info);
			}
		}

		void DropDbUserOnEdwDatabase(AdminConnection securityConnection, AdminConnection auditConnection, AdminConnection dwConnection, StaffLoginInfo info)
		{
			if (dwConnection != null)
			{
				if (!string.Equals(dwConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(dwConnection.ServerNameReportedByDatabase, auditConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase))
				{
					DropLogin(dwConnection, info);
				}
				DropDbUserOnDatabase(dwConnection, Db.EdwDatabaseName, info);
			}
		}

		void DropDbUser(AdminConnection securityConnection, StaffLoginInfo info)
		{
			foreach (var dbName in GetEnterpriseDbs(securityConnection))
			{
				DropDbUserOnDatabase(securityConnection, dbName, info);
			}
		}

		void DropDbUserOnDatabase(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			if (connection.IsDbWriteableAndOnline(dbName))
			{
				DropDbUser(connection, dbName, info);
			}
		}

		#endregion

		#endregion // Apply GlbStaff changes to DB Server

		#region Synchronise Staff and Database Logins

		public void SetPasswordForStaff(GlbStaff staff, string newPassword)
		{
			staff.GS_SqlLoginPasswordHash = SqlLoginPasswordHashGenerator.GenerateFromDb(Db.Connection, newPassword);
		}

		public void SynchroniseAllStaffAndDbLogins(AdminConnection securityConnection)
		{
			if (CheckADConnectivity())
			{
				var loginsToCreate = GetUsersThatShouldHaveDbLogins(securityConnection);
				var staffUserDataTable = GetDataTableOfStaffProxyUsersList(loginsToCreate);

				CreateOrDropLogins(securityConnection, loginsToCreate, staffUserDataTable);
				CreateOrDropUsers(securityConnection, loginsToCreate, staffUserDataTable);
			}
		}

		public void SynchroniseAllStaffAndDbLoginsForAllDatabases(AdminConnection securityConnection)
		{
			if (CheckADConnectivity())
			{
				var loginsToCreate = GetUsersThatShouldHaveDbLogins(securityConnection);
				var staffUserDataTable = GetDataTableOfStaffProxyUsersList(loginsToCreate);

				CreateOrDropLogins(securityConnection, loginsToCreate, staffUserDataTable);
				CreateOrDropUsers(securityConnection, loginsToCreate, staffUserDataTable);

				using (var auditConnection = GetAuditConnection(securityConnection))
				{
					if (auditConnection != null)
					{
						var recreateLogin = !string.Equals(auditConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase);
						SynchroniseAllStaffAndDbLoginsForBiDatabase(auditConnection, Db.AuditDatabaseName, loginsToCreate: biEnableAuditAccess ? loginsToCreate : new List<StaffLoginInfo>(), staffUserDataTable, recreateLogin);
					}
				}

				using (var dataWarehouseConnection = GetDataWarehouseConnection(securityConnection))
				{
					if (dataWarehouseConnection != null)
					{
						var recreateLogin = !string.Equals(dataWarehouseConnection.ServerNameReportedByDatabase, securityConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase);
						SynchroniseAllStaffAndDbLoginsForBiDatabase(dataWarehouseConnection, Db.EdwDatabaseName, loginsToCreate: new List<StaffLoginInfo>(), staffUserDataTable, recreateLogin);
					}
				}
			}
		}

		void SynchroniseAllStaffAndDbLoginsForBiDatabase(AdminConnection biConnection, string dbName, List<StaffLoginInfo> loginsToCreate, DataTable staffUserDataTable, bool recreateLogin = true)
		{
			if (biConnection != null)
			{
				if (recreateLogin)
				{
					CreateOrDropLoginsOnBiDatabase(biConnection, dbName, loginsToCreate, staffUserDataTable);
				}
				CreateOrDropUsersOnBiDatabase(biConnection, dbName, loginsToCreate, staffUserDataTable);
			}
		}

		bool CheckADConnectivity()
		{
			if (ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled)
			{
				try
				{
					ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(ObjectFactory.Get<IADRegistry>().DefaultDomainCredentials, false);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Occasionally, when this is called by System Upgrade Service (UPG) (by calling AlwaysOnManagementServiceTask.RunTask()), a COMException with Unknown error (0x80005000) could be thrown when it is trying to connect to the Domain/AD.
					// We skip the entire process when this happens as AOM will re-run this process later.
					// See WI00320149 for more details
					Logger?.Log(LogType.Warning, (NoResString)"User Login Synchronization cannot be run at this time as it failed to connect to the Domain or Active Directory server. This can be ignored if the system is currently performing an upgrade, the AlwaysOn Management Service will correct it. Alternatively, you can run 'Synchronize Staff Database Logins' manually after the upgrade.");
					return false;
				}
			}
			return true;
		}

		void CreateOrDropLogins(AdminConnection connection, List<StaffLoginInfo> loginsToCreate, DataTable staffUserDataTable)
		{
			try
			{
				if (loginsToCreate.Count > 0)
				{
					foreach (var info in loginsToCreate)
					{
						CreateLoginWithPermissions(connection, info, Db.DatabaseName);
					}

					PropagateLogin(loginsToCreate, (newConnection, newInfo) => CreateLoginWithPermissions(newConnection, newInfo, Db.DatabaseName));
				}

				var loginsToDrop = GetLoginsToDrop(connection, staffUserDataTable);
				if (loginsToDrop.Count > 0)
				{
					foreach (var info in loginsToDrop)
					{
						DropLogin(connection, info);
					}

					PropagateLogin(loginsToDrop, DropLogin);
				}
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void CreateOrDropLoginsOnBiDatabase(AdminConnection biConnection, string dbName, List<StaffLoginInfo> loginsToCreate, DataTable staffUserDataTable)
		{
			try
			{
				if (loginsToCreate.Count > 0)
				{
					foreach (var info in loginsToCreate)
					{
						CreateLoginWithPermissions(biConnection, info, biConnection.CurrentDatabase);
					}
				}

				if (biConnection.DatabaseExists(dbName))
				{
					using (((ICurrentDbControl)biConnection).UseDatabase(dbName))
					{
						var loginsToDrop = GetLoginsToDrop(biConnection, staffUserDataTable);
						if (loginsToDrop.Count > 0)
						{
							foreach (var info in loginsToDrop)
							{
								DropLogin(biConnection, info);
							}
						}
					}
				}
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void CreateOrDropUsers(AdminConnection connection, List<StaffLoginInfo> loginsToCreate, DataTable staffUserDataTable)
		{
			CreateUserRepositoryDatabaseIfNeeded(connection, loginsToCreate);

			var requiresReaderRole = loginsToCreate.Any(info => info.IsReadOnlyDBUser);
			try
			{
				foreach (var dbName in GetEnterpriseDbs(connection))
				{
					if (connection.IsDbWriteableAndOnline(dbName))
					{
						if (requiresReaderRole)
						{
							EnsureReaderRoleExist(connection, dbName);
						}

						foreach (var info in loginsToCreate)
						{
							CreateDbUserWithPermissions(connection, dbName, info);
						}

						var staffUserPattern = RefDbTableNameResolver.IsSharedDatabase(dbName) ? specificStaffUserPattern : genericStaffUserPattern;
						var dbUsersToDrop = GetDbUsersToDrop(connection, dbName, staffUserPattern, staffUserDataTable);

						foreach (var dbUser in dbUsersToDrop)
						{
							DropDbUser(connection, dbName, dbUser);
						}
					}
				}
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		void CreateOrDropUsersOnBiDatabase(AdminConnection biConnection, string dbName, List<StaffLoginInfo> loginsToCreate, DataTable staffUserDataTable)
		{
			var requiresReaderRole = loginsToCreate.Any(info => info.IsReadOnlyDBUser);
			try
			{
				if (biConnection.IsDbWriteableAndOnline(dbName))
				{
					if (requiresReaderRole)
					{
						EnsureReaderRoleExist(biConnection, dbName);
					}

					foreach (var info in loginsToCreate)
					{
						CreateDbUserWithPermissions(biConnection, dbName, info);
					}

					var staffUserPattern = RefDbTableNameResolver.IsSharedDatabase(dbName) ? specificStaffUserPattern : genericStaffUserPattern;
					using (((ICurrentDbControl)biConnection).UseDatabase(dbName))
					{
						var dbUsersToDrop = GetDbUsersToDrop(biConnection, dbName, staffUserPattern, staffUserDataTable);

						foreach (var dbUser in dbUsersToDrop)
						{
							DropDbUser(biConnection, dbName, dbUser);
						}
					}
				}
			}
			catch (EmailSendFailedException ex)
			{
				LogEmailSendFailedException(ex);
			}
		}

		#region Remove Old DB Logins and Users

		void DropLogin(AdminConnection connection, StaffLoginInfo info)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DropLogin
if (EXISTS (SELECT NULL FROM sys.server_principals WHERE name = @userLogin))
begin
	ALTER LOGIN {0} DISABLE;
	DROP LOGIN {0};
end
"
				, info.LoginName.QuoteName() // 0
				);

			ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to drop login \"{0}\". Server: [{1}]", info.LoginName, connection.ServerName)
				, () =>
				{
					using (var cmd = connection.Command(sql))
					{
						cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);

						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotDropLoginCurrentlyLoggedIn)
						{
							KillUserConnections(connection, info);
						}
					}
				});
		}

		void KillUserConnections(AdminConnection connection, StaffLoginInfo info)
		{
			var sql = @"-- KillUserConnections
DECLARE
	@stmt nvarchar(4000)

SELECT
	@stmt = ISNULL(@stmt, N'')
		+ N'KILL ' + CONVERT(nvarchar(10), session_id) + N';'
FROM
	sys.dm_exec_sessions
WHERE
	original_login_name = @userLogin

if (@stmt is NOT NULL) EXEC sys.sp_executesql @stmt;
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		void DropDbUser(AdminConnection connection, string dbName, StaffLoginInfo dbUser)
		{
			// Drop DB user and its schemas
			var baseSqlText = string.Format(CultureInfo.InvariantCulture, @"-- DropDbUser
DECLARE @execCmd nvarchar(4000);

if (EXISTS (SELECT NULL FROM [{0}].sys.database_principals WHERE name = @userLogin))
begin
	{1}
end
"
				, "{0}"
				, SQL_BaseDropUser
				);
			var sql = string.Format(CultureInfo.InvariantCulture, baseSqlText, dbName);
			try
			{
				using var cmd = connection.Command(sql);
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, dbUser.LoginName);
				cmd.ExecuteNonQuery();
			}
			catch (Exception e) when (e.IsCriticalException()) { throw; }
			catch (SqlException ex)
			{
				if (Logger != null)
				{
					if (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotDropSchemaReferencedByObject)
					{
						var message = string.Format(CultureInfo.InvariantCulture,
							"Failed to drop database user \"{0}\". Server: [{1}], Database: [{2}], Sql Error: [{3}]. Please drop these objects manually or contact your database administrator if this cannot be done."
							, dbUser.LoginName      // 0
							, connection.ServerName // 1
							, dbName                // 2
							, ex.Message            // 3
						);
						Logger.Log(LogType.Warning, message);
					}
					else
					{
						var message = string.Format(CultureInfo.InvariantCulture,
							"Failed to drop database user \"{0}\". Server: [{1}], Database: [{2}]. This can be because the user to be dropped owns schemas with associated objects."
							, dbUser.LoginName      // 0
							, connection.ServerName // 1
							, dbName                // 2
						);
						Logger.Log(LogType.Error, message, ex);
					}
				}
				else
				{
					throw;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query, Logging information")]
		List<StaffLoginInfo> GetDbUsersToDrop(AdminConnection connection, string dbName, string staffUserPattern, DataTable staffUserDataTable)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"-- GetDbUsersToDrop
SELECT
	dp.name
FROM
	{0}.sys.database_principals AS dp LEFT OUTER JOIN @StaffUserTable AS s
		ON dp.name = s.Value COLLATE SQL_Latin1_General_CP1_CI_AS
WHERE
	(s.Value IS NULL)
AND (dp.name LIKE ISNULL(@StaffUserPattern, N'%'))
"
				, dbName.QuoteName());

			var dbUsersToDrop = new List<StaffLoginInfo>();
			var message = string.Format(CultureInfo.InvariantCulture, "Failed to get database users to drop. Server: [{0}], Database: [{1}]", connection.ServerName, dbName);

			ExecuteWithLog(message, () =>
			{
				using (var cmd = connection.Command(sqlText))
				{
					string tvpTypeName = TVPHelper.GetTVPName(SqlDbType.NVarChar, 128);
					cmd.AddTableValuedParameter("@StaffUserTable", tvpTypeName, staffUserDataTable);
					cmd.AddParameter("@StaffUserPattern", SqlDbType.NVarChar, 100, staffUserPattern);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							dbUsersToDrop.Add(new StaffLoginInfo()
							{
								StaffLoginAction = StaffLoginInfo.StaffLoginActions.Drop,
								LoginName = (string)reader["name"]
							});
						}
					}

					staffUserDataTable.Dispose();
				}
			});

			return dbUsersToDrop;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query, Logging information")]
		List<StaffLoginInfo> GetLoginsToDrop(AdminConnection connection, DataTable staffUserDataTable)
		{
			var sqlText = @"-- GetLoginsToDrop
SELECT
	sp.name
FROM
	sys.server_principals AS sp LEFT OUTER JOIN @StaffUserTable AS s
		ON sp.name = s.Value COLLATE SQL_Latin1_General_CP1_CI_AS
WHERE
	(s.Value IS NULL)
AND (sp.type <> 'R')
AND (sp.name LIKE ISNULL(@StaffUserPattern, N'%'))
";

			var loginsToDrop = new List<StaffLoginInfo>();
			var message = string.Format(CultureInfo.InvariantCulture, "Failed to get logins to drop. Server: [{0}]", connection.ServerName);

			ExecuteWithLog(message, () =>
			{
				using (var cmd = connection.Command(sqlText))
				{
					string tvpTypeName = TVPHelper.GetTVPName(SqlDbType.NVarChar, 128);
					cmd.AddTableValuedParameter("@StaffUserTable", tvpTypeName, staffUserDataTable);
					cmd.AddParameter("@StaffUserPattern", SqlDbType.NVarChar, 100, specificStaffUserPattern);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							loginsToDrop.Add(new StaffLoginInfo()
							{
								StaffLoginAction = StaffLoginInfo.StaffLoginActions.Drop,
								LoginName = (string)reader["name"]
							});
						}
					}

					staffUserDataTable.Dispose();
				}
			});

			return loginsToDrop;
		}

		const string genericStaffUserPattern = DbUserRepository.StaffDbLoginPrefix + "[_]%";
		readonly string specificStaffUserPattern = DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)) + "%";
		public enum DatabaseAuthenticationMode { Windows, Sql }

		#endregion // Remove Old DB Logins and Users

		#region Grant Staff Permissions to New Database

		public void GrantStaffPermissionsToNewDatabase(AdminConnection connection, string dbName, DbConnection readConnection = null)
		{
			if (connection.IsDbWriteableAndOnline(dbName))
			{
				var staffThatShouldHaveDbLogins = GetUsersThatShouldHaveDbLogins(connection, skipInvalid: true, readConnection);

				var requiresReaderRole = staffThatShouldHaveDbLogins.Any(info => info.IsReadOnlyDBUser);
				if (requiresReaderRole)
				{
					EnsureReaderRoleExist(connection, dbName);
				}

				foreach (var user in staffThatShouldHaveDbLogins)
				{
					var cmd = connection.Command("SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT NULL FROM sys.server_principals WHERE name = @userLogin) THEN 1 ELSE 0 END)");
					cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, user.LoginName);

					if (Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture))
					{
						CreateDbUserWithPermissions(connection, dbName, user);
					}
				}
			}
		}

		#endregion // Grant Staff Permissions to New Database

		public List<string> GetStaffDbLogins(DbConnection mainDbConnection)
		{
			return GetUsersThatShouldHaveDbLogins(mainDbConnection).Select(usr => usr.LoginName).ToList();
		}

		public List<StaffLoginInfo> GetUsersThatShouldHaveDbLogins(DbConnection connection, bool skipInvalid = false, DbConnection readConnection = null)
		{
			var result = new List<StaffLoginInfo>();
			var staffCollection = GetStaffThatShouldHaveDbLogins(readConnection ?? connection);
			using (((ICurrentDbControl)connection).UseDatabase(Db.DatabaseName))
			{
				foreach (var staff in staffCollection)
				{
					var info = new StaffLoginInfo()
					{
						StaffLoginAction = StaffLoginInfo.StaffLoginActions.Create,

						LoginName = staff.GS_LoginName,
						LoginNameChanged = false,

						StaffPK = staff.PK.ToGuid(),
						PasswordChanged = false,

						StaffDatabaseAccessGroupRoles = staff.DatabaseAccessGroupRoles,

						DbPermissionChanged = false,
						GetDownLevelLogonName = staff.GetDownLevelLogonName,
					};

					if (!staff.MissingDirectoryEntry)
					{
						if (DbSupportsWindowsAuthentication(info.GetDownLevelLogonName))
						{
							var winInfo = StaffCopy(info);
							winInfo.LoginName = GetFullUserLoginName(connection.CurrentDatabase, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Windows);
							winInfo.DbAuthenticationMode = DatabaseAuthenticationMode.Windows;
							result.Add(winInfo);
						}

						if (DbSupportsSqlAuthentication())
						{
							if (!staff.GS_SqlLoginPasswordHash.IsEmpty)
							{
								var sqlInfo = StaffCopy(info);
								sqlInfo.LoginName = GetFullUserLoginName(connection.CurrentDatabase, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Sql);
								sqlInfo.DbAuthenticationMode = DatabaseAuthenticationMode.Sql;
								sqlInfo.HashedPassword = DataUtils.BytesToHexString(staff.GS_SqlLoginPasswordHash);

								if (IsLoginNameValid(sqlInfo, skipInvalid))
								{
									result.Add(sqlInfo);
								}
							}
							else
							{
								Logger?.Log(LogType.Warning,
									string.Format(CultureInfo.InvariantCulture,
									$"Skipped synchronising SQL login for staff record {staff.GS_LoginName}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password"));
							}
						}
					}
					else
					{
						Logger?.Log(LogType.Error, $@"Skipped synchronising staff and db logins for missing AD entity {staff.GS_LoginName}.");
					}
				}
			}

			return result;
		}

		internal GlbStaffCollection GetStaffThatShouldHaveDbLogins(DbConnection connection)
		{
			var databaseAccessRoles = DatabaseAccessGroupRoleTypes.AllDbRolesWithDescriptions.Keys.ToArray();

			var glbGroupRoleSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupRole), GlbGroupLinkSchema.GK_GG);
			glbGroupRoleSubQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, SQLComparisonOperator.Equal, databaseAccessRoles);

			var glbGroupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
			glbGroupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, GlbGroupRoleSchema.GGR_GG_Group, glbGroupRoleSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(GlbStaff)) { ReLoadExistingRows = true };
			query.AddToFilter(GlbStaffSchema.GS_IsActive, ZBool.True);
			query.AddSubQuery(GlbStaffSchema.PK, GlbGroupLinkSchema.GK_GS, glbGroupLinkSubQuery, JoinCondition.And);

			var factory = new BusinessObjectFactory(connection);
			return new GlbStaffCollection(factory, query);
		}

		#endregion // Synchronise Staff and Database Logins

		#region Create/Drop a Login

		bool CreateLoginWithPermissions(AdminConnection securityConnection, StaffLoginInfo info, string defaultDatabase)
		{
			if (CreateOrAlterSqlLogin(securityConnection, info, defaultDatabase))
			{
				return ManageServerLevelPermissions(securityConnection, info);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		bool CreateOrAlterSqlLogin(AdminConnection securityConnection, StaffLoginInfo info, string defaultDatabase)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				@"-- CreateOrAlterSqlLogin
DECLARE
	@isDisabled int = (SELECT is_disabled FROM sys.server_principals WHERE name = @userLogin);

if (@isDisabled is NULL)
begin
	{0}
end
else
begin
	DECLARE @isSqlLogin bit = CASE WHEN EXISTS (SELECT NULL FROM sys.sql_logins WHERE name = @userLogin) THEN 1 ELSE 0 END

	if (@passwordChanged = 1
		OR NOT EXISTS (SELECT NULL FROM sys.server_principals WHERE name = @userLogin AND default_database_name = @defaultDatabase AND default_language_name = N'us_english'))
	begin
		{1}
	end
	else if (@isSqlLogin = 1 AND @hashedPassword != '0x0')
	begin
		DECLARE @passwordHash varchar(256) = (SELECT sys.fn_varbintohexsubstring(1, convert(varbinary(256), password_hash), 1, 0) FROM sys.sql_logins WHERE name = @userLogin)
		if (@hashedPassword != @passwordHash)
		begin
			{1}
		end
	end

	if (@isDisabled = 1)
	begin
		{2}
	end
end
"
				, SQL_CreateLogin(info, defaultDatabase) // 0
				, SQL_AlterLogin(info, defaultDatabase)  // 1
				, SQL_EnableLogin(info)                  // 2
				);

			return ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, "Failed to create/alter login \"{0}\". Server: [{1}]", info.LoginName, securityConnection.ServerName)
				, () =>
				{
					using (var cmd = securityConnection.Command(sql))
					{
						cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);
						cmd.AddParameter("@defaultDatabase", SqlDbType.NVarChar, 128, defaultDatabase);
						cmd.AddParameter("@passwordChanged", SqlDbType.Bit, info.PasswordChanged);
						cmd.AddParameter("@hashedPassword", SqlDbType.VarChar, 256, info.HashedPassword ?? "0x0");
						cmd.ExecuteNonQuery();
					}
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		bool ManageServerLevelPermissions(AdminConnection connection, StaffLoginInfo info)
		{
			return ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, "Failed to setup permissions for login \"{0}\". Server: [{1}]", info.LoginName, connection.ServerName)
				, () =>
				{
					ManageServerLevelMembership(connection, info);

					var expectedServerPermissions = GetExpectedServerPermissions(connection, info);
					var actualServerPermissions = GetServerPermissions(connection, info);

					foreach (var item in Merge(expectedServerPermissions, actualServerPermissions))
					{
						if (item.Value == MergeItemActions.Create)
						{
							var permissionAction = expectedServerPermissions[item.Key];
							ChangePermission(connection, Db.SqlMasterDb, info, item.Key, permissionAction);
						}
						else
						{
							ChangePermission(connection, Db.SqlMasterDb, info, item.Key, PermissionStates.REVOKE);
						}
					}
				});
		}

		void ManageServerLevelMembership(AdminConnection connection, StaffLoginInfo info)
		{
			var expectedServerRoles = GetExpectedServerRoles();
			var actualServerRoles = GetServerRoles(connection, info);

			foreach (var item in Merge(expectedServerRoles, actualServerRoles))
			{
				ChangeServerMembership(connection, info, item.Key, item.Value);
			}
		}

		void ChangeServerMembership(AdminConnection connection, StaffLoginInfo info, string role, MergeItemActions action)
		{
			var actionString = (action == MergeItemActions.Create) ? "ADD" : "DROP";
			var sql = string.Format(CultureInfo.InvariantCulture,
				@"ALTER SERVER ROLE {0} {1} MEMBER {2};"
				, role.QuoteName()           // 0
				, actionString               // 1
				, info.LoginName.QuoteName() // 2
				);

			connection.ExecuteNonQuery(sql);
		}

		SortedDictionary<string, PermissionStates> GetExpectedServerRoles()
		{
			return new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);
		}

		SortedDictionary<string, PermissionStates> GetServerRoles(AdminConnection connection, StaffLoginInfo info)
		{
			var serverRoles = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			var sql = @"-- GetServerRoles
SELECT
	server_role_name = r.name
FROM
	sys.server_principals        AS r
	JOIN sys.server_role_members AS rm ON rm.role_principal_id = r.principal_id
	JOIN sys.server_principals   AS m  ON m.principal_id = rm.member_principal_id
WHERE
	m.name = @userLogin
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						serverRoles.Add((string)reader["server_role_name"], PermissionStates.GRANT);
					}
				}
			}

			return serverRoles;
		}

		bool IsHostedInWiseCloudSharedServer(DbConnection connection)
		{
			var isDedicatedSqlServerInstance = IsDedicatedDbServerInstance(connection.ServerInstanceName);
			return isHostedWithCargowise && !isDedicatedSqlServerInstance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Server Permission")]
		SortedDictionary<string, PermissionStates> GetExpectedServerPermissions(AdminConnection connection, StaffLoginInfo info)
		{
			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			result.Add("CONNECT SQL", PermissionStates.GRANT);

			var isHostedInWiseCloudSharedServer = IsHostedInWiseCloudSharedServer(connection);

			if (isHostedInWiseCloudSharedServer)
			{
				result.Add("VIEW ANY DATABASE", PermissionStates.DENY);
			}

			if (info.IsDatabaseDeveloper && !isHostedInWiseCloudSharedServer)
			{
				result.Add("ALTER TRACE", PermissionStates.GRANT);
				result.Add("VIEW SERVER STATE", PermissionStates.GRANT);
				result.Add("ALTER ANY EVENT SESSION", PermissionStates.GRANT);
				result.Add("VIEW ANY DEFINITION", PermissionStates.GRANT);
				if (connection.ServerVersionNumber.IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2022))
				{
					result.Add("VIEW ANY ERROR LOG", PermissionStates.GRANT);
				}
			}

			return result;
		}

		SortedDictionary<string, PermissionStates> GetServerPermissions(AdminConnection connection, StaffLoginInfo info)
		{
			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			var sql = @"-- GetServerPermissions
SELECT
	permission_name,
	state
FROM
	sys.server_principals       AS u
	join sys.server_permissions AS p ON p.grantee_principal_id = u.principal_id
WHERE 1=1
	AND u.name = @userLogin
";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var state = GetPermissionState(Convert.ToChar(reader["state"], CultureInfo.InvariantCulture));
						result.Add((string)reader["permission_name"], state);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a SQL Script")]
		string SQL_CreateLogin(StaffLoginInfo info, string defaultDatabase)
		{
			if (info.DbAuthenticationMode == DatabaseAuthenticationMode.Windows)
			{
				return Invariant($@"
BEGIN TRY
    CREATE LOGIN {info.LoginName.QuoteName()} FROM WINDOWS WITH DEFAULT_DATABASE = {defaultDatabase.QuoteName()}, DEFAULT_LANGUAGE = us_english;
END TRY
BEGIN CATCH
    IF 15025 = ERROR_NUMBER()
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = 'USE [master]' + CHAR(13) + CHAR(10)
        SELECT @sql = @sql + N'DROP LOGIN [' + name + ']' + CHAR(13) + CHAR(10)
        FROM sys.server_principals
        WHERE SID = SUSER_SID('{info.LoginName.QuoteEscapedName('\'')}')

        EXEC(@sql)
        CREATE LOGIN {info.LoginName.QuoteName()} FROM WINDOWS WITH DEFAULT_DATABASE = {defaultDatabase.QuoteName()}, DEFAULT_LANGUAGE = us_english;
    END
    ELSE THROW -- rethrow
END CATCH
");
			}

			var loginSid = "";
			if (!string.IsNullOrWhiteSpace(info.Sid))
			{
				loginSid = ", SID = " + info.Sid;
			}

			var hashedPassword = info.HashedPassword + " HASHED";

			return string.Format(CultureInfo.InvariantCulture,
				"CREATE LOGIN {0} WITH PASSWORD = {1}, DEFAULT_DATABASE = {2}{3}, CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english;"
				, info.LoginName.QuoteName()           // 0
				, hashedPassword                       // 1
				, defaultDatabase.QuoteName()          // 2
				, loginSid                             // 3
			);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query")]
		string SQL_AlterLogin(StaffLoginInfo info, string defaultDatabase)
		{
			if (info.DbAuthenticationMode == DatabaseAuthenticationMode.Windows)
			{
				return string.Format(CultureInfo.InvariantCulture,
					"ALTER LOGIN {0} WITH DEFAULT_DATABASE = {1}, DEFAULT_LANGUAGE = us_english;"
					, info.LoginName.QuoteName()  // 0
					, defaultDatabase.QuoteName() // 1
					);
			}
			else
			{
				var passwordHash = (string.IsNullOrEmpty(info.HashedPassword) ?  "0x0" : info.HashedPassword) + " HASHED";

				return string.Format(CultureInfo.InvariantCulture,
					"ALTER LOGIN {0} WITH PASSWORD = {1}, CHECK_POLICY = OFF, DEFAULT_DATABASE = {2}, DEFAULT_LANGUAGE = us_english;"
					, info.LoginName.QuoteName()    // 0
					, passwordHash                  // 1
					, defaultDatabase.QuoteName()   // 2
					);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query")]
		string SQL_RenameLogin(StaffLoginInfo info)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"ALTER LOGIN {0} WITH NAME = {1};"
				, info.OldLoginName.QuoteName() // 0
				, info.LoginName.QuoteName()    // 1
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Direct SQL query")]
		protected string SQL_EnableLogin(StaffLoginInfo info)
		{
			try
			{
				return !info.GetDownLevelLogonName().IsNullOrEmpty()
					? string.Format(CultureInfo.InvariantCulture, "ALTER LOGIN {0} ENABLE;", info.LoginName.QuoteName())
					: new DbTryCatchWrapper().GetEnableDbLoginCommandSafe(info.LoginName.QuoteEscapedName());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger?.Log(LogType.Error, "Failed to enable login.", ex);
				return null;
			}
		}

		#endregion // Create/Drop a Login

		#region Create/Drop a User

		void CreateDbUserWithPermissions(AdminConnection connection, StaffLoginInfo info)
		{
			CreateUserRepositoryDatabaseIfNeeded(connection, info);

			foreach (var dbName in GetEnterpriseDbs(connection))
			{
				CreateDbUserWithPermissionsOnDatabase(connection, dbName, info);
			}
		}

		void CreateDbUserWithPermissionsOnBiDatabase(AdminConnection biConnection, string dbName, StaffLoginInfo info)
		{
			var requiresReaderRole = info.IsReadOnlyDBUser;

			if (biConnection.IsDbWriteableAndOnline(dbName))
			{
				if (requiresReaderRole)
				{
					EnsureReaderRoleExist(biConnection, dbName);
				}

				CreateDbUserWithPermissions(biConnection, dbName, info);
			}
		}

		void CreateDbUserWithPermissionsOnDatabase(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			var requiresReaderRole = info.IsReadOnlyDBUser;

			if (connection.IsDbWriteableAndOnline(dbName))
			{
				if (requiresReaderRole)
				{
					EnsureReaderRoleExist(connection, dbName);
				}

				CreateDbUserWithPermissions(connection, dbName, info);
			}
		}

		void CreateDbUserWithPermissions(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			if (connection.IsDbWriteableAndOnline(dbName) && CreateDbUser(connection, dbName, info))
			{
				ManageDatabasePermissions(connection, dbName, info);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		bool CreateDbUser(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, SQL_BaseCreateUser, dbName);
			return ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, "Failed to create database user \"{0}\". Database: [{1}]. This can be because of a pre-existing database user for the associated login.", info.LoginName, dbName)
				, LogType.Warning
				, () =>
				{
					try
					{
						ExecuteCreateDbUser();
					}
					catch (SqlException sqlException) when (sqlException.Number == 15063)
					{
						DropAnotherBlockingUserHavingSameSid();
						ExecuteCreateDbUser();
					}

					void ExecuteCreateDbUser()
					{
						using (var cmd = connection.Command(sql))
						{
							cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);
							cmd.AddParameter("@impersonatedToUserLogin", SqlDbType.NVarChar, 128, UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName));
							cmd.ExecuteNonQuery();
						}
					}

					void DropAnotherBlockingUserHavingSameSid()
					{
						var script = Invariant($@"
SELECT name
FROM {dbName.QuoteName()}.sys.database_principals
WHERE 1=1
    AND sid = SUSER_SID(@userLogin)
    AND name != @userLogin
");
						var oldUserName = connection.ExecuteScalar(script, cmd =>
						{
							cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);
						});

						if (oldUserName != null && oldUserName != DBNull.Value)
						{
							script = Invariant($@"
DECLARE	@execCmd nvarchar(4000);

{string.Format(CultureInfo.InvariantCulture, SQL_BaseDropUser, dbName)}
");
							using (var cmd = connection.Command(script))
							{
								cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, oldUserName);
								cmd.ExecuteNonQuery();
							}
						}
					}
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		void ManageDatabasePermissions(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, "Failed to setup permissions for database user \"{0}\". Database: [{1}]", info.LoginName, dbName)
				, () =>
				{
					ManageDatabaseLevelMembership(connection, dbName, info);

					var expectedDatabasePermissions = GetExpectedDatabasePermissions(dbName, info);
					var actualDatabasePermissions = GetDatabasePermissions(connection, dbName, info);

					foreach (var item in Merge(expectedDatabasePermissions, actualDatabasePermissions))
					{
						if (item.Value == MergeItemActions.Create)
						{
							var permissionAction = expectedDatabasePermissions[item.Key];
							ChangePermission(connection, dbName, info, item.Key, permissionAction);
						}
						else
						{
							ChangePermission(connection, dbName, info, item.Key, PermissionStates.REVOKE);
						}
					}
				});
		}

		void ManageDatabaseLevelMembership(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			var expectedDatabaseRoles = GetExpectedDatabaseRoles(dbName, info);
			var actualDatabaseRoles = GetDatabaseRoles(connection, dbName, info);

			foreach (var item in Merge(expectedDatabaseRoles, actualDatabaseRoles))
			{
				ChangeDatabaseMembership(connection, dbName, info, item.Key, item.Value);
			}
		}

		void ChangeDatabaseMembership(AdminConnection connection, string dbName, StaffLoginInfo info, string roleName, MergeItemActions action)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				var dbRole = DbRoleTypes.AllDbRoles.FirstOrDefault(role => role.Name == roleName);
				if (dbRole != null)
				{
					dbRole.CreateRoleAndPermissionGrantCommandsIfRequired(connection, dbName, out var cmdBuilder);

					var stmt = cmdBuilder.ToString().Trim();
					if (stmt.Length > 0)
					{
						connection.ExecuteNonQuery(stmt);
					}
				}

				var actionString = (action == MergeItemActions.Create) ? "ADD" : "DROP";
				var sql = string.Format(CultureInfo.InvariantCulture,
					@"ALTER ROLE {0} {1} MEMBER {2}"
					, roleName.QuoteName()           // 0
					, actionString               // 1
					, info.LoginName.QuoteName() // 2
					);

				connection.ExecuteNonQuery(sql);
			}
		}

		SortedDictionary<string, PermissionStates> GetExpectedDatabaseRoles(string dbName, StaffLoginInfo info)
		{
			var dbType = GetDbTypeFromDbName(dbName);
			var roles = info.GetApplicableRoles(dbType);

			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);
			foreach (var role in roles)
			{
				result.Add(role, PermissionStates.GRANT);
			}
			return result;
		}

		SortedDictionary<string, PermissionStates> GetDatabaseRoles(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- GetDatabaseRoles
SELECT
	database_role_name = r.name
FROM
	{0}.sys.database_principals        AS r
	JOIN {0}.sys.database_role_members AS rm ON rm.role_principal_id = r.principal_id
	JOIN {0}.sys.database_principals   AS m  ON m.principal_id = rm.member_principal_id
WHERE
	m.name = @userLogin
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add((string)reader["database_role_name"], PermissionStates.GRANT);
					}
				}
			}

			return result;
		}

		SortedDictionary<string, PermissionStates> GetExpectedDatabasePermissions(string dbName, StaffLoginInfo info)
		{
			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			DbSecurity.CommonPermissionList.ForEach(p => result.Add(p, PermissionStates.GRANT));

			if (string.Equals(dbName, UserRepositoryDb, StringComparison.OrdinalIgnoreCase))
			{
				if (info.IsReadOnlyDBUser)
				{
					DbSecurity.ReadOnlyDBUserPermissionList.ForEach(p => result.Add(p, PermissionStates.GRANT));
				}

				if (info.IsDatabaseDeveloper)
				{
					DbSecurity.DatabaseDeveloperPermissionList.ForEach(p => result.Add(p, PermissionStates.GRANT));
					DbSecurityLockDown.DatabaseDeveloperSchemaPermissionList.ForEach(p => result.Add($"{p} ON SCHEMA::[dbo]", PermissionStates.GRANT));
				}
			}

			return result;
		}

		SortedDictionary<string, PermissionStates> GetDatabasePermissions(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			var result = new SortedDictionary<string, PermissionStates>(StringComparer.OrdinalIgnoreCase);

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- GetDatabasePermissions
SELECT
	permission_name =
		CASE
			WHEN p.class = 3 THEN p.permission_name + N' ON SCHEMA::' + QUOTENAME(s.name) COLLATE database_default
			ELSE p.permission_name
		END,
	state
FROM
	{0}.sys.database_principals       AS u
	JOIN {0}.sys.database_permissions AS p ON p.grantee_principal_id = u.principal_id
	LEFT JOIN {0}.sys.schemas         AS s ON s.schema_id = p.major_id AND p.class = 3
WHERE 1=1
	AND u.name = @userLogin
	AND p.class in (0, 3)
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var state = GetPermissionState(Convert.ToChar(reader["state"], CultureInfo.InvariantCulture));
						result.Add((string)reader["permission_name"], state);
					}
				}
			}

			return result;
		}

		#endregion // Create/Drop a User

		#region Base scripts

		const string SQL_BaseDropUser = @"-- BaseDropUser
SET @execCmd = NULL;

-- Drop owned schemas
SELECT
	@execCmd = ISNULL(@execCmd, N'')
		+ N'DROP SCHEMA ' + QUOTENAME(name) + N';'
FROM
	[{0}].sys.schemas
WHERE
	principal_id = (SELECT principal_id FROM [{0}].sys.database_principals WHERE name = @userLogin);

if (@execCmd is NOT NULL) EXEC [{0}].sys.sp_executesql @execCmd;

-- Revoke Impersonate
SET @execCmd = NULL
;
SELECT
	@execCmd = isnull(@execCmd + ';', '') +
	' REVOKE ' + perm.permission_name + ' ON USER::' + QUOTENAME(dp_grantor.name) +
	' FROM ' + QUOTENAME(dp_grantee.name)
FROM
	[{0}].sys.database_permissions perm
	join [{0}].sys.database_principals dp_grantee on dp_grantee.principal_id = perm.grantee_principal_id
	join [{0}].sys.database_principals dp_grantor on dp_grantor.principal_id = perm.grantor_principal_id
WHERE
	perm.class = 4
	AND perm.type = 'IM'
	AND dp_grantor.name = @userLogin
;
if (@execCmd is NOT NULL) EXEC [{0}].sys.sp_executesql @execCmd;

-- Drop User
SET @execCmd = N'DROP USER ' + QUOTENAME(@userLogin);
EXEC [{0}].sys.sp_executesql @execCmd;
";

		const string SQL_BaseCreateUser = @"-- BaseCreateUser
DECLARE
	@execCmd nvarchar(4000);

if (EXISTS (SELECT NULL FROM [{0}].sys.database_principals WHERE name = @userLogin))
begin
	-- if sid does not match, drop and re-create
	if (NOT EXISTS (
			SELECT NULL
			FROM
				[{0}].sys.database_principals AS usr
				JOIN sys.server_principals AS log ON log.sid = usr.sid
			WHERE
				usr.name = @userLogin
				AND log.name = @userLogin
		))
	begin
" + SQL_BaseDropUser + @"
		SET @execCmd = N'CREATE USER ' + QUOTENAME(@userLogin);
		EXEC [{0}].sys.sp_executesql @execCmd;
	end;
end
else
begin
	SET @execCmd = 'CREATE USER ' + QUOTENAME(@userLogin);
	EXEC [{0}].sys.sp_executesql @execCmd;
end;

if (EXISTS (SELECT NULL FROM [{0}].sys.database_principals WHERE name = @impersonatedToUserLogin))
begin
	SET @execCmd = 'GRANT IMPERSONATE ON USER:: ' + QUOTENAME(@userLogin) + ' TO ' + QUOTENAME(@impersonatedToUserLogin);
	EXEC [{0}].sys.sp_executesql @execCmd;
end;
";

		#endregion // Base scripts

		#region Permissions

		enum PermissionStates { DENY, REVOKE, GRANT, GRANT_WITH_GRANT_OPTION }

		PermissionStates GetPermissionState(char state)
		{
			switch (state)
			{
				case 'D':
					return PermissionStates.DENY;
				case 'R':
					return PermissionStates.REVOKE;
				case 'G':
					return PermissionStates.GRANT;
				case 'W':
					return PermissionStates.GRANT_WITH_GRANT_OPTION;
				default:
					throw new ArgumentException("Permission state must be one of the following: D,R,G,W");
			}
		}

		enum MergeItemActions { Create, Drop }

		Dictionary<string, MergeItemActions> Merge(SortedDictionary<string, PermissionStates> expectedList, SortedDictionary<string, PermissionStates> actualList)
		{
			var result = new Dictionary<string, MergeItemActions>();

			var ind_expected = 0;
			var ind_actual = 0;
			while (ind_expected < expectedList.Count && ind_actual < actualList.Count)
			{
				var expected = expectedList.ElementAt(ind_expected);
				var actual = actualList.ElementAt(ind_actual);
				var compared = string.Compare(expected.Key, actual.Key, StringComparison.OrdinalIgnoreCase);
				if (compared < 0)
				{
					ind_expected++;
					result.Add(expected.Key, MergeItemActions.Create);
				}
				else if (compared == 0)
				{
					ind_expected++;
					ind_actual++;
					if (expected.Value != actual.Value)
					{
						result.Add(expected.Key, MergeItemActions.Create);
					}
				}
				else
				{
					ind_actual++;
					result.Add(actual.Key, MergeItemActions.Drop);
				}
			}

			if (ind_expected < expectedList.Count)
			{
				foreach (var item in expectedList.Skip(ind_expected))
				{
					result.Add(item.Key, MergeItemActions.Create);
				}
			}
			else if (ind_actual < actualList.Count)
			{
				foreach (var item in actualList.Skip(ind_actual))
				{
					result.Add(item.Key, MergeItemActions.Drop);
				}
			}

			return result;
		}

		void ChangePermission(AdminConnection connection, string dbName, StaffLoginInfo info, string permission, PermissionStates permissionAction)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				@"DECLARE @stmt nvarchar(1000) = N'{0} {1} TO ' + QUOTENAME(@userLogin) + N';'; EXEC {2}.sys.sp_executesql @stmt;"
				, permissionAction.ToString() // 0
				, permission                  // 1
				, dbName.QuoteName()          // 2
				);

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, info.LoginName);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion // Permissions

		#region Implementation

		readonly ILogger Logger;

		internal string GetFullUserLoginName(string databaseName, string staffLoginName, Func<string> getDownLevelLogonName, DatabaseAuthenticationMode dbAuthenticationMode)
		{
			if (dbAuthenticationMode == DatabaseAuthenticationMode.Sql)
			{
				return new ZString(DbUserRepository.GetStaffDbLoginFullPrefix(databaseName) + staffLoginName).SubstringSafe(0, 128);
			}
			else
			{
				try
				{
					return new ZString(getDownLevelLogonName()).SubstringSafe(0, 128);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Logger?.Log(LogType.Error, (NoResString)"Failed to get Full User Login Name.", ex);
					return null;
				}
			}
		}

		internal bool DbSupportsWindowsAuthentication(Func<string> getDownLevelLogonName)
		{
			try
			{
				return ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled && !getDownLevelLogonName().IsNullOrEmpty();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger?.Log(LogType.Error, (NoResString)"Failed to determine if Windows Authentication is supported.", ex);
				return false;
			}
		}

		internal bool DbSupportsSqlAuthentication()
		{
			return !ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled ||
				EnvProxy.IsHostedWithCargowise;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		bool IsLoginNameValid(StaffLoginInfo info, bool skipInvalid = false)
		{
			if (new ZString(info.LoginName).ContainsAnyChar(GlbStaffValidation.InvalidLoginNameCharacters))
			{
				var message = string.Format(CultureInfo.InvariantCulture,
					"Failed to create login for user \"{0}\". The user's login contains invalid characters ({1})"
					, info.LoginName
					, GlbStaffValidation.InvalidLoginNameCharacters
					);

				if (Logger != null)
				{
					Logger.Warning(message);

					return false;
				}

				if (skipInvalid)
				{
					return false;
				}
				else
				{
					throw new InvalidOperationException(message);
				}
			}

			return true;
		}

		void CreateUserRepositoryDatabaseIfNeeded(AdminConnection connection, List<StaffLoginInfo> infos)
		{
			foreach (var info in infos)
			{
				if (CreateUserRepositoryDatabaseIfNeeded(connection, info))
				{
					return;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging information")]
		bool CreateUserRepositoryDatabaseIfNeeded(AdminConnection connection, StaffLoginInfo info)
		{
			if (info.IsDatabaseDeveloper)
			{
				CreateUserRepositoryDatabaseIfMissing(connection);

				return true;
			}

			return false;
		}

		bool CreateUserRepositoryDatabaseIfMissing(AdminConnection adminConnection)
		{
			if (!adminConnection.DatabaseExists(UserRepositoryDb))
			{
					return ExecuteWithLog(string.Format(CultureInfo.InvariantCulture, (NoResString)"Failed to create User repository database \"{0}\"", UserRepositoryDb)
					, () =>
					{
						// Use a new connection as CREATE DATABASE is not allowed in a transaction
						new DbUserRepository().CreateRepositoryDatabase();
					});
			}

			return false;
		}

		public static void CreateUserRepositoryDatabaseIfMissing()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				new DbUserManager().CreateUserRepositoryDatabaseIfMissing(adminConnection);
			}
		}

		bool ExecuteWithLog(string message, Action action)
		{
			return ExecuteWithLog(message, LogType.Error, action);
		}

		bool ExecuteWithLog(string message, LogType logType, Action action)
		{
			try
			{
				action();
				return true;
			}
			catch (Exception e) when (e.IsCriticalException()) { throw; }
			catch (SqlException ex)
			{
				if (Logger != null)
				{
					Logger.Log(logType, message, ex);
				}
				else
				{
					throw;
				}
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Column name of DataTable")]
		DataTable GetDataTableOfStaffProxyUsersList(List<StaffLoginInfo> infos)
		{
			string tvpTypeName = TVPHelper.GetTVPName(SqlDbType.NVarChar, 128);

			var table = new DataTable(tvpTypeName) { Locale = CultureInfo.InvariantCulture };
			var column = new DataColumn("Value", typeof(string)) { AllowDBNull = false };
			table.Columns.Add(column);
			table.PrimaryKey = new DataColumn[] { column };

			foreach (var info in infos)
			{
				if (!table.Rows.Contains(info.LoginName))
				{
					table.Rows.Add(info.LoginName);
				}
			}

			return table;
		}

		bool IsDedicatedDbServerInstance(string serverInstanceName)
		{
			return DbEnv.Instance.DedicatedSqlServerInstanceDefinition.IsDedicated(serverInstanceName);
		}

		readonly bool isHostedWithCargowise = EnvProxy.IsHostedWithCargowise;

		public static byte[] GetRandomPasswordHash()
		{
			using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
			{
				var tokenData = new byte[24];
				rng.GetBytes(tokenData);
				return SqlLoginPasswordHashGenerator.GenerateFromDb(Db.Connection, Convert.ToBase64String(tokenData));
			}
		}

		void EnsureReaderRoleExist(AdminConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				new CwRestrictedReaderRole().CreateRoleAndPermissionGrantCommandsIfRequired(connection, dbName, out var cmdBuilder);

				var stmt = cmdBuilder.ToString().Trim();
				if (stmt.Length > 0)
				{
					connection.ExecuteNonQuery(stmt);
				}
			}
		}

		HashSet<string> GetEnterpriseDbs(DbConnection securityConnection)
		{
			if (enterpriseDbs == null)
			{
				string securityConnectionInitialDb = ((ICurrentDbControl)securityConnection).InitialDatabase;
				if (!string.Equals(securityConnectionInitialDb, Db.DatabaseName, StringComparison.OrdinalIgnoreCase))
				{
					ErrorReporter.ReportOnce("GetEnterpriseDbs_DifferentInitialDb",
						string.Format(
							"[GetEnterpriseDbs] Connection initial database must match the application main database. Db.DatabaseName = {0}; This Connection Initial DB = {1};",
							Db.DatabaseName, securityConnectionInitialDb));
				}

				enterpriseDbs = new HashSet<string>(securityConnection.GetDatabases(DatabaseType.AllWritable, writable: true), StringComparer.OrdinalIgnoreCase);
			}

			return enterpriseDbs;
		}
		HashSet<string> enterpriseDbs;

		internal static readonly string UserRepositoryDb = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;

		internal enum DbType { eDocs, ReferenceDatabase, UserRepository, Other }

		static DbType GetDbTypeFromDbName(string dbName)
		{
			if (RefDbTableNameResolver.IsExclusiveDatabase(Db.DatabaseName, dbName))
			{
				return DbType.ReferenceDatabase;
			}

			if (dbName.StartsWith(string.Format(CultureInfo.CurrentCulture, "{0}{1}", Db.DatabaseName, Db.SDDatabaseAffix), StringComparison.OrdinalIgnoreCase))
			{
				return DbType.eDocs;
			}

			if (string.Equals(dbName, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return DbType.UserRepository;
			}

			return DbType.Other;
		}

		void LogEmailSendFailedException(EmailSendFailedException ex)
		{
			Logger?.Log(LogType.Error, (NoResString)"Couldn't send email: " + ex.Message); //If Logger is null, this exception has to be swallowed.
		}

		protected virtual void PropagateLogin(List<StaffLoginInfo> infoList, Action<AdminConnection, StaffLoginInfo> loginAction)
		{
			var task = LoginPropagation.PropagateLogin(Logger, infoList, loginAction);
#if DEBUG
			task?.Wait(TimeSpan.FromSeconds(30));
#endif
		}

		#endregion // Implementation

		#region StaffLoginInfo

		public class StaffLoginInfo
		{
			protected internal enum StaffLoginActions { None, Create, Alter, Drop }

			public StaffLoginInfo()
			{
			}

			internal static Action New(
				StaffLoginActions staffLoginAction
				, string old_LoginName, string new_LoginName
				, Guid staffPK
				, bool old_IsActive, bool new_IsActive
				, ISet<string> old_StaffDatabaseAccessGroupRoles, ISet<string> new_StaffDatabaseAccessGroupRoles
				, Func<string> getDownLevelLogonName
				, DatabaseAuthenticationMode dbAuthenticationMode
				, string new_PasswordHash = null
				)
			{
				if (staffLoginAction == StaffLoginActions.Create)
				{
					return CreateLogin(new_LoginName, new_PasswordHash, staffPK, new_IsActive, new_StaffDatabaseAccessGroupRoles, getDownLevelLogonName, dbAuthenticationMode);
				}
				else if (staffLoginAction == StaffLoginActions.Alter)
				{
					return AlterLogin(
						old_LoginName, new_LoginName
						, staffPK, new_PasswordHash
						, old_IsActive, new_IsActive
						, old_StaffDatabaseAccessGroupRoles, new_StaffDatabaseAccessGroupRoles
						, getDownLevelLogonName
						, dbAuthenticationMode
						);
				}
				else if (staffLoginAction == StaffLoginActions.Drop)
				{
					return DropLogin(old_LoginName, staffPK, old_IsActive, old_StaffDatabaseAccessGroupRoles, getDownLevelLogonName);
				}

				return null;
			}

			static Action CreateLogin(string new_LoginName, string new_PasswordHash, Guid staffPK, bool new_IsActive, ISet<string> new_StaffDatabaseAccessGroupRoles, Func<string> getDownLevelLogonName, DatabaseAuthenticationMode dbAuthenticationMode)
			{
				if (new_IsActive && (new_StaffDatabaseAccessGroupRoles != null && new_StaffDatabaseAccessGroupRoles.Count > 0))
				{
					if (new ZString(new_LoginName).ContainsAnyChar(GlbStaffValidation.InvalidLoginNameCharacters))
					{
						throw new InvalidOperationException("Could not add login to database. Login name: \"" + new_LoginName + "\" contains invalid characters");
					}

					var info = new StaffLoginInfo()
					{
						StaffLoginAction = StaffLoginActions.Create,
						LoginNameChanged = true,
						LoginName = new_LoginName,
						StaffPK = staffPK,
						PasswordChanged = true,
						HashedPassword = new_PasswordHash,
						DbPermissionChanged = true,
						StaffDatabaseAccessGroupRoles = new_StaffDatabaseAccessGroupRoles,
						GetDownLevelLogonName = getDownLevelLogonName,
						DbAuthenticationMode = dbAuthenticationMode
					};

					return () => new DbUserManager().CreateStaffLogin(info);
				}

				return null;
			}

			static Action AlterLogin(
				string old_LoginName, string new_LoginName
				, Guid staffPK, string new_PasswordHash
				, bool old_IsActive, bool new_IsActive
				, ISet<string> old_StaffDatabaseAccessGroupRoles, ISet<string> new_StaffDatabaseAccessGroupRoles
				, Func<string> getDownLevelLogonName
				, DatabaseAuthenticationMode dbAuthenticationMode)
			{
				var old_LoginExists = old_IsActive && (old_StaffDatabaseAccessGroupRoles != null && old_StaffDatabaseAccessGroupRoles.Any());
				var new_LoginExists = new_IsActive && (new_StaffDatabaseAccessGroupRoles != null && new_StaffDatabaseAccessGroupRoles.Any());
				if (old_LoginExists)
				{
					if (new_LoginExists)
					{
						var loginChanged = old_LoginName != new_LoginName;
						var passwordChanged = !new_PasswordHash.IsNullOrEmpty();
						var dbPermissionChanged = (new_StaffDatabaseAccessGroupRoles.Count != old_StaffDatabaseAccessGroupRoles.Count) ||
														new_StaffDatabaseAccessGroupRoles.Any(new_group => !old_StaffDatabaseAccessGroupRoles.Contains(new_group))
						;

						var hasChanges =
							loginChanged
							|| passwordChanged
							|| dbPermissionChanged
							;

						if (hasChanges)
						{
							if (new ZString(new_LoginName).ContainsAnyChar(GlbStaffValidation.InvalidLoginNameCharacters))
							{
								throw new InvalidOperationException("Could not alter database login. Login name: \"" + new_LoginName + "\" contains invalid characters");
							}

							var info = new StaffLoginInfo()
							{
								StaffLoginAction = StaffLoginActions.Alter,
								LoginNameChanged = loginChanged,
								OldLoginName = old_LoginName,
								LoginName = new_LoginName,
								StaffPK = staffPK,
								PasswordChanged = passwordChanged,
								HashedPassword = new_PasswordHash,
								DbPermissionChanged = dbPermissionChanged,
								StaffDatabaseAccessGroupRoles = new_StaffDatabaseAccessGroupRoles,
								GetDownLevelLogonName = getDownLevelLogonName,
								DbAuthenticationMode = dbAuthenticationMode
							};

							return () => new DbUserManager().AlterStaffLogin(info);
						}
					}
					else
					{
						return DropLogin(old_LoginName, staffPK, old_IsActive, old_StaffDatabaseAccessGroupRoles, getDownLevelLogonName);
					}
				}
				else
				{
					if (new_LoginExists)
					{
						return CreateLogin(new_LoginName, new_PasswordHash, staffPK, new_IsActive, new_StaffDatabaseAccessGroupRoles, getDownLevelLogonName, dbAuthenticationMode);
					}
				}

				return null;
			}

			static Action DropLogin(string old_LoginName, Guid staffPK, bool old_IsActive, ISet<string> old_StaffDatabaseAccessGroupRoles, Func<string> getDownLevelLogonName)
			{
				if (old_IsActive && (old_StaffDatabaseAccessGroupRoles != null && old_StaffDatabaseAccessGroupRoles.Count > 0))
				{
					var info = new StaffLoginInfo()
					{
						StaffLoginAction = StaffLoginActions.Drop,
						LoginNameChanged = false,
						LoginName = old_LoginName,
						StaffPK = staffPK,
						PasswordChanged = false,
						DbPermissionChanged = false,
						StaffDatabaseAccessGroupRoles = old_StaffDatabaseAccessGroupRoles,
						GetDownLevelLogonName = getDownLevelLogonName,
					};

					return () => new DbUserManager().DropStaffLogin(info);
				}

				return null;
			}

			protected internal StaffLoginActions StaffLoginAction;

			public string Sid;

			public string OldLoginName;
			public string LoginName;
			public bool LoginNameChanged;

			public Guid StaffPK;
			public bool PasswordChanged;

			public string HashedPassword;

			public bool DbPermissionChanged;
			public ISet<string> StaffDatabaseAccessGroupRoles;
			public bool IsReadOnlyDBUser => StaffDatabaseAccessGroupRoles.Contains(DbRoleTypes.CwRestrictedReaderRole);
			public bool IsDatabaseDeveloper => StaffDatabaseAccessGroupRoles.Contains(DbRoleTypes.DbDataWriterRole);
			public bool IsBackupOperator => StaffDatabaseAccessGroupRoles.Contains(DbRoleTypes.DbBackupOperatorRole);
			public Func<string> GetDownLevelLogonName;
			public DatabaseAuthenticationMode DbAuthenticationMode;

			internal IEnumerable<string> GetApplicableRoles(DbType dbType)
			{
				var result = new List<string>();

				foreach (var role in StaffDatabaseAccessGroupRoles)
				{
					if (role != DbRoleTypes.DbDataWriterRole || dbType == DbType.UserRepository)
					{
						result.Add(role);
					}
				}

				return result;
			}
		}

		public static StaffLoginInfo StaffCopy(StaffLoginInfo info)
		{
			var copyInfo = new StaffLoginInfo();
			copyInfo.StaffLoginAction = info.StaffLoginAction;
			copyInfo.LoginNameChanged = info.LoginNameChanged;
			copyInfo.OldLoginName = info.OldLoginName;
			copyInfo.LoginName = info.LoginName;
			copyInfo.StaffPK = info.StaffPK;
			copyInfo.PasswordChanged = info.PasswordChanged;
			copyInfo.DbPermissionChanged = info.DbPermissionChanged;
			copyInfo.StaffDatabaseAccessGroupRoles = info.StaffDatabaseAccessGroupRoles;
			copyInfo.GetDownLevelLogonName = info.GetDownLevelLogonName;
			copyInfo.HashedPassword = info.HashedPassword;
			copyInfo.Sid = info.Sid;
			copyInfo.DbAuthenticationMode = info.DbAuthenticationMode;

			return copyInfo;
		}

		public static bool StaffUserOwnsSchema(DbConnection connection, string userName, Func<string> getDownLevelLogonName)
		{
			var dbUserManager = new DbUserManager();

			var sqlLogin = dbUserManager.GetFullUserLoginName(Db.DatabaseName, userName, getDownLevelLogonName, DatabaseAuthenticationMode.Sql);
			var adLogin = dbUserManager.GetFullUserLoginName(Db.DatabaseName, userName, getDownLevelLogonName, DatabaseAuthenticationMode.Windows);

			const string sql = @"-- StaffLoginOwnsSchema
FROM
	sys.schemas AS sch
WHERE 1=1
	AND sch.principal_id in (DATABASE_PRINCIPAL_ID(@sqlLogin), DATABASE_PRINCIPAL_ID(@adLogin))
";

			return connection.Exists(sql, cmd =>
			{
				cmd.CommandTimeout = 30;
				cmd.AddParameter("@sqlLogin", SqlDbType.NVarChar, 128, string.IsNullOrEmpty(sqlLogin) ? NoValue : sqlLogin);
				cmd.AddParameter("@adLogin", SqlDbType.NVarChar, 128, string.IsNullOrEmpty(adLogin) ? NoValue : adLogin);
			});
		}

		public static bool StaffLoginOwnsSchemaWithCurrentObjects(DbConnection connection, string userName, Func<string> getDownLevelLogonName)
		{
			var dbUserManager = new DbUserManager();
			var sqlLogin = dbUserManager.GetFullUserLoginName(Db.DatabaseName, userName, getDownLevelLogonName, DatabaseAuthenticationMode.Sql);
			var adLogin = dbUserManager.GetFullUserLoginName(Db.DatabaseName, userName, getDownLevelLogonName, DatabaseAuthenticationMode.Windows);
			foreach (var db in connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef))
			{
				using (((ICurrentDbControl)connection).UseDatabase(db))
				{
					const string sql = @"-- StaffLoginOwnsSchemaWithCurrentObjects
FROM
	sys.schemas      AS sch
	JOIN sys.objects AS obj ON obj.schema_id = sch.schema_id
WHERE 1=1
	AND sch.principal_id in (DATABASE_PRINCIPAL_ID(@sqlLogin), DATABASE_PRINCIPAL_ID(@adLogin))
";

					if (connection.Exists(sql, cmd =>
					{
						cmd.CommandTimeout = 30;
						cmd.AddParameter("@sqlLogin", SqlDbType.NVarChar, 128, string.IsNullOrEmpty(sqlLogin) ? NoValue : sqlLogin);
						cmd.AddParameter("@adLogin", SqlDbType.NVarChar, 128, string.IsNullOrEmpty(adLogin) ? NoValue : adLogin);
					}))
					{
						return true;
					}
				}
			}

			return false;
		}
		readonly static string NoValue = "NOVALUEPROVIDED";

		#endregion // StaffLoginInfo

		#region

		public void EnsureApplicationLoginForBiDatabase(string serverName, string dbName)
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			using (var biConnection = GetBiConnection(serverName))
			{
				EnsureAppLoginsMappedToBiDatabase(biConnection, dbName);
			}
		}

		void EnsureAppLoginsMappedToBiDatabase(AdminConnection biConnection, string dbName)
		{
			if (biConnection.DatabaseExists(dbName))
			{
				using (((ICurrentDbControl)biConnection).UseDatabase(dbName))
				{
					((IDbLoginRepair)biConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
				}
			}
		}

		#endregion
	}

	#region ReplicationResult

	class ReplicationResult
	{
		readonly ConcurrentBag<Tuple<string, string, string>> notificationsList = new ConcurrentBag<Tuple<string, string, string>>();

		#region Properties

		public ConcurrentBag<Tuple<string, string, string>> NotificationsList { get { return notificationsList; } }

		#endregion

		public void NewNotification(Tuple<string, string, string> notification)
		{
			notificationsList.Add(notification);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "Baseline")]
		void SendEmail(string subject, string body)
		{
			EmailGroupUtility emailUntility = new EmailGroupUtility();
			StringCollection groupEmails = emailUntility.SendNotificationToDatabaseAdministrator(NotificationDataRegistry.Instance.DatabaseHealthCheckNotificationGroup.Value, false);

			if (groupEmails.Count == 0)
			{
				groupEmails = emailUntility.GetCompanyNotificationGroupEmails();
				body = Res.GetString("58154992-7908-45b6-bc56-f3b4b0ca3cea", "Database Health Check Notification Group does not contain any users with email addresses. Email being sent to current company Notification Group instead.\r\n{0}", body);
			}

			if (groupEmails.Count == 0)
			{
				groupEmails = emailUntility.PostMasters;
			}

			if (groupEmails.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("970C64ED-4DEB-4F1A-869A-46577048B9D1", "Cannot send email without recipients. Following notification groups do not contain any users with email address: Database Health Check Notification Group, Company Notification Group (for Company {0}), Company Notification Group (for System level). Please ensure that there is at least one user with valid email address in above notification groups. You can find these notification groups in Registry -> Notification.", EnvProxy.Instance.CurrentCompany.Name));
				return;
			}

			EmailDef email = new EmailDef();
			email.Body = body;
			email.Subject = subject;
			email.AddRecipientForUserCommunication(groupEmails);
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		internal void SendLoginNotCreatedNotification(string fullUserLogin)
		{
			var subject = Res.GetString("11665d55-51af-42b3-a297-17a8adda671b", "SQL Login is not created on the SQL replica server.");

			var body = new StringBuilder();

			foreach (var result in notificationsList)
			{
				var msg = "\r\n\r\n" + Res.GetString("50c476db-81d0-4a75-b077-b527818c418f", "Login for user [{0}] on SQL Replica [{1}] is not created.\r\n\r\n{2}",
					fullUserLogin,
					result.Item1,
					result.Item2);

				body.AppendLine().Append(msg);
			}

			SendEmail(subject, body.ToString());
		}

		internal void SendLoginNotDeletedNotification(string fullUserLogin)
		{
			var subject = Res.GetString("b1a70f27-e8d6-4f80-adcd-da1a041587ed", "SQL Login is not deleted on the SQL instance.");

			var body = new StringBuilder();

			foreach (var result in notificationsList)
			{
				var msg = Res.GetString("c49e48d4-718d-4270-ac07-be763fb3014f", "Login for user [{0}] on SQL instance [{1}] is not deleted.\r\n\r\n{2}",
					fullUserLogin,
					result.Item1,
					result.Item2);

				body.AppendLine().Append(msg);
			}

			SendEmail(subject, body.ToString());
		}

		internal void SendLoginNotPropagatedNotification()
		{
			var subject = Res.GetString("b9fc1e5c-2ec6-461b-8797-9a8270f1bb2f", "SQL Logins are not propagated on the SQL replica server.");

			var body = new StringBuilder();

			foreach (var result in notificationsList)
			{
				var msg = "\r\n\r\n" + Res.GetString("a1532ceb-eb47-4071-b6ba-04d073659e37", "Login for user [{0}] on SQL Replica [{1}] is not propagated.\r\n\r\n{2}",
					result.Item1,
					result.Item2,
					result.Item3);

				body.AppendLine().Append(msg);
			}

			SendEmail(subject, body.ToString());
		}
	}

	#endregion // ReplicationResult
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	public partial class DbUserManager
	{
		internal HashSet<string> GetEnterpriseDbs_Exposed(DbConnection connection)
		{
			return GetEnterpriseDbs(connection);
		}

		internal string GetFullUserLoginName_Exposed(string staffLoginName, Func<string> getDownLevelLogonName, DatabaseAuthenticationMode databaseAuthenticationMode)
		{
			return GetFullUserLoginName(Db.DatabaseName, staffLoginName, getDownLevelLogonName, databaseAuthenticationMode);
		}

		internal void CreateDbUsersAndPermissions_Exposed(AdminConnection connection, StaffLoginInfo info)
		{
			info.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Sql);
			CreateDbUserWithPermissions(connection, info);
		}

		internal void CreateDbUsersAndPermissions_Exposed(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			info.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, DatabaseAuthenticationMode.Sql);
			CreateDbUserWithPermissions(connection, dbName, info);
		}

		internal void CreateBiDbUsersAndPermissions_Exposed(AdminConnection connection, string dbName, StaffLoginInfo info)
		{
			CreateDbUserWithPermissionsOnBiDatabase(connection, dbName, info);
		}

		internal void EnsureReaderRoleExist_Exposed(AdminConnection connection, string dbName)
		{
			EnsureReaderRoleExist(connection, dbName);
		}

		internal void CreateLoginWithPermissions_Exposed(AdminConnection connection, StaffLoginInfo info, DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			info.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, databaseAuthenticationMode);
			CreateLoginWithPermissions(connection, info, Db.DatabaseName);
		}

		internal void DropDbUsers_Exposed(AdminConnection connection, StaffLoginInfo info, DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			info.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, databaseAuthenticationMode);
			DropDbUser(connection, info);
		}

		internal void DropDbUsersWithLoginInfo_Exposed(AdminConnection connection, StaffLoginInfo info)
		{
			DropDbUser(connection, info);
		}

		internal void DropLogin_Exposed(AdminConnection connection, StaffLoginInfo info, DatabaseAuthenticationMode databaseAuthenticationMode = DatabaseAuthenticationMode.Sql)
		{
			info.LoginName = GetFullUserLoginName(Db.DatabaseName, info.LoginName, info.GetDownLevelLogonName, databaseAuthenticationMode);
			DropLogin(connection, info);
		}

		internal void CreateStaffLogin_Exposed(StaffLoginInfo info)
		{
			CreateStaffLogin(info);
		}

		internal void AlterStaffLogin_Exposed(StaffLoginInfo info)
		{
			AlterStaffLogin(info);
		}

		internal void DropStaffLogin_Exposed(StaffLoginInfo info)
		{
			DropStaffLogin(info);
		}

		internal string SQL_EnableLogin_Exposed(StaffLoginInfo info)
		{
			return SQL_EnableLogin(info);
		}
	}
}

#endif
#endregion
