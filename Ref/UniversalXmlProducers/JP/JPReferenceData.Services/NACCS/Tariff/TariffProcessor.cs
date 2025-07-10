using System;
using System.Net.Http;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class TariffProcessor
	{
		public static void WriteXml(bool isImport)
		{
			using (var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromSeconds(60), PooledConnectionIdleTimeout = TimeSpan.FromSeconds(1), MaxConnectionsPerServer = 256 })
			using (var sourceProvider = new WebSourceProvider(handler, true))
			{
				var dowloader = new TariffDownloader(sourceProvider);

				var publishDateText = isImport ? dowloader.DownloadImportTariffData() : dowloader.DownloadExportTariffData();
				if (DateTime.TryParseExact(publishDateText, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var publishDate))
				{
					BuildXml(publishDate, isImport);
				}
			}
		}

		static void BuildXml(DateTime publishDate, bool isImport)
		{
			var parsers = new Parser[]
			{
				new NomenclatureParser(publishDate, isImport), new TariffParser(new HttpClientHelper(), publishDate, isImport)
			};

			foreach (var parser in parsers)
			{
				parser.Parse();
			}
		}
	}
}
