using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
{
	sealed class TariffProgram : NZProgram
	{
		public override string ProgramName => "Tariff";

		protected override void RunCore()
		{
			var downloadInfos = new (string DirPrefix, string Url)[]
			{
				("Tariff", ApplicationConfig.TariffDataListUrl),
				("Concession", ApplicationConfig.ConcessionDataListUrl)
			};

			var tempDir = Path.Combine(Path.GetTempPath(), "NZ" + nameof(TariffProgram));

			using (var client = new HttpClient())
			{
				var downloader = new TariffListFileDownloader();
				foreach (var downloadInfo in downloadInfos)
				{
					var destDir = Path.Combine(tempDir, downloadInfo.DirPrefix);
					Directory.CreateDirectory(destDir);

					downloader.Download(client, downloadInfo.Url, destDir);
				}
			}

			var processingDataManager = new ProcessingDataManager<NZTariffProcessingData>(Path.Combine(Services.ApplicationConfig.OutputDirectory, "NZCustomsProcessingData"), "NZTariffProcessingData.json");
			var parser = new TariffParser(Logger);
			var parseResult = parser.Parse(tempDir, new DateProvider(), processingDataManager.ProcessingData);

			if (parseResult)
			{
				parser.ExportToXMLFile(ApplicationConfig.OutputDirectory);
				processingDataManager.SaveData();
				Directory.Delete(tempDir, true);
			}
			Logger.LogInfo($"NZ Tariff data process completed.");
		}
	}
}
