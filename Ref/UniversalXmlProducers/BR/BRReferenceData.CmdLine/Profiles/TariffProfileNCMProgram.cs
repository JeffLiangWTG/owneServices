using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffProfileNCMProgram : GenericCheckUpdateProgram<string>
	{
		public TariffProfileNCMProgram(bool isProduction)
		{
			this.isProduction = isProduction;
		}
		readonly bool isProduction;

		string ProfileType => isProduction ? Constants.ProfileTypes.Codes.Tariff : Constants.ProfileTypes.Codes.TariffTest;

		string AddTest => isProduction ? string.Empty : " Test";

		protected override string DataSourceFriendlyName => "BR Tariff Attributes" + AddTest;

		protected override string LogFileSuffix => $"{(isProduction ? Constants.AttributesLogName : Constants.AttributesTestLogName)}";

		protected override void ExportToXMLFile(string bFile)
		{
			using (var client = GetHttpClient())
			{
				var attributeDownloader = new TariffCharacteristicAttributesDownloader(isProduction);
				var tariffDownloader = new TariffCharacteristicNCMDownloader(isProduction);
				var attributes = attributeDownloader.Download(client);
				var ncms = tariffDownloader.Download(client, false);

				var tariffAttributesDTO = new TariffAttributesDTO() { Attributes = attributes.Attributes, Tariffs = ncms.Ncms };

				var parser = new TariffProfileNCMParser(DataSourceFriendlyName, isProduction, profileDate: GetDateFromFileName(ncms.Filename), questionDate: GetDateFromFileName(attributes.Filename));
				parser.ExportToXMLFile(tariffAttributesDTO,
					outputFileName: GetOutputFilePath($"RefCusProfile_BR_{ProfileType}.xml"),
					outputTypeFileName: GetOutputFilePath($"RefCusProfileType_BR_{ProfileType}.xml"),
					outputQuestionFileName: GetOutputFilePath($"RefCusProfileQuestion_BR_{ProfileType}.xml"),
					outputQuestionPathwayFileName: GetOutputFilePath($"RefCusProfileQuestionPathway_BR_{ProfileType}.xml")
				);
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

		protected override bool CompareDownloadedContentWithLog(string downloadedContent) => true;

		public override void UpdateLogFile(string downloadedContent)
		{
			File.WriteAllText(LogFilePath, downloadedContent);
		}

		public static DateTime GetDateFromFileName(string filename)
		{
			var dateString = filename.Replace(".json", string.Empty);
			if (dateString.Length > 10)
			{
				dateString = dateString.Substring(dateString.Length - 10, 10);
			}
			if (DateTime.TryParseExact(dateString, "yyyy_MM_dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
			{
				return result;
			}
			throw new InvalidOperationException($"Filename '{filename}' doesn't contain a valid date.");
		}
	}
}
