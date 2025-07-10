using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Services;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	sealed class TariffDetailsBuilder : IBuilder<NZTariffProcessingData>
	{
		public TariffDetailsBuilder(IDateProvider dateProvider, ILogger logger)
		{
			tariffs = new Dictionary<string, RefCusTariff>();
			this.dateProvider = dateProvider;
			Logger = logger;
		}

		readonly Dictionary<string, RefCusTariff> tariffs;
		readonly IDateProvider dateProvider;
		ILogger Logger { get; }

		public bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, NZTariffProcessingData processingData)
		{
			var tariffDetailFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.TariffDetail);
			if (tariffDetailFilePath == null)
			{
				throw new ArgumentException($"TariffDetailsBuilds needs a file for symbol {BuilderFilePathSymbol.TariffDetail}");
			}

			Logger.LogInfo("Start processing Details file...");

			var result = true;
			var filePath = tariffDetailFilePath.FilePath;
			var lines = File.ReadLines(filePath);
			var activeDate = dateProvider.ActiveDate;
			var tariffOverrideFilePath = filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.TariffOverride)?.FilePath;
			var descriptionOverrides = CodeDescription.GetCodeDescriptionsFromJsonPath(tariffOverrideFilePath, Logger);

			foreach (var line in lines.Skip(1))
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				var tariffDetails = line.Trim().Split(Constants.TariffSplit);

				if (tariffDetails.Length == 15)
				{
					var startDate = Constants.MinSmallDateTime;
					var endDate = Constants.MaxSmallDateTime;
					if (!string.IsNullOrEmpty(tariffDetails[12]) && !DateTime.TryParse(tariffDetails[12], out startDate))
					{
						throw new InvalidOperationException($"Can't parse start date from line '{line}' in '{filePath}'.");
					}
					if (!string.IsNullOrEmpty(tariffDetails[13]) && !DateTime.TryParse(tariffDetails[13], out endDate))
					{
						throw new InvalidOperationException($"Can't parse end date from line '{line}' in '{filePath}'.");
					}
					startDate = startDate < Constants.MinSmallDateTime ? Constants.MinSmallDateTime : startDate;
					endDate = endDate > Constants.MaxSmallDateTime ? Constants.MaxSmallDateTime : endDate;

					var tariffCode = string.Concat(tariffDetails.Take(6));
					var tariffCodeWithoutCheckDigit = tariffCode.Substring(0, 10);
					if (endDate > activeDate)
					{
						if (tariffs.TryGetValue(tariffCodeWithoutCheckDigit, out var duplicatedTariff))
						{
							if (duplicatedTariff.ZZ1_EndDate < endDate)
							{
								FillTariffData(duplicatedTariff, tariffCode, startDate, endDate, tariffDetails, descriptionOverrides);
							}
						}
						else
						{
							var tariff = new RefCusTariff();
							FillTariffData(tariff, tariffCode, startDate, endDate, tariffDetails, descriptionOverrides);
							tariffs.Add(tariffCodeWithoutCheckDigit, tariff);
						}
					}
				}
				else
				{
					var errorMessage = $"Can't process data line '{line}' in '{filePath}'.";
					Logger.LogError(errorMessage);
				}
			}

			if (tariffs.Count == 0)
			{
				throw new InvalidOperationException($"Can't create any tariff data from {filePath}.");
			}
			else
			{
				dataRepo.AddRange(tariffs);
			}

			return result;
		}

		static void FillTariffData(RefCusTariff tariff, string tariffCode, DateTime startDate, DateTime endDate, string[] tariffDetails, Dictionary<string, CodeDescription> descriptionOverrides)
		{
			tariff.ZZ1_TariffCode = tariffCode;
			tariff.ZZ1_StartDate = startDate;
			tariff.ZZ1_EndDate = endDate;
			tariff.ZZ1_Description = descriptionOverrides.TryGetValue(tariffCode, out var lookupItem) ? lookupItem.Description : tariffDetails[14];

			var refCusTariffUOMs = new List<RefCusTariffUOM>();
			AddTariffUOM(refCusTariffUOMs, Constants.TariffUOMTypes.CU1, tariffDetails[7]);
			AddTariffUOM(refCusTariffUOMs, Constants.TariffUOMTypes.CU2, tariffDetails[8]);
			tariff.RefCusTariffUOMs = refCusTariffUOMs.ToArray();
		}

		static void AddTariffUOM(List<RefCusTariffUOM> refCusTariffUOMs, string type, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				refCusTariffUOMs.Add(new RefCusTariffUOM
				{
					ZZ8_Type = type,
					ZZ8_UOM = value
				});
			}
		}
	}
}
