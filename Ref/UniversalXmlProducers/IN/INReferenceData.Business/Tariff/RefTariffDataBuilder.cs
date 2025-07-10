using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class RefTariffDataBuilder : IRefTariffDataBuilder
	{
		public RefTariffDataBuilder(ILogger logger)
		{
			this.logger = logger;
		}

		public Dictionary<string, List<RefCusTariff>> GetRefData(string jsonFolder)
		{
			var tariffDictionary = new Dictionary<string, List<RefCusTariff>>();

			var folderInfo = new DirectoryInfo(jsonFolder);
			if (folderInfo.Exists)
			{
				var jsonFiles = folderInfo.GetFiles("*.json");
				foreach (var item in jsonFiles)
				{
					try
					{
						var result = ParseRefData(item.FullName);
						if (result.Count == 0)
						{
							logger.Log(LogType.ReviewRequired, $"No data found in JSON file {item.FullName}");
						}
						else
						{
							var chapterNumber = result.GroupBy(x => x.ZZ1_TariffCode[..2]).OrderByDescending(g => g.Count()).First().Key;
							if (tariffDictionary.TryGetValue(chapterNumber, out var tariffData))
							{
								tariffData.AddRange(result);
							}
							else
							{
								tariffDictionary[chapterNumber] = result;
							}
						}
					}
					catch (UnhandledApplicationException ex)
					{
						logger.Log(LogType.ReviewRequired, $"Skipping the file {item.FullName}, because ", ex);
					}
				}
				if (tariffDictionary.Count > 0)
				{
					Validator.ValidateChapter(tariffDictionary);
				}
			}
			else
			{
				logger.Log(LogType.ReviewRequired, $"Folder {jsonFolder} does not exist");
			}

			return tariffDictionary;
		}

		List<RefCusTariff> ParseRefData(string filePath)
		{
			try
			{
				var jsonText = File.ReadAllText(filePath);
				var tariffData = JsonSerializer.Deserialize<List<TariffDataItem>>(jsonText, Constants.Tariff.Processing.JsonSerializerOptions);
				return ParseRefDataCore(tariffData);
			}
			catch (Exception ex)
			{
				throw new UnhandledApplicationException($"Error parsing JSON file {filePath}", ex);
			}
		}

		List<RefCusTariff> ParseRefDataCore(List<TariffDataItem> tariffData)
		{
			var tariffDataItems = new List<RefCusTariff>();
			foreach (var row in tariffData)
			{
				var cleanTariffCode = Regex.Replace(row.TariffItem, @"[#*`\s-]", "");
				if (cleanTariffCode.Length != 8)
				{
					continue;
				}

				var item = new RefCusTariff
				{
					ZZ1_TariffCode = cleanTariffCode,
					ZZ1_Description = Regex.Replace(row.Description, @"\*|#(?!omitted\b)", "").Trim()
				};

				var uom = GetUomFromUnit(row);

				if (!string.IsNullOrEmpty(uom))
				{
					var tariffUom = new RefCusTariffUOM
					{
						ZZ8_UOM = uom,
					};
					item.RefCusTariffUOMs = new[] { tariffUom };
				}

				tariffDataItems.Add(item);
			}

			return tariffDataItems;
		}

		string GetUomFromUnit(TariffDataItem row)
		{
			var unit = row.Unit.Replace(" ", "").ToUpperInvariant();
			if (UnitMapping.TryGetValue(unit, out var uom))
			{
				return uom;
			}
			logger.Log(LogType.ReviewRequired, "Unknown Unit", row);
			return "";
		}

		Dictionary<string, string> UnitMapping => unitmapping ??= AppConfig.Tariff.UnitMapping;
		Dictionary<string, string> unitmapping;

		TariffDataValidator Validator => validator ?? (validator = new TariffDataValidator(logger));
		TariffDataValidator validator;

		readonly ILogger logger;
	}
}
