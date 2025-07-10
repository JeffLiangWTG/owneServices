using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxy.Interface.Converters;
using CargoWise.Data.SqlProxy.Interface.Models;
using ICSharpCode.SharpZipLib.GZip;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.SqlProxyServer.Core;

[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "connection over HTTP")]
public class SqlProxy : ISqlProxy
{
	#region Transactions

	public BeginTransactionResult BeginTransaction(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		Interlocked.Increment(ref transactionCount);
		return new BeginTransactionResult(Manager.CreateSlot(request.Connection!, request.IsolationLevel));
	}

	public VoidResult CommitTransaction(Guid transactionId, CancellationToken cancellationToken)
	{
		Interlocked.Decrement(ref transactionCount);
		var slot = Manager.ReleaseSlot(transactionId);
		slot?.Commit();

		return new VoidResult();
	}

	public VoidResult RollbackTransaction(Guid transactionId, CancellationToken cancellationToken)
	{
		Interlocked.Decrement(ref transactionCount);
		var slot = Manager.ReleaseSlot(transactionId);
		slot?.Rollback();

		return new VoidResult();
	}

	public async Task<BeginTransactionResult> BeginTransactionAsync(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		return await Task.Run(() => BeginTransaction(request, cancellationToken), cancellationToken);
	}

	public async Task<VoidResult> CommitTransactionAsync(Guid transactionId, CancellationToken cancellationToken)
	{
		return await Task.Run(() => CommitTransaction(transactionId, cancellationToken), cancellationToken);
	}

	public async Task<VoidResult> RollbackTransactionAsync(Guid transactionId, CancellationToken cancellationToken)
	{
		return await Task.Run(() => RollbackTransaction(transactionId, cancellationToken), cancellationToken);
	}

	#endregion

	#region ExecuteScalar

	public ExecuteScalarResult ExecuteScalar(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		using var connection = GetConnection(request);
		using var sqlCommand = GetSqlCommand(connection, request);

		// Retrieve metadata to get the type of the result
		using var reader = sqlCommand.ExecuteReader(CommandBehavior.SchemaOnly);
		if (!reader.Read() && reader.FieldCount > 0)
		{
			var type = reader.GetDataTypeName(0);
			reader.Close();

			object? value = sqlCommand.ExecuteScalar();
			if (type == "sql_variant")
			{
				// sql_variant is a special case
				var rawDataType = value?.GetType().Name;
				type = !string.IsNullOrEmpty(rawDataType) ? rawDataType : type;
			}

			var serialized = SqlValueConverter.ToJson(value, type!);
			var result =
				new ExecuteScalarResult(type!, serialized) { Parameters = GetResultParameters(sqlCommand) };
			return result;
		}

		throw new InvalidOperationException($"Failed to read the schema {sqlCommand.CommandText}.");
	}

	public async Task<ExecuteScalarResult> ExecuteScalarAsync(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		return await Task.Run(() => ExecuteScalar(request, cancellationToken), cancellationToken);
	}

	#endregion

	#region ExecuteNonQuery

	public ExecuteNonQueryResult ExecuteNonQuery(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		if (request.SqlStatement!.IndexOf("sp_set_session_context", StringComparison.OrdinalIgnoreCase) > -1)
		{
			return new ExecuteNonQueryResult(0);
		}

		using var connection = GetConnection(request);
		using var sqlCommand = GetSqlCommand(connection, request);
		var result = new ExecuteNonQueryResult(sqlCommand.ExecuteNonQuery()) { Parameters = GetResultParameters(sqlCommand) };

		return result;
	}

	public async Task<ExecuteNonQueryResult> ExecuteNonQueryAsync(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		return await Task.Run(() => ExecuteNonQuery(request, cancellationToken), cancellationToken);
	}

	#endregion

	#region ExecuteReader

	static ResultSet GetResultSet(SqlDataReader reader, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(reader);

		var result = new ResultSet(new DataField[reader.FieldCount]);
		for (var i = 0; i < reader.FieldCount; i++)
		{
			result.FieldNames[i] = new DataField(reader.GetName(i), reader.GetDataTypeName(i));
		}

		while (reader.Read())
		{
			var row = new object[reader.FieldCount];
			result.Rows.Add(row);

			for (var i = 0; i < reader.FieldCount; i++)
			{
				row[i] = reader[i];
			}

			cancellationToken.ThrowIfCancellationRequested();
		}

		return result;
	}

