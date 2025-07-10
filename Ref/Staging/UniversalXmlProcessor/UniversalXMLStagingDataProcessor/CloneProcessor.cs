using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class CloneProcessor
	{
		readonly IServiceFactory serviceFactory;
		readonly ICacheProvider cacheProvider;
		readonly SourceData sourceData;
		readonly int maximumRetries;
		readonly int maxNoOfBatches;
		ISafeDataProvider safeDataProvider;
		string _tblPrefix;

		public CloneProcessor(IServiceFactory serviceFactory, ICacheProvider cacheProvider, SourceData sourceData, int maximumRetries, int maxNoOfBatches)
		{
			this.serviceFactory = serviceFactory;
			this.cacheProvider = cacheProvider;
			this.sourceData = sourceData;
			this.maximumRetries = maximumRetries;
			this.maxNoOfBatches = maxNoOfBatches;
		}

		public async Task<bool> Process(int batchSize)
		{
			safeDataProvider = serviceFactory.GetSafeDataProvider(cacheProvider);
			using (var stagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR))
			{
				_tblPrefix = stagingDataProvider.GetDataProcessingInformation(sourceData).FirstOrDefault()?.DPI_ParentTableCode;
				var parentTableType = safeDataProvider.GetTypeFromTblPrefix(_tblPrefix);
				if (parentTableType.Name != nameof(RefCusTariff) || CloneBlackList.Contains(sourceData.SDA_SubSource))
				{
					return true;
				}

				Guid startGuid = Guid.Empty;
				var retryProcessedClonesWithExceptionList = new List<(DataProcessingClone, CloneProcessObject)>();
				while (true)
				{
					var dataProcessingCloneWithCloneProcessObjectList = new List<(DataProcessingClone, CloneProcessObject)>();
					var processingCloneRecords = stagingDataProvider.GetQueuedProcessingClone(sourceData.SDA_PK, _tblPrefix, startGuid).Take(batchSize * maxNoOfBatches).AsEnumerable();
					if (processingCloneRecords == null || !processingCloneRecords.Any())
					{
						break;
					}

					startGuid = processingCloneRecords.Last().DPC_PK;
					var expiredPKs = processingCloneRecords.Select(x => x.DPC_ExpiredRecordPK);
					var dataProcessingResults = stagingDataProvider.GetProcessingResults(sourceData).Where(x => expiredPKs.Contains(x.DPR_ExpirableAncestorPK.Value)).AsEnumerable();
					foreach (var processingCloneRecord in processingCloneRecords)
					{
						var exceptionList = dataProcessingResults.Where(x => parentTableType.GetCollectionNavigationPropertyInfo(safeDataProvider.GetTypeFromTblPrefix(x.DPR_ParentTableCode)) != null)
							.GroupBy(x => x.DPR_ParentTableCode, x => x.DPR_ParentPK).ToDictionary(x => x.Key, x => x.ToList());

						var cloneProcessObject = new CloneProcessObject { DataProcessingClonePK = processingCloneRecord.DPC_PK, ExpiredRecordPk = processingCloneRecord.DPC_ExpiredRecordPK, NewRecordPk = processingCloneRecord.DPC_NewRecordPK, ExceptionListForCloning = exceptionList };
						dataProcessingCloneWithCloneProcessObjectList.Add((processingCloneRecord, cloneProcessObject));
					}

					var batchResult = await ProcessBatch(dataProcessingCloneWithCloneProcessObjectList, batchSize);
					if (batchResult.Count > 0)
					{
						retryProcessedClonesWithExceptionList.AddRange(batchResult);
					}
				}
				var result = await ProcessBatchWithRetry(stagingDataProvider, retryProcessedClonesWithExceptionList, MergeProcessorExtension.CalculateRetryMergeSize(batchSize, retryProcessedClonesWithExceptionList.Count));
				return result;
			}
		}

		async Task<bool> ProcessBatchWithRetry(IStagingDataProvider stagingDataProvider, List<(DataProcessingClone, CloneProcessObject)> dataProcessingClones, int batchSize)
		{
			Argument.NotNull(dataProcessingClones, nameof(dataProcessingClones));
			Argument.GreaterThan(batchSize, 0, nameof(batchSize));

			var failedProcessedRecords = await MergeProcessorExtension.Retry(dataProcessingClones, ProcessBatch, x => x, 1, maximumRetries, batchSize, sourceData.SDA_PK);
			if (failedProcessedRecords.Count > 0)
			{
				stagingDataProvider.UpdateDataProcessingCloneStatus(failedProcessedRecords.Select(x => x.Item1), DataProcessingStatus.ERR);
			}
			return failedProcessedRecords.Count <= 0;
		}

		async Task<List<(DataProcessingClone, CloneProcessObject)>> ProcessBatch(IEnumerable<(DataProcessingClone, CloneProcessObject)> records, int batchSize)
		{
			var processingRecords = new List<(DataProcessingClone, CloneProcessObject)>();
			var failedProcessedRecords = new List<(DataProcessingClone, CloneProcessObject)>();
			int idx = 0;
			while (idx < records.Count())
			{
				processingRecords.Add(records.ElementAt(idx));
				if (processingRecords.Count >= batchSize * maxNoOfBatches || idx == records.Count() - 1)
				{
					var recordGroups = processingRecords.SplitProcessingEntitiesIntoArray(batchSize, maxNoOfBatches);
					var failedProcessResults = await Task.WhenAll(recordGroups.Select(g => Task.Run(async () => await ProcessSingleBatch(g))).ToArray());
					failedProcessedRecords.AddRange(failedProcessResults.SelectMany(x => x));
					processingRecords = new List<(DataProcessingClone, CloneProcessObject)>();
				}
				idx++;
			}
			return failedProcessedRecords;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		async Task<(DataProcessingClone, CloneProcessObject)[]> ProcessSingleBatch((DataProcessingClone, CloneProcessObject)[] records)
		{
			var failedRecords = new (DataProcessingClone, CloneProcessObject)[0];
			try
			{
				using (var batchStagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR))
				using (var transaction = batchStagingDataProvider.BeginTransaction())
				{
					var cloneProcessResults = await safeDataProvider.CloneExistingRecordChildrenIntoNewRecord(_tblPrefix, records.Select(x => x.Item2));
					if (cloneProcessResults.Any())
					{
						Console.WriteLine($"Creating DPR records for cloned data ...");
						var savedDPRCount = await batchStagingDataProvider.CreateDPRForCloneResults(cloneProcessResults);
						Console.WriteLine($"Saved DPR records: {savedDPRCount}");
						Console.WriteLine($"Update DataProcessingClone status to: {DataProcessingStatus.PRS}");
						batchStagingDataProvider.UpdateDataProcessingCloneStatus(records.Select(x => x.Item1), DataProcessingStatus.PRS);
						await batchStagingDataProvider.SaveChangesAsync();

						foreach (var cloneResult in cloneProcessResults)
						{
							var clonedRecordType = cacheProvider.TableNameAndTypeCache.GetOrAdd(cloneResult.ClonedRecordTypeName, v => typeof(Safe.RefCusTariff).GetTypeFromBaseType(cloneResult.ClonedRecordTypeName));
							var clonedRecord = ConvertToSafeObject(clonedRecordType, cloneResult.ClonedRecord);
							if (clonedRecord != null)
							{
								safeDataProvider.Add(clonedRecord, clonedRecordType);
							}
						}
						Console.WriteLine($"Saving cloned data ...");
						await safeDataProvider.SaveChangesAsync();
						Console.WriteLine($"Saved cloned records: {cloneProcessResults.Count()}");
						batchStagingDataProvider.Commit(transaction);
					}
				}
			}
			catch (Exception ex)
			{
				failedRecords = records;
				Console.WriteLine($"Save cloned data failed: {records.Length}");
				Console.Error.WriteLine(ex);
			}
			return failedRecords;
		}

		static object ConvertToSafeObject(Type clonedRecordType, object clonedRecord)
		{
			if (clonedRecord is JObject clonedObject)
			{
				var clonedSafeRecord = clonedObject.ToObject(clonedRecordType);
				foreach (var property in clonedRecordType.GetProperties())
				{
					var propertyType = property.PropertyType;
					var propertyValue = property.GetValue(clonedSafeRecord);
					if (propertyValue == null)
					{
						continue;
					}
					if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
					{
						var value = ((DateTime?)propertyValue).Value.ToUTCDateTimeOffset().DateTime;
						property.SetValue(clonedSafeRecord, value);
					}
					else if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
					{
						var value = ((DateTimeOffset?)propertyValue).Value.ToUTCDateTimeOffset();
						property.SetValue(clonedSafeRecord, value);
					}
				}

				return clonedSafeRecord;
			}
			return null;
		}
	}
}
