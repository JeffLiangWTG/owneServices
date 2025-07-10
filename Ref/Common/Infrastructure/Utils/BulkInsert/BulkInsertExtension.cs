using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class BulkInsertExtension
	{
		public static async Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> entities, IDbTransaction transaction = null, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null, int? timeout = null)
		{
			var options = new BulkInsertOptions { SqlBulkCopyOption = sqlBulkCopyOptions, Transaction = transaction };
			if (batchSize.HasValue)
			{
				options.BatchSize = batchSize.Value;
			}
			if (timeout.HasValue)
			{
				options.TimeOut = timeout.Value;
			}
			await BulkInsertCoreAsync(context, entities, options);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		static async Task BulkInsertCoreAsync<T>(DbContext context, IEnumerable<T> entities, BulkInsertOptions options)
		{
			var dbConnection = context.Database.GetDbConnection();
			if (dbConnection.State != ConnectionState.Open)
			{
				await dbConnection.OpenAsync();
			}

			if (options.Transaction != null)
			{
				await RunSqlBulkCopyAsync(entities, (SqlConnection)dbConnection, options, (SqlTransaction)options.Transaction);
			}
			else
			{
				using (var transaction = await dbConnection.BeginTransactionAsync())
				{
					try
					{
						await RunSqlBulkCopyAsync(entities, (SqlConnection)dbConnection, options, (SqlTransaction)transaction);
						await transaction.CommitAsync();
					}
					catch
					{
						if (transaction.Connection != null)
						{
							await transaction.RollbackAsync();
						}
						throw;
					}
				}
			}
		}

		static async Task RunSqlBulkCopyAsync<T>(IEnumerable<T> entities, SqlConnection sqlConnection, BulkInsertOptions options, SqlTransaction sqlTransaction)
		{
			using (var bulkCopy = new SqlBulkCopy(sqlConnection, options.SqlBulkCopyOption, sqlTransaction))
			{
				bulkCopy.DestinationTableName = typeof(T).Name;
				bulkCopy.BatchSize = options.BatchSize;
				bulkCopy.BulkCopyTimeout = options.TimeOut;
				using (var table = entities.ToDataTable())
				{
					foreach(var column in table.Columns)
					{
						bulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
					}
					await bulkCopy.WriteToServerAsync(table);
				}
			}
		}

		static class BulkInsertDefaults
		{
			public const int BatchSize = 5000;
			public const int TimeOut = 30;
			public const int NotifyAfter = 1000;
			public const SqlBulkCopyOptions SqlBulkCopyOption = SqlBulkCopyOptions.Default;
		}

		class BulkInsertOptions
		{
			public int BatchSize { get; set; } = BulkInsertDefaults.BatchSize;
			public int TimeOut { get; set; } = BulkInsertDefaults.TimeOut;
			public int NotifyAfter { get; set; } = BulkInsertDefaults.NotifyAfter;
			public SqlBulkCopyOptions SqlBulkCopyOption { get; set; } = BulkInsertDefaults.SqlBulkCopyOption;
			public IDbTransaction Transaction { get; set; }
		}
	}
}
