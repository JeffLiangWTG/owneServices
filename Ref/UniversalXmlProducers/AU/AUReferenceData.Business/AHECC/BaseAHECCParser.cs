using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public abstract class BaseAHECCParser
	{
		protected BaseAHECCParser(IDateTimeProvider dateProvider)
		{
			HttpClientHelper = new HttpClientHelper();
			this.dateProvider = dateProvider;
		}
		IDateTimeProvider dateProvider;

		public void Parse()
		{
			try
			{
				ParseCore(dateProvider.CurrentLocalDateTime);
			}
			catch (Exception ex) when (!(ex is UnhandledApplicationException))
			{
				throw new UnhandledApplicationException(ex);
			}
		}

		protected virtual void ParseCore(DateTime runTime)
		{
			var todaysDate = runTime.Date;

			var writer = new XmlWriter(AHECSSXMLWriterConfigurationBuilder.Build(DataGrouping));
			writer.SetDataSource(DataSource);
			writer.SetUpdateType(UpdateType);

			Uri fileUri;
			DateTime publishedDate;
			string fileContent;

			try
			{
				(fileUri, publishedDate) = DownloadHelper.DiscoverDataFile(HttpClientHelper, DownloadFileNamePrefix, DownloadDirectory);
			}
			catch (UnhandledApplicationException)
			{
				Console.Error.WriteLine($"Cannot find {DownloadFileNamePrefix} file in {DownloadDirectory}");
				return;
			}

			try
			{
				fileContent = HttpClientHelper.GetWebPageAsync(fileUri.AbsoluteUri).Result;
			}
			catch (Exception)
			{
				Console.Error.WriteLine($"Cannot load {fileUri.AbsoluteUri}");
				throw;
			}

			var tariffEntities = TextToEntitiesConverter<RefCusTariff>.ConvertFromString(fileContent, BuildTariffMappings());
			var uomEntities = TextToEntitiesConverter<RefCusTariffUOM>.ConvertFromString(fileContent, BuildUOMMappings());

			foreach (var (tariff, uom) in tariffEntities.Zip(uomEntities, (tariff, uom) => (tariff, uom)))
			{
				var endDate = tariff.ZZ1_EndDate.Date;
				if (endDate >= todaysDate.AddYears(-5))
				{
					if (tariff.ZZ1_StartDate < tariff.ZZ1_EndDate)
					{
						tariff.RefCusTariffUOMs = new[] { uom };
						writer.PopulateData(tariff);
					}
					else
					{
						Console.WriteLine($"Skipped {tariff.ZZ1_TariffCode}: Start Date is later than End Date.");
					}
				}
			}

			var outputPath = Path.Combine(ApplicationConfig.OutputDirectory, OutputFileName);
			writer.SetPublicationTime(publishedDate);
			writer.SaveXml(outputPath);

			Console.WriteLine($"AHECC Parser completed. '{OutputFileName}' generated.");
		}

		public IHttpClientHelper HttpClientHelper { get; set; }

		protected abstract string DataSource { get; }

		protected abstract string DownloadDirectory { get; }

		protected abstract string DownloadFileNamePrefix { get; }

		protected abstract string OutputFileName { get; }

		protected abstract string DataGrouping {  get; }

		protected abstract UpdateType UpdateType { get; }

		static IEnumerable<PropertyMapping<RefCusTariff>> BuildTariffMappings()
		{
			return new[]
			{
				new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_TariffCode, 1, 8),
				new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_StartDate, 10, 8),
				new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_EndDate, 19, 8, Constants.RefData_Common.MaximumDateTime),
				new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_Description, 33, 253),
			};
		}

		static IEnumerable<PropertyMapping<RefCusTariffUOM>> BuildUOMMappings()
		{
			return new[]
			{
				new PropertyMapping<RefCusTariffUOM>(entity => entity.ZZ8_UOM, 28, 2),
			};
		}
	}
}
