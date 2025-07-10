using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.EntityFrameworkCore.Storage;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class MergeProcessor
	{
		public MergeProcessor(
			IStagingDataProvider readonlyStagingDataProvider,
			IServiceFactory serviceFactory,
			IMetadataProvider metadataProvider,
			IOdataUriProvider odataUriProvider,
			SourceData sourceData,
			ICacheProvider cacheProvider,
			int maximumRetries,
			int maxNoOfBatches,
			UpdateType updateType)
		{
			Argument.NotNull(serviceFactory, nameof(serviceFactory));
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(cacheProvider, nameof(cacheProvider));
			Argument.NotNull(readonlyStagingDataProvider, nameof(readonlyStagingDataProvider));

			this.serviceFactory = serviceFactory;
			this.sourceData = sourceData;
			this.maximumRetries = maximumRetries;
			this.maxNoOfBatches = maxNoOfBatches;
			this.metadataProvider = metadataProvider;
			this.odataUriProvider = odataUriProvider;
			this.cacheProvider = cacheProvider;
			this.updateType = updateType;
			this.readonlyStagingDataProvider = readonlyStagingDataProvider;
		}

		readonly IServiceFactory serviceFactory;
		readonly IStagingDataProvider readonlyStagingDataProvider;
		readonly IMetadataProvider metadataProvider;
		readonly IOdataUriProvider odataUriProvider;
		readonly SourceData sourceData;
		readonly int maximumRetries;
		readonly int maxNoOfBatches;
		readonly ICacheProvider cacheProvider;
		readonly UpdateType updateType;

		public async Task Process(int mergeSize)
		{
			Argument.GreaterThan(mergeSize, 0, nameof(mergeSize));
			Argument.InRangeWithBoundIncluded(mergeSize, 1, MergeProcessorExtension.CalculateMaxInitialMergeSize(maximumRetries), nameof(mergeSize));

			var retryProcessedEntities = new List<(object, DataProcessingInformation)>();
			DataProcessingInformation lastInfo = null;
			Console.WriteLine("Processing Source Data");

			await CheckAndSetAsErrorEmptyDPIParentPK();

			while (true)
			{
				var entities = readonlyStagingDataProvider.GetNextDataBatch(sourceData, metadataProvider, mergeSize * maxNoOfBatches, lastInfo).Select(x => (x.Item1, x.Item2)).ToArray();
				if (entities.Length == 0)
				{
					break;
				}
				lastInfo = entities[entities.Length - 1].Item2;
				var batchResult = await ProcessBatch(entities, mergeSize);
				retryProcessedEntities.AddRange(batchResult);
			}

			if (!Constants.EnableCleanDPRPerBatch)
			{
				using var stagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR);
				await stagingDataProvider.CleanUpOldUpdateResultsAsync(sourceData);
			}

			await ProcessBatchWithRetry(retryProcessedEntities.ToArray(), MergeProcessorExtension.CalculateRetryMergeSize(mergeSize, retryProcessedEntities.Count));
		}

		async Task CheckAndSetAsErrorEmptyDPIParentPK()
		{
			using var stagingDataProvider = serviceFactory.GetStagingDataProvider();
			await stagingDataProvider.UpdateEmptyDPIParentPKStatusToErrAsync(sourceData);
		}

		async Task ProcessBatchWithRetry(IEnumerable<(object, DataProcessingInformation)> entities, int mergeSize)
		{
			Argument.NotNull(entities, nameof(entities));
			Argument.GreaterThan(mergeSize, 0, nameof(mergeSize));

			var result = await MergeProcessorExtension.Retry(entities, ProcessBatch, x => x, 1, maximumRetries, mergeSize, sourceData.SDA_PK);
			using var stagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR);
			if (result.Any())
			{
				UpdateStatuses(stagingDataProvider, result.Select(x => x.Item2), DataProcessingStatus.ERR);
				await stagingDataProvider.SaveChangesAsync();
				await PopulateDatasetPKForErrorRecords(result);
			}
			if (!Constants.EnableCleanDPRPerBatch || result.Any())
			{
				await stagingDataProvider.CleanUpOldUpdateResultsAsync(sourceData);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		async Task PopulateDatasetPKForErrorRecords(IEnumerable<(object, DataProcessingInformation)> failedProcessedEntities)
		{
			Argument.NotNull(failedProcessedEntities, nameof(failedProcessedEntities));
			using var batchStagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR);
			using var trans = batchStagingDataProvider.BeginTransaction();
			foreach (var (stagingObj, dpi) in failedProcessedEntities)
			{
				try
				{
					var entityType = stagingObj.GetEntityType();
					var safeObjPKs = GetMatchedSafeObjPKs(stagingObj);
					if (safeObjPKs != null && safeObjPKs.Any())
					{
						foreach (var safeObjPK in safeObjPKs)
						{
							var updateResult = new SafeObjectUpdaterResult
							{
								ParentPK = safeObjPK,
								ParentCode = entityType.GetTablePrefix(),
								Action = ResultAction.Update,
								ExpirableAncestorPK = null,
								NewRecordForCloneActionPK = null,
								DatasetPK = safeObjPK
							};
							Console.WriteLine($"Final trial saving error header update result: {safeObjPK} ...");
							batchStagingDataProvider.SaveErrorUpdateResult(sourceData, updateResult);
						}

						dpi.DPI_HasDPRRecordWhenError = true;
						batchStagingDataProvider.Update(dpi);
					}
					else
					{
						Console.Error.WriteLine("Cannot find a match");
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("Exception happens while saving error header update result for the final trial:");
					Console.Error.WriteLine(ex);
				}
			}
			await batchStagingDataProvider.SaveChangesAsync();
			batchStagingDataProvider.Commit(trans);
		}

		static void UpdateStatuses(IStagingDataProvider stagingDataProvider, IEnumerable<DataProcessingInformation> infos, DataProcessingStatus status)
		{
			Argument.NotNull(infos, nameof(infos));
			Argument.NotNull(stagingDataProvider, nameof(stagingDataProvider));

			foreach (var info in infos)
			{
				info.DPI_Status = status.ToString();
				stagingDataProvider.Update(info);
			}
		}

		async Task<IEnumerable<(object, DataProcessingInformation)>> ProcessBatch(IEnumerable<(object, DataProcessingInformation)> entities, int mergeSize)
		{
			int idx = 0;
			var processingEntities = new List<(object, DataProcessingInformation)>();
			var failedProcessedEntities = new List<(object, DataProcessingInformation)>();
			while (idx < entities.Count())
			{
				processingEntities.Add(entities.ElementAt(idx));
				if (processingEntities.Count >= mergeSize * maxNoOfBatches || idx == entities.Count() - 1)
				{
					var entityGroups = processingEntities.SplitProcessingEntitiesIntoArray(mergeSize, maxNoOfBatches);

					var failedProcessResults = await Task.WhenAll(entityGroups.Select(g => Task.Run(async () => await ProcessSingleBatch(g, mergeSize == 1))).ToArray());
					Console.WriteLine("Saving merged results ...");
					if (Constants.EnableCleanDPRPerBatch)
					{
						using var stagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR);
						await stagingDataProvider.CleanUpOldUpdateResultsAsync(sourceData);
					}
					processingEntities = new List<(object, DataProcessingInformation)>();
					foreach (var failedProcessResult in failedProcessResults)
					{
						failedProcessedEntities.AddRange(failedProcessResult);
					}
				}
				idx++;
			}
			return failedProcessedEntities;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		async Task<IEnumerable<(object, DataProcessingInformation)>> ProcessSingleBatch((object, DataProcessingInformation)[] entities, bool isFinalTrial)
		{
			var updateResults = new SafeObjectUpdaterResult[0];
			var stagingObj = entities.FirstOrDefault().Item1;
			var entityType = stagingObj.GetEntityType();
			var stageObjPK = stagingObj.GetPKValue();

			try
			{
				using var batchStagingDataProvider = serviceFactory.GetStagingDataProvider(timeOut: Constants.SQLExecutionTimeoutInSecondsInvolvesDPR);
				using var transaction = batchStagingDataProvider.BeginTransaction();
				var safeDataProvider = serviceFactory.GetSafeDataProvider(cacheProvider);
				var nonPersistentObjectTransformer = new NonPersistentObjectTransformer(safeDataProvider);
				var wrapperFactory = serviceFactory.GetStagingDataWrapperFactory(safeDataProvider, metadataProvider, batchStagingDataProvider);
				var isDeletionType = updateType == UpdateType.Deletion;
				var merger = serviceFactory.GetSafeObjectUpdater(safeDataProvider, metadataProvider, isDeletionType);
				updateResults = merger.Update(entities.SelectMany(x => wrapperFactory.CreateWrapper(x.Item1)).ToArray());
				LogForFinalTrial($"Start to transform non-persistent objects: {updateResults.Length}.", isFinalTrial);
				updateResults = nonPersistentObjectTransformer.TransformNonPersistentObjects(updateResults);
				LogForFinalTrial($"Transformed to persistent objects: {updateResults.Length}.", isFinalTrial);
				await batchStagingDataProvider.CreateDataProcessingCloneRecords(sourceData.SDA_PK, updateResults.Where(x => x.NewRecordForCloneActionPK.HasValue), transaction.GetDbTransaction());
				LogForFinalTrial($"Saving merged data ...", isFinalTrial);
				await batchStagingDataProvider.SaveUpdateResults(sourceData, updateResults, transaction.GetDbTransaction());
				UpdateStatuses(batchStagingDataProvider, entities.Select(x => x.Item2), DataProcessingStatus.PRS);
				LogForFinalTrial($"Saving Processing Information...", isFinalTrial);
				await batchStagingDataProvider.SaveChangesAsync();
				LogForFinalTrial($"Processed: {entities.Length}", isFinalTrial);
				await safeDataProvider.SaveChangesAsync();
				batchStagingDataProvider.Commit(transaction);
				return [];
			}
			catch (Exception ex)
			{
				if (isFinalTrial)
				{
					Console.WriteLine($"Failed: {entities.Length}.");
					var safeObjPKs = GetMatchedSafeObjPKs(entities[0].Item1);
					SetOdataUriForException(ex, entityType, stageObjPK, safeObjPKs, DateTime.UtcNow);
					Console.Error.WriteLine(ex);
				}
				return entities;
			}
		}

		static void LogForFinalTrial(string message, bool isFinalTrial)
		{
			if (isFinalTrial)
			{
				Console.WriteLine(message);
			}
		}

		void SetOdataUriForException(Exception ex, Type entityType, Guid stageObjPK, Guid[] safeObjPKs, DateTime dateTime)
		{
			var strBuilder = new StringBuilder();
			strBuilder.AppendLine(CultureInfo.InvariantCulture, $"Stage Odata Uri: {odataUriProvider.GetUri(DbSource.RefDbRepoStaging, entityType, stageObjPK, dateTime)}");
			if (safeObjPKs != null && safeObjPKs.Any())
			{
				Array.ForEach(safeObjPKs, item => strBuilder.AppendLine(CultureInfo.InvariantCulture, $"Safe Odata Uri: {odataUriProvider.GetUri(DbSource.RefDbRepoSafe, entityType, item, dateTime)}"));
			}
			var filedInfo = typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
			filedInfo.SetValue(ex, ex.Message + "\r\n" + strBuilder.ToString().TrimEnd());
		}

		Guid[] GetMatchedSafeObjPKs(object stagingObj)
		{
			var safeDataProvider = serviceFactory.GetSafeDataProvider(cacheProvider);
			using var stagingDataProvider = serviceFactory.GetStagingDataProvider();
			var wrapperFactory = serviceFactory.GetStagingDataWrapperFactory(safeDataProvider, metadataProvider, stagingDataProvider);
			var wrappers = wrapperFactory.CreateWrapper(stagingObj).ToArray();
			var entityType = stagingObj.GetEntityType();
			var stagingObjPK = stagingObj.GetPKValue();
			var safeObjPKs = cacheProvider.MatchedSafeObjPKCache.GetOrAdd(stagingObjPK.ToString(), key => new Lazy<Guid[]>(() =>
			{
				var safeObjType = typeof(Safe.RefCusTariff).GetTypeFromBaseType(entityType.Name);
				var safeObjs = safeDataProvider.GetData(safeObjType, wrappers, metadataProvider)?.ToArray();
				var keyOrderAndSafeNewestsObjects = safeDataProvider.GetNewestObjectFromList(safeObjs, safeObjType, wrappers[0], metadataProvider);
				if (keyOrderAndSafeNewestsObjects.Keys.Count > 0)
				{
					var firstKeyOrder = keyOrderAndSafeNewestsObjects.Keys.OrderBy(x => x).FirstOrDefault();
					return keyOrderAndSafeNewestsObjects[firstKeyOrder].Select(x => x.GetPKValue()).ToArray();
				}

				return [];
			}));
			return safeObjPKs.Value;
		}
	}
}
