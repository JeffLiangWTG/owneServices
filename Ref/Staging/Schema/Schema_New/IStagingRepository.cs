using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public interface IStagingRepository : IDisposable
	{
		EntityEntry<T> Add<T>(T data) where T : class;
		IQueryable<T> Get<T>() where T : class;
		void Remove<T>(T data) where T : class;
		IDbContextTransaction BeginTransaction();
		void Commit(object transaction);
		int SaveChanges();
		Task<int> SaveChangesAsync();
		int ExecuteSqlCommand(string sql, params object[] parameters);
		Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
		Task BulkInsertAsync<T>(IEnumerable<T> data, int? timeout = null, IDbTransaction transaction = null);
		Task BulkInsertWithRetryAsync<T>(IEnumerable<T> data, int? timeout = null);
		void Update<T>(T data) where T : class;
		int AffectedRecords { get; }
		T AddOrUpdate<T>(T data) where T : class;
		int? CommandTimeout { get; set; }
		bool DatabaseExists { get; }
		(DateTime sysStartTime, DateTime? sysEndTime)? GetTemporalStartAndEndTime<T>(T data) where T : class;
	}
}
