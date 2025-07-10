using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public abstract class TemplateApplicationRaceConditionHandlerBase<T> : ITemplateApplicationRaceConditionHandler where T : EnterpriseBusinessObject
	{
		public const int ProcessBatchSize = 50;
		protected BusinessObjectFactory Factory { get; }
		protected bool TryHandleConflictsAutomatically { get; }
		readonly ITableSchema tableSchema;
		readonly SchemaGuidColumn parentColumn;
		readonly SchemaGuidColumn columnToLookup;

		#region Events

		public event EventHandler OnBeforeProcess;
		public event EventHandler OnBeforeLock;
		public event EventHandler OnAfterLock;

		#endregion

		public TemplateApplicationRaceConditionHandlerBase(BusinessObjectFactory factory, bool tryHandleConflictsAutomatically, SchemaGuidColumn parentColumn, SchemaGuidColumn parentTemplateColumn = null)
		{
			Factory = factory;
			TryHandleConflictsAutomatically = tryHandleConflictsAutomatically;
			this.parentColumn = parentColumn;
			tableSchema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
			columnToLookup = parentTemplateColumn ?? parentColumn;
		}

		public bool ShouldProcess() => ShouldProcessCore();

		public void Process(IEnumerable<BusinessObject> businesObjects)
		{
			if (ShouldProcess())
			{
				ProcessCore(businesObjects.OfType<T>());
			}
		}

		protected abstract bool ShouldLock { get; }
		protected abstract bool ShouldProcessCore();
		protected abstract bool ShouldCheck(T businesObjects);
		protected abstract bool ParentExists(T businesObjects);
		protected abstract bool IsDuplicate(T original, T supected);
		protected abstract bool Merge(T original, T duplicate);

		void ProcessCore(IEnumerable<T> businesObjects)
		{
			BeforeProcess();

			businesObjects = Filter(businesObjects);

			if (!businesObjects.Any())
			{
				return;
			}

			var lookup = businesObjects.ToLookup(bo => (ZGuid)bo[columnToLookup]);
			var parentIDBatch = businesObjects.Select(x => (ZGuid)x[parentColumn])
				.Distinct()
				.Batch(ProcessBatchSize);

			foreach (var parentIDs in parentIDBatch)
			{
				Check(parentIDs, lookup);
			}
		}

		ICollection<T> Filter(IEnumerable<T> businesObjects)
		{
			return businesObjects.Where(bo => !bo.IsDeleted && !bo.IsInDatabase && ShouldCheck(bo) && ParentExists(bo)).ToList();
		}

		void Check(IEnumerable<ZGuid> parentIDs, ILookup<ZGuid, T> lookup)
		{
			var shouldCheck = TryLock(parentIDs);

			if (!shouldCheck)
			{
				return;
			}

			var query = new ZDBOnlyQuery(typeof(T));
			query.AddToFilter(parentColumn, parentIDs);
			query.IgnoreDbQueryCache = true;
			var originals = Factory.Load<T>(query);

			foreach (var original in originals)
			{
				var pk = (ZGuid)original[columnToLookup];
				var suspecteds = lookup[pk].Where(suspect =>
					!original.IsDeleted
					&& !suspect.IsDeleted
					&& suspect.PK != original.PK);

				var duplicateds = suspecteds.Where(suspected => IsDuplicate(original, suspected));

				foreach (T suspected in duplicateds)
				{
					if (TryHandleConflictsAutomatically)
					{
						Merge(original, suspected);
					}
				}
			}
		}

		bool TryLock(IEnumerable<ZGuid> parentIDs)
		{
			if (!ShouldLock)
			{
				return true;
			}

			BeforeLock();

			var hasLocked = TemplateApplicationTableLocker.Lock(((IDbConnected)Factory).Connection, tableSchema, parentColumn, parentIDs);

			AfterLock();

			return hasLocked;
		}

		void BeforeProcess() => OnBeforeProcess?.Invoke(this, default);
		void BeforeLock() => OnBeforeLock?.Invoke(this, default);
		void AfterLock() => OnAfterLock?.Invoke(this, default);

		protected internal static void ThrowConcurrencyException(T duplicate, string message)
		{
			var row = ((INeedRow)duplicate).Row;
			throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new DBConcurrencyException(message, null, new[] { row }), row, Db.Connection), duplicate.Factory);
		}
	}
}

