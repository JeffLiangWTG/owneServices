using System.Text.Json.Nodes;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace eServices.Dms.Core.StorageRepository;

public class DmsStorageRepository_V0_1(DmsStorageContext context) : IDmsStorageRepository
{
	public async Task<int> RegisterOwnerAsync(string userName)
	{
		if (!await context.Database.SqlQuery<string>($"SELECT [name] AS [Value] FROM [sys].[database_principals]").AnyAsync(name => name == userName))
		{
			await context.Database.ExecuteSqlRawAsync("""
				DECLARE @sql nvarchar(2000) = CONCAT('CREATE USER ', QUOTENAME(@user), ' WITHOUT LOGIN;');
				EXEC (@sql);
				""",
				new SqlParameter("@user", userName));
		}

		if (!await context.Database.SqlQuery<string>($"SELECT [name] AS [Value] FROM [sys].[schemas]").AnyAsync(name => name == userName))
		{
			await context.Database.ExecuteSqlRawAsync("""
				DECLARE @sql nvarchar(2000) = CONCAT('CREATE SCHEMA ', QUOTENAME(@user), ' AUTHORIZATION ', QUOTENAME(@user), ';');
				EXEC (@sql);
				""",
				new SqlParameter("@user", userName));
			await context.Database.ExecuteSqlRawAsync("""
				DECLARE @sql nvarchar(2000) = CONCAT('ALTER USER ', QUOTENAME(@user), ' WITH DEFAULT_SCHEMA=', QUOTENAME(@user), ';');
				EXEC (@sql);
				""",
				new SqlParameter("@user", userName));
		}

		return 1;
	}

	public Task<bool> TableExistsAsync(string owner, string tableName, DmsTableTypes? type = null)
		=> context.DmsStorageCatalog.AnyAsync(c
			=> c.SC_Schema == owner
			&& c.SC_Name == tableName
			&& (type == null || c.SC_Type == type.ToString())
			&& c.SC_Version == "0.1"
			&& c.SC_DeleteTime == null);

	public async Task<DmsStorageCatalog> CreateTableAsync(DmsStorageCatalog table)
	{
		await context.Database.ExecuteSqlRawAsync("""
			DECLARE @sql nvarchar(2000) = CONCAT('
			CREATE TABLE ', QUOTENAME(@owner), '.', QUOTENAME(@tableName), ' (
			ST_ID int NOT NULL IDENTITY (1, 1) CONSTRAINT ', QUOTENAME(@pkName), ' PRIMARY KEY CLUSTERED,
			ST_Key nvarchar(400) NOT NULL CONSTRAINT ', QUOTENAME(@ixName), ' UNIQUE NONCLUSTERED,
			ST_Document nvarchar(MAX) NULL,
			ST_Object varbinary(MAX) NULL,
			ST_LastUpdateTime datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
			ST_LastUpdateUser nvarchar(128) NOT NULL DEFAULT SUSER_NAME()
			) ON [PRIMARY]');
			EXEC (@sql);
			""",
			new SqlParameter("@owner", table.SC_Schema),
			new SqlParameter("@tableName", table.SC_Name),
			new SqlParameter("@pkName", $"PK_{table.SC_Name}"),
			new SqlParameter("@ixName", $"IX_{table.SC_Name}"));

		context.DmsStorageCatalog.Add(table);
		await context.SaveChangesAsync();
		return table;
	}

	public async Task<DmsStorageCatalog> DeleteTableAsync(DmsStorageCatalog table)
	{
		var delete = await context.DmsStorageCatalog.SingleAsync(c
			=> c.SC_Schema == table.SC_Schema && c.SC_Name == table.SC_Name);
		delete.SC_DeleteTime = DateTime.UtcNow;
		delete.SC_DeleteUser = table.SC_DeleteUser;
		
		await context.Database.ExecuteSqlRawAsync("""
			DECLARE @sql nvarchar(2000) = CONCAT(
			'DROP TABLE IF EXISTS ', QUOTENAME(@owner), '.', QUOTENAME(@tableName));
			EXEC (@sql);
			""",
			new SqlParameter("@owner", table.SC_Schema),
			new SqlParameter("@tableName", table.SC_Name));

		await context.SaveChangesAsync();
		return table;
	}

	public async Task<JsonNode?> GetDocumentAsJsonAsync(string owner, string tableName, string key)
	{
		if (!await TableExistsAsync(owner, tableName, DmsTableTypes.Document))
			throw new InvalidOperationException($"No matching catalog entry for table: {(owner, tableName, DmsTableTypes.Document)}");

		var document = await Task.Run(() => context.Database.SqlQueryRaw<string?>("""
				DECLARE @sql nvarchar(2000) = CONCAT('SELECT ST_Document FROM ', QUOTENAME(@owner), '.', QUOTENAME(@tableName), ' WHERE ST_Key=@key');
				EXEC sp_executesql @sql, N'@key nvarchar(400)', @key;
				""",
			new SqlParameter("@owner", owner),
			new SqlParameter("@tableName", tableName),
			new SqlParameter("@key", key)).AsEnumerable().FirstOrDefault());

		return document is null
			? null
			: JsonNode.Parse(document);
	}

