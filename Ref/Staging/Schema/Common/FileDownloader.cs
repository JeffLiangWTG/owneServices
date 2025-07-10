using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public class FileDownloader : IFileDownloader
	{
		public FileDownloader(Uri uri, string userAgent = null, HttpClient httpClientOverride = null, HttpClientRetryHandler httpClientRetryHandlerOverride = null)
		{
			Argument.NotNull(uri, nameof(uri));

			this.uri = uri;
			this.userAgent = userAgent;
			client = httpClientOverride;
			retryHandler = httpClientRetryHandlerOverride;
		}

		public static string[] ExtractZipFile(string filePath, string destinationPath = "")
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));

			var result = new List<string>();
			if (Path.GetExtension(filePath) == ".zip")
			{
				var directory = string.IsNullOrEmpty(destinationPath) ? Path.GetFullPath(Path.GetDirectoryName(filePath)) : destinationPath;
				using (var zipFile = ZipFile.OpenRead(filePath))
				{
					foreach (var entry in zipFile.Entries)
					{
						if (string.IsNullOrEmpty(entry.Name))
						{
							continue;
						}

						var fileName = string.IsNullOrEmpty(destinationPath) ? entry.FullName : entry.Name;
						CreateDirectoryIfMissing(directory, fileName);
						var desFile = Path.GetFullPath(Path.Combine(directory, fileName));

						if (desFile.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
						{
							entry.ExtractToFile(desFile, true);
							result.Add(desFile);
						}
					}
				}
				return result.ToArray();
			}
			return new[] { filePath };
		}

		static void CreateDirectoryIfMissing(string directory, string zipFullNameWithDirectory)
		{
			Argument.NotNullOrEmpty(directory, nameof(directory));
			Argument.NotNullOrEmpty(zipFullNameWithDirectory, nameof(zipFullNameWithDirectory));

			var zipDirectories = Path.GetDirectoryName(zipFullNameWithDirectory);
			if (!string.IsNullOrEmpty(zipDirectories))
			{
				var newDir = Path.Combine(directory, zipDirectories);
				Directory.CreateDirectory(newDir);
			}
		}

		HttpRequestMessage CreateRequestMessage(HttpMethod method, AuthenticationHeaderValue authenticationHeaderValue = null)
		{
			var requestMessage = new HttpRequestMessage(method, uri);
			if (authenticationHeaderValue != null)
			{
				requestMessage.Headers.Authorization = authenticationHeaderValue;
			}
			if (!string.IsNullOrEmpty(userAgent))
			{
				requestMessage.Headers.TryAddWithoutValidation("User-Agent", userAgent);
			}
			return requestMessage;
		}

		public DateTime GetCreationTime(AuthenticationHeaderValue authenticationHeaderValue = null)
		{
			var responseMessage = GetResponseMessage(HttpMethod.Head, HttpCompletionOption.ResponseHeadersRead, authenticationHeaderValue);
			var result = DateTime.Now;
			if (responseMessage.Content.Headers.LastModified.HasValue)
			{
				result = responseMessage.Content.Headers.LastModified.Value.DateTime;
			}
			else
			{
				Console.WriteLine($"Unable to get last modified time from {uri}, set to DateTime.Now.");
			}
			return result;
		}

		public IResponseStream GetFileStream(AuthenticationHeaderValue authenticationHeaderValue = null)
		{
			return new ResponseStream(GetResponseMessage(HttpMethod.Get, authHeaderValue: authenticationHeaderValue));
		}

		public string[] SaveAndExtract(string pathToSave, bool copyAlways = false)
		{
			Argument.NotNullOrEmpty(pathToSave, nameof(pathToSave));
			var remoteLastModifiedTime = GetCreationTime();
			if (!copyAlways && File.Exists(pathToSave))
			{
				var localLastModifiedTime = File.GetLastWriteTime(pathToSave);
				if (remoteLastModifiedTime <= localLastModifiedTime)
				{
					return ExtractZipFile(pathToSave);
				}
			}

			var responseMessage = GetResponseMessage(HttpMethod.Get);
			using (var responseStream = responseMessage.Content.ReadAsStream())
			using (var fileStream = File.Create(pathToSave))
			{
				responseStream.CopyTo(fileStream);
			}
			File.SetLastWriteTime(pathToSave, remoteLastModifiedTime);
			return ExtractZipFile(pathToSave);
		}

		HttpResponseMessage GetResponseMessage(HttpMethod httpMethod, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead, AuthenticationHeaderValue authHeaderValue = null)
		{
			return RetryHandler.Execute(uri, () =>
			{
				using (var requestMessage = CreateRequestMessage(httpMethod, authHeaderValue))
				{
					return Client.Send(requestMessage, httpCompletionOption);
				}
			});
		}

		readonly Uri uri;
		readonly string userAgent;

		HttpClient Client => client ?? (client = DefaultClient);
		HttpClient client;
		static readonly HttpClient DefaultClient = new HttpClient();

		HttpClientRetryHandler RetryHandler => retryHandler ?? (retryHandler = DefaultRetryHandler);
		HttpClientRetryHandler retryHandler;
		static readonly HttpClientRetryHandler DefaultRetryHandler = new HttpClientRetryHandler();
	}
}
