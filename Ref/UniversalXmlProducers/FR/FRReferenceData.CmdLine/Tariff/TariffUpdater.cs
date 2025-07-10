using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public class TariffUpdater : BaseTariff
	{
		public async static Task<bool> Run()
		{
			var provider = new RITADataProvider(new RITADataDownloader());
			var error = Errors.No;

			error = await DownloadDataAndGenerateUDRFiles(provider, error);

			return ExitProgram(error);
		}

		static async Task<Errors> DownloadDataAndGenerateUDRFiles(RITADataProvider provider, Errors error)
		{
			var updateDay = Resumer.GetProcessStartDate();
			var finalEndDate = DateTime.Today.AddDays(-1);

			while (updateDay <= finalEndDate)
			{
				Console.WriteLine($"Getting updated tariffs list for {updateDay.ToShortDateString()}");
				var declarableEUTariffs = provider.GetEUDeclarableTariffsThatDay(updateDay);
				var updatedFRTariffs = provider.GetUpdatedOrNewFRTariffsThatDay(updateDay);

				if (!updatedFRTariffs.Any())
				{
					Console.WriteLine($"No update available from French Customs on {updateDay.ToShortDateString()}");
				}
				else
				{
					Console.WriteLine("Done.");
					var tariffListToProcess = new List<string>();
					Console.WriteLine("Downloading fresh tariff data...");
					var downloadIssuesToReport = string.Empty;
					foreach (var updatedFRTariff in updatedFRTariffs)
					{
						try
						{
							await provider.DownloadFRTariffRegulation(updatedFRTariff);
						}
						catch (RITAWebServiceException e)
						{
							Console.WriteLine($"{e.Message}, {updatedFRTariff} tariff skipped.");
							downloadIssuesToReport += "${tariff} skipped: {e.Message}\r\n";
							continue;
						}

						Console.WriteLine("{0} tariff data downloaded from Customs", updatedFRTariff);
						if (declarableEUTariffs.Contains(updatedFRTariff))
						{
							tariffListToProcess.Add(updatedFRTariff);
						}
					}

					if (!string.IsNullOrEmpty(downloadIssuesToReport))
					{
						var exception = new RITAWebServiceException(downloadIssuesToReport);
						Console.Error.WriteLine(exception);
					}

					if (tariffListToProcess.Count == 0)
					{
						throw new RITAWebServiceException("No tariff data could be downloaded from Customs. End of process");
					}
					else
					{
						Console.WriteLine("Done.");
						GenerateURDFiles(tariffListToProcess, UpdateType.Partial, updateDay);
					}
				}

				if (error == Errors.No)
				{
					updateDay = updateDay.AddDays(1);
					Resumer.StoreProcessStartDate(updateDay);
				}
			}

			return error;
		}
	}
}
