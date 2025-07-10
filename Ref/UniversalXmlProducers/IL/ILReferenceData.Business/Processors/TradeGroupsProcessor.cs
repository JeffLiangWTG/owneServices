using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class TradeGroupsProcessor
	{
		public static void GenerateFiles(DateTime publicationDate, ILogger logger)
		{
			if (!Directory.Exists(ApplicationConfig.Instance.DownloadsDirectory))
			{
				return;
			}

			var tradeGroupsResponseFiles = Directory.EnumerateFiles(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupsResponse*.xml");
			var tradeGroupCountriesResponseFiles = Directory.EnumerateFiles(ApplicationConfig.Instance.DownloadsDirectory, "IL_TradeGroupCountriesResponse*.xml");

			if (tradeGroupsResponseFiles.Any() || tradeGroupCountriesResponseFiles.Any())
			{
				logger.Log(LogType.Information, "Generating Trade Groups files...");

				try
				{
					if (!tradeGroupsResponseFiles.Any())
					{
						logger.Log(LogType.Error, "TradeGroups file not found");
						return;
					}

					if (!tradeGroupCountriesResponseFiles.Any())
					{
						logger.Log(LogType.Error, "TradeGroupCountries file not found");
						return;
					}

					if (tradeGroupsResponseFiles.Count() > 1)
					{
						logger.Log(LogType.Error, $"Multiple TradeGroups files were found");
						return;
					}

					if (tradeGroupCountriesResponseFiles.Count() > 1)
					{
						logger.Log(LogType.Error, $"Multiple TradeGroupCountries files were found");
						return;
					}

					var pathToTradeGroupsInputFile = tradeGroupsResponseFiles.Single();
					var pathToTradeGroupCountriesInputFile = tradeGroupCountriesResponseFiles.Single();
					var tradeGroupsDocument = XDocument.Parse(File.ReadAllText(pathToTradeGroupsInputFile));
					var tradeGroupCountriesDocument = XDocument.Parse(File.ReadAllText(pathToTradeGroupCountriesInputFile));

					List<RefCusTradeGroup> tradeGroups = new List<RefCusTradeGroup>();

					var countryGroupElements = tradeGroupCountriesDocument.Root.Elements("Table").ToList();

					foreach (var tradeGroupInput in tradeGroupsDocument.Root.Elements("Table"))
					{
						if (!ValidateMandatoryElements(tradeGroupInput, logger, "ID", "Name", "CountryGroupID"))
						{
							continue;
						}

						var countryGroupId = tradeGroupInput.Element("CountryGroupID").Value;
						var tradeGroupCountries = GetCusTradeGroupCountries(countryGroupId, countryGroupElements, logger);

						if (tradeGroupCountries.Length == 0)
						{
							logger.Log(LogType.Error, $"No TradeGroupCountry not found for CountryGroupID {countryGroupId}");
							continue;
						}

						var tradeGroup = new RefCusTradeGroup
						{
							ZZA_TradeGroup = tradeGroupInput.Element("ID")?.Value,
							ZZA_Description = tradeGroupInput.Element("Name")?.Value,
							RefCusTradeGroupCountries = tradeGroupCountries
						};

						tradeGroups.Add(tradeGroup);
					}

					var dataSource = UniversalDataHelper.Constants.ILTradeGroups;

					var outputPath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, $"{dataSource}.xml");
					RefDataRepoFileWriter<RefCusTradeGroup>.StoreDataCollection(dataSource, publicationDate, outputPath, GetXmlWriterConfiguration(dataSource), tradeGroups);

					logger.Log(LogType.Information, $"{dataSource} successfully saved to {outputPath}");
				}
				finally
				{
					CleanUpFiles(tradeGroupsResponseFiles, tradeGroupCountriesResponseFiles);
				}
			}
		}

		static void CleanUpFiles(IEnumerable<string> tradeGroupsResponseFiles, IEnumerable<string> tradeGroupCountriesResponseFiles)
		{
			foreach (var file in tradeGroupsResponseFiles)
			{
				File.Delete(file);
			}

			foreach (var file in tradeGroupCountriesResponseFiles)
			{
				File.Delete(file);
			}
		}

		static bool ValidateMandatoryElements(XElement element, ILogger logger, params string[] mandatoryChildren)
		{
			foreach (var mandatoryChild in mandatoryChildren)
			{
				if (element.Element(mandatoryChild) is null)
				{
					logger.Log(LogType.Error, $"TradeGroup {mandatoryChild} is null for {element}");
					return false;
				}
			}

			return true;
		}

		static RefCusTradeGroupCountry[] GetCusTradeGroupCountries(string countryGroupId, List<XElement> countryGroupElements, ILogger logger)
		{
			var elements = countryGroupElements.Where(x => x.Element("ExtraNumericData")?.Value == countryGroupId);
			var tradeGroupCountries = new List<RefCusTradeGroupCountry>();

			foreach (var element in elements)
			{
				if (!ValidateMandatoryElements(element, logger, "Name", "CountryAlphaCode_2"))
				{
					continue;
				}

				tradeGroupCountries.Add(new RefCusTradeGroupCountry
				{
					ZZB_Description = element.Element("Name").Value,
					ZZB_RN_NKTradeGroupCountryCode = element.Element("CountryAlphaCode_2").Value
				});
			}

			return tradeGroupCountries.ToArray();
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration(string dataSource)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tradeGroupConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_Description);
			tradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, false, UniversalDataHelper.Constants.Israel);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_StartDate, false, UniversalDataHelper.MinimumDateTime);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_EndDate, false, UniversalDataHelper.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupConfiguration);

			var tradeGroupCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_Description);
			tradeGroupCountryConfiguration.IncludeColumnWithConstantValue(x => x.ZZB_StartDate, false, UniversalDataHelper.MinimumDateTime);
			tradeGroupCountryConfiguration.IncludeColumnWithConstantValue(x => x.ZZB_EndDate, false, UniversalDataHelper.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountryConfiguration);

			return writerConfiguration;
		}
	}
}
