using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class Processor
	{
		readonly SourceData _sourceData;
		readonly IServiceFactory _serviceFactory;
		readonly IStagingDataProvider _stagingDataProvider;
		readonly IStagingDataProvider _readOnlyStagingDataProvider;
		readonly IDataSourceProvider _dataSourceProvider;
		readonly ICacheProvider _cacheProvider;
		readonly IMetadataProvider _metadataProvider;
		readonly IOdataUriProvider _odataUriProvider;

		public Processor(SourceData sourceData, IServiceFactory serviceFactory, IStagingDataProvider stagingDataProvider, IStagingDataProvider readOnlyStagingDataProvider,
			IDataSourceProvider dataSourceProvider, ICacheProvider cacheProvider, IMetadataProvider metadataProvider, IOdataUriProvider odataUriProvider)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			Argument.NotNull(serviceFactory, nameof(serviceFactory));
			Argument.NotNull(stagingDataProvider, nameof(stagingDataProvider));
			Argument.NotNull(readOnlyStagingDataProvider, nameof(readOnlyStagingDataProvider));
			Argument.NotNull(dataSourceProvider, nameof(dataSourceProvider));
			Argument.NotNull(cacheProvider, nameof(cacheProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(odataUriProvider, nameof(odataUriProvider));

			_sourceData = sourceData;
			_serviceFactory = serviceFactory;
			_stagingDataProvider = stagingDataProvider;
			_readOnlyStagingDataProvider = readOnlyStagingDataProvider;
			_dataSourceProvider = dataSourceProvider;
			_cacheProvider = cacheProvider;
			_metadataProvider = metadataProvider;
			_odataUriProvider = odataUriProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public async Task<ProducerStatus> DoProcess()
		{
			try
			{
				PublishDateProvider.SetPublishedDate(_sourceData.SDA_SourceTime.Value);
				await _stagingDataProvider.SaveUpdateResults(_sourceData, [], null); // Intialization to overcome bug No Mapping Table found
				_metadataProvider.Validate();

				var updateType = _dataSourceProvider.GetUpdateType();
				var merger = new MergeProcessor(_readOnlyStagingDataProvider, _serviceFactory, _metadataProvider, _odataUriProvider, _sourceData, _cacheProvider, MAXIMUM_RETRY, MAX_NO_OF_BATCHES, updateType);
				await merger.Process(MERGE_SIZE);

				if (_dataSourceProvider.IsAutoSchema)
				{
					await _stagingDataProvider.MarkDataProcessingInformationAsIgnored(_sourceData);
				}

				var autoExpirationResult = await AutoExpiration();

				var cloneResult = await Clone();

				_stagingDataProvider.MarkSourceDataStatus(_sourceData, autoExpirationResult, cloneResult);
				await _stagingDataProvider.SaveChangesAsync();

				if (_sourceData.IsSourceDataMergedStatus())
				{
					return ProducerStatus.Success;
				}

				Console.Error.WriteLine("Failed to merge");
				return ProducerStatus.MergeFailure;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to process");
				Console.Error.WriteLine(ex);
				_sourceData.SDA_Status = StatusProvider.GetERRStatus();
				await _stagingDataProvider.SaveChangesAsync();

				return ProducerStatus.MergeFailure;
			}
		}

		async Task<bool> Clone()
		{
			var cacheProvider = CacheProvider.Create();
			var cloneProcessor = new CloneProcessor(_serviceFactory, cacheProvider, _sourceData, MAXIMUM_RETRY, MAX_NO_OF_BATCHES);
			var cloneResult = await cloneProcessor.Process(MERGE_SIZE);
			return cloneResult;
		}

		async Task<bool> AutoExpiration()
		{
			var autoExpirationResult = true;
			if (_dataSourceProvider.IsFullUpdate)
			{
				var expirationProcessor = new ExpirationProcessor(_sourceData, _stagingDataProvider, _serviceFactory.GetSafeDataProvider(CacheProvider.Create()), _metadataProvider.OriginalMetadataProvider ?? _metadataProvider, _serviceFactory.OverlappingCalculator);
				autoExpirationResult = await expirationProcessor.UpdateResultsAndAutoExpire();
			}

			return autoExpirationResult;
		}

		const int MAX_NO_OF_BATCHES = 10;
		const int MERGE_SIZE = 10;
		const int MAXIMUM_RETRY = 2;
	}
}
