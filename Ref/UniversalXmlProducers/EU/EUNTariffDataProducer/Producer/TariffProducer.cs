using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class TariffProducer : BaseProducer, ITariffProducer
	{
		protected ITariffDataProducer tariffDataProducer { get; set; }
		public IXmlProducer<RefCusTariff> xmlProducer { get; set; }

		public virtual ITariffDataProducer GetTariffDataProducer() => tariffDataProducer;

		public void Run(IEnumerable<RefCusTariff> tariffs)
		{
			xmlProducer = GetNewTariffXmlProducer();
			var processedTariffs = this.LoadAllTariffs(tariffs);
			if (processedTariffs != null)
			{
				xmlProducer.ExportToXmlInBatch(processedTariffs, PublishTime);
			}
		}

		public void Run(IEnumerable<string> tariffHeaders, ICollection<RefCusTariff> tariffCollection)
		{
			Argument.NotNull(tariffCollection, nameof(tariffCollection));
			xmlProducer = GetNewTariffXmlProducerIncludingCompositeKey(false);

			if (!PrepareTariffDataProducer(null))
			{
				return;
			}

			var tariffs = tariffDataProducer.LoadTariffRates(EUNUtils.GenerateWebTariffHeadersFromRefCusTariff(tariffHeaders, tariffCollection));
			xmlProducer.InitializeWriter(PublishTime);
			xmlProducer.ExportToXml(tariffs);
		}

		public virtual bool PrepareTariffDataProducer(IEnumerable<RefCusTariff> tariffs)
		{
			var files = LocateWebFiles().ToList();
			var error = files.FirstOrDefault(x => x.Exception != null);
			if (error != null)
			{
				xmlProducer.InitializeWriter(DateTime.Today);
				var errorMessage = error.Exception.Message;
				xmlProducer.ReportDataSourceError(errorMessage);
				throw error.Exception;
			}

			var filesDownloadedSuccessfully = DownloadFiles(files);
			if (!filesDownloadedSuccessfully)
			{
				throw new InvalidOperationException("Failure downloading files.");
			}

			var measuringUnitTransformer = new MeasuringUnitTransformer();
			var agriculturalComponentFormulaExtractor = new AgriculturalComponentFormulaExtractor(measuringUnitTransformer);
			var conditionFormulaExtractor = new ConditionFormulaExtractor(measuringUnitTransformer);
			var formulaExtractor = new FormulaExtractor(measuringUnitTransformer, agriculturalComponentFormulaExtractor, conditionFormulaExtractor);
			var rateGenerator = new RateGenerator(formulaExtractor);
			var conditionValueDescriptionExtractor = new ConditionValueDescriptionExtractor();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor);
			var tariffGenerator = new TariffGenerator(rateGenerator, conditionGenerator);

			var preferenceMapper = new PreferenceMapper();
			var tariffCodeExtractor = new TariffCodeExtractor(tariffs);
			var tariffMerger = GetNewTariffMerger(tariffCodeExtractor, preferenceMapper);

			var rawRecords = GetNewRawRecord();
			rawRecords.Parse(files);

			tariffDataProducer = new TariffDataProducer(rawRecords, tariffGenerator, tariffMerger);

			return true;
		}

		protected abstract TariffXmlProducer GetNewTariffXmlProducer();

		protected abstract TariffXmlProducer GetNewTariffXmlProducerIncludingCompositeKey(bool includeCompositeKey);

		protected abstract ITariffMerger GetNewTariffMerger(ITariffCodeExtractor tariffCodeExtractor, IPreferenceMapper preferenceMapper);

		protected abstract RawRecord GetNewRawRecord();

		internal IEnumerable<RefCusTariff> LoadAllTariffs(IEnumerable<RefCusTariff> tariffs)
		{
			if (PrepareTariffDataProducer(tariffs))
			{
				var processedTariffs = tariffDataProducer.LoadAllTariffRates();
				return LoadTariffs(tariffs, processedTariffs);
			}

			return null;
		}

		protected abstract IEnumerable<RefCusTariff> LoadTariffs(IEnumerable<RefCusTariff> allTariffs, IEnumerable<RefCusTariff> processedTariffs);

		internal class PreferenceMapperNoChangesImplementation : IPreferenceMapper
		{
			public IEnumerable<RefCusCondition> SetPreferenceOnConditions(IEnumerable<RefCusCondition> originalConditions)
			{
				Argument.NotNull(originalConditions, nameof(originalConditions));
				return originalConditions;
			}

			public IEnumerable<RefCusRate> SetPreferenceOnRates(IEnumerable<RefCusRate> originalRates)
			{
				Argument.NotNull(originalRates, nameof(originalRates));
				return originalRates;
			}
		}

		public override string EUNLibraryBasePage => ApplicationConfig.EUTariffBaseUrl;
		protected override IFileDownloaderWrapper GetFileDownloaderWrapper() => new FileDownloaderWrapper(httpClient);

		static readonly HttpClient httpClient = FileDownloaderHttpClientFactory.CreateHttpClient();

	}
}
