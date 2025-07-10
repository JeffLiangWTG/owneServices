using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface IDataLoader
	{
		Task<bool> LoadAsync(Uri baseUri);
	}

	public abstract class BaseDataLoader : IDataLoader
	{
		protected HttpClient HttpClient { get; }
		protected ILogger Logger { get; }

		protected BaseDataLoader(ILogger logger, HttpClient httpClient)
		{
			HttpClient = Argument.NotNull(httpClient, nameof(httpClient));
			Logger = Argument.NotNull(logger, nameof(logger));
		}

		public abstract Task<bool> LoadAsync(Uri baseUri);
	}
}
