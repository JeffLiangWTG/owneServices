using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class WorkflowConditionValueParentCache : IWorkflowConditionValueParentCache
	{
		class CacheItems
		{
			public readonly ZDateTime RefreshDate;
			readonly Dictionary<ZGuid, ZString> recordValueMap;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Avoid BusinessObject Creation Overhead")]
			public CacheItems(BusinessObjectFactory factory, ZGuid parentId)
			{
				recordValueMap = new Dictionary<ZGuid, ZString>();
				using (var command = Db.Connection.Command($"SELECT {StmNoteSchema.ST_ParentID.Name}, {StmNoteSchema.ST_NoteText.Name} FROM {StmNoteSchema.Constants.SqlSchemaName}.{StmNoteSchema.Constants.TableName} WHERE {StmNoteSchema.ST_Table.Name}=@p1 AND {StmNoteSchema.ST_NoteType.Name}=@p2 AND {StmNoteSchema.ST_Description.Name}=@p3 AND {StmNoteSchema.ST_ParentID.Name} IN (SELECT {ProcessTasksSchema.PK.Name} FROM {ProcessTasksSchema.Constants.SqlSchemaName}.{ProcessTasksSchema.Constants.TableName} WHERE {ProcessTasksSchema.P9_ParentID.Name}=@p4)"))
				{
					command.AddParameter("@p1", System.Data.SqlDbType.VarChar, ProcessTasksSchema.Constants.TableName);
					command.AddParameter("@p2", System.Data.SqlDbType.VarChar, nameof(StmNoteVisibility.DOC));
					command.AddParameter("@p3", System.Data.SqlDbType.VarChar, "User Defined Condition");
					command.AddParameter("@p4", System.Data.SqlDbType.UniqueIdentifier, parentId.ToGuid());

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var key = (Guid)reader[StmNoteSchema.ST_ParentID.Name];
							if (!recordValueMap.ContainsKey(key))
							{
								recordValueMap.Add(key, (string)reader[StmNoteSchema.ST_NoteText.Name]);
							}
						}
					}
				}

				RefreshDate = ZDateTime.UtcNow;
			}

			public bool TryGetValue(ProcessTask task, out ZString value)
			{
				return recordValueMap.TryGetValue(task.PK, out value);
			}
		}

		readonly Dictionary<ZGuid, CacheItems> parentCacheItemsMap = new Dictionary<ZGuid, CacheItems>();
		readonly ReaderWriterLockSlim cacheItemsReaderWriterLock = new ReaderWriterLockSlim();

		public ZString GetConditionValue(ProcessTask task)
		{
			try
			{
				cacheItemsReaderWriterLock.EnterReadLock();
				if (!parentCacheItemsMap.TryGetValue(task.P9_ParentID, out var cacheItems) || cacheItems.RefreshDate < task.P9_SystemLastEditTimeUtc)
				{
					cacheItemsReaderWriterLock.ExitReadLock();
					cacheItemsReaderWriterLock.EnterWriteLock();

					if (!parentCacheItemsMap.TryGetValue(task.P9_ParentID, out cacheItems) || cacheItems.RefreshDate < task.P9_SystemLastEditTimeUtc)
					{
						parentCacheItemsMap[task.P9_ParentID] = (cacheItems = new CacheItems(task.Factory, task.P9_ParentID));
					}
				}

				if (task.P9_SystemLastEditTimeUtc.IsValid && cacheItems.RefreshDate < task.P9_SystemLastEditTimeUtc.AddMinutes(1))
				{
					return task.TemplateConditions.TemplateCondition2Value;
				}

				return cacheItems.TryGetValue(task, out var result) ? result : task.TemplateConditions.TemplateCondition2Value;
			}
			finally
			{
				if (cacheItemsReaderWriterLock.IsReadLockHeld)
				{
					cacheItemsReaderWriterLock.ExitReadLock();
				}
				else if (cacheItemsReaderWriterLock.IsWriteLockHeld)
				{
					cacheItemsReaderWriterLock.ExitWriteLock();
				}
			}
		}
	}
}
