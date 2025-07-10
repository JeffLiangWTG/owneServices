using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class DispatchInstructionDocumentsProgram : GenericCheckUpdateProgram<IEnumerable<DossierDataDTO>>
	{
		public DispatchInstructionDocumentsProgram(bool isProduction)
		{
			this.isProduction = isProduction;
		}
		readonly bool isProduction;

		string TariffType => isProduction ? Constants.TariffTypes.Codes.EADOC : Constants.TariffTypes.Codes.EADTE;

		string ProfileType => isProduction ? Constants.ProfileTypes.Codes.DispatchInstructionDocument : Constants.ProfileTypes.Codes.DispatchInstructionDocumentTest;

		protected override string DataSourceFriendlyName => "BR Dispatch Instruction Documents" + (isProduction ? string.Empty : " Test");

		protected override string LogFileSuffix => $"{(isProduction ? Constants.DispatchInstructionDocumentLogName : Constants.DispatchInstructionDocumentTestLogName)}";

		protected override IEnumerable<DossierDataDTO> DowloadContent()
		{
			using (var handler = new HttpClientHandler
			{
				ClientCertificateOptions = ClientCertificateOption.Manual,
				SslProtocols = SslProtocols.None,
				CheckCertificateRevocationList = true
			})
			{
				return new DocumentTypeDownloader().Download(GetHttpClient(handler), isProduction);
			}
		}

		protected override HttpClient GetHttpClient(HttpClientHandler handler) => HttpClientHelper.GetHttpClientWithCertificate(handler, isProduction);

		protected override void ExportToXMLFile(IEnumerable<DossierDataDTO> bFile)
		{
			var parser = new DispatchInstructionDocumentsParser(DataSourceFriendlyName, isProduction);
			parser.ExportToXMLFile(bFile,
				outputFileName: GetOutputFilePath($"RefCusProfile_BR_{ProfileType}.xml"),
				outputTypeFileName: GetOutputFilePath($"RefCusProfileType_BR_{ProfileType}.xml"),
				outputQuestionFileName: GetOutputFilePath($"RefCusProfileQuestion_BR_{ProfileType}.xml"),
				outputTariffFileName: GetOutputFilePath($"RefCusTariff_BR_{TariffType}.xml"),
				outputTariffTypeFileName: GetOutputFilePath($"RefCusTariffType_BR_{TariffType}.xml")
			);
		}

		protected override void CheckDowloadContent(IEnumerable<DossierDataDTO> downloadedContent)
		{
			if (!downloadedContent.Any())
			{
				throw new InvalidOperationException($"The application was unable to verify the last version {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(IEnumerable<DossierDataDTO> downloadedContent)
		{
			return File.ReadAllText(LogFilePath) != ObjectToJson(downloadedContent);
		}

		public override void UpdateLogFile(IEnumerable<DossierDataDTO> downloadedContent)
		{
			File.WriteAllText(LogFilePath, ObjectToJson(downloadedContent));
		}

		static string ObjectToJson(IEnumerable<DossierDataDTO> dTOs) => JsonConvert.SerializeObject(dTOs);
	}
}
