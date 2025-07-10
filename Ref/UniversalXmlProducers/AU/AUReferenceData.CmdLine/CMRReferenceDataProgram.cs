using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	public class CMRReferenceDataProgram
	{
		public void Run()
		{
			var runTime = RunTime;
			var clientHelper = HttpClient;

			foreach (var parser in RefDataParsers)
			{
				(var content, _) = CMRReferenceFileDownloader.Download(clientHelper, parser.FileNamePrefix, parser.IndexUri);

				parser.Parse(content, runTime, OutputDirectory);
			}

			Console.WriteLine($"End of {nameof(CMRReferenceDataProgram)}.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public void RunPartial()
		{
			var runTime = RunTime;
			var todaysDate = runTime.Date;
			var clientHelper = HttpClient;

			foreach (var parser in RefDataPartialParsers)
			{
				var parserName = parser.GetType().Name;
				var dataManager = new ProcessingDataManager<ExportTariffProcessingData>(OutputDirectory, parserName + "ProcessingData.json");
				var processingData = dataManager.ProcessingData;

				if (DateTime.TryParse(processingData.PartialParserLastRun, out var lastRunDateTime) && lastRunDateTime.Date == todaysDate)
				{
					Console.WriteLine($"{parserName} ran today at {lastRunDateTime}. Skipping partial update.");
					continue;
				}

				try
				{
					(var content, _) = CMRReferenceFileDownloader.Download(clientHelper, parser.FileNamePrefix, ApplicationConfig.AUReferenceFilesPartialDirectory);
					parser.Parse(content, todaysDate, OutputDirectory);

					processingData.PartialParserLastRun = runTime.ToString("s");
					dataManager.SaveData();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"{parserName} failed. {ex.Message}");
				}
			}

			Console.WriteLine($"End of {nameof(CMRReferenceDataProgram)}.");
		}

		protected virtual DateTime RunTime => DateTime.Now;
		protected virtual IHttpClientHelper HttpClient => new HttpClientHelper();
		protected virtual string OutputDirectory => ApplicationConfig.OutputDirectory;
		protected virtual IEnumerable<ICMRDataParser> RefDataParsers => CMRParserProvider.RefDataParsers;
		protected virtual IEnumerable<ICMRDataParser> RefDataPartialParsers => CMRParserProvider.RefDataPartialParsers;
	}
}
