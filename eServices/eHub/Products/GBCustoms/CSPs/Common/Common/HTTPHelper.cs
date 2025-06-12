using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;

namespace CargoWise.eHub.Products.GBCustoms.CSPs.Common
{
    public static class HTTPHelper
    {
        public static string FindHeaderValueOrDefault(HttpResponse httpResponse, string keys, string delimiter = "|")
        {
			if (httpResponse.Headers == null)
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(keys))
			{
				return string.Empty;
			}

			var keyList = SplitKeys(keys, delimiter);

			foreach (var key in keyList)
			{
				if (!string.IsNullOrEmpty(key))
				{
					var matchedHeaders = httpResponse.Headers
						.Where(header => header.Key.ToLower() == key.ToLower())
						.ToArray();

					if (matchedHeaders.Length == 1)
					{
						return matchedHeaders
						.Single()
						.Value;
					}
				}
			}

			return string.Empty;
		}

		public static string FindHeaderValueOrDefault(XDocument requestMessage, string xPath, List<string> keys)
        {
			if (keys.Count == 0)
			{
				return string.Empty;
			}

			foreach(var key in keys)
            {
				if (!string.IsNullOrEmpty(key))
				{
					var headerValue = requestMessage.XPathSelectElement($"{xPath}[@name='{key}']")?.Attribute("value")?.Value;
					if (headerValue != null)
					{
						return headerValue;
					}
				}
			}

			return string.Empty;
		}

		public static string FindElementValueOrDefault(string httpResponseText, string xPath)
		{
			var messageDoc = XDocument.Parse(httpResponseText);

			if (!string.IsNullOrEmpty(xPath))
			{
				var elementValue = messageDoc.XPathSelectElement($"{xPath}")?.Value;
				if (elementValue != null)
				{
					return elementValue;
				}
			}
			return string.Empty;
		}

		public static bool ElementExists(string httpResponseText, string xPath)
		{
			return !string.IsNullOrEmpty(FindElementValueOrDefault(httpResponseText, xPath));
		}

		private static List<string> SplitKeys(string keys, string delimiter)
        {
			string[] delimiterArray = { delimiter };
			return keys.Split(delimiterArray, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

		public static string GetMessageHeaders(HttpResponse httpResponse)
		{
			if(httpResponse.Headers == null)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.AppendLine();

			foreach (var header in httpResponse.Headers)
            {
				sb.AppendLine($"{header.Key ?? ""} - {header.Value ?? ""}");
            }

			return sb.ToString();
		}
	}
}
