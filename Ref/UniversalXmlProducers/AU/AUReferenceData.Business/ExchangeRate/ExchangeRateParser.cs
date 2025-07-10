using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public abstract class ExchangeRateParser<T>
		where T : RefDataRepoModelEntityType, new()
	{
		public void Parse(string outputFilePath)
		{
			ErrorBuilder.Clear();

			var remoteFileUrl = GetSourceLocation(WebPageUrl);
			if (!string.IsNullOrEmpty(remoteFileUrl))
			{
				var publicationDate = GetPublicationDate(remoteFileUrl);
				var localFilePath = GetTempFileName();
				try
				{
					if (ReadFromResource(remoteFileUrl, localFilePath))
					{
						var entities = ConvertData(localFilePath);
						WriteToDestination(entities, outputFilePath, publicationDate);
					}
				}
				finally
				{
					File.Delete(localFilePath);
				}
			}
		}

		protected virtual string GetTempFileName()
		{
			return Path.GetTempFileName();
		}

		public virtual string GetSourceLocation(string webPageUrl)
		{
			string result = WithErrorReporting($"Error Retrieving Source from {webPageUrl}", () =>
			{
				return ExchangeRateFileUrlFinder.Find(GetHttpClientHelper(), webPageUrl, FileNamePrefix);
			});

			return result ?? string.Empty;
		}

		public virtual DateTime GetPublicationDate(string remoteFileUrl)
		{
			var matchResults = Regex.Matches(remoteFileUrl, PublicationDatePattern);
			var matchCount = matchResults.Count;
			var publicationTimeString = string.Empty;
			if (matchCount > 0)
			{
				publicationTimeString = matchResults[matchCount - 1].Value;
			}

			DateTime publicationTime;
			try
			{
				publicationTime = DateTime.ParseExact(publicationTimeString, "yyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
			}
			catch (FormatException formatException)
			{
				publicationTime = GetLastModifiedDate(remoteFileUrl);
				if (publicationTime == DateTime.MinValue)
				{
					throw new UnhandledApplicationException($"Invalid format of publication time string: {publicationTimeString}, remote file url: {remoteFileUrl}", formatException);
				}
			}
			return publicationTime > DateTime.Today ? publicationTime : DateTime.Today;
		}
		const string PublicationDatePattern = @"\d{10}";

		protected DateTime GetLastModifiedDate(string remoteFileUrl)
		{
			return WithErrorReporting($"Error Retrieving LastModifiedDate from {remoteFileUrl}", () =>
			{
				var lastModifiedDate = DateTime.MinValue;
				using (var client = GetHttpClient())
				using (var message = new HttpRequestMessage(HttpMethod.Head, new Uri(remoteFileUrl)))
				{
					var response = client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead).Result;
					var modifiedDate = response.Content.Headers.LastModified;
					if (modifiedDate.HasValue)
					{
						lastModifiedDate = modifiedDate.Value.UtcDateTime;
						lastModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(lastModifiedDate, TimeZoneInfo.FindSystemTimeZoneById(AUEastTimeZoneID));
					}
				}
				return lastModifiedDate;
			});
		}

		const string AUEastTimeZoneID = "AUS Eastern Standard Time";

		public virtual bool ReadFromResource(string remoteFileUrl, string localFilePath)
		{
			return WithErrorReporting($"Error Downloading from {remoteFileUrl}", () =>
			{
				ExchangeRateFileDownloader.Download(GetHttpClientHelper(), remoteFileUrl, localFilePath);
				return true;
			});
		}

		public virtual IEnumerable<T> ConvertData(string localFilePath)
		{
			return TextToEntitiesConverter<T>.Convert(localFilePath, Mappings).Where(Filter);
		}

		public virtual void WriteToDestination(IEnumerable<T> entities, string outputFilePath, DateTime publicationDate)
		{
			var outputFileFullPath = Path.Combine(outputFilePath, OutputFileName);
			Helper.ExportToXMLFile(XMLWriterDataSource, outputFileFullPath, Helper.GetExchangeRateWriterConfiguration(), publicationDate, UpdateType.Partial, entities);
		}

		protected virtual HttpClient GetHttpClient() => new HttpClient();

		protected virtual IHttpClientHelper GetHttpClientHelper() => new HttpClientHelper();

		protected abstract string WebPageUrl { get; }

		protected abstract string FileNamePrefix { get; }

		protected abstract IEnumerable<PropertyMapping<T>> Mappings { get; }

		protected abstract Func<T, bool> Filter { get; }

		protected abstract string XMLWriterDataSource { get; }

		public abstract string OutputFileName { get; }

		#region ErrorNotification

		private R WithErrorReporting<R>(string message, Func<R> func) // T is already in use.  R performs the same function.
		{
			R result = default(R);

			try
			{
				result = func.Invoke();
			}
			catch (Exception ex) 
			{
				var fullMessage = $"{message}: {ex.Message.Trim()}";

				if (ex is HttpRequestException
					|| ex is UriFormatException
					|| ex is InvalidOperationException)
				{
					ErrorBuilder.AppendLine(fullMessage);
				}
				else
				{
					throw new InvalidOperationException(fullMessage, ex);
				}
			}

			return result;
		}

		public bool HasErrorNotification => ErrorBuilder.Length > 0;
		public string GetErrorNotification()
		{
			return ErrorBuilder.ToString().Trim();
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		#endregion
	}
}
