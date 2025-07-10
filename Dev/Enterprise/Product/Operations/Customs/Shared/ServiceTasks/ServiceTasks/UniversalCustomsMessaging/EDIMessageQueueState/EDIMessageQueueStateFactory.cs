using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

#pragma warning disable CW1108
#pragma warning disable CW1107

namespace Enterprise.Customs.ServiceTasks
{
	public class EDIMessageQueueStateFactory : IQueueStateFactory<EDIMessageQueueState>
	{
		public EDIMessageQueueStateFactory(LoggingInformation logger, string applicationCode, Func<GrEngineLogOptions> getLogOptions, int maxConcurrentHandles)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.ApplicationCode = Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode));
			LogOptionsSetup = new Lazy<GrEngineLogOptions>(getLogOptions);
			_appLockPrefix = applicationCode + "DEQUEUEGRENGINE";
			this.maxConcurrentHandles = Argument.GreaterThanOrEqual(maxConcurrentHandles, 0, nameof(maxConcurrentHandles));
		}
		public readonly string ApplicationCode;
		readonly string _appLockPrefix;
		readonly LoggingInformation logger;
		readonly int maxConcurrentHandles;

		public EDIMessageQueueState NewQueueState<TEntity>(TEntity businessObject, string[] keys, ZString messageNumber)
			where TEntity : BusinessObject
		{
			return new EDIMessageQueueState(businessObject as EDIMessage, keys, messageNumber);
		}

		public GrEngineLogOptions LogOptions => LogOptionsSetup.Value;
		Lazy<GrEngineLogOptions> LogOptionsSetup { get; }

		const string MAX_ROWS_PARAM = "@maxRows";

		public QueueBatch<EDIMessageQueueState> GetDequeueBatch(BusinessObjectFactory factory, INotifications notifications, ISqlApplicationLockProvider lockProvider, GrEngineLogOptions logOptions, int dequeueBatchSize, int dequeueListSizeEstimate, GrEngineEnums.ChainOption chainOption)
		{
			var connection = ((IDbConnected)factory).Connection;
			// the maximum batch size should be based on how many concurrent instance the system support
			var maxBatchSize = dequeueBatchSize * maxConcurrentHandles;
			var queuedRows = LoadPks(connection, maxBatchSize);

			if (logOptions.AllWorkerLoads)
			{
				notifications.AddInfo(FormattableString.Invariant($"Loaded {queuedRows.Length} keys."));
			}

			QueueBatch<EDIMessageQueueState> result = null;
			if (queuedRows.Length > 0)
			{
				result = LoadBatch(connection, queuedRows, notifications, lockProvider, logOptions, dequeueBatchSize, dequeueListSizeEstimate, chainOption);
				// If for some unknown reason we are unable to lock any then try again
				if (queuedRows.Length == maxBatchSize && !result.Any())
				{
					result = GetDequeueBatch(factory, notifications, lockProvider, logOptions, dequeueBatchSize, dequeueListSizeEstimate, chainOption);
				}
			}
			return result ?? new (Enumerable.Empty<EDIMessageQueueState>());
		}

		/// <summary>
		/// This is a fallback mechanism to increase the throughput of GrEngine services.
		/// This mechanism will cause blocked items to be loaded with the assumption that they will be unblocked by items in the same batch.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		QueueBatch<EDIMessageQueueState> LoadBatch(DbConnection connection, (Guid pk, Guid? chainId)[] queuedItems, INotifications notifications, ISqlApplicationLockProvider lockProvider, GrEngineLogOptions logOptions, int dequeueBatchSize, int dequeueListSizeEstimate, GrEngineEnums.ChainOption chainOption)
		{
			var result = new List<EDIMessageQueueState>(dequeueListSizeEstimate);
			var appLocks = new List<ISqlApplicationLock>(dequeueListSizeEstimate);

			var queue = new Queue<(Guid pk, Guid? chainId)>(queuedItems);

			while (queue.Any() && result.Count < dequeueBatchSize)
			{
				var (itemId, chainId) = queue.Dequeue();
				if (TryGetLocks(ref itemId, ref chainId, appLocks, out var newAppLocks, lockProvider, chainOption))
				{
					LogLocksTaken(notifications, newAppLocks, logOptions);

					try
					{
						// It is important to ensure that the state hasn't changed between the initial load and the locking
						// We do this by loading again.
						const string pk = "@pk";
						var filter = FormattableString.Invariant($@"WHERE EQS_ApplicationCode = {ApplicationCodeParam}
AND EQS_PK = {pk}
AND EQS_Status = '{QueueStatusCodes.Codes.Queued}'");
						var queueStateList = Load(connection, filter, command =>
						{
							AddApplicationCodeParam(command);
							command.AddParameter(pk, SqlDbType.UniqueIdentifier, itemId);
						}, 1);

						if (queueStateList.Count == 1)
						{
							var queueState = queueStateList[0];
							result.Add(queueState);
							appLocks.AddRange(newAppLocks);
							var maxRows = dequeueBatchSize - result.Count;

							if (!queueState.ChainID.Equals(chainId ?? Guid.Empty))
							{
								ErrorReporter.Instance.Report("EDIMessageQueueState - Chain ID should never change after a row is queued", FormattableString.Invariant($"Chain ID should never change after a row is queued: OriginalChainID:{chainId}, QueueStateChainID:{queueState.ChainID}, QueueStateParentID:{queueState.MessagePK}"), null);
							}
							else if (chainOption == GrEngineEnums.ChainOption.Yes && chainId.HasValue && maxRows > 0)
							{
								result.AddRange(LoadChain(connection, queueState.ChainID, maxRows));
							}
						}
						else
						{
							newAppLocks.ForEach(l => l.Dispose()); // Couldn't load the EDIMessageQueueState, since it was probably already processed.
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						appLocks.ForEach(l => l.Dispose());
						newAppLocks.ForEach(l => l.Dispose());
						throw;
					}
				}
			}

			if (logOptions.AllWorkerLoads && result.Count > 0)
			{
				notifications.AddInfo(FormattableString.Invariant($"Messages loaded ({result.Count}): {string.Join(", ", result.Select(s => s.GetParentDetails()).Distinct())}"));
			}

			return new QueueBatch<EDIMessageQueueState>(result, appLocks.ToArray());
		}

		void LogLocksTaken(INotifications notifications, List<ISqlApplicationLock> newAppLocks, GrEngineLogOptions logOptions)
		{
			if (logOptions.AllWorkerLocks)
			{
				notifications.AddInfo(FormattableString.Invariant($"Locks taken: {string.Join(", ", newAppLocks.Select(s => s.Key))}"));
			}
		}

		bool TryGetLocks(ref Guid item, ref Guid? chainId, List<ISqlApplicationLock> appLocks, out List<ISqlApplicationLock> newAppLocks, ISqlApplicationLockProvider lockProvider, GrEngineEnums.ChainOption chainOption)
		{
			// The rule is, that if something is queued it can no longer get a chain id added.
			// This means we only have to take 1 lock for the whole chain.
			// And it means that queue'd items can't change after we loaded them.
			// THIS RULE WAS BROKEN - a queued item with an item lock was getting a chain id and a second chain lock allowed it to get processed twice.
			// This is now fixed in WI00244129.
			// We continue to take both an item lock and chain lock to minimize risk.
			newAppLocks = new List<ISqlApplicationLock>(2);
			if (lockProvider.TryGetLock(_appLockPrefix + item.ToString().ToUpperInvariant(), out var itemAppLock))
			{
				if (appLocks.Any(l => l.Key == itemAppLock.Key))
				{
					//lets not get the same item lock in the same process.
					itemAppLock.Dispose();
				}
				else
				{
					if (chainOption == GrEngineEnums.ChainOption.Yes && chainId.HasValue)
					{
						if (lockProvider.TryGetLock(_appLockPrefix + chainId.ToString().ToUpperInvariant(), out var chainAppLock))
						{
							if (appLocks.Any(l => l.Key == chainAppLock.Key))
							{
								//if we have alredy got this chain app lock in this process then we are already processing the chain
								itemAppLock.Dispose();
								chainAppLock.Dispose();
							}
							else
							{
								newAppLocks.Add(itemAppLock);
								newAppLocks.Add(chainAppLock);
							}
						}
						else
						{
							itemAppLock.Dispose();
						}
					}
					else
					{
						newAppLocks.Add(itemAppLock);
					}
				}
			}
			return newAppLocks.Any();
		}

		IEnumerable<EDIMessageQueueState> LoadChain(DbConnection connection, Guid chainKey, int maxRows)
		{
			if (chainKey == Guid.Empty)
			{
				ErrorReporter.ReportOnce("Something terrible has happened.");
				return Enumerable.Empty<EDIMessageQueueState>();
			}
			else
			{
				const string chainIdParam = "@ChainId";
				var filter = FormattableString.Invariant($@"
WHERE EQS_ApplicationCode = {ApplicationCodeParam}
AND EQS_ChainID = {chainIdParam}
AND EQS_Status = '{QueueStatusCodes.Codes.Blocked}'
ORDER BY EQS_ParentSystemCreateTimeUtc ASC, EQS_ParentMessageNumber ASC");
				return Load(connection, filter, (command) =>
				{
					AddApplicationCodeParam(command);
					command.AddParameter(chainIdParam, EDIMessageQueueStateSchema.EQS_ChainID.SqlDbType, chainKey);
				}, maxRows, maxRows: maxRows);
			}
		}

		(Guid id, Guid? chainId)[] LoadPks(DbConnection connection, int dequeueBatchSize)
		{
			var queryText = FormattableString.Invariant($@"
SELECT TOP ({MAX_ROWS_PARAM}) EQS_PK, EQS_ChainID 
FROM dbo.EDIMessageQueueState 
WHERE EQS_ApplicationCode = {ApplicationCodeParam}
AND EQS_Status = '{QueueStatusCodes.Codes.Queued}'
AND --need lock on ChainID if exists
	CASE WHEN EQS_ChainID = '00000000-0000-0000-0000-000000000000' THEN 1 ELSE APPLOCK_TEST('public', '{_appLockPrefix}' + CONVERT(NVARCHAR(50), EQS_ChainID), 'exclusive', 'session') END = 1
AND --need lock on PK always
	APPLOCK_TEST('public', '{_appLockPrefix}' + CONVERT(NVARCHAR(50), EQS_PK), 'exclusive', 'session') = 1");

			try
			{
				using (var command = connection.Command(queryText))
				{
					AddApplicationCodeParam(command);
					command.AddParameter(MAX_ROWS_PARAM, SqlDbType.Int, dequeueBatchSize);
					var result = new List<(Guid, Guid?)>();
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var chainID = reader.GetGuid(1);
							result.Add((reader.GetGuid(0), Guid.Empty.Equals(chainID) ? null : chainID));
						}
					}
					return result.ShuffleListInRandomOrder();
				}
			}
			catch (SqlException ex)
			{
				ErrorReporter.ReportOnce("Unexpected exception loading batch.", ex);
				return Array.Empty<(Guid, Guid?)>();
			}
		}

		public int Notify(IEnumerable<QueueStateResult<EDIMessageQueueState>> batch)
		{
			var modified = 0;
			using (Db.Connection.TemporarySetDeadlockPriority(DeadlockPriority.Low))
			{
				var groups = batch.GroupBy(g => g.Type);

				foreach (var group in groups)
				{
					switch (group.Key)
					{
						case QueueStateResultType.Failed: // Failure gets marked as success. This is because error handling should be implemented by consumers.
						case QueueStateResultType.Processed:
							modified += UpdateColumn(group.Select(s => s.State), EDIMessageQueueStateSchema.EQS_Status, QueueStatusCodes.Codes.Notify, BuildSingleUpdateColumnQueries);
							break;

						case QueueStateResultType.None:
							// Do nothing.
							break;

						default:
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unsupported type {0} for notification", group.Key));
					}
				}
			}
			return modified;
		}

		int UpdateColumn(IEnumerable<EDIMessageQueueState> items, SchemaColumn columnName, object value, Func<IEnumerable<EDIMessageQueueState>, SchemaColumn, object, IEnumerable<(string updateScript, int itemsCount)>> buildUpdateColumnQuery)
		{
			var count = 0;
			foreach (var updateQueryData in buildUpdateColumnQuery(items, columnName, value))
			{
				RunScriptWithRetryIfDeadLock(updateQueryData.updateScript);
				count += updateQueryData.itemsCount;
			}
			return count;
		}

		void RunScriptWithRetryIfDeadLock(string script)
		{
			try
			{
				Db.Connection.ExecuteNonQuery(script);
			}
			catch (SqlException ex) when (ex.IsInnermostDeadlock())
			{
				// We retry again in next run
				RunScriptWithRetryIfDeadLock(script);
			}
		}

		IEnumerable<(string updateScript, int itemsCount)> BuildSingleUpdateColumnQueries(IEnumerable<EDIMessageQueueState> items, SchemaColumn columnName, object value)
		{
			var applicationCodeColumnType = $"{ApplicationCodeParam} {EDIMessageQueueStateSchema.EQS_ApplicationCode.SqlDbTypeDeclaration}, ";
			var applicationCodeParameterBinding = $"{ApplicationCodeParam} = '{ApplicationCode}', ";
			foreach (var itemsToUpdate in items.Select(b => b.Identifier).Batch(100))
			{
				var builder = new StringBuilder();
				var newValues = new List<(SchemaColumn columnName, object value)>();
				newValues.Add((columnName, value));
				newValues.Add((EDIMessageQueueStateSchema.EQS_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code));
				newValues.Add((EDIMessageQueueStateSchema.EQS_SystemLastEditTimeUtc, ZDateTime.UtcNow.ToDateTime()));

				var count = 0;
				foreach (var pk in itemsToUpdate)
				{
					#region SuppressResourceStringsCheckRegion
					var columnSetters = string.Join(", ", newValues.Select((value, i) => FormattableString.Invariant($"{value.columnName.Name} = @{i}")));
					var valuesWithPk = newValues.Append((EDIMessageQueueStateSchema.PK, pk)); // This is the last parameter, and is not part of the parameter list, so we manually declare the parameter in the EXEC statement.
					var columnTypes = applicationCodeColumnType + string.Join(", ", valuesWithPk.Select((value, i) => FormattableString.Invariant($"@{i} {value.columnName.SqlDbTypeDeclaration}")));
					var parameterBindings = applicationCodeParameterBinding + string.Join(", ", valuesWithPk.Select((value, i) => FormattableString.Invariant($"@{i} = '{value.value}'")));
					builder.AppendLine(FormattableString.Invariant($"EXEC sys.sp_executesql N'UPDATE dbo.EDIMessageQueueState SET {columnSetters} WHERE EQS_ApplicationCode = {ApplicationCodeParam} AND EQS_PK = @{newValues.Count} AND EQS_Status NOT IN (''NTF'', ''PRS'')', N'{columnTypes}', {parameterBindings};"));
					#endregion
					count++;
				}
				yield return (builder.ToString(), count);
			}
		}

		IEnumerable<(string updateScript, int itemsCount)> BuildUpdateColumnQuery(IEnumerable<EDIMessageQueueState> items, SchemaColumn columnName, object value)
		{
			var newValues = new List<(SchemaColumn columnName, object value)>();
			newValues.Add((columnName, value));
			newValues.Add((EDIMessageQueueStateSchema.EQS_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code));
			newValues.Add((EDIMessageQueueStateSchema.EQS_SystemLastEditTimeUtc, ZDateTime.UtcNow.ToDateTime()));
			var itemsToUpdateBatches = items.Select(b => b.Identifier).Batch(100);
			var columnSettersStringBuilder = new ZStringBuilder();
			var columnTypesStringBuilder = new ZStringBuilder();
			var parameterBindingsStringBuilder = new ZStringBuilder();
			newValues.ForEach(x =>
			{
				var columnName = x.columnName;
				columnSettersStringBuilder.Append(FormattableString.Invariant($"{columnName.Name} = @_{columnName.Name}"));
				columnTypesStringBuilder.Append(FormattableString.Invariant($"@_{columnName.Name} {columnName.SqlDbTypeDeclaration}"));
				parameterBindingsStringBuilder.Append(FormattableString.Invariant($"@_{columnName.Name} = '{x.value}'"));
			});
			var columnSetters = columnSettersStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			var columnTypes = $"{ApplicationCodeParam} {EDIMessageQueueStateSchema.EQS_ApplicationCode.SqlDbTypeDeclaration}, " + columnTypesStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			var parameterBindings = $"{ApplicationCodeParam} = '{ApplicationCode}', " + parameterBindingsStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
			var pkSqlTypeDeclaration = EDIMessageQueueStateSchema.PK.SqlDbTypeDeclaration;
			foreach (var itemsToUpdateBatch in itemsToUpdateBatches)
			{
				var count = 0;
				var pkWhereClausesStringBuilder = new ZStringBuilder();
				var pkParameterDefinitionsStringBuilder = new ZStringBuilder();
				var pkParameterBindingsStringBuilder = new ZStringBuilder();
				foreach (var pk in itemsToUpdateBatch.OrderBy(x => x))
				{
					count++;
					var pkParameterName = "@_PK" + count;
					pkWhereClausesStringBuilder.Append(pkParameterName);
					pkParameterDefinitionsStringBuilder.Append(FormattableString.Invariant($"{pkParameterName} {pkSqlTypeDeclaration}"));
					pkParameterBindingsStringBuilder.Append(FormattableString.Invariant($"{pkParameterName} = '{pk}'"));
				}
				yield return (FormattableString.Invariant($"EXEC sys.sp_executesql N'UPDATE dbo.EDIMessageQueueState WITH (REPEATABLEREAD) SET {columnSetters} WHERE EQS_ApplicationCode = {ApplicationCodeParam} AND EQS_PK IN ({pkWhereClausesStringBuilder.ToStringWithDelimiterBetweenAppends(", ")}) AND EQS_Status NOT IN (''NTF'', ''PRS'')', N'{columnTypes}, {pkParameterDefinitionsStringBuilder.ToStringWithDelimiterBetweenAppends(", ")}', {parameterBindings}, {pkParameterBindingsStringBuilder.ToStringWithDelimiterBetweenAppends(", ")};"), count);
			}
		}

		public IEnumerable<EDIMessageQueueState> LoadAll(int enqueueBatchSize)
		{
			return Load(Db.Connection, GetApplicationCodeFilter(), AddApplicationCodeParam, enqueueBatchSize);
		}

		public const string ApplicationCodeParam = "@ApplicationCode";

		void AddApplicationCodeParam(DbCommand command)
		{
			command.AddParameter(ApplicationCodeParam, SqlDbType.Char, ApplicationCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		string GetApplicationCodeFilter()
		{
			return string.Format(CultureInfo.InvariantCulture, @" WHERE EQS_ApplicationCode = {0}", ApplicationCodeParam);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public IList<EDIMessageQueueState> LoadAllUnprocessed(int enqueueBatchSize)
		{
			var filter = string.Format(CultureInfo.InvariantCulture,
				"{0} AND EQS_Status IN ('{1}', '{2}')", GetApplicationCodeFilter(),
				QueueStatusCodes.Codes.Queued,
				QueueStatusCodes.Codes.Blocked);

			return Load(Db.Connection, filter, AddApplicationCodeParam, enqueueBatchSize);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public IList<EDIMessageQueueState> LoadPreKeys(int enqueueBatchSize, int? maximumRows, ZQuery query)
		{
			var additionalFilter = query.FilterString;

			var filter = string.Format(CultureInfo.InvariantCulture,
				"{0} AND EQS_Status = '{1}' {2} ORDER BY EQS_ParentSystemCreateTimeUtc ASC, EQS_ParentMessageNumber ASC", GetApplicationCodeFilter(),
				QueueStatusCodes.Codes.PreKey,
				string.IsNullOrEmpty(additionalFilter) ? string.Empty : "AND " + additionalFilter);
			return Load(Db.Connection, filter, command =>
			{
				AddApplicationCodeParam(command);
				foreach (var parameter in query.Params)
				{
					command.AddParameter(parameter);
				}
			}, enqueueBatchSize, maximumRows);
		}

		public IEnumerable<EDIMessageQueueState> LoadNotified(int enqueueBatchSize)
		{
			try
			{
				using (Db.Connection.TemporarySetDeadlockPriority(DeadlockPriority.Low))
				{
					var systemLastEditUserParam = "@SystemLastEditUser";
					var systemLastEditTimeUtcParam = "@SystemLastEditTimeUtc";

					var preSqlText = string.Format(CultureInfo.InvariantCulture, $@"
DECLARE @t TABLE (PK UNIQUEIDENTIFIER)
UPDATE dbo.EDIMessageQueueState WITH (READPAST, READCOMMITTEDLOCK) SET EQS_Status = '{QueueStatusCodes.Codes.Processed}', EQS_SystemLastEditUser = @SystemLastEditUser, EQS_SystemLastEditTimeUtc = @SystemLastEditTimeUtc
OUTPUT inserted.EQS_PK INTO @T
WHERE EQS_ApplicationCode = {ApplicationCodeParam} AND EQS_Status = '{QueueStatusCodes.Codes.Notify}'
AND --need lock on ChainID if exists in case there is still active blocked rows being processed by UCQ
	CASE WHEN EQS_ChainID = '00000000-0000-0000-0000-000000000000' THEN 1 ELSE APPLOCK_TEST('public', '{_appLockPrefix}' + CONVERT(NVARCHAR(50), EQS_ChainID), 'exclusive', 'session') END = 1
");
					return Load(Db.Connection, $@"INNER JOIN @T ON PK = EQS_PK
WHERE EQS_ApplicationCode = {ApplicationCodeParam}", command =>
					{
						AddApplicationCodeParam(command);
						command.AddParameterBasedOnDbColumn(systemLastEditUserParam, GlbStaff.CurrentUser.GS_Code.ToString(), EDIMessageQueueStateSchema.EQS_SystemLastEditUser);
						command.AddParameterBasedOnDbColumn(systemLastEditTimeUtcParam, ZDateTime.UtcNow.ToDateTime(), EDIMessageQueueStateSchema.EQS_SystemLastEditTimeUtc);
					}, enqueueBatchSize, preSqlText: preSqlText);
				}
			}
			catch (SqlException ex) when (ex.IsInnermostDeadlock())
			{
				// We retry again in next run
				return Enumerable.Empty<EDIMessageQueueState>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public void DeleteOldProcessed(INotifications notifications, int additionalHours)
		{
			try
			{
				var daysParam = "@Days";

				var commandSql = FormattableString.Invariant($@"DELETE FROM dbo.EDIMessageQueueState WITH (READPAST, READCOMMITTEDLOCK)
WHERE EQS_ApplicationCode = {ApplicationCodeParam}
AND EQS_SystemCreateTimeUtc <= {daysParam} AND EQS_Status = '{QueueStatusCodes.Codes.Processed}'");

				var connection = Db.Connection;
				using (connection.TemporarySetDeadlockPriority(DeadlockPriority.Low))
				using (var command = connection.Command(commandSql))
				{
					AddApplicationCodeParam(command);
					var hours = additionalHours + 1;
					var utcNow = ZDateTime.UtcNow;
					command.AddParameter(daysParam, SqlDbType.DateTime, new ZDateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0).AddMinutes(1).AddHours(-hours).ToDateTime());

					var rowCount = command.ExecuteNonQuery();
					if (rowCount > 0)
					{
						notifications?.Add(CargoWise.ComponentModel.NotificationType.Information, string.Format(CultureInfo.InvariantCulture, "Deleting {0} processed rows from {1} hours ago for {2}.", rowCount, hours, ApplicationCode));
					}
				}
			}
			catch (SqlException ex) when (ex.IsInnermostDeadlock())
			{
				// Deadlock will try again in next run
			}
		}

		public QueueResult QueueResultObject(string queueStatus, GrEngineEnums.Status includeOrExclude) => GetQueueResultObject(ApplicationCode, queueStatus, includeOrExclude == GrEngineEnums.Status.Include);

		public static QueueResult GetQueueResultObject(string applicationCode, string queueStatus, bool include)
		{
			var result = GetCountBacklog(applicationCode, queueStatus, include);
			return new QueueResult(result.count, result.maximumItemAge);
		}

		public int CountBacklog(string queueStatus, GrEngineEnums.Status includeOrExclude) => GetCountBacklog(ApplicationCode, queueStatus, includeOrExclude == GrEngineEnums.Status.Include).count;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public static (int count, TimeSpan maximumItemAge) GetCountBacklog(string applicationCode, string queueStatus, bool include)
		{
			var count = 0;
			var maximumItemAge = TimeSpan.Zero;
			const string code = "@code";
			using (var cmd = Db.Connection.Command($"SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, EQS_SystemCreateTimeUtc, GETUTCDATE())), 0) FROM dbo.EDIMessageQueueState WHERE EQS_ApplicationCode = @code AND EQS_Status {(include ? "=" : "<>")} '{queueStatus}'"))
			{
				cmd.AddParameter(code, EDIMessageQueueStateSchema.EQS_ApplicationCode.SqlDbType, applicationCode);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						count = reader.GetInt32(0);
						maximumItemAge = TimeSpan.FromSeconds(reader.GetInt32(1));
					}
				}
			}
			return (count, maximumItemAge);
		}

		public void Update(IEnumerable<EDIMessageQueueState> queues)
		{
			var chainIdChangedDictionary = new Dictionary<Guid, List<EDIMessageQueueState>>();
			var statusChangedDictionary = new Dictionary<string, List<EDIMessageQueueState>>();
			var syncedList = new List<EDIMessageQueueState>();
			foreach (var queueState in queues)
			{
				if (queueState.ChainIdChanged)
				{
					chainIdChangedDictionary.GetOrAdd(queueState.ChainID, () => new List<EDIMessageQueueState>()).Add(queueState);
				}
				if (queueState.StatusChanged)
				{
					statusChangedDictionary.GetOrAdd(queueState.Status, () => new List<EDIMessageQueueState>()).Add(queueState);
				}
				syncedList.Add(queueState);
			}

			foreach (var queueGroup in chainIdChangedDictionary)
			{
				UpdateColumn(queueGroup.Value, EDIMessageQueueStateSchema.EQS_ChainID, queueGroup.Key, BuildUpdateColumnQuery);
			}
			foreach (var queueGroup in statusChangedDictionary)
			{
				UpdateColumn(queueGroup.Value, EDIMessageQueueStateSchema.EQS_Status, queueGroup.Key, BuildUpdateColumnQuery);
			}

			syncedList.ForEach(queuedState => queuedState.SetAsDatabaseSynced());
		}

		public void InsertAsQueued(IEnumerable<EDIMessageQueueState> queues)
		{
			if (queues.Any())
			{
				var syncedList = new List<EDIMessageQueueState>();
				using (var table = new DataTable(EDIMessageQueueStateSchema.Constants.TableName))
				{
					table.Locale = CultureInfo.InvariantCulture;
					foreach (var col in EDIMessageQueueStateSchema.All)
					{
						table.Columns.Add(col.Name, col.DotNetType);
					}

					var now = ZDateTime.UtcNow.ToDateTime();
					foreach (var queue in queues)
					{
						var row = table.NewRow();
						row[EDIMessageQueueStateSchema.Constants.PK] = queue.Identifier;
						row[EDIMessageQueueStateSchema.Constants.EQS_EM] = queue.MessagePK;
						row[EDIMessageQueueStateSchema.Constants.EQS_ParentMessageNumber] = queue.ParentMessageNumber;
						row[EDIMessageQueueStateSchema.Constants.EQS_ParentSystemCreateTimeUtc] = queue.ParentSystemCreateTimeUtc;
						row[EDIMessageQueueStateSchema.Constants.EQS_ApplicationCode] = ApplicationCode;
						row[EDIMessageQueueStateSchema.Constants.EQS_Keys] = string.Join(Delimiter.ToString(), queue.Keys);
						row[EDIMessageQueueStateSchema.Constants.EQS_SystemCreateTimeUtc] = now;
						row[EDIMessageQueueStateSchema.Constants.EQS_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
						row[EDIMessageQueueStateSchema.Constants.EQS_SystemLastEditTimeUtc] = now;
						row[EDIMessageQueueStateSchema.Constants.EQS_SystemLastEditUser] = GlbStaff.CurrentUser.GS_Code;
						row[EDIMessageQueueStateSchema.Constants.EQS_ChainID] = queue.ChainID;
						row[EDIMessageQueueStateSchema.Constants.EQS_Status] = queue.Status;
						if (queue.ChainID != Guid.Empty && (LogOptions?.SetChainID ?? false))
						{
							logger.LogWarning($"Adding record - PK:{queue.Identifier}, EM:{queue.MessagePK}, OldChainID:{queue.ChainID}");
						}
						table.Rows.Add(row);
						syncedList.Add(queue);
					}

					SaveDataTable(table);
				}

				syncedList.ForEach(queuedState => queuedState.SetAsDatabaseSynced());
			}
		}

		static void SaveDataTable(DataTable table)
		{
			using (var dataSet = new DataSet())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				dataSet.Tables.Add(table);

				var saver = new ZSqlSaver(dataSet, Db.Connection, ObjectFactory.Get<IApplicationSchemaResolver>());
				saver.Save();
			}
		}

		public void AddFilterOutAlreadyQueuedItems(ZDBOnlyQuery query)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ApplicationCodeParam, ApplicationCode, EDIMessageQueueStateSchema.EQS_ApplicationCode);
			query.AddFilterAndZSQLParameterCollection(
				string.Format(CultureInfo.InvariantCulture, "{0} NOT IN (SELECT EQS_EM FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK) WHERE EQS_ApplicationCode = {2} AND EQS_Status <> '{1}')",
					query.PKColumn.Name,
					QueueStatusCodes.Codes.Processed,
					ApplicationCodeParam),
				parameters);
		}

		const int BETTER_LOAD_ESTIMATE = 200;
		const char Delimiter = ',';

		static List<EDIMessageQueueState> Load(DbConnection connection, string filter, Action<DbCommand> addParameters, int rowsEstimate, int? maxRows = null, string preSqlText = "")
		{
			var result = new List<EDIMessageQueueState>(Math.Min(BETTER_LOAD_ESTIMATE, rowsEstimate));

			var maxRowsExpression = maxRows.HasValue ? FormattableString.Invariant($"TOP ({MAX_ROWS_PARAM})") : string.Empty;
			var sqlText = FormattableString.Invariant($@"{preSqlText} SELECT {maxRowsExpression}
EQS_PK,
EQS_EM,
EQS_ParentMessageNumber,
EQS_ParentSystemCreateTimeUtc,
EQS_Status,
EQS_Keys,
EQS_ChainID
FROM dbo.EDIMessageQueueState {filter}");

			using (var command = connection.Command(sqlText))
			{
				if (maxRows.HasValue)
				{
					command.AddParameter(MAX_ROWS_PARAM, SqlDbType.Int, maxRows.Value);
				}
				addParameters(command);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new EDIMessageQueueState(
							reader.GetGuid(0),
							reader.GetGuid(1),
							reader.GetString(2),
							reader.GetDateTime(3),
							reader.GetString(4),
							reader.GetString(5).Split(Delimiter),
							reader.GetGuid(6)
							));
					}
				}
			}
			return result;
		}
	}
}
