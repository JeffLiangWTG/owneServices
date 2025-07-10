using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public static class AppConfig
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.INReferenceData.CmdLine.config.json";

		public static class Shared
		{
			public static string OutputDirectory => Config["shared:outputDirectory"];
		}

		public static class EDILocation
		{
			public static string Url => Config["ediLocation:url"];
		}

		public static class ExchangeRate
		{
			public static string Url => Config["exchangeRate:url"];

			public static int SleepInterval => int.Parse(Config["exchangeRate:sleepInterval"], CultureInfo.InvariantCulture);

			public static int MaxRetry => int.Parse(Config["exchangeRate:maxRetry"], CultureInfo.InvariantCulture);
		}

		public static class Tariff
		{
			public static string BaseUrl => Config["tariff:baseUrl"];

			public static string ContentBaseUrl => Config["tariff:contentBaseUrl"];

			public static string TariffSubUrl => Config["tariff:tariffSubUrl"];

			public static string PartToDownload => Config["tariff:partToDownloadRegEx"];

			public static string WeightUnitsInDescriptionRegEx => Config["tariff:weightUnitsInDescriptionRegEx"];

			public static bool DownloadPdfFiles => bool.Parse(Config["tariff:downloadPdfFiles"]);

			public static bool ProcessPdfFiles => bool.Parse(Config["tariff:processPdfFiles"]);

			public static bool GenerateRefDataXml => bool.Parse(Config["tariff:generateRefDataXml"]);

			public static string IntermediateOutputDirectory => Config["tariff:intermediateOutputDirectory"];

			public static int SleepInterval => int.Parse(Config["tariff:sleepInterval"], CultureInfo.InvariantCulture);

			public static int MaxRetry => int.Parse(Config["tariff:maxRetry"], CultureInfo.InvariantCulture);

			public static Dictionary<string, string> UnitMapping => Config.GetSection("tariff:unitMapping").Get<Dictionary<string, string>>();
		}

		public static class ErrorCodes
		{
			public static string BaseUrl => Config["errorCodes:baseUrl"];

			public static string BeUrl => Config["errorCodes:beUrl"];

			public static string AirCgmUrl => Config["errorCodes:airCgmUrl"];

			public static string SeaCgmUrl => Config["errorCodes:seaCgmUrl"];

			public static string BeXpath => Config["errorCodes:beXpath"];

			public static string AirCgmXpath => Config["errorCodes:airCgmXpath"];

			public static string SeaCgmXpath => Config["errorCodes:seaCgmXpath"];

			public static int CodeSearchStart => int.Parse(Config["errorCodes:codeSearchStart"], CultureInfo.InvariantCulture);

			public static int CodeSearchEnd => int.Parse(Config["errorCodes:codeSearchEnd"], CultureInfo.InvariantCulture);

			public static int SleepInterval => int.Parse(Config["errorCodes:sleepInterval"], CultureInfo.InvariantCulture);

			public static int MaxRetry => int.Parse(Config["errorCodes:maxRetry"], CultureInfo.InvariantCulture);
		}

		public static class DBKTariff
		{
			public static Dictionary<string, string> UnitMapping => Config.GetSection("DBKTariff:unitMapping").Get<Dictionary<string, string>>();
		}

		public static class WarehouseCode
		{
			public static string CodeUrl => Config[Key(nameof(CodeUrl))];

			public static string CaptchaUrl => Config[Key(nameof(CaptchaUrl))];

			public static int SleepInterval => GetInt(Key(nameof(SleepInterval)));

			public static int MaxRetry => GetInt(Key(nameof(MaxRetry)));

			public static string TableXpath => Config[Key(nameof(TableXpath))];

			public static int CodeIndex => GetInt(Key(nameof(CodeIndex)));

			public static int NameIndex => GetInt(Key(nameof(NameIndex)));

			public static int AddressIndex => GetInt(Key(nameof(AddressIndex)));

			public static int LegalTableColumns => GetInt(Key(nameof(LegalTableColumns)));

			static string Key(string name) => $"{nameof(WarehouseCode)}:{name}";
		}

		static int GetInt(string key) => Config.GetValue<int>(key);

		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
				}
				return config;
			}
		}
		static IConfiguration config;
	}
}
