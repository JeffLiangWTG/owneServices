using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class ExpirationProcessor : IExpirationProcessor
	{
		public ExpirationProcessor(SourceData sourceData, IStagingDataProvider stagingProvider,
			ISafeDataProvider safeProvider, IMetadataProvider metadataProvider,
			IOverlappingCalculator overlappingCalculator)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(stagingProvider, nameof(stagingProvider));
			Argument.NotNull(safeProvider, nameof(safeProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(overlappingCalculator, nameof(overlappingCalculator));

			this.sourceData = sourceData;
			this.stagingProvider = stagingProvider;
			this.safeProvider = safeProvider;
			this.metadataProvider = metadataProvider;
			this.overlappingCalculator = overlappingCalculator;

			sourceTime = sourceData.SDA_SourceTime!.Value;
			errorDPRHeaderPKList = stagingProvider.GetProcessingResults(sourceData)
				.Where(x => x.DPR_Status == DataProcessingStatus.ERR.ToString() &&
							x.DPR_PublicationTime == sourceTime).Select(x => x.DPR_ParentPK);
		}

		readonly IStagingDataProvider stagingProvider;
		readonly ISafeDataProvider safeProvider;
		readonly IMetadataProvider metadataProvider;
		readonly IOverlappingCalculator overlappingCalculator;
		readonly SourceData sourceData;
		readonly DateTime sourceTime;
		readonly IEnumerable<Guid> errorDPRHeaderPKList;

		public async Task<bool> UpdateResultsAndAutoExpire()
		{
			var isAutoExpiredEnabled = stagingProvider.IsAutoExpiredEnabled(sourceData);
			if (!isAutoExpiredEnabled)
			{
				Console.WriteLine("Auto Expiration won't run as DSI_EnableAutoExpiration is set to false");
				await stagingProvider.UpdateAllExpiredDataProcessingResultAsync(sourceData);
				return true;
			}

			if (HasError())
			{
				return false;
			}

			if (!HasProcessingResultsToProcess())
			{
				Console.WriteLine("Auto Expiration won't run as no expirable DPR record found");
				return true;
			}

			Console.WriteLine("Auto Expiration Start...");

			var tblPrefix = stagingProvider.GetDataProcessingInformation(sourceData).FirstOrDefault()
				?.DPI_ParentTableCode;
			var result = await DeleteOrExpireRecursivelyAsync(tblPrefix);

			Console.WriteLine(result ? "Auto Expiration finish... Succeed" : "Auto Expiration finish... Failed");

			return result;
		}

		async Task<bool> DeleteOrExpireRecursivelyAsync(string tblPrefix)
		{
			if (string.IsNullOrEmpty(tblPrefix))
			{
				Console.WriteLine("TablePrefix is empty and ignore it.");
				return true;
			}

			var safeEntityType = safeProvider.GetTypeFromTblPrefix(tblPrefix);
			if (!DataProviderHelper.IsExpirableType(safeEntityType) && !safeEntityType.ContainsIsActiveColumn())
			{
				await stagingProvider.UpdateDescendentDPRWithExpiredAncestor(tblPrefix, sourceData);
			}

			var failedProcessingResults = await BatchDeleteOrExpireAsync(tblPrefix, safeEntityType);
			var success = failedProcessingResults.Count == 0;
			if (!success)
			{
				await stagingProvider.UpdateDataProcessingResultStatusAndExpirationTime(DataProcessingStatus.ERR,
					sourceTime, failedProcessingResults.Select(x => x.DPR_ParentPK), sourceData);
			}

			var entityType = stagingProvider.GetTypeFromTblPrefix(tblPrefix);
			var relatedTypes = stagingProvider.GetRelatedEntityTypes(entityType.Name, metadataProvider);
			if (relatedTypes != null)
			{
				foreach (var relatedType in relatedTypes)
				{
					success = await DeleteOrExpireRecursivelyAsync(relatedType.GetTablePrefix()) && success;
				}
			}

			return success;
		}

		async Task<List<DataProcessingResult>> BatchDeleteOrExpireAsync(string tblPrefix, Type safeEntityType)
		{
			var failedProcessingResults = new List<DataProcessingResult>();
			var expireCurrentEntity = ShouldExpireCurrentEntity(tblPrefix);
			var isData = metadataProvider.IsData(safeEntityType.Name);

			var lastProcessedId = Guid.Empty;

			Console.WriteLine($"Start to auto-expire {safeEntityType.Name} records");

			while (true)
			{
				var expiredResults = stagingProvider.GetProcessingResults(sourceData)
					.Where(x => x.DPR_PublicationTime < sourceTime
								&& x.DPR_ParentTableCode == tblPrefix
								&& x.DPR_Status != DataProcessingStatus.PRS.ToString()
								&& x.DPR_PK > lastProcessedId
								&& !errorDPRHeaderPKList.Contains(x.DPR_DatasetPK.Value))
					.OrderBy(x => x.DPR_PK)
					.Take(Constants.ExpiredBatchSize)
					.ToArray();
				if (expiredResults.Length == 0)
				{
					break;
				}

				var failedExpiredProcessingResults = await MergeProcessorExtension.Retry(
					expiredResults,
					(processResults, batchSize) =>
						DeleteOrExpireAsync(processResults, tblPrefix, batchSize, isData, expireCurrentEntity),
					x => x, 1, 3, 100, sourceData.SDA_PK);
				failedProcessingResults.AddRange(failedExpiredProcessingResults);

				lastProcessedId = expiredResults.Last().DPR_PK;
			}

			Console.WriteLine($"Finish to auto-expire {safeEntityType.Name} records");

			return failedProcessingResults;
		}

		bool HasError()
		{
			var dpiList = stagingProvider.GetDataProcessingInformation(sourceData);
			if (!dpiList.Any())
			{
				Console.Error.WriteLine("Error: Auto Expiration won't run as no DPI record found");
				return true;
			}

			if (dpiList.Any(x => x.DPI_Status == DataProcessingStatus.QUE.ToString()))
			{
				Console.Error.WriteLine("Error: Auto Expiration won't run as there is QUE DPI record found");
				return true;
			}

			if (dpiList.Where(x => x.DPI_Status == DataProcessingStatus.ERR.ToString())
				.Any(x => !x.DPI_HasDPRRecordWhenError))
			{
				Console.Error.WriteLine(
					"Error: Auto Expiration won't run, as there are error records whose DPR records could not be located");
				return true;
			}

			return false;
		}

		bool HasProcessingResultsToProcess()
		{
			return stagingProvider.GetProcessingResults(sourceData).Any(x =>
				x.DPR_Status != DataProcessingStatus.PRS.ToString() && x.DPR_PublicationTime < sourceTime);
		}

		internal bool ShouldExpireCurrentEntity(string tblPrefix)
		{
			var result = true;

			var stagingEntityType = stagingProvider.GetTypeFromTblPrefix(tblPrefix);
			if (ExpirableForeignKeyExclusion.ContainsKey(stagingEntityType))
			{
				var stagingRelatedEntityTypes =
					stagingProvider.GetRelatedEntityTypes(stagingEntityType.Name, metadataProvider);

				if (stagingRelatedEntityTypes.Any(entityType =>
						ExpirableForeignKeyExclusion.ContainsPair(stagingEntityType, entityType)))
				{
					result = false;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		async Task<IEnumerable<DataProcessingResult>> DeleteOrExpireAsync(
			IEnumerable<DataProcessingResult> processingResults, string tblPrefix, int batchSize, bool isData,
			bool shouldExpireCurrentEntity)
		{
			var failedResults = new List<DataProcessingResult>();

			for (var batchIndex = 0; batchIndex * batchSize < processingResults.Count(); batchIndex++)
			{
				var currentBatch = processingResults.Skip(batchIndex * batchSize).Take(batchSize).ToList();

				foreach (var expirationGroup in currentBatch.GroupBy(x => x.DPR_ExpirationTime))
				{
					try
					{
						var expirationTime = expirationGroup.Key ?? sourceTime;
						var parentPks = expirationGroup.Select(x => x.DPR_ParentPK);

						await (isData
							? DeleteOrExpireDataAsync(tblPrefix, shouldExpireCurrentEntity, parentPks, expirationTime)
							: stagingProvider.UpdateDataProcessingResultExpirationTime(sourceTime, parentPks,
								sourceData));
					}
					catch (Exception)
					{
						failedResults.AddRange(expirationGroup);
					}
				}
			}

			return failedResults;
		}

		async Task DeleteOrExpireDataAsync(string tblPrefix, bool shouldExpireCurrentEntity,
			IEnumerable<Guid> parentPks, DateTime expirationTime)
		{
			if (shouldExpireCurrentEntity)
			{
				var safeEntityType = safeProvider.GetTypeFromTblPrefix(tblPrefix);

				if (DataProviderHelper.IsExpirableType(safeEntityType))
				{
					await safeProvider.BatchExpire(tblPrefix, parentPks,
						overlappingCalculator.GetEndDate(expirationTime));
				}
				else if (safeEntityType.ContainsIsActiveColumn())
				{
					await safeProvider.BatchInActive(tblPrefix, parentPks);
				}
				else
				{
					await safeProvider.BatchDelete(tblPrefix, parentPks);
				}
			}

			await stagingProvider.UpdateDataProcessingResultStatusAndExpirationTime(DataProcessingStatus.PRS,
				sourceTime, parentPks, sourceData);
		}
	}
}
