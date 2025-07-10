using System;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common
{
	public interface IWebScraper : IAsyncDisposable
	{
		Task<string> ScrapeWithRetriesAsync(Uri uri, int maxAttempts = Constants.WebSraper.DefaultMaxAttempts);
	}
}
