using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public class StagingRepository : IStagingRepository
	{
		int _affectedRecords = -1;
		readonly StagingDbContext dbContext;

		public StagingRepository()
		{
			dbContext = new StagingDbContext();
		}

		public StagingRepository(string nameOrConnectionString, bool autoDetectChanges = true, IInterceptor[] interceptors = null)
		{
			dbContext = new StagingDbContext(nameOrConnectionString, autoDetectChanges, interceptors);
		}

		public bool DatabaseExists => dbContext.Database.GetService<IRelationalDatabaseCreator>()?.Exists() ?? false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Pls remove this attribute once implement the method")]
		public int? CommandTimeout
		{
			get { return dbContext.Database.GetCommandTimeout(); }
			set { dbContext.Database.SetCommandTimeout(value); }
		}

		public int AffectedRecords
		{
			get { return _affectedRecords; }
			private set { _affectedRecords = value; }
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return dbContext.Set<T>();
		}

		public EntityEntry<T> Add<T>(T data) where T : class
		{
			return dbContext.Add(data);
		}

		public (DateTime sysStartTime, DateTime? sysEndTime)? GetTemporalStartAndEndTime<T>(T data) where T : class
		{
			var entry = dbContext.Entry(data);
			if (entry != null)
			{
				var properties = entry.Properties;
				var sysStartTime = properties.FirstOrDefault(x => x.Metadata.Name.EndsWith("_SysStartTime", StringComparison.OrdinalIgnoreCase));
				var sysEndTime = properties.FirstOrDefault(x => x.Metadata.Name.EndsWith("_SysEndTime", StringComparison.OrdinalIgnoreCase));
				if (sysStartTime != null && sysEndTime != null)
				{
					return (Convert.ToDateTime(sysStartTime.CurrentValue, null), sysEndTime.CurrentValue != null ? Convert.ToDateTime(sysEndTime.CurrentValue, null) : null);
				}
			}

			return null;
		}

		public T AddOrUpdate<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			var pkColumn = TypeExtension.GetPKPropertyInfo(typeof(T));
			var pkValue = pkColumn.GetValue(data);

			var dbObject = Get<T>().FirstOrDefault(ExpressionHelper.GetPropertyFiltersExpression<T>(pkColumn, [pkValue], Operations.Equals));
			if (dbObject == null)
			{
				Add(data);
			}
			else
			{
				Update(data);
			}

			return data;
		}

		public void Update<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			var dbSet = dbContext.Set<T>();
			var entry = dbContext.Entry<T>(data);
			if (entry.State == EntityState.Detached)
			{
				dbSet.Attach(data);
				entry.State = EntityState.Modified;
			}
		}

		public void Remove<T>(T data) where T : class
		{
			var dbSet = dbContext.Set<T>();
			var entry = dbContext.Entry<T>(data);
			if (entry.State == EntityState.Detached)
			{
				dbSet.Attach(data);
			}
			dbSet.Remove(data);
		}

		public int SaveChanges()
		{
			AffectedRecords = dbContext.SaveChanges();
			return AffectedRecords;
		}

		public async Task<int> SaveChangesAsync()
		{
			AffectedRecords = await dbContext.SaveChangesAsync();
			return AffectedRecords;
		}

		public void Dispose()
		{
			dbContext.Dispose();
		}

		public IDbContextTransaction BeginTransaction()
		{
			return dbContext.Database.BeginTransaction();
		}

		public void Commit(object transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			var dbTransaction = transaction as IDbContextTransaction;
			dbTransaction.Commit();
		}

		public int ExecuteSqlCommand(string sql, params object[] parameters)
		{
			return dbContext.Database.ExecuteSqlRaw(sql, parameters);
		}

		public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
		{
			return await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
		}

		public async Task BulkInsertAsync<T>(IEnumerable<T> data, int? timeout = null, IDbTransaction transaction = null)
		{
			Argument.NotNull(data, nameof(data));
			var bulkInsertCore = new BulkInsertCore();
			await bulkInsertCore.BulkInsertAsync(dbContext, data, timeout, transaction);
		}

		public async Task BulkInsertWithRetryAsync<T>(IEnumerable<T> data, int? timeout = null)
		{
			Argument.NotNull(data, nameof(data));
			var bulkInsertCore = new BulkInsertCore();
			await BulkInsertCore(bulkInsertCore, data, timeout);
		}

#if DEBUG
		internal
#endif
		async Task BulkInsertCore<T>(IBulkInsertCore bulkInsertCore, IEnumerable<T> data, int? timeout = null)
		{
			var sleepMilliSeconds = RandomNumberGenerator.GetInt32(100, 5000);
			await RetryExtensions.Retry<int, AggregateException, SqlException>(
				() => bulkInsertCore.BulkInsertAsync(dbContext, data, timeout),
				3,
				() => Thread.Sleep(sleepMilliSeconds),
				x => (x.InnerException as SqlException)?.Number == KnownSqlExceptionsNumbers.TransactionDeadlock,
				x => x.Number == KnownSqlExceptionsNumbers.TransactionDeadlock);
		}
	}
}
