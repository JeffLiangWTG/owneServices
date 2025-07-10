using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Services;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IBuildersProvider
	{
		IEnumerable<(BuildersFilePath[] FilePaths, IBuilder<NZTariffProcessingData> Builder)> GetBuilderInfos(string dir, IDateProvider dateProvider, ILogger logger);
	}

	public class BuildersProvider : IBuildersProvider
	{
		public IEnumerable<(BuildersFilePath[] FilePaths, IBuilder<NZTariffProcessingData> Builder)> GetBuilderInfos(string dir, IDateProvider dateProvider, ILogger logger)
		{
			var portalDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			yield return (new[] { new BuildersFilePath(Path.Combine(dir, "Tariff", "time_stamp.txt")) }, new TariffPublicationTimeBuilder<NZTariffProcessingData>(
				(NZTariffProcessingData processingData) => processingData.LastRunDateTariff,
				(NZTariffProcessingData processingData, DateTime dateTime) => processingData.LastRunDateTariff = dateTime));

			var filePaths = new List<BuildersFilePath>
			{
				new BuildersFilePath(Path.Combine(dir, "Tariff", "Tariff_Details.csv"), BuilderFilePathSymbol.TariffDetail)
			};
			var tariffOverrideFilePath = GetOverrideFilePath(portalDirPath, ApplicationConfig.TariffOverrideFileName, BuilderFilePathSymbol.TariffOverride);
			if (tariffOverrideFilePath != null)
			{
				filePaths.Add(tariffOverrideFilePath);
			}

			yield return (filePaths.ToArray(), new TariffDetailsBuilder(dateProvider, logger));
			yield return (new[] { new BuildersFilePath(Path.Combine(dir, "Tariff", "Tariff_Rates.csv")) }, new TariffRatesBuilder(dateProvider, logger));
			yield return (new[]
			{
				new BuildersFilePath(Path.Combine(dir, "Tariff", "Tariff_Levy_Formulas.csv"), BuilderFilePathSymbol.LevyFormula),
				new BuildersFilePath(Path.Combine(dir, "Tariff", "Tariff_Levies.csv"), BuilderFilePathSymbol.Levy),
			}, new TariffLeviesBuilder(dateProvider, logger));
		}

		private static BuildersFilePath GetOverrideFilePath(string portalDirPath, string fileName, BuilderFilePathSymbol builderFilePathSymbol)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				return new BuildersFilePath(
					Path.Combine(portalDirPath, fileName),
					builderFilePathSymbol
				);
			}
			return null;
		}
	}
}
