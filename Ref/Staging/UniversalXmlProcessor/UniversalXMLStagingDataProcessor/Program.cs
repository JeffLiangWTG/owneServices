using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	[SuppressMessage("Microsoft.Contracts", "Nonnull-6-0")]
	public static class Program
	{
		public static async Task<int> Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			ErrorConstants.AddJsonFile(Constants.JsonConfigFile);
			return (int)await ProcessAsync(Constants.SafeUpdateServiceUri);
		}

		static async Task<ProducerStatus> ProcessAsync(string safeUpdateServiceUri)
		{
			using var heartbeatStagingDataProvider = new StagingDataProvider(new StagingRepository(Constants.ConnectionStrings) { CommandTimeout = Constants.SQLExecutionTimeoutInSeconds });
			using var readOnlyStagingDataProvider = new StagingDataProvider(new StagingRepository(Constants.ConnectionStrings, interceptors: [new SqlDurationInterceptor()]) { CommandTimeout = Constants.SQLExecutionTimeoutInSeconds });
			using var stagingDataProvider = new StagingDataProvider(new StagingRepository(Constants.ConnectionStrings, interceptors: [new SqlDurationInterceptor()]) { CommandTimeout = Constants.SQLExecutionTimeoutInSecondsInvolvesDPR });
			using var accessTokenProvider = new AccessTokenProvider(Constants.TenantId, Constants.ClientId, Constants.ServiceId, Constants.PrivateKeyFileName, Constants.CertificateFileName, Constants.RefreshAdvanceInMinutes);
			var sourceData = stagingDataProvider.GetSourceData().FirstOrDefault();
			if (sourceData != null)
			{
				Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.AppContext)}:{sourceData.SDA_PK}");
				Console.WriteLine($"Merger starting to process {sourceData.SDA_Filename}");

				var dateTimeProvider = new DateTimeProvider();
				var datasourceProvider = new DataSourceProvider(sourceData.SDA_ContentText);
				var serviceFactory = new ServiceFactory(safeUpdateServiceUri, datasourceProvider.InclusiveEndDate, accessTokenProvider);

				using (var heartbeat = new Heartbeat(Constants.HeartbeatIntervalInMinutes, sourceData.SDA_PK, heartbeatStagingDataProvider, dateTimeProvider, sourceData.SDA_NotProcessedUntil))
				{
					heartbeat.Start();

					if (!string.IsNullOrEmpty(datasourceProvider.AppName))
					{
						Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.SourceDataAppName)}:{datasourceProvider.AppName}");
					}
					if (!string.IsNullOrEmpty(datasourceProvider.SubSource))
					{
						Console.WriteLine($"{FlagHelper.GetFlag(UXMLProducerHelper.SubSource)}:{datasourceProvider.SubSource}");
					}

					if (sourceData.SDA_NotProcessedUntil.HasValue && sourceData.SDA_NotProcessedUntil.Value > sourceData.SDA_CreatedTime.AddHours(Constants.TimeOutHours))
					{
						sourceData.SDA_Status = StatusProvider.GetERRStatus();
						await stagingDataProvider.SaveChangesAsync();
						var errorMessage = $"SourceData {sourceData.SDA_SubSource} is not processed after {Constants.TimeOutHours} hours.";
						Console.Error.WriteLine(errorMessage);
						return ProducerStatus.MergeFailure;
					}

					var IsdependencyTimeout = dateTimeProvider.GetUTCNow() > sourceData.SDA_CreatedTime.AddMinutes(Constants.TimeToProcessMinutes);
					var processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, datasourceProvider.Dependencies, IsdependencyTimeout);
					switch (processResult.Action)
					{
						case ProcessAction.ReportError:
							sourceData.SDA_Status = StatusProvider.GetERRStatus();
							await stagingDataProvider.SaveChangesAsync();
							var errorMessage = $"SourceData {sourceData.SDA_SubSource} is not processed because dependency {processResult.Dependency.DataSource} is missing or has error.";
							Console.Error.WriteLine(errorMessage);
							return ProducerStatus.MergeFailure;
						case ProcessAction.WaitForDependency:
							sourceData.SDA_NotProcessedUntil = dateTimeProvider.GetUTCNow().AddMinutes(Constants.TimeToProcessMinutes);
							await stagingDataProvider.SaveChangesAsync();
							Console.WriteLine($"SourceData {sourceData.SDA_SubSource} should wait for dependency {processResult.Dependency.DataSource}.");
							return ProducerStatus.Success;
						default:
							break;
					}

					var cacheProvider = CacheProvider.Create();
					var cacheProviderForOriginal = CacheProvider.Create();

					var metadataText = string.Empty;
					if (datasourceProvider.IsAutoSchema)
					{
						var autoSchemas = stagingDataProvider.GetAutoSchemas(sourceData).ToArray();
						if (autoSchemas.Length > 1)
						{
							Console.Error.WriteLine("Multiple auto schemas are not supported.");
							return ProducerStatus.MergeFailure;
						}
						metadataText = autoSchemas.FirstOrDefault();
					}
					else
					{
						metadataText = sourceData.SDA_ContentText;
					}

					var metadata = new MetadataProvider(metadataText, cacheProvider, cacheProviderForOriginal, datasourceProvider.IsAutoSchema);
					DataProviderHelper.SetEnableExpirable(metadata.EnableExpirable);
					var odataUriProvider = new OdataUriProvider(metadata);
					var processor = new Processor(sourceData, serviceFactory, stagingDataProvider, readOnlyStagingDataProvider, datasourceProvider, cacheProvider, metadata, odataUriProvider);
					return await processor.DoProcess();
				}
			}

			return ProducerStatus.Success;
		}
	}
}
