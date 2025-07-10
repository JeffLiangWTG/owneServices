using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Common
{
	public class HttpLoader : TextLoader
	{
		public HttpLoader(Uri uri, HttpClient httpClient = null) : base(uri)
		{
			this.httpClient = httpClient ?? new HttpClient { Timeout = new TimeSpan(0, 10, 0) };
			GetResponse = Task.Run(() => this.httpClient.GetAsync(uri));
		}

		public async override Task<string> LoadAsync()
		{
			string result = null;

			var attempts = 0;
			try
			{
				while (attempts++ < 3)
				{
					if (await GetResponse is HttpResponseMessage responseMessage && responseMessage.IsSuccessStatusCode)
					{
						result = await responseMessage.Content.ReadAsStringAsync();
						break;
					}
				}
			}
			catch (TaskCanceledException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
			}
			catch (HttpRequestException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
			}

			return result;
		}

		public override void Dispose()
		{
			httpClient.Dispose();
		}

		readonly HttpClient httpClient;
		readonly Task<HttpResponseMessage> GetResponse;
	}
}
