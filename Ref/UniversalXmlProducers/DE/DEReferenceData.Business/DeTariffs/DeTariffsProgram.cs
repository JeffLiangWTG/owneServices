using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.Business.DeTariffs;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine
{
	public class DeTariffsProgram
	{
		internal HttpMessageHandler HttpMessageHandler { get; set; } = new HttpClientHandler();

		public static async Task Run(string outputPath)
		{
			await new DeTariffsProgram().ConvertTariffs(outputPath);
		}

		internal async Task ConvertTariffs(string outputPath)
		{
			using var client = new HttpClient(HttpMessageHandler);
			AuthorizationHelper.SetDefaultBasicAuthorisationHeader(client, ApplicationConfig.DeTariffsClientId, ApplicationConfig.DeTariffsSecret);
			await new DeTariffsDownloader(client: client,
				baselineIndex: new Uri(ApplicationConfig.DeTariffsBaselineIndexURL),
				updateIndex: new Uri(ApplicationConfig.DeTariffsUpdateIndexURL),
				downloadDirectory: ApplicationConfig.DeTariffsArchiveCachePath,
				extractionDirectory: ApplicationConfig.DeTariffsExtractedCachePath).DownloadTariffs();

			new DeTariffParser(ApplicationConfig.DeTariffsExtractedCachePath, Path.Join(outputPath, "RefCusTariff_DE_IMP.xml")).ConvertToXMLFile();
		}
	}
}