	internal ExecuteReaderResult ExecuteReaderResponse(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		var executeReaderResult = new ExecuteReaderResult(new SqlProxyDataReader([]));

		using var connection = GetConnection(request);
		using var sqlCommand = GetSqlCommand(connection, request);
		using var reader = sqlCommand.ExecuteReader();

		do
		{
			executeReaderResult.ProxyDataReader.ResultSets.Add(GetResultSet(reader, cancellationToken));
			cancellationToken.ThrowIfCancellationRequested();
		}
		while (reader.NextResult());

		executeReaderResult.Parameters = GetResultParameters(sqlCommand);

		return executeReaderResult;
	}

	public DbDataReader ExecuteReader(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		var connection = GetConnection(request);
		var sqlCommand = GetSqlCommand(connection, request);
		var reader = sqlCommand.ExecuteReader(request.CommandBehavior);
		return new DisposableSqlDataReaderWrapper(reader!, () => connection.Dispose());
	}

	public async Task<DbDataReader> ExecuteReaderAsync(SqlProxyRequest request, CancellationToken cancellationToken)
	{
		return await Task.Run(() => ExecuteReader(request, cancellationToken), cancellationToken);
	}

	#endregion

	#region BulkCopy

	public ExecuteBulkCopyResult BulkCopy(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken)
	{
		using var connection = GetConnection(request);
		using var bulkCopy = new SqlBulkCopy(connection.Connection, request.Options, connection.Transaction);

		bulkCopy.BatchSize = request.BatchSize;
		bulkCopy.BulkCopyTimeout = request.BulkCopyTimeout;
		bulkCopy.DestinationTableName = request.DestinationTableName.QuoteName();
		bulkCopy.NotifyAfter = request.NotifyAfter;

		foreach (var item in request.ColumnMappings!)
		{
			_ = bulkCopy.ColumnMappings.Add(item.Key, item.Value);
		}

		using var schemaReader = new StringReader(request.TableSchema!);
		using var dataTable = new DataTable();
		dataTable.Locale = CultureInfo.InvariantCulture;

		using var serializationStream = new MemoryStream(request.Payload!);
		using var unzipStream = new GZipInputStream(serializationStream);
		using var unzipStreamReader = new StreamReader(unzipStream);

		dataTable.ReadXmlSchema(schemaReader);
		var dataTypes = dataTable.Columns.Cast<DataColumn>().Select(x => x.DataType).ToArray();

		var deserializedString = unzipStreamReader.ReadToEnd();
		var payload = JsonHelper.DeserializeObjectArray(deserializedString, dataTypes);

		payload.ForEach(row => JsonHelper.AddRow(dataTable, row));
		bulkCopy.WriteToServer(dataTable);

		return new ExecuteBulkCopyResult(dataTable.Rows.Count);
	}

	public async Task<ExecuteBulkCopyResult> BulkCopyAsync(SqlProxyBulkCopyRequest request, CancellationToken cancellationToken)
	{
		return await Task.Run(() => BulkCopy(request, cancellationToken), cancellationToken);
	}

	#endregion

	#region Implementations

	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Auto generated baseline suppressions - WI00637629")]
	static volatile int transactionCount;

	[ThreadSafe]
	static readonly Lazy<SqlTransactionManager> LazyManager = new(() => new SqlTransactionManager(), LazyThreadSafetyMode.ExecutionAndPublication);

	[ThreadSafe]
	static SqlTransactionManager Manager => LazyManager.Value;

	public static DisposableConnection GetConnection(SqlProxyRequest request)
	{
		return
			request.TransactionId != null && request.TransactionId != Guid.Empty
				? Manager.GetConnection(request.TransactionId)
				: new DisposableConnection(SqlConnectionProvider.GetNewOpenConnection(request.Connection!), null);
	}

	static SqlCommand GetSqlCommand(DisposableConnection connection, SqlProxyRequest request)
	{
		ArgumentNullException.ThrowIfNull(connection);
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(request.SqlStatement);

		var sqlCommand = new SqlCommand(request.SqlStatement, connection.Connection, connection.Transaction); // this is the connection passed through HTTP

		try
		{
			sqlCommand.CommandType = request.CommandType;
			sqlCommand.CommandTimeout = request.CommandTimeout;
			if (request.Parameters != null)
			{
				foreach (var parameter in request.Parameters)
				{
					sqlCommand.Parameters.Add(parameter.ToSqlParameter());
				}
			}

			return sqlCommand;
		}
		catch
		{
			sqlCommand.Dispose();
			throw;
		}
	}

	static SqlParameterDTO[] GetResultParameters(SqlCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		return
			(from param in command.Parameters.OfType<SqlParameter>()
			 where param.Direction != ParameterDirection.Input
			 select SqlParameterDTO.FromSqlParameter(param)).ToArray();
	}

	#endregion
}
