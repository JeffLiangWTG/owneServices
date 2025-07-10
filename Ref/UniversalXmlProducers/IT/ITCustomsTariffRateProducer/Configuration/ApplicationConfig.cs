using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Configuration
{
	public static class ApplicationConfig
	{
		public static string SafeDbServiceUrl { get; private set; }
		public static string CustomsWebUrl { get; private set; }
		public static string CustomsWebUrlPublicationDate { get; private set; }
		public static bool SafeDbServiceUrlRequireSecureConnection { get; private set; }
		public static bool ITCustomsWebUrlRequireSecureConnection { get; private set; }
		public static string PartialUrdXmlStoragePath { get; private set; }
		public static string UrdXmlStoragePath { get; private set; }
		public static int BatchSize { get; private set; }
		public static int NoOfRecordsProcessed { get; private set; }
		public static string LastSyncXmlFilePath { get; private set; }
		public static int ConnectionLimit { get; private set; }
		public const string DataSource = "IT VAT and Excise";
		public const string DefaultEndDateString = "2079-06-06 23:59:00";

		public static void ConfigEnvironment()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile(ConfigurationJsonFileName)
				.Build();

			SafeDbServiceUrl = config["SafeDbServiceUrl"];
			CustomsWebUrl = config["ITCustomsWebUrl"];
			CustomsWebUrlPublicationDate = config["ITCustomsWebUrl_PublicationDate"];

			SafeDbServiceUrlRequireSecureConnection = IsSecureConnectionRequired("SafeDBServiceUrl_RequireSecureConnection", config);
			ITCustomsWebUrlRequireSecureConnection = IsSecureConnectionRequired("ITCustomsWebUrl_RequireSecureConnection", config);

			BatchSize = Convert.ToInt32(config["BatchSize"], CultureInfo.InvariantCulture);
			ConnectionLimit = Convert.ToInt32(config["ConnectionLimit"], CultureInfo.InvariantCulture);
			var noOfRecordsProcessed = Convert.ToInt32(config["NoOfRecordsProcessed"], CultureInfo.InvariantCulture);
			NoOfRecordsProcessed = noOfRecordsProcessed == 0 ? 1 : noOfRecordsProcessed;

			LastSyncXmlFilePath = SetFilePath("LastSyncXmlFilePath", config);
			PartialUrdXmlStoragePath = SetFilePath("PartialUniversalXmlStoragePath", config);
			UrdXmlStoragePath = SetFilePath("UniversalXmlStoragePath", config);
		}

		static string SetFilePath(string configKey, IConfigurationRoot config)
		{
			var path = config[configKey];

			return Path.GetFullPath(path);
		}

		static bool IsSecureConnectionRequired(string configurationKey, IConfigurationRoot config)
		{
			return bool.TryParse(config[configurationKey], out var secureConnectionRequired) && secureConnectionRequired;
		}

		internal static DateTime GetLastSyncTime()
		{
			var defaultSyncTime = new DateTime(1900, 1, 1, 00, 00, 00);

			if (!File.Exists(LastSyncXmlFilePath))
			{
				return defaultSyncTime;
			}

			var lastSyncXml = new XmlDocument();
			lastSyncXml.Load(LastSyncXmlFilePath);

			if (lastSyncXml.DocumentElement == null)
			{
				return defaultSyncTime;
			}

			var syncNode = lastSyncXml.SelectSingleNode("//LastSuccessfulSync");
			if (syncNode == null)
			{
				return defaultSyncTime;
			}

			return DateTime.TryParse(syncNode.InnerText, out var lastSyncTime)
				? lastSyncTime
				: defaultSyncTime;
		}

		internal static void SetLastSyncTime(DateTime lastSyncTime)
		{
			var lastSyncXml = new XmlDocument();

			if (!File.Exists(LastSyncXmlFilePath))
			{
				lastSyncXml.LoadXml("<LastSuccessfulSync></LastSuccessfulSync>");
			}
			else
			{
				lastSyncXml.Load(LastSyncXmlFilePath);
			}

			var syncNode = lastSyncXml.SelectSingleNode("//LastSuccessfulSync");
			if (syncNode != null)
			{
				syncNode.InnerText = lastSyncTime.ToString("s");
			}

			lastSyncXml.Save(LastSyncXmlFilePath);
		}

		const string ConfigurationJsonFileName = "CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.config.json";
	}
}
