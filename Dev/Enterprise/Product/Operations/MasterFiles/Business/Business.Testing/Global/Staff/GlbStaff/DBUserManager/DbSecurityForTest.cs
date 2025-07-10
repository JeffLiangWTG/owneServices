using System.Collections.Generic;
using System.Text;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DbSecurityForTest : DbSecurityLockDown
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
		
		public void SetLockModeSqlServerSysadminPwd_Exposed(AdminConnection connection)
		{
			base.SetLockModeSqlServerSysadminPwd(connection);
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
}
