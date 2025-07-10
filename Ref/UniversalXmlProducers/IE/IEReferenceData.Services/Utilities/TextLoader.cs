using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public abstract class TextLoader : IDisposable
	{
		public static TextLoader New(Uri uri, string userAgent = "")
		{
			var schema = uri.Scheme;
			TextLoader result;
			if (schema == Uri.UriSchemeHttp || schema == Uri.UriSchemeHttps)
			{
				result = new HttpLoader(uri, userAgent);
			}
			else if (schema == Uri.UriSchemeFile)
			{
				result = new LocalFileLoader(uri);
			}
			else
			{
				throw new InvalidOperationException($@"URI: ""{uri.OriginalString} "" is not supported yet. Please either add a support or use another type of URI.");
			}
			return result;
		}

		TextLoader(Uri uri) => Uri = uri;

		public abstract Task<string> LoadAsync();

		public abstract void Dispose();

		protected Uri Uri { get; }

		class HttpLoader : TextLoader
		{
			public HttpLoader(Uri uri, string userAgent) : base(uri)
			{
				if (ApplicationConfig.Instance.IsRefDbServiceSecure)
				{
					OriginalSecurityProtocol = ServicePointManager.SecurityProtocol;
					OriginalUseNagleAlgorithm = ServicePointManager.UseNagleAlgorithm;
				}
				httpClient = new HttpClient { Timeout = new TimeSpan(0, 10, 0) };
				httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

				if (!string.IsNullOrEmpty(userAgent))
				{
					httpRequestMessage.Headers.UserAgent.ParseAdd(userAgent);
				}

				GetResponse = Task.Run(() => httpClient.SendAsync(httpRequestMessage));
			}
			Task<HttpResponseMessage> GetResponse;

			public async override Task<string> LoadAsync()
			{
				string result = null;

				HttpResponseMessage responseMessage = null;
				var attempts = 0;
				while (attempts++ < 3)
				{
					if ((responseMessage = await GetResponse) is HttpResponseMessage)
					{
						if (responseMessage.IsSuccessStatusCode)
						{
							result = await responseMessage.Content.ReadAsStringAsync();
							break;
						}
					}
				}
				if (attempts == 3 && responseMessage != null)
				{
					var errorMessage = $"Http request {httpRequestMessage.RequestUri} failed after 3 attempts: {(int)responseMessage.StatusCode} - {responseMessage.StatusCode}";
					Console.Error.WriteLine(errorMessage);
					throw new HttpRequestException(errorMessage);
				}

				return result;
			}

			public override void Dispose()
			{
				ServicePointManager.UseNagleAlgorithm = OriginalUseNagleAlgorithm;
				ServicePointManager.SecurityProtocol = OriginalSecurityProtocol;
				httpClient.Dispose();
			}

			HttpClient httpClient { get; }
			HttpRequestMessage httpRequestMessage { get; }
			SecurityProtocolType OriginalSecurityProtocol { get; }
			bool OriginalUseNagleAlgorithm { get; }
		}

		class LocalFileLoader : TextLoader
		{
			public LocalFileLoader(Uri uri) : base(uri)
			{
				GetText = Task.Run(() => File.ReadAllText(Uri.LocalPath));
			}
			Task<string> GetText;

			public override async Task<string> LoadAsync()
			{
				try
				{
					return await GetText;
				}
				catch (IOException ex)
				{
					Console.Error.WriteLine(ex.Message);
					Console.Error.WriteLine(ex.StackTrace);
					return string.Empty;
				}
			}

			public override void Dispose() { }
		}
	}
}
