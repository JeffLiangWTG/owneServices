using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffRatesManager
	{
		Thread RunTariffRatesDownloadAsync(IEnumerable<string> tariffs, HttpClient clientLogged = null)
		{
			var thread = new Thread(() =>
			{
				using var client = clientLogged ?? HttpClient;
				new TariffRatesDownloader().DownloadRates(client, tariffs, SaveInMemory);
			});
			thread.Start();

			return thread;
		}

		public IEnumerable<TariffDTO> GetTariffsMultiThread(HttpClient client)
		{
			var tariffCodes = GetTariffCodes(client);
			var numberOfTariffsPerThread = tariffCodes.Count() > 5 ? tariffCodes.Count() / 5 : 1;
			var threads = new List<Thread>();
			var tempTariffCodes = new List<string>();
			var firstThread = true;
			foreach (var tariffCode in tariffCodes)
			{
				tempTariffCodes.Add(tariffCode);
				if (tempTariffCodes.Count == numberOfTariffsPerThread)
				{
					threads.Add(RunTariffRatesDownloadAsync([.. tempTariffCodes], firstThread ? client : null));
					firstThread = false;
					tempTariffCodes.Clear();
				}
			}

			threads.ForEach(t => t.Join());

			return DownloadedTariffs;
		}

		void SaveInMemory(IEnumerable<TariffDTO> tariffs)
		{
			lock (DownloadedTariffs)
			{
				DownloadedTariffs.AddRange(tariffs);
			}
		}


		List<TariffDTO> DownloadedTariffs => fDownloadedTariffs;
		readonly List<TariffDTO> fDownloadedTariffs = [];

		static IEnumerable<string> GetTariffCodes(HttpClient client) => new TariffsDownloader().Download(client);

		protected virtual HttpClient HttpClient => new(HttpHandler) { Timeout = TimeSpan.FromMinutes(5) };

		static SocketsHttpHandler HttpHandler => new() { UseCookies = true, };
	}
}
