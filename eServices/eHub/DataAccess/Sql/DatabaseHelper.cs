using System;
using System.Configuration;
using System.Data.SqlClient;
using Common.Logging;

namespace CargoWise.eHub.DataAccess.Sql
{
	public interface IDatabaseHelper
	{
		SqlConnection GeteHubDbConnection();
		SqlConnection GetEdiProdCacheDbConnection();
		SqlConnection GetEdiProdDbConnection();
		SqlCommand GetCommand(string commandText, SqlConnection connection);
		SqlCommand GetCommand(string commandText, SqlConnection connection, SqlTransaction transaction);
	}

	public class DatabaseHelper : IDatabaseHelper
	{
		public DatabaseHelper() : this(new eServices.eHubDataAccess.Sql.DatabaseHelper()) { }

		public DatabaseHelper(eServices.eHubDataAccess.Sql.DatabaseHelper databaseHelper)
		{
			this.databaseHelper = databaseHelper;
		}

		private readonly eServices.eHubDataAccess.Sql.DatabaseHelper databaseHelper;

		public static T ConvertDBValue<T>(object obj)
			=> eServices.eHubDataAccess.Sql.DatabaseHelper.ConvertDBValue<T>(obj);

		static DatabaseHelper()
		{
			if (int.TryParse(ConfigurationManager.AppSettings["DatabaseAccessRetryTimeLimit"], out int result) && result > 0)
				RetryTimeLimit = result * 1000;
		}
		internal static int RetryTimeLimit = 600000;


		public virtual SqlConnection GeteHubDbConnection() => databaseHelper.GeteHubDbConnection();

		public virtual SqlConnection GetEdiProdCacheDbConnection() => databaseHelper.GetEdiProdCacheDbConnection();

		public virtual SqlConnection GetEdiProdDbConnection() => databaseHelper.GetEdiProdDbConnection();

		public virtual SqlCommand GetCommand(string commandText, SqlConnection connection)
			=> databaseHelper.GetCommand(commandText, connection);

		public virtual SqlCommand GetCommand(string commandText, SqlConnection connection, SqlTransaction transaction)
			=> databaseHelper.GetCommand(commandText, connection, transaction);

		public static void ExecuteWithRetries(SqlConnection connection, Action<SqlTransaction> processing)
			=> eServices.eHubDataAccess.Sql.DatabaseHelper.ExecuteWithRetries(connection, processing);

		public static T AccessDatabaseWithRetries<T>(Func<T> processing, ILog logger = null)
			=> eServices.eHubDataAccess.Sql.DatabaseHelper.AccessDatabaseWithRetries(processing, logger);

		public static void AccessDatabaseWithRetries(Action processing, ILog logger = null)
			=> eServices.eHubDataAccess.Sql.DatabaseHelper.AccessDatabaseWithRetries(processing, logger);
	}
}
