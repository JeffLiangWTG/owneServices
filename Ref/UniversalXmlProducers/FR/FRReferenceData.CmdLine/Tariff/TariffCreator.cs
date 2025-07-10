using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FRReferenceData.Business;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class TariffCreator : BaseTariff
	{
		public async static Task<bool> Run(bool forceDownload)
		{
			var provider = new RITADataProvider(new RITADataDownloader());
			var error = Errors.No;

			error = await DownloadAndGenerateURDFiles(provider, forceDownload, error);

			Resumer.WriteOutStatus(Resumer.DoNotResume);
			return ExitProgram(error);
		}

		public static async Task<Errors> DownloadAndGenerateURDFiles(RITADataProvider provider, bool forceDownload, Errors error)
		{
			var processStartDate = DateTime.Today;

			Console.WriteLine("Downloading whole data set. This can take up to 8 days...");
			Console.WriteLine("Building complete tariff list...");

			var euDeclarableTariffs = provider.GetEUDeclarableTariffsThatDay(DateTime.Today);

			var tariffListToProcess = new List<string>();

			var resumeStatus = Resumer.ParseStatus();

			var filePresenceToCheck = Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "*_MI.XML");

			if (resumeStatus != Resumer.ResumeGeneration)
			{
				bool lastDownloadedTariffHasBeenReached = resumeStatus == Resumer.DoNotResume;
				var lastDownloadedTariff = Resumer.GetTariffFromStatus(resumeStatus);
				var downloadIssuesToReport = string.Empty;

				foreach (var euDeclarableTariff in euDeclarableTariffs)
				{
					if (!lastDownloadedTariffHasBeenReached)
					{
						if (resumeStatus.Contains(Resumer.ResumeDownload) && euDeclarableTariff == lastDownloadedTariff)
						{
							lastDownloadedTariffHasBeenReached = true;
						}
						tariffListToProcess.Add(euDeclarableTariff);
						continue;
					}

					Resumer.WriteOutStatus($"{Resumer.ResumeDownload} {euDeclarableTariff}");

					if (forceDownload || !File.Exists($@"{filePresenceToCheck.Replace("*", euDeclarableTariff)}"))
					{
						try
						{
							await provider.DownloadFRTariffRegulation(euDeclarableTariff);
						}
						catch (RITAWebServiceException e)
						{
							Console.WriteLine($"{e.Message}, {euDeclarableTariff} tariff skipped.");
							downloadIssuesToReport += "${tariff} skipped: {e.Message}\r\n";

							continue;
						}

						Console.WriteLine("{0} tariff data downloaded from FR Customs", euDeclarableTariff);
					}
					else
					{
						Console.WriteLine("{0} tariff data won't be downloaded again.", euDeclarableTariff);
					}


					tariffListToProcess.Add(euDeclarableTariff);
				}

				if (!string.IsNullOrEmpty(downloadIssuesToReport))
				{
					var exception = new RITAWebServiceException(downloadIssuesToReport);
					Console.Error.WriteLine(exception);
				}

				if (tariffListToProcess.Count == 0)
				{
					throw new RITAWebServiceException("No tariff data could be downloaded from FR Customs. End of process");
				}
			}
			else
			{
				foreach (var tariff in euDeclarableTariffs)
				{
					if (File.Exists($@"{filePresenceToCheck.Replace("*", tariff)}"))
					{
						tariffListToProcess.Add(tariff);
					}
				}

				if (tariffListToProcess.Count != euDeclarableTariffs.Count)
				{
					var missingFRTariffs = euDeclarableTariffs.Except(tariffListToProcess).OrderBy(x => x).Take(100);
					var currentProgram = forceDownload ? Constants.ProgramFunctions.TariffCreation : Constants.ProgramFunctions.TariffRegen;
					var errorMessage = string.Join(Environment.NewLine,
						$"FR Reference Data {currentProgram} program failed when generating output files.",
						"Not all required raw data files from Customs were available in download directory.",
						"Here are the missing tariffs (only the first 100 items are shown):",
						string.Join(",", missingFRTariffs),
						"You should consider rescheduling the program again.");
					Console.WriteLine(errorMessage);
					UniversalDataHelper.SendEmail($"FR Reference Data {currentProgram} program failed.", errorMessage);
					return Errors.No;
				}
			}

			if (error != Errors.DownloadErr)
			{
				Resumer.WriteOutStatus(Resumer.ResumeGeneration);

				GenerateURDFiles(tariffListToProcess, UpdateType.Full, DateTime.Now);
				Resumer.StoreProcessStartDate(processStartDate);
			}

			return error;
		}
	}
}

