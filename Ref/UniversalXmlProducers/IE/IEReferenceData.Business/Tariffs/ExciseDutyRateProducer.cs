using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Business;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.IEReferenceData.Tariffs.Business
{
	public static class ExciseDutyRateProducer
	{
		const string DataSourceName = "IE Excise Taxes";

		public static void ExtractAndWriteToXml(IApplicationConfig config, ILogger logger)
		{
			var result = ExciseDutyRate.ExtractAndTryMapping(config, logger);
			var outputPath = Path.Combine(config.OutputDirectory, config.ExciseDuty_OutputFileName);

			if (result.UpdateDates.Any() && result.Tariffs.Any())
			{
				var publishDate = result.UpdateDates.OrderByDescending(date => date.UpdateDate).First().UpdateDate;

				Helper.ExportToXmlFile(
					DataSourceName,
					outputPath,
					GetXmlWriterConfiguration(publishDate),
					publishDate,
					result.Tariffs
				);
				foreach (var date in result.UpdateDates)
				{
					RuntimeDataRecorder.Write(date.Key, date.UpdateDate);
				}
			}
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration(DateTime publishDate)
		{
			var config = new XmlWriterConfiguration();

			var tariff = new EntityTypeConfiguration<RefCusTariff>(true);
			tariff.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.IECountryCode);
			tariff.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariff.IncludeColumn(x => x.ZZ1_Description);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, false, 1);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_StartDate, false, publishDate);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_EndDate, false, Constants.MaximumDateTime);
			tariff.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.IECountryCode);
			tariff.IncludeColumn(x => x.RefCusRates);
			config.IncludeEntityTypeConfiguration(tariff);

			var rate = new EntityTypeConfiguration<RefCusRate>(true);
			rate.IncludeColumn(x => x.ZZ2_RateFormula);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, publishDate);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.MaximumDateTime);
			rate.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.IECountryCode);
			config.IncludeEntityTypeConfiguration(rate);


			return config;
		}
	}
}
