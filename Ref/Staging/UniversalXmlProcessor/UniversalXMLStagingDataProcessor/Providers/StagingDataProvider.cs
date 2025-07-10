using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SequentialGuid;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public sealed class StagingDataProvider : IStagingDataProvider
	{
		public StagingDataProvider(IStagingRepository repo)
		{
			Argument.NotNull(repo, nameof(repo));
			this.repo = repo;
		}

		readonly IStagingRepository repo;

		public bool IsAutoExpiredEnabled(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			return repo.Get<DataSourceInformation>().Any(x => x.DSI_SubSource == sourceData.SDA_SubSource && x.DSI_EnableAutoExpiration);
		}

		public IDbContextTransaction BeginTransaction()
		{
			return repo.BeginTransaction();
		}

		public void Commit(object transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			repo.Commit(transaction);
		}

		public IQueryable<SourceData> GetSourceData()
		{
			return from sourceData in repo.Get<SourceData>()
				   where sourceData.SDA_ContentType == DataSourceConstants.ContentType.UniversalXML && sourceData.SDA_Status == StatusProvider.GetPRSStatus()
					&& (!sourceData.SDA_NotProcessedUntil.HasValue || sourceData.SDA_NotProcessedUntil.Value < DateTime.UtcNow)
				   select sourceData;
		}

		public IQueryable<SourceData> GetDependencySourceData(Dependency dependency)
		{
			return from sourceData in repo.Get<SourceData>()
				   where sourceData.SDA_ContentType == DataSourceConstants.ContentType.UniversalXML && sourceData.SDA_SubSource == dependency.DataSource
					&& sourceData.SDA_SourceTime >= dependency.PublicationTime
				   select sourceData;
		}

		public SourceData GetSingleSourceData(Guid pk)
		{
			var sourceDatas =
			 from sourceData in repo.Get<SourceData>().AsNoTracking()
			 where sourceData.SDA_PK == pk
			 select sourceData;
			return sourceDatas.FirstOrDefault();
		}

		public IQueryable<DataProcessingInformation> GetDataProcessingInformation(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			return from info in repo.Get<DataProcessingInformation>()
				   where info.DPI_SourceId == sourceData.SDA_PK
				   select info;
		}

		IQueryable<DataProcessingInformation> GetQueuedDataProcessingInformation(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));

			return GetDataProcessingInformation(sourceData).Where(x => x.DPI_Status == DataProcessingStatus.QUE.ToString());
		}

		public IEnumerable<Tuple<object, DataProcessingInformation>> GetNextDataBatch(SourceData sourceData, IMetadataProvider metadataProvider, int? batchSize, DataProcessingInformation startInfo)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			var info = GetQueuedDataProcessingInformation(sourceData).FirstOrDefault();
			if (info == null)
			{
				return [];
			}
			var tblPrefix = info.DPI_ParentTableCode;
			var stagingEntityType = GetTypeFromTblPrefix(tblPrefix);
			var result = (IEnumerable<Tuple<object, DataProcessingInformation>>)this.InvokeGenericMethod(nameof(GetNextDataBatchCore), stagingEntityType, sourceData, metadataProvider, batchSize, startInfo);
			return result;
		}

		IEnumerable<Tuple<object, DataProcessingInformation>> GetNextDataBatchCore<T>(SourceData sourceData, IMetadataProvider metadataProvider, int? batchSize, DataProcessingInformation startInfo) where T : class
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			var infoQuery = GetQueuedDataProcessingInformation(sourceData);
			if (startInfo != null)
			{
				infoQuery = infoQuery.Where(x => x.DPI_ParentPk.HasValue && x.DPI_ParentPk.Value.CompareTo(startInfo.DPI_ParentPk.Value) > 0);
			}
			infoQuery = infoQuery.OrderBy(x => x.DPI_ParentPk);
			if (batchSize.HasValue)
			{
				infoQuery = infoQuery.Take(batchSize.Value);
			}
			var result = repo.Get<T>().TagWith(string.Format(CultureInfo.InvariantCulture, SqlCommentAdder.PrefixCommentTemplate, nameof(GetNextDataBatch)));
			if (metadataProvider.FilterData)
			{
				var constantValuesFilterExp = GetConstantValuesFilterExpression<T>(metadataProvider);
				result = result.Where(constantValuesFilterExp);
			}

			foreach (var includePath in DataProviderHelper.OrderInclude(IncludePaths(metadataProvider, typeof(T))))
			{
				result = result.NewInclude(includePath);
			}
			var infoData = infoQuery.ToArray();
			var pkPropertyInfo = typeof(T).GetPKPropertyInfo();
			var pkSelector = ExpressionHelper.GetPKExpression<T>().Compile();
			var data = result.AsSplitQuery().Where(ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(
				infoData.Select(y => y.DPI_ParentPk.Value).ToList(), pkPropertyInfo))
				.ToDictionary(pkSelector);

			foreach (var info in infoQuery.ToArray())
			{
				if (data.ContainsKey(info.DPI_ParentPk.Value))
				{
					var item = data[info.DPI_ParentPk.Value];
					item = metadataProvider.FilterData ? ConstantValuesFilters(item, metadataProvider) : item;
					if (item != null)
					{
						item.BuildNonPersistentObjects();
						yield return Tuple.Create((object)item, info);
					}
				}
			}
		}

		T ConstantValuesFilters<T>(T entity, IMetadataProvider metadataProvider) where T : class
		{
			Argument.NotNull(entity, nameof(entity));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			var result = entity;
			var entityType = typeof(T).Name;
			var relatedEntityTypeNames = metadataProvider.GetProperties(entityType)
				.Where(x => !x.StartsWith(typeof(T).GetTablePrefix(), StringComparison.OrdinalIgnoreCase));
			foreach (var relatedEntityTypeName in relatedEntityTypeNames)
			{
				var constantPropertyNamesAndValues = metadataProvider.GetConstantPropertyNamesAndValues(relatedEntityTypeName);
				var isRelatedEntityMandatory = metadataProvider.IsMandatory(typeof(T).Name, relatedEntityTypeName);
				if (isRelatedEntityMandatory || constantPropertyNamesAndValues.Length > 0)
				{
					var relatedEntityType = GetTypefromName(relatedEntityTypeName);
					if (relatedEntityType != null)
					{
						result = (T)GetType().InvokeStaticGenericMethod(nameof(ConstantValuesFilter), new[] { typeof(T), relatedEntityType }, entity, metadataProvider, isRelatedEntityMandatory);
						if (result == null)
						{
							return result;
						}
					}
				}
			}
			return result;
		}

		static T ConstantValuesFilter<T, TRelated>(T entity, IMetadataProvider metadataProvider, bool isMandatory) where T : class
		{
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			if (entity != null)
			{
				var relatedEntityProperty = typeof(T).GetProperties().FirstOrDefault(x => typeof(ICollection<TRelated>).IsAssignableFrom(x.PropertyType));
				if (relatedEntityProperty != null)
				{
					var oldValue = ((ICollection<TRelated>)relatedEntityProperty.GetValue(entity));
					var newValue = oldValue.Where(GetConstantValuesFilterExpression<TRelated>(metadataProvider).Compile()).ToList();
					if (newValue.Count > 0 || !isMandatory)
					{
						relatedEntityProperty.SetValue(entity, newValue);
						return entity;
					}
				}
			}
			return null;
		}

		static Expression<Func<T, bool>> GetConstantValuesFilterExpression<T>(IMetadataProvider metadataProvider)
		{
			Argument.NotNull(metadataProvider, nameof(metadataProvider));

			Expression result = null;
			var param = Expression.Parameter(typeof(T));
			var entityType = typeof(T).Name;
			var constantPropertyNamesAndValues = metadataProvider.GetConstantPropertyNamesAndValues(entityType);

			foreach (var propertyAndValue in constantPropertyNamesAndValues)
			{
				var propertyExp = Expression.Property(param, propertyAndValue.Item1);
				var propertyCondExp = Expression.Equal(propertyExp, Expression.Constant(propertyAndValue.Item2, typeof(string)));
				result = result != null ? Expression.And(result, propertyCondExp) : propertyCondExp;
			}
			return Expression.Lambda<Func<T, bool>>(result ?? Expression.Constant(true), param);
		}

		static IEnumerable<string> IncludePaths(IMetadataProvider metadataProvider, Type type)
		{
			var relatedEntityTypeNames = (metadataProvider.OriginalMetadataProvider ?? metadataProvider).GetProperties(type.Name).Where(x => !x.StartsWith(type.GetTablePrefix(), StringComparison.OrdinalIgnoreCase));
			foreach (var relatedEntityTypeName in relatedEntityTypeNames)
			{
				var relatedEntityType = GetTypefromName(relatedEntityTypeName);
				if (relatedEntityType != null)
				{
					var relatedEntityPropertyType = typeof(ICollection<>).MakeGenericType(relatedEntityType);
					var relatedEntityProperty = type.GetProperties().FirstOrDefault(x => relatedEntityPropertyType.IsAssignableFrom(x.PropertyType));
					if (relatedEntityProperty != null)
					{
						var childPaths = IncludePaths(metadataProvider, relatedEntityType);
						if (childPaths.Any())
						{
							foreach (var childPath in childPaths)
							{
								yield return relatedEntityProperty.Name + "." + childPath;
							}
						}
						else
						{
							yield return relatedEntityProperty.Name;
						}
					}
				}
			}
		}

		public IEnumerable<TRelated> GetRelatedEntities<T, TRelated>(T stagingObj)
			where T : class
			where TRelated : class
		{
			Argument.NotNull(stagingObj, nameof(stagingObj));
			IEnumerable<TRelated> result = null;

			var relatedEntityPropertyType = typeof(ICollection<>).MakeGenericType(typeof(TRelated));
			var relatedEntityProperty = typeof(T).GetProperties().FirstOrDefault(x => relatedEntityPropertyType.IsAssignableFrom(x.PropertyType));
			if (relatedEntityProperty != null)
			{
				result = ((IEnumerable)relatedEntityProperty.GetValue(stagingObj))?.Cast<TRelated>();
			}
			return result ?? Enumerable.Empty<TRelated>();
		}

		public IEnumerable<Type> GetRelatedEntityTypes(string entityName, IMetadataProvider metadataProvider)
		{
			Argument.NotNullOrEmpty(entityName, nameof(entityName));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			foreach (var propertyName in metadataProvider.GetProperties(entityName).Where(x => x.IndexOf("_", StringComparison.Ordinal) < 0))
			{
				yield return typeof(RefCusTariff).Assembly.DefinedTypes
					.FirstOrDefault(x => x.Name == propertyName);
			}
		}

		public string GetFKPropertyName(Type entityType, Type parentEntityType)
		{
			Argument.NotNull(entityType, nameof(entityType));
			Argument.NotNull(parentEntityType, nameof(parentEntityType));
			var entityTblPrefix = entityType.GetTablePrefix();
			var parentTblPrefix = parentEntityType.GetTablePrefix();
			return entityType.GetProperties().FirstOrDefault(x => (x.PropertyType == typeof(Guid) || x.PropertyType == typeof(Guid?))
				&& (x.Name.StartsWith(entityTblPrefix + "_" + parentTblPrefix, StringComparison.OrdinalIgnoreCase))
				|| x.Name.StartsWith(entityTblPrefix + "_ParentPK", StringComparison.OrdinalIgnoreCase))?.Name;
		}

		public Type GetTypeFromTblPrefix(string tablePrefix)
		{
			Argument.NotNullOrEmpty(tablePrefix, nameof(tablePrefix));
			return typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x =>
				x.GetProperty(tablePrefix + "_PK") != null);
		}

		public Type GetTypeFromTypeName(string typeName)
		{
			Argument.NotNullOrEmpty(typeName, nameof(typeName));
			return typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x =>
				x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
		}

		public async Task<int> UpdateSDA_NotProcessedUntil(Guid sdaPk, DateTime dateTime)
		{
			return await repo.ExecuteSqlCommandAsync($@"
UPDATE {nameof(SourceData)}
SET {nameof(SourceData.SDA_NotProcessedUntil)} = @p0
WHERE {nameof(SourceData.SDA_PK)} = '{sdaPk}'",
			new SqlParameter("@p0", System.Data.SqlDbType.DateTime2) { Value = dateTime });
		}

		public async Task<int> SaveChangesAsync()
		{
			return await repo.SaveChangesAsync();
		}

		static Type GetTypefromName(string entityName)
		{
			return typeof(RefCusTariff).Assembly.DefinedTypes?.FirstOrDefault(x =>
				x.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
		}

		public async Task SaveUpdateResults(SourceData sourceData, IEnumerable<SafeObjectUpdaterResult> updateResults, IDbTransaction tran)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(updateResults, nameof(updateResults));
			var processingResults = updateResults.Where(x => x.Action != ResultAction.Expire)
				.Select(x => CreateProcessingResult(sourceData, x, DataProcessingStatus.QUE.ToString())).ToArray();
			await repo.BulkInsertAsync(processingResults, transaction: tran);
			await SetDataProcessingResultExpireStatus(sourceData, updateResults.Where(x => x.Action == ResultAction.Expire));
		}

		public void SaveErrorUpdateResult(SourceData sourceData, SafeObjectUpdaterResult updateResult)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(updateResult, nameof(updateResult));
			var dpr = CreateProcessingResult(sourceData, updateResult, DataProcessingStatus.ERR.ToString());
			repo.Add(dpr);
		}

		public async Task<int> UpdateDescendentDPRWithExpiredAncestor(string tblPrefix, SourceData sourceData)
		{
			Argument.NotNullOrEmpty(tblPrefix, nameof(tblPrefix));
			Argument.NotNull(sourceData, nameof(sourceData));
			const string sql = $@"
UPDATE r1 SET r1.{nameof(DataProcessingResult.DPR_Status)} = r2.{nameof(DataProcessingResult.DPR_Status)},
	r1.{nameof(DataProcessingResult.DPR_ExpirationTime)} = r2.{nameof(DataProcessingResult.DPR_ExpirationTime)}
FROM {nameof(DataProcessingResult)} r1
JOIN {nameof(DataProcessingResult)} r2 ON r1.{nameof(DataProcessingResult.DPR_ExpirableAncestorPK)} = r2.{nameof(DataProcessingResult.DPR_ParentPK)}
WHERE r1.{nameof(DataProcessingResult.DPR_SubSource)} = @subSource
AND r2.{nameof(DataProcessingResult.DPR_PublicationTime)} < @sourceTime
AND r1.{nameof(DataProcessingResult.DPR_ParentTableCode)} = @tblPrefix";
			return await repo.ExecuteSqlCommandAsync(SqlCommentAdder.Create().WithSql(sql).WithId(nameof(UpdateDescendentDPRWithExpiredAncestor)).Build().ToString()
, new SqlParameter("subSource", sourceData.SDA_SubSource)
, new SqlParameter("sourceTime", System.Data.SqlDbType.DateTime2) { Value = sourceData.SDA_SourceTime }
, new SqlParameter("@tblPrefix", tblPrefix));
		}

		public async Task<int> CleanUpOldUpdateResultsAsync(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			var sql = FormattableString.Invariant($@"
WITH data
AS (
SELECT {nameof(DataProcessingResult.DPR_PK)}, ItemNumber = ROW_NUMBER()
	OVER (PARTITION BY {nameof(DataProcessingResult.DPR_ParentPK)}, {nameof(DataProcessingResult.DPR_SubSource)}
	ORDER BY {nameof(DataProcessingResult.DPR_PublicationTime)} DESC)
	FROM
	{nameof(DataProcessingResult)}
	WHERE {nameof(DataProcessingResult.DPR_SubSource)} = '{sourceData.SDA_SubSource}'
)
DELETE FROM data WHERE ItemNumber > 1
");
			return await repo.ExecuteSqlCommandAsync(SqlCommentAdder.Create().WithSql(sql).WithId(nameof(CleanUpOldUpdateResultsAsync)).Build().ToString());
		}

		static DataProcessingResult CreateProcessingResult(SourceData sourceData, SafeObjectUpdaterResult updateResult, string status)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(updateResult, nameof(updateResult));

			var sequentialGuidInstance = SequentialSqlGuidGenerator.Instance;

			return new DataProcessingResult
			{
				DPR_ParentPK = updateResult.ParentPK,
				DPR_ParentTableCode = updateResult.ParentCode,
				DPR_PK = sequentialGuidInstance.NewGuid(),
				DPR_SubSource = sourceData.SDA_SubSource,
				DPR_PublicationTime = sourceData.SDA_SourceTime.Value,
				DPR_ExpirableAncestorPK = updateResult.ExpirableAncestorPK,
				DPR_Status = status,
				DPR_DatasetPK = updateResult.DatasetPK
			};
		}

		async Task SetDataProcessingResultExpireStatus(SourceData sourceData, IEnumerable<SafeObjectUpdaterResult> updateResults)
		{
			if (updateResults.Any())
			{
				await repo.ExecuteSqlCommandAsync(FormattableString.Invariant($@"
Update {nameof(DataProcessingResult)} set {nameof(DataProcessingResult.DPR_Status)} = '{DataProcessingStatus.PRS}'
Where {nameof(DataProcessingResult.DPR_SubSource)} = '{sourceData.SDA_SubSource}'
AND {nameof(DataProcessingResult.DPR_ParentPK)} in ({string.Join(",", updateResults.Select(x => $"'{x.ParentPK}'"))})
"));
			}
		}

		public void Remove<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			repo.Remove(data);
		}

		public void Update<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			repo.Update(data);
		}

		public IQueryable<DataProcessingResult> GetProcessingResults(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			return repo.Get<DataProcessingResult>().Where(x => x.DPR_SubSource == sourceData.SDA_SubSource);
		}

		public void Dispose()
		{
			repo.Dispose();
		}

		public IEnumerable<string> GetAutoSchemas(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			var info = GetDataProcessingInformation(sourceData).FirstOrDefault();
			if (info != null)
			{
				return repo.Get<AutoSchema>().Where(x => x.AS_TableCode == info.DPI_ParentTableCode)
					.Select(x => x.AS_Schema).ToArray();
			}
			return Enumerable.Empty<string>();
		}

		public void MarkSourceDataStatus(SourceData sourceData, bool autoExpirationResult, bool cloneResult)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			if (GetDataProcessingInformation(sourceData).Any(x => x.DPI_Status == DataProcessingStatus.QUE.ToString()))
			{
				Console.Error.WriteLine("SDA_Status set to ERR as there is QUE DPI record found when merging is finished");
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
				return;
			}

			if (GetDataProcessingInformation(sourceData).Any(x => x.DPI_Status == DataProcessingStatus.ERR.ToString()))
			{
				Console.Error.WriteLine("SDA_Status set to ERR as there is ERR DPI record found");
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
				return;
			}

			if (!autoExpirationResult)
			{
				Console.Error.WriteLine("SDA_Status set to ERR as auto-expiration failed");
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
				return;
			}

			if (!cloneResult)
			{
				Console.Error.WriteLine("SDA_Status set to ERR as clone failed");
				sourceData.SDA_Status = StatusProvider.GetERRStatus();
				return;
			}

			sourceData.SDA_Status = StatusProvider.GetMERStatus();
		}

		public async Task<int> MarkDataProcessingInformationAsIgnored(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			return await repo.ExecuteSqlCommandAsync($@"
UPDATE {nameof(DataProcessingInformation)}
SET {nameof(DataProcessingInformation.DPI_Status)} = '{DataProcessingStatus.IGR.ToString()}'
WHERE {nameof(DataProcessingInformation.DPI_Status)} = '{DataProcessingStatus.QUE.ToString()}'
AND {nameof(DataProcessingInformation.DPI_SourceId)} = '{sourceData.SDA_PK}'
");
		}

		public async Task UpdateAllExpiredDataProcessingResultAsync(SourceData sourceData)
		{
			var sql = FormattableString.Invariant($@"
UPDATE {nameof(DataProcessingResult)}
SET {nameof(DataProcessingResult.DPR_Status)} = '{DataProcessingStatus.PRS}',
	{nameof(DataProcessingResult.DPR_ExpirationTime)} = ISNULL({nameof(DataProcessingResult.DPR_ExpirationTime)}, @sourceTime)
WHERE {nameof(DataProcessingResult.DPR_SubSource)} = @subSource
	AND {nameof(DataProcessingResult.DPR_Status)} = '{DataProcessingStatus.QUE}'
	AND {nameof(DataProcessingResult.DPR_PublicationTime)} < @sourceTime");
			await repo.ExecuteSqlCommandAsync(SqlCommentAdder.Create().WithSql(sql).WithId(nameof(UpdateAllExpiredDataProcessingResultAsync)).Build().ToString()
, new SqlParameter("subSource", sourceData.SDA_SubSource)
, new SqlParameter("sourceTime", System.Data.SqlDbType.DateTime2) { Value = sourceData.SDA_SourceTime });
		}

		public async Task UpdateEmptyDPIParentPKStatusToErrAsync(SourceData sourceData)
		{
			await repo.ExecuteSqlCommandAsync(FormattableString.Invariant($@"
UPDATE {nameof(DataProcessingInformation)} SET {nameof(DataProcessingInformation.DPI_Status)} = '{DataProcessingStatus.ERR}'
WHERE {nameof(DataProcessingInformation.DPI_SourceId)} = '{sourceData.SDA_PK}'
AND {nameof(DataProcessingInformation.DPI_Status)} = '{DataProcessingStatus.QUE}'
AND {nameof(DataProcessingInformation.DPI_ParentPk)} IS NULL
"));
		}

		public IQueryable<DataProcessingClone> GetQueuedProcessingClone(Guid sourceID, string tblPrefix, Guid startGuid)
		{
			var queuedClones = repo.Get<DataProcessingClone>().Where(x => x.DPC_SourceId == sourceID && x.DPC_TableCode == tblPrefix && x.DPC_Status == DataProcessingStatus.QUE.ToString()
			&& x.DPC_PK.CompareTo(startGuid) > 0).OrderBy(x => x.DPC_PK);
			return queuedClones;
		}

		public void UpdateDataProcessingCloneStatus(IEnumerable<DataProcessingClone> dataProcessingClones, DataProcessingStatus status)
		{
			foreach (var dataProcessingClone in dataProcessingClones)
			{
				dataProcessingClone.DPC_Status = status.ToString();
				repo.Update(dataProcessingClone);
			}
		}

		public async Task UpdateDataProcessingResultStatusAndExpirationTime(DataProcessingStatus status, DateTime expirationTime,
			IEnumerable<Guid> dataProcessingResultParentPks, SourceData sourceData)
		{
			if (!dataProcessingResultParentPks.Any())
			{
				return;
			}

			var parameters = new List<SqlParameter>
			{
				new("@status", status.ToString()),
				new("@expirationTime", SqlDbType.DateTime2) { Value = expirationTime },
				new("@subSource", sourceData.SDA_SubSource)
			};
			var pkParameters = dataProcessingResultParentPks
				.Select((pk, index) => new SqlParameter($"@pk{index}", pk));
			parameters.AddRange(pkParameters);

			var pkPlaceholders = string.Join(",", dataProcessingResultParentPks.Select((_, index) => $"@pk{index}"));

			var sql = FormattableString.Invariant($@"
UPDATE {nameof(DataProcessingResult)}
SET {nameof(DataProcessingResult.DPR_Status)} = @status,
    {nameof(DataProcessingResult.DPR_ExpirationTime)} = ISNULL({nameof(DataProcessingResult.DPR_ExpirationTime)}, @expirationTime)
WHERE {nameof(DataProcessingResult.DPR_SubSource)} = @subSource
AND {nameof(DataProcessingResult.DPR_ParentPK)} IN ({pkPlaceholders})");

			await repo.ExecuteSqlCommandAsync(sql, [.. parameters]);
		}

		public async Task UpdateDataProcessingResultExpirationTime(DateTime expirationTime, IEnumerable<Guid> dataProcessingResultParentPks, SourceData sourceData)
		{
			if (!dataProcessingResultParentPks.Any())
			{
				return;
			}

			var parameters = new List<SqlParameter>
			{
				new("@expirationTime", SqlDbType.DateTime2) { Value = expirationTime },
				new("@subSource", sourceData.SDA_SubSource)
			};
			var pkParameters = dataProcessingResultParentPks
				.Select((pk, index) => new SqlParameter($"@pk{index}", pk));
			parameters.AddRange(pkParameters);

			var pkPlaceholders = string.Join(",", dataProcessingResultParentPks.Select((_, index) => $"@pk{index}"));

			var updateSql = FormattableString.Invariant($@"
UPDATE {nameof(DataProcessingResult)}
SET {nameof(DataProcessingResult.DPR_ExpirationTime)} = @expirationTime
WHERE {nameof(DataProcessingResult.DPR_SubSource)} = @subSource
AND {nameof(DataProcessingResult.DPR_ParentPK)} IN ({pkPlaceholders})
AND {nameof(DataProcessingResult.DPR_ExpirationTime)} IS NULL");

			await repo.ExecuteSqlCommandAsync(updateSql, [.. parameters]);
		}

		public async Task CreateDataProcessingCloneRecords(Guid sourceID, IEnumerable<SafeObjectUpdaterResult> updateResults, IDbTransaction tran)
		{
			Argument.GuidIsNotEmpty(sourceID, nameof(sourceID));
			Argument.NotNull(updateResults, nameof(updateResults));

			var dpcRecords = updateResults.Where(x => x.NewRecordForCloneActionPK.HasValue && x.ParentCode == "ZZ1").Select(x => CreateProcessingClone(sourceID, x));
			await repo.BulkInsertAsync(dpcRecords, transaction: tran);
		}

		static DataProcessingClone CreateProcessingClone(Guid sourceID, SafeObjectUpdaterResult updateResult)
		{
			return new DataProcessingClone
			{
				DPC_PK = Guid.NewGuid(),
				DPC_SourceId = sourceID,
				DPC_TableCode = updateResult.ParentCode,
				DPC_Status = DataProcessingStatus.QUE.ToString(),
				DPC_ExpiredRecordPK = updateResult.ParentPK,
				DPC_NewRecordPK = updateResult.NewRecordForCloneActionPK.Value
			};
		}

		public async Task<int> CreateDPRForCloneResults(IEnumerable<CloneProcessResult> cloneProcessResults)
		{
			Argument.NotNull(cloneProcessResults, nameof(cloneProcessResults));

			var cloneResults = cloneProcessResults.Select(x => $"('{x.OriginalRecordPK}', '{x.ClonedRecordPK}', '{x.DataSetPK}', '{x.ClonedRecordExpirableAncestorPK}')");
			var cloneResultsValues = string.Join(",\r\n", cloneResults);
			var createDPRSql = $@"IF OBJECT_ID('tempdb..#CloneProcessResults') IS NOT NULL
    DROP TABLE #CloneProcessResults;
CREATE TABLE #CloneProcessResults (OriginalRecordPK UNIQUEIDENTIFIER, NewRecordPK UNIQUEIDENTIFIER, DataSetPK UNIQUEIDENTIFIER, ExpirableAncestorPK UNIQUEIDENTIFIER)
INSERT #CloneProcessResults VALUES {cloneResultsValues};

INSERT INTO DataProcessingResult (DPR_PK, DPR_SubSource, DPR_PublicationTime, DPR_ParentTableCode, DPR_ParentPK, DPR_ExpirableAncestorPK, DPR_DatasetPK, DPR_Status, DPR_ExpirationTime)
SELECT NEWID(), DPR_SubSource, DPR_PublicationTime, DPR_ParentTableCode, NewRecordPK, ExpirableAncestorPK, DataSetPK, DPR_Status, DPR_ExpirationTime FROM DataProcessingResult
JOIN #CloneProcessResults ON DPR_ParentPK = OriginalRecordPK";

			var result = await repo.ExecuteSqlCommandAsync(createDPRSql);
			return result - cloneResults.Count();
		}
	}
}
