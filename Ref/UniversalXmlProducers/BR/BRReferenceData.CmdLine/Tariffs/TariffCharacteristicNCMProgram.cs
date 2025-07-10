using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffCharacteristicNCMProgram : GenericCheckUpdateProgram<string>
	{
		public TariffCharacteristicNCMProgram(bool isProduction, bool isExpOnly)
		{
			this.isProduction = isProduction;
			this.isExpOnly = isExpOnly;
		}
		readonly bool isProduction;
		readonly bool isExpOnly;

		string CharacteristicType => isProduction ? Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NCM : Constants.RefCusTariffBRCharacteristicCodes.CharacteristicTypes.NCMTE;

		protected override string DataSourceFriendlyName => isProduction ? "Customs Tariff NCM Attribute file" : "Customs Tariff NCM Test Attribute file";

		protected override string LogFileSuffix => $"{(isProduction ? Constants.TariffBRCharacteristicNcmLogName : Constants.TariffBRCharacteristicNcmTeLogName)}_{(isExpOnly ? "EXP" : "ALL")}";

		protected override void ExportToXMLFile(string bFile)
		{
			using (var client = GetHttpClient())
			{
				var attributeDownloader = new TariffCharacteristicAttributesDownloader(isProduction);
				var tariffDownloader = new TariffCharacteristicNCMDownloader(isProduction);
				var attributes = attributeDownloader.Download(client);
				var ncms = tariffDownloader.Download(client, isExpOnly);

				if (!attributes.Attributes.Any() || !ncms.Ncms.Any())
				{
					throw new InvalidOperationException($"The application was unable to download {DataSourceFriendlyName}.");
				}

				var outputFileName = GetOutputFilePath($"RefCusTariffBRCharacteristic_BR_{CharacteristicType}_{(isExpOnly ? "EXP" : "ALL")}.xml");
				var parser = new TariffCharacteristicNCMParser(isProduction ? "BR Tariff Characteristic NCM" : "BR Tariff Characteristic NCM Test");
				parser.ExportToXMLFile(attributes.Attributes, ncms.Ncms, outputFileName, DateTime.Now, CharacteristicType);
			}
		}

		protected override string DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				return new TariffCharacteristicNCMVersionDownloader(isProduction).GetLastVersion(client);
			}
		}

		protected override void CheckDowloadContent(string downloadedContent)
		{
			if (string.IsNullOrEmpty(downloadedContent))
			{
				throw new InvalidOperationException($"The application was unable to verify the last version {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(string downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != downloadedContent;
		}

		public override void UpdateLogFile(string downloadedContent)
		{
			File.WriteAllText(LogFilePath, downloadedContent);
		}
	}
}
