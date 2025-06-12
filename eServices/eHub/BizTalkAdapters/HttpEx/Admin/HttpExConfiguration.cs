using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx.Admin
{
	public class HttpExConfiguration
	{
		public string DestinationUrl { get; set; }
		public string UniqueName { get; set; }
		public int Timeout { get; set; }
		public HttpMethod Method { get; set; }
		public MediaTypeHeaderValue ContentType { get; set; }
		public List<KeyValuePair<string, string>> CustomHeaders { get; set; }
		public List<string> CustomHeaderNamesWithoutValidation { get; set; }
		public List<HttpMethod> SuppressMessageBodyForHttpVerbs { get; set; }
		public string SuccessStatusRanges { get; set; }
		public string TerminateStatusRanges { get; set; }
		public List<Tuple<int, int, StatusType>> StatusRanges;
		public int LogMaxSize { get; set; }
		public int LogMaxCount { get; set; }
		public string LogLevel { get; set; }
		public UriBuilder LocationUri { get; set; }
		public string Certificate { get; set; }
		public string CertificatePassphrase { get; set; }
		public string LogFormat { get; set; }

		public static HttpExConfiguration Parse(XmlDocument configXml)
		{
			var config = new HttpExConfiguration();

			config.LocationUri = null;
			config.DestinationUrl = ConfigProperties.IfExistsExtract(configXml, "/Config/DestinationUrl", string.Empty);

			if (!string.IsNullOrWhiteSpace(config.DestinationUrl))
			{
				config.LocationUri = new UriBuilder(config.DestinationUrl);
			}

			config.UniqueName = ConfigProperties.IfExistsExtract(configXml, "/Config/UniqueName", string.Empty);
			if (!string.IsNullOrWhiteSpace(config.UniqueName))
			{
				if (config.LocationUri == null)
					config.LocationUri = new UriBuilder("httpex://" + config.UniqueName);
				else
					config.LocationUri.Fragment = config.UniqueName;
			}

			if (config.LocationUri == null)
				throw new AdapterException("Destination URL or Unique Name must be specified.");

			switch (config.LocationUri.Scheme)
			{
				case "http":
				case "httpex":
					config.LocationUri.Scheme = "httpex";
					break;
				case "https":
				case "httpsex":
					config.LocationUri.Scheme = "httpsex";
					break;
				default:
					throw new AdapterException("Address scheme '" + config.LocationUri.Scheme + "' is not valid.");
			}

			config.Timeout = ConfigProperties.ExtractInt(configXml, "/Config/Timeout");
			if (config.Timeout <= 0)
				throw new AdapterException("Timeout must be positive.");

			var method = ConfigProperties.Extract(configXml, "/Config/Method", null);
			if (string.IsNullOrWhiteSpace(method))
				throw new AdapterException("HTTP Method is required.");
			config.Method = new HttpMethod(method);

			var contentType = ConfigProperties.IfExistsExtract(configXml, "/Config/ContentType", null);
			if (!string.IsNullOrWhiteSpace(contentType))
				config.ContentType = ParseContentType(contentType);

			config.CustomHeaders = new List<KeyValuePair<string, string>>();
			var headersText = ConfigProperties.IfExistsExtract(configXml, "/Config/CustomHeaders", null);
			ParseHeaders(headersText, config);

			var noValidationCustomHeaderNamesText = ConfigProperties.IfExistsExtract(configXml, "/Config/CustomHeaderNamesWithoutValidation", null);

			config.CustomHeaderNamesWithoutValidation = string.IsNullOrEmpty(noValidationCustomHeaderNamesText)
				? new List<string>()
				: noValidationCustomHeaderNamesText
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(token => token.Trim())
					.ToList();

			config.SuppressMessageBodyForHttpVerbs
				= Regex.Split(ConfigProperties.IfExistsExtract(configXml, "/Config/SuppressMessageBodyForHttpVerbs", "GET, DELETE"), @"\W+")
					.Select(v => new HttpMethod(v)).ToList();

			config.SuccessStatusRanges = ConfigProperties.Extract(configXml, "/Config/Success", null);
			config.TerminateStatusRanges = ConfigProperties.IfExistsExtract(configXml, "/Config/Terminate", string.Empty);
			config.StatusRanges = ParseHttpStatusRanges(config.SuccessStatusRanges, config.TerminateStatusRanges);

			config.LogMaxSize = ConfigProperties.ExtractInt(configXml, "/Config/LogMaxSize");
			if (config.LogMaxSize <= 0)
				throw new AdapterException("Log Max File Size (MB) must be positive.");

			config.LogMaxCount = ConfigProperties.ExtractInt(configXml, "/Config/LogMaxCount");
			if (config.LogMaxCount <= 0)
				throw new AdapterException("Log Max Rollover Count must be positive.");

			config.LogLevel = ConfigProperties.Extract(configXml, "/Config/LogLevel", null);
			if (config.LogLevel != "Info" && config.LogLevel != "Debug" && config.LogLevel != "Trace")
				throw new AdapterException("Log Level not a valid value.");

			config.LogFormat = ConfigProperties.IfExistsExtract(configXml, "/Config/LogFormat", "Flat");
			if (config.LogFormat != "Flat" && config.LogFormat != "Structured" && config.LogFormat != "Both")
				throw new AdapterException("Log Format not a valid value.");

			config.Certificate = ConfigProperties.IfExistsExtract(configXml, "/Config/Certificate", string.Empty);
			config.CertificatePassphrase = ConfigProperties.IfExistsExtract(configXml, "/Config/CertificatePassphrase", string.Empty);

			return config;
		}

		public static HttpExConfiguration Parse(XmlDocument configXml, IBaseMessageContext messageContext)
		{
			var config = Parse(configXml);
			var userHttpHeaders = (string)messageContext.Read(UserHttpHeaders.Name.Name, UserHttpHeaders.Name.Namespace);
			ParseHeaders(userHttpHeaders, config);
			var httpContentType = (string)messageContext.Read(HttpContentType.Name.Name, HttpContentType.Name.Namespace);
			if (!string.IsNullOrWhiteSpace(httpContentType))
				config.ContentType = ParseContentType(httpContentType);
			return config;
		}

		static void ParseHeaders(string headersText, HttpExConfiguration config)
		{
			if (!string.IsNullOrWhiteSpace(headersText))
			{
				const string headerExp = @"^(?<key>\S+)\:\s+(?<value>.*)$";
				if (!Regex.IsMatch(headersText, "(" + headerExp + ")+", RegexOptions.Multiline))
					throw new AdapterException("HTTP header format is invalid.");
				foreach (Match match in Regex.Matches(headersText, headerExp, RegexOptions.Multiline))
					config.CustomHeaders.Add(
						new KeyValuePair<string, string>(match.Groups["key"].Value, match.Groups["value"].Value.Trim()));
			}
		}

		private static MediaTypeHeaderValue ParseContentType(string contentType)
		{
			try
			{
				var parts = contentType.Split(';');
				if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
					return null;

				var contentHeader = new MediaTypeHeaderValue(parts[0].Trim());
				for (int i = 1; i < parts.Length; i++)
				{
					var subParts = parts[i].Split('=');
					if (subParts[0].Trim() == "charset")
					{
						contentHeader.CharSet = subParts[1].Trim();
					}
					else
					{
						contentHeader.Parameters.Add(new NameValueHeaderValue(subParts[0].Trim(), subParts[1].Trim()));
					}
				}

				return contentHeader;
			}
			catch (FormatException ex)
			{
				throw new AdapterException("Error parsing Content Type value. " + ex.Message);
			}
		}

		static List<Tuple<int, int, StatusType>> ParseHttpStatusRanges(string successStatusRanges, string terminateStatusRanges)
		{
			var rangeVals = new List<Tuple<int, int, StatusType>>();
			AddRanges(successStatusRanges, rangeVals, StatusType.Success);
			if (!string.IsNullOrWhiteSpace(terminateStatusRanges))
				AddRanges(terminateStatusRanges, rangeVals, StatusType.NontransientFailure);
			rangeVals.Sort((a, b) => a.Item1.CompareTo(b.Item1));

			for (int i = 0; i < rangeVals.Count; i++)
			{
				var from = rangeVals[i].Item1;
				var to = rangeVals[i].Item2;
				if (to < from)
					throw new FormatException("Status range format is incorrect.");
				if (i > 0)
				{
					var prevTo = rangeVals[i - 1].Item2;
					if (from <= prevTo)
						throw new FormatException("Status ranges overlap.");
				}
			}

			return rangeVals;
		}

		static void AddRanges(string statusRanges, List<Tuple<int, int, StatusType>> rangeVals, StatusType statusType)
		{
			const string rangeExp = @"(?<from>\d{3})(-(?<to>\d{3}))?";

			if (!Regex.IsMatch(statusRanges, @"^" + rangeExp + @"(," + rangeExp + @")*$"))
				throw new FormatException("Status range format is incorrect.");

			var matches = Regex.Matches(statusRanges, rangeExp);
			foreach (Match match in matches)
			{
				var from = int.Parse(match.Groups["from"].Value);
				var to = match.Groups["to"].Success ? int.Parse(match.Groups["to"].Value) : from;
				rangeVals.Add(new Tuple<int, int, StatusType>(from, to, statusType));
			}
		}

		public override string ToString()
		{
			return new XElement("Config",
				new XElement("uri", LocationUri),
				new XElement("UniqueName", UniqueName),
				new XElement("DestinationUrl", DestinationUrl),
				new XElement("Timeout", Timeout),
				new XElement("Method", Method),
				new XElement("ContentType", ContentType),
				new XElement("CustomHeaders", string.Join(Environment.NewLine, CustomHeaders.Select(h => h.Key + ": " + h.Value))),
				new XElement("CustomHeaderNamesWithoutValidation", string.Join(", ", CustomHeaderNamesWithoutValidation)),
				new XElement("SuppressMessageBodyForHttpVerbs", string.Join(", ", SuppressMessageBodyForHttpVerbs)),
				new XElement("Success", SuccessStatusRanges),
				new XElement("Terminate", TerminateStatusRanges),
				new XElement("LogMaxSize", LogMaxSize),
				new XElement("LogMaxCount", LogMaxCount),
				new XElement("LogLevel", LogLevel),
				new XElement("LogFormat", LogFormat),
				new XElement("Certificate", Certificate),
				new XElement("CertificatePassphrase", CertificatePassphrase)
			).ToString(SaveOptions.DisableFormatting);
		}

		public enum StatusType
		{
			Success,
			NontransientFailure,
			TransientFailure
		}

		static readonly HTTP.UserHttpHeaders UserHttpHeaders = new HTTP.UserHttpHeaders();
		static readonly HTTP.ContentType HttpContentType = new HTTP.ContentType();
	}
}
