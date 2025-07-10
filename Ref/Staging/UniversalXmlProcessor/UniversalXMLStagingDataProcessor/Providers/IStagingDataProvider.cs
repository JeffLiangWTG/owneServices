using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.EntityFrameworkCore.Storage;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IStagingDataProvider : IDisposable
	{
		bool IsAutoExpiredEnabled(SourceData sourceData);
		IQueryable<SourceData> GetSourceData();
		IQueryable<SourceData> GetDependencySourceData(Dependency dependency);
		SourceData GetSingleSourceData(Guid pk);
		IQueryable<DataProcessingInformation> GetDataProcessingInformation(SourceData sourceData);

		IEnumerable<Tuple<object, DataProcessingInformation>> GetNextDataBatch(SourceData sourceData, IMetadataProvider metadataProvider, int? batchSize, DataProcessingInformation startInfo);

		IEnumerable<TRelated> GetRelatedEntities<T, TRelated>(T stagingObj)
			where T : class
			where TRelated : class;
		IEnumerable<Type> GetRelatedEntityTypes(string entityName, IMetadataProvider metadataProvider);
		string GetFKPropertyName(Type entityType, Type parentEntityType);
		Type GetTypeFromTblPrefix(string tablePrefix);
		Type GetTypeFromTypeName(string typeName);
		Task<int> SaveChangesAsync();
		void Remove<T>(T data) where T : class;
		void Update<T>(T data) where T : class;
		IQueryable<DataProcessingResult> GetProcessingResults(SourceData sourceData);
		Task SaveUpdateResults(SourceData sourceData, IEnumerable<SafeObjectUpdaterResult> updateResults, IDbTransaction tran);
		Task<int> CleanUpOldUpdateResultsAsync(SourceData sourceData);
		IEnumerable<string> GetAutoSchemas(SourceData sourceData);
		void MarkSourceDataStatus(SourceData sourceData, bool autoExpirationResult, bool cloneResult);
		Task<int> MarkDataProcessingInformationAsIgnored(SourceData sourceData);
		Task<int> UpdateDescendentDPRWithExpiredAncestor(string tblPrefix, SourceData sourceData);
		IDbContextTransaction BeginTransaction();
		void Commit(object transaction);
		Task UpdateAllExpiredDataProcessingResultAsync(SourceData sourceData);
		Task UpdateEmptyDPIParentPKStatusToErrAsync(SourceData sourceData);
		Task<int> UpdateSDA_NotProcessedUntil(Guid sdaPk, DateTime dateTime);
		void SaveErrorUpdateResult(SourceData sourceData, SafeObjectUpdaterResult updateResult);
		IQueryable<DataProcessingClone> GetQueuedProcessingClone(Guid sourceID, string tblPrefix, Guid startGuid);
		Task<int> CreateDPRForCloneResults(IEnumerable<CloneProcessResult> cloneProcessResults);
		Task CreateDataProcessingCloneRecords(Guid sourceID, IEnumerable<SafeObjectUpdaterResult> updateResults, IDbTransaction tran);
		void UpdateDataProcessingCloneStatus(IEnumerable<DataProcessingClone> dataProcessingClones, DataProcessingStatus status);
		Task UpdateDataProcessingResultStatusAndExpirationTime(DataProcessingStatus status, DateTime expirationTime, IEnumerable<Guid> dataProcessingResultParentPks, SourceData sourceData);
		Task UpdateDataProcessingResultExpirationTime(DateTime expirationTime, IEnumerable<Guid> dataProcessingResultParentPks, SourceData sourceData);
	}
}
