using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ITReferenceData.Services;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public class ExportMeasuresProducer
	{
		public ExportMeasuresProducer(ILogger logger, IExportMeasuresLoader loader, IExportMeasureMapper mapper, ICusTariffLoader tariffLoader, IDataLookup tradeGroupLookup, IDataLookup additionalCodeLookup, IPublicationTimeLoader publicationTimeLoader)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.loader = Argument.NotNull(loader, nameof(loader));
			this.mapper = Argument.NotNull(mapper, nameof(mapper));
			this.tariffLoader = Argument.NotNull(tariffLoader, nameof(tariffLoader));
			this.tradeGroupLookup = Argument.NotNull(tradeGroupLookup, nameof(tradeGroupLookup));
			this.additionalCodeLookup = Argument.NotNull(additionalCodeLookup, nameof(additionalCodeLookup));
			this.publicationTimeLoader = Argument.NotNull(publicationTimeLoader, nameof(publicationTimeLoader));
		}

		public async Task<IReadOnlyCollection<RefCusTariff>> ProduceEntitiesAsync()
		{
			var startDate = publicationTimeLoader.GetDateTime();
			await loader.DoHandshakeAsync();

			if (!await tradeGroupLookup.LoadAsync(ApplicationConfig.RefDataRepoUri))
			{
				logger.Log("Failed to load trade groups");
				return [];
			}

			if (!await additionalCodeLookup.LoadAsync(ApplicationConfig.RefDataRepoUri))
			{
				logger.Log("Failed to load additional codes");
				return [];
			}

			if (!await tariffLoader.LoadAsync(ApplicationConfig.RefDataRepoUri))
			{
				logger.Log("Failed to load tariffs");
				return [];
			}

			var result = new List<RefCusTariff>();

			foreach (var tariffCode in tariffLoader.AllCodes)
			{
				try
				{
					var measures = await loader.GetMeasureInformationAsync(tariffCode);

					if (measures.Count == 0)
					{
						continue;
					}

					var additionalCodesInput = measures
						.GroupBy(x => x.AdditionalCode)
						.Select(x => CreateAdditionalCodeInput(x, tariffCode, startDate))
						.Where(x => x.Applicabilities.Length > 0)
						.ToArray();

					if (additionalCodesInput.Length == 0)
					{
						continue;
					}

					var mapperInput = new TariffInput(tariffCode, additionalCodesInput);
					result.Add(mapper.GetMapping(mapperInput));
				}
				catch (HttpRequestException ex)
				{
					logger.Log(ex, $"Tariff {tariffCode} - Failed to get measures");
				}
			}

			return result;
		}

		AdditionalCodeInput CreateAdditionalCodeInput(IGrouping<string, MeasureInformation> group, string tariffCode, DateTime applicabilityStartDate)
		{
			var code = group.Key;
			var description = additionalCodeLookup.Lookup(code) ?? code;
			var tradeGroups = CreateApplicabilities(group, tariffCode, applicabilityStartDate);

			return new AdditionalCodeInput(code, description, tradeGroups);
		}

		ApplicabilityInput[] CreateApplicabilities(IEnumerable<MeasureInformation> measures, string tariffCode, DateTime applicabilityStartDate)
		{
			return measures
				.Select(measure => LookupTradeGroup(measure.TradeGroup, tariffCode))
				.Where(tradeGroup => tradeGroup != null)
				.Select(x => new ApplicabilityInput(applicabilityStartDate, x))
				.ToArray();
		}

		string LookupTradeGroup(string tradeGroupCode, string tariffCode)
		{
			var lookupResult = tradeGroupLookup.Lookup(tradeGroupCode);

			if (lookupResult == null)
			{
				logger.Log($"Tariff {tariffCode} - Failed to lookup trade group '{tradeGroupCode}'");
			}

			return lookupResult;
		}

		readonly ILogger logger;
		readonly IExportMeasuresLoader loader;
		readonly IExportMeasureMapper mapper;
		readonly ICusTariffLoader tariffLoader;
		readonly IDataLookup tradeGroupLookup;
		readonly IDataLookup additionalCodeLookup;
		readonly IPublicationTimeLoader publicationTimeLoader;
	}
}
