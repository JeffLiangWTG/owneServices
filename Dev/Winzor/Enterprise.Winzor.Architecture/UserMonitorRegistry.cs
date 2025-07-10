using System;
using System.Data;
using System.Runtime.ExceptionServices;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;

namespace Enterprise.Winzor.Architecture;

public class UserMonitorRegistry
{
	const string ProcedureNameGetValueNOD = "DataRegGetValueNOD";
	readonly ILogger<UserMonitorRegistry> logger;

	public virtual bool? MonitoringEnabled { get; private set; }
	public virtual Uri MonitoringURI { get; private set; }

	const int ConnectTimeout = 15;
	const int MaxPoolSize = 100;
	const int MinPoolSize = 0;
	const int LoadBalanceTimeout = 0;
	const string ApplicationName = "Blazor.AppServer";

	public string ServerName { get; private set; }
	public string DatabaseName { get; private set; }

	readonly IProtectedDataServiceFactory protectedDataServiceFactory;
	readonly ISqlConnectionProvider sqlConnectionProvider;

	public UserMonitorRegistry(ILogger<UserMonitorRegistry> logger,  IProtectedDataServiceFactory protectedDataServiceFactory, ISqlConnectionProvider sqlConnectionProvider)
	{
		this.logger = logger;
		this.protectedDataServiceFactory = protectedDataServiceFactory;
		this.sqlConnectionProvider = sqlConnectionProvider;
	}

	/// <summary>
	/// Try to Get registry Item before Initialization.SetupDbConnection which execute in the WinzorDispatcher.InvokeAsync.
	/// If directly access DataRegistry.Instance.WebVersionUserMonitoringEnabled, exception throw `System.InvalidOperationException: Protected Data Services are not configured`
	/// try to new Extra Connection and use Raw Query to Get Registry.
	/// </summary>
	public void RefreshMonitorConfiguration(string serverName, string databaseName)
	{
		ServerName = serverName;
		DatabaseName = databaseName;

		try
		{
			using (var dbConnection = CreateConnectionWithRetry())
			{
				MonitoringEnabled = GetBoolValue(dbConnection, nameof(RawDataRegistry.WebVersionUserMonitoringEnabled));
				var monitoringUrl = GetStringValue(dbConnection, nameof(RawDataRegistry.WebVersionUserMonitoringUrl));
				if (!string.IsNullOrEmpty(monitoringUrl))
				{
					if (Uri.TryCreate(monitoringUrl, UriKind.Absolute, out var uriResult))
					{
						MonitoringURI = uriResult;
					}
					else
					{
						logger.LogWarning(monitoringUrl, $"Monitor Url could not created due to Invalid Url");
					}
				}
			}
		}
		catch (SqlException ex)
		{
			logger.LogError(ex, $"Monitor Url could not created due to SQLException");
		}
	}

	bool? GetBoolValue(IDbConnection extraConnection, string registryName)
	{
		var bytes = GetBinaryValue(extraConnection, registryName);
		if (bytes == null)
		{
			return null;
		}

		return new BooleanRegistryDataType().Deserialise(bytes);
	}

	string GetStringValue(IDbConnection extraConnection, string registryName)
	{
		var bytes = GetBinaryValue(extraConnection, registryName);
		if (bytes == null)
		{
			return null;
		}
		return new StringRegistryDataType().Deserialise(bytes);
	}

	byte[] GetBinaryValue(IDbConnection extraConnection, string registryName)
	{
		using (var command = extraConnection.CreateCommand())
		{
			command.CommandText = ProcedureNameGetValueNOD;
			command.CommandType = CommandType.StoredProcedure;
			var dbParameter = command.CreateParameter();
			dbParameter.ParameterName = "@Name";
			dbParameter.Value = registryName;
			command.Parameters.Add(dbParameter);

			var data = command.ExecuteScalar();
			return data as byte[];
		}
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

		Exception lastEx = null;
		for (int i = 0; i < maxRetries; i++)
		{
			try
			{
				return CreateConnection();
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, $"Error attempting open database connection, on try:{i + 1}");
				lastEx = ex;
			}
			System.Threading.Thread.Sleep(delayMilliseconds);
		}

		if (lastEx != null)
		{
			ExceptionDispatchInfo.Capture(lastEx).Throw();
		}

		return null;
	}
}
