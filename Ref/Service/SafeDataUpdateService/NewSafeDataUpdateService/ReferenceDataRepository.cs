using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ReferenceDataRepository : IReferenceDataRepository
	{
		public ReferenceDataRepository(SystemVersionInterceptor systemVersionInterceptor)
			: this(false, "name=ReferenceDataEntities", systemVersionInterceptor)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		public ReferenceDataRepository(bool batchProcessing, string nameOrConnectionString, SystemVersionInterceptor systemVersionInterceptor = null)
			: this(new SafeDbContext(GetRepositoryContextOptions(nameOrConnectionString, systemVersionInterceptor)))
		{
		}

		public ReferenceDataRepository(SafeDbContext entities)
		{
			Argument.NotNull(entities, nameof(entities));
			this.entities = entities;
			this.entities.Database.SetCommandTimeout(300);
		}

		readonly SafeDbContext entities;

		static DbContextOptions<SafeDbContext> GetRepositoryContextOptions(string nameOrConnectionString, SystemVersionInterceptor systemVersionInterceptor)
		{
			return new DbContextOptionsBuilder<SafeDbContext>()
				.UseSqlServer(nameOrConnectionString, x => x.UseNetTopologySuite())
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
				.AddInterceptors(systemVersionInterceptor ?? new SystemVersionInterceptor(new SystemVersionContext()))
				.Options;
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return entities.Set<T>()?.AsNoTracking();
		}

		public IQueryable<T> GetWithExpand<T>(IDictionary<Type, List<Type>> dict) where T : class
		{
			var result = entities.Set<T>()?.AsSplitQuery();
			var relatedPaths = new List<string>();
			GenerateRelationship(typeof(T), "", relatedPaths, dict);
			foreach (var relatedPath in relatedPaths)
			{
				result = result.Include(relatedPath);
			}
			return result;
		}

		static void GenerateRelationship(Type type, string prefix, List<string> relationships, IDictionary<Type, List<Type>> dict, string[] dataSetInfo = null)
		{
			var properties = type.GetProperties();
			dataSetInfo = dataSetInfo == null ? DataSetStructureProvider.StructuredDataSets.First(x => x.First() == type.Name) : dataSetInfo;
			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					var elementType = propertyType.GetGenericArguments()[0];
					if (!dataSetInfo.Contains(elementType.Name))
					{
						continue;
					}
					var newPrefix = !string.IsNullOrEmpty(prefix) ? $"{prefix}.{property.Name}" : $"{property.Name}";
					if (elementType.GetProperties().Any(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
					{
						GenerateRelationship(elementType, newPrefix, relationships, dict, dataSetInfo);
					}
					else
					{
						relationships.Add($"{newPrefix}");
					}
					if (dict.ContainsKey(type))
					{
						if (!dict[type].Contains(propertyType))
						{
							dict[type].Add(propertyType);
						}
					}
					else
					{
						dict.Add(type, new List<Type> { propertyType });
					}
				}
			}
		}

		public void Add<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			Entry(data).State = EntityState.Added;
		}

		public void Update<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			Entry(data).State = EntityState.Modified;
		}

		public void Delete<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			Entry(data).State = EntityState.Deleted;
		}

#if DEBUG
		public
#endif
		EntityEntry<T> Entry<T>(T data) where T : class
		{
			return entities.Entry(data);
		}

		public async Task<int> SaveChangeWithBulkInsert(IBulkInsertCore bulkInsertCore)
		{
			var sleepMilliSeconds = RandomNumberGenerator.GetInt32(100, 5000);
			var modifiedEntries = entities.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted).ToArray();
			var itemStates = modifiedEntries.ToDictionary(x => x, x => x.State);
			return await RetryExtensions.Retry<int, AggregateException, SqlException>(
				() => bulkInsertCore.SaveChangeWithBulkInsertCore(modifiedEntries, itemStates, entities),
				3,
				() => Thread.Sleep(sleepMilliSeconds),
				x => (x.InnerException as SqlException)?.Number == KnownSqlExceptionsNumbers.TransactionDeadlock || (x.InnerException as SqlException)?.Number == KnownSqlExceptionsNumbers.Timeout,
				x => x.Number == KnownSqlExceptionsNumbers.TransactionDeadlock || x.Number == KnownSqlExceptionsNumbers.Timeout);
		}

		public async Task<int> SaveChangesAsync(string userId, bool bulkInsert = false)
		{
			var result = 0;
			if (bulkInsert)
			{
				var bulkInserCore = new BulkInsertCore(userId);
				result = await SaveChangeWithBulkInsert(bulkInserCore);
			}
			else
			{
				if (!string.IsNullOrEmpty(userId))
				{
					entities.SetUserId(userId);
				}
				result = await entities.SaveChangesAsync();
			}
			return result;
		}

		public void Dispose()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			entities.Dispose();
		}

		public async Task RecursiveDeleteAsync<TE>(IQueryable<TE> data) where TE : class
		{
			var recursiveDeletion = new RecursiveDeletion(entities);
			await recursiveDeletion.RecursiveDeleteAsync(data);
		}
	}
}
