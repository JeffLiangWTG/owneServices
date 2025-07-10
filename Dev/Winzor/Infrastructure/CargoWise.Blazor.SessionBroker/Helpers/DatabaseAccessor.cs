using System;
using System.Data;
using System.Runtime.ExceptionServices;
using CargoWise.Blazor.Common;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.Blazor.SessionBroker.Helpers;

public interface IDatabaseAccessor
{
	public string ServerName { get; }
	public string DatabaseName { get; }

	T ExecuteScalar<T>(
		string commandText,
		CommandType commandType = CommandType.Text,
		params (string ParameterName, object Value)[] parameters);

	T ExecuteDbCommand<T>(Func<IDbCommand, T> execDbCommand);
}

public class DatabaseAccessor : IDatabaseAccessor
{
	readonly ILogger logger;
	const int ConnectTimeout = 15;
	const int MaxPoolSize = 100;
	const int MinPoolSize = 0;
	const int LoadBalanceTimeout = 0;
	const string ApplicationName = "Blazor.SessionBroker";
	readonly IProtectedDataServiceFactory protectedDataServiceFactory;
	readonly ISqlConnectionProvider sqlConnectionProvider;

	public string ServerName { get; private set; }
	public string DatabaseName { get; private set; }

	public DatabaseAccessor(ILogger logger, IOptions<CargoWiseOptions> cargoWiseOptions, IProtectedDataServiceFactory protectedDataServiceFactory, ISqlConnectionProvider sqlConnectionProvider)
	{
		this.logger = logger;
		SetDatabase(cargoWiseOptions.Value);
		this.protectedDataServiceFactory = protectedDataServiceFactory;
		this.sqlConnectionProvider = sqlConnectionProvider;
	}

	public T ExecuteScalar<T>(
			string commandText,
			CommandType commandType = CommandType.Text,
			params (string ParameterName, object Value)[] parameters)
	{
		logger.LogInformation($"SQL run: {commandText}");
		return ExecuteDbCommand(command =>
		{
			command.CommandText = commandText;
			command.CommandType = commandType;
			foreach (var param in parameters)
			{
				var dbParameter = command.CreateParameter();
				dbParameter.ParameterName = param.ParameterName;
				dbParameter.Value = param.Value;
				command.Parameters.Add(dbParameter);
			}

			return (T)command.ExecuteScalar();
		});
	}

	public T ExecuteDbCommand<T>(Func<IDbCommand, T> execDbCommand)
	{
		using var dbConnection = CreateConnectionWithRetry();
		using var command = dbConnection.CreateCommand();
		try
		{
			var upgradeState = DatabaseUpgradeDetector.CheckForUpgradeLockout(dbConnection);
			if (upgradeState == DbUpgradeState.UpgradeInProgress)
			{
				throw new DatabaseAccessException("A system upgrade is in progress. Please try again later.");
			}

			return execDbCommand(command);
		}
		catch (SqlException ex)
		{
			logger.LogWarning(ex, $"Failed to execute Sql command: {command.CommandText}");
			var dbError = new DbErrorMatch(ex);
			throw new DatabaseAccessException(dbError.GetUserFriendlyMessage(null), ex);
		}
	}

	void SetDatabase(CargoWiseOptions cwOptions)
	{
		if (string.IsNullOrEmpty(cwOptions.DbServerName) || string.IsNullOrEmpty(cwOptions.DatabaseName))
		{
			throw new InvalidDatabaseConfigurationException("No database connection information available.");
		}

		ServerName = cwOptions.DbServerName;
		DatabaseName = cwOptions.DatabaseName;
		logger.LogInformation($"SetDatabase ServerName: {ServerName}, DatabaseName: {DatabaseName}");
	}

	internal string GetConnectionString()
	{
		return CreateConnection().ConnectionString;
	}

	IDbConnection CreateConnection()
	{
		IDataProviderFactory dataProviderFactory = new SqlDataProviderFactory(protectedDataServiceFactory.CreateSystemService(ServerName, DatabaseName), sqlConnectionProvider);
		var newConnection = dataProviderFactory.OpenNewDbConnection<RestrictedReaderLoginCredentials>(
			serverName: ServerName,
			databaseName: DatabaseName,
			applicationName: ApplicationName,
			connectTimeout: ConnectTimeout,
			connectionPooling: true,
			loadBalanceTimeout: LoadBalanceTimeout,
			maxPoolSize: MaxPoolSize,
			minPoolSize: MinPoolSize);
		return newConnection;
	}

	IDbConnection CreateConnectionWithRetry()
	{
		var maxRetries = 3;
		var delayMilliseconds = 500;

		ExceptionDispatchInfo lastEx = null;
		for (int i = 0; i < maxRetries; i++)
		{
			try
			{
				return CreateConnection();
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, $"Error attempting open database connection, on try:{i + 1}");
				lastEx = ExceptionDispatchInfo.Capture(ex);
			}
			System.Threading.Thread.Sleep(delayMilliseconds);
		}

		if (lastEx != null)
		{
			lastEx.Throw();
		}

		return null;
	}
}

static class DatabaseCommandExtensions
{
	public static IDbDataParameter AddWithValue(this IDataParameterCollection collection, string key, object value)
	{
		var dbParameter = new SqlParameter(key, value);
		collection.Add(dbParameter);
		return dbParameter;
	}

	public static IDbDataParameter Add(this IDataParameterCollection collection, string paramName, SqlDbType dbType, object value = null)
	{
		var dbParameter = new SqlParameter(parameterName: paramName, dbType: dbType);
		if (value != null)
		{
			dbParameter.Value = value;
		}
		collection.Add(dbParameter);
		return dbParameter;
	}
}

public class DatabaseAccessException : Exception
{
	public DatabaseAccessException(string message) : base(message)
	{
	}

	public DatabaseAccessException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
