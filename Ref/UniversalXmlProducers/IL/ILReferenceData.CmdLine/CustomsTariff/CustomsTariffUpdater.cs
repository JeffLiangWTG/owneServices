using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using Environment = System.Environment;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	public class CustomsTariffUpdater
	{
		public static bool Run(ILogger logger, string[] cmdLineArguments)
		{
			if (cmdLineArguments.Length < 2)
			{
				logger.Log(LogType.Error, "Invalid command line arguments. Path to the folder with Customs Tariff files not provided.");
				Environment.Exit((int)Errors.BadCommandLineArguments);
				return false;
			}

			var customsTariffFolder = cmdLineArguments[1];
			if (!Directory.Exists(customsTariffFolder))
			{
				logger.Log(LogType.Error, $"Folder '{customsTariffFolder}' does not exist.");
				Environment.Exit((int)Errors.PathNotFound);
				return false;
			}

			var customsTariffUpdater = new CustomsTariffUpdater();
			customsTariffUpdater.Run(customsTariffFolder, logger);

			return true;
		}

		void Run(string customsTariffFolder, ILogger logger)
		{
			LoadCustomsTariffFiles(customsTariffFolder, logger);

			var customsTariffProcessor = new ILCustomsTariffProcessor(logger);
			customsTariffProcessor.GenerateFiles(DateTimeUtil.GetNow, customsItemComputedDataDict, propertiesDetailsHistoryDict, customsItemDetailsHistoryDict);
		}

		void LoadCustomsTariffFiles(string customsTariffFolder, ILogger logger)
		{
			logger.Log(LogType.Information, $"Loading Customs Tariff files from '{customsTariffFolder}'...");

			var xmlFiles = Directory.GetFiles(customsTariffFolder, "*.xml");

			foreach (string file in xmlFiles)
			{
				logger.Log(LogType.Information, $"Processing file {Path.GetFileName(file)}...");

				try
				{
					CustomsBookOut deserializedObject = DeserializeXml(file);
					if (deserializedObject.CustomsBookGeneralTables != null)
					{
						if (deserializedObject.CustomsBookGeneralTables.CustomsItemComputedData != null)
						{
							foreach (var item in deserializedObject.CustomsBookGeneralTables.CustomsItemComputedData)
							{
								customsItemComputedDataDict[$"{item.BaseFullClassification}|{item.CustomsBookTypeIDNum}"] = item;
							}
						}
						if (deserializedObject.CustomsBookGeneralTables.PropertiesDetailsHistory != null)
						{
							foreach (var item in deserializedObject.CustomsBookGeneralTables.PropertiesDetailsHistory)
							{
								propertiesDetailsHistoryDict[item.ID] = item;
							}
						}
						if (deserializedObject.CustomsBookGeneralTables.CustomsItemDetailsHistory != null)
						{
							foreach (var item in deserializedObject.CustomsBookGeneralTables.CustomsItemDetailsHistory)
							{
								customsItemDetailsHistoryDict[item.ID] = item;
							}
						}
					}
				}
				catch (InvalidOperationException ex)
				{
					logger.Log(LogType.Error, $"Error processing file {Path.GetFileName(file)}: {ex.Message}");
				}
			}

			logger.Log(LogType.Information, "Loading Customs Tariff files done.");
		}

		static CustomsBookOut DeserializeXml(string filePath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(CustomsBookOut));

			using (FileStream fs = new FileStream(filePath, FileMode.Open))
			using (XmlReader reader = XmlReader.Create(fs))
			{
				return (CustomsBookOut)serializer.Deserialize(reader);
			}
		}

		readonly Dictionary<string, CustomsItemComputedData> customsItemComputedDataDict = new Dictionary<string, CustomsItemComputedData>();
		readonly Dictionary<int, PropertiesDetailsHistory> propertiesDetailsHistoryDict = new Dictionary<int, PropertiesDetailsHistory>();
		readonly Dictionary<int, CustomsItemDetailsHistory> customsItemDetailsHistoryDict = new Dictionary<int, CustomsItemDetailsHistory>();
	}
}
