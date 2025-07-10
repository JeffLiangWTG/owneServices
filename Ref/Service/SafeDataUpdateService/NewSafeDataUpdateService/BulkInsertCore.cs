using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Polly;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class BulkInsertCore : IBulkInsertCore
	{
		readonly string userId;

		public BulkInsertCore() { }

		public BulkInsertCore(string userId)
		{
			this.userId = userId;
		}

		public async Task<int> SaveChangeWithBulkInsertCore(EntityEntry[] entries, IDictionary<EntityEntry, EntityState> itemStates, SafeDbContext entities)
		{
			Argument.NotNull(entries, nameof(entries));
			Argument.NotNull(itemStates, nameof(itemStates));
			Argument.NotNull(entities, nameof(entities));
			var result = 0;
			var deletedItems = entries.Where(x => itemStates[x] == EntityState.Deleted).GroupBy(x => x.Entity.GetType(), x => x).OrderBy(x => x.Key.Name).ToList();
			var addedItems = entries.Where(x => itemStates[x] == EntityState.Added).GroupBy(x => x.Entity.GetType(), x => x).OrderBy(x => x.Key.Name).ToList();
			var modifiedItems = entries.Where(x => itemStates[x] == EntityState.Modified).GroupBy(x => x.Entity.GetType(), x => x).OrderBy(x => x.Key.Name).ToList();
			Array.ForEach(entries, x => x.State = EntityState.Detached);

			await using var trans = entities.Database.BeginTransaction();
			if (!string.IsNullOrEmpty(userId))
			{
				entities.SetUserId(userId);
			}
			// keep the order of saving the entities
			result += await SaveChangesInOrder(modifiedItems, itemStates, entities, trans);
			result += await SaveChangesInOrder(deletedItems, itemStates, entities, trans);
			result += await SaveChangesInOrder(addedItems, itemStates, entities, trans);
			trans.Commit();
			return result;
		}

		async Task<int> SaveChangesInOrder(List<IGrouping<Type, EntityEntry>> changedItems, IDictionary<EntityEntry, EntityState> itemStates, SafeDbContext entities, IDbContextTransaction transaction)
		{
			var result = 0;
			var idx = 0;
			while (changedItems.Count > 0)
			{
				var pickedElement = changedItems.ElementAtOrDefault(idx);
				var entityState = itemStates[pickedElement.First()];
				var hasDependency = entityState == EntityState.Deleted ?
					changedItems.Any(x => x.Key.IsRelatedTo(pickedElement.Key, false)) :
					changedItems.Any(x => pickedElement.Key.IsRelatedTo(x.Key, false));
				if (hasDependency)
				{
					if (idx < changedItems.Count - 1)
					{
						idx++;
					}
					else
					{
						throw new NotSupportedException("All the entities have dependency. It cannot happen.");
					}
				}
				else
				{
					var pickedElementItems = pickedElement.ToArray();
					if (entityState == EntityState.Added)
					{
						result += await ProcessAddedItems(pickedElement.Key, pickedElementItems, entities, transaction);
					}
					else
					{
						Array.ForEach(pickedElementItems, x => x.State = itemStates[x]);
						result += await entities.SaveChangesAsync();
					}
					changedItems.RemoveAll(o => o.Key == pickedElement.Key);
					idx = 0;
				}
			}
			return result;
		}

		async Task<int> ProcessAddedItems(Type entityType, EntityEntry[] addedItems, SafeDbContext entities, IDbContextTransaction transaction)
		{
			int result = 0;
			var useBulkInsert = ShouldUseBulkInsert(entityType);
			var bulkInsertMethod = GetType().GetMethod(nameof(BulkInsertAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
			if (addedItems.Length > 0)
			{
				if (!useBulkInsert)
				{
					Array.ForEach(addedItems, x => x.State = EntityState.Added);
				}

				var policy = Policy
					.Handle<SqlException>()
					.Or<DbUpdateException>(ex => ex.InnerException is SqlException)
					.WaitAndRetryAsync(
						retryCount: 3,
						sleepDurationProvider: _ => TimeSpan.Zero,
						onRetryAsync: async (exception, _, _, _) =>
						{
							Console.WriteLine($"Retrying due to exception: {exception.Message}");
							if (!useBulkInsert)
							{
								// manually create savepoint and roll back to it if exception occurs because savepoint is incompatible with MARS
								await transaction.RollbackToSavepointAsync("BeforeRetry");
							}
							var ex = exception as SqlException ?? (exception as DbUpdateException)?.InnerException as SqlException;
							var bulkInsertHelper = Activator.CreateInstance(typeof(BulkInsertDuplicateKeyExceptionHandler<>).MakeGenericType(entityType), ex, entities);
							var deleteDuplicateRecordMethod = bulkInsertHelper.GetType().GetMethod(nameof(BulkInsertDuplicateKeyExceptionHandler<object>.DeleteDuplicateRecord), BindingFlags.Instance | BindingFlags.Public);
							var deleteDuplicateRecordInvoke = (Task<bool>)deleteDuplicateRecordMethod.Invoke(bulkInsertHelper, []);
							if (!await deleteDuplicateRecordInvoke)
							{
								throw exception;
							}
						});

				await policy.ExecuteAsync(async () =>
				{
					if (useBulkInsert)
					{
						var genericBulkInsertMethod = bulkInsertMethod.MakeGenericMethod(entityType);
						await (Task)genericBulkInsertMethod.Invoke(this, new[] { (object)addedItems.Select(x => x.Entity).ToArray(), entities });
						result += addedItems.Length;
					}
					else
					{
						await transaction.CreateSavepointAsync("BeforeRetry");
						result += await entities.SaveChangesAsync();
					}
				});

			}
			return result;
		}

		protected static async Task BulkInsertAsync<T>(IEnumerable<object> data, SafeDbContext entities)
		{
			Argument.NotNull(data, nameof(data));
			Argument.NotNull(entities, nameof(entities));
			var transaction = entities.Database?.CurrentTransaction?.GetDbTransaction();
			await entities.BulkInsertAsync(data.Cast<T>(), transaction, SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints);
		}

		static bool ShouldUseBulkInsert(Type entityType)
		{
			var entityName = entityType.Name;
			// todo : will support bulk insert into views in the future
			bool isEntityView = entityName.EndsWith("View", StringComparison.OrdinalIgnoreCase);
			bool isEntityNotUseBulkInsert = ConfigurationProvider.TablesNotUseBulkInsert.Contains(entityName, StringComparer.OrdinalIgnoreCase);
			return !(isEntityView || isEntityNotUseBulkInsert);
		}
	}
}