	public async Task<int> PutDocumentAsJsonAsync(string owner, string tableName, string key, JsonNode document, string user)
	{
		if (!await TableExistsAsync(owner, tableName, DmsTableTypes.Document))
			throw new InvalidOperationException($"No matching catalog entry for table: {(owner, tableName, DmsTableTypes.Document)}");

		return await context.Database.ExecuteSqlRawAsync("""
				DECLARE @sql nvarchar(2000) = CONCAT(
				'MERGE INTO ', QUOTENAME(@owner), '.', QUOTENAME(@tableName), ' USING (VALUES (@key, @doc, @user)) T([Key],[Doc],[User]) ON [Key] = ST_Key
				WHEN MATCHED THEN UPDATE SET ST_Document = [Doc], ST_LastUpdateTime = SYSUTCDATETIME(), ST_LastUpdateUser = [User]
				WHEN NOT MATCHED THEN INSERT (ST_Key, ST_Document, ST_LastUpdateTime, ST_LastUpdateUser) VALUES ([Key], [Doc], SYSUTCDATETIME(), [User]);');
				EXEC sp_executesql @sql, N'@key nvarchar(400), @doc nvarchar(max), @user nvarchar(128)', @key, @doc, @user;
				""",
			new SqlParameter("@owner", owner),
			new SqlParameter("@tableName", tableName),
			new SqlParameter("@key", key),
			new SqlParameter("@doc", System.Data.SqlDbType.NVarChar, -1) { Value = document.ToJsonString() },
			new SqlParameter("@user", user));
	}

	public async Task<Stream?> GetObjectAsStreamAsync(string owner, string tableName, string key)
	{
		if (!await TableExistsAsync(owner, tableName, DmsTableTypes.Object))
			throw new InvalidOperationException($"No matching catalog entry for table: {(owner, tableName, DmsTableTypes.Object)}");

		var connection = context.Database.GetDbConnection();
		if (connection.State != System.Data.ConnectionState.Open)
			await connection.OpenAsync();
		using var command = connection.CreateCommand();
		command.Transaction = transaction?.GetDbTransaction();
		command.CommandText = """
			DECLARE @sql nvarchar(2000) = CONCAT('SELECT ST_Object FROM ', QUOTENAME(@owner), '.', QUOTENAME(@tableName), ' WHERE ST_Key=@key');
			EXEC sp_executesql @sql, N'@key nvarchar(400)', @key;
			""";
		command.Parameters.Add(new SqlParameter("@owner", owner));
		command.Parameters.Add(new SqlParameter("@tableName", tableName));
		command.Parameters.Add(new SqlParameter("@key", key));
		using var reader = await command.ExecuteReaderAsync();

		return await reader.ReadAsync() && !(await reader.IsDBNullAsync(0))
			? reader.GetStream(0)
			: null;
	}

	public async Task<int> PutObjectAsStreamAsync(string owner, string tableName, string key, Stream blob, string user)
	{
		if (!await TableExistsAsync(owner, tableName, DmsTableTypes.Object))
			throw new InvalidOperationException($"No matching catalog entry for table: {(owner, tableName, DmsTableTypes.Object)}");

		return await context.Database.ExecuteSqlRawAsync("""
			DECLARE @sql nvarchar(2000) = CONCAT(
			'MERGE INTO ', QUOTENAME(@owner), '.', QUOTENAME(@tableName), ' USING (VALUES (@key, @blob, @user)) T([Key],[Blob],[User]) ON [Key] = ST_Key
			WHEN MATCHED THEN UPDATE SET ST_Object = [Blob], ST_LastUpdateTime = SYSUTCDATETIME(), ST_LastUpdateUser = [User]
			WHEN NOT MATCHED THEN INSERT (ST_Key, ST_Object, ST_LastUpdateTime, ST_LastUpdateUser) VALUES ([Key], [Blob], SYSUTCDATETIME(), [User]);');
			EXEC sp_executesql @sql, N'@key nvarchar(400), @blob varbinary(max), @user nvarchar(128)', @key, @blob, @user;
			""",
			new SqlParameter("@owner", owner),
			new SqlParameter("@tableName", tableName),
			new SqlParameter("@key", key),
			new SqlParameter("@blob", System.Data.SqlDbType.VarBinary, -1) { Value = blob },
			new SqlParameter("@user", user));
	}

	public async Task<IDbContextTransaction> BeginTransactionAsync()
	{
		return transaction = await context.Database.BeginTransactionAsync();
	}

	public async Task CommitTransactionAsync()
	{
		if (transaction is not null)
			await transaction.CommitAsync();
	}

	private IDbContextTransaction? transaction;

	public async ValueTask DisposeAsync()
	{
		if (transaction is not null)
			await transaction.DisposeAsync();
	}
}
