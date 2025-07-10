using System.Data;
using CargoWise.Data.HttpClient;
using CargoWise.Data.SqlProxy.Interface.Models;
using CargoWise.Data.SqlProxyServer.Core;
using Moq;

namespace CargoWise.Data.SqlProxyServer.Test;

static class TestHelper
{
	public static IDisposable SaveDbExtendedProperty(string propertyName, object? value)
	{
		var sqlConnection = SqlConnectionProvider.GetNewOpenConnection(LazyDatabaseConnection.Value);
		var transaction = sqlConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
		var disposableConnection = new DisposableConnection(sqlConnection, transaction);
		disposableConnection.Disposing += (sender, e) => transaction.Dispose();

		var addOrUpdateScript = $@"
IF EXISTS(SELECT null FROM [{Db.DatabaseName}].sys.extended_properties WHERE class = 0 AND name = @propertyName)
  EXEC [{Db.DatabaseName}].sys.sp_updateextendedproperty @name = @propertyName, @value = @propertyValue;
ELSE
EXEC [{Db.DatabaseName}].sys.sp_addextendedproperty @name = @propertyName, @value = @propertyValue;
";
		var propertyNameParameter = new SqlParameter("@propertyName", SqlDbType.NVarChar, 128) { Value = propertyName };
		var propertyValueParameter = new SqlParameter("@propertyValue", SqlDbType.Variant, 8016) { Value = value ?? DBNull.Value };

		using var cmd = sqlConnection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = addOrUpdateScript;
		cmd.Parameters.Add(propertyNameParameter);
		cmd.Parameters.Add(propertyValueParameter);

		cmd.ExecuteNonQuery();

		return disposableConnection;
	}

	public static IDisposable DropDbExtendedProperty(string propertyName)
	{
		var sqlText = $@"
IF EXISTS(SELECT null FROM sys.extended_properties WHERE class = 0 AND name = @propertyName)
EXEC [{Db.DatabaseName}].sys.sp_dropextendedproperty @name = @propertyName;
";
		var sqlConnection = SqlConnectionProvider.GetNewOpenConnection(LazyDatabaseConnection.Value);
		var transaction = sqlConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
		var disposableConnection = new DisposableConnection(sqlConnection, transaction);
		disposableConnection.Disposing += (sender, e) => transaction.Dispose();

		var propertyNameParameter = new SqlParameter("@propertyName", SqlDbType.NVarChar, 128) { Value = propertyName };

		using var cmd = sqlConnection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = sqlText;
		cmd.Parameters.Add(propertyNameParameter);

		cmd.ExecuteNonQuery();

		return disposableConnection;
	}

	public static Lazy<SqlProxyDatabaseDetails> LazyDatabaseConnection = new(GetDatabaseConnection);

	static SqlProxyDatabaseDetails GetDatabaseConnection()
	{
		if (!Db.DatabaseNameIsInitialized || !Db.ServerNameIsInitialized)
		{
			throw new InvalidOperationException("Database name and server name must be initialized before calling this method.");
		}

		var credentials = HttpDataProviderFactory.GetCredentials<CargoWise.DataProtection.RestrictedWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

		return Mock.Of<SqlProxyDatabaseDetails>(x =>
			x.ServerName == Db.ServerName && x.Database == Db.DatabaseName && x.UserName == credentials.UserName && x.Password == credentials.Password);
	}
}
