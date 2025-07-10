using System.Data;
using System.Data.Common;
using CargoWise.Data.HttpClient;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.SqlProxyServer.Test;

static class SqlProxyTestExtensions
{
	public static ExecuteScalarResult ExecuteScalarRequest(this Core.SqlProxy glowLoader, string sql, IDbConnectionInfo databaseConnection, Action<SqlProxyRequest>? requestAction = null)
	{
		var request = new SqlProxyRequest(databaseConnection)
		{
			SqlStatement = sql,
			CommandType = CommandType.Text,
			Parameters = [],
		};
		requestAction?.Invoke(request);

		return glowLoader.ExecuteScalar(request, CancellationToken.None);
	}

	public static DbDataReader ExecuteReaderRequest(this Core.SqlProxy glowLoader, string sql, IDbConnectionInfo databaseConnection, Action<SqlProxyRequest>? requestAction = null)
	{
		var request = new SqlProxyRequest(databaseConnection)
		{
			SqlStatement = sql,
			CommandType = CommandType.Text,
			Parameters = [],
		};

		requestAction?.Invoke(request);

		return glowLoader.ExecuteReader(request, CancellationToken.None);
	}

	public static object ExecuteScalar(this HttpConnection httpConnection, string sql, Action<HttpCommand>? commandAction = null)
	{
		using var dbCommand = new HttpCommand(httpConnection);
		dbCommand.CommandText = sql;
		dbCommand.CommandType = CommandType.Text;
		commandAction?.Invoke(dbCommand);

		return dbCommand.ExecuteScalar();
	}

	public static DbDataReader ExecuteReader(this HttpConnection httpConnection, string sql, Action<HttpCommand>? commandAction = null, Func<CommandBehavior>? getCommandBehavior = null)
	{
		using var dbCommand = new HttpCommand(httpConnection);
		dbCommand.CommandText = sql;
		dbCommand.CommandType = CommandType.Text;
		commandAction?.Invoke(dbCommand);

		var commandBehavior = getCommandBehavior?.Invoke() ?? CommandBehavior.Default;

		return dbCommand.ExecuteReader(commandBehavior);
	}

	public static void ExecuteNoQuery(this HttpConnection httpConnection, string sql, Action<HttpCommand>? commandAction = null, Func<CommandBehavior>? getCommandBehavior = null)
	{
		using var dbCommand = new HttpCommand(httpConnection);
		dbCommand.CommandText = sql;
		dbCommand.CommandType = CommandType.Text;
		commandAction?.Invoke(dbCommand);

		dbCommand.ExecuteNonQuery();
	}
}
