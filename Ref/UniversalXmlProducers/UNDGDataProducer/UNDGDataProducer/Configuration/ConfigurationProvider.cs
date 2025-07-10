using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class ConfigurationProvider
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.config.json";

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFile).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string RefIATACsvFilePath => Configuration[nameof(RefIATACsvFilePath)];
		public static string RefIMOZipFilePath => Configuration[nameof(RefIMOZipFilePath)];
		public static string RefADRCsvFilePath => Configuration[nameof(RefADRCsvFilePath)];
		public static string RefRIDCsvFilePath => Configuration[nameof(RefRIDCsvFilePath)];
		public static string RefADNCsvFilePath => Configuration[nameof(RefADNCsvFilePath)];
		public static string RefCFRZipFilePath => Configuration[nameof(RefCFRZipFilePath)]; 
		public static string RefUNDGCountryReferenceNonPSACSVFilePath => Configuration[nameof(RefUNDGCountryReferenceNonPSACSVFilePath)];
		public static string RefUNDGCountryReferenceCSVFilePath => Configuration[nameof(RefUNDGCountryReferenceCSVFilePath)];
		public static string RefUNDGCountryReferencePivotCSVFilePath => Configuration[nameof(RefUNDGCountryReferencePivotCSVFilePath)];
		public static string RefJTTCsvFilePath => Configuration[nameof(RefJTTCsvFilePath)];
		public static string FilePathIATA => Configuration[nameof(FilePathIATA)];
		public static string FilePathIMO => Configuration[nameof(FilePathIMO)];
		public static string FilePathCommonData => Configuration[nameof(FilePathCommonData)];
		public static string FilePathADR => Configuration[nameof(FilePathADR)];
		public static string FilePathPSA => Configuration[nameof(FilePathPSA)];
		public static string FilePathRID => Configuration[nameof(FilePathRID)];
		public static string FilePathADN => Configuration[nameof(FilePathADN)];
		public static string FilePathCFR => Configuration[nameof(FilePathCFR)];
		public static string FilePathJTT => Configuration[nameof(FilePathJTT)];
		public static string FilePathCountryReference => Configuration[nameof(FilePathCountryReference)];
		public static string FilePathCountryReferencePSA => Configuration[nameof(FilePathCountryReferencePSA)];
		public static string ProxyUser => Configuration[nameof(ProxyUser)];
		public static string ProxyPassword => Configuration[nameof(ProxyPassword)];
	}
}
