using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CsvHelper;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class NexDocCodeSetRESTService<T>
		where T : CsvToItemCodeSetsConverter
	{
		public NexDocCodeSetRESTService(DateTime lastUpdated)
		{
			this.lastUpdated = lastUpdated;
		}
		readonly DateTime lastUpdated;

		public IEnumerable<IListCodeSet> GetCodeSet(string codeSetName)
		{
			IListCodeSet[] result = null;
			ErrorBuilder.Clear();
			try
			{
				var endPoint = ApplicationConfig.NexDocRESTReferenceDataEndPoint;
				var uri = new Uri($"{endPoint}/{codeSetName}{(lastUpdated == DateTime.MinValue ? "" : $"?lastUpdated={lastUpdated.ToString(DateFormat, null)}")}");

				using (var client = new HttpClient())
				using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
				{
					var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes(ApplicationConfig.NexDocReferenceDataUsername + ":" + ApplicationConfig.NexDocReferenceDataPassword));
					request.Headers.Add("Authorization", "Basic " + authHeader);
					using (var response = client.SendAsync(request).Result)
					using (var responseStream = response.Content.ReadAsStreamAsync().Result)
					{
						if (responseStream == null || responseStream.Length == 0)
						{
							ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"No data return for Code Set: {codeSetName}");
						}
						else
						{
							using (var reader = new StreamReader(responseStream))
							using (var csv = new CsvReader(reader))
							{
								var converter = new CsvToListCodeSetsConverter<T>();
								result = converter.Convert(csv).ToArray(); // Need to download all data before csv is disposed
								var errorNotification = converter.ErrorNotification;
								if (!string.IsNullOrEmpty(errorNotification))
								{
									ErrorBuilder.AppendLine(errorNotification);
								}
							}
						}
					}
				}
			}
			catch (ProtocolViolationException pEx)
			{
				ErrorBuilder.AppendLine(pEx.Message);
			}
			catch (WebException wEx)
			{
				ErrorBuilder.AppendLine(wEx.Message);
			}
			return result ?? Enumerable.Empty<IListCodeSet>();
		}
		const string DateFormat = "yyyy-MM-ddThh:MM:ss";

		public bool HasErrorNotification => ErrorBuilder.Length > 0;
		public string GetErrorNotification()
		{
			return ErrorBuilder.ToString();
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
