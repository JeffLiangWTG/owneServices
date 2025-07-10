using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CargoWise.RefDbRepo.CAReferenceData.Services
{
	public class WebSvcScraper
	{
		public WebSvcScraper(string apiURL, string queryType, Hashtable parameters, string acceptLanguage)
		{
			_apiURL = apiURL;
			_queryType = queryType;
			_parameters = parameters;
			_acceptLanguage = acceptLanguage;
		}
		readonly string _apiURL;
		readonly string _queryType;
		readonly string _acceptLanguage;
		readonly Hashtable _parameters;

		HttpWebRequest Request
		{
			get
			{
				if (_request == null)
				{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
#pragma warning disable CA2234 // Pass system uri objects instead of strings
					_request = (HttpWebRequest)WebRequest.Create(_apiURL + "/" + _queryType + "?" + WebScraper.ParsToString(_parameters));
#pragma warning restore CA2234 // Pass system uri objects instead of strings
#pragma warning restore SYSLIB0014 // Type or member is obsolete
					_request.Method = "GET";
					_request.Headers.Add("Accept-Language", _acceptLanguage);
					return _request;
				}
				return _request;
			}
		}

		HttpWebRequest _request;

		public async Task<int> QueryAndSaveXmlResponseFile(string outputPath, string xmlFileName)
		{
			var count = 0;
			XmlDocument doc = new XmlDocument();
			var xmlSavedFileFullPath = Path.Combine(outputPath, xmlFileName);
			try
			{
				if (!string.IsNullOrEmpty(xmlSavedFileFullPath))
				{
					if (File.Exists(xmlSavedFileFullPath))
					{
						File.Delete(xmlSavedFileFullPath);
					}
				}

				using (var response = await Request.GetResponseAsync())
				using (var responseStream = response.GetResponseStream())
				using (var reader = new StreamReader(responseStream, Encoding.UTF8))
				{
					var retXml = await reader.ReadToEndAsync();
					doc.LoadXml(retXml);
					count = doc.GetElementsByTagName("entry").Count;
					if (count > 0)
					{
						doc.Save(xmlSavedFileFullPath);
					}
				}
			}

			catch (ProtocolViolationException pEx)
			{
				AppendMessageWithRequestSnapshot(pEx.Message);
			}
			catch (WebException wEx)
			{
				var passException = false;
				if (wEx.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.Conflict)
				{
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						var responseText = await reader.ReadToEndAsync();
						passException = responseText.Contains(NoDataSelected);
					}
				}

				if (!passException)
				{
					AppendMessageWithRequestSnapshot(wEx.Message);
				}
			}
			catch (Exception ex)
			{
				AppendMessageWithRequestSnapshot(ex.Message);
			}
			return count;
		}

		public async Task<XmlDocument> QueryAndReadXmlResponse()
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				using (var response = await Request.GetResponseAsync())
				using (var responseStream = response.GetResponseStream())
				using (var reader = new StreamReader(responseStream, Encoding.UTF8))
				{
					var retXml = await reader.ReadToEndAsync();
					doc.LoadXml(retXml);
				}
			}
			catch (ProtocolViolationException pEx)
			{
				AppendMessageWithRequestSnapshot(pEx.Message);
			}
			catch (WebException wEx)
			{
				AppendMessageWithRequestSnapshot(wEx.Message);
			}
			catch (Exception ex)
			{
				AppendMessageWithRequestSnapshot(ex.Message);
			}
			return doc;
		}

		public bool HasErrorNotification => ErrorBuilder.Length > 0;

		void AppendMessageWithRequestSnapshot(string message)
		{
			ErrorBuilder.AppendLine(message);
			ErrorBuilder.Append(Environment.NewLine + "Request Snapshot: ");
			ErrorBuilder.Append(Environment.NewLine + "URL: " + Request.RequestUri);
			ErrorBuilder.Append(Environment.NewLine + "Method: " + Request.Method);
			ErrorBuilder.Append(Environment.NewLine + "Headers: ");
			foreach (string header in Request.Headers)
			{
				ErrorBuilder.Append(header + ": " + Request.Headers[header] + ";");
			}
		}

		public string GetErrorNotification()
		{
			return ErrorBuilder.ToString();
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
		const string NoDataSelected = "<message>No Data Selected</message>";
	}
}
